namespace API.Common.Helpers;

public static class Config
{
    public static string GetEnvironmentVariable(string variableName)
    {
        return Environment.GetEnvironmentVariable(variableName) ?? throw new InvalidOperationException($"{variableName} environment variable is not set");
    }
}

public static class ConfigManager
{
    public static IConfiguration LoadDevelopmentConfig()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true);

        return builder.Build();
    }

    public static IConfiguration LoadProductionConfig()
    {
        var builder = new ConfigurationBuilder()
            .AddEnvironmentVariables();

        return builder.Build();
    }
}