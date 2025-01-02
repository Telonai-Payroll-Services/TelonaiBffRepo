
namespace TelonaiWebApi.Services;

using AutoMapper;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;


public interface IFAQService
{
    Task<FAQ> CreateAsync(FAQModel person);
    Task<FAQ> Update(int id, FAQModel person);
    IList<FAQModel> Get();
    HelpPageResponse GetFAQAndContact();
    FAQModel GetById(int id);
    void Delete(int id);
}

public class FAQService : IFAQService
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public FAQService(DataContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _mapper = mapper;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<FAQ> CreateAsync(FAQModel person)
    {
        var obj = _mapper.Map<FAQ>(person);
        _context.FAQ.Add(obj);
        _context.SaveChanges();
        return obj;
    }

    public void Delete(int id)
    {
        var faq = _context.FAQ.Find(id);
        _context.FAQ.Remove(faq);
        _context.SaveChanges();
    }

    public IList<FAQModel> Get()
    {
        var obj = _context.FAQ;
        var result = _mapper.Map<IList<FAQModel>>(obj);
        return result;
    }

    public HelpPageResponse GetFAQAndContact()
    {
        var faq = _context.FAQ;
        var result = _mapper.Map<IList<FAQModel>>(faq);

        var contact = _context.TelonaiSpecificFieldValue;
        var _telonaiSpecificFieldValues = _mapper.Map<IList<TelonaiSpecificFieldValueModel>>(contact);

        var response = new HelpPageResponse()
        {
            TelonaiContact = _telonaiSpecificFieldValues.ToList(),
            Faqs = result.ToList()
        };

        return response;
    }

    public FAQModel GetById(int id)
    {
        var obj = _context.FAQ.Find(id);
        return _mapper.Map<FAQModel>(obj);
    }

    public async Task<FAQ> Update(int id, FAQModel request)
    {
        var faq = _context.FAQ.Find(id) ?? throw new AppException("FAQ not found");

        if (request.Question != null) faq.Question = request.Question;
        if (request.Answer != null) faq.Answer = request.Answer;

        _context.FAQ.Update(faq);
        _context.SaveChanges();

        return faq;
    }
}

