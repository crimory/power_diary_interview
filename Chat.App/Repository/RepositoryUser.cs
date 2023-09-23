using Chat.App.Repository.Models;
using Chat.Domain;
using Dapper;
using Microsoft.Data.Sqlite;

namespace Chat.App.Repository;

public interface IRepositoryUser
{
    Task<bool> AddUsersAsync(User[] users);
}

public class RepositoryUser : IRepositoryUser
{
    private readonly IRepositorySettings _repositorySettings;

    public RepositoryUser(IRepositorySettings repositorySettings)
    {
        _repositorySettings = repositorySettings;
    }

    public async Task<bool> AddUsersAsync(User[] users)
    {
        return await RepositoryAddObjectBatched.AddObjectsGeneric(
            users,
            "INSERT INTO Users (Id, Name)VALUES (@Id, @Name)",
            user => new DbUser
            {
                Id = user.Id.ToString(),
                Name = user.Name
            },
            _repositorySettings.DatabaseConnectionString);
    }
}