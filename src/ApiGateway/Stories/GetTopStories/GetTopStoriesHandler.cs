using FluentValidation;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Stories.API.Models;
using Stories.API.Services;
using System.Text.Json;

namespace Stories.API.Stories.GetTopStories
{
    public record GetTopStoriesQuery(int Count) : IRequest<GetTopStoriesResponse>;

    public class GetTopStoriesQueryValidator : AbstractValidator<GetTopStoriesQuery>
    {
        public GetTopStoriesQueryValidator()
        {
            RuleFor(o => o.Count)
                .GreaterThan(0);
        }
    }


    public class GetTopStoriesHandler(
        IMemoryCache cache,
        IHttpClientFactory httpClientFactory)
        : IRequestHandler<GetTopStoriesQuery, GetTopStoriesResponse>
    {
        private const string BaseUrl = "https://hacker-news.firebaseio.com/v0/";
        private const string CacheKey = "top_stories";

        // Concurrency limit: SemaphoreSlim with a limit of 10 allows a maximum of 10 HTTP requests
        // to be executed simultaneously.
        // This prevents external API from being overloaded
        private static readonly SemaphoreSlim Semaphore = new(10); 


        private readonly HttpClient _httpClient = httpClientFactory.CreateClient("HackerNews");
        private readonly CacheService _cache = new(cache);


        public async Task<GetTopStoriesResponse> Handle(GetTopStoriesQuery query, CancellationToken cancellationToken)
        {
            var stories = await _cache.GetOrCreateAsync(CacheKey, FetchStoriesAsync);
            return new GetTopStoriesResponse(stories.Take(query.Count));
        }

        private async Task<IEnumerable<Story>> FetchStoriesAsync()
        {
            var storyIds = await GetAsync<List<int>>($"{BaseUrl}beststories.json");

            var tasks = storyIds
                .Select(id => GetAsync<Story>($"{BaseUrl}item/{id}.json"));

            return (await Task.WhenAll(tasks))
                .OrderByDescending(s => s.Score);
        }

        private async Task<T> GetAsync<T>(string url)
        {
            await Semaphore.WaitAsync(); // We are waiting for an available slot.
            try
            {
                var storyResponse = await _httpClient.GetAsync(url);
                storyResponse.EnsureSuccessStatusCode();
                await using var stream = await storyResponse.Content.ReadAsStreamAsync();
                var result = await JsonSerializer.DeserializeAsync<T>(stream,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (result == null)
                    throw new InvalidOperationException("Deserialized object is null.");

                return result;

            }
            finally
            {
                Semaphore.Release(); // Free the slot after completing the request.
            }
        }

    }
}
