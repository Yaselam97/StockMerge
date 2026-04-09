using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaFornitori.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ArticoliFornitori_Brand",
                table: "ArticoliFornitori",
                column: "Brand");

            migrationBuilder.CreateIndex(
                name: "IX_ArticoliFornitori_Colore",
                table: "ArticoliFornitori",
                column: "Colore");

            migrationBuilder.CreateIndex(
                name: "IX_ArticoliFornitori_DataImport",
                table: "ArticoliFornitori",
                column: "DataImport");

            migrationBuilder.CreateIndex(
                name: "IX_ArticoliFornitori_Fornitore",
                table: "ArticoliFornitori",
                column: "Fornitore");

            migrationBuilder.CreateIndex(
                name: "IX_ArticoliFornitori_Taglia",
                table: "ArticoliFornitori",
                column: "Taglia");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ArticoliFornitori_Brand",
                table: "ArticoliFornitori");

            migrationBuilder.DropIndex(
                name: "IX_ArticoliFornitori_Colore",
                table: "ArticoliFornitori");

            migrationBuilder.DropIndex(
                name: "IX_ArticoliFornitori_DataImport",
                table: "ArticoliFornitori");

            migrationBuilder.DropIndex(
                name: "IX_ArticoliFornitori_Fornitore",
                table: "ArticoliFornitori");

            migrationBuilder.DropIndex(
                name: "IX_ArticoliFornitori_Taglia",
                table: "ArticoliFornitori");
        }
    }
}
