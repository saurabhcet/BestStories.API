using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace BestStories.API
{
    public class HackerNewsService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HackerNewsService> _logger;        
        private readonly IMemoryCache _memoryCache;
        private readonly IConfiguration _configuration;
        

        private const string BEST_STORIES = "BestStories";
        private const string HACKERAPI_STORYIDS_CONFIG = "HackerAPI:StoryIds";
        private const string HACKERAPI_STORYDETAIL_CONFIG = "HackerAPI:StoryDetail";

        public HackerNewsService(HttpClient httpClient, IMemoryCache memoryCache, IConfiguration configuration, ILogger<HackerNewsService> logger)
        {
            _httpClient = httpClient;
            _memoryCache = memoryCache;
            _configuration = configuration;
            _logger = logger;            
        }

        public async Task<IEnumerable<Story>> GetBestStories(int n)
        {
            var bestStories = new List<Story>();

            //Check if the result exists in cache for count n to avoid hitting hacker-news API repeatedly
            if (_memoryCache.TryGetValue(BEST_STORIES, out List<Story> cachedStories) && cachedStories?.Count >= n)
            {
                return cachedStories?.Take(n);
            }

            var bestStoryIds = await GetBestStoryIds();
            if (bestStoryIds != null && bestStoryIds.Any())
            {
                //Cache lookup
                var bestStoryIdsAfterCacheExclusion = GetStoryIdsAfterCacheExclusion(bestStoryIds, cachedStories);
                var actualN = GetStoryIdCountAfterCacheExclusion(cachedStories, n);

                foreach (var id in bestStoryIdsAfterCacheExclusion.Take(actualN))
                {
                    //get story details for every id                    
                    var storyUrl = _configuration[HACKERAPI_STORYDETAIL_CONFIG]?.Replace("*", id.ToString());
                    var storyResponse = await _httpClient.GetAsync(storyUrl).ConfigureAwait(false);
                    if (!storyResponse.IsSuccessStatusCode)
                    {
                        _logger.LogError($"Error response code from story provider API for story Id:{id}, status code:{storyResponse.StatusCode}, url:{id}");
                        continue;
                    }
                    var story = await JsonSerializer.DeserializeAsync<Story>(await storyResponse.Content.ReadAsStreamAsync()).ConfigureAwait(false);
                    if (story != null)
                        bestStories.Add(story);
                }
            }
            if (cachedStories != null && cachedStories.Any())
            {
                bestStories.AddRange(cachedStories);
            }

            var sortedBestStories = bestStories.OrderByDescending(x => x.Score).ToList();

            //Refresh the cache every 24 hr(adjust the expiration time based on needs)
            _memoryCache.Set(BEST_STORIES, sortedBestStories, TimeSpan.FromDays(1));

            return sortedBestStories;
        }

        private async Task<List<int>> GetBestStoryIds()
        {
            //get the story Ids from base API
            var storyIdsResponse = await _httpClient.GetAsync(_configuration[HACKERAPI_STORYIDS_CONFIG]);
            if (!storyIdsResponse.IsSuccessStatusCode)
            {
                _logger.LogError($"\"Couldn't connect to story provider API, \n url:{_configuration[HACKERAPI_STORYIDS_CONFIG]}");
                throw new Exception("Something went wrong and we couldn't connect to our service provider, please try after some time.");
            }
            return await JsonSerializer.DeserializeAsync<List<int>>(await storyIdsResponse.Content.ReadAsStreamAsync()).ConfigureAwait(false);            
        }

        private List<int> GetStoryIdsAfterCacheExclusion(List<int> bestStoryIds, List<Story> cachedStories)
        {
            //Exclude the subset from the list which are available in cache            
            var bestStoryIdsAfterCacheExclusion = bestStoryIds;
            if (cachedStories != null && cachedStories.Any())
            {
                bestStoryIdsAfterCacheExclusion = bestStoryIds.Where(num => !cachedStories.Select(x => x.Id).Contains(num)).ToList();                
            }
            return bestStoryIdsAfterCacheExclusion;
        }

        private int GetStoryIdCountAfterCacheExclusion(List<Story> cachedStories, int n)
        {
            //Exclude the subset from the list which are available in cache
            int actualN = n;            
            if (cachedStories != null && cachedStories.Any())
            {                
                actualN = n - cachedStories.Count;
            }
            return actualN;
        }
    }
}