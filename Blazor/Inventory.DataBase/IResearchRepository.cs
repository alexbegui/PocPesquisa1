using CensusFieldSurvey.Model.Common.Response;
using Gestao.Client.Libraries.Utilities;

namespace CensusFieldSurvey.DataBase
{
    public interface IResearchRepository : IRepository<CensusFieldSurvey.Model.EntitesBD.Research>
    {
        Task<PaginatedList<ResearchResponse>> GetAll(string? searchWord = null, int pageNumber = 1, int pageSize = 10);
    }
}