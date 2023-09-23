namespace Chat.Domain;

public abstract record ChatEvent(DateTime TimestampUtc, User Author);

public record EnterRoom(DateTime TimestampUtc, User Author)
    : ChatEvent(TimestampUtc, Author);
public record LeaveRoom(DateTime TimestampUtc, User Author)
    : ChatEvent(TimestampUtc, Author);
public record Comment(DateTime TimestampUtc, User Author, string Message)
    : ChatEvent(TimestampUtc, Author);
public record HighFive(DateTime TimestampUtc, User Author, User Receiver)
    : ChatEvent(TimestampUtc, Author);