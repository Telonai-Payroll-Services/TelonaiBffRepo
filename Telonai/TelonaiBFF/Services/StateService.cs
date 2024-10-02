namespace TelonaiWebApi.Services;

using AutoMapper;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;

public interface IStateService
{
    IEnumerable<State> GetByCountryId(int countryId);
    State GetById(int id);
    State GetByName(string name, int countryId);
    void Delete(int id);
}

public class StateService : IStateService
{
    private DataContext _context;

    public StateService(DataContext context)
    {
        _context = context;
    }

    public IEnumerable<State> GetByCountryId(int countryId)
    {
        return  _context.State.Include(e=>e.Country).Where(e=>e.CountryId==countryId);
    }

    public State GetById(int id)
    {
        return getState(id);
    }

    public State GetByName(string name, int countryId)
    {
        return _context.State.Include(e => e.Country).FirstOrDefault(e => e.Name == name && e.CountryId == countryId);
    }
 

    public void Delete(int id)
    {
        var user = getState(id);
        _context.State.Remove(user);
        _context.SaveChanges();
    }

    private State getState(int id)
    {
        var result = _context.State.Find(id);
        if (result == null) throw new KeyNotFoundException("State not found");
        return result;
    }
}