using Chat.App.Repository;

namespace Chat.Blazor.Server.Repository;

public class RepositorySettings : IRepositorySettings
{
    private readonly IConfiguration _configuration;

    public RepositorySettings(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string DatabaseConnectionString =>
        _configuration.GetValue<string>("ConnectionStrings:Database")
        ?? throw new ArgumentNullException(nameof(DatabaseConnectionString));
}