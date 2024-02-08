using System.Text.Json.Serialization;

namespace BestStories.API
{
    public class Story
    {
        [JsonPropertyName("id")] 
        public int Id { get; set; }

        [JsonPropertyName("score")] 
        public int Score { get; set; }

        [JsonPropertyName("descendants")]
        public int CommentCount { get; set; }

        [JsonPropertyName("time")] 
        public long Time { get; set; }

        [JsonPropertyName("title")] 
        public string? Title { get; set; }

        [JsonPropertyName("type")] 
        public string? Type { get; set; }

        [JsonPropertyName("url")]
        public string? Uri { get; set; }

        [JsonPropertyName("by")] 
        public string? PostedBy { get; set; }        
    }
}