using GoStay.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoStay.DataDto.News
{
    public class NewsDataDto
    {
        public int? Id { get; set; }
        public int? IdCategory { get; set; }
        public int? IdUser { get; set; }
        public string? Keysearch { get; set; }
        public string? Title { get; set; } = null!;
        public string? Description { get; set; }
        public int? LangId { get; set; }
        public int? IdDomain { get; set; }
        public string? Slug { get; set; }
        public string? Content { get; set; }

        public byte Status { get; set; }
        public DateTime? DateCreate { get; set; }
        public DateTime DateEdit { get; set; }
        public int Total { get; set; }

        public string? PictureTitle { get; set; }
        public string? Url { get; set; }

        public List<TopicNewsDataDto>? Topics { get; set; }
        public List<int>? TopicIds { get; set; }
        public List<string>? TopicValues
        {
            get; set;
        }
        public UserDataDto? UserData { get; set; }
        public NewsCategoryDataDto? Category { get; set; }
    }
    public class NewsCategoryDataDto
    {
        public int Id { get; set; }
        public string Category { get; set; } = null!;
        public string? CategoryEng { get; set; }
        public string? CategoryChi { get; set; }
    }
    public class TopicNewsDataDto
    {
        public int Id { get; set; }
        public string? Topic { get; set; }
    }
    public class UserDataDto
    {
        public int UserId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? UserName { get; set; }
    }
    public class NewsParamDto
    {
        public NewsDataDto News { get; set; } = new NewsDataDto();
        public List<NewsCategoryDataDto> Categories { get; set; } = new List<NewsCategoryDataDto>();
        public List<TopicNewsDataDto> Topics { get; set; } = new List<TopicNewsDataDto>();
    }
    public class UpdateStatusNewsParam
    {
        public int Id { get; set; }
        public byte Status { get; set; }
        public int IdUser { get; set; }
    }

    public class NewsAdminData
    {
        public List<NewsDataDto> ListNews { get; set; }
        public List<NewsDataDto>? ListNewsTop { get; set; }
        public int Total { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public int? IdDomain { get; set; }
    }


}
