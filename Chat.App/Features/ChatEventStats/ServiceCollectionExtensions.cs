using Chat.App.Features.ChatEventStats.Mappers;
using Microsoft.Extensions.DependencyInjection;

namespace Chat.App.Features.ChatEventStats;

public static class ServiceCollectionExtensions
{
    public static void AddChatEventStats(this IServiceCollection services)
    {
        services.AddSingleton<IDbToDomainMapper, DbToDomainMapper>();
        services.AddScoped<IChatEventStats, ChatEventStats>();
    }
}