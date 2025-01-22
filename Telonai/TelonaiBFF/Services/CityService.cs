namespace TelonaiWebApi.Services;

using Microsoft.EntityFrameworkCore;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;

public interface ICityService
{
    IEnumerable<City> GetByCountryId(int countryId);
    IEnumerable<City> GetByStateId(int stateId);
    City GetById(int id);
    void Delete(int id);
}

public class CountyService : ICityService
{
    private readonly DataContext _context;

    public CountyService(DataContext context)
    {
        _context = context;
    }

    public IEnumerable<City> GetByCountryId(int countryId)
    {
        return _context.City.Include(e => e.State).ThenInclude(e => e.Country).Where(e => e.State.CountryId == countryId);
    }

    public IEnumerable<City> GetByStateId(int stateId)
    {
        return _context.City.Include(e => e.State).ThenInclude(e => e.Country).Where(e=>e.StateId==stateId);
    }

    public City GetById(int id)
    {
        return GetCity(id);
    }


    public void Delete(int id)
    {
        var user = GetCity(id);
        _context.City.Remove(user);
        _context.SaveChanges();
    }

    private City GetCity(int id)
    {
        var result = _context.City.Include(e => e.State).SingleOrDefault(e => e.Id == id);
        return result == null ? throw new KeyNotFoundException("City not found") : result;
    }
}