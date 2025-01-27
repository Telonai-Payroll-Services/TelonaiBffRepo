using Microsoft.EntityFrameworkCore;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;

namespace TelonaiWebApi.Services
{
    public interface ICountyService
    {
        Task<County> GetById(int id);

        Task<List<County>> GetByStateId(int stateid);

        County GetByNameAndStateId(string name, int stateId); 
    }
    public class CountyService : ICountyService
    {
        private DataContext _context;

        public CountyService(DataContext context)
        {
            _context = context;
        }
        public async   Task<County> GetById(int id)
        {
            return await GetCountyById(id);
        }

        public County GetByNameAndStateId(string name, int stateId)
        {
            return _context.County.FirstOrDefault(e => e.Name == name && e.StateId == stateId);
        }

        public async Task<List<County>> GetByStateId(int stateid)
        {
            return _context.County.Where(x=>x.StateId == stateid).ToList();
        }

        private async Task<County> GetCountyById(int id)
        {
            var result = await _context.County.FindAsync(id);
            if (result == null) throw new KeyNotFoundException("County not found");
            return result;
        }
    }
}
