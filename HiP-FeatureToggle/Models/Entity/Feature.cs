using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Collections.Generic;

namespace PaderbornUniversity.SILab.Hip.FeatureToggle.Models.Entity
{
    public class Feature
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public Feature Parent { get; set; }

        public int? ParentId { get; set; }

        public List<Feature> Children { get; set; }

        /// <summary>
        /// The feature groups where this features is enabled.
        /// The opposite direction of this many-to-many relation is <see cref="FeatureGroup.EnabledFeatures"/>.
        /// </summary>
        public IList<FeatureToFeatureGroupMapping> GroupsWhereEnabled { get; set; }
    }

    public static class FeatureMap
    {
        public static void Configure(this EntityTypeBuilder<Feature> entityBuilder)
        {
            entityBuilder.HasIndex(f => f.Name);
        }
    }
}
