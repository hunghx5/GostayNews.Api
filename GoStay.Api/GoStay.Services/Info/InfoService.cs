using GoStay.DataAccess.DBContext;
using GoStay.DataDto.Info;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace GoStay.Services.Info
{
    public class InfoService : IInfoService
    {
        private readonly CommonDBContext _context;

        public InfoService(CommonDBContext context)
        {
            _context = context;
        }

        public async Task AddEmailAsync(string email, string? domain, CancellationToken cancellationToken = default)
        {
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"INSERT INTO [Info] ([email], [domain]) VALUES ({email.Trim()}, {domain?.Trim()})",
                cancellationToken);
        }

        public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var affectedRows = await _context.Database.ExecuteSqlInterpolatedAsync(
                $"DELETE FROM [Info] WHERE [id] = {id}", cancellationToken);
            return affectedRows > 0;
        }

        public async Task<List<InfoDto>> GetByDomainAsync(string domain, CancellationToken cancellationToken = default)
        {
            var connection = _context.Database.GetDbConnection();
            var shouldClose = connection.State != ConnectionState.Open;
            if (shouldClose)
            {
                await _context.Database.OpenConnectionAsync(cancellationToken);
            }

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = "SELECT [email], [domain], [id] FROM [Info] WHERE @domain = '0' OR [domain] = @domain ORDER BY [email], [id]";
                var parameter = command.CreateParameter();
                parameter.ParameterName = "@domain";
                parameter.DbType = DbType.AnsiString;
                parameter.Size = 255;
                parameter.Value = domain.Trim();
                command.Parameters.Add(parameter);

                var items = new List<InfoDto>();
                using var reader = await command.ExecuteReaderAsync(cancellationToken);
                while (await reader.ReadAsync(cancellationToken))
                {
                    items.Add(new InfoDto
                    {
                        Id = reader.GetInt32(2),
                        Email = reader.IsDBNull(0) ? null : reader.GetString(0),
                        Domain = reader.IsDBNull(1) ? null : reader.GetString(1)
                    });
                }

                return items;
            }
            finally
            {
                if (shouldClose)
                {
                    await _context.Database.CloseConnectionAsync();
                }
            }
        }
    }
}
