using AutoMapper;
using GoStay.Common;
using GoStay.Common.Configuration;
using GoStay.Common.Extention;
using GoStay.Common.Helpers;
using GoStay.Data.Base;
using GoStay.DataAccess.Entities;
using GoStay.DataAccess.Interface;
using GoStay.DataAccess.UnitOfWork;
using GoStay.DataDto.News;
using GoStay.Repository.Repositories;
using GoStay.Web.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using Microsoft.IdentityModel.Tokens;
using System.Net.WebSockets;
using System.Security.Cryptography.X509Certificates;
using ErrorCodeMessage = GoStay.Data.Base.ErrorCodeMessage;
using ResponseBase = GoStay.Data.Base.ResponseBase;

namespace GoStay.Services.Newss
{
    public partial class NewsService : INewsService
    {
        private readonly ICommonRepository<News> _newsRepository;
        private readonly ICommonRepository<User> _userRepository;
        private readonly ICommonRepository<Language> _languageRepository;

        private readonly ICommonRepository<NewsCategory> _newsCategoryRepository;
        private readonly ICommonRepository<NewsTopic> _newsTopicRepository;
        private readonly ICommonRepository<TopicNews> _topicRepository;
        private readonly ICommonRepository<VideoNews> _videoRepository;
        private readonly ICommonRepository<CommentNews> _commentNewsRepo;
        private readonly ICommonRepository<CommentVideo> _commentVideoRepo;
        private readonly ICommonRepository<Hotel> _hotelRepo;
        private readonly ICommonRepository<HotelRoom> _roomRepo;



        private readonly IMapper _mapper;
        private readonly ICommonRepository<Picture> _pictureRepository;
        private readonly ICommonUoW _commonUoW;

