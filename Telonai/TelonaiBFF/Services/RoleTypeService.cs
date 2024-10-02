namespace TelonaiWebApi.Services;

using AutoMapper;
using BCrypt.Net;

using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;

public interface IRoleTypeService
{
    IEnumerable<RoleType> GetAll();
    RoleType GetById(int id);
    RoleType GetByValue(string value);
    void Delete(int id);
}

public class RoleTypeService : IRoleTypeService
{
    private readonly DataContext _context;

    public RoleTypeService(DataContext context)
    {
        _context = context;
    }

    public IEnumerable<RoleType> GetAll()
    {
        return  _context.RoleType;
    }

    public RoleType GetById(int id)
    {
        return GetRoleType(id);
    }


    public RoleType GetByValue(string value)
    {
        return _context.RoleType.FirstOrDefault(e => e.Value == value);
    }

    public void Delete(int id)
    {
        var user = GetRoleType(id);
        _context.RoleType.Remove(user);
        _context.SaveChanges();
    }

    private RoleType GetRoleType(int id)
    {
        var result = _context.RoleType.Find(id);
        if (result == null) throw new KeyNotFoundException("RoleType not found");
        return result;
    }
}