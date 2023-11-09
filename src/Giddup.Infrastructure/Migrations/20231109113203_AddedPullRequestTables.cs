using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Giddup.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedPullRequestTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PullRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AutoCompleteMode = table.Column<int>(type: "integer", nullable: false),
                    CheckForLinkedWorkItemsMode = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PullRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OptionalReviewer",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Feedback = table.Column<int>(type: "integer", nullable: false),
                    PullRequestId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OptionalReviewer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OptionalReviewer_PullRequests_PullRequestId",
                        column: x => x.PullRequestId,
                        principalTable: "PullRequests",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RequiredReviewer",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Feedback = table.Column<int>(type: "integer", nullable: false),
                    PullRequestId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequiredReviewer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequiredReviewer_PullRequests_PullRequestId",
                        column: x => x.PullRequestId,
                        principalTable: "PullRequests",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_OptionalReviewer_PullRequestId",
                table: "OptionalReviewer",
                column: "PullRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_RequiredReviewer_PullRequestId",
                table: "RequiredReviewer",
                column: "PullRequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OptionalReviewer");

            migrationBuilder.DropTable(
                name: "RequiredReviewer");

            migrationBuilder.DropTable(
                name: "PullRequests");
        }
    }
}
