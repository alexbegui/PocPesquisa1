using Gestao.Client.Libraries.Utilities;
using Gestao.Domain;
using Gestao.Domain.Enums;
using Gestao.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CensusFieldSurvey.DataBase.Repositories
{
    public class FinancialTransactionRepository : Repository<FinancialTransaction>
    {
        private readonly AppDbContext _db;
        
        public FinancialTransactionRepository(AppDbContext db) : base(db)
        {
            _db = db;
        }

        public async Task<PaginatedList<FinancialTransaction>> GetAll(int companyId, TypeFinancialTransaction type, int pageIndex, int pageSize, string searchWord = "")
        {
            var query = _db.FinancialTransactions.AsQueryable();

            var items = await query
                .Where(a => a.CompanyId == companyId && a.TypeFinancialTransaction == type)
                .Where(a => a.Description.Contains(searchWord))
                .OrderByDescending(a => a.ReferenceDate)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Include(a => a.Category)
                .Include(a => a.Account)
                .Include(a => a.Documents)
                .ToListAsync();

            var count = await query
                .Where(a => a.CompanyId == companyId && a.TypeFinancialTransaction == type)
                .Where(a => a.Description.Contains(searchWord))
                .CountAsync();

            int totalPages = (int)Math.Ceiling((decimal)count / pageSize);

            return new PaginatedList<FinancialTransaction>(items, pageIndex, totalPages);
        }

        public async Task<List<FinancialTransaction>> GetTransactionsSameGroup(int Id)
        {
            return await _db.FinancialTransactions
                .Where(a => a.RepeatGroup == Id)
                .OrderBy(a => a.Id)
                .ToListAsync();
        }

        public async Task<int> GetCountTransactionsSameGroup(int Id)
        {
            return await _db.FinancialTransactions
                .Where(a => a.RepeatGroup == Id)
                .CountAsync();
        }
    }
}
