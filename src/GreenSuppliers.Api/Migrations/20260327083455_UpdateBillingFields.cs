using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GreenSuppliers.Api.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBillingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Subscriptions",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "pending",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "active");

            migrationBuilder.AddColumn<string>(
                name: "PayFastToken",
                table: "Subscriptions",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TrialEnd",
                table: "Subscriptions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PrioritySupport",
                table: "Plans",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "TrialDays",
                table: "Plans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "SavedSuppliers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "NEWSEQUENTIALID()"),
                    BuyerUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SupplierProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedSuppliers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SavedSuppliers_SupplierProfiles_SupplierProfileId",
                        column: x => x.SupplierProfileId,
                        principalTable: "SupplierProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SavedSuppliers_Users_BuyerUserId",
                        column: x => x.BuyerUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_EmailVerificationToken",
                table: "Users",
                column: "EmailVerificationToken",
                filter: "[EmailVerificationToken] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Users_PasswordResetToken",
                table: "Users",
                column: "PasswordResetToken",
                filter: "[PasswordResetToken] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SavedSuppliers_BuyerUser_SupplierProfile",
                table: "SavedSuppliers",
                columns: new[] { "BuyerUserId", "SupplierProfileId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SavedSuppliers_SupplierProfileId",
                table: "SavedSuppliers",
                column: "SupplierProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SavedSuppliers");

            migrationBuilder.DropIndex(
                name: "IX_Users_EmailVerificationToken",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_PasswordResetToken",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PayFastToken",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "TrialEnd",
                table: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "PrioritySupport",
                table: "Plans");

            migrationBuilder.DropColumn(
                name: "TrialDays",
                table: "Plans");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Subscriptions",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "active",
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20,
                oldDefaultValue: "pending");
        }
    }
}
