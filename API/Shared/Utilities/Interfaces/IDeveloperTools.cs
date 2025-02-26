namespace API.Shared.Utilities.Interfaces;

/// <summary>
/// Interface for developer tools that provide logging functionality.
/// </summary>
public interface IDeveloperTools
{
    /// <summary>
    /// Logs debug information to the console.
    /// </summary>
    void LogDebugInfo(string message);
}