using CensusFieldSurvey.Model.Common.Response;
using Gestao.Client.Libraries.Utilities;

namespace Gestao.Client.Repositories
{
    public interface IResearchRepository
    {
        Task<PaginatedList<ResearchResponse>> GetAll(string? searchWord = null, int pageNumber = 1, int pageSize = 10);
    }
}