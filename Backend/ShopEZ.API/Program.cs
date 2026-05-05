using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ShopEZ.API.Data;
using ShopEZ.API.Repositories;
using ShopEZ.API.Repositories.Interfaces;
using ShopEZ.API.Services;
using ShopEZ.API.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

//
// 1.  CONTROLLERS + JSON OPTIONS
//
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// 
// 2.  ENTITY FRAMEWORK CORE — SQL SERVER
// 
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptions =>
        {
            sqlServerOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorNumbersToAdd: null);
        }
    )
);

// 
// 3.  JWT AUTHENTICATION
// 
string jwtSecretKey = builder.Configuration["JwtSettings:SecretKey"]
    ?? throw new InvalidOperationException("JwtSettings:SecretKey is not configured.");

string jwtIssuer = builder.Configuration["JwtSettings:Issuer"]
    ?? throw new InvalidOperationException("JwtSettings:Issuer is not configured.");

string jwtAudience = builder.Configuration["JwtSettings:Audience"]
    ?? throw new InvalidOperationException("JwtSettings:Audience is not configured.");

builder.Services
    .AddAuthentication(options =>
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
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(
                                           Encoding.UTF8.GetBytes(jwtSecretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// 
// 4.  DEPENDENCY INJECTION — REPOSITORIES
//
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();

// 
// 5.  DEPENDENCY INJECTION — SERVICES
// 
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// 
// 6.  CORS
// 
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

// 
// 7.  SWAGGER + OPENAPI  (Swashbuckle 10.x / .NET 10 compatible)
// 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();

// 
// BUILD
// 
var app = builder.Build();

// 
// 8.  AUTO-APPLY MIGRATIONS ON STARTUP
// 
using (IServiceScope scope = app.Services.CreateScope())
{
    ApplicationDbContext db = scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();
    db.Database.Migrate();
}

// 
// 9.  MIDDLEWARE PIPELINE
//     UseAuthentication MUST come before UseAuthorization
// 
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();