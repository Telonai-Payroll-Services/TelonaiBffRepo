namespace TelonaiWebApi.Services;

using AutoMapper;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;

public interface IIncomeTaxRateService<IncomeTaxRateModel, IncomeTaxRate> : IDataService<IncomeTaxRateModel, IncomeTaxRate>
{
    IList<IncomeTaxRateModel> GetModelByCountryId(int countryId);
    IList<IncomeTaxRate> GetByCountryId(int countryId);

}

public class IncomeTaxRateService : IIncomeTaxRateService<IncomeTaxRateModel, IncomeTaxRate>
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;
    
    public IncomeTaxRateService(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    public IList<IncomeTaxRateModel> Get()
    {
        var dto = _context.IncomeTaxRate;
        return _mapper.Map<IList<IncomeTaxRateModel>>(dto);
    }

    public IncomeTaxRateModel GetById(int id)
    {
        var dto = _context.IncomeTaxRate.Find(id);
        return _mapper.Map<IncomeTaxRateModel>(dto);
    }

    public IList<IncomeTaxRateModel> GetModelByCountryId(int countryId)
    {
        var dto =  _context.IncomeTaxRate.Where(e=> e.IncomeTaxType.CountryId==countryId).ToList();
        return _mapper.Map<List<IncomeTaxRateModel>>(dto);
    }

    public IList<IncomeTaxRate> GetByCountryId(int countryId)
    {
        try
        {
            var dto = _context.IncomeTaxRate.Where(e => e.IncomeTaxType.CountryId == countryId && e.EffectiveDate.Year <= DateTime.Now.Year).ToList();

            return dto;
        }
        catch(Exception ex)
        { return null; }
    }

    public async Task<IncomeTaxRate> CreateAsync(IncomeTaxRateModel model)
    {
        throw new NotImplementedException();
    }

    public async Task UpdateAsync(int id, IncomeTaxRateModel model)
    {
        throw new NotImplementedException();
    }

    public async Task DeleteAsync(int id)
    {
        throw new NotImplementedException();
    }

}