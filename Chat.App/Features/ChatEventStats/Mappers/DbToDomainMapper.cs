using Chat.App.Repository.Models;
using Chat.Domain;

namespace Chat.App.Features.ChatEventStats.Mappers;

public interface IDbToDomainMapper
{
    ChatStat[] Map(DbStat[] input);
}

public class DbToDomainMapper : IDbToDomainMapper
{
    public ChatStat[] Map(DbStat[] input)
    {
        return input.Select(Map).ToArray();
    }

    private static ChatStat Map(DbStat input)
    {
        return new ChatStat(
            input.TimeFrame,
            input.Description);
    }
}