using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebScrapper_Prototype.Migrations
{
    public partial class localInit_Identity : Migration
    {		   
        private string ManagerRoleId = Guid.NewGuid().ToString();
		private string UserRoleId = Guid.NewGuid().ToString();
		private string AdminRoleId = Guid.NewGuid().ToString();

		private string BrandenId = Guid.NewGuid().ToString();
		private string JamesId = Guid.NewGuid().ToString();

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
				@$"INSERT [dbo].[AspNetUsers] ([Id], [FirstName], [LastName], [Email], [Cellphone], [CountryCode], [UserName], [NormalizedUserName], 
[NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], 
[PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount])
VALUES 
(N'{BrandenId}', N'Branden', N'van Staden', N'brandenconnected@gmail.com', N'0', N'ZA', N'Branden',N'BRANDEN', N'BRANDENCONNECTED@GMAIL.COM', 0,  
N'AQAAAAEAACcQAAAAEH+MdVIf9JyT0rq9Q/+YY8LEzJtSkL1kCdWQPPQTo06tmiiuDbpWMfaDthgRlIMfXg==', 
N'JCVZLCTXVZ5SFOBGLTDTSR5PVYJY6Q4M', N'1049d989-5078-4780-8e9e-520fd71f3f11', N'0815650206', 0, 0, NULL, 1, 0)");
			migrationBuilder.Sql(
	@$"INSERT [dbo].[AspNetUsers] ([Id], [FirstName], [LastName], [Email], [Cellphone], [CountryCode], [UserName], [NormalizedUserName], 
[NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], 
[PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount])
VALUES 
(N'{JamesId}', N'James', N'Muir', N'james@thecloudmasters.com', N'0', N'ZA', N'James',N'JAMES', N'JAMES@THECLOUDMASTERS.COM', 0,  
N'AQAAAAEAACcQAAAAEK5j7OyYciyuMldcSD7kA14elPaWqy3lEUaqspKimiAJ01V8t9vXO22sC4q6jlB7qA==', 
N'4JQ7UDPB2WSCDL2WGV6MBS7FCQDDC72R', N'b68db617-c93d-40f5-a903-fb6da0fbf9c9', N'0815650522', 0, 0, NULL, 1, 0)");
		}
		private void SeedUserRoles(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.Sql(@$"
        INSERT INTO [dbo].[AspNetUserRoles]
           ([UserId]
           ,[RoleId])
        VALUES
           ('{BrandenId}', '{ManagerRoleId}');
        INSERT INTO [dbo].[AspNetUserRoles]
           ([UserId]
           ,[RoleId])
        VALUES
           ('{BrandenId}', '{UserRoleId}');
        INSERT INTO [dbo].[AspNetUserRoles]
           ([UserId]
           ,[RoleId])
        VALUES
           ('{BrandenId}', '{AdminRoleId}');");

			migrationBuilder.Sql(@$"
        INSERT INTO [dbo].[AspNetUserRoles]
           ([UserId]
           ,[RoleId])
        VALUES
           ('{JamesId}', '{ManagerRoleId}');
        INSERT INTO [dbo].[AspNetUserRoles]
           ([UserId]
           ,[RoleId])
        VALUES
           ('{JamesId}', '{UserRoleId}');");
		}

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
