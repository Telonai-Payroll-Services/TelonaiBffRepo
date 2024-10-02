namespace TelonaiWebApi.Services;

using AutoMapper;
using BCrypt.Net;

using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;

public interface IBusinessTypeService
{
    IEnumerable<BusinessType> GetAll();
    BusinessType GetById(int id);
    BusinessType GetByValue(string value);
    void Delete(int id);
}

public class BusinessTypeService : IBusinessTypeService
{
    private DataContext _context;

    public BusinessTypeService(DataContext context)
    {
        _context = context;
    }

    public IEnumerable<BusinessType> GetAll()
    {
        return  _context.BusinessType;
    }

    public BusinessType GetById(int id)
    {
        return getBusinessType(id);
    }

    public BusinessType GetByValue(string value)
    {
        return _context.BusinessType.FirstOrDefault(e => e.Value == value);
    }

    public void Delete(int id)
    {
        var user = getBusinessType(id);
        _context.BusinessType.Remove(user);
        _context.SaveChanges();
    }

    private BusinessType getBusinessType(int id)
    {
        var result = _context.BusinessType.Find(id);
        return result == null ? throw new KeyNotFoundException("BusinessType not found") : result;
    }
}