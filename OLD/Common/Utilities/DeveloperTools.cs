namespace API.Common.Utilities;

public interface IDeveloperTools
{
    void LogDebugInfo(string message);
}

public class DeveloperTools : IDeveloperTools
{
    public void LogDebugInfo(string message)
    {
        Console.WriteLine($"[DEBUG] {message}");
    }
} 