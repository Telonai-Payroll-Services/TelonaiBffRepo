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
using Document = Entities.Document;
using FilingStatus = Models.FilingStatus;
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
    Tuple<string, string> GetSelectedFilingStatus(Models.FilingStatus filingStatus);
    DateTime GetInvitationDateForEmployee(int id);
    Task<Guid> UpdateW4PdfWithSignature(Guid id, byte[] file, DocumentTypeModel document);
    Task<W4PdfResult> GenerateW4pdf(int employmentId, W4Form model);
    Task<byte[]> SignW4DoumentAsync(Guid id, int employmentId, SignatureModel signature);
    Task<Guid> Confirm(Guid id);
    Task<W4PdfResult> GenerateNC4pdf(int employmentId, NC4Form model);
    Task<byte[]> SignNC4DoumentAsync(Guid id, int employmentId, SignatureModel signature);
    Task<List<DocumentType>> GetDocumentTypes();
}

public class DocumentService : IDocumentService
{
    private Person _person = null;
    private readonly IDocumentManager _documentManager;
    private readonly DataContext _context;
    private readonly IMapper _mapper;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IPersonService<PersonModel, Person> _personService;
    private readonly IScopedAuthorization _scopedAuthorization;
    private readonly IInvitationService<InvitationModel, Invitation> _invitationService;

