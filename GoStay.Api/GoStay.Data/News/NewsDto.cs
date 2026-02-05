

using GoStay.DataAccess.Entities;

namespace GoStay.DataDto.News
{
    public partial class NewsDto
    {
        public int? Id { get; set; }
        public int? IdCategory { get; set; }
        public int? IdUser { get; set; }
        public string? Keysearch { get; set; }
        public string? Title { get; set; } = null!;
        public string? Description { get; set; }
        public int? LangId { get; set; }
        public List<int>? IdTopics { get; set; }
        public int? IdDomain { get; set; }
        public string? Url { get; set; }
    }
    public partial class NewsDetailDto
    {
        public int? Id { get; set; }
        public int? IdCategory { get; set; }
        public int? IdUser { get; set; }
        public int? IdDomain { get; set; }

        public byte? Status { get; set; }
        public string? Title { get; set; } = null!;
        public string? Content { get; set; }
        public string Category { get; set; }
        public string? PictureTitle { get; set; }
        public string? Description { get; set; }
        public string? Url { get; set; }
        public int? LangId { get; set; }
        public string? Language { get; set; }
        public List<int>? IdTopics { get; set; }
        public List<string>? Topics { get; set; }
        public string Slug { get; set; }
        public string? Tag { get; set; }
        public string? UserName { get; set; }
        public DateTime DateCreate { get; set; }
        public List<NewRelateDto>? NewRelates { get; set; }
        public List<NewByIdCategoryDto>? NewCategory { get; set; }
        public int QuatityComment { get; set; }
    }
    public class GetListNewsParam
    {
        public int? UserId { get; set; }
        public int? IdCategory { get; set; }
        public int? ParentIdCategory { get; set; }

        public int? IdTopic { get; set; }
        public int? IdDomain { get; set; }

        public byte? Status { get; set; }
        public string? TextSearch { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int? Style { get; set; }
    }
    public class NewSearchOutDto
    {
        public int Id { get; set; }
        public int Status { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime DateCreate { get; set; }
        public int IdCategory { get; set; }

        public string? PictureTitle { get; set; }
        public string? Description { get; set; }
        public string? Url { get; set; }

        public string Category { get; set; }
        public string CategoryEng { get; set; }
        public string CategoryChi { get; set; }

        public string UserName { get; set; }
        public int Total { get; set; }
        public int TotalPicture { get; set; }

        private string ListTopic
        {
            set
            {
                if (string.IsNullOrEmpty(value))
                    Topics = new List<int>();
                else
                {
                    Topics = Array.ConvertAll<string, int>(value.Split(';').ToArray(), int.Parse).ToList();
                }
            }
        }
        public List<int> Topics { get; set; } = new List<int>();
        public string Language { get; set; }
        public int LangId { get; set; }
        public string Tag { get; set; }
        public int Click { get; set; }
        public string Slug { get; set; }
    }
    public class EditNewsContentParam
    {
        public string Content { get; set; }
        public int NewsId { get; set; }
    }
    public class EditNewsPictureTitleParam
    {
        public string Url { get; set; }
        public int NewsId { get; set; }
    }
    public class KeyListVideo
    {
        public int IdCategory { get; set; }
        public int PageIndex { get; set; }
    } 
    public class VideoNewsDto
    {
        public int Id { get; set; }
        public string Video { get; set; } = null!;
        public string? Descriptions { get; set; }

        public string UrlVideo
        {
            get
            {
                return "https://cdn.realtech.com.vn" + Video;
            }
        }
        public int? IdCategory { get; set; }
        public string? Category { get; set; }
        public int? Total { get; set; }
        public string? Title { get; set; }
        public DateTime? DateCreate { get; set; }
        public int? IdUser { get; set; }
        public string? UserName { get; set; }
        public string? PictureTitle { get; set; }
        public string? Name { get; set; }
        public string? Slug
        {
            get; set;
        }
        public int? Click { get; set; }
    }
    public class GetListVideoNewsParam
    {
        public int? UserId { get; set; }
        public int? IdCategory { get; set; }
        public byte? Status { get; set; }
        public string? TextSearch { get; set; }

        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }
    public class EditVideoNewsDto
    {
        public int Id { get; set; }
        public string Video { get; set; } = null!;
        public int? IdCategory { get; set; }
        public string? Title { get; set; }
        public string? Descriptions { get; set; }

        public int? IdUser { get; set; }
        public string? PictureTitle { get; set; }
        public string? Name { get; set; }
        public decimal? Lon { get; set; }
        public decimal? Lat { get; set; }
    }
    public class VideoNewsDetailDto
    {
        public int? Id { get; set; }
        public string Video { get; set; } = null!;
        public int? IdCategory { get; set; }
        public int? IdUser { get; set; }
        public int? Status { get; set; }
        public string? Title { get; set; } = null!;
        public string Category { get; set; }
        public string? Descriptions { get; set; }
        public string? PictureTitle { get; set; }
        public int? LangId { get; set; }
        public string? Language { get; set; }
        public string? Slug
        {
            get
            {
                return Title.Replace(" ", "-").Replace(",", string.Empty)
                        .Replace("/", "-").Replace("--", string.Empty)
                        .Replace("\"", string.Empty).Replace("\'", string.Empty)
                        .Replace("(", string.Empty).Replace(")", string.Empty)
                        .Replace("*", string.Empty).Replace("%", string.Empty)
                        .Replace("&", "-").Replace("@", string.Empty).ToLower();
            }
        }
        public string? UserName { get; set; }
        public DateTime? DateCreate { get; set; }
        public string? Avatar { get; set; }
        public int? Click { get; set; }
        public int QuatityComment { get; set; }
        public string? Lon { get; set; }
        public string? Lat { get; set; }
    }
    public class VideoModel
    {
        public string Video { get; set; } = null!;
        public int? IdCategory { get; set; }
        public int? LangId { get; set; }
        public string? Title { get; set; }
        public DateTime? DateCreate { get; set; }
        public int? IdUser { get; set; }
        public int? Status { get; set; }
        public string? PictureTitle { get; set; }
        public string? Descriptions { get; set; }
        public decimal? Lon { get; set; }
        public decimal? Lat { get; set; }

    }
    public class DataSupportNews
    {
        public List<NewsCategory> ListCategory { get; set; }
        public List<Language> ListLanguage { get; set; }
        public List<TopicNews> ListTopic { get; set; }
    }
    public class NewsTopicTotal
    {
        public int Id { get; set; }
        public string Topic { get; set; }
        public string Slug { get; set; }
        public int Total { get; set; }
    }

    public class NewsCategoryTotal
    {
        public int Id { get; set; }
        public string Category { get; set; }
        public string? CategoryEng { get; set; }
        public string? CategoryChi { get; set; }
        public string Slug { get; set; }
        public int Total { get; set; }
    }

    public class NewRelateDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? PictureTitle { get; set; }
        public DateTime? DateCreate { get; set; }
        public UserDataDto? UserData { get; set; }
    }


    public class NewByIdCategoryDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? PictureTitle { get; set; }
        public DateTime? DateCreate { get; set; }
        public int? IdCategory { get; set; }
        public int? IdUser { get; set; }
        public int? IdDomain { get; set; }
        public byte? Status { get; set; }
        public string? Content { get; set; }
        public string Category { get; set; }
        public string? Description { get; set; }
        public int? LangId { get; set; }
        public string? Language { get; set; }
        public List<int>? IdTopics { get; set; }
        public List<string>? Topics { get; set; }
        public string Slug { get; set; }
        public string? Tag { get; set; }
        public UserDataDto? UserData { get; set; }

    }


}
