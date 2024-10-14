namespace TelonaiWebApi.Services;

using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;
using iTextSharp.text.pdf.parser;
using iTextSharp.text.pdf;
using System.Text;

public interface IDocumentService
{
    Task<DocumentModel> GetOwnDocumentDetailsByDocumentTypeAsync(DocumentTypeModel documentType);
    Task<DocumentModel> GetDocumentDetailsByDocumentTypeAsync(DocumentTypeModel documentType);
    Task<Tuple<Stream, string>> GetOwnDocumentByDocumentTypeAsync(DocumentTypeModel documentType);
    Task<Tuple<Stream, string>> GetDocumentByDocumentTypeAsync(DocumentTypeModel documentType);

    Task<Tuple<Stream, string>> GetOwnDocumentByDocumentIdAsync(Guid documentId);
    Task<Tuple<Stream, string>> GetDocumentByDocumentIdAsync(Guid documentId);
    Task<DocumentModel> GetOwnDocumentDetailsByDocumentIdAsync(Guid documentId);
    Task<DocumentModel> GetDocumentDetailsByDocumentIdAsync(Guid documentId);
    Task<Document> GetDocument(Guid id);
    Task UploadInternalDocumentAsync(DocumentTypeModel documentType);
    Task CreateAsync(DocumentModel model, Stream file);
    Task AddGovernmentDocumentAsync(Stream file, DocumentTypeModel documentType);
    void Update(Guid id, DocumentModel model);
    void Delete(Guid id);
}

public class DocumentService : IDocumentService
{
    private readonly IDocumentManager _documentManager;
    private readonly DataContext _context;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IPersonService<PersonModel, Person> _personService;
    private readonly IScopedAuthorization _scopedAuthorization;
    public DocumentService(DataContext context, IMapper mapper, IDocumentManager documentManager, 
                           IHttpContextAccessor httpContextAccessor, IPersonService<PersonModel, Person> personService, 
                           IScopedAuthorization scopedAuthorization)
    {
        _context = context;
        _mapper = mapper;
        _documentManager = documentManager;
        _httpContextAccessor = httpContextAccessor;
        _personService = personService;
        _scopedAuthorization = scopedAuthorization;
    }
    public  async Task<DocumentModel> GetOwnDocumentDetailsByDocumentTypeAsync(DocumentTypeModel documentType)
    {
        var person = await _personService.GetCurrentUserAsync();
        _scopedAuthorization.ValidateByCompanyId(_httpContextAccessor.HttpContext.User, AuthorizationType.User, person.CompanyId);
        var dto =  _context.Document.OrderByDescending(e => e.CreatedDate).FirstOrDefault(e => e.PersonId == person.Id && e.DocumentTypeId == (int)documentType);

        if (dto == null)
            return null;

        var result = _mapper.Map<DocumentModel>(dto);

        return result;
    }

    public async Task<Tuple<Stream, string>> GetOwnDocumentByDocumentTypeAsync(DocumentTypeModel documentType)
    {
        var person = await _personService.GetCurrentUserAsync();
        _scopedAuthorization.ValidateByCompanyId(_httpContextAccessor.HttpContext.User, AuthorizationType.User, person.CompanyId);

        var dto =  _context.Document.OrderByDescending(e => e.CreatedDate).FirstOrDefault(e => e.PersonId == person.Id
        && e.DocumentTypeId == (int)documentType);

        if (dto == null)
            return null;

        var document = await _documentManager.GetDocumentByTypeAndIdAsync(documentType.ToString(), dto.Id.ToString());

        return Tuple.Create(document,dto.FileName);
    }

    public async Task<Tuple<Stream, string>> GetOwnDocumentByDocumentIdAsync(Guid documentId)
    {
        var person = await _personService.GetCurrentUserAsync();
        _scopedAuthorization.ValidateByCompanyId(_httpContextAccessor.HttpContext.User, AuthorizationType.User, person.CompanyId);

        var dto = await _context.Document.FindAsync(documentId);
        if (dto == null || dto.PersonId != person.Id)
            return null;

        var result = _mapper.Map<DocumentModel>(dto);

        var document = await _documentManager.GetDocumentByTypeAndIdAsync(dto.DocumentType.Value, documentId.ToString());
        return Tuple.Create(document, dto.FileName);
    }

    public async Task<DocumentModel> GetOwnDocumentDetailsByDocumentIdAsync(Guid documentId)
    {
        var person = await _personService.GetCurrentUserAsync();
        _scopedAuthorization.ValidateByCompanyId(_httpContextAccessor.HttpContext.User, AuthorizationType.User, person.CompanyId);

        var dto = await _context.Document.FindAsync(documentId);
        if (dto == null || dto.PersonId!=person.Id)
            return null;

        var result = _mapper.Map<DocumentModel>(dto);
        return result;
    }

    public async Task<Tuple<Stream, string>> GetDocumentByDocumentIdAsync(Guid documentId)
    {
        var person = await _personService.GetCurrentUserAsync();
        _scopedAuthorization.ValidateByCompanyId(_httpContextAccessor.HttpContext.User, AuthorizationType.Admin, person.CompanyId);

        var dto = await _context.Document.FindAsync(documentId);
        if (dto == null)
            return null;

        var document = await _documentManager.GetDocumentByTypeAndIdAsync(dto.DocumentType.ToString(), dto.Id.ToString());

        return Tuple.Create(document, dto.FileName);
    }

