using System.Data;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Chat.App.Repository;

public interface IRepositoryInitializer
{
    void Migration();
}

public class RepositoryInitializer : IRepositoryInitializer
{
    private readonly IRepositorySettings _repositorySettings;

    public RepositoryInitializer(IRepositorySettings repositorySettings)
    {
        _repositorySettings = repositorySettings;
    }

    public void Migration()
    {
        using var connection = new SqliteConnection(_repositorySettings.DatabaseConnectionString);
        connection.Open();
        
        if (!TableExists(connection, "Users"))
        {
            connection.Execute(
                @"CREATE TABLE Users(
                Id TEXT PRIMARY KEY,
                Name TEXT NOT NULL);
                ");
        }
        
        if (!TableExists(connection, "Comments"))
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
    }

    private static bool TableExists(IDbConnection connection, string tableName)
    {
        var tableCount = connection.QueryFirstOrDefault<int>(
            $"SELECT count(*) FROM sqlite_master WHERE type='table' AND name='{tableName}';");
        return tableCount > 0;
    }
}