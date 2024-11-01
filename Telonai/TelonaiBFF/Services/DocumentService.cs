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
using Microsoft.OpenApi.Extensions;
using Amazon.SimpleEmail.Model;

//using Amazon.Runtime.Documents;

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
    Task Update(Guid id, DocumentModel model);
    Task Delete(Guid id);
    Task<Tuple<Stream, string, DateOnly>> GetDocumentByDocumentTypeAndIdAsync(DocumentTypeModel documentType, Guid id);
    Task<Guid> SaveGeneratedUnsignedW4Pdf(string fileName, byte[]file);
    Task<byte[]> SetPdfFormFilds(W4Form model, Stream documentStream, string filingStatus);
    Tuple<string, string> GetSelectedFilingStatus(Models.FilingStatus filingStatus);
    Task CreateEmployeeWithholdingAsync(EmployeeWithholdingModel model);
    DateTime GetInvitationDateForEmployee(int id);
    Task<Guid> UpdateW4PdfWithSignature(Guid id, byte[] file);
    Task<DocumentModel> CreateDocumentModel(Guid documentId, string filename, DateOnly effectiveDate);
    EmployeeWithholdingModel CreateEmployeeWithholdingModel(Person person, Guid documentId, int fieldId, string fieldValue, DocumentModel documentModel);
    Task<List<EmployeeWithholdingModel>> CreatemployeeWithholdingModels( Guid documentId, string filingStatus, W4Form model, DocumentModel documentModel);
}

