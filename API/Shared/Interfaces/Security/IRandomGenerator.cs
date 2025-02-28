namespace API.Shared.Interfaces.Security;

/// <summary>
/// Interface for generating random strings.
/// </summary>
public interface IRandomGenerator
{
    /// <summary>
    /// Generates a random numeric code.
    /// </summary>
    /// <param name="length">The length of the code.</param>
    /// <returns>The numeric code.</returns>
    string GenerateNumberCode(int length = 8);

    /// <summary>
    /// Generates a random alphanumeric code.
    /// </summary>
    /// <param name="length">The length of the code.</param>
    /// <param name="includeSymbols">Whether to include symbols in the code.</param>
    /// <returns>The alphanumeric code.</returns>
    string GenerateAlphanumericCode(int length = 8, bool includeSymbols = false);

    /// <summary>
    /// Generates a random salt.
    /// </summary>
    /// <param name="saltSize">The size of the salt in bytes.</param>
    /// <returns>The salt as a base64 string.</returns>
    string GenerateSalt(int saltSize = 32);
}

