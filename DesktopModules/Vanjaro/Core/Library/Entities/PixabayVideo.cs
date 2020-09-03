using System.Collections.Generic;

namespace Vanjaro.Core.Entities
{
    public class PixabayVideo
    {
        public List<VideoSearchHit> Hits { get; set; }

        public int Total { get; set; }

        public int TotalHits { get; set; }
    }

    public class VideoSearchHit
    {
        public int Id { get; set; }
        public string PageURL { get; set; }
        public string Type { get; set; }
        public string Tags { get; set; }
        public int Duration { get; set; }
        public string picture_id { get; set; }
        public Videos Videos { get; set; }
        public int Views { get; set; }
        public int Downloads { get; set; }
        public int Favorites { get; set; }
        public int Likes { get; set; }
        public int Comments { get; set; }
        public int UserId { get; set; }
        public string User { get; set; }
        public string UserImageURL { get; set; }
    }

    public class Videos
    {
        public Video Large { get; set; }
        public Video Medium { get; set; }
        public Video Small { get; set; }

    }

    public class Video
    {
        public string url { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int Size { get; set; }
    }
}