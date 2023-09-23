using Chat.App.Repository.Models;
using Chat.Domain;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Chat.App.Repository;

public interface IRepositorySql
{
    Task<bool> AddUsersAsync(User[] users);
    Task<bool> AddEnterRoomsAsync(EnterRoom[] comments);
    Task<bool> AddLeaveRoomsAsync(LeaveRoom[] comments);
    Task<bool> AddCommentsAsync(Comment[] comments);
    Task<bool> AddHighFivesAsync(HighFive[] comments);
}

public class RepositorySql : IRepositorySql
{
    private const int BatchSize = 1000;
    private readonly IRepositorySettings _repositorySettings;

    public RepositorySql(IRepositorySettings repositorySettings)
    {
        _repositorySettings = repositorySettings;
    }

    private async Task<bool> AddObjectsGeneric<TInput, TDbInput>(
        TInput[] input, string sql, Func<TInput, TDbInput> mapInput)
    {
        if (!input.Any())
            return false;

        await using var connection = new SqliteConnection(_repositorySettings.DatabaseConnectionString);
        await connection.OpenAsync();

        var dbInput = input.Select(mapInput).ToArray();

        var totalProcessed = 0;
        for (var i = 0; i < dbInput.Length; i += BatchSize)
        {
            var batch = dbInput.Skip(i).Take(BatchSize).ToList();
            var newEnterRoomsCount = await connection.ExecuteAsync(sql, batch);
            totalProcessed += newEnterRoomsCount;
        }

        return totalProcessed == input.Length;
    }

    public async Task<bool> AddUsersAsync(User[] users)
    {
        return await AddObjectsGeneric(
            users,
            "INSERT INTO Users (Id, Name)VALUES (@Id, @Name)",
            user => new DbUser
            {
                Id = user.Id.ToString(),
                Name = user.Name
            });
    }

    public async Task<bool> AddEnterRoomsAsync(EnterRoom[] enters)
    {
        return await AddObjectsGeneric(
            enters,
            "INSERT INTO EnterRooms (Id, DateTimeUtc, UserId) VALUES (@Id, @DateTimeUtc, @UserId)",
            x => new DbEnterRoom
            {
                Id = Guid.NewGuid().ToString(),
                DateTimeUtc = MapDateTimeUtc(x.TimestampUtc),
                UserId = x.Author.Id.ToString()
            });
    }

    public async Task<bool> AddLeaveRoomsAsync(LeaveRoom[] leaves)
    {
        return await AddObjectsGeneric(
            leaves,
            "INSERT INTO LeaveRooms (Id, DateTimeUtc, UserId) VALUES (@Id, @DateTimeUtc, @UserId)",
            x => new DbLeaveRoom
            {
                Id = Guid.NewGuid().ToString(),
                DateTimeUtc = MapDateTimeUtc(x.TimestampUtc),
                UserId = x.Author.Id.ToString()
            });
    }

    public async Task<bool> AddCommentsAsync(Comment[] comments)
    {
        return await AddObjectsGeneric(
            comments,
            "INSERT INTO Comments (Id, Message, DateTimeUtc, UserId) VALUES (@Id, @Message, @DateTimeUtc, @UserId)",
            x => new DbComment
            {
                Id = Guid.NewGuid().ToString(),
                Message = x.Message,
                DateTimeUtc = MapDateTimeUtc(x.TimestampUtc),
                UserId = x.Author.Id.ToString()
            });
    }

    public async Task<bool> AddHighFivesAsync(HighFive[] highFives)
    {
        return await AddObjectsGeneric(
            highFives,
            "INSERT INTO HighFives (Id, DateTimeUtc, AuthorId, ReceiverId) VALUES (@Id, @DateTimeUtc, @AuthorId, @ReceiverId)",
            x => new DbHighFive
            {
                Id = Guid.NewGuid().ToString(),
                DateTimeUtc = MapDateTimeUtc(x.TimestampUtc),
                AuthorId = x.Author.Id.ToString(),
                ReceiverId = x.Receiver.Id.ToString()
            });
    }

    private static string MapDateTimeUtc(DateTime dateTime)
    {
        return dateTime.ToString("O");
    }
}