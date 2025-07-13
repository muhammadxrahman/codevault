// Snippet models
public class Snippet
{
    // Core Identity
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";

    // Code Content
    public string Code { get; set; } = "";
    public string Language { get; set; } = ""; // "csharp", "javascript", "python", etc.
    public string Framework { get; set; } = ""; // "react", "dotnet", "django", etc. (optional)

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