    public async Task<DocumentModel> GetDocumentDetailsByDocumentIdAsync(Guid documentId)
    {
        var person = await _personService.GetCurrentUserAsync();
        _scopedAuthorization.ValidateByCompanyId(_httpContextAccessor.HttpContext.User, AuthorizationType.Admin, person.CompanyId);

        var dto = await _context.Document.FindAsync(documentId);
        if (dto == null)
            return null;

        var result = _mapper.Map<DocumentModel>(dto);
        return result;
    }

    public async Task<DocumentModel> GetDocumentDetailsByDocumentTypeAsync(DocumentTypeModel documentType)
    {
        var person = await _personService.GetCurrentUserAsync();
        _scopedAuthorization.ValidateByCompanyId(_httpContextAccessor.HttpContext.User, AuthorizationType.Admin, person.CompanyId);

        var dto = await _context.Document.OrderByDescending(e => e.CreatedDate).FirstOrDefaultAsync(e => e.DocumentTypeId == (int)documentType);

        if (dto == null)
            return null;

        var result = _mapper.Map<DocumentModel>(dto);

        return result;
    }
    public async Task<Tuple<Stream, string>> GetDocumentByDocumentTypeAsync(DocumentTypeModel documentType)
    {
        var person = await _personService.GetCurrentUserAsync();
        _scopedAuthorization.ValidateByCompanyId(_httpContextAccessor.HttpContext.User, AuthorizationType.Admin, person.CompanyId);

        var dto = await _context.Document.OrderByDescending(e => e.CreatedDate).FirstOrDefaultAsync(e => e.DocumentTypeId == (int)documentType);
        if (dto == null)
            return null;

        var document = await _documentManager.GetDocumentByTypeAndIdAsync(documentType.ToString(), dto.Id.ToString());

        return Tuple.Create(document, dto.FileName);
    }

    public async Task CreateAsync(DocumentModel model, Stream file)
    {
        var person = _context.Person.Find(model.PersonId) ?? throw new InvalidDataException("PersonId Missing");
        _scopedAuthorization.ValidateByCompanyId(_httpContextAccessor.HttpContext.User, AuthorizationType.User, person.CompanyId);
                
        var dto = _mapper.Map<Document>(model);
        dto.Id=Guid.NewGuid();

        await _context.Document.AddAsync(dto);
        await _documentManager.UploadDocumentAsync(dto.Id,file,model.DocumentType);
        await _context.SaveChangesAsync();
    }

    public async Task AddGovernmentDocumentAsync(Stream file, DocumentTypeModel documentType)
    {
        _scopedAuthorization.Validate(_httpContextAccessor.HttpContext.User, AuthorizationType.SystemAdmin);

        if (!documentType.ToString().EndsWith("Unsigned"))
            throw new InvalidDataException("Document should be the original document provided by authorities.");

        var dto = new Entities.Document
        {
            Id = Guid.NewGuid(),
            DocumentTypeId = (int)documentType,
            FileName = documentType.ToString()
        };
        _context.Document.Add(dto);
        await _context.SaveChangesAsync();
        await _documentManager.UploadDocumentAsync(dto.Id, file, documentType);

    }

    public async void Update(Guid id, DocumentModel model)
    {
        var dto = await GetDocument(id) ?? throw new KeyNotFoundException("Document not found");
        _context.Document.Update(dto);
        _context.SaveChanges();
    }

    public async void Delete(Guid id)
    {
        var dto = await GetDocument(id);
        _context.Document.Remove(dto);
        _context.SaveChanges();
    }

    public async Task<Document> GetDocument(Guid id)
    {
        var dto = await _context.Document.FindAsync(id);
        return dto;
    }

    public async Task UploadInternalDocumentAsync(DocumentTypeModel documentType)
    {
        string fileName;
        switch (documentType)
        {
            case DocumentTypeModel.WFourUnsigned:
                fileName = "Resources/w4.pdf";
                break;
            case DocumentTypeModel.NCFourUnsigned:
                fileName = "Resources/nc4.pdf";
                break;
            case DocumentTypeModel.INineUnsigned:
                fileName = "Resources/i-9.pdf";
                break;
            default: throw new KeyNotFoundException();
        }
        if (File.Exists(fileName))
        {
            using Stream stream = new MemoryStream(System.IO.File.ReadAllBytes(fileName));
            await AddGovernmentDocumentAsync(stream, documentType);
        }
    }
    private void ReadPdfDocument(string fileName)
    {
        StringBuilder text = new();

        if (File.Exists(fileName))
        {
            PdfReader pdfReader = new PdfReader(fileName);

            for (int page = 1; page <= pdfReader.NumberOfPages; page++)
            {
                ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                string currentText = PdfTextExtractor.GetTextFromPage(pdfReader, page, strategy);

                currentText = Encoding.UTF8.GetString(ASCIIEncoding.Convert(Encoding.Default, Encoding.UTF8, Encoding.Default.GetBytes(currentText)));
                text.Append(currentText);
            }
            pdfReader.Close();
        }
        var result= text.ToString();
    }

    
}

