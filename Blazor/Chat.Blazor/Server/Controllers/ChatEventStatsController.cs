using Chat.App.Features.ChatEventStats;
using Chat.Blazor.Shared;
using Chat.Domain;
using Microsoft.AspNetCore.Mvc;

namespace Chat.Blazor.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class ChatEventStatsController : ControllerBase
{
    private readonly IChatEventStats _chatEventStats;

    public ChatEventStatsController(IChatEventStats chatEventStats)
    {
        _chatEventStats = chatEventStats;
    }

    [HttpGet("seconds/{page}")]
    public async Task<ChatStatFrontend[]> GetStatsEverySecondAsync(uint page)
    {
        var chatStatsDesc = await _chatEventStats.GetLatestStatsDesc(ChatEventGranularity.EverySecond, page);
        return MapDomainAndReverse(chatStatsDesc);
    }

    private static ChatStatFrontend[] MapDomainAndReverse(ChatStat[] chatStatsDesc)
    {
        return chatStatsDesc
            .Reverse()
            .Select(x => new ChatStatFrontend
            {
                TimeFrame = x.TimeFrame,
                Description = x.Description
            })
            .ToArray();
    }
}