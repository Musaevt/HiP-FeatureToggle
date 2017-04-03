using Microsoft.EntityFrameworkCore;
using PaderbornUniversity.SILab.Hip.FeatureToggle.Models;

namespace PaderbornUniversity.SILab.Hip.FeatureToggle.Data
{
    public class ToggleDbContext: DbContext
    {
		/// <summary>
		/// Constructor that passes a parameterized DbContextOptions object to the base constructor.
		/// </summary>
		/// <param name="context">The context options object -- passed via DI.</param>
		public ToggleDbContext(DbContextOptions<ToggleDbContext> context): base(context) {}
        /*
         * TODO: add DbSets
         * Example:
         * public DbSet<AssociatedTopic> AssociatedTopics { get; set; }
         */

		public DbSet<Values> Values { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            /*
             * TODO: create any mappings that you need
             * Example:
             * modelBuilder.Entity<User>().HasIndex(b => b.Email).IsUnique();
             * new AssociatedTopicMap(modelBuilder.Entity<AssociatedTopic>());
             */
        }
    }
}
