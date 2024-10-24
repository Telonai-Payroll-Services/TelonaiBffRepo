namespace TelonaiWebApi.Controllers;

using Amazon.Runtime.Documents;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Net.Http.Headers;
using System.Xml.Linq;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;
using TelonaiWebApi.Services;

[ApiController]
[Route("[controller]")]
[Authorize()]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;
    private readonly IScopedAuthorization _scopedAuthorization;
    private Guid id;
    private readonly IEmploymentService<EmploymentModel, Employment> _employmentService;

    public DocumentsController(IDocumentService documentService, IScopedAuthorization scopedAuthorization, IEmploymentService<EmploymentModel, Employment> employmentService)
    {
        _documentService = documentService;
        _scopedAuthorization = scopedAuthorization;
        _employmentService = employmentService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var document = await _documentService.GetDocumentByDocumentIdAsync(id);
        return File(document.Item1, "application/octet-stream", $"{document.Item2}.pdf");

    }
    [HttpGet("{id}/own")]
    public async Task<IActionResult> GetOwnById(Guid id)
    {
        var document = await _documentService.GetDocumentByDocumentIdAsync(id);
        return File(document.Item1, "application/octet-stream", $"{document.Item2}.pdf");
    }
    [HttpGet("{id}/details")]
    public async Task<IActionResult> GetDetailsById(Guid id)
    {
        var document = await _documentService.GetDocumentDetailsByDocumentIdAsync(id);
        return Ok(document);
    }
    [HttpGet("{id}/details/own")]
    public async Task<IActionResult> GetOwnDetailsById(Guid id)
    {
        var document = await _documentService.GetDocumentDetailsByDocumentIdAsync(id);
        return Ok(document);
    }

    [HttpGet("documentType/{documentType}/own")]
    public async Task<IActionResult> GetOwnByDocumentType(DocumentTypeModel documentType)
    {
        var document = await _documentService.GetOwnDocumentByDocumentTypeAsync(documentType);
        if(document != null)
        {
            return File(document.Item1, "application/octet-stream", $"{document.Item2}.pdf");
        }
        else
        {
            return NotFound();
        }
    }


    [HttpGet("documentType/{documentType}")]
    public async Task<IActionResult> GetByDocumentType(DocumentTypeModel documentType)
    {
        var document = await _documentService.GetDocumentByDocumentTypeAsync(documentType);
        if (document != null)
        {
            return File(document.Item1, "application/octet-stream", $"{document.Item2}.pdf");
        }
        else
        {
            return NotFound();
        }
    }


    [HttpGet("documentType/{documentType}/unsigned")]
    public async Task<IActionResult> GetGovernmentDocumentByDocumentType(DocumentTypeModel documentType)
    {
        _scopedAuthorization.Validate(Request.HttpContext.User, AuthorizationType.SystemAdmin);

        var document = await _documentService.GetDocumentByDocumentTypeAsync(documentType);
        return File(document.Item1, "application/octet-stream", $"{document.Item2}.pdf");
    }

    [HttpPost("documentType/{documentType}/unsigned")]
    [Authorize(Policy = "SystemAdmin")]
    public async Task<IActionResult> AddGovernmentDocument(DocumentTypeModel documentType)
    {
        await _documentService.UploadInternalDocumentAsync(documentType);
        return Ok(201);
    }


    [HttpPost()]
    public async Task<IActionResult> AddDocument([FromForm] IFormFile file, [FromBody] DocumentModel model)
    {
        using (Stream stream = new MemoryStream())
        {
            file.CopyTo(stream);
            await _documentService.CreateAsync(model, stream);
        }
        return Ok(201);
    }


    [HttpPut("{id}")]
    [Authorize(Policy = "SystemAdmin")]
    public async Task<IActionResult> Update(Guid id, DocumentModel model)
    {
        _scopedAuthorization.Validate(Request.HttpContext.User, AuthorizationType.SystemAdmin);
        var doc = await _documentService.GetDocumentDetailsByDocumentIdAsync(id);
        if (doc != null)
        {
            _documentService.Update(id, model);
            return Ok(new { message = "Documents updated." });
        }
        else
        {
            return NotFound();
        }
    }


    [HttpDelete("{id}")]
    [Authorize(Policy = "SystemAdmin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        _scopedAuthorization.Validate(Request.HttpContext.User, AuthorizationType.SystemAdmin);

        await _documentService.Delete(id);
        return Ok(new { message = "Document deleted." });
    }

    private async Task<string> PostAttachment(byte[] data, Uri url, string contentType)
    {
        HttpContent content = new ByteArrayContent(data);

        content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        using (var form = new MultipartFormDataContent())
        {
            form.Add(content);

            using (var client = new HttpClient())
            {
                var response = await client.PostAsync(url, form);
                return await response.Content.ReadAsStringAsync();
            }
        }
    }
    [HttpPost("edit")]
    public async Task<IActionResult> EditPdf([FromBody] W4Form model)
    {
        var documentType = DocumentTypeModel.WFourUnsigned;
        Guid.TryParse("8712e3e1-e380-4bc5-8cfc-96ef92a53b41", out id);
        var document = await _documentService.GetDocumentByDocumentTypeAndIdAsync(documentType, id);
        if (document == null)
        {
            return NotFound();
        };

        var filingStatus = _documentService.GetSelectedFilingStatus(model.FilingStatus);
        if (string.IsNullOrEmpty(filingStatus.Item1))
        {
            throw new InvalidOperationException("No filing status selected or more than one status selected.");
        }

        var fileBytes = _documentService.SetPdfFormFilds(model, document.Item1, filingStatus.Item1);

        var person = await _documentService.GetPersonAsync();

        var doumentId = await _documentService.SaveGeneratedUnsignedW4Pdf(document.Item2, fileBytes);
        var doumentModel = CreateDocumentModel(doumentId, document.Item2, person.Id, document.Item3);
      
        string prefix = "Step1c_FilingStatus_";
        string result = filingStatus.Item2.Substring(prefix.Length);
        var employeeWithHodingModel1C = CreateEmployeeWithholdingModel(person,doumentId,1, result, doumentModel);   
        var employeeWithHodingModel2C = CreateEmployeeWithholdingModel(person, doumentId, 4, model.MultipleJobsOrSpouseWorks.ToString(), doumentModel); 
        var employeeWithHodingModel3 = CreateEmployeeWithholdingModel(person, doumentId, 5, model.Dependents.TotalClaimedAmount.ToString(), doumentModel); 
        var employeeWithHodingModel4A = CreateEmployeeWithholdingModel(person, doumentId, 7, model.OtherIncome.ToString(), doumentModel);  
        var employeeWithHodingModel4B = CreateEmployeeWithholdingModel(person, doumentId, 8, model.Deductions.ToString(), doumentModel); 
        var employeeWithHodingModel4C = CreateEmployeeWithholdingModel(person, doumentId, 9, model.ExtraWithholding.ToString(), doumentModel);
      
        var employeeWithHodingModelList = new List<EmployeeWithholdingModel>
        {
         employeeWithHodingModel1C,
         employeeWithHodingModel2C,
         employeeWithHodingModel3,
         employeeWithHodingModel4A,
         employeeWithHodingModel4B,
         employeeWithHodingModel4C,
        };
        foreach (EmployeeWithholdingModel employee in employeeWithHodingModelList)
        {
            _documentService.CreateEmployeeWithholdingAsync(employee, person);
        }



        return File(fileBytes, "application/pdf", "edited_fw4.pdf");

    }




    [HttpGet("{id}/WFourUnsigned")]
    public async Task<IActionResult> GetPdf(Guid id)
    {
        var documentType = DocumentTypeModel.WFourUnsigned;
        var document = await _documentService.GetDocumentByDocumentTypeAndIdAsync(documentType, id);
        if (document == null)
        {
            return NotFound();
        };
        return File(document.Item1, "application/pdf", "edited_fw4.pdf");
    }
    private EmployeeWithholdingModel CreateEmployeeWithholdingModel(Person person, Guid documentId, int fieldId, string fieldValue, DocumentModel documentModel)
    {

        var employments = _employmentService.GetByPersonId(person.Id).ToList();
        var employeeAtCompany = employments.FirstOrDefault(e => e.CompanyId == person.CompanyId);
        var effectiveDate = _documentService.GetInvitationDateForEmployee(employeeAtCompany.PersonId);
        return new EmployeeWithholdingModel
        {
            DocumentId = documentId,
            EmploymentId = employeeAtCompany.Id,
            WithholdingYear = DateTime.Now.Year,
            EffectiveDate = DateOnly.FromDateTime(effectiveDate),
            Document = documentModel,
            FieldId = fieldId,
            FieldValue = fieldValue
        };
    }
    private DocumentModel CreateDocumentModel(Guid doumentId, string fielename,int id,DateOnly effectiveDate)
    {
        return new DocumentModel
        {
            Id = doumentId,
            FileName = fielename,
            DocumentType = DocumentTypeModel.WFourUnsigned,
            PersonId = id,
            EffectiveDate = effectiveDate,
        };

    }
}