        public NewsService(ICommonRepository<News> newsRepository, IMapper mapper, ICommonUoW commonUoW,
            ICommonRepository<Picture> pictureRepository, ICommonRepository<NewsCategory> newsCategoryRepository, ICommonRepository<User> userRepository
            , ICommonRepository<NewsTopic> newsTopicRepository, ICommonRepository<TopicNews> topicRepository, ICommonRepository<VideoNews> videoRepository,
            ICommonRepository<Language> languageRepository, ICommonRepository<CommentNews> commentNewsRepo, ICommonRepository<CommentVideo> commentVideoRepo
            , ICommonRepository<Hotel> hotelRepo, ICommonRepository<HotelRoom> roomRepo)
        {
            _mapper = mapper;
            _pictureRepository = pictureRepository;
            _newsCategoryRepository = newsCategoryRepository;
            _newsRepository = newsRepository;
            _commonUoW = commonUoW;
            _userRepository = userRepository;
            _newsTopicRepository = newsTopicRepository;
            _topicRepository = topicRepository;
            _videoRepository = videoRepository;
            _languageRepository = languageRepository;
            _commentNewsRepo = commentNewsRepo;
            _commentVideoRepo = commentVideoRepo;
            _hotelRepo = hotelRepo;
            _roomRepo = roomRepo;
        }
        public ResponseBase GetNewsDefault(int idUser, int idNews)
        {
            ResponseBase response = new ResponseBase();
            try
            {
                var data = new NewsParamDto();
                var idRecord = 0;
                var exception = "";
                var categories = _newsCategoryRepository.FindAll(x => x.Iddomain == AppConfigs.IdDomain).Select(x => new NewsCategoryDataDto
                {
                    Id = x.Id,
                    Category = x.Category,
                    CategoryChi = x.CategoryChi,
                    CategoryEng = x.CategoryEng,
                }).ToList();
                data.Categories = categories;
                var topics = _topicRepository.FindAll(x => x.Iddomain == AppConfigs.IdDomain).Select(x => new TopicNewsDataDto
                {
                    Id = x.Id,
                    Topic = x.Topic,
                }).ToList();
                data.Topics = topics;
                var tempRecord = _newsRepository.FindAll(x => x.IdUser == idUser && x.Status == 0 && x.Iddomain == AppConfigs.IdDomain).AsNoTracking();

                if (tempRecord.Any())
                {
                    var tempRecordIds = tempRecord.Select(x => x.Id);
                    if (tempRecord.Count() > 1)
                    {

                        //topics
                        var oldTopics = _newsTopicRepository.FindAll(x => tempRecordIds.Contains(x.IdNews));
                        if (oldTopics.Any())
                        {
                            try
                            {
                                _commonUoW.BeginTransaction();
                                _newsTopicRepository.RemoveMultiple(oldTopics);
                                _commonUoW.Commit();
                            }
                            catch
                            {
                                exception += "Remove old topic fail";
                            }
                        }
                        //pictures
                        var oldPicture = _pictureRepository.FindAll(x => tempRecordIds.Contains((x.NewsId != null) ? (int)x.NewsId : 0));

                        if (oldPicture.Any())
                        {
                            //FileHelper.DeleteFolder($"/uploads/SGOland/ctv/{newsEntity.IdUser}/news/{Id}");
                            try
                            {
                                _commonUoW.BeginTransaction();
                                _pictureRepository.RemoveMultiple(oldPicture);
                                _commonUoW.Commit();
                            }
                            catch
                            {
                                exception += "Remove old pictures fail";
                            }
                        }
                        try
                        {
                            idRecord = 0;
                            _commonUoW.BeginTransaction();
                            _newsRepository.RemoveMultiple(tempRecord);
                            _commonUoW.Commit();
                        }
                        catch
                        {
                            exception += "Remove old news fail";
                        }
                    }
                    else
                    {
                        idRecord = tempRecord.SingleOrDefault().Id;
                    }
                }
                if (idNews > 0)
                {
                    idRecord = idNews;
                }
                if (idRecord == 0)
                {
                    var newRecord = new News()
                    {
                        IdCategory = categories.Min(x => x.Id),
                        IdUser = idUser,
                        DateCreate = DateTime.Now,
                        DateEdit = DateTime.Now,
                        Status = 0,
                        Keysearch = "",
                        Title = "",
                        Description = "",
                        Content = "",
                        Deleted = 0,
                        LangId = 1,
                        Click = 0,
                        Tag = "",
                        Iddomain = AppConfigs.IdDomain,

                    };

                    try
                    {
                        _commonUoW.BeginTransaction();
                        _newsRepository.Insert(newRecord);
                        _commonUoW.Commit();
                        idRecord = newRecord.Id;
                    }
                    catch
                    {
                        exception += "Add temp news fail";
                    }
                }

                var news = _newsRepository.FindAll(x => x.Id == idRecord)
                                            .Include(x => x.NewsTopics)
                                            .Include(x => x.IdUserNavigation)
                                            .SingleOrDefault()
                                            ;
                data.News = new NewsDataDto()
                {
                    Id = news.Id,
                    IdCategory = news.IdCategory,
                    IdUser = news.IdUser,
                    Keysearch = news.Keysearch,
                    Title = news.Title,
                    Description = news.Description,
                    LangId = news.LangId,
                    IdDomain = news.Iddomain,
                    PictureTitle = (idNews > 0) ? news.PictureTitle : "",
                    DateCreate = news.DateCreate,
                    Topics = (idNews > 0) ? topics.Where(x => news.NewsTopics.Select(y => y.IdNewsTopic).Contains(x.Id)).ToList() : new List<TopicNewsDataDto>(),
                    TopicIds = (idNews > 0) ? topics.Where(x => news.NewsTopics.Select(y => y.IdNewsTopic).Contains(x.Id)).Select(x => x.Id).ToList() : new List<int>(),
                    TopicValues = (idNews > 0) ? topics.Where(x => news.NewsTopics.Select(y => y.IdNewsTopic).Contains(x.Id)).Select(x => x.Topic).ToList() : new List<string>(),
                    UserData = new UserDataDto()
                    {
                        UserId = news.IdUser,
                        FirstName = news.IdUserNavigation.FirstName,
                        LastName = news.IdUserNavigation.LastName,
                    },
                    Content = news.Content,
                    Status = news.Status,
                    Category = categories.SingleOrDefault(x => x.Id == news.IdCategory),
                    Slug = SlugHelper.GenerateSlug(VietnameseNormalizer.NormalizeVietnamese(news.Title))
                };

                response.Code = ErrorCodeMessage.Success.Key;
                response.Message = ErrorCodeMessage.Success.Value;
                response.Data = data;
                return response;

            }
            catch (Exception e)
            {
                _commonUoW.RollBack();
                response.Code = ErrorCodeMessage.Exception.Key;
                response.Message = e.Message;
                return response;
            }

        }
        public ResponseBase GetListNews2(GetListNewsParam param)
        {

            ResponseBase response = new ResponseBase();
            try
            {


                var data = new NewsAdminData()

                {
                    ListNews = new List<NewsDataDto>(),
                };

                if (AppConfigs.AdminIds.Contains(param.UserId ?? 0))
                {
                    param.UserId = 0;
                }
                //var listNews = _newsRepository.FindAll(x => x.Deleted != 1
                //                                        && ((param.UserId > 0) ? x.IdUser == param.UserId : x.Id > 0)
                //                                        && ((param.Status > 0) ? x.Status == param.Status : x.Status > 2)
                //                                        && ((param.IdCategory > 0) ? x.IdCategory == param.IdCategory : x.IdCategory > 0)
                //                                        && ((param.IdTopic > 0) ? x.NewsTopics.Select(y => y.IdNewsTopic).Contains((int)param.IdTopic) : x.Id > 0)
                //                                        && ((param.IdDomain > 0) ? x.Iddomain == param.IdDomain : x.Iddomain == AppConfigs.IdDomain)
                //                                        && ((param.TextSearch != null) ? x.Keysearch.Contains(param.TextSearch) : x.Id > 0))
                //                                .Include(x => x.IdCategoryNavigation)
                //                                .Include(x => x.NewsTopics).ThenInclude(x => x.IdNewsTopicNavigation)
                //                                .Include(x => x.IdUserNavigation).OrderBy(x => x.Status).ThenByDescending(x => x.DateEdit).AsNoTracking();


                var query = _newsRepository.FindAll(x => x.Deleted != 1
                                    && ((param.UserId > 0) ? x.IdUser == param.UserId : x.Id > 0)
                                    && ((param.IdCategory > 0) ? x.IdCategory == param.IdCategory : x.IdCategory > 0)
                                    && ((param.IdTopic > 0) ? x.NewsTopics.Any(y => y.IdNewsTopic == param.IdTopic) : x.Id > 0)
                                    && ((param.IdDomain > 0) ? x.Iddomain == param.IdDomain : x.Iddomain == AppConfigs.IdDomain)
                                    && ((param.TextSearch != null) ? x.Keysearch.Contains(param.TextSearch) : x.Id > 0))
                            .Include(x => x.IdCategoryNavigation)
                            .Include(x => x.NewsTopics).ThenInclude(x => x.IdNewsTopicNavigation)
                            .Include(x => x.IdUserNavigation)
                            .OrderBy(x => x.Status)
                            .ThenByDescending(x => x.DateEdit)
                            .AsNoTracking();

                // Điều kiện Status thay đổi tùy theo Style
                query = (param.Style == 1) ? query.Where(x => (param.Status > 0) ? x.Status == param.Status : x.Status > 2)
                                            : query.Where(x => (param.Status > 0) ? x.Status == param.Status : x.Status > 0);

                var listNews = query.ToList();


                var count = listNews.Count();
                var pageSize = param.PageSize;
                var pageIndex = param.PageIndex;
                if (pageIndex > ((count - 1) / pageSize + 1))
                {
                    response.Code = ErrorCodeMessage.OutRange.Key;
                    response.Message = ErrorCodeMessage.OutRange.Value;
                    response.Data = listNews;
                    return response;
                }
                data.ListNews = listNews.Select(x => new NewsDataDto
                {
                    Id = x.Id,
                    IdCategory = x.IdCategory,
                    IdUser = x.IdUser,
                    Keysearch = x.Keysearch,
                    Title = x.Title,
                    Description = x.Description,
                    LangId = x.LangId,
                    PictureTitle = x.PictureTitle,
                    DateCreate = x.DateCreate,
                    IdDomain = x.Iddomain,
                    Status = x.Status,
                    Content = x.Content,
                    DateEdit = x.DateEdit,
                    Total = listNews.Count(),

                    Topics = x.NewsTopics.Select(z => z.IdNewsTopicNavigation).Select(y => new TopicNewsDataDto
                    {
                        Id = y.Id,
                        Topic = y.Topic,
                    }).ToList(),
                    UserData = new UserDataDto()
                    {
                        UserId = x.IdUser,
                        FirstName = x.IdUserNavigation.FirstName,
                        LastName = x.IdUserNavigation.LastName,
                        UserName = x.IdUserNavigation.UserName
                    },
                    Category = new NewsCategoryDataDto
                    {
                        Id = x.IdCategory,
                        Category = x.IdCategoryNavigation.Category,
                        CategoryChi = x.IdCategoryNavigation.CategoryChi,
                        CategoryEng = x.IdCategoryNavigation.CategoryEng,
                    },
                    Slug = SlugHelper.GenerateSlug(VietnameseNormalizer.NormalizeVietnamese(x.Title))
                }).Skip(pageSize * (pageIndex - 1)).Take(pageSize).OrderByDescending(x => x.DateEdit).ToList();

                response.Code = ErrorCodeMessage.Success.Key;
                response.Message = ErrorCodeMessage.Success.Value;
                response.Data = data;
                return response;

            }
            catch (Exception e)
            {
                _commonUoW.RollBack();
                response.Code = ErrorCodeMessage.Exception.Key;
                response.Message = e.Message;
                return response;
            }

        }
        public ResponseBase GetListNews(GetListNewsParam param)
        {

            ResponseBase response = new ResponseBase();
            try
            {

                var listNews = NewsRepository.SearchListNews(param);
                listNews.ForEach(x => x.Slug = SlugHelper.GenerateSlug(VietnameseNormalizer.NormalizeVietnamese(x.Title)));
                response.Code = ErrorCodeMessage.Success.Key;
                response.Message = ErrorCodeMessage.Success.Value;
                response.Data = listNews;
                return response;

            }
            catch (Exception e)
            {
                _commonUoW.RollBack();
                response.Code = ErrorCodeMessage.Exception.Key;
                response.Message = e.Message;
                return response;
            }

        }
        public ResponseBase SubmitNews(NewsDataDto news)
        {

            ResponseBase response = new ResponseBase();
            try
            {
                var topics = _topicRepository.FindAll(x => x.Iddomain == AppConfigs.IdDomain).Select(x => new TopicNewsDataDto
                {
                    Id = x.Id,
                    Topic = x.Topic,
                }).ToList();
                _commonUoW.BeginTransaction();
                var newsEntity = _newsRepository.GetById(news.Id);
                if (newsEntity == null)
                {
                    response.Code = ErrorCodeMessage.NotFound.Key;
                    response.Message = ErrorCodeMessage.NotFound.Value;
                    response.Data = "Not found";
                    return response;
                }
                newsEntity.IdCategory = (int)news.IdCategory;
                newsEntity.Title = news.Title;
                newsEntity.Description = news.Description;
                newsEntity.Keysearch = news.Title.RemoveUnicode2().Replace(" ", string.Empty).ToLower();
                newsEntity.DateEdit = DateTime.UtcNow;
                newsEntity.LangId = (int)news.LangId;
                newsEntity.Iddomain = AppConfigs.IdDomain;
                newsEntity.Content = news.Content;
                newsEntity.Status = news.Status;
                if (news.TopicValues != null && news.TopicValues.Any())
                {
                    news.TopicIds = topics.Where(x => news.TopicValues.Contains(x.Topic)).Select(x => x.Id).ToList();
                }
                _newsRepository.Update(newsEntity);
                _commonUoW.Commit();
                _commonUoW.BeginTransaction();
                var oldtopic = _newsTopicRepository.FindAll(x => x.IdNews == news.Id);
                if (oldtopic.Any())
                {
                    _newsTopicRepository.RemoveMultiple(oldtopic);
                    _commonUoW.Commit();
                    _commonUoW.BeginTransaction();

                }

                if (news.TopicIds.Any())
                {
                    var newTopic = news.TopicIds.Select(x => new NewsTopic
                    {
                        IdNews = newsEntity.Id,
                        IdNewsTopic = x
                    });
                    _newsTopicRepository.InsertMultiple(newTopic);
                }
                _commonUoW.Commit();

                response.Code = ErrorCodeMessage.Success.Key;
                response.Message = ErrorCodeMessage.Success.Value;
                response.Data = newsEntity.Id;
                return response;

            }
            catch (Exception e)
            {
                _commonUoW.RollBack();
                response.Code = ErrorCodeMessage.Exception.Key;
                response.Message = e.Message;
                return response;
            }

        }
        public ResponseBase UpdateStatusNews(UpdateStatusNewsParam param)
        {

            ResponseBase response = new ResponseBase();
            try
            {
                var data = _newsRepository.FindAll(x => x.Id == param.Id).SingleOrDefault();
                if (data == null)
                {
                    response.Code = ErrorCodeMessage.NotFound.Key;
                    response.Message = ErrorCodeMessage.NotFound.Value;
                    response.Data = 0;
                    return response;
                }
                data.Status = param.Status;
                _commonUoW.BeginTransaction();

                _commonUoW.Commit();

                response.Code = ErrorCodeMessage.Success.Key;
                response.Message = ErrorCodeMessage.Success.Value;
                response.Data = 3;
                return response;

            }
            catch (Exception e)
            {
                _commonUoW.RollBack();
                response.Code = ErrorCodeMessage.Exception.Key;
                response.Message = e.Message;
                return response;
            }

        }
        public ResponseBase GetNewsForHomePage(int latestQuantity, int categoryQuantity, int hotQuantlty, DateTime dateStart,
            DateTime dateEnd, int idcategory, int idtopic)
        {

            ResponseBase response = new ResponseBase();
            var data = new NewsTabHome()
            {
                HotNews = new List<NewsHomeData>(),
                LatestNews = new List<NewsHomeData>(),
                NewsForCategories = new Dictionary<int, List<NewsHomeData>>()
            };
            try
            {
                var listId = new List<int>();
                if (idtopic > 0)
                {
                    listId = _newsTopicRepository.FindAll(x => x.IdNewsTopic == idtopic).Select(x => x.IdNews).ToList();
                }
                var allnews = _newsRepository.FindAll(x => x.Status == (int)NewsStatus.Accepted && x.Deleted != 1
                                                         && x.Iddomain == AppConfigs.IdDomain);

                //&& ((idcategory > 0) ? x.IdCategory == idcategory : true)
                //&& ((idtopic > 0) ? listId.Contains(x.Id) : true))
                //.Include(x => x.IdCategoryNavigation)
                //.Include(x => x.IdUserNavigation)
                //.Include(x => x.NewsTopics).ThenInclude(y => y.IdNewsTopicNavigation)
                //.AsNoTracking()
                ;
                var list = allnews.Select(x => new NewsHomeData
                {
                    Id = x.Id,
                    Status = x.Status,
                    Title = x.Title,
                    DateCreate = x.DateEdit,
                    IdCategory = x.IdCategory,
                    IdTopics = x.NewsTopics.Select(y => y.IdNewsTopic).ToList(),
                    PictureTitle = x.PictureTitle,
                    Description = x.Description,
                    Category = x.IdCategoryNavigation.Category,
                    CategoryChi = x.IdCategoryNavigation.CategoryChi,
                    CategoryEng = x.IdCategoryNavigation.CategoryEng,
                    UserName = x.IdUserNavigation.UserName,
                    CommentCount = x.CommentNews.Where(x => x.Published == true).Count(),
                    Click = x.Click ?? 0,
                    Slug = SlugHelper.GenerateSlug(VietnameseNormalizer.NormalizeVietnamese(x.Title ?? string.Empty))

                });

                data.HotNews = list.OrderByDescending(x => x.DateCreate).Take(hotQuantlty).ToList();
                var temp = list.Where(x => ((idcategory > 0) ? x.IdCategory == idcategory : true)
                                        && ((idtopic > 0) ? listId.Contains(x.Id) : true));
                var total = temp.Count();
                data.LatestNews = temp.OrderByDescending(x => x.DateCreate).Take(latestQuantity).ToList();

                data.LatestNews.ForEach(x => x.Total = total);
                data.LatestNews.ForEach(x => x.PageNum = (latestQuantity / 10));
                data.Categories = _newsCategoryRepository.FindAll(x => x.Iddomain == AppConfigs.IdDomain)
                                    .Select(x => new CategoryNews
                                    {
                                        Id = x.Id,
                                        Name = x.Category
                                    }).ToList();
                var temp2 = data.Categories.Select(x => x.Id);

                foreach (var cate in temp2)
                {
                    var news = temp.Where(x => x.IdCategory == cate).OrderByDescending(x => x.DateCreate).Take(categoryQuantity).ToList();
                    data.NewsForCategories.Add(cate, news);
                }

                response.Code = ErrorCodeMessage.Success.Key;
                response.Message = ErrorCodeMessage.Success.Value;
                response.Data = data;
                return response;

            }
            catch (Exception e)
            {
                _commonUoW.RollBack();
                response.Code = ErrorCodeMessage.Exception.Key;
                response.Message = e.Message;
                return response;
            }

        }

