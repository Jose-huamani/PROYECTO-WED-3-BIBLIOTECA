using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Proyecto3Api.Configuration;
using Proyecto3Api.Hubs;
using Proyecto3Api.Infrastructure;
using Proyecto3Api.Infrastructure.Auth;
using Proyecto3Api.Infrastructure.Persistence;
using Proyecto3Api.SignalR;
using Serilog;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Proyecto3Api.Services;
using Proyecto3Api.BackgroundServices;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, services, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration).ReadFrom.Services(services).Enrich.FromLogContext().WriteTo.Console());

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.Configure<LibrarySettings>(builder.Configuration.GetSection(LibrarySettings.SectionName));

var jwt = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>() ?? new JwtSettings();
if (string.IsNullOrWhiteSpace(jwt.Key) || jwt.Key.Length < 32)
{
    throw new InvalidOperationException("Configura JwtSettings:Key con al menos 32 caracteres en appsettings.json");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwt.Issuer,
        ValidAudience = jwt.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
        ClockSkew = TimeSpan.FromMinutes(1)
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();
builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
});
builder.Services.AddSignalR();
builder.Services.AddSingleton<Microsoft.AspNetCore.SignalR.IUserIdProvider, UserIdProvider>();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddScoped<IFirebaseNotificationService, FirebaseNotificationService>();
builder.Services.AddHostedService<FinesBackgroundService>();

try
{
    var credentialsPath = Path.Combine(builder.Environment.ContentRootPath, "firebase-adminsdk.json");
    if (File.Exists(credentialsPath))
    {
        FirebaseApp.Create(new AppOptions
        {
            Credential = GoogleCredential.FromFile(credentialsPath)
        });
    }
}
catch (Exception ex)
{
    Console.WriteLine($"[WARNING] No se pudo inicializar Firebase: {ex.Message}");
}
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Biblioteca Inteligente API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Pega el JWT como: Bearer {tu_token}"
    });
    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        { new OpenApiSecuritySchemeReference("Bearer"), [] }
    });
});

var redis = builder.Configuration["Redis:ConnectionString"];
if (!string.IsNullOrWhiteSpace(redis))
{
    builder.Services.AddStackExchangeRedisCache(options => options.Configuration = redis);
}
else
{
    builder.Services.AddDistributedMemoryCache();
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontends", policy =>
    {
        policy.SetIsOriginAllowed(_ => true).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseCors("Frontends");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notificaciones");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var cfg = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    if (cfg.GetValue("Database:UseSqlite", false))
    {
        await db.Database.EnsureCreatedAsync();
        await EnsureSqliteSchemaAsync(db);
    }
    else
    {
        await db.Database.MigrateAsync();
    }

    await DbSeeder.SeedAsync(db, cfg);
}

app.Run();

static async Task EnsureSqliteSchemaAsync(AppDbContext db)
{
    var connection = db.Database.GetDbConnection();
    var shouldClose = connection.State != System.Data.ConnectionState.Open;
    if (shouldClose)
    {
        await connection.OpenAsync();
    }

    try
    {
        await using var checkCommand = connection.CreateCommand();
        
        // --- Libros ---
        checkCommand.CommandText = "PRAGMA table_info('Libros');";
        var hasPrecio = false;
        var hasIntroduccion = false;
        var hasDescripcion = false;
        var hasEditorial = false;
        var hasNumeroPaginas = false;
        var hasIdioma = false;
        var hasEstadoLibro = false;

        await using (var reader = await checkCommand.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                var col = reader["name"]?.ToString();
                if (string.Equals(col, "Precio", StringComparison.OrdinalIgnoreCase)) hasPrecio = true;
                if (string.Equals(col, "Introduccion", StringComparison.OrdinalIgnoreCase)) hasIntroduccion = true;
                if (string.Equals(col, "Descripcion", StringComparison.OrdinalIgnoreCase)) hasDescripcion = true;
                if (string.Equals(col, "Editorial", StringComparison.OrdinalIgnoreCase)) hasEditorial = true;
                if (string.Equals(col, "NumeroPaginas", StringComparison.OrdinalIgnoreCase)) hasNumeroPaginas = true;
                if (string.Equals(col, "Idioma", StringComparison.OrdinalIgnoreCase)) hasIdioma = true;
                if (string.Equals(col, "EstadoLibro", StringComparison.OrdinalIgnoreCase)) hasEstadoLibro = true;
            }
        }

        if (!hasPrecio) await ExecuteNonQueryAsync(connection, "ALTER TABLE Libros ADD COLUMN Precio decimal(18,2) NOT NULL DEFAULT 0;");
        if (!hasIntroduccion) await ExecuteNonQueryAsync(connection, "ALTER TABLE Libros ADD COLUMN Introduccion TEXT;");
        if (!hasDescripcion) await ExecuteNonQueryAsync(connection, "ALTER TABLE Libros ADD COLUMN Descripcion TEXT;");
        if (!hasEditorial) await ExecuteNonQueryAsync(connection, "ALTER TABLE Libros ADD COLUMN Editorial TEXT;");
        if (!hasNumeroPaginas) await ExecuteNonQueryAsync(connection, "ALTER TABLE Libros ADD COLUMN NumeroPaginas INTEGER NOT NULL DEFAULT 0;");
        if (!hasIdioma) await ExecuteNonQueryAsync(connection, "ALTER TABLE Libros ADD COLUMN Idioma TEXT;");
        if (!hasEstadoLibro) await ExecuteNonQueryAsync(connection, "ALTER TABLE Libros ADD COLUMN EstadoLibro TEXT;");

        // --- Usuarios ---
        checkCommand.CommandText = "PRAGMA table_info('Usuarios');";
        var hasTelefono = false;
        var hasCarrera = false;
        var hasDireccion = false;

        await using (var reader = await checkCommand.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                var col = reader["name"]?.ToString();
                if (string.Equals(col, "Telefono", StringComparison.OrdinalIgnoreCase)) hasTelefono = true;
                if (string.Equals(col, "Carrera", StringComparison.OrdinalIgnoreCase)) hasCarrera = true;
                if (string.Equals(col, "Direccion", StringComparison.OrdinalIgnoreCase)) hasDireccion = true;
            }
        }

        if (!hasTelefono) await ExecuteNonQueryAsync(connection, "ALTER TABLE Usuarios ADD COLUMN Telefono TEXT;");
        if (!hasCarrera) await ExecuteNonQueryAsync(connection, "ALTER TABLE Usuarios ADD COLUMN Carrera TEXT;");
        if (!hasDireccion) await ExecuteNonQueryAsync(connection, "ALTER TABLE Usuarios ADD COLUMN Direccion TEXT;");
    }
    finally
    {
        if (shouldClose)
        {
            await connection.CloseAsync();
        }
    }
}

static async Task ExecuteNonQueryAsync(System.Data.Common.DbConnection conn, string sql)
{
    await using var cmd = conn.CreateCommand();
    cmd.CommandText = sql;
    await cmd.ExecuteNonQueryAsync();
}
