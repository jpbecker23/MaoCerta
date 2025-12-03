using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MaoCerta.API.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentPathToProfessional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.AddColumn<string>(
                name: "DocumentPath",
                table: "Professionals",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocumentPath",
                table: "Professionals");

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedAt", "Description", "Icon", "IsActive", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 10, 29, 1, 21, 57, 473, DateTimeKind.Utc).AddTicks(4978), "Serviços de manutenção e reparo de chuveiros", "🚿", true, "Manutenção de Chuveiros", null },
                    { 2, new DateTime(2025, 10, 29, 1, 21, 57, 473, DateTimeKind.Utc).AddTicks(4985), "Serviços de jardinagem e paisagismo", "🌱", true, "Jardinagem", null },
                    { 3, new DateTime(2025, 10, 29, 1, 21, 57, 473, DateTimeKind.Utc).AddTicks(4990), "Serviços de limpeza doméstica", "🧹", true, "Limpeza Residencial", null },
                    { 4, new DateTime(2025, 10, 29, 1, 21, 57, 473, DateTimeKind.Utc).AddTicks(4995), "Serviços elétricos residenciais", "⚡", true, "Elétrica", null },
                    { 5, new DateTime(2025, 10, 29, 1, 21, 57, 473, DateTimeKind.Utc).AddTicks(5000), "Serviços hidráulicos e encanamento", "🔧", true, "Hidráulica", null },
                    { 6, new DateTime(2025, 10, 29, 1, 21, 57, 473, DateTimeKind.Utc).AddTicks(5005), "Serviços de pintura residencial", "🎨", true, "Pintura", null }
                });
        }
    }
}
