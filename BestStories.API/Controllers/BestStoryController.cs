using Microsoft.AspNetCore.Mvc;
using System.Text;

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
        public async Task<ActionResult<IEnumerable<Story>>> GetBestStories(int n)
        {
            try
            {
                if (n > _maxAllowedStories)
                {                    
                    StringBuilder sbHeaders = new StringBuilder();                    
                    foreach (var header in HttpContext.Request.Headers)                    
                        sbHeaders.AppendLine($"{header.Key}:{header.Value}");
                    
                    _logger.LogError($"User is trying to abuse the API, header details:{sbHeaders.ToString()}");
                    throw new InvalidOperationException($"Sorry we couldn't process your response, maximum allowed best stories: {_maxAllowedStories}");
                }

                var stories = await _hackerNewsService.GetBestStories(n);

                return stories.Any() ? Ok(stories): NoContent();
            }
            catch(Exception ex)
            {                
                return Problem(ex.Message);
            }
        }        
    }
}