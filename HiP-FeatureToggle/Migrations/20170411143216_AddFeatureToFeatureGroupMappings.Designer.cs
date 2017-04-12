using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using PaderbornUniversity.SILab.Hip.FeatureToggle.Data;

namespace PaderbornUniversity.SILab.Hip.FeatureToggle.Migrations
{
    [DbContext(typeof(ToggleDbContext))]
    [Migration("20170411143216_AddFeatureToFeatureGroupMappings")]
    partial class AddFeatureToFeatureGroupMappings
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn)
                .HasAnnotation("ProductVersion", "1.1.1");

            modelBuilder.Entity("PaderbornUniversity.SILab.Hip.FeatureToggle.Models.Entity.Feature", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Name");

                    b.Property<int?>("ParentId");

                    b.HasKey("Id");

                    b.HasIndex("Name");

                    b.HasIndex("ParentId");

                    b.ToTable("Features");
                });

            modelBuilder.Entity("PaderbornUniversity.SILab.Hip.FeatureToggle.Models.Entity.FeatureGroup", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<bool>("IsProtected");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.HasIndex("Name");

                    b.ToTable("FeatureGroups");
                });

            modelBuilder.Entity("PaderbornUniversity.SILab.Hip.FeatureToggle.Models.Entity.FeatureToFeatureGroupMapping", b =>
                {
                    b.Property<int>("FeatureId");

                    b.Property<int>("GroupId");

                    b.HasKey("FeatureId", "GroupId");

                    b.HasIndex("GroupId");

                    b.ToTable("FeatureToFeatureGroupMappings");
                });

            modelBuilder.Entity("PaderbornUniversity.SILab.Hip.FeatureToggle.Models.Entity.User", b =>
                {
                    b.Property<string>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("FeatureGroupId");

                    b.HasKey("Id");

                    b.HasIndex("FeatureGroupId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("PaderbornUniversity.SILab.Hip.FeatureToggle.Models.Entity.Feature", b =>
                {
                    b.HasOne("PaderbornUniversity.SILab.Hip.FeatureToggle.Models.Entity.Feature", "Parent")
                        .WithMany("Children")
                        .HasForeignKey("ParentId");
                });

            modelBuilder.Entity("PaderbornUniversity.SILab.Hip.FeatureToggle.Models.Entity.FeatureToFeatureGroupMapping", b =>
                {
                    b.HasOne("PaderbornUniversity.SILab.Hip.FeatureToggle.Models.Entity.Feature", "Feature")
                        .WithMany("GroupsWhereEnabled")
                        .HasForeignKey("FeatureId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("PaderbornUniversity.SILab.Hip.FeatureToggle.Models.Entity.FeatureGroup", "Group")
                        .WithMany("EnabledFeatures")
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("PaderbornUniversity.SILab.Hip.FeatureToggle.Models.Entity.User", b =>
                {
                    b.HasOne("PaderbornUniversity.SILab.Hip.FeatureToggle.Models.Entity.FeatureGroup", "FeatureGroup")
                        .WithMany("Members")
                        .HasForeignKey("FeatureGroupId");
                });
        }
    }
}
