using PoMiniApps.Shared.Models;

namespace PoMiniApps.Web.Services.Data;

public interface IRapperRepository
{
    Task<List<Rapper>> GetAllRappersAsync();
    Task SeedInitialRappersAsync();
    Task UpdateWinLossRecordAsync(string winnerName, string loserName);
}
