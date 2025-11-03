using AlbionNavigator.Services;

namespace UnitTests;

[TestClass]
public sealed partial class LinkServiceTests
{
    [TestMethod]
    public void LinkServiceTest1()
    {
        var service = new LinkService(() => { }, (_, _) => { });
        ZoneLink[] items =
        [
            new (0, 1, DateTime.UtcNow.AddHours(1).ToString("O")),
            new (1, 2, DateTime.UtcNow.AddHours(3).ToString("O")),
            new (2, 3, DateTime.UtcNow.AddHours(2).ToString("O")),
            new (3, 4, DateTime.UtcNow.AddHours(5).ToString("O")),
            new (4, 5, DateTime.UtcNow.AddHours(4).ToString("O")),
        ];
        service.AddLink(items);

        for (var i = 1; i < service.Links.Count; i++)
        {
            var firstLink = service.Links[i - 1];
            var secondLink = service.Links[i];
            Assert.IsTrue(firstLink.IsLaterThanExpiration(secondLink.Expiration));
        }
    }
}