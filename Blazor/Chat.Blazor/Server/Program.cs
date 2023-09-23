using Chat.App;
using Chat.App.Repository;
using Chat.Blazor.Server.Repository;
using Chat.Domain.ChatHistoryGenerator;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<IRepositorySettings, RepositorySettings>();
builder.Services.AddChatApp();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var repositoryInitializer = services.GetRequiredService<IRepositoryInitializer>();
    var migrationHappened = repositoryInitializer.Migration();
    if (migrationHappened)
    {
        Console.WriteLine("=== Filling Database with random data started ===");
        var yearBack = new TimeSpan(365, 0, 0, 0);
        const uint numberOfUsers = 6;
        var randomData = ChatHistoryGenerator.GenerateHistory(
            yearBack, numberOfUsers);
        
        var userRepo = services.GetRequiredService<IRepositoryUser>();
        await userRepo.AddUsersAsync(randomData.users);
        Console.WriteLine("Filling Database with random users completed");
        
        var chatEventRepo = services.GetRequiredService<IRepositoryChatEvent>();
        await chatEventRepo.AddEnterRoomsAsync(randomData.enters);
        Console.WriteLine("Filling Database with random chat enters completed");
        await chatEventRepo.AddLeaveRoomsAsync(randomData.leaves);
        Console.WriteLine("Filling Database with random chat leaves completed");
        await chatEventRepo.AddCommentsAsync(randomData.comments);
        Console.WriteLine("Filling Database with random chat comments completed");
        await chatEventRepo.AddHighFivesAsync(randomData.highFives);
        Console.WriteLine("Filling Database with random high fives completed");
        Console.WriteLine("=== Filling Database with random data completed ===");
    }
}

app.Run();