using Microsoft.Extensions.Caching.Memory;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using Stories.API.Models;
using Stories.API.Stories.GetTopStories;
using System.Net;
using System.Text.Json;

namespace StoriesTests
{
    [TestFixture]
    public class GetTopStoriesHandlerTests
    {
        private Mock<IHttpClientFactory> _mockHttpClientFactory;
        private Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private Mock<IMemoryCache> _mockMemoryCache;
        private GetTopStoriesHandler _handler;

        [SetUp]
        public void Setup()
        {
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _mockMemoryCache = new Mock<IMemoryCache>();

            var httpClient = new HttpClient(_mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://hacker-news.firebaseio.com/v0/")
            };

            _mockHttpClientFactory
                .Setup(x => x.CreateClient("HackerNews"))
                .Returns(httpClient);

            _handler = new GetTopStoriesHandler(_mockMemoryCache.Object, _mockHttpClientFactory.Object);
        }

        [Test]
        public async Task Handle_ReturnsTopStories()
        {
            // Arrange
            var storyIds = new List<int> { 1, 2, 3 };
            var stories = new List<Story>
            {
                new() { Id = 1, Score = 100 },
                new() { Id = 2, Score = 150 },
                new() { Id = 3, Score = 120 }
            };

            SetupMockHttpMessageHandler(storyIds, stories);
            SetupMockCacheService(stories);

            var query = new GetTopStoriesQuery(2) ;

            // Act
            var response = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.That(2, Is.EqualTo(response.Stories.Count()));
            Assert.That(2, Is.EqualTo(response.Stories.First().Id));
            Assert.That(3, Is.EqualTo(response.Stories.Skip(1).First().Id));

        }

        private void SetupMockHttpMessageHandler(List<int> storyIds, List<Story> stories)
        {
            _mockHttpMessageHandler.SetupSendAsync("beststories.json", JsonSerializer.Serialize(storyIds));
            foreach (var story in stories)
            {
                _mockHttpMessageHandler.SetupSendAsync($"item/{story.Id}.json", JsonSerializer.Serialize(story));
            }
        }

        private void SetupMockCacheService(List<Story> stories)
        {
            _mockMemoryCache.Setup(x => x.CreateEntry(It.IsAny<object>()))
                .Returns(Mock.Of<ICacheEntry>);
            _mockMemoryCache.Setup(x => x.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny))
                .Returns(false)  // Simulate cache miss
                .Callback((object _, out object value) =>
                {
                    value = stories;
                });
        }
    }

    public static class MockHttpMessageHandlerExtensions
    {
        public static void SetupSendAsync(this Mock<HttpMessageHandler> mockHttpMessageHandler, string requestUri, string responseContent)
        {
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().EndsWith(requestUri) && req.Method == HttpMethod.Get),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(responseContent)
                });
        }
    }
}
