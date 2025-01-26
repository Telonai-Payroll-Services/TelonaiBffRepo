using TelonaiWebApi.Helpers.Cache;
using TelonaiWebApi.Entities;
using static TelonaiWebApi.Services.StaticDataService;
using TelonaiWebApi.Models;

namespace TelonaiWebApi.Services
{
    public interface IStaticDataService
    {
        //Cities
        List<City> GetCitiesByStateId(int id);
        List<City> GetCitiesByCountryId(int id);
        City GetCityById(int id);


        //states
        List<State> GetStatesByCountryId(int id);
        State GetStateById(int id);
        State GetStateByName(string name, int countryId);

        //countries
        List<Country> GetCountries();
        Country GetCountryById(int id);
        Country GetCountryByName(string name);

        //zipcodes
        List<Zipcode> GetZipcodesByCityId(int id);
        List<Zipcode> GetZipcodesByCodeAndCountryId(string code, int countryId);
        Zipcode GetZipcodeById(int id);

        //business types
        List<BusinessType> GetBusinessTypes();
        List<ContactType> GetContactTypes();
        
        //Role types
        List<RoleType> GetRoleTypes();
        List<IncomeTaxRateModel> GetIncomeTaxRateModelsByCountryId(int countryId);
        List<IncomeTaxRate> GetIncomeTaxRatesByCountryId(int countryId);
        List<StateStandardDeduction> GetStateStandardDeductionsByStateId(int stateId, int year);
        List<IncomeTaxRate> GetIncomeTaxRatesByCountryIdAndPayrollYear(int countryId, int payrollYear);
        County GetCountyById(int id);
        List<County> GetCountyByStateId(int stateId);
        County GetCountyByNameAndStateId(string name, int stateId);
    }

    public class StaticDataService: IStaticDataService
    {
        private readonly ITelonaiCache _cache;
        private readonly ICityService _cityService;
        private readonly ICountryService _countryService;
        private readonly IStateService _stateService;
        private readonly IZipcodeService _zipcodeService;
        private readonly IBusinessTypeService _businessTypeService;
        private readonly IContactTypeService _contactTypeService;
        private readonly IRoleTypeService _roleTypeService;
        private readonly IHolidaysService _holidaysService;
        private readonly IIncomeTaxRateService<IncomeTaxRateModel,IncomeTaxRate> _incomeTaxService;
        private readonly IStateStandardDeductionService _stateStandardDeduction;
        private readonly ICountyService _countyService;

        public StaticDataService(ITelonaiCache cache, ICityService cityService, ICountryService countryService, IStateService stateService,
            IZipcodeService zipcodeService, IBusinessTypeService businessTypeService, IContactTypeService contactTypeService,
            IRoleTypeService roleTypeService, IHolidaysService holidaysService, IIncomeTaxRateService<IncomeTaxRateModel, IncomeTaxRate> incomeTaxService,
            IStateStandardDeductionService stateStandardDeductionService, ICountyService countyService)
        {
            _cache = cache;
            _cityService = cityService;
            _countryService = countryService;
            _stateService = stateService;
            _zipcodeService = zipcodeService;
            _businessTypeService = businessTypeService;
            _contactTypeService = contactTypeService;
            _roleTypeService = roleTypeService;
            _holidaysService = holidaysService;
            _incomeTaxService = incomeTaxService;
            _stateStandardDeduction = stateStandardDeductionService;
            _countyService = countyService;
        }


        public List<IncomeTaxRate> GetIncomeTaxRatesByCountryId(int countryId)
        {
            return _cache.Get<List<IncomeTaxRate>>($"IncomeTaxRates_countryId_{countryId}", f => _incomeTaxService.GetByCountryId(countryId).ToList());
        }

        public List<IncomeTaxRate> GetIncomeTaxRatesByCountryIdAndPayrollYear(int countryId, int payrollYear)
        {
            return _cache.Get<List<IncomeTaxRate>>($"IncomeTaxRates_countryId_{countryId}_PayrollYear_{payrollYear}", f => _incomeTaxService.GetByCountryIdAndPayrollYear(countryId, payrollYear).ToList());
        }

