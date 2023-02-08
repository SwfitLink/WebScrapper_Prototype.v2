using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebScrapper_Prototype.Migrations.ApplicationDb
{
    public partial class IdentitySeed : Migration
    {
		private string ManagerRoleId = Guid.NewGuid().ToString();
		private string UserRoleId = Guid.NewGuid().ToString();
		private string AdminRoleId = Guid.NewGuid().ToString();

		private string AdminUserId = Guid.NewGuid().ToString();

		protected override void Up(MigrationBuilder migrationBuilder)
		{
			SeedRolesSQL(migrationBuilder);

			SeedUser(migrationBuilder);

			SeedUserRoles(migrationBuilder);
		}

		private void SeedRolesSQL(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql(@$"INSERT INTO [dbo].[AspNetRoles] ([Id],[Name],[NormalizedName],[ConcurrencyStamp])
            VALUES ('{AdminRoleId}', 'Administrator', 'ADMINISTRATOR', null);");
			migrationBuilder.Sql(@$"INSERT INTO [dbo].[AspNetRoles] ([Id],[Name],[NormalizedName],[ConcurrencyStamp])
            VALUES ('{ManagerRoleId}', 'Manager', 'MANAGER', null);");
			migrationBuilder.Sql(@$"INSERT INTO [dbo].[AspNetRoles] ([Id],[Name],[NormalizedName],[ConcurrencyStamp])
            VALUES ('{UserRoleId}', 'User', 'USER', null);");
		}

		private void SeedUser(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql(
				@$"INSERT [dbo].[AspNetUsers] ([Id], [FirstName], [LastName], [Cellphone], [CountryCode], [UserName], [NormalizedUserName], 
[Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], 
[PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount]) 
VALUES 
(N'{AdminUserId}', N'Admin', N'User',N'0',N'ZA', N'admin@swiftlink.com', N'ADMIN@SWIFTLINK.COM', 
N'admin@swiftlink.com', N'ADMIN@SWIFTLINK.COM', 0, 
N'AQAAAAEAACcQAAAAELS/l+IHqAN33EpcsSqLTuJfoYeEhZFZah9RQ8jaY7vqmj3ujcUvulfr9qpS2hO+xQ==', 
N'CJF5NK5UFNUWSSN4Z5ASLKEXR2JXZ6T3', N'11e93459-e34e-4541-865a-23f0d36de631', NULL, 0, 0, NULL, 1, 0)");
		}

		private void SeedUserRoles(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql(@$"
        INSERT INTO [dbo].[AspNetUserRoles]
           ([UserId]
           ,[RoleId])
        VALUES
           ('{AdminUserId}', '{AdminRoleId}');
        INSERT INTO [dbo].[AspNetUserRoles]
           ([UserId]
           ,[RoleId])
        VALUES
           ('{AdminUserId}', '{ManagerRoleId}');");

		}

		protected override void Down(MigrationBuilder migrationBuilder)
		{

		}
	}
}
