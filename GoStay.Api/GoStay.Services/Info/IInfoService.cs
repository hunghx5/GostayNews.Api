namespace GoStay.Services.Info
{
    public interface IInfoService
    {
        Task AddEmailAsync(string email, CancellationToken cancellationToken = default);
    }
}
