using API.Shared.Utilities.Interfaces;

namespace API.Shared.Utilities;

/// <summary>
/// Implementation of the IDeveloperTools interface for logging debug information.
/// </summary>
public class DeveloperTools : IDeveloperTools
{
    public void LogDebugInfo(string message)
    {
        Console.WriteLine($"[DEBUG] {message}");
    }
} 