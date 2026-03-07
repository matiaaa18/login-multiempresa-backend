// ============================================
// PROGRAM.CS: El archivo principal que conecta TODO
// ============================================

using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Backend.Data;
using Backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// --- Conectar a PostgreSQL ---
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- Registrar servicios ---
builder.Services.AddScoped<TokenService>();
builder.Services.AddControllers();

// --- Swagger: documentación de la API ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// --- Configurar JWT ---
// FIX: Leer secreto de variable de entorno, con fallback a appsettings
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET")
    ?? builder.Configuration["Jwt:Secret"]!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSecret)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// --- Rate Limiting: protección contra fuerza bruta ---
builder.Services.AddRateLimiter(options =>
{
    // Limitar el endpoint de login: máximo 10 intentos por minuto por IP
    options.AddFixedWindowLimiter("login", limiter =>
    {
        limiter.PermitLimit = 10;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiter.QueueLimit = 0;
    });

    options.RejectionStatusCode = 429; // Too Many Requests
});

// --- CORS ---
// FIX: Restringir en producción, permisivo solo en desarrollo
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        if (builder.Environment.IsDevelopment())
        {
            // En desarrollo: permitir todo (para que Angular proxy funcione)
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        }
        else
        {
            // En producción: solo dominios específicos
            policy.WithOrigins(
                    "https://traza.miapp.cl",
                    "https://acme.miapp.cl",
                    "https://soltech.miapp.cl")
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        }
    });
});

var app = builder.Build();

// Swagger solo en desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
