using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using NzbDrone.Common.Http;
using NzbDrone.Core.ImportLists.RSSImport;
using NzbDrone.Core.Test.Framework;

namespace NzbDrone.Core.Test.ImportList
{
    [TestFixture]
    public class RSSImportFixture : CoreTest<RSSImport>
    {
        [SetUp]
        public void Setup()
        {
            Subject.Definition = Subject.DefaultDefinitions.First();
        }

        private void GivenRecentFeedResponse(string rssXmlFile)
        {
            var recentFeed = ReadAllText(@"Files/" + rssXmlFile);

            Mocker.GetMock<IHttpClient>()
                .Setup(o => o.Execute(It.IsAny<HttpRequest>()))
                .Returns<HttpRequest>(r => new HttpResponse(r, new HttpHeader(), recentFeed));
        }

        [Test]
        public void should_fetch_imdb_list()
        {
            GivenRecentFeedResponse("imdb_watchlist.xml");

            var result = Subject.Fetch();

            result.Movies.First().Title.Should().Be("Think Like a Man Too");
        }
    }
}
