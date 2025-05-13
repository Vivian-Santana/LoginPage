using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoginPage.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaColunaStatus : Migration
    {
        /// <inheritdoc />
        //Adiciona a coluna 'Status' do tipo booleano (bit no SQL Server)
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Status",
                table: "Usuarios",
                type: "bit",
                nullable: false,
                defaultValue: true); //padrão true (usuario ativo)
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove a coluna 'Status' caso a migration seja revertida
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Usuarios");
        }
    }
}
