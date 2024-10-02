namespace TelonaiWebApi.Services;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;

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

    public CompanyService(DataContext context, IMapper mapper, IStaticDataService staticDataService, IPayrollService payrollService)
    {
        _context = context;
        _mapper = mapper;
        _staticDataService = staticDataService;
        _payrollService = payrollService;
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
        var user = GetCompany(id) ?? throw new AppException("Company not found");           

        _mapper.Map(model, user);
        _context.Company.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var user = GetCompany(id) ?? throw new AppException("Company not found");
        _context.Company.Remove(user);
        await _context.SaveChangesAsync();
    }

    private Company GetCompany(int id)
    {
        return _context.Company.Find(id);
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