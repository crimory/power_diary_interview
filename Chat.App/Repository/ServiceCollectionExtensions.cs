using Microsoft.Extensions.DependencyInjection;

namespace Chat.App.Repository;

public static class ServiceCollectionExtensions
{
    public static void AddRepository(this IServiceCollection services)
    {
        services.AddSingleton<IRepositoryInitializer, RepositoryInitializer>();
        services.AddScoped<IRepositoryUser, RepositoryUser>();
        services.AddScoped<IRepositoryChatEvent, RepositoryChatEvent>();
        services.AddScoped<IRepositoryChatStats, RepositoryChatStats>();
    }
}