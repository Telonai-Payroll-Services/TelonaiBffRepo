namespace TelonaiWebApi.Services;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using Npgsql;
using System.Threading.Tasks;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;

public interface IPersonService<Tmodel, Tdto> : IDataService<Tmodel, Tdto>
{
    void Update(int id, PersonModel model);
    Task<PersonModel> CreateAsync(Tdto person);
    PersonModel GetDetailsById(int id);
    Task<List<PersonModel>> GetListByEmailAsync(string email);
    Task<PersonModel> GetByEmailAsync(string email);
    IList<PersonModel> GetByCompanyId(int companyId);
    IList<PersonModel> GetIncompleteInineByCompanyId(int companyId);
    Task<PersonModel> GetByEmailAndCompanyIdAsync(string email, int companyId);
    Task<Person> GetCurrentUserAsync();
    Person GetPersonById(int Id);
    Task DeleteUserDataByEmailAsync(string email);
    bool IsEmployeeMinor(DateOnly dateOfBirth);
}

public class PersonService : IPersonService<PersonModel,Person>
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IZipcodeService _zipCodeService;
    private readonly ICityService _cityService;
    private readonly IStateService _stateService;
    public PersonService(DataContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor, IZipcodeService zipCodeService, ICityService cityService, IStateService stateService)
    {
        _context = context;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
        _zipCodeService = zipCodeService;
        _cityService = cityService;
        _stateService = stateService;
    }
    public IList<PersonModel> GetByCompanyId(int companyId)
    {
        var obj = _context.Person.Where(e=>e.CompanyId==companyId && !e.Deactivated);
        var result = _mapper.Map<IList<PersonModel>>(obj);
        return result;
    }
    public IList<PersonModel> GetIncompleteInineByCompanyId(int companyId)
    {
        var completeStatusId = (int)INineVerificationStatusModel.INineSectionTwoSubmitted;

        var jobIds = _context.Job
            .Where(e => e.CompanyId == companyId)
            .Select(e => e.Id)
            .ToList();

        var filteredPersons = _context.Person
            .Where(p => p.CompanyId == companyId
                        && p.INineVerificationStatusId < completeStatusId
                        && !p.Deactivated
                        && !_context.Employment
                            .Any(e => jobIds.Contains(e.JobId) && e.PersonId == p.Id && e.Deactivated))
            .ToList();

        var result = _mapper.Map<IList<PersonModel>>(filteredPersons);
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
        var obj = _context.Person.FirstOrDefault(e=>e.Email.ToLower() == email.ToLower() && !e.Deactivated);
        var result = _mapper.Map<PersonModel>(obj);
        return result;
    }

    public async Task<List<PersonModel>> GetListByEmailAsync(string email)
    {
        var obj = _context.Person.Where(e => e.Email == email && !e.Deactivated).ToList();
        var result = _mapper.Map<List<PersonModel>>(obj);
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
        await _context.SaveChangesAsync();
    }

    public void Update(int id, PersonModel model)
    {
        var person = GetPerson(id) ?? throw new AppException("Account not found.");
        if (person != null)
        {
            person.FirstName = string.IsNullOrWhiteSpace(model.FirstName) ? person.FirstName : model.FirstName;
            person.LastName = string.IsNullOrWhiteSpace(model.LastName) ? person.LastName : model.LastName;
            person.AddressLine1 = string.IsNullOrWhiteSpace(model.AddressLine1) ? person.AddressLine1 : model.AddressLine1;
            person.AddressLine2 = string.IsNullOrWhiteSpace(model.AddressLine2) ? person.AddressLine2 : model.AddressLine2;
            person.MobilePhone = string.IsNullOrEmpty(model.MobilePhone) ? person.MobilePhone : model.MobilePhone;
            person.Ssn = string.IsNullOrWhiteSpace(model.Ssn) ? person.Ssn : model.Ssn;
            if (model.ZipcodeId > 0)
            {
                if (person.Zipcode == null)
                {
                    person.Zipcode = _zipCodeService.GetById(model.ZipcodeId);
                    person.ZipcodeId = model.ZipcodeId;
                    person.CountyId = model.CountyId;
                    person.Zipcode.City = _cityService.GetById(person.Zipcode.CityId);
                    person.Zipcode.City.State = _stateService.GetById(person.Zipcode.City.StateId);
                }
                else
                {
                    person.ZipcodeId = model.ZipcodeId == 0 ? person.ZipcodeId : model.ZipcodeId;
                    person.Zipcode.CityId = model.CityId == 0 ? person.Zipcode.CityId : model.CityId;
                    person.Zipcode.City.StateId = model.StateId == 0 ? person.Zipcode.City.StateId : model.StateId;
                    person.CountyId = model.CountyId == 0 ? person.CountyId : model.CountyId;

                }
            }
            if (model.Ssn != null)
            {
                var emp = _context.Employment.FirstOrDefault(e => e.PersonId == id);
                if (emp != null && (emp.SignUpStatusTypeId == null || emp.SignUpStatusTypeId < (int)SignUpStatusTypeModel.UserProfileCreationCompleted))
                {
                    emp.SignUpStatusTypeId = (int)SignUpStatusTypeModel.UserProfileCreationCompleted;
                    _context.Employment.Update(emp);
                }
            }
            _context.Person.Update(person);
            _context.SaveChanges();
        }
    }

    public async Task DeleteAsync(int id)
    {
        var user = GetPerson(id) ?? throw new AppException("Account not found");
        user.Deactivated = true;
        _context.Person.Update(user);
        await _context.SaveChangesAsync();
    }

    public Person GetPersonById(int Id)
    {
        var result = _context.Person.Include(z => z.Zipcode).Include(c => c.Zipcode.City).FirstOrDefault(p => p.Id == Id);
        return result;
    }

    // helper methods

    private Person GetPerson(int id)
    {
        return _context.Person.Include(z=>z.Zipcode).Include(c=>c.Zipcode.City).FirstOrDefault(p=>p.Id == id);       
    }

    public async Task<Person> GetCurrentUserAsync()
    {
            var currentUserEmail = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(e => e.Type == "email").Value.ToLower();
            var person = await _context.Person.FirstOrDefaultAsync(e => e.Email.ToLower() == currentUserEmail) ?? throw new InvalidDataException("User not found");
            return person;       
        
    }
    public async Task DeleteUserDataByEmailAsync(string email)
    {
        var commandText = "CALL delete_user_data_by_email(@email)";
        var parameters = new List<NpgsqlParameter>
        {
            new NpgsqlParameter("@email", email)
        };
        await _context.Database.ExecuteSqlRawAsync(commandText, parameters.ToArray());
    }


    public bool IsEmployeeMinor(DateOnly dateOfBirth)
    {
        var yearDifference = DateTime.Today.Subtract(dateOfBirth.ToDateTime(TimeOnly.MinValue));
        if((yearDifference.TotalDays / 365.25) <= 17)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}