        public ResponseBase GetListNewsHomePage()
        {

            ResponseBase response = new ResponseBase();
            try
            {

                var dicNews = new Dictionary<int, List<NewSearchOutDto>>();
                var categories = _newsCategoryRepository.FindAll().Select(x => x.Id);

                foreach (var Id in categories)
                {
                    var data = NewsRepository.SearchListNews(new GetListNewsParam { IdCategory = Id, IdDomain = 1, PageIndex = 1, PageSize = 10 });
                    data.ForEach(x => x.Slug = SlugHelper.GenerateSlug(VietnameseNormalizer.NormalizeVietnamese(x.Title)));
                    dicNews.Add(Id, data);
                }
                response.Code = ErrorCodeMessage.Success.Key;
                response.Message = ErrorCodeMessage.Success.Value;
                response.Data = dicNews;
                return response;

            }
            catch (Exception e)
            {
                _commonUoW.RollBack();
                response.Code = ErrorCodeMessage.Exception.Key;
                response.Message = e.Message;
                return response;
            }

        }
        public ResponseBase GetListTopNewsByCategory(int? IdCategory, int? IdTopic)
        {

            ResponseBase response = new ResponseBase();
            try
            {
                var data = NewsRepository.GetListTopNews(IdCategory, IdTopic);
                data.ForEach(x => x.Slug = SlugHelper.GenerateSlug(VietnameseNormalizer.NormalizeVietnamese(x.Title)));

                response.Code = ErrorCodeMessage.Success.Key;
                response.Message = ErrorCodeMessage.Success.Value;
                response.Data = data;
                return response;

            }
            catch (Exception e)
            {
                _commonUoW.RollBack();
                response.Code = ErrorCodeMessage.Exception.Key;
                response.Message = e.Message;
                return response;
            }

        }
        public ResponseBase GetNews(int Id, int? domain)
        {

            ResponseBase response = new ResponseBase();
            try
            {
                var news = _newsRepository.FindAll(x => x.Id == Id && (domain == null || x.Iddomain == domain) && x.Deleted == 0 && x.Status == 3)
                            .Include(x => x.IdCategoryNavigation)
                            .Include(x => x.IdUserNavigation)
                            .Include(x => x.NewsTopics).ThenInclude(y => y.IdNewsTopicNavigation)
                            .Include(x => x.Lang)
                            .SingleOrDefault();
                if (news == null)
                {
                    response.Code = ErrorCodeMessage.NotFound.Key;
                    response.Message = ErrorCodeMessage.NotFound.Value;
                    return response;
                }
                var newsDetail = new NewsDetailDto();

                newsDetail.Id = news.Id;
                newsDetail.IdCategory = news.IdCategory;
                newsDetail.Status = news.Status;
                newsDetail.IdUser = news.IdUser;
                newsDetail.Title = news.Title;
                newsDetail.Content = news.Content;
                newsDetail.PictureTitle = news.PictureTitle;
                newsDetail.Description = news.Description;
                newsDetail.Category = news.IdCategoryNavigation.Category;
                newsDetail.LangId = news.LangId;
                newsDetail.DateCreate = news.DateCreate;
                newsDetail.Language = news.Lang.Language1;
                newsDetail.IdDomain = news.Iddomain;
                newsDetail.IdTopics = news.NewsTopics.Select(x => x.IdNewsTopic).ToList();

                newsDetail.Topics = news.NewsTopics.Select(x => x.IdNewsTopicNavigation.Topic).ToList();
                newsDetail.Tag = news.Tag;
                newsDetail.UserName = news.IdUserNavigation.UserName;
                newsDetail.Slug = SlugHelper.GenerateSlug(VietnameseNormalizer.NormalizeVietnamese(news.Title));
                var quatityComment = _commentNewsRepo.FindAll(x => x.NewsId == news.Id && x.Published == true && x.Deleted == false).Count();
                newsDetail.QuatityComment = quatityComment;
                var newserelate = _newsRepository.FindAll(x => x.Iddomain == 3 && x.Deleted == 0 && x.Id != Id && x.Status == 3).Include(x => x.IdUserNavigation).ToList();

                var newcategory = _newsRepository.FindAll(x => x.IdCategory == news.IdCategory && x.Deleted == 0 && x.Id != Id && x.Status == 3).Include(x => x.IdUserNavigation).ToList();

                var d = newcategory.OrderByDescending(x => x.DateCreate).Take(10);
                var c = newserelate.OrderByDescending(x => x.DateCreate).Take(10);

                if (c != null && c.Any())
                {
                    newsDetail.NewRelates = c.Select(x => new NewRelateDto
                    {

                        Id = x.Id,
                        Title = x.Title,

                        PictureTitle = x.PictureTitle,
                        DateCreate = x.DateCreate,
                        UserData = new UserDataDto()
                        {
                            UserId = x.IdUser,
                            FirstName = x.IdUserNavigation.FirstName,
                            LastName = x.IdUserNavigation.LastName,
                            UserName = x.IdUserNavigation.UserName
                        },
                    }).ToList();
                }

                if (d != null && d.Any())
                {
                    newsDetail.NewCategory = d.Select(x => new NewByIdCategoryDto
                    {

                        Id = x.Id,
                        Title = x.Title,
                        PictureTitle = x.PictureTitle,
                        DateCreate = x.DateCreate,
                        Description = x.Description,
                        Content = x.Content,
                        IdDomain = x.Iddomain,
                        IdCategory = x.IdCategory,
                        LangId = x.LangId,
                        Tag = x.Tag,
                        Slug = SlugHelper.GenerateSlug(VietnameseNormalizer.NormalizeVietnamese(x.Title)),
                        Topics = x.NewsTopics.Select(x => x.IdNewsTopicNavigation.Topic).ToList(),

                        UserData = new UserDataDto()
                        {
                            UserId = x.IdUser,
                            FirstName = x.IdUserNavigation.FirstName,
                            LastName = x.IdUserNavigation.LastName,
                            UserName = x.IdUserNavigation.UserName
                        },
                    }).ToList();
                }

                response.Code = ErrorCodeMessage.Success.Key;
                response.Message = ErrorCodeMessage.Success.Value;
                response.Data = newsDetail;
                return response;

            }
            catch (Exception e)
            {
                _commonUoW.RollBack();
                response.Code = ErrorCodeMessage.Exception.Key;
                response.Message = e.Message;
                return response;
            }

        }

