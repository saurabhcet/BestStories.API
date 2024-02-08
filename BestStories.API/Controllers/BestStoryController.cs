using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace BestStories.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BestStoryController : ControllerBase
    {
        private readonly HackerNewsService _hackerNewsService;
        private readonly ILogger<BestStoryController> _logger;        
        private readonly IConfiguration _configuration;
        private readonly short _maxAllowedStories;        
        private const string HACKERAPI_MAXALLOWEDSTORIES_CONFIG = "HackerAPI:MaxAllowedStories";

        public BestStoryController(HackerNewsService hackerNewsService, IConfiguration configuration, ILogger<BestStoryController> logger)
        {
            _hackerNewsService = hackerNewsService;
            _configuration = configuration;
            _logger = logger;
            _maxAllowedStories = Convert.ToInt16(_configuration[HACKERAPI_MAXALLOWEDSTORIES_CONFIG]);
        }

        [HttpGet("{n}")]
        public async Task<ActionResult<IEnumerable<Story>>> GetBestStories2(int n)
        {
            try
            {
                if (n > _maxAllowedStories)
                {
                    _logger.LogWarning($"User is trying to abuse the API, header details:{HttpContext.Request.Headers}");
                    return Problem($"Sorry we couldn't process your response, maximum allowed best stories: {_maxAllowedStories}");
                }

                var stories = await _hackerNewsService.GetBestStories(n);

                return stories.Any() ? Ok(stories): NoContent();
            }
            catch(Exception ex)
            {                
                return Problem(ex.Message);
            }
        }

        //[HttpGet("{n}")]
        //public async Task<ActionResult> GetBestStories(int n)
        //{
        //    var bestStories = new List<Story>();

        //    if(n > _maxAllowedStories)
        //    {
        //        _logger.LogWarning($"User is trying to abuse the API, header details:{HttpContext.Request.Headers}");
        //        return NotFound($"Sorry we couldn't process your response, maximum allowed best stories: {_maxAllowedStories}");
        //    }

        //    //check if the result exists in cache for count n to avoid hitting hacker-news API
        //    if (_memoryCache.TryGetValue(BEST_STORIES, out List<Story> cachedStories) && cachedStories?.Count > n)
        //    {
        //        return Ok(cachedStories?.Take(n));
        //    }

        //    //get the story Ids from base API
        //    var storyIdsResponse = await _httpClient.GetAsync(_configuration[HACKERAPI_STORYIDS_CONFIG]);          
        //    if (!storyIdsResponse.IsSuccessStatusCode)
        //    {
        //        _logger.LogError($"\"Couldn't connect to story provider API, \n url{n}");
        //        return NotFound("Something went wrong and we couldn't connect to our service provider, please try after some time.");
        //    }                
        //    var bestStoryIds = await JsonSerializer.DeserializeAsync<List<int>>(await storyIdsResponse.Content.ReadAsStreamAsync()).ConfigureAwait(false);
        //    if (bestStoryIds != null && bestStoryIds.Any())
        //    {

        //        //Exclude the subset from the list which are available in cache
        //        int actualN = n;
        //        var bestStoryIdsAfterCacheExclusion = bestStoryIds;
        //        if (cachedStories!= null && cachedStories.Any())
        //        {
        //            bestStoryIdsAfterCacheExclusion = bestStoryIds.Where(num => !cachedStories.Select(x => x.Id).Contains(num)).ToList();
        //            actualN = n - cachedStories.Count;
        //        }

        //        foreach (var id in bestStoryIdsAfterCacheExclusion.Take(actualN))
        //        {
        //            //get story details for every id                    
        //            var storyResponse = await _httpClient.GetAsync(_configuration[HACKERAPI_STORYDETAIL_CONFIG]?.Replace("*", id.ToString())).ConfigureAwait(false);
        //            if (!storyResponse.IsSuccessStatusCode)
        //            {
        //                _logger.LogError($"Error response code from story provider API for story Id:{id}, status code:{storyResponse.StatusCode}, url:{id}");
        //                continue;
        //            }                       
        //            var story = await JsonSerializer.DeserializeAsync<Story>(await storyResponse.Content.ReadAsStreamAsync()).ConfigureAwait(false);
        //            if (story != null)                    
        //                bestStories.Add(story);                                        
        //        }
        //    }
        //    if (cachedStories != null && cachedStories.Any())
        //    {
        //         bestStories.AddRange(cachedStories);
        //    }

        //    var sortedBestStories = bestStories.OrderByDescending(x => x.Score).ToList();            
            
        //    //Refresh the cache every 24 hr(adjust the expiration time based on needs)
        //    _memoryCache.Set(BEST_STORIES, sortedBestStories, TimeSpan.FromDays(1));

        //    return Ok(sortedBestStories);
        //}
    }
}