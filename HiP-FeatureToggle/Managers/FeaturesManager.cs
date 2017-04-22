using Microsoft.EntityFrameworkCore;
using PaderbornUniversity.SILab.Hip.FeatureToggle.Data;
using PaderbornUniversity.SILab.Hip.FeatureToggle.Models.Entity;
using PaderbornUniversity.SILab.Hip.FeatureToggle.Models.Rest;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PaderbornUniversity.SILab.Hip.FeatureToggle.Managers
{
    public class FeaturesManager : FeatureTogglesManagerBase
    {
        public FeaturesManager(ToggleDbContext db) : base(db)
        {
        }

        /// <exception cref="ArgumentNullException">The specified arguments are null</exception>
        /// <exception cref="ArgumentException">A feature with the specified name already exists</exception>
        /// <exception cref="ResourceNotFoundException{Feature}">The referenced parent feature does not exist</exception>
        public Feature CreateFeature(FeatureArgs args)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            if (_db.Features.Any(f => f.Name == args.Name))
                throw new ArgumentException($"A feature with name '{args.Name}' already exists");

            var feature = new Feature { Name = args.Name };

            if (args.Parent != null)
            {
                feature.Parent = GetFeature(args.Parent.Value);

                if (feature.Parent == null)
                    throw new ResourceNotFoundException<Feature>(args.Parent);
            }

            _db.Features.Add(feature);
            _db.SaveChanges();
            return feature;
        }

        /// <summary>
        /// Removes a feature. If the feature has children, these are made children of the deleted feature's parent.
        /// </summary>
        /// <remarks>
        /// Regarding descendants of a feature, there are multiple options (option 3 is implemented):
        /// 1) Deletion of a feature having children is forbidden
        ///
        /// 2) Deletion removes the whole subtree of features (i.e. cascading)
        ///
        ///    root ___ toBeDeleted ___ child1      >>>   root
        ///                         \__ child2
        ///
        /// 2) Deletion only removes one feature, its children are moved to the parent of the deleted feature
        ///
        ///    root ___ toBeDeleted ___ child1      >>>   root ___ child1
        ///                         \__ child2                 \__ child2
        /// </remarks>
        /// <exception cref="ResourceNotFoundException{Feature}">The feature with specified ID does not exist</exception>
        public void DeleteFeature(int featureId)
        {
            var feature = GetFeature(featureId, loadParent: true, loadChildren: true, loadGroups: true);

            if (feature == null)
                throw new ResourceNotFoundException<Feature>(featureId);

            var mappings = feature.GroupsWhereEnabled.ToList();
            var groups = feature.GroupsWhereEnabled.Select(m => m.Group).ToList();

            // 1) remove feature from groups where it is enabled
            foreach (var mapping in feature.GroupsWhereEnabled.ToList())
                _db.FeatureToFeatureGroupMappings.Remove(mapping);

            // 2) detach from parent feature
            feature.Parent?.Children.Remove(feature);

            // 3) feature might have children => move them to parent of deleted feature
            foreach (var child in feature.Children)
                child.Parent = feature.Parent;

            // 4) delete feature
            _db.Features.Remove(feature);
            _db.SaveChanges();
        }

        /// <summary>
        /// Updates a feature. If the reference to the parent is modified, this moves the whole subtree of features.
        /// </summary>
        /// <exception cref="ArgumentNullException">The specified arguments are null</exception>
        /// <exception cref="ArgumentException">The new feature name is already in use</exception>
        /// <exception cref="ResourceNotFoundException{Feature}">There is no feature with the specified ID</exception>
        /// <exception cref="InvalidOperationException">Parent modification causes a cycle (destroys tree structure)</exception>
        public void UpdateFeature(int featureId, FeatureArgs args)
        {
            if (args == null)
                throw new ArgumentNullException(nameof(args));

            if (_db.Features.Any(f => f.Name == args.Name && f.Id != featureId))
                throw new ArgumentException($"A feature with name '{args.Name}' already exists");

            var feature = GetFeature(featureId, loadParent: true, loadChildren: true, loadGroups: true);

            if (feature == null)
                throw new ResourceNotFoundException<Feature>(featureId);

            var newParent = args.Parent.HasValue ? GetFeature(args.Parent.Value, loadChildren: true) : null;

            if (newParent == null && args.Parent.HasValue)
                throw new ResourceNotFoundException<Feature>(args.Parent.Value);

            // ensure that the new parent is not a descendant of or equal to the feature to be updated
            // (this would create a cycle destroying the tree structure)
            if (IsDescendantOrEqual(newParent, feature))
                throw new InvalidOperationException($"Changing the parent of '{featureId}' to '{args.Parent.Value}' would destroy the tree structure because '{args.Parent.Value}' is a descendant of or the same as '{featureId}'");

            // 1) update name
            feature.Name = args.Name;

            // 2) detach from old parent
            feature.Parent?.Children.Remove(feature);

            // 3) attach to new parent
            newParent?.Children.Add(feature);
            feature.Parent = newParent;

            _db.SaveChanges();
        }

        /// <summary>
        /// Checks whether a specific feature is effectively enabled for a specific user or an anonymous
        /// (unauthenticated) user. Anonymous users are considered to be in the
        /// <see cref="FeatureTogglesManagerBase.PublicGroup"/>.
        /// </summary>
        /// <exception cref="ResourceNotFoundException{Feature}"/>
        public bool IsFeatureEffectivelyEnabledForUser(string userId, int featureId)
        {
            var group = GetGroupForUser(userId);
            var feature = GetFeature(featureId);

            if (feature == null)
                throw new ResourceNotFoundException<Feature>(featureId);

            return IsFeatureEffectivelyEnabledForGroup(feature, group);
        }

        /// <summary>
        /// Gets the features that are effectively enabled for a specific user or an anonymous (unauthenticated) user.
        /// Anonymous users are considered to be in the <see cref="FeatureTogglesManagerBase.PublicGroup"/>.
        /// </summary>
        /// <param name="userId">The ID of a user or null for anonymous users</param>
        /// <returns></returns>
        public IReadOnlyCollection<Feature> GetEffectivelyEnabledFeaturesForUser(string userId)
        {
            var group = GetGroupForUser(userId);
            return GetEffectivelyEnabledFeaturesForGroup(group);
        }


        /// <summary>
        /// Enable Feature for the Feature Group
        /// </summary>
        /// <exception cref="ArgumentException">The feature already exists in group</exception>
        /// <exception cref="ResourceNotFoundException{Feature}">There is no feature with the specified ID</exception>
        /// <exception cref="ResourceNotFoundException{FeatureGroup}">There is no group with the specified ID</exception>
         public FeatureToFeatureGroupMapping EnableFeautureForGroup(int featureId,int groupId)
        {
            var feature = GetFeature(featureId, loadParent: true, loadChildren: true, loadGroups: true);

            if (feature == null)
                throw new ResourceNotFoundException<Feature>(featureId);

            var group = GetGroup(groupId,loadFeatures:true);

            if (group == null)
                throw new ResourceNotFoundException<FeatureGroup>(groupId);

            if (group.EnabledFeatures.Any(x => x.FeatureId == featureId))
                throw new ArgumentException($"A feature '{featureId}' already exists in group '{groupId}'");

            var mapping = new FeatureToFeatureGroupMapping(feature, group);
            feature.GroupsWhereEnabled.Add(mapping);
            group.EnabledFeatures.Add(mapping);

            _db.FeatureToFeatureGroupMappings.Add(mapping);
            _db.SaveChanges();

            return mapping;

        }

        /// <summary>
        /// Disable Feature for the Feature Group
        /// </summary>
        /// <exception cref="ArgumentException">The feature don`t exists in group</exception>
        /// <exception cref="ResourceNotFoundException{Feature}">There is no feature with the specified ID</exception>
        /// <exception cref="ResourceNotFoundException{FeatureGroup}">There is no group with the specified ID</exception>
        public void DisableFeatureForGroup(int featureId,int groupId)
        {
            var feature = GetFeature(featureId, loadParent: true, loadChildren: true, loadGroups: true);

            if (feature == null)
                throw new ResourceNotFoundException<Feature>(featureId);

            var group = GetGroup(groupId, loadFeatures: true);

            if (group == null)
                throw new ResourceNotFoundException<FeatureGroup>(groupId);

           var mapping = group.EnabledFeatures.FirstOrDefault(x => x.FeatureId == featureId);

            if (mapping==null)
                throw new ArgumentException($"A feature '{featureId}' do not exists in group '{groupId}'");

            _db.FeatureToFeatureGroupMappings.Remove(mapping);
            _db.SaveChanges();

        }

        private FeatureGroup GetGroupForUser(string userId)
        {
            // for anonymous users, the protected public group is relevant
            if (userId == null)
                return PublicGroup;

            // for authenticated users, it is the group they are assigned to
            var user = GetOrCreateUser(userId);

            return _db.FeatureGroups
                .Include(g => g.EnabledFeatures).ThenInclude(m => m.Feature).ThenInclude(f => f.Children)
                .First(g => g.Id == user.FeatureGroupId); // per specification, this group must exist
        }

        private bool IsFeatureEffectivelyEnabledForGroup(Feature feature, FeatureGroup group)
        {
            // required navigation properties: FeatureGroup.EnabledFeatures
            var enabledFeatures = group.EnabledFeatures
                .Concat(PublicGroup.EnabledFeatures)
                .Distinct();

            return AncestorsAndSelf(feature).All(parent => enabledFeatures.Any(m => m.FeatureId == parent.Id));
        }

        private IReadOnlyCollection<Feature> GetEffectivelyEnabledFeaturesForGroup(FeatureGroup group)
        {
            // required navigation properties: FeatureGroup.EnabledFeatures.Feature

            // The feature group specifies which features are enabled.
            // From specification: "A feature X is [effectively] enabled for a certain user if X and all features
            // higher than X in the feature hierarchy are enabled for the user's feature group [or the public group]"
            // (features enabled in the public group are "added" to those of the user's group, HIPCMS-608)
            // Thus, the set of effectively enabled features is a subset of the enabled features.

            // Example:
            // Feature Hierarchy:        Features enabled in user Bob's group: [1, 2, 4]
            // (1)                       Features enabled in public group:     [3, 5]
            //    (2)
            // (3)                       Features effectively enabled for user Bob:        [1, 3, 5]
            //    (4)                    Features effectively enabled for unauthenticated users: [3]
            //       (5)
            //       (6)
            //    (7)

            var enabledFeatures = group.EnabledFeatures
                .Concat(PublicGroup.EnabledFeatures)
                .Distinct()
                .Select(m => m.Feature)
                .ToSet();

            return enabledFeatures
                .Where(f => AncestorsAndSelf(f).All(parent => enabledFeatures.Contains(parent)))
                .ToList();
        }

        private bool IsDescendantOrEqual(Feature feature, Feature other)
        {
            // checks if 'feature' is a descendant of 'other' in the feature tree structure
            // or if 'feature' and 'other' are the same.
            return feature != null && other != null && AncestorsAndSelf(feature).Contains(other);
        }

        private IEnumerable<Feature> AncestorsAndSelf(Feature feature)
        {
            // returns a collection of the feature itself, its parent, its parent's parent etc. (up to the root)
            // (potentially expensive operation, depending on the tree depth)

            while (feature != null)
            {
                yield return feature;

                // next parent must be explicitly loaded since usually not the whole tree structure is available here
                // (Note: Load() doesn't work here, probably because the entity is tracked)
                feature = _db.Entry(feature).Reference(f => f.Parent).Query().FirstOrDefault();
            }
        }
    }
}
