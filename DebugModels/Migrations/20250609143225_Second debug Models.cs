using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DebugModels.Migrations
{
    /// <inheritdoc />
    public partial class SeconddebugModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Sections_SectionId",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Courses_SectionId",
                table: "Courses");

            migrationBuilder.RenameColumn(
                name: "SectionId",
                table: "Courses",
                newName: "SectionsId");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_SectionsId",
                table: "Courses",
                column: "SectionsId",
                unique: true,
                filter: "[SectionsId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Sections_SectionsId",
                table: "Courses",
                column: "SectionsId",
                principalTable: "Sections",
                principalColumn: "SectionsId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Courses_Sections_SectionsId",
                table: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Courses_SectionsId",
                table: "Courses");

            migrationBuilder.RenameColumn(
                name: "SectionsId",
                table: "Courses",
                newName: "SectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_SectionId",
                table: "Courses",
                column: "SectionId",
                unique: true,
                filter: "[SectionId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_Sections_SectionId",
                table: "Courses",
                column: "SectionId",
                principalTable: "Sections",
                principalColumn: "SectionsId",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
