using Chat.App.Features.ChatEventStats.Mappers;
using Chat.App.Repository;
using Chat.Domain;

namespace Chat.App.Features.ChatEventStats;

public interface IChatEventStats
{
    Task<ChatStat[]> GetLatestStatsDesc(ChatEventGranularity granularity, uint page = 0);
}

public enum ChatEventGranularity
{
    EverySecond,
    EveryMinute,
    EveryHour,
    EveryDay,
    EveryMonth
}

public class ChatEventStats : IChatEventStats
{
    private readonly IRepositoryChatStats _repositoryStats;
    private readonly IDbToDomainMapper _mapper;

    public ChatEventStats(
        IRepositoryChatStats repositoryStats,
        IDbToDomainMapper mapper)
    {
        _repositoryStats = repositoryStats;
        _mapper = mapper;
    }

    public async Task<ChatStat[]> GetLatestStatsDesc(ChatEventGranularity granularity, uint page = 0)
    {
        var dbStats = granularity switch
        {
            ChatEventGranularity.EverySecond => await _repositoryStats.GetEventsEverySecond(page),
            ChatEventGranularity.EveryMinute => await _repositoryStats.GetEventsEveryMinute(page),
            ChatEventGranularity.EveryHour => await _repositoryStats.GetEventsEveryHour(page),
            ChatEventGranularity.EveryDay => await _repositoryStats.GetEventsEveryDay(page),
            ChatEventGranularity.EveryMonth => await _repositoryStats.GetEventsEveryMonth(page),
            _ => throw new ArgumentOutOfRangeException(nameof(granularity), granularity, null)
        };
        return _mapper.Map(dbStats);
    }
}