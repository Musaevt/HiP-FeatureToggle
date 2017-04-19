using PaderbornUniversity.SILab.Hip.FeatureToggle.Data;
using PaderbornUniversity.SILab.Hip.FeatureToggle.Models.Entity;
using PaderbornUniversity.SILab.Hip.FeatureToggle.Models.Rest;
using System;
using System.Linq;

namespace PaderbornUniversity.SILab.Hip.FeatureToggle.Managers
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// There exists a default feature group new users are assigned to.
    /// </remarks>
    public class FeatureGroupsManager : FeatureTogglesManagerBase
    {
        public FeatureGroupsManager(ToggleDbContext db) : base(db)
        {
        }

        /// <exception cref="ArgumentNullException">Specified argument is null</exception>
        /// <exception cref="ArgumentException">A feature group with the specified name already exists</exception>
        /// <exception cref="ResourceNotFoundException{Feature}">A referenced feature does not exist</exception>
        public void CreateGroup(FeatureGroupArgs args)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            if (_db.FeatureGroups.Any(g => g.Name == args.Name))
                throw new ArgumentException($"A feature group with name '{args.Name}' already exists");

            var group = new FeatureGroup { Name = args.Name };

            group.EnabledFeatures = GetFeatures(args.EnabledFeatures)
                .Select(f => new FeatureToFeatureGroupMapping(f, group))
                .ToList();

            group.Members = GetOrCreateUsers(args.Members).ToList();

            // "pre-assigned" members of the new group might - until now - be assigned to another group
            // => we have to correctly detach from the old group
            foreach (var user in group.Members.ToList())
                MoveUserToGroupCore(user, group);

            _db.FeatureGroups.Add(group);
            _db.SaveChanges();
        }

        /// <exception cref="ResourceNotFoundException{FeatureGroup}">Group with specified ID not found</exception>
        /// <exception cref="InvalidOperationException">Attempted to remove protected group</exception>
        public void RemoveGroup(int groupId)
        {
            var group = GetGroup(groupId, loadMembers: true);

            if (group == null)
                throw new ResourceNotFoundException<FeatureGroup>(groupId);

            if (group.IsProtected)
                throw new InvalidOperationException($"Protected group '{groupId}' cannot be removed");

            // before removing, move all group members to the default group
            foreach (var member in group.Members.ToList())
                MoveUserToGroupCore(member, DefaultGroup);

            _db.FeatureGroups.Remove(group);
            _db.SaveChanges();
        }

        /// <summary>
        /// Updates a feature group by replacing the enabled features and group members with new collections.
        /// Members that are effectively removed from the group are assigned to the default group.
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="newFeatures"></param>
        /// <param name="newMembers"></param>
        /// <exception cref="ArgumentNullException">Arguments are null</exception>
        /// <exception cref="ArgumentException">The new group name is already in use</exception>
        /// <exception cref="ResourceNotFoundException{Feature}">There is no feature with the specified ID</exception>
        /// <exception cref="ResourceNotFoundException{FeatureGroup}">There is no group with the specified ID</exception>
        /// <exception cref="InvalidOperationException">Tried to rename a protected feature group or tried to move users to the public group</exception>
        public void UpdateGroup(int groupId, FeatureGroupArgs args)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            if (_db.FeatureGroups.Any(g => g.Name == args.Name && g.Id != groupId))
                throw new ArgumentException($"A feature group with name '{args.Name}' already exists");

            var group = GetGroup(groupId, loadMembers: true, loadFeatures: true);

            if (group == null)
                throw new ResourceNotFoundException<FeatureGroup>(groupId);

            if (group.IsProtected && args.Name != group.Name)
                throw new InvalidOperationException($"Protected group '{group.Name}' cannot be renamed");

            if (group.Id == PublicGroup.Id && (args.Members?.Count ?? 0) > 0)
                throw new InvalidOperationException("Users cannot explicitly be moved into the public group");

            group.Name = args.Name;
            var newMembers = GetOrCreateUsers(args.Members).ToList();
            var newFeatures = GetFeatures(args.EnabledFeatures, loadGroups: true);

            // remove old members
            foreach (var user in group.Members.ToList())
                MoveUserToGroupCore(user, DefaultGroup);

            // add new members
            foreach (var user in newMembers)
                MoveUserToGroupCore(user, group);

            // remove old enabled features
            // (for EF to work, we have to make sure not to delete & re-add the same mapping)
            var featuresToRemove = group.EnabledFeatures.Where(m => !newFeatures.Any(f => f.Id == m.FeatureId)).ToList();
            var featuresToAdd = newFeatures.Where(f => !group.EnabledFeatures.Any(m => m.FeatureId == f.Id)).ToList();

            foreach (var mapping in featuresToRemove)
                _db.FeatureToFeatureGroupMappings.Remove(mapping);

            // add new enabled features
            foreach (var feature in featuresToAdd)
            {
                var mapping = new FeatureToFeatureGroupMapping(feature, group);
                feature.GroupsWhereEnabled.Add(mapping);
                group.EnabledFeatures.Add(mapping);
            }

            _db.SaveChanges();
        }

        /// <exception cref="ResourceNotFoundException{FeatureGroup}">The group with specified ID does not exist</exception>
        /// <exception cref="InvalidOperationException">Tried to move user to the public group</exception>
        public void MoveUserToGroup(string userId, int groupId)
        {
            if (groupId == PublicGroup.Id)
                throw new InvalidOperationException("Users cannot explicitly be moved into the public group");

            var user = GetOrCreateUser(userId);
            var group = GetGroup(groupId, loadMembers: true);

            if (group == null)
                throw new ResourceNotFoundException<FeatureGroup>(groupId);

            MoveUserToGroupCore(user, group);
            _db.SaveChanges();
        }
        
        private void MoveUserToGroupCore(User user, FeatureGroup group)
        {
            // remove user from current group, then add to new group
            user.FeatureGroup.Members.Remove(user);
            group.Members.Add(user);
            user.FeatureGroup = group;
        }
    }
}
