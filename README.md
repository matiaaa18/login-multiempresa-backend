# Login Multiempresa — Backend

API REST con C# .NET 10.0 para sistema de login multi-tenant con detección por subdominio.

## Stack
- **C# .NET 10.0** (net10.0)
- **Entity Framework Core** + PostgreSQL (Npgsql)
- **JWT** (access token 5 min) + **Refresh Tokens** (7 días)
- **BCrypt.Net-Next** para hashing de contraseñas
- **Swagger** para documentación de API
- **Rate Limiting** para protección contra fuerza bruta

## Requisitos
- .NET 10.0 SDK
- PostgreSQL con base de datos `login_multiempresa`

## Configuración
```bash
# Base de datos (ajustar en appsettings.json)
Host=localhost;Port=5432;Database=login_multiempresa;Username=postgres;Password=postgres

# Variable de entorno para JWT (opcional, tiene fallback a appsettings.json)
export JWT_SECRET="MiClaveSecretaSuperSeguraParaJWT2025LoginMultiempresa!!"
```

## Ejecutar
```bash
dotnet restore
dotnet run --urls "http://0.0.0.0:5050"
```

## Endpoints
| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/auth/empresa/{subdominio}` | Detectar empresa por subdominio |
| POST | `/api/auth/login` | Iniciar sesión |
| POST | `/api/auth/refresh` | Renovar access token |
| GET | `/api/auth/me` | Info del usuario logueado (protegido) |
| POST | `/api/auth/logout` | Cerrar sesión (revoca refresh token) |

## Swagger
Disponible en `http://localhost:5050/swagger` (solo en desarrollo).

## Estructura
```
├── Controllers/
│   └── AuthController.cs       # Endpoints de autenticación
├── Data/
│   ├── AppDbContext.cs          # DbContext (EF Core)
│   └── Configurations/         # IEntityTypeConfiguration<T>
│       ├── EmpresaConfiguration.cs
│       ├── UsuarioConfiguration.cs
│       └── RefreshTokenConfiguration.cs
├── DTOs/                        # Data Transfer Objects
│   ├── LoginRequest.cs          # Con Data Annotations
│   ├── LoginResponse.cs
│   ├── RefreshRequest.cs
│   ├── RefreshResponse.cs
│   ├── MeResponse.cs
│   ├── ErrorResponse.cs
│   ├── LogoutResponse.cs
│   └── EmpresaResponse.cs
├── Models/                      # Entidades (espejo de PostgreSQL)
│   ├── Empresa.cs
│   ├── Usuario.cs
│   └── RefreshToken.cs
├── Services/
│   └── TokenService.cs          # Generación JWT + refresh tokens
└── Program.cs                   # Configuración de la app
```

## Autor
Matías Gómez — 2026
