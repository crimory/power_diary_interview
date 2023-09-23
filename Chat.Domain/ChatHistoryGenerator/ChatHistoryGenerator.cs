using System.Collections.Concurrent;

namespace Chat.Domain.ChatHistoryGenerator;

public static class ChatHistoryGenerator
{
    private enum EventTypes {
        Enter,
        Leave,
        Comment,
        HighFive
    }
    
    private static readonly Random Rand = new();
    private static readonly string[] UserNames = {
        "Jon", "Annie", "Marcin", "Michael", "Lincoln", "Sara"
    };

    private static readonly string[] Messages =
    {
        "Thank the universe, it's Friday!",
        "How are you?",
        "How you doin'? ;)",
        "The truth is never what you would think of",
        "Amazing day today!",
        "Let's have some fun!",
        "F# is still better, than C# :P",
        "I require the highest of fives!"
    };
    
    public static (
        EnterRoom[] enters,
        LeaveRoom[] leaves,
        Comment[] comments,
        HighFive[] highFives,
        User[] users
        ) GenerateHistory(TimeSpan howFarBack, uint numberOfUsers, DateTime? endDateUtc = null)
    {
        if (numberOfUsers == 0 || howFarBack == TimeSpan.Zero)
            return (
                Array.Empty<EnterRoom>(),
                Array.Empty<LeaveRoom>(),
                Array.Empty<Comment>(),
                Array.Empty<HighFive>(),
                Array.Empty<User>()
            );
        
        var users = CreateRandomUsers(numberOfUsers);
        var room = new ChatRoom(new HashSet<User>());
        var enters = new ConcurrentBag<EnterRoom>();
        var leaves = new ConcurrentBag<LeaveRoom>();
        var comments = new ConcurrentBag<Comment>();
        var highFives = new ConcurrentBag<HighFive>();
        
        var endDate = endDateUtc ?? DateTime.UtcNow;
        if (howFarBack < TimeSpan.Zero)
            howFarBack = howFarBack.Negate();
        var dateBeingProcessed = endDate - howFarBack;
        
        while (dateBeingProcessed < endDate)
        {
            var randomEventType = (EventTypes)Rand.Next(Enum.GetNames(typeof(EventTypes)).Length);
            var success = randomEventType switch
            {
                EventTypes.Enter => AddRandomEnter(ref enters, dateBeingProcessed, users, ref room),
                EventTypes.Leave => AddRandomLeave(ref leaves, dateBeingProcessed, ref room),
                EventTypes.Comment => AddRandomComment(ref comments, dateBeingProcessed, ref room),
                EventTypes.HighFive => AddRandomHighFive(ref highFives, dateBeingProcessed, ref room),
                _ => throw new ArgumentOutOfRangeException(nameof(randomEventType))
            };
            if (!success)
                continue;
            
            dateBeingProcessed += new TimeSpan(0, 0, 0, Rand.Next(60));
        }

        while (AddRandomLeave(ref leaves, dateBeingProcessed, ref room))
        {
            dateBeingProcessed += new TimeSpan(0, 0, 0, 1);
        }

        return (
            enters.ToArray(),
            leaves.ToArray(),
            comments.ToArray(),
            highFives.ToArray(),
            users
        );
    }

    private static T SelectRandom<T>(T[] input)
    {
        return input[Rand.Next(input.Length)];
    }

    private static User[] CreateRandomUsers(uint numberOfUsers)
    {
        var users = new HashSet<User>();
        for (var i = 0; i < numberOfUsers; i++)
        {
            users.Add(new User(
                Guid.NewGuid(),
                SelectRandom(UserNames)
            ));
        }

        return users.ToArray();
    }

    private static bool AddRandomEnter(
        ref ConcurrentBag<EnterRoom> enters,
        DateTime timeUtc,
        User[] users,
        ref ChatRoom room)
    {
        var usersInTheRoom = room.Users.ToArray();
        var usersNotInTheRoom = users.Except(usersInTheRoom).ToArray();
        if (!usersNotInTheRoom.Any())
            return false;
        
        var randomUser = SelectRandom(usersNotInTheRoom);
        if (!room.Users.Add(randomUser))
            return false;
        
        enters.Add(new EnterRoom(timeUtc, randomUser));
        return true;
    }
    
    private static bool AddRandomLeave(
        ref ConcurrentBag<LeaveRoom> leaves,
        DateTime timeUtc,
        ref ChatRoom room)
    {
        if (room.Users.Count < 1)
            return false;
        
        var randomUser = SelectRandom(room.Users.ToArray());
        var success = room.Users.Remove(randomUser);
        if (!success)
            return false;
        
        leaves.Add(new LeaveRoom(timeUtc, randomUser));
        return true;
    }

    private static bool AddRandomComment(
        ref ConcurrentBag<Comment> comments,
        DateTime timeUtc,
        ref ChatRoom room)
    {
        if (room.Users.Count < 1)
            return false;
        
        var randomUser = SelectRandom(room.Users.ToArray());
        var message = SelectRandom(Messages);
        
        comments.Add(new Comment(timeUtc, randomUser, message));
        return true;
    }
    
    private static bool AddRandomHighFive(
        ref ConcurrentBag<HighFive> highFives,
        DateTime timeUtc,
        ref ChatRoom room)
    {
        if (room.Users.Count < 2)
            return false;
        
        var randomAuthor = SelectRandom(room.Users.ToArray());
        var otherUsersInTheChatRoom = room.Users.Except(new[] {randomAuthor}).ToArray();
        var randomReceiver = SelectRandom(otherUsersInTheChatRoom);
        
        highFives.Add(new HighFive(timeUtc, randomAuthor, randomReceiver));
        return true;
    }
}