using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nolivra.Gateway.Migrations
{
    /// <inheritdoc />
    public partial class AddAssistantRequestLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "assistant_request_logs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RawUserInput = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    RawAiResponse = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: false),
                    ParsedIntent = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Success = table.Column<bool>(type: "boolean", nullable: false),
                    ErrorDetail = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_assistant_request_logs", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "assistant_request_logs");
        }
    }
}
