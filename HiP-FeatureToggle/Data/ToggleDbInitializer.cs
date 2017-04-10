using PaderbornUniversity.SILab.Hip.FeatureToggle.Models.Entity;
using System.Linq;

namespace PaderbornUniversity.SILab.Hip.FeatureToggle.Data
{
    /// <summary>
    /// Populates an empty database with the default feature group.
    /// </summary>
    public static class ToggleDbInitializer
    {
        public static void Initialize(ToggleDbContext db)
        {
            if (db.FeatureGroups.Any())
                return; // DB is already seeded

            var defaultGroup = new FeatureGroup
            {
                Name = FeatureGroup.DefaultGroupName,
                IsProtected = true
            };

            var publicGroup = new FeatureGroup
            {
                Name = FeatureGroup.PublicGroupName,
                IsProtected = true
            };

            db.FeatureGroups.AddRange(defaultGroup, publicGroup);
            db.SaveChanges();
        }
    }
}