    public DocumentService(DataContext context, IMapper mapper, IDocumentManager documentManager, 
                           IHttpContextAccessor httpContextAccessor, IPersonService<PersonModel, Person> personService, 
                           IScopedAuthorization scopedAuthorization, IInvitationService<InvitationModel, Invitation> invitationService)
    {
        _context = context;
        _mapper = mapper;
        _documentManager = documentManager;
        _httpContextAccessor = httpContextAccessor;
        _personService = personService;
        _scopedAuthorization = scopedAuthorization;
        _invitationService = invitationService;
    }
    public  async Task<DocumentModel> GetOwnDocumentDetailsByDocumentTypeAsync(DocumentTypeModel documentType)
    {
        var person = await _personService.GetCurrentUserAsync(); ;
        _scopedAuthorization.ValidateByCompanyId(_httpContextAccessor.HttpContext.User, AuthorizationType.User, _person.CompanyId);
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

        var dto = await _context.Document.FindAsync(id);
        if (dto == null)
            return null;      

        var document = await _documentManager.GetDocumentByTypeAndIdAsync(documentType.ToString(), dto.Id.ToString());

        return Tuple.Create(document, dto.FileName,dto.EffectiveDate);
    }
    private async Task<Guid> SaveGeneratedUnsignedW4Pdf(string fileName, byte[] file, DocumentTypeModel documentType) 
    {

        var fileBytes = file;
        var documentModel = new DocumentModel
        {
            FileName = _person.FirstName+ _person.MiddleName+ _person.LastName+fileName,
            DocumentType = documentType,
            PersonId = _person.Id
        };
        using Stream stream = new MemoryStream(fileBytes);
        var dto = _mapper.Map<Document>(documentModel);
        dto.Id = Guid.NewGuid();

        await _context.Document.AddAsync(dto);
        await _documentManager.UploadDocumentAsync(dto.Id, stream, documentModel.DocumentType);
        await _context.SaveChangesAsync();
    
        return dto.Id;
    }
    private async Task<byte[]> SetPdfFormFilds(W4Form model, Stream documentStream,string filingStatus,Employment employee,bool formFlattening)
    {
        var firstName = _person?.FirstName ;
        var middeName= _person?.LastName ;
        var middeNameInitial = !string.IsNullOrEmpty(middeName) ? middeName[0].ToString() : "";
        var lastName= _person?.LastName ;
        var ssn = _person?.Ssn;
        var address= _person?.AddressLine1;
        var zipCode = _person?.Zipcode?.Code;
        var cityOrTown = _person?.Zipcode?.City?.Name ;
        var state=_person?.Zipcode?.City?.State.Name ;
        var company = _context.Company.FirstOrDefault(e => e.Id == employee.Person.CompanyId);

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

                formFields.SetField(filingStatus, "On");
               
                var multipleJobsOrSpouseWorks = model.MultipleJobs || model.SpouseWorks;

                formFields.SetField(PdfFields.Step2_MultipleJobsOrSpouseWorks, multipleJobsOrSpouseWorks ? "On" : "");
                var totalClaimedAmount = model.NumberOfChildrenUnder17 * 2000 + model.OtherDependents * 500;

                formFields.SetField(PdfFields.Step3_Dependents_NumberOfChildrenUnder17, model.NumberOfChildrenUnder17.ToString());
                formFields.SetField(PdfFields.Step3_Dependents_OtherDependents, model.OtherDependents.ToString());
                formFields.SetField(PdfFields.Step3_TotalClaimedAmount, totalClaimedAmount.ToString());

                formFields.SetField(PdfFields.Step4a_OtherIncome, model.OtherIncome.ToString());
                formFields.SetField(PdfFields.Step4b_Deductions, model.Deductions.ToString());
                formFields.SetField(PdfFields.Step4c_ExtraWithholding, model.ExtraWithholding.ToString());

                formFields.SetField(PdfFields.EmployerNameAndAddress, company.Name+""+company.Zipcode+""+ company.AddressLine1);
                formFields.SetField(PdfFields.EmployerFirstDateOfEmployement, employee.CreatedDate.ToShortDateString());              
                formFields.SetField(PdfFields.EmployerIdentificationNumber, company.TaxId);
                pdfStamper.FormFlattening = formFlattening;
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
    private async Task CreateEmployeeWithholdingAsync(EmployeeWithholdingModel employee)
    {
        var dto = _mapper.Map<EmployeeWithholding>(employee);
        var withholding = _context.EmployeeWithholding.OrderByDescending(e => e.EffectiveDate)
            .FirstOrDefault(e => e.FieldId == employee.FieldId && e.EmploymentId == employee.EmploymentId &&
            !e.Deactivated && e.WithholdingYear == employee.WithholdingYear);

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
    public async Task<byte[]> SignW4DoumentAsync(Guid id, int employmentId, SignatureModel signature)
    {
        var emp = _context.Employment.Include(e => e.Person).FirstOrDefault(e => e.Id == employmentId);
        _scopedAuthorization.ValidateByJobId(_httpContextAccessor.HttpContext.User, AuthorizationType.User, emp.JobId);
        _person = emp.Person;
        var documentType = DocumentTypeModel.WFourUnsigned;      
        var document = await GetDocumentByDocumentTypeAndIdAsync(documentType, id);
        if (document == null)
        {
            throw new KeyNotFoundException();
        };
       
        var font = await GetDocumentByDocumentTypeAndIdAsync(documentType, CursiveFont.Id);

        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

        using (var workStream = new MemoryStream())
        {

            using (PdfReader pdfReader = new PdfReader(document.Item1))
            using (PdfStamper pdfStamper = new PdfStamper(pdfReader, workStream))
            {
                AcroFields formFields = pdfStamper.AcroFields;
              
                formFields.SetField(PdfFields.Date, DateTime.Now.ToShortDateString());

                    using (var memoryStream = new MemoryStream())
                {
                    font.Item1.CopyTo(memoryStream); 
                    memoryStream.Position = 0; 
                    
                    BaseFont cursiveFont = BaseFont.CreateFont("Pacifico-Regular.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED, true, memoryStream.ToArray(), null);
                    var fieldPositions = formFields.GetFieldPositions(PdfFields.Signature);
                    if (fieldPositions != null && fieldPositions.Count > 0) 
                    { 
                        var position = fieldPositions[0].position; 
                        int pageNumber = fieldPositions[0].page; 
                        PdfContentByte cb = pdfStamper.GetOverContent(pageNumber);
                       
                        cb.BeginText();
                        cb.SetFontAndSize(cursiveFont, 12);
                   
                        cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, signature.Signature, position.Left, position.Bottom, 0);
                        cb.EndText(); 
                    } 
                }
                pdfStamper.FormFlattening = true;
                pdfStamper.Close();
                pdfReader.Close();
            }

            var fileBytes = workStream.ToArray();

            await UpdateW4PdfWithSignature(id, fileBytes, DocumentTypeModel.WFour);

            emp.SignUpStatusTypeId = (int)SignUpStatusTypeModel.UserCompletedSubmittingWFour;
            _context.Employment.Update(emp);
            await _context.SaveChangesAsync();

            return fileBytes;
        }
    }
    public async Task<W4PdfResult> GenerateW4pdf(int empId, W4Form model)
    {
        var emp = _context.Employment.Include(e=>e.Person).FirstOrDefault(e=>e.Id==empId);
       
        _scopedAuthorization.ValidateByJobId(_httpContextAccessor.HttpContext.User, AuthorizationType.User, emp.JobId);

        _person = emp.Person;
        var documentTypeId = (int)DocumentTypeModel.WFourUnsigned;
        var document = _context.Document.FirstOrDefault(e => e.DocumentTypeId == documentTypeId && !e.IsDeleted && e.FileName== DocumentTypeModel.WFourUnsigned.GetDisplayName());             

        if (document == null)
        {
            throw new KeyNotFoundException();
        };

        var filingStatus = GetSelectedFilingStatus(model.FilingStatus);
        if (string.IsNullOrEmpty(filingStatus.Item1))
        {
            throw new InvalidOperationException("No filing status selected or more than one status selected.");
        }

         var documentResult = await _documentManager.GetDocumentByTypeAndIdAsync(DocumentTypeModel.WFourUnsigned.ToString(),
             document.Id.ToString());
        
        var fileBytes = await SetPdfFormFilds(model, documentResult, filingStatus.Item1, emp,false);
        var documentForDisplay = await _documentManager.GetDocumentByTypeAndIdAsync(DocumentTypeModel.WFourUnsigned.ToString(),
            document.Id.ToString());
        var fileForDisplay = await SetPdfFormFilds(model, documentForDisplay, filingStatus.Item1, emp,true);
        var doumentId = await SaveGeneratedUnsignedW4Pdf(document.FileName, fileBytes, DocumentTypeModel.WFourUnsigned);
        var doumentModel = await CreateDocumentModel(doumentId, document.FileName, document.EffectiveDate);

        string prefix = "Step1c_FilingStatus_";
        string result = filingStatus.Item2.Substring(prefix.Length);

        var employeeWithhodingModelList = await CreatemployeeWithholdingModels(doumentId, result, model, doumentModel);
        foreach (EmployeeWithholdingModel employee in employeeWithhodingModelList)
        {
            await CreateEmployeeWithholdingAsync(employee);
        }

        emp.SignUpStatusTypeId = (int)SignUpStatusTypeModel.UserStartedSubmittingWFour;
        _context.Employment.Update(emp);
        await _context.SaveChangesAsync();

        return new W4PdfResult { FileBytes = fileForDisplay, DocumentId = doumentId };

    }

    public async Task<Guid> UpdateW4PdfWithSignature(Guid id, byte[] file, DocumentTypeModel documentType)
    {

        var documentModel = new DocumentModel
        {
            FileName = _person.FirstName + _person.MiddleName + _person.LastName + documentType.GetDisplayName(),
            DocumentType = documentType,
            PersonId =  _person.Id
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
    private EmployeeWithholdingModel CreateEmployeeWithholdingModel(Guid documentId, int fieldId, string fieldValue, DocumentModel documentModel)
    {
        var employeeAtCompany = _context.Employment.FirstOrDefault(e => e.PersonId == _person.Id && e.Job.CompanyId==_person.CompanyId);
        var effectiveDate = GetInvitationDateForEmployee(employeeAtCompany.PersonId);
        var employeeWithholdingModel = EmployeeWithholdingHelper.CreateEmployeeWithholdingModel(_person, documentId, fieldId, fieldValue, documentModel, employeeAtCompany.Id, effectiveDate);
        return employeeWithholdingModel;

    }

    private async Task<List<EmployeeWithholdingModel>> CreatemployeeWithholdingModels( Guid documentId,string filingStatus, W4Form model, DocumentModel documentModel)
    {
        var multipleJobsOrSpouseWorks = model.MultipleJobs || model.SpouseWorks;
        var totalClaimedAmount = model.NumberOfChildrenUnder17 * 2000 + model.OtherDependents * 500;
        var employeeWithHodingModel1C = CreateEmployeeWithholdingModel(documentId, 1, filingStatus, documentModel);
        var employeeWithHodingModel2C = CreateEmployeeWithholdingModel(documentId, 4, multipleJobsOrSpouseWorks.ToString(), documentModel);
        var employeeWithHodingModel3 = CreateEmployeeWithholdingModel(documentId, 5, totalClaimedAmount.ToString(), documentModel);
        var employeeWithHodingModel4A = CreateEmployeeWithholdingModel(documentId, 7, model.OtherIncome.ToString(), documentModel);
        var employeeWithHodingModel4B = CreateEmployeeWithholdingModel(documentId, 8, model.Deductions.ToString(), documentModel);
        var employeeWithHodingModel4C = CreateEmployeeWithholdingModel(documentId, 9, model.ExtraWithholding.ToString(), documentModel);

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

    private async Task<DocumentModel> CreateDocumentModel(Guid documentId, string filename, DateOnly effectiveDate)
    {
        var documentModel=EmployeeWithholdingHelper.CreateDocumentModel(documentId, filename, _person.Id, effectiveDate);
        return documentModel;
    }
    public async Task<Guid> Confirm(Guid id)
    {   
        var document = await GetDocument(id) ?? throw new KeyNotFoundException("Document not found");
        document.IsConfirmed = true;
        _context.Document.Update(document);
        await _context.SaveChangesAsync();
        return document.Id;
    }
   
    public async Task<W4PdfResult> GenerateNC4pdf(int empId, NC4Form model)
    {
        var emp = _context.Employment.Include(e => e.Person).FirstOrDefault(e => e.Id == empId);

        _scopedAuthorization.ValidateByJobId(_httpContextAccessor.HttpContext.User, AuthorizationType.User, emp.JobId);

        _person = emp.Person;
        var documentTypeId = (int)DocumentTypeModel.NCFourUnsigned;
        var document = _context.Document.FirstOrDefault(e => e.DocumentTypeId == documentTypeId && !e.IsDeleted && e.FileName == DocumentTypeModel.NCFourUnsigned.GetDisplayName());

        if (document == null)
        {
            throw new KeyNotFoundException();
        };

        var filingStatus = GetSelectedFilingStatus(model.FilingStatus);
        if (string.IsNullOrEmpty(filingStatus.Item1))
        {
            throw new InvalidOperationException("No filing status selected or more than one status selected.");
        }

        var documentResult = await _documentManager.GetDocumentByTypeAndIdAsync(DocumentTypeModel.NCFourUnsigned.ToString(),
            document.Id.ToString());

        var fileBytes = await SetNC4PdfFormFilds(model, documentResult, filingStatus.Item1, emp,false);

        var doumentId = await SaveGeneratedUnsignedW4Pdf(document.FileName, fileBytes, DocumentTypeModel.NCFourUnsigned);
        var documentForDisplay = await _documentManager.GetDocumentByTypeAndIdAsync(DocumentTypeModel.NCFourUnsigned.ToString(),
            document.Id.ToString());
        var fileForDisplay = await SetNC4PdfFormFilds(model, documentForDisplay, filingStatus.Item1, emp,true);
        var doumentModel = await CreateDocumentModel(doumentId, document.FileName, document.EffectiveDate);

        string prefix = "Step1c_FilingStatus_";
        string result = filingStatus.Item2.Substring(prefix.Length);
        var employeeWithHodingModel1C = CreateEmployeeWithholdingModel(doumentId, 1, result, doumentModel);
        await CreateEmployeeWithholdingAsync(employeeWithHodingModel1C);   

        emp.SignUpStatusTypeId = (int)SignUpStatusTypeModel.UserStartedSubmittingWFour;
        _context.Employment.Update(emp);
        await _context.SaveChangesAsync();
        

        return new W4PdfResult { FileBytes = fileForDisplay, DocumentId = doumentId };

    }
    private async Task<byte[]> SetNC4PdfFormFilds(NC4Form model, Stream documentStream, string filingStatus, Employment employee,bool formFlattening)
    {
        var firstName = _person?.FirstName;
        var middeName = _person?.LastName;
        var middeNameInitial = !string.IsNullOrEmpty(middeName) ? middeName[0].ToString() : "";
        var lastName = _person?.LastName;
        var ssn = _person?.Ssn;
        var address = _person?.AddressLine1;
        var zipCode = _person?.Zipcode?.Code;
        var city = _person?.Zipcode?.City?.Name;
        var state = _person?.Zipcode?.City?.State.Name;
        var country = _person?.Zipcode?.City?.State?.Country.Name;

        //TODO AddCounty
        //var county       
        string firstPart = ssn.Substring(0, 3);
        string secondPart = ssn.Substring(3, 2);
        string thirdPart = ssn.Substring(5, 4);

        using (var workStream = new MemoryStream())
        {

            using (PdfReader pdfReader = new PdfReader(documentStream))
            using (PdfStamper pdfStamper = new PdfStamper(pdfReader, workStream))
            {
                AcroFields formFields = pdfStamper.AcroFields;

                
                
                formFields.SetField(NC4PdfFields.NumberOfAllowance, model.NumberOfAllowance.ToString());
                formFields.SetField(NC4PdfFields.AdditionalAmt, model.AdditionalAmt.ToString());

                formFields.SetField(NC4PdfFields.SocialSecurity1stPart, firstPart);
                formFields.SetField(NC4PdfFields.LastName, lastName);
                formFields.SetField(NC4PdfFields.FilingStatus_FilingStatus1, "On");
                formFields.SetField(NC4PdfFields.FirstName, firstName.ToUpper());
                formFields.SetField(NC4PdfFields.FilingStatus_undefined_4, "On");
                formFields.SetField(NC4PdfFields.FilingStatus_undefined_5, "On");
                formFields.SetField(NC4PdfFields.MI, middeNameInitial);
                formFields.SetField(NC4PdfFields.Address, address.ToUpper());
                formFields.SetField(NC4PdfFields.ZipCode, $"{zipCode}");
                formFields.SetField(NC4PdfFields.Country, $"{country}");
                formFields.SetField(NC4PdfFields.City, $"{city}");
                formFields.SetField(NC4PdfFields.State, $"{state}");
                formFields.SetField(NC4PdfFields.FilingStatus_FilingStatus2, "On");
                formFields.SetField(NC4PdfFields.SocialSecurity2ndPart,secondPart);
                formFields.SetField(NC4PdfFields.SocialSecurity3rdPart ,thirdPart);


                 pdfStamper.FormFlattening = formFlattening;
                 pdfStamper.Close();
                 pdfReader.Close();
            }


            var fileBytes = workStream.ToArray();
            return fileBytes;
        }

    }
    public async Task<byte[]> SignNC4DoumentAsync(Guid id, int employmentId, SignatureModel signature)
    {
        var emp = _context.Employment.Include(e => e.Person).FirstOrDefault(e => e.Id == employmentId);
        _scopedAuthorization.ValidateByJobId(_httpContextAccessor.HttpContext.User, AuthorizationType.User, emp.JobId);
        _person = emp.Person;
        var documentType = DocumentTypeModel.NCFourUnsigned;
        var document = await GetDocumentByDocumentTypeAndIdAsync(documentType, id);
        if (document == null)
        {
            throw new KeyNotFoundException();
        };
        var font = await GetDocumentByDocumentTypeAndIdAsync(DocumentTypeModel.WFourUnsigned, CursiveFont.Id);

        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        using (var workStream = new MemoryStream())
        {

            using (PdfReader pdfReader = new PdfReader(document.Item1))
            using (PdfStamper pdfStamper = new PdfStamper(pdfReader, workStream))
            {
                AcroFields formFields = pdfStamper.AcroFields;
               
                formFields.SetField(NC4PdfFields.Date, DateTime.Now.ToShortDateString());
                using (var memoryStream = new MemoryStream())
                {
                    font.Item1.CopyTo(memoryStream);
                    memoryStream.Position = 0;

                    BaseFont cursiveFont = BaseFont.CreateFont("Pacifico-Regular.ttf", BaseFont.IDENTITY_H, BaseFont.EMBEDDED, true, memoryStream.ToArray(), null);
                    var fieldPositions = formFields.GetFieldPositions(NC4PdfFields.Signature);
                    if (fieldPositions != null && fieldPositions.Count > 0)
                    {
                        var position = fieldPositions[0].position;
                        int pageNumber = fieldPositions[0].page;
                        PdfContentByte cb = pdfStamper.GetOverContent(pageNumber);

                        cb.BeginText();
                        cb.SetFontAndSize(cursiveFont, 12);

                        cb.ShowTextAligned(PdfContentByte.ALIGN_LEFT, signature.Signature, position.Left, position.Bottom, 0);
                        cb.EndText();
                    }
                }
                pdfStamper.FormFlattening = true;
                pdfStamper.Close();
                pdfReader.Close();
            }

            var fileBytes = workStream.ToArray();

            await UpdateW4PdfWithSignature(id, fileBytes, DocumentTypeModel.NCFour);

            emp.SignUpStatusTypeId = (int)SignUpStatusTypeModel.UserCompletedSubmittingWFour;
            _context.Employment.Update(emp);
            await _context.SaveChangesAsync();

            return fileBytes;
        }
    }

    public async Task<List<DocumentType>> GetDocumentTypes()
    {
        var docTypes = await _context.DocumentType.ToListAsync();
        return docTypes;
    }
}

