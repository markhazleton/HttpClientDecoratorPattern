using HttpClientDecorator.Models.Joke;

namespace HttpClientDecorator.Tests.Models.Joke
{
    [TestClass]
    public class FlagsModelTests
    {
        [TestMethod]
        public void FlagsModel_SetProperties_ValidData_PropertiesSetCorrectly()
        {
            // Arrange
            var model = new FlagsModel();
            bool nsfw = true;
            bool religious = false;
            bool political = true;
            bool racist = false;
            bool sexist = true;
            bool explicitFlag = false;

            // Act
            model.nsfw = nsfw;
            model.religious = religious;
            model.political = political;
            model.racist = racist;
            model.sexist = sexist;
            model.@explicit = explicitFlag;

            // Assert
            Assert.AreEqual(nsfw, model.nsfw);
            Assert.AreEqual(religious, model.religious);
            Assert.AreEqual(political, model.political);
            Assert.AreEqual(racist, model.racist);
            Assert.AreEqual(sexist, model.sexist);
            Assert.AreEqual(explicitFlag, model.@explicit);
        }
    }
}
