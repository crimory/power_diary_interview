namespace Chat.App.Repository.Models;

public class DbComment
{
    public string Id { get; set; }
    public string Message { get; set; }
    public string DateTimeUtc { get; set; }
    public string UserId { get; set; }
}