using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniApp1Api.Migrations
{
    /// <inheritdoc />
    public partial class TMMealDb2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RestourantOwnerApp");

            migrationBuilder.CreateTable(
                name: "Restourants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    City = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    District = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Neighborhood = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DeliveryService = table.Column<bool>(type: "boolean", nullable: false),
                    PassportOrTaxNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    RestaurantName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    CuisineType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ReferenceCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RestourantPhoneNumber = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Restourants", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Restourants");

            migrationBuilder.CreateTable(
                name: "RestourantOwnerApp",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false),
                    City = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    CuisineType = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    DeliveryService = table.Column<bool>(type: "boolean", nullable: false),
                    District = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Email = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IpAddress = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    LastName = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Neighborhood = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    NormalizedEmail = table.Column<string>(type: "text", nullable: true),
                    NormalizedUserName = table.Column<string>(type: "text", nullable: true),
                    PassportOrTaxNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Password = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: false),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    ReferenceCode = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    RestaurantName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestourantOwnerApp", x => x.Id);
                });
        }
    }
}
