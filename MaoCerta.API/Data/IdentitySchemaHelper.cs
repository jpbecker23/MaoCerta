using Microsoft.EntityFrameworkCore;
using MaoCerta.Infrastructure.Data;

namespace MaoCerta.API.Data;

public static class IdentitySchemaHelper
{
    public static async Task EnsureIdentityProfileColumnsAsync(ApplicationDbContext context)
    {
        const string sql = @"
DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'AspNetUsers' AND column_name = 'FirstName'
    ) THEN
        ALTER TABLE ""AspNetUsers"" ADD COLUMN ""FirstName"" character varying(100) NOT NULL DEFAULT '';
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'AspNetUsers' AND column_name = 'LastName'
    ) THEN
        ALTER TABLE ""AspNetUsers"" ADD COLUMN ""LastName"" character varying(100) NOT NULL DEFAULT '';
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'AspNetUsers' AND column_name = 'Address'
    ) THEN
        ALTER TABLE ""AspNetUsers"" ADD COLUMN ""Address"" character varying(200);
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM information_schema.columns
        WHERE table_name = 'AspNetUsers' AND column_name = 'Age'
    ) THEN
        ALTER TABLE ""AspNetUsers"" ADD COLUMN ""Age"" integer;
    END IF;
END$$;";

        await context.Database.ExecuteSqlRawAsync(sql);
    }
}
