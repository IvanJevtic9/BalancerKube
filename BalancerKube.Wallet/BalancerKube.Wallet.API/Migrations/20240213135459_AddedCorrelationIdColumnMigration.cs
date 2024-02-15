using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BalancerKube.Wallet.API.Migrations
{
    /// <inheritdoc />
    public partial class AddedCorrelationIdColumnMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Code",
                table: "Transactions");

            migrationBuilder.AddColumn<Guid>(
                name: "CorrelationId",
                table: "Transactions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CorrelationId",
                table: "Transactions");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Transactions",
                type: "text",
                nullable: true);
        }
    }
}
