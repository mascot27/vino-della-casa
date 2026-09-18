using System.Reflection;
using VinoDellaCasa.Mcp.Handlers;

namespace VinoDellaCasa.Mcp.Tests;

public class SecurityConstraintsTests
{
    [Fact]
    public void McpAssembly_HasNoHttpClientUsageInHandlers()
    {
        var asm = typeof(ToolHandlers).Assembly;
        var sourceTypes = asm.GetTypes().Where(t => t.Namespace?.StartsWith("VinoDellaCasa.Mcp") == true);
        foreach (var type in sourceTypes)
        {
            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                Assert.False(typeof(HttpClient).IsAssignableFrom(field.FieldType),
                    $"{type.Name}.{field.Name} must not hold HttpClient (stdio-only, no network).");
            }
        }
    }

    [Fact]
    public void Program_UsesStdioTransportOnly()
    {
        var programPath = Path.GetFullPath(Path.Combine(
            AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "VinoDellaCasa.Mcp", "Program.cs"));
        Assert.True(File.Exists(programPath), $"Program.cs not found at {programPath}");
        var text = File.ReadAllText(programPath);
        Assert.Contains("WithStdioServerTransport", text);
        Assert.DoesNotContain("WithHttpTransport", text);
        Assert.DoesNotContain("MapMcp", text);
        Assert.DoesNotContain("AspNetCore", text);
    }
}
