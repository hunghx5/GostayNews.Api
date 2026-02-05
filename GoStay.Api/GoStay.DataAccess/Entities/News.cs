using System;
using System.Collections.Generic;

namespace GoStay.DataAccess.Entities
{
    public partial class News
    {
        public News()
        {
            CommentNews = new HashSet<CommentNews>();
            NewsTopics = new HashSet<NewsTopic>();
            Pictures = new HashSet<Picture>();
        }

        public int Id { get; set; }
        public int IdCategory { get; set; }
        public int IdUser { get; set; }
        public DateTime DateCreate { get; set; }
        public DateTime DateEdit { get; set; }
        public byte Status { get; set; }
        public string? Keysearch { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public string? Content { get; set; }
        public int Deleted { get; set; }
        public string? PictureTitle { get; set; }
        public int LangId { get; set; }
        public int? Click { get; set; }
        public string? Tag { get; set; }
        public int? Iddomain { get; set; }
        public string? Url { get; set; }

        public virtual NewsCategory IdCategoryNavigation { get; set; } = null!;
        public virtual User IdUserNavigation { get; set; } = null!;
        public virtual Domain? IddomainNavigation { get; set; }
        public virtual Language Lang { get; set; } = null!;
        public virtual ICollection<CommentNews> CommentNews { get; set; }
        public virtual ICollection<NewsTopic> NewsTopics { get; set; }
        public virtual ICollection<Picture> Pictures { get; set; }
    }
}
