using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BackEndElectronicaDeny.Migrations
{
    /// <inheritdoc />
    public partial class FixRolPermisosComposite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.tables 
        WHERE table_schema = 'public' AND table_name = 'RolPermisos'
    ) THEN
        CREATE TABLE ""RolPermisos""(
            ""RolId"" integer NOT NULL,
            ""PermisoId"" integer NOT NULL,
            CONSTRAINT ""PK_RolPermisos"" PRIMARY KEY (""RolId"", ""PermisoId""),
            CONSTRAINT ""FK_RolPermisos_Roles_RolId"" 
                FOREIGN KEY (""RolId"") REFERENCES ""Roles"" (""Id"") ON DELETE CASCADE,
            CONSTRAINT ""FK_RolPermisos_Permisos_PermisoId"" 
                FOREIGN KEY (""PermisoId"") REFERENCES ""Permisos"" (""Id"") ON DELETE CASCADE
        );
    END IF;
END
$$;
");

            // 2) Si existe columna Id, migrar a PK compuesta
            migrationBuilder.Sql(@"
DO $$
DECLARE col_exists boolean;
BEGIN
    SELECT EXISTS (
        SELECT 1 
        FROM information_schema.columns 
        WHERE table_schema='public' AND table_name='RolPermisos' AND column_name='Id'
    ) INTO col_exists;

    IF col_exists THEN
        -- soltar PK si apunta a Id (ignore si no existe)
        BEGIN
            ALTER TABLE ""RolPermisos"" DROP CONSTRAINT IF EXISTS ""PK_RolPermisos"";
        EXCEPTION WHEN others THEN
            -- noop
        END;

        -- borrar índices que usen Id (por si los hubiera)
        DROP INDEX IF EXISTS ""IX_RolPermisos_Id"";
        DROP INDEX IF EXISTS ""IX_RolPermisos_RolId_PermisoId"";

        -- quitar columna Id
        ALTER TABLE ""RolPermisos"" DROP COLUMN IF EXISTS ""Id"";

        -- crear PK compuesta
        ALTER TABLE ""RolPermisos""
            ADD CONSTRAINT ""PK_RolPermisos"" PRIMARY KEY (""RolId"", ""PermisoId"");
    END IF;
END
$$;
");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
