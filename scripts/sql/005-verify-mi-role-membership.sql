-- Verifies managed identity least-privilege role membership.
-- Replace @UserName if the managed identity database principal name differs.

DECLARE @UserName sysname = N'PlantwiseOnSQLserver';

SELECT
	member.name AS principal_name,
	role_principal.name AS role_name
FROM sys.database_role_members role_members
JOIN sys.database_principals role_principal
	ON role_members.role_principal_id = role_principal.principal_id
JOIN sys.database_principals member
	ON role_members.member_principal_id = member.principal_id
WHERE member.name = @UserName
ORDER BY role_name;

IF EXISTS (
	SELECT 1
	FROM sys.database_role_members role_members
	JOIN sys.database_principals role_principal
		ON role_members.role_principal_id = role_principal.principal_id
	JOIN sys.database_principals member
		ON role_members.member_principal_id = member.principal_id
	WHERE member.name = @UserName
	  AND role_principal.name = N'db_owner')
BEGIN
	THROW 51000, 'Validation failed: managed identity must not be assigned db_owner.', 1;
END;
