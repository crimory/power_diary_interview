namespace Chat.Domain.Tests.ChatHistoryGeneratorTests;

public class ChatHistoryGeneratorTests
{
    public static IEnumerable<object[]> WeirdInputs =>
        new List<object[]>
        {
            new object[] {new TimeSpan(0, 0, 0, 0, 0), 2},
            new object[] {new TimeSpan(0, 1, 0, 0, 0), 0}
        };
    
    [Theory]
    [MemberData(nameof(WeirdInputs))]
    public void GenerateHistory_ShouldReturnEmpties_ForWeirdInputs(
        TimeSpan howFarBack, uint numberOfUsers)
    {
        var result = ChatHistoryGenerator.ChatHistoryGenerator.GenerateHistory(
            howFarBack, numberOfUsers);
        
        Assert.Empty(result.enters);
        Assert.Empty(result.leaves);
        Assert.Empty(result.comments);
        Assert.Empty(result.highFives);
        Assert.Empty(result.users);
    }
    
    [Theory]
    [InlineData(2)]
    [InlineData(23)]
    [InlineData(76)]
    public void GenerateHistory_ShouldReturnMatchingNumberOfUsers(uint numberOfUsers)
    {
        var result = ChatHistoryGenerator.ChatHistoryGenerator.GenerateHistory(
            new TimeSpan(0, 1, 0, 0), numberOfUsers);
        
        Assert.Equal((int)numberOfUsers, result.users.Length);
    }
    
    [Theory]
    [InlineData(1)]
    [InlineData(4)]
    [InlineData(10)]
    [InlineData(15)]
    [InlineData(50)]
    public void GenerateHistory_ShouldReturnEnterAndMatchingLeave(uint numberOfUsers)
    {
        var result = ChatHistoryGenerator.ChatHistoryGenerator.GenerateHistory(
            new TimeSpan(50, 1, 0, 0), numberOfUsers);

        foreach (var user in result.users)
        {
            var matchingEntersNumber = result.enters.Count(x => x.Author == user);
            var matchingLeavesNumber = result.leaves.Count(x => x.Author == user);
            Assert.NotEqual(0, matchingEntersNumber);
            Assert.Equal(matchingEntersNumber, matchingLeavesNumber);
        }
    }
}