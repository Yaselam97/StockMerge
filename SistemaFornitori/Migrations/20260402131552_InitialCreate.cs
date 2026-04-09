using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaFornitori.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ArticoliFornitori",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EAN = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: false),
                    Articolo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Fornitore = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Taglia = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Colore = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SKU = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Prezzo = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    DataImport = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticoliFornitori", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ArticoliFornitori_EAN",
                table: "ArticoliFornitori",
                column: "EAN",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ArticoliFornitori");
        }
    }
}
