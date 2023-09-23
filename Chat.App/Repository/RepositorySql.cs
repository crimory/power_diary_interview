using Chat.App.Repository.Models;
using Chat.Domain;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Chat.App.Repository;

public interface IRepositorySql
{
    Task<bool> AddUsersAsync(User[] users);
    Task<bool> AddCommentsAsync(Comment[] comments);
}

public class RepositorySql : IRepositorySql
{
    private readonly IRepositorySettings _repositorySettings;

    public RepositorySql(IRepositorySettings repositorySettings)
    {
        _repositorySettings = repositorySettings;
    }

    public async Task<bool> AddUsersAsync(User[] users)
    {
        if (!users.Any())
            return false;
        
        await using var connection = new SqliteConnection(_repositorySettings.DatabaseConnectionString);
        await connection.OpenAsync();

        var dbUsers = users.Select(x => new DbUser
        {
            Id = x.Id.ToString(),
            Name = x.Name
        }).ToArray();
        
        var newUsersCount = await connection.ExecuteAsync(@"
            INSERT INTO Users (Id, Name)
            VALUES (@Id, @Name)", dbUsers);
        
        return newUsersCount == users.Length;
    }

    public async Task<bool> AddCommentsAsync(Comment[] comments)
    {
        if (!comments.Any())
            return false;
        
        await using var connection = new SqliteConnection(_repositorySettings.DatabaseConnectionString);
        await connection.OpenAsync();

        var dbComments = comments.Select(x => new DbComment
        {
            Id = Guid.NewGuid().ToString(),
            Message = x.Message,
            DateTimeUtc = MapDateTimeUtc(x.TimestampUtc),
            UserId = x.Author.Id.ToString()
        }).ToArray();
        
        var newCommentsCount = await connection.ExecuteAsync(@"
            INSERT INTO Comments (Id, Message, DateTimeUtc, UserId)
            VALUES (@Id, @Message, @DateTimeUtc, @UserId)", dbComments);
        
        return newCommentsCount == comments.Length;
    }

    private static string MapDateTimeUtc(DateTime dateTime)
    {
        return dateTime.ToString("O");
    }
}