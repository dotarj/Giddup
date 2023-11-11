﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Giddup.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventProjectionOffsets",
                columns: table => new
                {
                    AggregateType = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventProjectionOffsets", x => x.AggregateType);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    Offset = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityAlwaysColumn),
                    AggregateId = table.Column<Guid>(type: "uuid", nullable: false),
                    AggregateType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    AggregateVersion = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Data = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.Offset);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    LastName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorkItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PullRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceBranch = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    TargetBranch = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AutoCompleteMode = table.Column<int>(type: "integer", nullable: false),
                    CheckForLinkedWorkItemsMode = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PullRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PullRequests_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OptionalReviewer",
                columns: table => new
                {
                    PullRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Feedback = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OptionalReviewer", x => new { x.UserId, x.PullRequestId });
                    table.ForeignKey(
                        name: "FK_OptionalReviewer_PullRequests_PullRequestId",
                        column: x => x.PullRequestId,
                        principalTable: "PullRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OptionalReviewer_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PullRequestWorkItem",
                columns: table => new
                {
                    PullRequestsId = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkItemsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PullRequestWorkItem", x => new { x.PullRequestsId, x.WorkItemsId });
                    table.ForeignKey(
                        name: "FK_PullRequestWorkItem_PullRequests_PullRequestsId",
                        column: x => x.PullRequestsId,
                        principalTable: "PullRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PullRequestWorkItem_WorkItems_WorkItemsId",
                        column: x => x.WorkItemsId,
                        principalTable: "WorkItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RequiredReviewer",
                columns: table => new
                {
                    PullRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Feedback = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequiredReviewer", x => new { x.UserId, x.PullRequestId });
                    table.ForeignKey(
                        name: "FK_RequiredReviewer_PullRequests_PullRequestId",
                        column: x => x.PullRequestId,
                        principalTable: "PullRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RequiredReviewer_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "FirstName", "LastName" },
                values: new object[,]
                {
                    { new Guid("6e76dc39-633a-422d-a922-626c3c220e6b"), "Albert", "Einstein" },
                    { new Guid("769f1cfe-eaab-4a4f-9776-755b89dfb973"), "Isaac", "Newton" },
                    { new Guid("8dd689c0-7c67-4936-8e89-c4e4896396bc"), "Niels", "Bohr" },
                    { new Guid("e9faa5fd-2832-4d47-ac55-0655a20e274e"), "Galileo", "Galilei" }
                });

            migrationBuilder.InsertData(
                table: "WorkItems",
                columns: new[] { "Id", "Title" },
                values: new object[,]
                {
                    { new Guid("06256a69-6160-4f38-9bc0-4e255fae4087"), "Lorem ipsum dolor sit amet, consectetur adipiscing elit." },
                    { new Guid("ce037a68-9e4e-4a48-994b-f702dd63f102"), "In congue erat lacus, vitae iaculis turpis accumsan vel." }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Events_AggregateId_AggregateVersion",
                table: "Events",
                columns: new[] { "AggregateId", "AggregateVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OptionalReviewer_PullRequestId",
                table: "OptionalReviewer",
                column: "PullRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_PullRequests_CreatedById",
                table: "PullRequests",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_PullRequestWorkItem_WorkItemsId",
                table: "PullRequestWorkItem",
                column: "WorkItemsId");

            migrationBuilder.CreateIndex(
                name: "IX_RequiredReviewer_PullRequestId",
                table: "RequiredReviewer",
                column: "PullRequestId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventProjectionOffsets");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "OptionalReviewer");

            migrationBuilder.DropTable(
                name: "PullRequestWorkItem");

            migrationBuilder.DropTable(
                name: "RequiredReviewer");

            migrationBuilder.DropTable(
                name: "WorkItems");

            migrationBuilder.DropTable(
                name: "PullRequests");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}