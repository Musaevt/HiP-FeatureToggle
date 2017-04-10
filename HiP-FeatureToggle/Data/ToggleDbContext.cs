using Microsoft.EntityFrameworkCore;
using PaderbornUniversity.SILab.Hip.FeatureToggle.Models;
using PaderbornUniversity.SILab.Hip.FeatureToggle.Models.Entity;

namespace PaderbornUniversity.SILab.Hip.FeatureToggle.Data
{
    public class ToggleDbContext: DbContext
    {
        /// <summary>
        /// Constructor that passes a parameterized DbContextOptions object to the base constructor.
        /// </summary>
        /// <param name="context">The context options object -- passed via DI.</param>
        public ToggleDbContext(DbContextOptions<ToggleDbContext> context): base(context) {}

        public DbSet<User> Users { get; set; }

        public DbSet<Feature> Features { get; set; }

        public DbSet<FeatureGroup> FeatureGroups { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            /*
             * TODO: create any mappings that you need
             * Example:
             * modelBuilder.Entity<User>().HasIndex(b => b.Email).IsUnique();
             * new AssociatedTopicMap(modelBuilder.Entity<AssociatedTopic>());
             */
            modelBuilder.Entity<FeatureToFeatureGroupMapping>().Configure();
            modelBuilder.Entity<FeatureGroup>().Configure();
        }
    }
}
