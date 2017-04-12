using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PaderbornUniversity.SILab.Hip.FeatureToggle.Migrations
{
    public partial class AddFeatureToFeatureGroupMappings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FeatureToFeatureGroupMapping_Features_FeatureId",
                table: "FeatureToFeatureGroupMapping");

            migrationBuilder.DropForeignKey(
                name: "FK_FeatureToFeatureGroupMapping_FeatureGroups_GroupId",
                table: "FeatureToFeatureGroupMapping");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FeatureToFeatureGroupMapping",
                table: "FeatureToFeatureGroupMapping");

            migrationBuilder.RenameTable(
                name: "FeatureToFeatureGroupMapping",
                newName: "FeatureToFeatureGroupMappings");

            migrationBuilder.RenameIndex(
                name: "IX_FeatureToFeatureGroupMapping_GroupId",
                table: "FeatureToFeatureGroupMappings",
                newName: "IX_FeatureToFeatureGroupMappings_GroupId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FeatureToFeatureGroupMappings",
                table: "FeatureToFeatureGroupMappings",
                columns: new[] { "FeatureId", "GroupId" });

            migrationBuilder.CreateIndex(
                name: "IX_Features_Name",
                table: "Features",
                column: "Name");

            migrationBuilder.AddForeignKey(
                name: "FK_FeatureToFeatureGroupMappings_Features_FeatureId",
                table: "FeatureToFeatureGroupMappings",
                column: "FeatureId",
                principalTable: "Features",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FeatureToFeatureGroupMappings_FeatureGroups_GroupId",
                table: "FeatureToFeatureGroupMappings",
                column: "GroupId",
                principalTable: "FeatureGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FeatureToFeatureGroupMappings_Features_FeatureId",
                table: "FeatureToFeatureGroupMappings");

            migrationBuilder.DropForeignKey(
                name: "FK_FeatureToFeatureGroupMappings_FeatureGroups_GroupId",
                table: "FeatureToFeatureGroupMappings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FeatureToFeatureGroupMappings",
                table: "FeatureToFeatureGroupMappings");

            migrationBuilder.DropIndex(
                name: "IX_Features_Name",
                table: "Features");

            migrationBuilder.RenameTable(
                name: "FeatureToFeatureGroupMappings",
                newName: "FeatureToFeatureGroupMapping");

            migrationBuilder.RenameIndex(
                name: "IX_FeatureToFeatureGroupMappings_GroupId",
                table: "FeatureToFeatureGroupMapping",
                newName: "IX_FeatureToFeatureGroupMapping_GroupId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FeatureToFeatureGroupMapping",
                table: "FeatureToFeatureGroupMapping",
                columns: new[] { "FeatureId", "GroupId" });

            migrationBuilder.AddForeignKey(
                name: "FK_FeatureToFeatureGroupMapping_Features_FeatureId",
                table: "FeatureToFeatureGroupMapping",
                column: "FeatureId",
                principalTable: "Features",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FeatureToFeatureGroupMapping_FeatureGroups_GroupId",
                table: "FeatureToFeatureGroupMapping",
                column: "GroupId",
                principalTable: "FeatureGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
