using System;
using Newtonsoft.Json;

namespace VideoDownloader.App.Model
{
    public class Course
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("courseId")]
        public Guid CourseId { get; set; }

        [JsonProperty("slug")]
        public string Slug { get; set; }

        [JsonProperty("publishedOn")]
        public DateTimeOffset PublishedOn { get; set; }

        [JsonProperty("updatedOn")]
        public DateTimeOffset UpdatedOn { get; set; }

        [JsonProperty("displayDate")]
        public DateTimeOffset DisplayDate { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("shortDescription")]
        public string ShortDescription { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("level")]
        public string Level { get; set; }

        [JsonProperty("duration")]
        public string Duration { get; set; }

        [JsonProperty("rating")]
        public Rating Rating { get; set; }

        [JsonProperty("popularityScore")]
        public long PopularityScore { get; set; }

        [JsonProperty("courseImageUrl")]
        public Uri CourseImageUrl { get; set; }

        [JsonProperty("courseImage")]
        public Image CourseImage { get; set; }

        [JsonProperty("image")]
        public Image Image { get; set; }

        [JsonProperty("audiences")]
        public string[] Audiences { get; set; }

        [JsonProperty("modules")]
        public Module[] Modules { get; set; }

        [JsonProperty("playerUrl")]
        public string PlayerUrl { get; set; }

        [JsonProperty("authors")]
        public Author[] Authors { get; set; }

        [JsonProperty("skillPaths")]
        public object[] SkillPaths { get; set; }

        [JsonProperty("hasLearningCheck")]
        public bool HasLearningCheck { get; set; }

        [JsonProperty("hasTranscript")]
        public bool HasTranscript { get; set; }

        public bool SupportsWideScreen { get; set; }
    }

    public class Image
    {
        [JsonProperty("alt")]
        public string Alt { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("isDefault")]
        public bool IsDefault { get; set; }
    }

    public class Rating
    {
        [JsonProperty("average")]
        public long Average { get; set; }

        [JsonProperty("ratersCount")]
        public long RatersCount { get; set; }
    }
}
