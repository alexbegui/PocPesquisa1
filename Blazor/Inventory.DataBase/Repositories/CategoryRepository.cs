using Gestao.Client.Libraries.Utilities;
using Gestao.Domain;
using Gestao.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CensusFieldSurvey.DataBase.Repositories
{
    public class CategoryRepository : Repository<Category>
    {
        private readonly AppDbContext _db;
        
        public CategoryRepository(AppDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<PaginatedList<Category>> GetAll(int companyId, int pageIndex, int pageSize, string searchWord = "")
        {
            var query = _db.Categories.AsQueryable();

            var items = await query.Where(a => a.CompanyId == companyId)
                .OrderBy(a => a.Name)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();


            var count = await _db.Categories.Where(a => a.CompanyId == companyId).CountAsync();
            int totalPages = (int)Math.Ceiling((decimal)count / pageSize);

            return new PaginatedList<Category>(items, pageIndex, totalPages);
        }
    }
}
