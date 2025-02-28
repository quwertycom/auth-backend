namespace API.Shared.Interfaces.Configuration;

/// <summary>
/// Provides access to environment variables in a way that can be mocked for testing
/// </summary>
public interface IEnvironmentVariableProvider
{
    /// <summary>
    /// Gets an environment variable by name
    /// </summary>
    /// <param name="name">The name of the environment variable</param>
    /// <returns>The value of the environment variable, or null if it doesn't exist</returns>
    string? GetVariable(string name);
    
    /// <summary>
    /// Gets an environment variable by name with a default value
    /// </summary>
    /// <param name="name">The name of the environment variable</param>
    /// <param name="defaultValue">The default value to return if the variable doesn't exist</param>
    /// <returns>The value of the environment variable, or the default value if it doesn't exist</returns>
    string GetVariable(string name, string defaultValue);
} 