        public ResponseBase AddNews(NewsDto news)
        {

            ResponseBase response = new ResponseBase();
            try
            {
                _commonUoW.BeginTransaction();
                var newsEntity = new News()
                {
                    IdCategory = (int)news.IdCategory,
                    IdUser = (int)news.IdUser,
                    Status = 1,
                    Title = news.Title,
                    Keysearch = news.Keysearch,
                    Description = news.Description,
                    DateCreate = DateTime.UtcNow,
                    DateEdit = DateTime.UtcNow,
                    Deleted = 0,
                    LangId = (int)news.LangId,
                    PictureTitle = "",
                    Click = 0,
                    Iddomain = news.IdDomain
                };

                _newsRepository.Insert(newsEntity);
                _commonUoW.Commit();

                _commonUoW.BeginTransaction();

                if (news.IdTopics.Count > 0)
                {
                    foreach (var IdTopic in news.IdTopics)
                    {
                        _newsTopicRepository.Insert(new NewsTopic() { IdNews = newsEntity.Id, IdNewsTopic = IdTopic });

                    }
                }
                _commonUoW.Commit();

                response.Code = ErrorCodeMessage.Success.Key;
                response.Message = ErrorCodeMessage.Success.Value;
                response.Data = newsEntity.Id;
                return response;

            }
            catch (Exception e)
            {
                _commonUoW.RollBack();
                response.Code = ErrorCodeMessage.Exception.Key;
                response.Message = e.Message;
                return response;
            }

        }
        public ResponseBase EditNews(NewsDto news)
        {

            ResponseBase response = new ResponseBase();
            try
            {
                _commonUoW.BeginTransaction();
                var newsEntity = _newsRepository.GetById(news.Id);
                if (newsEntity == null)
                {
                    response.Code = ErrorCodeMessage.NotFound.Key;
                    response.Message = ErrorCodeMessage.NotFound.Value;
                    response.Data = news.Id;
                    return response;
                }
                newsEntity.IdCategory = (int)news.IdCategory;
                newsEntity.IdUser = (int)news.IdUser;
                newsEntity.Title = news.Title;
                newsEntity.Description = news.Description;
                newsEntity.Keysearch = news.Keysearch;
                newsEntity.DateEdit = DateTime.UtcNow;
                newsEntity.LangId = (int)news.LangId;
                newsEntity.Iddomain = (int)news.IdDomain;



                _newsRepository.Update(newsEntity);
                _commonUoW.Commit();
                _commonUoW.BeginTransaction();

                _newsTopicRepository.RemoveMultiple(_newsTopicRepository.FindAll(x => x.IdNews == news.Id));

                if (news.IdTopics.Count > 0)
                {
                    foreach (var IdTopic in news.IdTopics)
                    {
                        _newsTopicRepository.Insert(new NewsTopic() { IdNews = newsEntity.Id, IdNewsTopic = IdTopic });

                    }
                }
                _commonUoW.Commit();

                response.Code = ErrorCodeMessage.Success.Key;
                response.Message = ErrorCodeMessage.Success.Value;
                response.Data = newsEntity.Id;
                return response;

            }
            catch (Exception e)
            {
                _commonUoW.RollBack();
                response.Code = ErrorCodeMessage.Exception.Key;
                response.Message = e.Message;
                return response;
            }

        }

