namespace TelonaiWebApi.Services;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;

public interface IZipcodeService
{
    List<Zipcode> GetByCityId(int id);
    List<Zipcode> GetByZipcodeAndCountryId(string code, int countryId);
    List<ZipcodeModel> GetModelByZipcodeAndCountryId(string code, int countryId);
    Zipcode GetById(int id);
    void Delete(int id);
}

public class ZipcodeService : IZipcodeService
{
    private DataContext _context;
    private readonly IMapper _mapper;

    public ZipcodeService(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public List<Zipcode> GetByCityId(int id)
    {
        return _context.Zipcode.Include(e => e.City).ThenInclude(e => e.State).ThenInclude(e => e.Country).Where(e=>e.CityId==id).ToList();
    }
    public List<Zipcode> GetByZipcodeAndCountryId(string code, int countryId)
    {
        return _context.Zipcode.Include(e => e.City).ThenInclude(e => e.State).Where(e => e.Code == code && e.City.State.CountryId == countryId).ToList();
    }
    public List<ZipcodeModel> GetModelByZipcodeAndCountryId(string code, int countryId)
    {
        var zips= _context.Zipcode.Include(e => e.City).ThenInclude(e => e.State).Where(e => e.Code == code && e.City.State.CountryId == countryId).ToList();
        var result = zips.Select(e =>
        {
            var model = _mapper.Map<ZipcodeModel>(e);
            var counties = _context.County.Where(c => e.CountyId.Contains(c.Id)).ToList();
            model.Counties = _mapper.Map<List<CountyModel>>(counties);
            return model;
        }).ToList();
        return result;
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