using CurrencyConverter.Application.Factories;
using CurrencyConverter.Application.Services;
using CurrencyConverter.Domain.Interfaces;
using CurrencyConverter.Infrastructure;
using CurrencyConverter.Api.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using AspNetCoreRateLimit;
using OpenTelemetry.Trace;
using OpenTelemetry.Resources;
using Serilog;
using CurrencyConverter.Api.LoggingEnrichers;

var builder = WebApplication.CreateBuilder(args);

// Configuration
builder.Configuration.AddJsonFile("appsettings.json", optional: false)
                     .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
                     .AddEnvironmentVariables();

// Serilog
builder.Host.UseSerilog((ctx, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "CurrencyConverter")
    .Enrich.With<ActivityEnricher>()          // custom enricher, see below
    .WriteTo.Console()
    .WriteTo.Seq(ctx.Configuration["Seq:Url"]) // send to Seq for structured logs
);

// Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CurrencyConverter API", Version = "v1" });
});


builder.Services.AddApiVersioning(opts =>
{
    opts.AssumeDefaultVersionWhenUnspecified = true;
    opts.DefaultApiVersion = new ApiVersion(1, 0);
    opts.ReportApiVersions = true;
});

// With the fully qualified namespace to resolve ambiguity:
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
});
// Authentication & Authorization
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Convert", policy =>
        policy.RequireClaim("permission", "Convert"));

    options.AddPolicy("History", policy =>
        policy.RequireClaim("permission", "History"));
});

// Rate limiting
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddMemoryCache();
builder.Services.AddInMemoryRateLimiting();

// OpenTelemetry
builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("CurrencyConverter"))
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddZipkinExporter(o => o.Endpoint = new Uri(builder.Configuration["Zipkin:Endpoint"]));
    });


// Application & Infrastructure
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddSingleton<IExchangeProviderFactory, ExchangeProviderFactory>();
builder.Services.AddScoped<IExchangeService, ExchangeService>();
builder.Services.AddScoped<ICurrencyValidationService, CurrencyValidationService>();
builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

// Middleware pipeline
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseIpRateLimiting();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CurrencyConverter API v1"));
}

app.MapControllers();
app.Run();