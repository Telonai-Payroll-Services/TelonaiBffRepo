using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;

namespace TelonaiWebApi.Services;

public interface IAgentService 
{
    Task<IList<AgentFieldValueModel>> Get();
    Task<AgentFieldValueModel> GetById(int id);
    Task<AgentFieldValue> CreateAsync(AgentFieldValueModel model);
    Task UpdateAsync(int id, AgentFieldValueModel model);
    Task DeleteAsync(int id);

}

public class AgentService : IAgentService
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IPersonService<PersonModel, Person> _personService;
    private readonly IScopedAuthorization _scopedAuthorization;
    public AgentService(DataContext context, IMapper mapper,
        IHttpContextAccessor httpContextAccessor,
    IPersonService<PersonModel, Person> personService,
                           IScopedAuthorization scopedAuthorization)
    {
        _context = context;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
        _personService = personService;
        _scopedAuthorization = scopedAuthorization;

    }
    public async Task<AgentFieldValue> CreateAsync(AgentFieldValueModel model)
    {

        var person = await _personService.GetCurrentUserAsync(); ;
        _scopedAuthorization.ValidateByCompanyId(_httpContextAccessor.HttpContext.User, AuthorizationType.User, person.CompanyId);

        if (model == null) 
        { 
            throw new ArgumentNullException(nameof(model));
        }
     
        /*var existingAgentFieldValue =  _context.AgentFieldValue.FirstOrDefault(x => x.Id == model.Id && x.PersonId == model.PersonId);

        if (existingAgentFieldValue != null) 
        { 
              return existingAgentFieldValue;
        } */
          
        var agentFieldValue = _mapper.Map<AgentFieldValue>(model);
        _context.AgentFieldValue.Add(agentFieldValue);

        await _context.SaveChangesAsync();
        return agentFieldValue;
      
    }

  
    public async Task DeleteAsync(int id)
    {

        var person = await _personService.GetCurrentUserAsync(); ;
        _scopedAuthorization.ValidateByCompanyId(_httpContextAccessor.HttpContext.User, AuthorizationType.User, person.CompanyId);

        var agentFieldValue = await _context.AgentFieldValue.FirstOrDefaultAsync(x => x.Id == id);
        if (agentFieldValue == null)
        {
            throw new KeyNotFoundException($"AgentFieldValue with Id {id} not found.");
        }

        _context.AgentFieldValue.Remove(agentFieldValue);
        await _context.SaveChangesAsync();
    }


    public async Task<IList<AgentFieldValueModel>> Get()
    {

        var person = await _personService.GetCurrentUserAsync(); ;
        _scopedAuthorization.ValidateByCompanyId(_httpContextAccessor.HttpContext.User, AuthorizationType.User, person.CompanyId);

        var agentFieldValues = _context.AgentFieldValue.Include(e => e.Person).ToList();
        return _mapper.Map<IList<AgentFieldValueModel>>(agentFieldValues);
    }


    public async Task<AgentFieldValueModel> GetById(int id)
    {

        var person = await _personService.GetCurrentUserAsync(); ;
        _scopedAuthorization.ValidateByCompanyId(_httpContextAccessor.HttpContext.User, AuthorizationType.User, person.CompanyId);

        var agentFieldValue = _context.AgentFieldValue.Include(e => e.Person).FirstOrDefault(x => x.Id == id);
        if (agentFieldValue == null)
        {
            throw new KeyNotFoundException($"AgentFieldValue with Id {id} not found.");
        }

        return _mapper.Map<AgentFieldValueModel>(agentFieldValue);
    }


    public async Task UpdateAsync(int id, AgentFieldValueModel model)
    {

        var person = await _personService.GetCurrentUserAsync(); ;
        _scopedAuthorization.ValidateByCompanyId(_httpContextAccessor.HttpContext.User, AuthorizationType.User, person.CompanyId);

        if (model == null)
        {
            throw new ArgumentNullException(nameof(model));
        }

        var agentFieldValue = await _context.AgentFieldValue.Include(e => e.Person).FirstOrDefaultAsync(x => x.Id == id);
        if (agentFieldValue == null)
        {
            throw new KeyNotFoundException($"AgentFieldValue with Id {id} not found.");
        }

        _mapper.Map(model, agentFieldValue);
        _context.AgentFieldValue.Update(agentFieldValue);
        await _context.SaveChangesAsync();
    }

}

