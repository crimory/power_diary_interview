using Chat.App.Repository.Models;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Chat.App.Repository;

public interface IRepositoryChatStats
{
    Task<DbStat[]> GetEventsEverySecond(uint page);
    Task<DbStat[]> GetEventsEveryMinute(uint page);
    Task<DbStat[]> GetEventsEveryHour(uint page);
    Task<DbStat[]> GetEventsEveryDay(uint page);
    Task<DbStat[]> GetEventsEveryMonth(uint page);
}

public class RepositoryChatStats : IRepositoryChatStats
{
    private const int PageSize = 20;
    private readonly IRepositorySettings _repositorySettings;
    
    private const string MostOfGroupedSqlQuery = @"CASE
                WHEN X.Type = 'Enter' AND COUNT(*) = 1 THEN '1 person entered'
                WHEN X.Type = 'Enter' AND COUNT(*) > 1 THEN COUNT(*) || ' people entered'
                WHEN X.Type = 'Leave' THEN COUNT(*) || ' left'
                WHEN X.Type = 'Comment' THEN COUNT(*) || ' comments'
                WHEN X.Type = 'Five' AND COUNT(DISTINCT X.UserId) = 1 AND COUNT(DISTINCT X.ReceiverId) = 1
                    THEN '1 person high-fived 1 other person'
                WHEN X.Type = 'Five' AND COUNT(DISTINCT X.UserId) = 1 AND COUNT(DISTINCT X.ReceiverId) > 1
                    THEN ' 1 person high-fived ' || COUNT(DISTINCT X.ReceiverId) || ' other people'
                WHEN X.Type = 'Five' AND COUNT(DISTINCT X.UserId) > 1 AND COUNT(DISTINCT X.ReceiverId) = 1
                    THEN COUNT(DISTINCT X.UserId) || ' people high-fived 1 other person'
                WHEN X.Type = 'Five' AND COUNT(DISTINCT X.UserId) > 1 AND COUNT(DISTINCT X.ReceiverId) > 1
                    THEN COUNT(DISTINCT X.UserId) || ' people high-fived ' || COUNT(DISTINCT X.ReceiverId) || ' other people'
                ELSE 'Error occured'
            END AS Description
        FROM
        (
            SELECT 'Enter' AS Type, DateTimeUtc, UserId, '' AS ReceiverId FROM EnterRooms
            UNION 
            SELECT 'Leave' AS Type, DateTimeUtc, UserId, '' AS ReceiverId FROM LeaveRooms
            UNION
            SELECT 'Comment' AS Type, DateTimeUtc, UserId, '' AS ReceiverId FROM Comments
            UNION
            SELECT 'Five' AS Type, DateTimeUtc, AuthorId, ReceiverId FROM HighFives
            ORDER BY DateTimeUtc DESC
        ) X
        GROUP BY TimeFrame, X.Type
        LIMIT @PageSize OFFSET @PageNumber * @PageSize;";

    public RepositoryChatStats(IRepositorySettings repositorySettings)
    {
        _repositorySettings = repositorySettings;
    }

    private async Task<DbStat[]> GetEventsGeneral(string sql, uint page)
    {
        await using var connection = new SqliteConnection(_repositorySettings.DatabaseConnectionString);
        await connection.OpenAsync();

        var queryParams = new {PageSize, PageNumber = page};
        var maybeLatestDbStats = await connection.QueryAsync(sql, queryParams);
        
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

    public async Task<DbStat[]> GetEventsEverySecond(uint page)
    {
        return await GetEventsGeneral(@"SELECT
                    strftime('%Y-%m-%d %H:%M:%S', X.DateTimeUtc) AS TimeFrame,
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
                LEFT JOIN Users Receivers ON X.ReceiverId = Receivers.Id;", page);
    }

    public async Task<DbStat[]> GetEventsEveryMinute(uint page)
    {
        return await GetEventsGeneral(
            "SELECT strftime('%Y-%m-%d %H:%M', X.DateTimeUtc) AS TimeFrame," + MostOfGroupedSqlQuery,
            page);
    }
    
    public async Task<DbStat[]> GetEventsEveryHour(uint page)
    {
        return await GetEventsGeneral(
            "SELECT strftime('%Y-%m-%d %H', X.DateTimeUtc) AS TimeFrame," + MostOfGroupedSqlQuery,
            page);
    }
    
    public async Task<DbStat[]> GetEventsEveryDay(uint page)
    {
        return await GetEventsGeneral(
            "SELECT strftime('%Y-%m-%d', X.DateTimeUtc) AS TimeFrame," + MostOfGroupedSqlQuery,
            page);
    }
    
    public async Task<DbStat[]> GetEventsEveryMonth(uint page)
    {
        return await GetEventsGeneral(
            "SELECT strftime('%Y-%m', X.DateTimeUtc) AS TimeFrame," + MostOfGroupedSqlQuery,
            page);
    }
}