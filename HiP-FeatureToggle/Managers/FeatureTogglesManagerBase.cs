using Microsoft.EntityFrameworkCore;
using PaderbornUniversity.SILab.Hip.FeatureToggle.Data;
using PaderbornUniversity.SILab.Hip.FeatureToggle.Models.Entity;
using System.Collections.Generic;
using System.Linq;

namespace PaderbornUniversity.SILab.Hip.FeatureToggle.Managers
{
    public abstract class FeatureTogglesManagerBase
    {
        private static readonly User[] _noUsers = new User[0];
        private static readonly Feature[] _noFeatures = new Feature[0];

        protected readonly ToggleDbContext _db;

        /// <summary>
        /// The default group for authorized users.
        /// </summary>
        public FeatureGroup DefaultGroup { get; }

        /// <summary>
        /// The group for unauthorized users.
        /// </summary>
        public FeatureGroup PublicGroup { get; }

        public FeatureTogglesManagerBase(ToggleDbContext db)
        {
            _db = db;

            // Load standard groups which are always available and can't be deleted.
            // If this fails, the database is not correctly initialized.
            DefaultGroup = GetAllGroups(true, true)
                .Include(g => g.EnabledFeatures)
                    .ThenInclude(m => m.Feature) // loading this is required for checking effectively enabled features
                        .ThenInclude(f => f.Children)
                .Single(g => g.Name == FeatureGroup.DefaultGroupName);

            PublicGroup = GetAllGroups(true, true)
                .Include(g => g.EnabledFeatures)
                    .ThenInclude(m => m.Feature) // loading this is required for checking effectively enabled features
                        .ThenInclude(f => f.Children)
                .Single(g => g.Name == FeatureGroup.PublicGroupName);
        }

        protected User GetOrCreateUser(string userId)
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

        protected IEnumerable<User> GetOrCreateUsers(IEnumerable<string> userIds)
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

        public IQueryable<FeatureGroup> GetAllGroups(bool loadMembers = false, bool loadFeatures = false)
        {
            return _db.FeatureGroups
                .IncludeIf(loadMembers, nameof(FeatureGroup.Members))
                .IncludeIf(loadFeatures, nameof(FeatureGroup.EnabledFeatures));
        }

        public FeatureGroup GetGroup(int groupId, bool loadMembers = false, bool loadFeatures = false)
        {
            return GetAllGroups(loadMembers, loadFeatures)
                .FirstOrDefault(g => g.Id == groupId);
        }
        
        /// <exception cref="ResourceNotFoundException{Feature}">No features exist for one or multiple of the specified IDs</exception>
        protected IReadOnlyCollection<Feature> GetFeatures(IEnumerable<int> featureIds, bool loadGroups = false)
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

        public IQueryable<Feature> GetAllFeatures(bool loadParent = false, bool loadChildren = false, bool loadGroups = false)
        {
            return _db.Features
                .IncludeIf(loadParent, nameof(Feature.Parent))
                .IncludeIf(loadChildren, nameof(Feature.Children))
                .IncludeIf(loadGroups, nameof(Feature.GroupsWhereEnabled));
        }

        public Feature GetFeature(int featureId, bool loadParent = false, bool loadChildren = false, bool loadGroups = false)
        {
            return GetAllFeatures(loadParent, loadChildren, loadGroups)
                .FirstOrDefault(f => f.Id == featureId);
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
