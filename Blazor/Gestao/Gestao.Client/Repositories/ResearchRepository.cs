using CensusFieldSurvey.Model.Common.Response;
using Gestao.Client.Libraries.Utilities;
using System.Net.Http.Json;

namespace Gestao.Client.Repositories
{
    public class ResearchRepository : IResearchRepository
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public ResearchRepository(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<PaginatedList<ResearchResponse>> GetAll(string? searchWord = null, int pageNumber = 1, int pageSize = 10)
        {
            string url = $"{_configuration["ApiSettings:BaseUrl"]}/api/Research/GetResearchAll";
            
            // Add query parameters if needed
            if (!string.IsNullOrEmpty(searchWord) || pageNumber > 1 || pageSize != 10)
            {
                url += $"?searchWord={Uri.EscapeDataString(searchWord ?? "")}&pageNumber={pageNumber}&pageSize={pageSize}";
            }
            
            var response = await _httpClient.GetFromJsonAsync<List<ResearchResponse>>(url);
            if (response == null)
            {
                return new PaginatedList<ResearchResponse>(new List<ResearchResponse>(), pageNumber, 0);
            }
            
            // For now, we're handling pagination on the client side
            // In the future, the API should return a paginated response
            int totalItems = response.Count;
            int totalPages = (int)Math.Ceiling((decimal)totalItems / pageSize);
            
            var pagedItems = response
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();
                
            return new PaginatedList<ResearchResponse>(pagedItems, pageNumber, totalPages);
        }
    }
}