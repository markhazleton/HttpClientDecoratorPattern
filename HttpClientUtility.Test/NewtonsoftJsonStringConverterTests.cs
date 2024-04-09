using HttpClientUtility.StringConverter;

namespace HttpClientUtility.Test;

[TestClass]
public class NewtonsoftJsonStringConverterTests
{
    /// <summary>
    /// Test Model
    /// </summary>
    public class TestModel
    {
        public TestModel(string name, int value)
        {
            Name = name;
            Value = value;
        }
        public string Name { get; set; }
        public int Value { get; set; }
    }
    [TestMethod]
    public void ConvertFromModel_WithValidModel_ReturnsValidJsonString()
    {
        // Arrange
        var converter = new NewtonsoftJsonStringConverter();
        TestModel testModel = new("Test", 123);

        // Act
        var jsonString = converter.ConvertFromModel(testModel);

        // Assert
        Assert.IsFalse(string.IsNullOrWhiteSpace(jsonString));
        Assert.IsTrue(jsonString.Contains("\"Name\":\"Test\""));
        Assert.IsTrue(jsonString.Contains("\"Value\":123"));
    }

    [TestMethod]
    public void ConvertFromString_WithValidJsonString_ReturnsValidModel()
    {
        // Arrange
        var converter = new NewtonsoftJsonStringConverter();
        var jsonString = "{\"Name\":\"Test\",\"Value\":123}";

        // Act
        var result = converter.ConvertFromString<TestModel>(jsonString);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Test", result.Name);
        Assert.AreEqual(123, result.Value);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void ConvertFromString_WithNullOrWhitespace_ThrowsArgumentException()
    {
        // Arrange
        var converter = new NewtonsoftJsonStringConverter();
        string jsonString = " ";

        // Act
        converter.ConvertFromString<dynamic>(jsonString);

        // Assert is handled by ExpectedException
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void ConvertFromString_WithInvalidJsonString_ThrowsInvalidOperationException()
    {
        // Arrange
        var converter = new NewtonsoftJsonStringConverter();
        var jsonString = "{\"Invalid\":\"Json\""; // Deliberately malformed JSON

        // Act
        converter.ConvertFromString<dynamic>(jsonString);

        // Assert is handled by ExpectedException
    }


}