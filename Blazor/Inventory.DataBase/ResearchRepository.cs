using CensusFieldSurvey.Model.Common.Response;
using CensusFieldSurvey.Model.EntitesBD;
using Gestao.Client.Libraries.Utilities;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace CensusFieldSurvey.DataBase
{
    public class ResearchRepository : Repository<Research>, IResearchRepository
    {
        private readonly AppDbContext _db;

        public ResearchRepository(AppDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<PaginatedList<ResearchResponse>> GetAll(string? searchWord = null, int pageNumber = 1, int pageSize = 10)
        {
            var query = _db.Researchs.AsQueryable();
            
            if (!string.IsNullOrEmpty(searchWord))
            {
                query = query.Where(r => r.Form != null && r.Form.Contains(searchWord));
            }

            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var count = await query.CountAsync();
            int totalPages = (int)Math.Ceiling((decimal)count / pageSize);

            var responseItems = items.Select(r => new ResearchResponse
            {
                IdResearch = r.IdResearch,
                Form = r.Form
            }).ToList();

            return new PaginatedList<ResearchResponse>(responseItems, pageNumber, totalPages);
        }
    }
}
