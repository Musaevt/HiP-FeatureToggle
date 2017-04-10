using Microsoft.EntityFrameworkCore;
using PaderbornUniversity.SILab.Hip.FeatureToggle.Data;
using PaderbornUniversity.SILab.Hip.FeatureToggle.Models.Entity;
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

        /// <exception cref="ArgumentException">No features exist for one or multiple of the specified IDs</exception>
        public IReadOnlyCollection<Feature> GetFeatures(IEnumerable<int> featureIds)
        {
            if (featureIds == null)
                return _noFeatures;

            var featureIdsSet = featureIds.ToSet();
            var storedFeatures = _db.Features.Where(f => featureIdsSet.Contains(f.Id)).ToList();
            var missingFeatureIds = featureIdsSet.Except(storedFeatures.Select(f => f.Id));

            if (missingFeatureIds.Any())
                throw new ArgumentException("The following features do not exist: " + string.Join(", ", missingFeatureIds));

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

        /// <exception cref="ArgumentNullException">The specified group is null</exception>
        /// <exception cref="ArgumentException">A feature group with the specified name already exists</exception>
        public void AddGroup(FeatureGroup group)
        {
            if (group == null)
                throw new ArgumentNullException(nameof(group));

            if (_db.FeatureGroups.Any(g => g.Name == group.Name))
                throw new ArgumentException($"A feature group with name '{group.Name}' already exists");

            // "pre-assigned" members of the new group might - until now - be assigned to another group
            // => we have to correctly detach from the old group
            foreach (var user in group.Members.ToList())
                MoveUserToGroupCore(user, group);

            _db.FeatureGroups.Add(group);
            _db.SaveChanges();
        }

        public bool RemoveGroup(int groupId)
        {
            var group = GetGroup(groupId, loadMembers: true);

            if (group == null || group.IsProtected)
                return false;

            // before removing, move all group members to the default group
            foreach (var member in group.Members.ToList())
                MoveUserToGroup(member, DefaultGroup);

            _db.FeatureGroups.Remove(group);
            _db.SaveChanges();
            return true;
        }

        /// <summary>
        /// Updates a feature group by replacing the enabled features and group members with new collections.
        /// Members that are effectively removed from the group are assigned to the default group.
        /// </summary>
        /// <param name="groupId"></param>
        /// <param name="newFeatures"></param>
        /// <param name="newMembers"></param>
        /// <exception cref="ArgumentException">The new group name is already in use or there is no group with the specified ID</exception>
        /// <exception cref="InvalidOperationException">It is attempted to rename a protected feature group</exception>
        public void UpdateGroup(int groupId, string newName, IEnumerable<Feature> newFeatures, IEnumerable<User> newMembers)
        {
            if (_db.FeatureGroups.Any(g => g.Name == newName && g.Id != groupId))
                throw new ArgumentException($"A feature group with name '{newName}' already exists");

            var group = GetGroup(groupId, loadMembers: true, loadFeatures: true);

            if (group == null)
                throw new ArgumentException($"There is no feature group with ID '{groupId}'");

            if (group.IsProtected && newName != group.Name)
                throw new InvalidOperationException($"Protected group '{group.Name}' cannot be renamed");

            group.Name = newName;

            // remove old members
            foreach (var user in group.Members.ToList())
                MoveUserToGroupCore(user, DefaultGroup);

            // add new members
            foreach (var user in newMembers)
                MoveUserToGroupCore(user, group);

            // remove old enabled features
            foreach (var mapping in group.EnabledFeatures.ToList())
            {
                mapping.Feature.GroupsWhereEnabled.Remove(mapping);
                group.EnabledFeatures.Remove(mapping);
            }

            // add new enabled features
            foreach (var feature in newFeatures)
            {
                var mapping = new FeatureToFeatureGroupMapping(feature, group);
                feature.GroupsWhereEnabled.Add(mapping);
                group.EnabledFeatures.Add(mapping);
            }

            _db.SaveChanges();
        }

        /// <exception cref="ArgumentNullException">Any argument is null</exception>
        public void MoveUserToGroup(User user, FeatureGroup group)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (group == null)
                throw new ArgumentNullException(nameof(group));

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

    internal static class LinqExtensions
    {
        public static ISet<T> ToSet<T>(this IEnumerable<T> collection) => new HashSet<T>(collection);

        public static IQueryable<T> IncludeIf<T>(this IQueryable<T> query, bool include, string navigationPropertyPath)
            where T : class
        {
            return include ? query.Include(navigationPropertyPath) : query;
        }
    }
}
