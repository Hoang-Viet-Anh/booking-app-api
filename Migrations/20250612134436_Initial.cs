using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace booking_api.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Coworking",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Location = table.Column<string>(type: "text", nullable: false),
                    ImageUrls = table.Column<List<string>>(type: "text[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coworking", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Workspace",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    ImageUrls = table.Column<List<string>>(type: "text[]", nullable: false),
                    Amenities = table.Column<List<string>>(type: "text[]", nullable: false),
                    AreaType = table.Column<string>(type: "text", nullable: false),
                    AreaTypeEmoji = table.Column<string>(type: "text", nullable: false),
                    MaxBookingDays = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Workspace", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkspaceCapacity",
                columns: table => new
                {
                    CoworkingModelId = table.Column<Guid>(type: "uuid", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkspaceCapacity", x => new { x.CoworkingModelId, x.Id });
                    table.ForeignKey(
                        name: "FK_WorkspaceCapacity_Coworking_CoworkingModelId",
                        column: x => x.CoworkingModelId,
                        principalTable: "Coworking",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Booking",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    CoworkingId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkspaceId = table.Column<Guid>(type: "uuid", nullable: false),
                    DateSlot_StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateSlot_EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateSlot_IsStartTimeSelected = table.Column<bool>(type: "boolean", nullable: false),
                    DateSlot_IsEndTimeSelected = table.Column<bool>(type: "boolean", nullable: false),
                    RoomSizes = table.Column<List<int>>(type: "integer[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Booking", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Booking_Coworking_CoworkingId",
                        column: x => x.CoworkingId,
                        principalTable: "Coworking",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Booking_Workspace_WorkspaceId",
                        column: x => x.WorkspaceId,
                        principalTable: "Workspace",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Availability",
                columns: table => new
                {
                    WorkspaceCapacityCoworkingModelId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkspaceCapacityId = table.Column<int>(type: "integer", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Amounts = table.Column<int>(type: "integer", nullable: false),
                    Capacity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Availability", x => new { x.WorkspaceCapacityCoworkingModelId, x.WorkspaceCapacityId, x.Id });
                    table.ForeignKey(
                        name: "FK_Availability_WorkspaceCapacity_WorkspaceCapacityCoworkingMo~",
                        columns: x => new { x.WorkspaceCapacityCoworkingModelId, x.WorkspaceCapacityId },
                        principalTable: "WorkspaceCapacity",
                        principalColumns: new[] { "CoworkingModelId", "Id" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Booking_CoworkingId",
                table: "Booking",
                column: "CoworkingId");

            migrationBuilder.CreateIndex(
                name: "IX_Booking_WorkspaceId",
                table: "Booking",
                column: "WorkspaceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Availability");

            migrationBuilder.DropTable(
                name: "Booking");

            migrationBuilder.DropTable(
                name: "WorkspaceCapacity");

            migrationBuilder.DropTable(
                name: "Workspace");

            migrationBuilder.DropTable(
                name: "Coworking");
        }
    }
}
