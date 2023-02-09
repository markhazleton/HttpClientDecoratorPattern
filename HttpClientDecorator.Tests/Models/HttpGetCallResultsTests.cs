namespace HttpClientDecorator.Tests.Models;

[TestClass]
public class HttpGetCallResultsTests
{
    [TestMethod]
    public void HttpGetCallResults_DefaultConstructor_ShouldInitializeIterationAndStatusPath()
    {
        // Arrange & Act
        var results = new HttpGetCallResults();

        // Assert
        Assert.AreEqual(0, results.Iteration);
        Assert.AreEqual(string.Empty, results.StatusPath);
    }

    [TestMethod]
    public void HttpGetCallResults_CopyConstructor_ShouldCopyIterationAndStatusPath()
    {
        // Arrange
        var statusCall = new HttpGetCallResults
        {
            Iteration = 1,
            StatusPath = "https://example.com"
        };

        // Act
        var results = new HttpGetCallResults(statusCall);

        // Assert
        Assert.AreEqual(statusCall.Iteration, results.Iteration);
        Assert.AreEqual(statusCall.StatusPath, results.StatusPath);
    }

    [TestMethod]
    public void HttpGetCallResults_ValueConstructor_ShouldInitializeIterationAndStatusPath()
    {
        // Arrange
        const int iteration = 2;
        const string statusPath = "https://example.com/status";

        // Act
        var results = new HttpGetCallResults(iteration, statusPath);

        // Assert
        Assert.AreEqual(iteration, results.Iteration);
        Assert.AreEqual(statusPath, results.StatusPath);
    }

    [TestMethod]
    public void HttpGetCallResults_CompletionDate_ShouldStoreCompletionDateAndTime()
    {
        // Arrange
        var completionDate = DateTime.Now;

        // Act
        var results = new HttpGetCallResults
        {
            CompletionDate = completionDate
        };

        // Assert
        Assert.AreEqual(completionDate, results.CompletionDate);
    }

    [TestMethod]
    public void HttpGetCallResults_ElapsedMilliseconds_ShouldStoreElapsedTime()
    {
        // Arrange
        const long elapsedMilliseconds = 12345;

        // Act
        var results = new HttpGetCallResults
        {
            ElapsedMilliseconds = elapsedMilliseconds
        };

        // Assert
        Assert.AreEqual(elapsedMilliseconds, results.ElapsedMilliseconds);
    }

    [TestMethod]
    public void HttpGetCallResults_StatusResults_ShouldStoreStatusResults()
    {
        // Arrange
        var statusResults = new
        {
            Status = "Success",
            Message = "The request was successful."
        };

        // Act
        var results = new HttpGetCallResults
        {
            StatusResults = statusResults
        };

        // Assert
        Assert.AreEqual(statusResults, results.StatusResults);
    }
}
