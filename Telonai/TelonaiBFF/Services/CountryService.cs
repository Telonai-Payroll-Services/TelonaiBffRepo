namespace TelonaiWebApi.Services;

using AutoMapper;
using BCrypt.Net;

using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;

public interface ICountryService
{
    IEnumerable<Country> GetAll();
    Country GetById(int id);
    Country GetByName(string name);
    void Delete(int id);
}

public class CountryService : ICountryService
{
    private DataContext _context;

    public CountryService(DataContext context)
    {
        _context = context;
    }

    public IEnumerable<Country> GetAll()
    {
        return _context.Country.Where(e=>e.Id!=1).OrderBy(e=>e.Name);
    }

    public Country GetById(int id)
    {
        return getCountry(id);
    }
    public Country GetByName(string name)
    {
        return _context.Country.FirstOrDefault(e => e.Name == name);
    }

    public void Delete(int id)
    {
        var user = getCountry(id);
        _context.Country.Remove(user);
        _context.SaveChanges();
    }

    private Country getCountry(int id)
    {
        var result = _context.Country.Find(id);
        return result == null ? throw new KeyNotFoundException("Country not found") : result;
    }
}