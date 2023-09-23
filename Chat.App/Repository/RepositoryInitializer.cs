using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Chat.App.Repository;

public interface IRepositoryInitializer
{
    bool Migration();
}

public class RepositoryInitializer : IRepositoryInitializer
{
    private readonly IRepositorySettings _repositorySettings;

    public RepositoryInitializer(IRepositorySettings repositorySettings)
    {
        _repositorySettings = repositorySettings;
    }

    public bool Migration()
    {
        using var connection = new SqliteConnection(_repositorySettings.DatabaseConnectionString);
        connection.Open();

        var usersDoNotExist = !TableExists(connection, "Users");
        if (usersDoNotExist)
        {
            connection.Execute(
                @"CREATE TABLE Users(
                Id TEXT PRIMARY KEY,
                Name TEXT NOT NULL);
                ");
        }
        
        var enterRoomsDoNotExist = !TableExists(connection, "EnterRooms");
        if (enterRoomsDoNotExist)
        {
            connection.Execute(
                @"CREATE TABLE EnterRooms(
                Id TEXT PRIMARY KEY,
                DateTimeUtc TEXT NOT NULL,
                UserId TEXT NOT NULL,
                FOREIGN KEY (UserId) REFERENCES Users(Id));
                ");
        }
        
        var leaveRoomsDoNotExist = !TableExists(connection, "LeaveRooms");
        if (leaveRoomsDoNotExist)
        {
            connection.Execute(
                @"CREATE TABLE LeaveRooms(
                Id TEXT PRIMARY KEY,
                DateTimeUtc TEXT NOT NULL,
                UserId TEXT NOT NULL,
                FOREIGN KEY (UserId) REFERENCES Users(Id));
                ");
        }
        
        var commentsDoNotExist = !TableExists(connection, "Comments");
        if (commentsDoNotExist)
        {
            connection.Execute(
                @"CREATE TABLE Comments(
                Id TEXT PRIMARY KEY,
                Message TEXT NOT NULL,
                DateTimeUtc TEXT NOT NULL,
                UserId TEXT NOT NULL,
                FOREIGN KEY (UserId) REFERENCES Users(Id));
                ");
        }

        var highFivesDoNotExist = !TableExists(connection, "HighFives");
        if (highFivesDoNotExist)
        {
            connection.Execute(
                @"CREATE TABLE HighFives(
                Id TEXT PRIMARY KEY,
                DateTimeUtc TEXT NOT NULL,
                AuthorId TEXT NOT NULL,
                ReceiverId TEXT NOT NULL,
                FOREIGN KEY (AuthorId) REFERENCES Users(Id),
                FOREIGN KEY (ReceiverId) REFERENCES Users(Id));
                ");
        }

        return new[]
        {
            usersDoNotExist,
            enterRoomsDoNotExist,
            leaveRoomsDoNotExist,
            commentsDoNotExist,
            highFivesDoNotExist
        }.Any(x => x);
    }

    private static bool TableExists(IDbConnection connection, string tableName)
    {
        var tableCount = connection.QueryFirstOrDefault<int>(
            $"SELECT count(*) FROM sqlite_master WHERE type='table' AND name='{tableName}';");
        return tableCount > 0;
    }
}