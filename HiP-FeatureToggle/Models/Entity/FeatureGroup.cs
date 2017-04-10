using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Collections.Generic;

namespace PaderbornUniversity.SILab.Hip.FeatureToggle.Models.Entity
{
    public class FeatureGroup
    {
        public const string DefaultGroupName = "Default"; // name of the default group for authorized users
        public const string PublicGroupName = "Public"; // name of the group for unauthorized users

        public int Id { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// If true, the group can't be deleted or renamed.
        /// </summary>
        public bool IsProtected { get; set; }

        public IList<User> Members { get; set; }

        /// <summary>
        /// The features that are enabled in this feature group.
        /// The opposite direction of this many-to-many relation is <see cref="Feature.GroupsWhereEnabled"/>.
        /// </summary>
        public IList<FeatureToFeatureGroupMapping> EnabledFeatures { get; set; }

        public override string ToString() =>
            $"'{Name}' ({Members?.Count ?? 0} members, {EnabledFeatures?.Count ?? 0} features)";
    }

    public static class FeatureGroupMap
    {
        public static void Configure(this EntityTypeBuilder<FeatureGroup> entityBuilder)
        {
            entityBuilder.HasIndex(group => group.Name);
        }
    }
}
