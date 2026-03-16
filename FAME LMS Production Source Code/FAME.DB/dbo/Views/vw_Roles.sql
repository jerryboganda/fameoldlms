CREATE VIEW [dbo].[vw_Roles]
AS
SELECT        dbo.AspNetUsers.Id, dbo.AspNetUsers.Email, dbo.AspNetRoles.Name
FROM            dbo.AspNetRoles INNER JOIN
                         dbo.AspNetUserRoles ON dbo.AspNetRoles.Id = dbo.AspNetUserRoles.RoleId INNER JOIN
                         dbo.AspNetUsers ON dbo.AspNetUserRoles.UserId = dbo.AspNetUsers.Id