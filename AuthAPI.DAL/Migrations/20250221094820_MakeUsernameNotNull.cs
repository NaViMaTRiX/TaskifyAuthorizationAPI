using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthorizationAPI.Migrations
{
    /// <inheritdoc />
    public partial class MakeUsernameNotNull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Сначала обновляем пустые значения Username, используя Email до @
            migrationBuilder.Sql(@"
                UPDATE ""Users"" 
                SET ""Username"" = SPLIT_PART(""Email"", '@', 1) 
                WHERE ""Username"" = '' OR ""Username"" IS NULL;
            ");
            
            // Добавляем ограничение NOT NULL
            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "Users",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);
        }
    }
}
