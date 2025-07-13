using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using DotNetEnv;


Env.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddOpenApi();

string connectionString;

var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrEmpty(databaseUrl))
{
    // prod
    connectionString = databaseUrl;
}
else
{
    var host = Environment.GetEnvironmentVariable("DB_HOST");
    var database = Environment.GetEnvironmentVariable("DB_NAME");
    var username = Environment.GetEnvironmentVariable("DB_USER");
    var password = Environment.GetEnvironmentVariable("DB_PASSWORD");

    connectionString = $"Host={host};Database={database};Username={username};Password={password}";

}

builder.Services.AddDbContext<CodeVaultContext>(options =>
    options.UseNpgsql(connectionString));

// JWT config
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET");
var jwtExpirationDays = builder.Configuration.GetValue<int>("JwtSettings:ExpirationDays");

if (string.IsNullOrEmpty(jwtSecret) || jwtSecret.Length < 32)
{
    throw new InvalidOperationException("JWT Secret must be at least 32 characters long");
}

var key = Encoding.ASCII.GetBytes(jwtSecret);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false; // For development only
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddAuthorization();


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<CodeVaultContext>();
    context.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.Run();


// Snippet models
public class Snippet
{
    // Core Identity
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";

    // Code Content
    public string Code { get; set; } = "";
    public string Language { get; set; } = ""; // "csharp", "javascript", "python"
    public string Framework { get; set; } = ""; // "react", "dotnet", "django"

    // Metadata
    public List<string> Tags { get; set; } = new(); // ["api", "authentication", "jwt"]
    public bool IsPublic { get; set; } = false; // Private by default
    public bool IsFavorite { get; set; } = false; // User can star snippets

    // Usage & Analytics
    public int ViewCount { get; set; } = 0;
    public int CopyCount { get; set; } = 0; // Track how often it's copied
    public DateTime LastAccessedAt { get; set; } = DateTime.UtcNow;

    // Versioning & History
    public int Version { get; set; } = 1;
    public string? PreviousVersionId { get; set; } // Link to previous version

    // Organization
    public string? FolderPath { get; set; } = null; // "/work/apis/authentication"
    public string? SourceUrl { get; set; } = null; // Link to GitHub, docs, etc.

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Relationships
    public int UserId { get; set; }
    public User User { get; set; } = null!;
}

public class CreateSnippetRequest
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Code { get; set; } = "";
    public string Language { get; set; } = "";
    public string Framework { get; set; } = "";
    public List<string> Tags { get; set; } = new();
    public bool IsPublic { get; set; } = false;
    public string? FolderPath { get; set; } = null;
    public string? SourceUrl { get; set; } = null;
}

public class UpdateSnippetRequest
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Code { get; set; } = "";
    public string Language { get; set; } = "";
    public string Framework { get; set; } = "";
    public List<string> Tags { get; set; } = new();
    public bool IsPublic { get; set; } = false;
    public bool IsFavorite { get; set; } = false;
    public string? FolderPath { get; set; } = null;
    public string? SourceUrl { get; set; } = null;
}


// User models
public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string DisplayName { get; set; } = ""; // For showing in UI
    public string? Bio { get; set; } = null; // Optional profile bio
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastLoginAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public ICollection<Snippet> Snippets { get; set; } = new List<Snippet>();
}

public class RegisterRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
    public string DisplayName { get; set; } = "";
}

public class LoginRequest
{
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}


// Db Context
public class CodeVaultContext : DbContext
{
    public CodeVaultContext(DbContextOptions<CodeVaultContext> options) : base(options) { }
    
    public DbSet<User> Users { get; set; }
    public DbSet<Snippet> Snippets { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
            entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.HasIndex(e => e.Username).IsUnique();
        });

        // Snippet configuration
        modelBuilder.Entity<Snippet>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Code).HasMaxLength(50000); // Optional, but with max length
            entity.Property(e => e.Language).HasMaxLength(50); // Optional
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");
            
            // Convert Tags list to JSON string for database storage
            entity.Property(e => e.Tags)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                );
                
            // Configure relationship
            entity.HasOne(e => e.User)
                  .WithMany(e => e.Snippets)
                  .HasForeignKey(e => e.UserId);
        });
    }
}