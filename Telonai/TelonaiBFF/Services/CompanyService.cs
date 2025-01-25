namespace TelonaiWebApi.Services;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

public interface ICompanyService<CompanyModel, Company> : IDataService<CompanyModel, Company>
{
    CompanySummaryModel GetSummary(int companyId,int count);
    IList<JobModel> GetJobsById(int companyId);
}
    public class CompanyService : ICompanyService<CompanyModel, Company>
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IStaticDataService _staticDataService;
        private readonly IPayrollService _payrollService;
        private readonly ICompanyContactService _companyContactService;
        private readonly IPersonService<PersonModel,Person> _personService;
        private readonly IZipcodeService _zipCodeService;
        private readonly ICityService _cityService;
        private readonly IStateService _stateService;
        private readonly IEncryption _encryption;
    public CompanyService(DataContext context, IMapper mapper, IStaticDataService staticDataService, IPayrollService payrollService,
                             ICompanyContactService companyContactService, IPersonService<PersonModel, Person> personService,
                             IZipcodeService zipCodeService, ICityService cityService, IStateService stateService, IEncryption encryption)
        {
                _context = context;
                _mapper = mapper;
                _staticDataService = staticDataService;
                _payrollService = payrollService;
                _companyContactService = companyContactService;
                _personService = personService;
                _zipCodeService = zipCodeService;
                _cityService = cityService;
                _stateService = stateService;
                _encryption = encryption;
        }

        public IList<CompanyModel> Get()
        {
            var obj =  _context.Company
                .Include(e=>e.Zipcode).ThenInclude(e=>e.City).ThenInclude(e => e.State)
                .ThenInclude(e => e.Country)?.ToList();
            var result=_mapper.Map<IList<CompanyModel>>(obj);
            return result;
        }

        public IList<JobModel> GetJobsById(int companyId)
        {
            var obj = _context.Job.Where(e=>e.CompanyId==companyId)
                .Include(e => e.Zipcode).ThenInclude(e => e.City).ThenInclude(e => e.State)
                .ThenInclude(e => e.Country)?.ToList();
            var result = _mapper.Map<IList<JobModel>>(obj);
            return result;
        }
        public CompanySummaryModel GetSummary(int companyId, int count)
        {
            var payrolls = _payrollService.GetLatestByCount(companyId, count);
            var emps = _context.Employment.Where(e=>e.Job.CompanyId==companyId && !e.Deactivated).ToList();
       
            var summary = new CompanySummaryModel
            {
                NumberOfEmployees = emps.Count(),
                NumberOfLocations = emps.Select(e=>e.JobId).Distinct().Count(),
                Payrolls=payrolls
            };
            return summary;
        }

        public CompanyModel GetById(int id)
        {
            var obj = GetCompany(id);
            var result = _mapper.Map<CompanyModel>(obj);
            
            return result;
        }

        public async Task<Company> CreateAsync(CompanyModel model)
        {
            var existing = GetByTaxId(model.TaxId);
            if (existing == null || existing.Count() < 1)
            {
                var result = _mapper.Map<Company>(model);
                _context.Company.Add(result);
                await _context.SaveChangesAsync();
                return result;
            }

            return existing.First();
        }

        public async Task UpdateAsync(int id, CompanyModel model)
        {
            var company = GetCompany(id) ?? throw new AppException("Company not found");
            var companyContact = await _companyContactService.GetByCompanyId(id);
            if (company != null)
            {
                company.Name = string.IsNullOrEmpty(model.Name) ? company.Name : model.Name;
                company.AddressLine1 = string.IsNullOrEmpty(model.AddressLine1) ? company.AddressLine1 : model.AddressLine1;
                company.AddressLine2 = string.IsNullOrEmpty(model.AddressLine2) ? company.AddressLine2 : model.AddressLine2;
                if (model.ZipcodeId != null && model.ZipcodeId > 0)
                {
                    if (company.Zipcode == null)
                    {
                        company.Zipcode = _zipCodeService.GetById(model.ZipcodeId);
                        company.ZipcodeId = model.ZipcodeId;
                        company.Zipcode.City = _cityService.GetById(company.Zipcode.CityId);
                        company.Zipcode.City.State = _stateService.GetById(company.Zipcode.City.StateId);
                    }
                    else
                    {
                        company.ZipcodeId = model.ZipcodeId == 0 ? company.ZipcodeId : model.ZipcodeId;
                        company.Zipcode.CityId = model.CityId == 0 ? company.Zipcode.CityId : model.CityId;
                        company.Zipcode.City.StateId = model.StateId == 0 ? company.Zipcode.City.StateId : model.StateId;
                    }
                }
                if (companyContact != null)
                {
                    var person =  _personService.GetPersonById(companyContact.PersonId);
                    if (person != null)
                    {
                        person.FirstName = string.IsNullOrEmpty(model.ReprsentativeFirstName) ? person.FirstName : model.ReprsentativeFirstName;
                        person.LastName = string.IsNullOrEmpty(model.ReprsentativeLastName) ? person.LastName : model.ReprsentativeLastName;
                        person.MobilePhone = string.IsNullOrEmpty(model.MobilePhone) ? person.MobilePhone : model.MobilePhone;
                        _context.Person.Update(person);
                    }
                }

                if(!string.IsNullOrEmpty(model.TaxId) && model.TaxId!= company.TaxId)
                {
                    company.TaxId = _encryption.Encrypt(model.TaxId);
                }
                _context.Company.Update(company);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(int id)
        {
            var user = GetCompany(id) ?? throw new AppException("Company not found");
            _context.Company.Remove(user);
            await _context.SaveChangesAsync();
        }

        private Company GetCompany(int id)
        {
            return _context.Company.Include(z=>z.Zipcode).Include(c=>c.Zipcode.City).Include(c=>c.Zipcode.City.State).FirstOrDefault(c=>c.Id == id);
        }

        private IEnumerable<Company> GetByCompanyNameAndCity(string companyName, int cityId)
        {
            var city= _staticDataService.GetCityById(cityId);
            var countryId = city.State.CountryId;

            companyName = companyName.Trim().ToLower();
            var objs = _context.Company.Where(e => e.Name.ToLower() == companyName && e.Zipcode.City.State.CountryId==countryId);
            return objs;
        }
        private IEnumerable<Company> GetByTaxId(string taxId)
        {
            var objs = _context.Company.Where(e => e.TaxId  == taxId);
            return objs;
        }
        private IEnumerable<Company> GetByRegistrationNumberAndCity(string registrationNum, int cityId)
        {
            var city = _staticDataService.GetCityById(cityId);
            var objs = _context.Company.Where(e => e.RegistrationNumber.ToLower() == registrationNum.Trim().ToLower() && e.Zipcode.City.StateId == city.StateId);
            return objs;
        }
}