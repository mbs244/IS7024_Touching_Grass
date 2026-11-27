-- Creates the Azure App Service managed identity database user and grants
-- least-privilege permissions required for EF Core startup migrations and runtime data access.
-- Replace [PlantwiseOnSQLserver] if the managed identity display name differs.

CREATE USER [PlantwiseOnSQLserver] FROM EXTERNAL PROVIDER;

ALTER ROLE db_datareader ADD MEMBER [PlantwiseOnSQLserver];
ALTER ROLE db_datawriter ADD MEMBER [PlantwiseOnSQLserver];
ALTER ROLE db_ddladmin ADD MEMBER [PlantwiseOnSQLserver];
