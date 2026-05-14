using System.Text.Json.Serialization;
using Coravel;
using Coravel.Queuing.Interfaces;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.OpenApi.Models;
using QuestPDF.Infrastructure;
using ZealEducation.API.Common.Json;
using ZealEducation.API.Middleware;
using ZealEducation.Application;
using ZealEducation.Infrastructure;
using ZealEducation.Infrastructure.Services.Scheduling;

QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// Allow uploads up to ~60 MB (50 MB for files + multipart overhead).
const long MaxRequestBytes = 60L * 1024 * 1024;
builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = MaxRequestBytes);
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = MaxRequestBytes;
    options.ValueLengthLimit = int.MaxValue;
});

// Add services to the container.
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.Converters.Add(new UtcDateTimeConverter());
        options.JsonSerializerOptions.Converters.Add(new NullableUtcDateTimeConverter());
    })
    .AddRazorRuntimeCompilation();

builder.Services.AddMailer(builder.Configuration);
builder.Services.AddQueue();
builder.Services.AddScheduler();

const string CorsPolicyName = "AllowFrontend";
builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? ["http://localhost:4200"];

        policy.WithOrigins("*")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithExposedHeaders(
                "Content-Disposition",
                "X-Project",
                "X-Author",
                "X-Email",
                "X-Created",
                "X-Course",
                "X-License");
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT token (without 'Bearer ' prefix)."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

app.Services.ConfigureQueue()
    .LogQueuedTaskProgress(app.Services.GetRequiredService<ILogger<IQueue>>());

app.Services.UseScheduler(scheduler =>
{
    scheduler
        .Schedule<SendInstallmentRemindersInvocable>()
        .EveryMinute()
        .PreventOverlapping(nameof(SendInstallmentRemindersInvocable));
    /* .DailyAt(8, 0) */
})
.OnError(ex => app.Services
    .GetRequiredService<ILogger<Program>>()
    .LogError(ex, "Scheduler task failed"));

//await app.Services.InitialiseDatabaseAsync();

// Configure the HTTP request pipeline.
app.UseMiddleware<RequestHeaderLoggingMiddleware>();
app.UseMiddleware<ProjectInfoHeadersMiddleware>();
app.UseMiddleware<GlobalExceptionHandler>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(CorsPolicyName);
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseMiddleware<PasswordResetTokenGuardMiddleware>();
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { }
