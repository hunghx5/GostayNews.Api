using GoStay.DataDto.Info;

namespace GoStay.Services.Info
{
    public interface IInfoService
    {
        Task AddEmailAsync(string email, string? domain, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
        Task<List<InfoDto>> GetByDomainAsync(string domain, CancellationToken cancellationToken = default);
    }
}
