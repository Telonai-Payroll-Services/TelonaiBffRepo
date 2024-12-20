using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Entities;

namespace TelonaiWebApi.Services
{
    public interface IDayOffService
    {
        Task<List<DayOffType>> GetAllDayOffType();

        Task<DayOffType> GetDayOffTypeById(int id);
    }
    public class DayOffService : IDayOffService
    {
        private readonly DataContext _context;
        public DayOffService(DataContext context)
        {
                _context = context;
        }
        public async Task<DayOffType> GetDayOffTypeById(int id)
        {
            var dayOffType = await  _context.DayoffType.FindAsync(id);
            return dayOffType;
        }

        public async Task<List<DayOffType>> GetAllDayOffType()
        {

            var allDayOffTypes = await _context.DayoffType.ToListAsync();
            return allDayOffTypes;
        }
    }
}
