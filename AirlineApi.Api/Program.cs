using System.Text;
using System.Threading.RateLimiting;

using AirlineApi.Api.Security;
using AirlineApi.Application.Interfaces;
using AirlineApi.Application.Services;
using AirlineApi.Infrastructure.Database;
using AirlineApi.Infrastructure.Repositories;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddScoped<IPassengerService, PassengerService>();
builder.Services.AddScoped<IPassengerRepository, PassengerRepository>();
builder.Services.AddScoped<IDbConnectionFactory, DbConnectionFactory>();
builder.Services.AddSingleton<ITokenService, JwtTokenService>();


builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Airline API",
        Version = "v1",
        Description = "Secure Airline Passenger API"
    });
    

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token."
    });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] =
                new List<string>()
        });
});


var jwtKey = builder.Configuration["Jwt:Key"];

if (string.IsNullOrWhiteSpace(jwtKey) ||
    Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException(
        "Jwt:Key is missing or too short (min 32 bytes). " +
        "Set it via user-secrets or the Jwt__Key environment variable.");
}

var jwtIssuer =
    builder.Configuration["Jwt:Issuer"] ?? "AirlineApi";

var jwtAudience =
    builder.Configuration["Jwt:Audience"] ?? "AirlineApiClients";

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,

                ValidateAudience = true,
                ValidAudience = jwtAudience,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey)),

                ValidateLifetime = true,

                ClockSkew = TimeSpan.FromMinutes(1)
            };
    });

builder.Services.AddAuthorization();

static string Ip(HttpContext context) =>
    context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;

    options.AddPolicy(
        "auth",
        context =>
            RateLimitPartition.GetFixedWindowLimiter(
                Ip(context),
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 10,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                }));

    options.AddPolicy(
        "lookup",
        context =>
            RateLimitPartition.GetFixedWindowLimiter(
                Ip(context),
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 20,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                }));

    options.GlobalLimiter =
        PartitionedRateLimiter.Create<HttpContext, string>(
            context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    Ip(context),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));
});



builder.Services.Configure<ForwardedHeadersOptions>(
    options =>
    {
        options.ForwardedHeaders =
            ForwardedHeaders.XForwardedFor;
    });


var origins =
    builder.Configuration
        .GetSection("Cors:Origins")
        .Get<string[]>()
        ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "DefaultPolicy",
        policy =>
            policy
                .WithOrigins(origins)
                .WithHeaders(
                    "Content-Type",
                    "Authorization")
                .WithMethods(
                    "GET",
                    "POST",
                    "PUT"));
});


var app = builder.Build();

app.UseForwardedHeaders();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "Airline API v1");

        options.RoutePrefix = "swagger";
    });
}
else
{
    app.UseHsts();
}


app.UseHttpsRedirection();


app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] =
        "nosniff";

    context.Response.Headers["X-Frame-Options"] =
        "DENY";

    context.Response.Headers["Referrer-Policy"] =
        "no-referrer";

    await next();
});


app.UseCors("DefaultPolicy");

app.UseRateLimiter();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();