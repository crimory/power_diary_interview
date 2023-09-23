using Dapper;
using Microsoft.Data.Sqlite;

namespace Chat.App.Repository;

internal static class RepositoryAddObjectBatched
{
    private const int BatchSize = 1000;
    
    internal static async Task<bool> AddObjectsGeneric<TInput, TDbInput>(
        TInput[] input, string sql, Func<TInput, TDbInput> mapInput,
        string dbConnectionString)
    {
        if (!input.Any())
            return false;

        await using var connection = new SqliteConnection(dbConnectionString);
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
}