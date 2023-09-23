using Chat.App.Repository.Models;
using Chat.Domain;

namespace Chat.App.Repository;

public interface IRepositoryChatEvent
{
    Task<bool> AddEnterRoomsAsync(EnterRoom[] comments);
    Task<bool> AddLeaveRoomsAsync(LeaveRoom[] comments);
    Task<bool> AddCommentsAsync(Comment[] comments);
    Task<bool> AddHighFivesAsync(HighFive[] comments);
}

public class RepositoryChatEvent : IRepositoryChatEvent
{
    private readonly IRepositorySettings _repositorySettings;

    public RepositoryChatEvent(IRepositorySettings repositorySettings)
    {
        _repositorySettings = repositorySettings;
    }
    
    public async Task<bool> AddEnterRoomsAsync(EnterRoom[] enters)
    {
        return await RepositoryAddObjectBatched.AddObjectsGeneric(
            enters,
            "INSERT INTO EnterRooms (Id, DateTimeUtc, UserId) VALUES (@Id, @DateTimeUtc, @UserId)",
            x => new DbEnterRoom
            {
                Id = Guid.NewGuid().ToString(),
                DateTimeUtc = MapDateTimeUtc(x.TimestampUtc),
                UserId = x.Author.Id.ToString()
            },
            _repositorySettings.DatabaseConnectionString);
    }

    public async Task<bool> AddLeaveRoomsAsync(LeaveRoom[] leaves)
    {
        return await RepositoryAddObjectBatched.AddObjectsGeneric(
            leaves,
            "INSERT INTO LeaveRooms (Id, DateTimeUtc, UserId) VALUES (@Id, @DateTimeUtc, @UserId)",
            x => new DbLeaveRoom
            {
                Id = Guid.NewGuid().ToString(),
                DateTimeUtc = MapDateTimeUtc(x.TimestampUtc),
                UserId = x.Author.Id.ToString()
            },
            _repositorySettings.DatabaseConnectionString);
    }

    public async Task<bool> AddCommentsAsync(Comment[] comments)
    {
        return await RepositoryAddObjectBatched.AddObjectsGeneric(
            comments,
            "INSERT INTO Comments (Id, Message, DateTimeUtc, UserId) VALUES (@Id, @Message, @DateTimeUtc, @UserId)",
            x => new DbComment
            {
                Id = Guid.NewGuid().ToString(),
                Message = x.Message,
                DateTimeUtc = MapDateTimeUtc(x.TimestampUtc),
                UserId = x.Author.Id.ToString()
            },
            _repositorySettings.DatabaseConnectionString);
    }

    public async Task<bool> AddHighFivesAsync(HighFive[] highFives)
    {
        return await RepositoryAddObjectBatched.AddObjectsGeneric(
            highFives,
            "INSERT INTO HighFives (Id, DateTimeUtc, AuthorId, ReceiverId) VALUES (@Id, @DateTimeUtc, @AuthorId, @ReceiverId)",
            x => new DbHighFive
            {
                Id = Guid.NewGuid().ToString(),
                DateTimeUtc = MapDateTimeUtc(x.TimestampUtc),
                AuthorId = x.Author.Id.ToString(),
                ReceiverId = x.Receiver.Id.ToString()
            },
            _repositorySettings.DatabaseConnectionString);
    }

    private static string MapDateTimeUtc(DateTime dateTime)
    {
        return dateTime.ToString("O");
    }
}