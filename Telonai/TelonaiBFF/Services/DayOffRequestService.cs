namespace TelonaiWebApi.Services;

using AutoMapper;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;

public interface IDayOffRequestService<Tmodel, Tdto> : IDataService<Tmodel, Tdto>
{
    List<DayOffRequestModel> GetByEmploymentId(int employmentIf);
    List<DayOffRequestModel> GetByPersonId(int personId);

}

public class DayOffRequestService : IDayOffRequestService<DayOffRequestModel,DayOffRequest>
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public DayOffRequestService(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public IList<DayOffRequestModel> Get()
    {
        var obj =  _context.DayOffRequest;        
        var result=_mapper.Map<IList<DayOffRequestModel>>(obj);
        return result;
    }

    public DayOffRequestModel GetById(int id)
    {
        var obj = GetDayOff(id);
        var result = _mapper.Map<DayOffRequestModel>(obj);
        return result;
    }
    public List<DayOffRequestModel> GetByEmploymentId(int employmentId)
    {
        var obj = _context.DayOffRequest.Where(e=>e.EmploymentId== employmentId && !e.IsCancelled)?.ToList();

        var result = _mapper.Map<List<DayOffRequestModel>>(obj);
        return result;
    }

    public List<DayOffRequestModel> GetByPersonId(int personId)
    {
        var obj = _context.DayOffRequest.Where(e => e.Employment.PersonId == personId && !e.IsCancelled)?.ToList();
        var result = _mapper.Map<List<DayOffRequestModel>>(obj);
        return result;
    }

    public async Task<DayOffRequest> CreateAsync(DayOffRequestModel model)
    {
        var dayOff = _context.DayOffRequest.FirstOrDefault(e => e.EmploymentId == model.EmploymentId
        && !e.IsCancelled && model.FromDate <= e.ToDate && e.FromDate <= model.ToDate);

        if (dayOff == null)
        {
            dayOff = _mapper.Map<DayOffRequest>(model);
            _context.DayOffRequest.Add(dayOff);
            _context.SaveChanges();
            return dayOff;
        }

        throw new AppException("You already have a day-off request in the range you selected.");
    }

    public async Task UpdateAsync(int id, DayOffRequestModel model)
    {
        throw new NotImplementedException();
    }

    public async Task DeleteAsync(int id)
    {
        var dayOff = GetDayOff(id) ?? throw new AppException("Day-off request not found");
        dayOff.IsCancelled = true;
        _context.DayOffRequest.Update(dayOff);
        await _context.SaveChangesAsync();
    }

    private DayOffRequest GetDayOff(int id)
    {
        return _context.DayOffRequest.Find(id);

    }
}