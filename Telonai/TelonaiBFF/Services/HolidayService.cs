namespace TelonaiWebApi.Services;

using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;

public interface IHolidaysService
{
    Holiday GetById(int id);
    Dictionary<string,DateOnly> GetByCountryIdAndYear(int countryId, int year);
    void Delete(int id);
}

public class HolidaysService : IHolidaysService
{
    private DataContext _context;

    public HolidaysService(DataContext context)
    {
        _context = context;
    }

    public Holiday GetById(int id)
    {
        return getHoliday(id);
    }

    public Dictionary<string, DateOnly> GetByCountryIdAndYear(int countryId, int year)
    {
        return (Dictionary<string, DateOnly>)_context.Holiday.Where(e => e.CountryId.Equals(countryId) && e.Date.Year== year).Select(e=> new KeyValuePair<string, DateOnly>(e.Name,e.Date));
    }

    public void Delete(int id)
    {
        var item = getHoliday(id);
        _context.Holiday.Remove(item);
        _context.SaveChanges();
    }

    private Holiday getHoliday(int id)
    {
        var result = _context.Holiday.Find(id);
        return result;
    }
}