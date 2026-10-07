using GoStay.DataAccess.DBContext;
using Microsoft.EntityFrameworkCore;

namespace GoStay.Services.Info
{
    public class InfoService : IInfoService
    {
        private readonly CommonDBContext _context;

        public InfoService(CommonDBContext context)
        {
            _context = context;
        }

        public async Task AddEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"INSERT INTO [Info] ([email]) VALUES ({email.Trim()})",
                cancellationToken);
        }
    }
}
