using Gestao.Client.Libraries.Utilities;
using Gestao.Domain;
using Gestao.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CensusFieldSurvey.DataBase.Repositories
{
    public class CompanyRepository : Repository<Company>
    {
        private readonly AppDbContext _db;
        
        public CompanyRepository(AppDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<PaginatedList<Company>> GetAll(Guid applicationUserId, int pageIndex, int pageSize, string searchWord = "")
        {
            var query = _db.Companies.AsQueryable();

            var items = await query
                .Where(a => a.UserId == applicationUserId)
                .Where(a => a.TradeName.Contains(searchWord) || a.LegalName.Contains(searchWord))
                .OrderBy(a => a.TradeName)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var count = await query
                .Where(a => a.UserId == applicationUserId)
                .Where(a => a.TradeName.Contains(searchWord) || a.LegalName.Contains(searchWord))
                .CountAsync();

            int totalPages = (int)Math.Ceiling((decimal)count / pageSize);

            return new PaginatedList<Company>(items, pageIndex, totalPages);
        }

        public new async Task Add(Company entity)
        {
            entity.CreatedAt = entity.CreatedAt.ToUniversalTime();
            if (entity.DeletedAt.HasValue)
            {
                entity.DeletedAt = entity.DeletedAt.Value.ToUniversalTime();
            }

            await _db.Companies.AddAsync(entity);
            await _db.SaveChangesAsync();
        }
    }
}
