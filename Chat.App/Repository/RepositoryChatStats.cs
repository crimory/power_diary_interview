using Chat.App.Repository.Models;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Chat.App.Repository;

public interface IRepositoryChatStats
{
    Task<DbStat[]> GetEventsEverySecond(uint page);
}

public class RepositoryChatStats : IRepositoryChatStats
{
    private const int PageSize = 50;
    private readonly IRepositorySettings _repositorySettings;

    public RepositoryChatStats(IRepositorySettings repositorySettings)
    {
        _repositorySettings = repositorySettings;
    }

    public async Task<DbStat[]> GetEventsEverySecond(uint page)
    {
        await using var connection = new SqliteConnection(_repositorySettings.DatabaseConnectionString);
        await connection.OpenAsync();

        var queryParams = new {PageSize, PageNumber = page};
        var maybeLatestDbStats = await connection.QueryAsync(
            @"SELECT
                    X.DateTimeUtc AS TimeFrame,
                    CASE
                        WHEN X.Type = 'Enter' THEN Authors.Name || ' enters the room'
                        WHEN X.Type = 'Leave' THEN Authors.Name || ' leaves'
                        WHEN X.Type = 'Comment' THEN Authors.Name || ' comments: ' || X.Message
                        WHEN X.Type = 'Five' THEN Authors.Name || ' high-fives ' || Receivers.Name
                        ELSE 'Error occured'
                    END AS Description
                FROM
                (
                    SELECT 'Enter' AS Type, DateTimeUtc, UserId, '' AS Message, '' AS ReceiverId FROM EnterRooms
                    UNION 
                    SELECT 'Leave' AS Type, DateTimeUtc, UserId, '' AS Message, '' AS ReceiverId FROM LeaveRooms
                    UNION
                    SELECT 'Comment' AS Type, DateTimeUtc, UserId, Message, '' AS ReceiverId FROM Comments
                    UNION
                    SELECT 'Five' AS Type, DateTimeUtc, AuthorId, '' AS Message, ReceiverId FROM HighFives
                    ORDER BY DateTimeUtc DESC
                    LIMIT @PageSize OFFSET @PageNumber * @PageSize
                ) X
                INNER JOIN Users Authors ON X.UserId = Authors.Id
                LEFT JOIN Users Receivers ON X.ReceiverId = Receivers.Id;",
            queryParams);
        
        if (maybeLatestDbStats == null)
            return Array.Empty<DbStat>();
        
        var latestDbStats = maybeLatestDbStats as dynamic[]
                                 ?? maybeLatestDbStats.ToArray();
        if (!latestDbStats.Any())
            return Array.Empty<DbStat>();

        return latestDbStats
            .Select(x => new DbStat
            {
                TimeFrame = x.TimeFrame,
                Description = x.Description
            }).ToArray();
    }
}