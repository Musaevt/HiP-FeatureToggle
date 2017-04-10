using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace PaderbornUniversity.SILab.Hip.FeatureToggle.Models.Entity
{
    /// <remarks>
    /// Since EF Core does not (yet) support many-to-many relationships, the relationship between features
    /// and feature groups is modelled as two 1-to-many relationships. In other words, this class acts as
    /// the join table.
    ///
    /// [Feature]----------------[FeatureToFeatureGroupMapping]-------------[FeatureGroup]
    ///          1              n                              m           1
    /// </remarks>
    public class FeatureToFeatureGroupMapping
    {
        public Feature Feature { get; set; }

        public int FeatureId { get; set; }

        public FeatureGroup Group { get; set; }

        public int GroupId { get; set; }

        public FeatureToFeatureGroupMapping()
        {
        }

        public FeatureToFeatureGroupMapping(Feature feature, FeatureGroup group)
        {
            Feature = feature;
            Group = group;
        }
    }

    public static class FeatureToFeatureGroupMappingMap
    {
        public static void Configure(this EntityTypeBuilder<FeatureToFeatureGroupMapping> entityBuilder)
        {
            entityBuilder.HasKey(m => new { m.FeatureId, m.GroupId });
        }
    }
}