        public List<IncomeTaxRateModel> GetIncomeTaxRateModelsByCountryId(int countryId)
        {
            return _cache.Get<List<IncomeTaxRateModel>>($"IncomeTaxRateModels_countryId_{countryId}", f => _incomeTaxService.GetModelByCountryId(countryId).ToList());
        }

        public List<Country> GetCountries()
        {
            return _cache.Get<List<Country>>($"Counties", f => _countryService.GetAll().ToList());
        }
        public List<City> GetCitiesByStateId(int id)
        {
           return  _cache.Get<List<City>>($"City_StateId_{id}", f => _cityService.GetByStateId(id).ToList());
        }
        public List<City> GetCitiesByCountryId(int id)
        {
            return _cache.Get<List<City>>($"City_CountryId_{id}", f => _cityService.GetByCountryId(id).ToList());
        }
        public List<State> GetStatesByCountryId(int id)
        {
            return _cache.Get<List<State>>($"State_CountryId_{id}", f => _stateService.GetByCountryId(id).ToList());
        }
        public City GetCityById(int id)
        {
            return _cache.Get<City>($"City_{id}", f => _cityService.GetById(id));
        }

        public Country GetCountryById(int id)
        {
            return _cache.Get<Country>($"Country_{id}", f => _countryService.GetById(id));
        }
        public Country GetCountryByName(string name)
        {
            return _cache.Get<Country>($"Country_{name}", f => _countryService.GetByName(name));
        }

        public State GetStateById(int id)
        {
            return _cache.Get<State>($"State_{id}", f => _stateService.GetById(id));
        }
        public State GetStateByName(string name, int countryId)
        {
            return _cache.Get<State>($"State_{name}", f => _stateService.GetByName(name, countryId));
        }

        public List<Zipcode> GetZipcodesByCityId(int id)
        {
            return _cache.Get<List<Zipcode>>($"Zipcode_CityId_{id}", f => _zipcodeService.GetByCityId(id).ToList());
        }
        public List<Zipcode> GetZipcodesByCodeAndCountryId(string code, int countryId)
        {
            return _cache.Get<List<Zipcode>>($"Zipcode_{code}_{countryId}", f => _zipcodeService.GetByZipcodeAndCountryId(code, countryId).ToList());
        }
        public Zipcode GetZipcodeById(int id)
        {
            return _cache.Get<Zipcode>($"Zipcode_{id}", f => _zipcodeService.GetById(id));
        }
        public List<BusinessType> GetBusinessTypes()
        {
            return _cache.Get<List<BusinessType>>($"BusinessTypes", f => _businessTypeService.GetAll().ToList());
        }
        public List<ContactType> GetContactTypes()
        {
            return _cache.Get<List<ContactType>>($"ContactTypes", f => _contactTypeService.GetAll().ToList());
        }
        public List<RoleType> GetRoleTypes()
        {
            return _cache.Get<List<RoleType>>($"RoleTypes", f => _roleTypeService.GetAll().ToList());
        }

        public Dictionary<string,  DateOnly> GetHolidays(int countryId, int year)
        {
            return _cache.Get<Dictionary<string, DateOnly>>($"GetHolidays_{countryId}_{year}", f => _holidaysService.GetByCountryIdAndYear(countryId,year));
        }

        public List<StateStandardDeduction> GetStateStandardDeductionsByStateId(int stateId, int year) 
        {
            return _cache.Get<List<StateStandardDeduction>>($"StateStandardDeduction_{stateId}_{year}", f => _stateStandardDeduction.GetByStateId(stateId,year).ToList());

        }

        public County GetCountyById(int id)
        {
            return _cache.Get<County>($"County_{id}", f => _countyService.GetById(id).Result);
        }

        public List<County> GetCountyByStateId(int stateId)
        {
            return _cache.Get<List<County>>($"CountyByStateId_{stateId}", f => _countyService.GetByStateId(stateId).Result);
        }

        public County GetCountyByNameAndStateId(string name,int stateId)
        {
            return _cache.Get<County>($"CountyByNameAndStateId_{name}_{stateId}", f => _countyService.GetByNameAndStateId(name,stateId));
        }
    }
}