        public ResponseBase EditContentNews(string content, int NewsId)
        {

            ResponseBase response = new ResponseBase();
            try
            {
                _commonUoW.BeginTransaction();
                var newsEntity = _newsRepository.GetById(NewsId);
                if (newsEntity == null)
                {
                    response.Code = ErrorCodeMessage.NotFound.Key;
                    response.Message = ErrorCodeMessage.NotFound.Value;
                    response.Data = NewsId;
                    return response;
                }
                newsEntity.Content = content;
                newsEntity.DateEdit = DateTime.UtcNow;

                _newsRepository.Update(newsEntity);
                _commonUoW.Commit();
                response.Code = ErrorCodeMessage.Success.Key;
                response.Message = ErrorCodeMessage.Success.Value;
                response.Data = newsEntity.Id;
                return response;

            }
            catch (Exception e)
            {
                _commonUoW.RollBack();
                response.Code = ErrorCodeMessage.Exception.Key;
                response.Message = e.Message;
                return response;
            }

        }
        public ResponseBase TopicDetailNew(int max)
        {
            ResponseBase response = new ResponseBase();
            try
            {
                var data = _newsRepository.FindAll().OrderByDescending(x => x.DateCreate).Take(max)
                    .Select(x => new NewSearchOutDto
                    {
                        Id = x.Id,
                        Status = x.Status,
                        Title = x.Title,
                        Content = x.Content == null ? null : x.Content,
                        DateCreate = x.DateCreate,
                        IdCategory = x.IdCategory,
                        PictureTitle = x.PictureTitle,
                        Description = x.Description,
                        LangId = x.LangId,
                        Tag = x.Tag == null ? null : x.Tag,
                        Click = x.Click == null ? 0 : (int)x.Click,
                        Category = x.IdCategoryNavigation.Category,
                        UserName = x.IdUserNavigation.UserName == null ? null : x.IdUserNavigation.UserName,
                        Language = x.Lang.Language1

                    })
                    .ToList();
                data.ForEach(x => x.Slug = SlugHelper.GenerateSlug(VietnameseNormalizer.NormalizeVietnamese(x.Title)));
                response.Code = ErrorCodeMessage.Success.Key;
                response.Message = ErrorCodeMessage.Success.Value;
                response.Data = data;
                return response;

            }
            catch (Exception e)
            {
                response.Code = ErrorCodeMessage.Exception.Key;
                response.Message = e.Message;
                return response;
            }

        }
        public ResponseBase TopicDetailNew(int max, int domain)
        {
            ResponseBase response = new ResponseBase();
            try
            {
                var data = _newsRepository.FindAll(x => x.Iddomain == domain).OrderByDescending(x => x.DateCreate).Take(max)
                    .Select(x => new NewSearchOutDto
                    {
                        Id = x.Id,
                        Status = x.Status,
                        Title = x.Title,
                        Content = x.Content == null ? null : x.Content,
                        DateCreate = x.DateCreate,
                        IdCategory = x.IdCategory,
                        PictureTitle = x.PictureTitle,
                        Description = x.Description,
                        LangId = x.LangId,
                        Tag = x.Tag == null ? null : x.Tag,
                        Click = x.Click == null ? 0 : (int)x.Click,
                        Category = x.IdCategoryNavigation.Category,
                        UserName = x.IdUserNavigation.UserName == null ? null : x.IdUserNavigation.UserName,
                        Language = x.Lang.Language1

                    })
                    .ToList();
                data.ForEach(x => x.Slug = SlugHelper.GenerateSlug(VietnameseNormalizer.NormalizeVietnamese(x.Title)));
                response.Code = ErrorCodeMessage.Success.Key;
                response.Message = ErrorCodeMessage.Success.Value;
                response.Data = data;
                return response;

            }
            catch (Exception e)
            {
                response.Code = ErrorCodeMessage.Exception.Key;
                response.Message = e.Message;
                return response;
            }

        }
        public ResponseBase EditPictureTitleNews(string url, int NewsId)
        {

            ResponseBase response = new ResponseBase();
            try
            {
                _commonUoW.BeginTransaction();
                var newsEntity = _newsRepository.GetById(NewsId);
                if (newsEntity == null)
                {
                    response.Code = ErrorCodeMessage.NotFound.Key;
                    response.Message = ErrorCodeMessage.NotFound.Value;
                    response.Data = NewsId;
                    return response;
                }
                newsEntity.PictureTitle = url;
                newsEntity.DateEdit = DateTime.UtcNow;

                _newsRepository.Update(newsEntity);
                _commonUoW.Commit();
                response.Code = ErrorCodeMessage.Success.Key;
                response.Message = ErrorCodeMessage.Success.Value;
                response.Data = newsEntity.Id;
                return response;

            }
            catch (Exception e)
            {
                _commonUoW.RollBack();
                response.Code = ErrorCodeMessage.Exception.Key;
                response.Message = e.Message;
                return response;
            }

        }
        public ResponseBase EditClickNews(int NewsId)
        {

            ResponseBase response = new ResponseBase();
            try
            {
                _commonUoW.BeginTransaction();
                var newsEntity = _newsRepository.GetById(NewsId);
                if (newsEntity == null)
                {
                    response.Code = ErrorCodeMessage.NotFound.Key;
                    response.Message = ErrorCodeMessage.NotFound.Value;
                    response.Data = NewsId;
                    return response;
                }
                if (newsEntity.Click == null)
                {
                    newsEntity.Click = 0;
                }
                newsEntity.Click = newsEntity.Click + 1;

                _newsRepository.Update(newsEntity);
                _commonUoW.Commit();
                response.Code = ErrorCodeMessage.Success.Key;
                response.Message = ErrorCodeMessage.Success.Value;
                response.Data = newsEntity.Id;
                return response;

            }
            catch (Exception e)
            {
                _commonUoW.RollBack();
                response.Code = ErrorCodeMessage.Exception.Key;
                response.Message = e.Message;
                return response;
            }

        }
        public ResponseBase DeleteNews(int Id)
        {

            ResponseBase response = new ResponseBase();
            try
            {
                _commonUoW.BeginTransaction();
                var newsEntity = _newsRepository.GetById(Id);
                if (newsEntity == null)
                {
                    response.Code = ErrorCodeMessage.NotFound.Key;
                    response.Message = ErrorCodeMessage.NotFound.Value;
                    response.Data = "Not found obj";
                    return response;
                }
                newsEntity.Deleted = 1;
                _newsRepository.Update(newsEntity);
                var listnewstopics = _newsTopicRepository.FindAll(x => x.IdNews == newsEntity.Id);
                _newsTopicRepository.RemoveMultiple(listnewstopics);
                _commonUoW.Commit();
                response.Code = ErrorCodeMessage.Success.Key;
                response.Message = ErrorCodeMessage.Success.Value;
                response.Data = "Success";
                return response;

            }
            catch (Exception e)
            {
                _commonUoW.RollBack();
                response.Code = ErrorCodeMessage.Exception.Key;
                response.Message = e.Message;
                response.Data = "Exception";

                return response;
            }

        }
        public ResponseBase GetTagNews()
        {
            var result = new ResponseBase();
            try
            {
                var listTopic = _topicRepository.FindAll(x => x.Iddomain == 1).Select(x => new TopicNewsDto
                {
                    Id = x.Id,
                    Topic = x.Topic,
                    Iddomain = x.Iddomain,
                    Tag = x.Topic.Trim().RemoveUnicode2().ToLower().Replace(" ", "-")
                });

                result.Data = listTopic;
                return result;
            }
            catch (Exception e)
            {
                result.Code = e.HResult;
                result.Message = e.Message;
                return result;
            }
        }
        public ResponseBase GetCategoryNews()
        {
            var result = new ResponseBase();
            try
            {
                var listNews = _newsRepository.FindAll(x => x.Iddomain == 1 && x.Deleted != 1 && x.Status == (int)NewsStatus.Accepted).Select(x => x.IdCategory).ToList();
                var categories = _newsCategoryRepository.FindAll(x => x.Iddomain == 1).Select(x => new NewsCategoryDto
                {
                    Id = x.Id,
                    Category = x.Category,
                    Slug = x.Category.Trim().RemoveUnicode2().ToLower().Replace(" ", "-"),
                }).ToList();
                categories.ForEach(x => x.Total = listNews.Count(z => z == x.Id));
                result.Data = categories;
                return result;
            }
            catch (Exception e)
            {
                result.Code = e.HResult;
                result.Message = e.Message;
                return result;
            }
        }
        public ResponseBase GetNewsByTopicAndCategory(int idCategory, int idTopic, int pageIndex, int pageSize)
        {
            var result = new ResponseBase();
            try
            {
                var listId = new List<int>();
                if (idTopic > 0)
                {
                    listId = _newsTopicRepository.FindAll(x => x.IdNewsTopic == idTopic).Select(x => x.IdNews).ToList();
                }
                var listNews = _newsRepository.FindAll(x => x.Iddomain == 1 && x.Deleted != 1 && x.Status == (int)NewsStatus.Accepted
                                                         && (idCategory > 0 ? x.IdCategory == idCategory : true)
                                                         && (idTopic > 0 ? listId.Contains(x.Id) : true))
                                                        .OrderByDescending(x => x.DateCreate);


                if (listNews != null && listNews.Any())
                {
                    var total = listNews.Count();
                    //var data = listNews.Skip((pageIndex - 1) * pageSize).Take(pageSize);
                    result.Data = listNews.Skip((pageIndex - 1) * pageSize).Take(pageSize).Select(x => new NewsHomeData
                    {
                        Id = x.Id,
                        Status = x.Status,
                        Title = x.Title,
                        DateCreate = x.DateCreate,
                        IdCategory = x.IdCategory,
                        IdTopics = x.NewsTopics.Select(z => z.IdNewsTopic).ToList(),
                        PictureTitle = x.PictureTitle,
                        Description = x.Description,
                        Category = x.IdCategoryNavigation.Category,
                        CategoryEng = x.IdCategoryNavigation.CategoryEng,
                        CategoryChi = x.IdCategoryNavigation.CategoryChi,
                        UserName = x.IdUserNavigation.UserName,
                        Click = x.Click ?? 0,
                        Slug = SlugHelper.GenerateSlug(VietnameseNormalizer.NormalizeVietnamese(x.Title)),
                        Total = total,
                        CommentCount = x.CommentNews.Where(x => x.Published == true).Count(),
                        PageNum = pageSize / 10,
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                result.Code = ex.HResult;
                result.Message = ex.Message;
                return result;
            }
        }
        public ResponseBase<List<NewsHomeData>> GetNewsByKeyword(string keyword, int pageIndex, int pageSize)
        {
            var result = new ResponseBase<List<NewsHomeData>>()
            {
                Data = new List<NewsHomeData>()
            };
            try
            {
                var listId = new List<int>();
                keyword = keyword.Trim();
                var key = keyword.RemoveUnicode().Replace(" ", string.Empty).Trim();
                var listNews = _newsRepository.FindAll(x => x.Iddomain == 1 && x.Deleted != 1 && x.Status == (int)NewsStatus.Accepted
                                                         && x.Keysearch.Contains(key))
                                                        .OrderByDescending(x => x.DateCreate);


                if (listNews != null && listNews.Any())
                {
                    var total = listNews.Count();
                    var data = listNews.Skip((pageIndex - 1) * pageSize).Take(pageSize).Select(x => new NewsHomeData
                    {
                        Id = x.Id,
                        Status = x.Status,
                        Title = x.Title,
                        DateCreate = x.DateCreate,
                        IdCategory = x.IdCategory,
                        IdTopics = x.NewsTopics.Select(z => z.IdNewsTopic).ToList(),
                        PictureTitle = x.PictureTitle,
                        Description = x.Description,
                        Category = x.IdCategoryNavigation.Category,
                        CategoryEng = x.IdCategoryNavigation.CategoryEng,
                        CategoryChi = x.IdCategoryNavigation.CategoryChi,
                        UserName = x.IdUserNavigation.UserName,
                        Click = x.Click ?? 0,
                        Slug = SlugHelper.GenerateSlug(VietnameseNormalizer.NormalizeVietnamese(x.Title)),
                        Total = total,
                        CommentCount = x.CommentNews.Where(x => x.Published == true).Count(),
                        PageNum = pageSize / 10,
                    }).ToList();
                    var keyIndex = keyword.RemoveUnicode().ToLower();
                    List<NewsHomeData> lst = new List<NewsHomeData>();
                    foreach (var item in data)
                    {
                        item.TitleNoCode = item.Title.ToLower().RemoveUnicode();
                        item.IndexOfKey = item.TitleNoCode.ToLower().IndexOf(keyIndex);
                        if (item.IndexOfKey < 0)
                        {
                            var keyIndex2 = keyIndex.Replace(" ", string.Empty);
                            item.IndexOfKey = item.TitleNoCode.ToLower().IndexOf(keyIndex2);
                        }

                        if (item.IndexOfKey >= 0)
                        {
                            item.TitlePartial = new List<TitlePartial>
                            {
                                new TitlePartial
                                {
                                    Index = 0,
                                    Value = item.Title.Substring(0, item.IndexOfKey),
                                    IsBold = false,
                                },
                                new TitlePartial
                                {
                                    Index = 1,
                                    Value = item.Title.Substring(item.IndexOfKey, keyword.Length),
                                    IsBold = true,
                                },
                                new TitlePartial
                                {
                                    Index = 2,
                                    Value = item.Title.Substring(item.IndexOfKey + keyword.Length),
                                    IsBold = false,
                                }
                            };

                            lst.Add(item);
                        }
                        else
                            item.TitlePartial = new List<TitlePartial>();
                    }
                    result.Data = lst;
                }

                return result;
            }
            catch (Exception ex)
            {
                result.Code = ex.HResult;
                result.Message = ex.Message;
                return result;
            }
        }
    }
}
