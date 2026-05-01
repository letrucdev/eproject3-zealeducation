using System.Text.Json.Serialization;
using Coravel;
using Coravel.Queuing.Interfaces;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.OpenApi.Models;
using QuestPDF.Infrastructure;
using ZealEducation.API.Middleware;
using ZealEducation.Application;
using ZealEducation.Infrastructure;

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
    })
    .AddRazorRuntimeCompilation();

builder.Services.AddMailer(builder.Configuration);
builder.Services.AddQueue();

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
            .WithExposedHeaders("Content-Disposition");
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

//await app.Services.InitialiseDatabaseAsync();

// Configure the HTTP request pipeline.
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
