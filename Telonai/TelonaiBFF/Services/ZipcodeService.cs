namespace TelonaiWebApi.Services;

using AutoMapper;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;

public interface IZipcodeService
{
    List<Zipcode> GetByCityId(int id);
    List<Zipcode> GetByZipcodeAndCountryId(string code, int countryId);
    Zipcode GetById(int id);
    void Delete(int id);
}

public class ZipcodeService : IZipcodeService
{
    private DataContext _context;

    public ZipcodeService(DataContext context)
    {
        _context = context;
    }

    public List<Zipcode> GetByCityId(int id)
    {
        return _context.Zipcode.Include(e => e.City).ThenInclude(e => e.State).ThenInclude(e => e.Country).Where(e=>e.CityId==id).ToList();
    }
    public List<Zipcode> GetByZipcodeAndCountryId(string code, int countryId)
    {
        return _context.Zipcode.Include(e => e.City).ThenInclude(e => e.State).Where(e => e.Code == code && e.City.State.CountryId == countryId).ToList();
    }

    public Zipcode GetById(int id)
    {
        return GetZipcode(id);
    }


    public void Delete(int id)
    {
        var user = GetZipcode(id);
        _context.Zipcode.Remove(user);
        _context.SaveChanges();
    }

    private Zipcode GetZipcode(int id)
    {
        var result = _context.Zipcode.Find(id);
        return result == null ? throw new KeyNotFoundException("Zipcode not found") : result;
    }
}