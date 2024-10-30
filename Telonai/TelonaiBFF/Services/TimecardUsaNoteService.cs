namespace TelonaiWebApi.Services;

using AutoMapper;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;

public interface ITimecardUsaNoteService
{
    IEnumerable<TimecardUsaNoteModel> GetByPersonAndTime(int personId, DateTime startTime, DateTime endTime);
    Task<List<TimecardUsaNoteModel>> GetNotesByPayrollId(int companyId, int payrollId);
    Task<List<TimecardUsaNoteModel>> GetNotesByTimeCardIds(int companyId, List<int> timeCardIds);
    IEnumerable<TimecardUsaNoteModel> GetByTimeCardId(int id);
    TimecardUsaNoteModel GetById(int id);
    void Create(TimecardUsaNoteModel model);
    void Update(int id, TimecardUsaNoteModel model);
    void Update(List<TimecardUsaNoteModel> models);
    void Delete(int id);
    void Create(List<TimecardUsaNoteModel> models);
}

public class TimecardUsaNoteService : ITimecardUsaNoteService
{
    private DataContext _context;
    private readonly IMapper _mapper;

    public TimecardUsaNoteService(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public IEnumerable<TimecardUsaNoteModel> GetByPersonAndTime(int personId, DateTime startTime, DateTime endTime)
    {
        var obj =  _context.TimecardUsaNote.Where(e=>e.TimecardUsa.PersonId==personId && e.CreatedDate>=startTime && e.CreatedDate<=endTime);        
        var result=_mapper.Map<IList<TimecardUsaNoteModel>>(obj);
        return result;
    }

    public IEnumerable<TimecardUsaNoteModel> GetByTimeCardId(int id)
    {
        var obj = _context.TimecardUsaNote.Where(e => e.TimecardUsaId == id);
        var result = _mapper.Map<IList<TimecardUsaNoteModel>>(obj);
        return result;
    }


    public async Task<List<TimecardUsaNoteModel>> GetNotesByPayrollId(int companyId, int payrollId)
    {
        var currentPayroll = _context.Payroll.Find(payrollId);

        var runDateTime = currentPayroll.ScheduledRunDate.ToDateTime(TimeOnly.MaxValue);
        var startDateTime = currentPayroll.StartDate.ToDateTime(TimeOnly.MinValue);

        var obj = _context.TimecardUsaNote.Where(e => e.TimecardUsa.ClockIn >= startDateTime && e.TimecardUsa.ClockIn < runDateTime
        && e.TimecardUsa.Job.CompanyId == companyId);

        var result = _mapper.Map<List<TimecardUsaNoteModel>>(obj);
        return result;
    }

    public async Task<List<TimecardUsaNoteModel>> GetNotesByTimeCardIds(int companyId, List<int> timecardIds)
    {
        var obj = _context.TimecardUsaNote.Where(e => timecardIds.Contains(e.TimecardUsa.Id)
        && e.TimecardUsa.Job.CompanyId == companyId);

        var result = _mapper.Map<List<TimecardUsaNoteModel>>(obj);
        return result;
    }

    public TimecardUsaNoteModel GetById(int id)
    {
        var obj = getTimecardUsaNote(id);
        var result = _mapper.Map<TimecardUsaNoteModel>(obj);
        return result;
    }

    public void Create(TimecardUsaNoteModel model)
    {
        var result = _mapper.Map<TimecardUsaNote>(model);
        
        _context.TimecardUsaNote.Add(result);
        _context.SaveChanges();
    }

    public void Update(int id, TimecardUsaNoteModel model)
    {
        var dto = getTimecardUsaNote(id);
           
        _mapper.Map(model, dto);
        _context.TimecardUsaNote.Update(dto);
        _context.SaveChanges();
    }
    public void Create(List<TimecardUsaNoteModel> models)
    {
        foreach (var model in models)
        {
           var result= _mapper.Map<TimecardUsaNote>(model);
           _context.TimecardUsaNote.Add(result);
            _context.SaveChanges();
        }
             
    }
    public void Update(List<TimecardUsaNoteModel> models)
    {
        foreach (var model in models)
        {
            var dto = getTimecardUsaNote(model.Id );
            _mapper.Map(model, dto);
            _context.TimecardUsaNote.Update(dto);
        }

        _context.SaveChanges();
    }

    public void Delete(int id)
    {
        var dto = getTimecardUsaNote(id);
        _context.TimecardUsaNote.Remove(dto);
        _context.SaveChanges();
    }

    private TimecardUsaNote getTimecardUsaNote(int id)
    {
        var dto = _context.TimecardUsaNote.Find(id);
        if (dto == null) throw new KeyNotFoundException("Note not found");
        return dto;
    }
}