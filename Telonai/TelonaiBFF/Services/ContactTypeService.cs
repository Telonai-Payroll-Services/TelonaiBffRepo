namespace TelonaiWebApi.Services;

using AutoMapper;
using BCrypt.Net;

using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;

public interface IContactTypeService
{
    IEnumerable<ContactType> GetAll();
    ContactType GetById(int id);
    ContactType GetByValue(string value);
    void Delete(int id);
}

public class ContactTypeService : IContactTypeService
{
    private readonly DataContext _context;

    public ContactTypeService(DataContext context)
    {
        _context = context;
    }

    public IEnumerable<ContactType> GetAll()
    {
        return  _context.ContactType;
    }

    public ContactType GetById(int id)
    {
        return getContactType(id);
    }


    public ContactType GetByValue(string value)
    {
        return _context.ContactType.FirstOrDefault(e => e.Value == value);
    }

    public void Delete(int id)
    {
        var user = getContactType(id);
        _context.ContactType.Remove(user);
        _context.SaveChanges();
    }

    private ContactType getContactType(int id)
    {
        var result = _context.ContactType.Find(id);
        return result == null ? throw new KeyNotFoundException("ContactType not found") : result;
    }
}