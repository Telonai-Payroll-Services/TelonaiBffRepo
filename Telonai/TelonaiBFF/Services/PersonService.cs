namespace TelonaiWebApi.Services;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;

public interface IPersonService<Tmodel, Tdto> : IDataService<Tmodel, Tdto>
{
    void Update(int id, PersonModel model);
    Task<PersonModel> CreateAsync(Tdto person);
    PersonModel GetDetailsById(int id);
    Task<PersonModel> GetByEmailAsync(string email);
    IList<PersonModel> GetByCompanyId(int companyId);
    Task<PersonModel> GetByEmailAndCompanyIdAsync(string email, int companyId);
    Task<Person> GetCurrentUserAsync();

}

public class PersonService : IPersonService<PersonModel,Person>
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public PersonService(DataContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
    }
    public IList<PersonModel> GetByCompanyId(int companyId)
    {
        var obj = _context.Person.Where(e=>e.CompanyId==companyId && !e.Deactivated);
        var result = _mapper.Map<IList<PersonModel>>(obj);
        return result;
    }

    public IList<PersonModel> Get()
    {
        var obj =  _context.Person;        
        var result=_mapper.Map<IList<PersonModel>>(obj);
        return result;
    }

    public async Task<PersonModel> GetByEmailAndCompanyIdAsync(string email, int companyId)
    {
        var obj = await _context.Person.FirstOrDefaultAsync(e => e.Email == email && e.CompanyId==companyId && !e.Deactivated);
        var result = _mapper.Map<PersonModel>(obj);
        return result;
    }
    public async Task<PersonModel> GetByEmailAsync(string email)
    {
        var obj = await _context.Person.FirstOrDefaultAsync(e=>e.Email==email && !e.Deactivated);
        var result = _mapper.Map<PersonModel>(obj);
        return result;
    }

    public PersonModel GetById(int id)
    {
        var obj = GetPerson(id);
        var result = _mapper.Map<PersonModel>(obj);
        return result;
    }
    public PersonModel GetDetailsById(int id)
    {
        var obj = _context.Person.Include(e => e.Zipcode)
            .ThenInclude(e => e.City).ThenInclude(e => e.State).ThenInclude(e => e.Country)
            .FirstOrDefault(e => e.Id == id);

        var result = _mapper.Map<PersonModel>(obj);
        return result;
    }

    public async Task<Person> CreateAsync(PersonModel model)
    {
        if (_context.Person.Any(x => x.Email == model.Email && x.CompanyId == model.CompanyId && !x.Deactivated))
            throw new AppException("Account with the email '" + model.Email + "' already exists.");

        var p = _mapper.Map<Person>(model);
        _context.Person.Add(p);
        await _context.SaveChangesAsync();
        return p;
    }

    public async Task<PersonModel> CreateAsync(Person person)
    {
        var existing = _context.Person.FirstOrDefault(x => !x.Deactivated && x.Email == person.Email && x.CompanyId == person.CompanyId);
        if (existing == null)
        {
            _context.Person.Add(person);
            await _context.SaveChangesAsync();
            return _mapper.Map<PersonModel>(person);
        }
        return _mapper.Map<PersonModel>(existing);
    }

    public async Task UpdateAsync(int id, PersonModel model)
    {
        var user = GetPerson(id) ?? throw new AppException("Account not found.");
        _mapper.Map(model, user);

        _context.Person.Update(user);

        if (model.Ssn != null)
        {
            var emp = _context.Employment.FirstOrDefault(e => e.PersonId == id);
            if (emp != null && emp.SignUpStatusTypeId == (int)SignUpStatusTypeModel.UserProfileCreationStarted)
            {
                emp.SignUpStatusTypeId = (int)SignUpStatusTypeModel.UserProfileCreationCompleted;
                _context.Employment.Update(emp);
            }
        }
        _context.SaveChangesAsync();
    }

    public void Update(int id, PersonModel model)
    {
        var user = GetPerson(id) ?? throw new AppException("Account not found.");

        user.FirstName = string.IsNullOrWhiteSpace(model.FirstName) ? user.FirstName : model.FirstName;
        user.LastName = string.IsNullOrWhiteSpace(model.LastName) ? user.LastName : model.LastName;
        user.AddressLine1 = string.IsNullOrWhiteSpace(model.AddressLine1) ? user.AddressLine1 : model.AddressLine1;
        user.AddressLine2 = string.IsNullOrWhiteSpace(model.AddressLine2) ? user.AddressLine2 : model.AddressLine2;
        user.AddressLine3 = string.IsNullOrWhiteSpace(model.AddressLine3) ? user.AddressLine3 : model.AddressLine3;
        user.Ssn = string.IsNullOrWhiteSpace(model.Ssn) ? user.Ssn : model.Ssn;
        user.ZipcodeId = model.ZipcodeId == 0 ? user.ZipcodeId : model.ZipcodeId;

        _context.Person.Update(user);

        if (model.Ssn != null)
        {
            var emp = _context.Employment.FirstOrDefault(e => e.PersonId == id);
            if (emp != null && (emp.SignUpStatusTypeId==null || emp.SignUpStatusTypeId < (int)SignUpStatusTypeModel.UserProfileCreationCompleted))
            {
                emp.SignUpStatusTypeId = (int)SignUpStatusTypeModel.UserProfileCreationCompleted;
                _context.Employment.Update(emp);
            }               
        }
        _context.SaveChanges();
    }

    public async Task DeleteAsync(int id)
    {
        var user = GetPerson(id) ?? throw new AppException("Account not found");
        user.Deactivated = true;
        _context.Person.Update(user);
        await _context.SaveChangesAsync();
    }

    // helper methods

    private Person GetPerson(int id)
    {
        return _context.Person.Find(id);       
    }

    public async Task<Person> GetCurrentUserAsync()
    {
        try
        {
            var currentUserEmail = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(e => e.Type == "email").Value.ToLower();
            var person = await _context.Person.FirstOrDefaultAsync(e => e.Email.ToLower() == currentUserEmail) ?? throw new InvalidDataException("User not found");
            return person;
        }
        catch (Exception ex) 
        {
            throw new InvalidOperationException("An error occurred while retrieving prson", ex);
        }
        
    }
}