using PaderbornUniversity.SILab.Hip.FeatureToggle.Data;
using PaderbornUniversity.SILab.Hip.FeatureToggle.Models.Entity;
using PaderbornUniversity.SILab.Hip.FeatureToggle.Models.Rest;
using System;
using System.Linq;

namespace PaderbornUniversity.SILab.Hip.FeatureToggle.Managers
{
    public class FeaturesManager
    {
        private readonly ToggleDbContext _db;

        public FeaturesManager(ToggleDbContext db)
        {
            _db = db;
        }

        public IQueryable<Feature> GetFeatures(bool loadParent = false, bool loadChildren = false, bool loadGroups = false)
        {
            return _db.Features
                .IncludeIf(loadParent, nameof(Feature.Parent))
                .IncludeIf(loadChildren, nameof(Feature.Children))
                .IncludeIf(loadGroups, nameof(Feature.GroupsWhereEnabled));
        }

        public Feature GetFeature(int featureId, bool loadParent = false, bool loadChildren = false, bool loadGroups = false)
        {
            return GetFeatures(loadParent, loadChildren, loadGroups)
                .FirstOrDefault(f => f.Id == featureId);
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

        private bool IsDescendantOrEqual(Feature feature, Feature other)
        {
            // checks if 'feature' is a descendant of 'other' in the feature tree structure
            // or if 'feature' and 'other' are the same.
            // (potentially expensive operation, depending on the tree depth)

            if (feature == null || other == null)
                return false;

            if (feature == other)
                return true;

            // next parent must be explicitly loaded since usually not the whole tree structure is available here
            // (Note: Load() doesn't work here, probably because the entity is tracked)
            var next = _db.Entry(feature).Reference(f => f.Parent).Query().FirstOrDefault();
            return IsDescendantOrEqual(next, other);
        }
    }
}
