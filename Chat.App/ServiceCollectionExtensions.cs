using Chat.App.Features.ChatEventStats;
using Chat.App.Repository;
using Microsoft.Extensions.DependencyInjection;

namespace Chat.App;

public static class ServiceCollectionExtensions
{
    public static void AddChatApp(this IServiceCollection services)
    {
        services.AddRepository();
        services.AddChatEventStats();
    }
}