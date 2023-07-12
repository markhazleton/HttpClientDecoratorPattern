using HttpClientDecorator.Models.Joke;

namespace HttpClientDecorator.Tests.Models.Joke;

[TestClass]
public class JokeModelTests
{
    [TestMethod]
    public void JokeModel_SetProperties_ValidData_PropertiesSetCorrectly()
    {
        // Arrange
        var model = new JokeModel();
        bool error = true;
        string category = "Funny";
        string type = "Single";
        string setup = "Why did the chicken cross the road?";
        string delivery = "To get to the other side!";
        string joke = "Why don't scientists trust atoms? Because they make up everything!";
        var flags = new FlagsModel();
        int id = 123;
        bool safe = true;
        string lang = "en";

        // Act
        model.error = error;
        model.category = category;
        model.type = type;
        model.setup = setup;
        model.delivery = delivery;
        model.joke = joke;
        model.flags = flags;
        model.id = id;
        model.safe = safe;
        model.lang = lang;

        // Assert
        Assert.AreEqual(error, model.error);
        Assert.AreEqual(category, model.category);
        Assert.AreEqual(type, model.type);
        Assert.AreEqual(setup, model.setup);
        Assert.AreEqual(delivery, model.delivery);
        Assert.AreEqual(joke, model.joke);
        Assert.AreEqual(flags, model.flags);
        Assert.AreEqual(id, model.id);
        Assert.AreEqual(safe, model.safe);
        Assert.AreEqual(lang, model.lang);
    }
}
