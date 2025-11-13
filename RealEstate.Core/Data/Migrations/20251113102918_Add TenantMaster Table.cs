using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RealEstate.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTenantMasterTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TenantMasterEntity",
                table: "TenantMasterEntity");

            migrationBuilder.RenameTable(
                name: "TenantMasterEntity",
                newName: "TenantMaster");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TenantMaster",
                table: "TenantMaster",
                column: "TenantIDP");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TenantMaster",
                table: "TenantMaster");

            migrationBuilder.RenameTable(
                name: "TenantMaster",
                newName: "TenantMasterEntity");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TenantMasterEntity",
                table: "TenantMasterEntity",
                column: "TenantIDP");
        }
    }
}
