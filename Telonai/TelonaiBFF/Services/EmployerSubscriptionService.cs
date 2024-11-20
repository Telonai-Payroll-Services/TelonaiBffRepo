namespace TelonaiWebApi.Services;

using AutoMapper;
using BCrypt.Net;

using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;

public interface IEmployerSubscriptionService
{
    Task CreateAsync(EmployerSubscriptionModel model);
    IEnumerable<EmployerSubscription> GetAll();
    EmployerSubscription GetById(int id);
    EmployerSubscription GetByCompanyName(string name);
    void Delete(int id);
}

public class EmployerSubscriptionService : IEmployerSubscriptionService
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public EmployerSubscriptionService(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public IEnumerable<EmployerSubscription> GetAll()
    {
        return _context.EmployerSubscription;
    }

    public EmployerSubscription GetById(int id)
    {
        return getEmployerSubscription(id);
    }
    public EmployerSubscription GetByCompanyName(string name)
    {
        return _context.EmployerSubscription.FirstOrDefault(e => e.Invitation.CompanyName== name && !e.IsCancelled);
    }
    public async Task CreateAsync(EmployerSubscriptionModel model)
    {
        var dto = _mapper.Map<EmployerSubscription>(model);
        _context.EmployerSubscription.Add(dto);
        await _context.SaveChangesAsync();
    }

    public void Delete(int id)
    {
        var item = getEmployerSubscription(id);
        item.IsCancelled = true;
        _context.EmployerSubscription.Update(item);
        _context.SaveChanges();
    }

    private EmployerSubscription getEmployerSubscription(int id)
    {
        var result = _context.EmployerSubscription.Find(id);
        return result == null ? throw new KeyNotFoundException("EmployerSubscription not found") : result;
    }
}