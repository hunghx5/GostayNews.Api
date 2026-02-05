using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoStay.DataDto.News
{
    public class NewsTabHome
    {
        public List<CategoryNews> Categories { get; set; }
        public Dictionary<int,List<NewsHomeData>> NewsForCategories { get; set; }
        public List<NewsHomeData> HotNews { get; set; }
        public List<NewsHomeData> LatestNews { get; set; }
    }
    public class NewsHomeData
    {
        public int Id { get; set; }
        public int Status { get; set; }
        public string Title { get; set; }
        public string TitleNoCode { get; set; }
        public int IndexOfKey { get; set; }
        public List<TitlePartial> TitlePartial { get; set; }

        public DateTime DateCreate { get; set; }
        public int IdCategory { get; set; }
        public List<int>? IdTopics { get; set; }
        public string? PictureTitle { get; set; }
        public string? Description { get; set; }
        public string? Url { get; set; }
        public string Category { get; set; }
        public string CategoryEng { get; set; }
        public string CategoryChi { get; set; }
        public string UserName { get; set; }
        public int Click { get; set; }
        public int CommentCount { get; set; }
        public string Slug { get; set; }
        public int Total { get; set; }
        public int PageNum { get; set; }
    }
    public class TitlePartial
    {
        public int Index { get; set; }
        public string Value { get; set; }=string.Empty;
        public bool IsBold { get; set; }
    }
    public class CategoryNews
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
