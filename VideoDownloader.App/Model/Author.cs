using System;
using Newtonsoft.Json;

namespace VideoDownloader.App.Model
{
    public class Author
    {
        [JsonProperty("authorId")]
        public Guid AuthorId { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("handle")]
        public string Handle { get; set; }

        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }
    }
}
