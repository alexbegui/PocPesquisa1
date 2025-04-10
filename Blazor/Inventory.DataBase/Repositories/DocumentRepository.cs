using Gestao.Client.Libraries.Utilities;
using Gestao.Domain;
using Gestao.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CensusFieldSurvey.DataBase.Repositories
{
    public class DocumentRepository : Repository<Document>
    {
        private readonly AppDbContext _db;
        
        public DocumentRepository(AppDbContext db) : base(db)
        {
            _db = db;
        }
    }
}
