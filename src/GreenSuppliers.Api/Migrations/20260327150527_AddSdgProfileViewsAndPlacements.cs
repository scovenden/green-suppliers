using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GreenSuppliers.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSdgProfileViewsAndPlacements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProfileViews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    SupplierProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ViewerIp = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    ViewerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Referrer = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileViews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProfileViews_SupplierProfiles_SupplierProfileId",
                        column: x => x.SupplierProfileId,
                        principalTable: "SupplierProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sdgs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Color = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sdgs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SupplierSdgs",
                columns: table => new
                {
                    SupplierProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SdgId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupplierSdgs", x => new { x.SupplierProfileId, x.SdgId });
                    table.ForeignKey(
                        name: "FK_SupplierSdgs_Sdgs_SdgId",
                        column: x => x.SdgId,
                        principalTable: "Sdgs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SupplierSdgs_SupplierProfiles_SupplierProfileId",
                        column: x => x.SupplierProfileId,
                        principalTable: "SupplierProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Sdgs",
                columns: new[] { "Id", "Color", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "#E5243B", "End poverty in all its forms everywhere", "No Poverty" },
                    { 2, "#DDA63A", "End hunger, achieve food security and improved nutrition and promote sustainable agriculture", "Zero Hunger" },
                    { 3, "#4C9F38", "Ensure healthy lives and promote well-being for all at all ages", "Good Health and Well-Being" },
                    { 4, "#C5192D", "Ensure inclusive and equitable quality education and promote lifelong learning opportunities for all", "Quality Education" },
                    { 5, "#FF3A21", "Achieve gender equality and empower all women and girls", "Gender Equality" },
                    { 6, "#26BDE2", "Ensure availability and sustainable management of water and sanitation for all", "Clean Water and Sanitation" },
                    { 7, "#FCC30B", "Ensure access to affordable, reliable, sustainable and modern energy for all", "Affordable and Clean Energy" },
                    { 8, "#A21942", "Promote sustained, inclusive and sustainable economic growth, full and productive employment and decent work for all", "Decent Work and Economic Growth" },
                    { 9, "#FD6925", "Build resilient infrastructure, promote inclusive and sustainable industrialization and foster innovation", "Industry, Innovation and Infrastructure" },
                    { 10, "#DD1367", "Reduce inequality within and among countries", "Reduced Inequalities" },
                    { 11, "#FD9D24", "Make cities and human settlements inclusive, safe, resilient and sustainable", "Sustainable Cities and Communities" },
                    { 12, "#BF8B2E", "Ensure sustainable consumption and production patterns", "Responsible Consumption and Production" },
                    { 13, "#3F7E44", "Take urgent action to combat climate change and its impacts", "Climate Action" },
                    { 14, "#0A97D9", "Conserve and sustainably use the oceans, seas and marine resources for sustainable development", "Life Below Water" },
                    { 15, "#56C02B", "Protect, restore and promote sustainable use of terrestrial ecosystems, sustainably manage forests, combat desertification, and halt and reverse land degradation and halt biodiversity loss", "Life on Land" },
                    { 16, "#00689D", "Promote peaceful and inclusive societies for sustainable development, provide access to justice for all and build effective, accountable and inclusive institutions at all levels", "Peace, Justice and Strong Institutions" },
                    { 17, "#19486A", "Strengthen the means of implementation and revitalize the Global Partnership for Sustainable Development", "Partnerships for the Goals" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProfileViews_SupplierProfileId_CreatedAt",
                table: "ProfileViews",
                columns: new[] { "SupplierProfileId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SupplierSdgs_SdgId",
                table: "SupplierSdgs",
                column: "SdgId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProfileViews");

            migrationBuilder.DropTable(
                name: "SupplierSdgs");

            migrationBuilder.DropTable(
                name: "Sdgs");
        }
    }
}
