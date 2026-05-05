using System.Text;
using Amazon.Runtime;
using Amazon.S3;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ZealEducation.Application.Common.Interfaces;
using ZealEducation.Domain.Interfaces;
using ZealEducation.Infrastructure.Data;
using ZealEducation.Infrastructure.Data.Interceptors;
using ZealEducation.Infrastructure.Excel;
using ZealEducation.Infrastructure.Pdf;
using ZealEducation.Infrastructure.Repositories;
using ZealEducation.Infrastructure.Services;
using ZealEducation.Infrastructure.Services.Email;
using ZealEducation.Infrastructure.Services.Scheduling;
using ZealEducation.Infrastructure.Storage;

namespace ZealEducation.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, AuditLogInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
        });

        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));

        services.AddScoped<ApplicationDbContextInitialiser>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUserService>();
        services.AddScoped<IAuditLogService, AuditLogService>();
        services.AddSingleton<IReceiptPdfGenerator, QuestPdfReceiptGenerator>();
        services.AddSingleton<ICertificatePdfGenerator, QuestPdfCertificateGenerator>();
        services.AddSingleton<IFinancialReportExcelGenerator, ClosedXmlFinancialReportExcelGenerator>();

        services.AddR2StorageServices(configuration);
        services.AddAuthenticationServices(configuration);

        services.AddScoped<IEnquiryConvertedNotificationService, EnquiryConvertedNotificationService>();
        services.AddScoped<ICourseAddedNotificationService, CourseAddedNotificationService>();
        services.AddScoped<ICandidatePasswordResetNotificationService, CandidatePasswordResetNotificationService>();
        services.AddScoped<IInstallmentReminderNotificationService, InstallmentReminderNotificationService>();
        services.AddScoped<IBatchFacultyAssignedNotificationService, BatchFacultyAssignedNotificationService>();
        services.AddScoped<IBatchCandidateEnrolledNotificationService, BatchCandidateEnrolledNotificationService>();
        services.AddScoped<IExaminationCreatedNotificationService, ExaminationCreatedNotificationService>();

        services.AddScoped<SendInstallmentRemindersInvocable>();

        return services;
    }

    private static IServiceCollection AddR2StorageServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<R2Options>(configuration.GetSection(R2Options.SectionName));

        services.AddSingleton<IAmazonS3>(sp =>
        {
            var r2 = sp.GetRequiredService<IOptions<R2Options>>().Value;

            var config = new AmazonS3Config
            {
                ServiceURL = $"https://{r2.AccountId}.r2.cloudflarestorage.com",
                ForcePathStyle = true,
                AuthenticationRegion = "auto"
            };

            var credentials = new BasicAWSCredentials(r2.AccessKeyId, r2.SecretAccessKey);
            return new AmazonS3Client(credentials, config);
        });

        services.AddScoped<IFileStorageService, CloudflareR2StorageService>();

        return services;
    }

    private static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
            ?? throw new InvalidOperationException($"Missing '{JwtSettings.SectionName}' configuration section.");

        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                ClockSkew = TimeSpan.FromSeconds(30)
            };
        });

        services.AddAuthorization();

        return services;
    }
}
