using PoMiniApps.Shared.Models;

namespace PoMiniApps.Web.Services.News;

public interface INewsService
{
    Task<List<NewsHeadline>> GetTopHeadlinesAsync(int count);
}
