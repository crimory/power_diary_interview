namespace Chat.Domain;

public record ChatRoom(HashSet<User> Users);