
You can use the Nuget Package Manager built-in scaffolld feature of the Nuget Package Manager to create your entity models.

PostgreSQL E.g.
/////////////


Scaffold-DbContext "Server=localhost;Port=5432; Database=DbConnectorTest2; User ID=postgres; Password=postgress;" Npgsql.EntityFrameworkCore.PostgreSQL -DataAnnotations -Force -OutputDir Context/Entities


/////////////