using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using UTCert.Model.Database;
using UTCert.Models.Database;

namespace UTCert.Data.Repository.Common.DbContext;

public class DatabaseContext : Microsoft.EntityFrameworkCore.DbContext, IDbContext{
    
    public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options)
    {
    }

    public void MarkAsModified(object o, string propertyName)
    {
        this.Entry(o).Property(propertyName).IsModified = true;
    }

    public async Task<T> ExecuteStoredProcedure<T>(string storedProcedure, params SqlParameter[] parameters)
    {
        await Database.ExecuteSqlRawAsync($"EXEC {storedProcedure}", parameters);
        var outputParam = parameters.FirstOrDefault(p => p.Direction == ParameterDirection.Output);

        if (outputParam?.Value is T result)
        {
            return result;
        }

        throw new InvalidOperationException("The output value is not of the expected type.");
    }
    
    #region Tables
    
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<Certificate> Certificates { get; set; }
    public virtual DbSet<Contact> Contacts { get; set; }
    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    #endregion Tables
}