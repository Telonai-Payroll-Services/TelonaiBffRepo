namespace TelonaiWebApi.Services;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;

public interface IEmploymentService<Tmodel, Tdto> : IDataService<Tmodel, Tdto>
{
    Task CompleteAddingEmployees(string currentUserEmail, int companyId);
    IList<EmploymentModel> GetByPersonId(int personId);
    Task<EmploymentModel> CreateAsync(Employment dto, int companyId);
    Task<Employment> CreateAsync(EmploymentModel model, int companyId);
    Task DeleteAsync(int id, int companyId);
}

public class EmploymentService : IEmploymentService<EmploymentModel,Employment>
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public EmploymentService(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public IList<EmploymentModel> Get()
    {
        var obj =  _context.Employment.Where(e=>!e.Deactivated);        
        var result=_mapper.Map<IList<EmploymentModel>>(obj);
        return result;
    }

    public EmploymentModel GetById(int id)
    {
        var obj = GetEmployment(id);
        var result = _mapper.Map<EmploymentModel>(obj);
        return result;
    }

    public IList<EmploymentModel> GetByPersonId(int personId)
    {
        var obj = _context.Employment.Include(e => e.Job).ThenInclude(e => e.Company).Where(e => e.PersonId == personId &&
        !e.Deactivated);
        var result = _mapper.Map<IList<EmploymentModel>>(obj);
        return result;
    }

    public async Task CompleteAddingEmployees(string currentUserEmail, int companyId)
    {
        var currentEmp = _context.Employment.First(e => e.Person.Email == currentUserEmail && e.Job.CompanyId == companyId && !e.Deactivated);
        currentEmp.SignUpStatusTypeId = (int)SignUpStatusTypeModel.EmployeeAddtionCompleted;
        _context.Employment.Update(currentEmp);
        await _context.SaveChangesAsync();
    }
    public async Task<Employment> CreateAsync(EmploymentModel model)
    {
        throw new NotImplementedException();    
    }
    
    public async Task<Employment> CreateAsync(EmploymentModel model, int companyId)
    {
        var emp = _context.Employment.FirstOrDefault(e => e.Job.CompanyId == companyId && e.PersonId == model.PersonId && !e.Deactivated);
        var result = _mapper.Map<Employment>(model);

        if (emp == null)
        {
            _context.Employment.Add(result);
            await _context.SaveChangesAsync();
        }
        return result;
    }

    public async Task<EmploymentModel> CreateAsync(Employment dto, int companyId)
    {
        var emp = _context.Employment.FirstOrDefault(e => e.Job.CompanyId == companyId && e.PersonId == dto.PersonId && !e.Deactivated);
        if (emp == null)
        {
            _context.Employment.Add(dto);
            await _context.SaveChangesAsync();
        }
        return _mapper.Map<EmploymentModel>(dto);
    }

    public async Task UpdateAsync(int id, EmploymentModel model)
    {
        var emp = GetEmployment(id) ?? throw new AppException("Employment not found");

        _mapper.Map(model, emp);
        _context.Employment.Update(emp);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        throw new NotImplementedException();
    }

    public async Task DeleteAsync(int id, int companyId)
    {
        var emp = _context.Employment.FirstOrDefault(e=>e.Id==id && e.Job.CompanyId==companyId);
        if (emp != null)
        {
            emp.Deactivated = true;
            _context.Employment.Update(emp);
            await _context.SaveChangesAsync();
        }
    }

    private Employment GetEmployment(int id)
    {
        return _context.Employment.Find(id);

    }
}