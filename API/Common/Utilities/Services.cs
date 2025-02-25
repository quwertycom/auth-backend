// This file is no longer needed and should be deleted.
// Extension methods have been moved to dedicated files in the API.Extensions namespace.
// See:
// - API/Extensions/ServiceCollectionExtensions.cs
// - API/Extensions/DatabaseExtensions.cs
// - API/Extensions/BusinessServiceExtensions.cs
// - API/Extensions/RepositoryExtensions.cs
// - API/Extensions/UtilityExtensions.cs
//
// In Program.cs, instead of using Services.Initialize(), now use:
// builder.Services.AddApplicationServices(builder.Configuration);
//
// This file is left as a placeholder and will be removed in a future update.