namespace TelonaiWebApi.Services;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;

public interface IStateStandardDeductionService 
{    List<StateStandardDeduction> GetByStateId(int stateId, int year);
}

public class StateStandardDeductionService : IStateStandardDeductionService
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;
    
    public StateStandardDeductionService(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public List<StateStandardDeduction> GetByStateId(int stateId, int year)
    {
        var dto = _context.StateStandardDeduction.Include(s => s.FilingStatus).OrderByDescending(e=>e.EffectiveYear).Where(e => e.StateId == stateId && e.EffectiveYear==year).ToList();

        return dto;
    }

    public async Task DeleteAsync(int id)
    {
        throw new NotImplementedException();
    }
    
}