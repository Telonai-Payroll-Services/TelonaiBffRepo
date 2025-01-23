namespace TelonaiWebApi.Services;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Ocsp;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;

public interface IEmployeeWithholdingService<Tmodel, Tdto> : IDataService<Tmodel, Tdto>
{
    Task CreateAsync(EmployeeWithholdingModel model, Stream file);
    EmployeeWithholdingModel GetByEmploymentIdAndFieldId(int empId, int fieldId);
    IList<EmployeeWithholdingModel> GetByCompanyIdAsync(int companyId);
}

public class EmployeeWithholdingService : IEmployeeWithholdingService<EmployeeWithholdingModel,EmployeeWithholding>
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;
    private readonly IDocumentManager _documentManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IScopedAuthorization _scopedAuthorization;
    public EmployeeWithholdingService(DataContext context, IMapper mapper, IDocumentManager documentManager,
      IHttpContextAccessor httpContextAccessor, IScopedAuthorization scopedAuthorization)
    {
        _context = context;
        _mapper = mapper;
        _documentManager = documentManager;
        _httpContextAccessor = httpContextAccessor;
        _scopedAuthorization = scopedAuthorization;
    }

    public IList<EmployeeWithholdingModel> Get()
    {
        var obj =  _context.EmployeeWithholding;        
        var result=_mapper.Map<IList<EmployeeWithholdingModel>>(obj);
        return result;
    }

    public IList<EmployeeWithholdingModel> GetByCompanyIdAsync(int companyId)
    {
        var person = GetCurrentUserAsync()?.Result;
        if (person == null || person.CompanyId != companyId)
        {
            throw new InvalidDataException("User not found");
        }

        _scopedAuthorization.ValidateByCompanyId(_httpContextAccessor.HttpContext.User, AuthorizationType.Admin, companyId);

        var obj = _context.EmployeeWithholding.Where(e=>e.Employment.Job.CompanyId==companyId);
        var result = _mapper.Map<IList<EmployeeWithholdingModel>>(obj);
        return result;
    }

    public EmployeeWithholdingModel GetById(int id)
    {
        var person = GetCurrentUserAsync()?.Result;
        _scopedAuthorization.ValidateByCompanyId(_httpContextAccessor.HttpContext.User, AuthorizationType.User, person.CompanyId);

        var obj = GetEmployeeWithholding(id);
        var result = _mapper.Map<EmployeeWithholdingModel>(obj);
        return result;
    }

    public EmployeeWithholdingModel GetByEmploymentIdAndFieldId(int empId,  int fieldId)
    {
        var emp = _context.Employment.FindAsync(empId).Result ?? throw new InvalidDataException("User not found");
        var person = GetCurrentUserAsync()?.Result;
        if (emp.PersonId == person.Id) { throw new InvalidDataException("User not found"); }

        _scopedAuthorization.ValidateByCompanyId(_httpContextAccessor.HttpContext.User, AuthorizationType.User, person.CompanyId);

        var obj = _context.EmployeeWithholding.Include(e => e.Field).FirstOrDefault(e => e.EmploymentId == empId &&
        e.FieldId == fieldId && !e.Deactivated);

        var result = _mapper.Map<EmployeeWithholdingModel>(obj);
        return result;
    }
    public async Task<EmployeeWithholding> CreateAsync(EmployeeWithholdingModel model)
    {
        throw new NotImplementedException();
    }

    public async Task CreateAsync(EmployeeWithholdingModel model, Stream file)
    {
        var person = await GetCurrentUserAsync();
        _scopedAuthorization.ValidateByCompanyId(_httpContextAccessor.HttpContext.User, AuthorizationType.User, person.CompanyId);

        var emp = await _context.Employment.FindAsync(model.EmploymentId) ?? throw new InvalidDataException("User not found");
        if(emp.PersonId== person.Id) { throw new InvalidDataException("User not found"); }

        var dto = _mapper.Map<EmployeeWithholding>(model);
        var withholding = _context.EmployeeWithholding.OrderByDescending(e=>e.EffectiveDate).FirstOrDefault(e => e.FieldId==model.FieldId && 
        e.EmploymentId==model.EmploymentId && !e.Deactivated &&  e.WithholdingYear==model.WithholdingYear);

        if (withholding == null || (dto.EffectiveDate>DateOnly.FromDateTime(DateTime.UtcNow) && withholding.EffectiveDate>DateOnly.FromDateTime(DateTime.UtcNow)))
        {
            _context.EmployeeWithholding.Add(dto);
            await _documentManager.UploadDocumentAsync(dto.DocumentId, file,(DocumentTypeModel) dto.Field.DocumentTypeId);

            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateAsync(int id, EmployeeWithholdingModel model)
    {
        _scopedAuthorization.Validate(_httpContextAccessor.HttpContext.User, AuthorizationType.SystemAdmin);

        var emp = GetEmployeeWithholding(id) ?? throw new AppException("EmployeeWithholding not found");

        _mapper.Map(model, emp);
        _context.EmployeeWithholding.Update(emp);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        _scopedAuthorization.Validate(_httpContextAccessor.HttpContext.User, AuthorizationType.SystemAdmin);

        var emp = _context.EmployeeWithholding.Find(id);
        if (emp != null)
        {
            _context.EmployeeWithholding.Remove(emp);
            await _context.SaveChangesAsync();
        }
    }

    private EmployeeWithholding GetEmployeeWithholding(int id)
    {
        return _context.EmployeeWithholding.Find(id);

    }

    private Task<Person> GetCurrentUserAsync()
    {
        var currentUserEmail = _httpContextAccessor.HttpContext.User.Claims.First(e => e.Type == "email").Value.ToLower();
        var person = _context.Person.FirstOrDefault(e => e.Email.ToLower() == currentUserEmail) ?? throw new InvalidDataException("User not found");
        return Task.FromResult(person);
    }
}