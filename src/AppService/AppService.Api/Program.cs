using System.Reflection;
using AppService.Api.Middleware;
using AppService.Application.Interfaces;
using AppService.Domain.Repositories;
using AppService.Infrastructure.Repositories;
using AppService.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Add controllers
builder.Services.AddControllers().AddJsonOptions(option => { });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "App Service API",
        Version = "v1"
    });
    options.AddSecurityDefinition("ApiKey", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Name = "X-API-KEY",
        Scheme = "X-API-KEY",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Description = "API Key needed to access the endpoints."
    });
    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            []
        }
    });
});

// Register MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(Assembly.Load("AppService.Application")));

// Register repositories
builder.Services.AddSingleton<IProductRepository, InMemoryProductRepository>();

// Register services
builder.Services.AddSingleton<ISignatureService, HmacSignatureService>();
builder.Services.AddHttpClient<IStorageServiceClient, StorageServiceClient>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Middleware
app.UseMiddleware<ApiKeyAuthenticationMiddleware>();

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "app-service" }))
    .WithName("HealthCheck")
    .WithOpenApi();

app.Run();