public class DocumentService : IDocumentService
{
    private readonly IDocumentManager _documentManager;
    private readonly DataContext _context;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IPersonService<PersonModel, Person> _personService;
    private readonly IScopedAuthorization _scopedAuthorization;
    private readonly IInvitationService<InvitationModel, Invitation> _invitationService;
    private readonly IEmploymentService<EmploymentModel, Employment> _employmentService;
    public DocumentService(DataContext context, IMapper mapper, IDocumentManager documentManager, 
                           IHttpContextAccessor httpContextAccessor, IPersonService<PersonModel, Person> personService, 
                           IScopedAuthorization scopedAuthorization, IInvitationService<InvitationModel, Invitation> invitationService
                           , IEmploymentService<EmploymentModel, Employment> employmentService)
    {
        _context = context;
        _mapper = mapper;
        _documentManager = documentManager;
        _httpContextAccessor = httpContextAccessor;
        _personService = personService;
        _scopedAuthorization = scopedAuthorization;
        _invitationService = invitationService;
        _employmentService = employmentService;
    }
    public  async Task<DocumentModel> GetOwnDocumentDetailsByDocumentTypeAsync(DocumentTypeModel documentType)
    {
        var person = await _personService.GetCurrentUserAsync(); ;
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

    public async Task Update(Guid id, DocumentModel model)
    {
        var document = await GetDocument(id) ?? throw new KeyNotFoundException("Document not found");
        var updatedDocument =  _mapper.Map<Document>(model);
        if ((int)updatedDocument.DocumentTypeId != document.DocumentTypeId)
        {
            document.DocumentTypeId = (int)updatedDocument.DocumentTypeId;
        }

        if (updatedDocument.FileName != document.FileName)
        {
            document.FileName = updatedDocument.FileName;
        }

        if (updatedDocument.PersonId != document.PersonId)
        {
            document.PersonId = updatedDocument.PersonId;
        }
        _context.Document.Update(document);
        await _context.SaveChangesAsync();
    }

    public async Task Delete(Guid id)
    {
        var dto = await GetDocument(id) ?? throw new KeyNotFoundException("Document not found"); ;
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
    public async Task<Tuple<Stream, string,DateOnly>> GetDocumentByDocumentTypeAndIdAsync(DocumentTypeModel documentType,Guid id)
    {
        var person = await _personService.GetCurrentUserAsync();
        _scopedAuthorization.ValidateByCompanyId(_httpContextAccessor.HttpContext.User, AuthorizationType.User, person.CompanyId);

        var dto = await _context.Document.OrderByDescending(e => e.CreatedDate).FirstOrDefaultAsync(e => e.Id == id);
        if (dto == null)
            return null;

        var document = await _documentManager.GetDocumentByTypeAndIdAsync(documentType.ToString(), dto.Id.ToString());

        return Tuple.Create(document, dto.FileName,dto.EffectiveDate);
    }
    public async Task<Guid> SaveGeneratedUnsignedW4Pdf(string fileName, byte[] file) 
    {
        var person =  await _personService.GetCurrentUserAsync();

        var fileBytes = file;
        var documentModel = new DocumentModel
        {
            FileName = person.FirstName+ person.MiddleName+ person.LastName+fileName,
            DocumentType = DocumentTypeModel.WFourUnsigned,
            PersonId = person.Id
        };
        using Stream stream = new MemoryStream(fileBytes);
        var dto = _mapper.Map<Document>(documentModel);
        dto.Id = Guid.NewGuid();

        await _context.Document.AddAsync(dto);
        await _documentManager.UploadDocumentAsync(dto.Id, stream, documentModel.DocumentType);
        await _context.SaveChangesAsync();
    
        return dto.Id;
    }
    public async Task<byte[]> SetPdfFormFilds(W4Form model, Stream documentStream,string filingStatus)
    {
        var person = await _personService.GetCurrentUserAsync();
        var firstName = person?.FirstName ?? model.Employee.FirstName;
        var middeName= person?.LastName ?? model.Employee.LastName;
        var middeNameInitial = !string.IsNullOrEmpty(middeName) ? middeName[0].ToString() : "";
        var lastName= person?.LastName ?? model.Employee.LastName;
        var ssn = person?.Ssn ?? model.Employee.SocialSecurityNumber;
        var address= person?.AddressLine1??model.Employee.Address;
        var zipCode = person?.Zipcode?.Code?? model.Employee.ZipCode;
        var cityOrTown = person?.Zipcode?.City?.Name ?? model.Employee.CityOrTown;
        var state=person?.Zipcode?.City?.State.Name ?? model.Employee.State;



        using (var workStream = new MemoryStream())
        {

            using (PdfReader pdfReader = new PdfReader(documentStream))
            using (PdfStamper pdfStamper = new PdfStamper(pdfReader, workStream))
            {
                AcroFields formFields = pdfStamper.AcroFields;

                formFields.SetField(PdfFields.Step1a_FirstName_MiddleInitial, $"{firstName} {middeNameInitial}");
                formFields.SetField(PdfFields.Step1a_LastName, lastName);
                formFields.SetField(PdfFields.Step1a_Address, address);
                formFields.SetField(PdfFields.Step1a_City_Or_Town_State_ZIPCode, $"{cityOrTown} {state} {zipCode}");
                formFields.SetField(PdfFields.Step1b_SocialSecurityNumber, ssn);
                formFields.SetField(filingStatus, "1");

                formFields.SetField(PdfFields.Step2_MultipleJobsOrSpouseWorks, model.MultipleJobsOrSpouseWorks ? "1" : "");

                formFields.SetField(PdfFields.Step3_Dependents_NumberOfChildrenUnder17, model.Dependents.NumberOfChildrenUnder17.ToString());
                formFields.SetField(PdfFields.Step3_Dependents_OtherDependents, model.Dependents.OtherDependents.ToString());
                formFields.SetField(PdfFields.Step3_TotalClaimedAmount, model.Dependents.TotalClaimedAmount.ToString());

                formFields.SetField(PdfFields.Step4a_OtherIncome, model.OtherIncome.ToString());
                formFields.SetField(PdfFields.Step4b_Deductions, model.Deductions.ToString());
                formFields.SetField(PdfFields.Step4c_ExtraWithholding, model.ExtraWithholding.ToString());

                formFields.SetField(PdfFields.MultipleJobsWorksheet_Step2b_1, model.MultipleJobsWorksheet.Step2b_1.ToString());
                formFields.SetField(PdfFields.MultipleJobsWorksheet_Step2b_2a, model.MultipleJobsWorksheet.Step2b_2a.ToString());
                formFields.SetField(PdfFields.MultipleJobsWorksheet_Step2b_2b, model.MultipleJobsWorksheet.Step2b_2b.ToString());
                formFields.SetField(PdfFields.MultipleJobsWorksheet_Step2b_2c, model.MultipleJobsWorksheet.Step2b_2c.ToString());
                formFields.SetField(PdfFields.MultipleJobsWorksheet_Step2b_3, model.MultipleJobsWorksheet.Step2b_3.ToString());
                formFields.SetField(PdfFields.MultipleJobsWorksheet_Step2b_4, model.MultipleJobsWorksheet.Step2b_4.ToString());

                formFields.SetField(PdfFields.DeductionsWorksheet_Step4b_1, model.DeductionsWorksheet.Step4b_1.ToString());
                formFields.SetField(PdfFields.DeductionsWorksheet_Step4b_2, model.DeductionsWorksheet.Step4b_2.ToString());
                formFields.SetField(PdfFields.DeductionsWorksheet_Step4b_3, model.DeductionsWorksheet.Step4b_3.ToString());
                formFields.SetField(PdfFields.DeductionsWorksheet_Step4b_4, model.DeductionsWorksheet.Step4b_4.ToString());
                formFields.SetField(PdfFields.DeductionsWorksheet_Step4b_5, model.DeductionsWorksheet.Step4b_5.ToString());

                pdfStamper.FormFlattening = true;
                pdfStamper.Close();
                pdfReader.Close();
            }


            var fileBytes = workStream.ToArray();
            return fileBytes;
        }

    }
    public Tuple<string, string> GetSelectedFilingStatus(Models.FilingStatus filingStatus)
    {
        int selectedCount = 0;
        string selectedFilingStatus = null;
        string selectedField = null;

        if (filingStatus.SingleOrMarriedFilingSeparately)
        {
            selectedCount++;
            selectedFilingStatus = PdfFields.Step1c_FilingStatus_SingleOrMarriedFilingSeparately;
            selectedField = nameof(PdfFields.Step1c_FilingStatus_SingleOrMarriedFilingSeparately);
        }
        if (filingStatus.MarriedFilingJointly)
        {
            selectedCount++;
            selectedFilingStatus = PdfFields.Step1c_FilingStatus_MarriedFilingJointly;
            selectedField = nameof(PdfFields.Step1c_FilingStatus_MarriedFilingJointly);
        }
        if (filingStatus.HeadOfHousehold)
        {
            selectedCount++;
            selectedFilingStatus = PdfFields.Step1c_FilingStatus_HeadOfHousehold;
            selectedField = nameof(PdfFields.Step1c_FilingStatus_HeadOfHousehold);
        }
        if (selectedCount != 1)
        {
            return null;
        }

        return Tuple.Create(selectedFilingStatus,selectedField);
    }
    public async Task CreateEmployeeWithholdingAsync(EmployeeWithholdingModel employee)
    {
        var person = await _personService.GetCurrentUserAsync();
        _scopedAuthorization.ValidateByCompanyId(_httpContextAccessor.HttpContext.User, AuthorizationType.User, person.CompanyId);
            var emp = await _context.Employment.FindAsync(employee.EmploymentId) ?? throw new InvalidDataException("User not found");


            var dto = _mapper.Map<EmployeeWithholding>(employee);
            var withholding = _context.EmployeeWithholding.OrderByDescending(e => e.EffectiveDate).FirstOrDefault(e => e.FieldId == employee.FieldId &&
            e.EmploymentId == employee.EmploymentId && !e.Deactivated && e.WithholdingYear == employee.WithholdingYear);

        if (withholding == null || (dto.EffectiveDate > DateOnly.FromDateTime(DateTime.UtcNow) && withholding.EffectiveDate > DateOnly.FromDateTime(DateTime.UtcNow)))
        {
            _context.EmployeeWithholding.Add(dto);

            await _context.SaveChangesAsync();
        }
        else if (withholding != null)
        {
            var employeeWithholding = await _context.EmployeeWithholding.FindAsync(withholding.Id) ?? throw new KeyNotFoundException("EmployeeWithholding not found");         

            if (employeeWithholding.FieldValue != dto.FieldValue)
             { 
            employeeWithholding.FieldValue = dto.FieldValue;
             }

            if (employeeWithholding.DocumentId != dto.DocumentId)
            { 
                employeeWithholding.DocumentId = dto.DocumentId;
            }
            if (employeeWithholding.WithholdingYear != dto.WithholdingYear)
            {
                employeeWithholding.WithholdingYear = dto.WithholdingYear;
            }
            if (employeeWithholding.EffectiveDate != dto.EffectiveDate)
            {
                employeeWithholding.EffectiveDate = dto.EffectiveDate;
            }

            _context.EmployeeWithholding.Update(employeeWithholding);
            await _context.SaveChangesAsync(); 
        }
       
    }
    public DateTime GetInvitationDateForEmployee(int id)
    {
       var person= _personService.GetById(id);
       var invitation = _invitationService.GetByInviteeEmail(person.Email).FirstOrDefault();
       var effectiveDate = invitation.CreatedDate;
       return effectiveDate;

    }
    public async Task<Guid> UpdateW4PdfWithSignature(Guid id, byte[] file)
    {
        var person = await _personService.GetCurrentUserAsync(); ;

        var documentModel = new DocumentModel
        {
            FileName = person.FirstName + person.MiddleName + person.LastName + DocumentTypeModel.WFour.GetDisplayName(),
            DocumentType = DocumentTypeModel.WFour,
            PersonId = person.Id
        };
        using Stream stream = new MemoryStream(file);

        var document = await GetDocument(id) ?? throw new KeyNotFoundException("Document not found");
        var updatedDocument = _mapper.Map<Document>(documentModel);
        if ((int)updatedDocument.DocumentTypeId != document.DocumentTypeId)
        {
            document.DocumentTypeId = (int)updatedDocument.DocumentTypeId;
        }

        if (updatedDocument.FileName != document.FileName)
        {
            document.FileName = updatedDocument.FileName;
        }
        _context.Document.Update(document);
        await _documentManager.UploadDocumentAsync(document.Id, stream, documentModel.DocumentType);
        await _context.SaveChangesAsync();

        return document.Id;
    }
   public EmployeeWithholdingModel CreateEmployeeWithholdingModel(Person person, Guid documentId, int fieldId, string fieldValue, DocumentModel documentModel)
    {
        
        var employments = _employmentService.GetByPersonId(person.Id).ToList();
        var employeeAtCompany = employments.FirstOrDefault(e => e.CompanyId == person.CompanyId);
        var effectiveDate = GetInvitationDateForEmployee(employeeAtCompany.PersonId);
        var employeeWithholdingModel= EmployeeWithholdingHelper.CreateEmployeeWithholdingModel(person, documentId, fieldId, fieldValue, documentModel, employeeAtCompany, effectiveDate);
        return employeeWithholdingModel;

    }
     public async Task<List<EmployeeWithholdingModel>> CreatemployeeWithholdingModels( Guid documentId,string filingStatus, W4Form model, DocumentModel documentModel)
    {
        var person = await _personService.GetCurrentUserAsync();
        var employeeWithHodingModel1C = CreateEmployeeWithholdingModel(person, documentId, 1, filingStatus, documentModel);
        var employeeWithHodingModel2C = CreateEmployeeWithholdingModel(person, documentId, 4, model.MultipleJobsOrSpouseWorks.ToString(), documentModel);
        var employeeWithHodingModel3 = CreateEmployeeWithholdingModel(person, documentId, 5, model.Dependents.TotalClaimedAmount.ToString(), documentModel);
        var employeeWithHodingModel4A = CreateEmployeeWithholdingModel(person, documentId, 7, model.OtherIncome.ToString(), documentModel);
        var employeeWithHodingModel4B = CreateEmployeeWithholdingModel(person, documentId, 8, model.Deductions.ToString(), documentModel);
        var employeeWithHodingModel4C = CreateEmployeeWithholdingModel(person, documentId, 9, model.ExtraWithholding.ToString(), documentModel);

        var employeeWithHodingModelList = new List<EmployeeWithholdingModel>
        {
         employeeWithHodingModel1C,
         employeeWithHodingModel2C,
         employeeWithHodingModel3,
         employeeWithHodingModel4A,
         employeeWithHodingModel4B,
         employeeWithHodingModel4C,
        };
        return employeeWithHodingModelList;
    }

    public async Task<DocumentModel> CreateDocumentModel(Guid documentId, string filename, DateOnly effectiveDate)
    {
        var person = await _personService.GetCurrentUserAsync();
        var documentModel=EmployeeWithholdingHelper.CreateDocumentModel(documentId, filename, person.Id, effectiveDate);
        return documentModel;
    }
}

