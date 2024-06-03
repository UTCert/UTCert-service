using Microsoft.EntityFrameworkCore;
using UTCert.Data.Repository;
using UTCert.Data.Repository.Common.DbContext;
using UTCert.Data.Repository.Interface;
using UTCert.Service.BusinessLogic;
using UTCert.Service.BusinessLogic.Interface;
using UTCert.Service.Helper;
using UTCert.Service.Helper.Interface;
using UTCert.Service.Helper.JwtUtils;

namespace utcert_service.Code;

public static class DependenciesInjectionRegister
{
    public static void RegisterDependencies(this WebApplicationBuilder builder)
    {
        var dbConfig = new DatabaseConfig();
        builder.Configuration.GetSection("DatabaseConfig").Bind(dbConfig);
        
        builder.Services.AddDbContext<DatabaseContext>(options => options
            .UseSqlServer(builder.Configuration["ConnectionString"], action =>
            {
                action.CommandTimeout(dbConfig.TimeoutTime);
            })
            .EnableDetailedErrors(dbConfig.DetailedError)
            .EnableSensitiveDataLogging(dbConfig.SensitiveDataLogging)
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
        );

        builder.Services.AddScoped<IDbContext, DatabaseContext>();
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped<IJwtUtils, JwtUtils>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<ICertificateService, CertificateService>();
        builder.Services.AddScoped<IPinataService, PinataService>();
        builder.Services.AddScoped<ICloudinaryService, CloudinaryService>();
    }
}