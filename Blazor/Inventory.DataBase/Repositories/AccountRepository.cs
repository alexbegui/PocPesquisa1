using CensusFieldSurvey.Model.Common.Response;
using Gestao.Client.Libraries.Utilities;
using Gestao.Domain;
using Gestao.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CensusFieldSurvey.DataBase.Repositories
{
    public class AccountRepository : Repository<Account>
    {
        private readonly AppDbContext _db;
        public AccountRepository(AppDbContext db) : base(db)
        {
            _db = db; 
        }
        public async Task<PaginatedList<Account>> GetAll(int companyId, int pageIndex, int pageSize, string searchWord = "")
        {
            var query = _db.Accounts.AsQueryable();

            var items = await query
                    .Where(a => a.CompanyId == companyId)
                    .Where(a => a.Description.Contains(searchWord))
                    .OrderBy(a => a.Description)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

            var count = await query
                .Where(a => a.CompanyId == companyId)
                .Where(a => a.Description.Contains(searchWord))
                .CountAsync();

            int totalPages = (int)Math.Ceiling((decimal)count / pageSize);

            return new PaginatedList<Account>(items, pageIndex, totalPages);
        }

        public async Task<List<Account>> GetAll(int companyId)
        {
            var query = _db.Accounts.AsQueryable();
            return await query.Where(a => a.CompanyId == companyId).ToListAsync();
        }
    }
}
