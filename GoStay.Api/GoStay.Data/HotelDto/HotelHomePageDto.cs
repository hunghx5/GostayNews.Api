using System.Text.Json.Serialization;

namespace GoStay.Data.HotelDto
{
	public class HotelHomePageDto
	{
        private const string CdnBaseUrl = "https://cdn.realtech.com.vn/";
        private const string WebsiteBaseUrl = "https://gostay.vn";

        public int Id { get; set; }
        public string HotelName { get; set; }
        public string TinhThanh { get; set; }
        public string QuanHuyen { get; set; }
        public int? Rating { get; set; }
        public decimal? AvgNight { get; set; }
        public double Review_score { get; set; }
        public double? Lat_map { get; set; }
        public double? Lon_map { get; set; }

        public int? NumberReviewers { get; set; }
        public int? PalletbedRoom { get; set; }
        public long? IntDate { get; set; }
        public int Total { get; set; }
        public int TotalPicture { get; set; }
        private string Urls 
        {
            set
            {
				if (string.IsNullOrEmpty(value))
					Pictures = new List<string>();
				else
				{
					Pictures = value.Split(';').ToList();

				}
			}
        }
        public List<string> Pictures { get; set; } = new List<string>();
        [JsonPropertyName("listImage")]
        public List<string> ListImage
        {
            get
            {
                return Pictures
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(ToCdnUrl)
                    .ToList();
            }
        }
        public DateTime LastOrderTime { get; set; }
        public string Slug { get; set; }
        [JsonPropertyName("detailLink")]
        public string DetailLink
        {
            get
            {
                return $"{WebsiteBaseUrl}/khach-san/{Id}/{Slug}";
            }
        }


        public double? Discount { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal ActualPrice 
        {
            get
            {
                return (decimal)(DailyBasePrice * (100 - Discount??0) / 100);
            }
        }
        public int IdRoom { get; set; }
        public double DailyPrice { get; set; }
        public double DailyBasePrice 
        {
            get
            {
                if (DailyPrice > 0)
                    return DailyPrice;
                else
                    return (double)OriginalPrice;
            }
        }
        public string? TopService { get; set; }
        public int RandomOrderNumber { get; set; } = 1;

        private static string ToCdnUrl(string url)
        {
            var trimmedUrl = url.Trim();
            if (trimmedUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                trimmedUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return trimmedUrl;
            }

            return CdnBaseUrl + trimmedUrl.TrimStart('/');
        }
    }

}
