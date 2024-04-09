using System;

namespace HttpClientUtility.Test;

[TestClass]
public class QueryStringParametersListTests
{
    [TestMethod]
    public void AddParameter_ValidParameters_AddedSuccessfully()
    {
        // Arrange
        var parameters = new QueryStringParametersList();

        // Act
        parameters.Add("key", "value");

        // Assert
        Assert.AreEqual("key=value", parameters.GetQueryStringPostfix());
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void AddParameter_NullKey_ThrowsArgumentException()
    {
        // Arrange
        var parameters = new QueryStringParametersList();

        // Act
        parameters.Add(null, "value");

        // Assert is handled by the ExpectedException
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void AddParameter_WhitespaceKey_ThrowsArgumentException()
    {
        // Arrange
        var parameters = new QueryStringParametersList();

        // Act
        parameters.Add(" ", "value");

        // Assert is handled by the ExpectedException
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AddParameter_NullValue_ThrowsArgumentNullException()
    {
        // Arrange
        var parameters = new QueryStringParametersList();

        // Act
        parameters.Add("key", null);

        // Assert is handled by the ExpectedException
    }

    [TestMethod]
    public void AddParameter_DuplicateKey_UpdatesValue()
    {
        // Arrange
        var parameters = new QueryStringParametersList();
        parameters.Add("key", "value1");

        // Act
        parameters.Add("key", "value2");

        // Assert
        Assert.AreEqual("key=value1&key=value2", parameters.GetQueryStringPostfix());
    }
}
