using Microsoft.EntityFrameworkCore;
using PaderbornUniversity.SILab.Hip.FeatureToggle.Data;
using PaderbornUniversity.SILab.Hip.FeatureToggle.Models.Entity;
using PaderbornUniversity.SILab.Hip.FeatureToggle.Models.Rest;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PaderbornUniversity.SILab.Hip.FeatureToggle.Managers
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// There exists a default feature group new users are assigned to.
    /// </remarks>
    public class FeatureGroupsManager
    {
        private static readonly User[] _noUsers = new User[0];
        private static readonly Feature[] _noFeatures = new Feature[0];

        private readonly ToggleDbContext _db;

        /// <summary>
        /// The default group for authorized users.
        /// </summary>
        public FeatureGroup DefaultGroup { get; }

        /// <summary>
        /// The group for unauthorized users.
        /// </summary>
        public FeatureGroup PublicGroup { get; }

        public FeatureGroupsManager(ToggleDbContext db)
        {
            _db = db;

            // Load standard groups which are always available and can't be deleted.
            // If this fails, the database is not correctly initialized.
            DefaultGroup = GetGroups(true, true).Single(g => g.Name == FeatureGroup.DefaultGroupName);
            PublicGroup = GetGroups(true, true).Single(g => g.Name == FeatureGroup.PublicGroupName);
        }

        public User GetOrCreateUser(string userId)
        {
            var user = _db.Users
                .Include(nameof(User.FeatureGroup))
                .FirstOrDefault(u => u.Id == userId);

            if (user != null)
                return user;

            // create new user
            var newUser = CreateUser(userId);
            _db.SaveChanges();
            return newUser;
        }

        public IEnumerable<User> GetOrCreateUsers(IEnumerable<string> userIds)
        {
            if (userIds == null)
                return _noUsers;

            var userIdsSet = userIds.ToSet();
            var storedUsers = _db.Users.Where(u => userIdsSet.Contains(u.Id)).ToList();
            var missingUserIds = userIdsSet.Except(storedUsers.Select(u => u.Id));

            if (missingUserIds.Any())
            {
                // Create missing users
                var newUsers = missingUserIds.Select(id => CreateUser(id)).ToList();
                _db.SaveChanges();
                return storedUsers.Concat(newUsers);
            }

            return storedUsers;
        }

        /// <exception cref="ResourceNotFoundException{Feature}">No features exist for one or multiple of the specified IDs</exception>
        public IReadOnlyCollection<Feature> GetFeatures(IEnumerable<int> featureIds, bool loadGroups = false)
        {
            if (featureIds == null)
                return _noFeatures;

            var featureIdsSet = featureIds.ToSet();

            var storedFeatures = _db.Features
                .IncludeIf(loadGroups, nameof(Feature.GroupsWhereEnabled))
                .Where(f => featureIdsSet.Contains(f.Id)).ToList();

            var missingFeatureIds = featureIdsSet.Except(storedFeatures.Select(f => f.Id));

            if (missingFeatureIds.Any())
                throw new ResourceNotFoundException<Feature>(missingFeatureIds);

            return storedFeatures;
        }

        public IQueryable<FeatureGroup> GetGroups(bool loadMembers = false, bool loadFeatures = false)
        {
            return _db.FeatureGroups
                .IncludeIf(loadMembers, nameof(FeatureGroup.Members))
                .IncludeIf(loadFeatures, nameof(FeatureGroup.EnabledFeatures));
        }

        public FeatureGroup GetGroup(int groupId, bool loadMembers = false, bool loadFeatures = false)
        {
            return GetGroups(loadMembers, loadFeatures)
                .FirstOrDefault(g => g.Id == groupId);
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
        /// <exception cref="InvalidOperationException">It is attempted to rename a protected feature group</exception>
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
        public void MoveUserToGroup(string userId, int groupId)
        {
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

        private User CreateUser(string id)
        {
            var user = new User
            {
                Id = id,
                FeatureGroup = DefaultGroup
            };

            _db.Users.Add(user);
            return user;
        }
    }
}
