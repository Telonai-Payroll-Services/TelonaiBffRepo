namespace TelonaiWebApi.Controllers;

using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Net.Http.Headers;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;
using TelonaiWebApi.Services;

[ApiController]
[Route("[controller]")]
//[Authorize()]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;
    private readonly IScopedAuthorization _scopedAuthorization;
    public DocumentsController(IDocumentService documentService, IScopedAuthorization scopedAuthorization)
    {
        _documentService = documentService;
        _scopedAuthorization = scopedAuthorization;
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
        return File(document.Item1, "application/octet-stream", $"{document.Item2}.pdf");
    }

    [HttpGet("documentType/{documentType}")]
    public async Task<IActionResult> GetByDocumentType(DocumentTypeModel documentType)
    {
        var document = await _documentService.GetDocumentByDocumentTypeAsync(documentType);
        return File(document.Item1, "application/octet-stream", $"{document.Item2}.pdf");
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
        var doc = await _documentService.GetDocumentDetailsByDocumentIdAsync(id);
        _scopedAuthorization.Validate(Request.HttpContext.User, AuthorizationType.SystemAdmin);

        _documentService.Update(id, model);
        return Ok(new { message = "Documents updated." });
    }


    [HttpDelete("{id}")]
    [Authorize(Policy = "SystemAdmin")]
    public IActionResult Delete(Guid id)
    {
        _scopedAuthorization.Validate(Request.HttpContext.User, AuthorizationType.SystemAdmin);

        _documentService.Delete(id);
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
    public IActionResult EditPdf([FromBody] W4Form model)
    {
        string binPath = Path.Combine(Directory.GetCurrentDirectory(), "bin");
        string pdfPath = Path.Combine(binPath, "fw4.pdf");
        string outputPath = Path.Combine(binPath, "edited_fw4.pdf");

        string filingStatus = GetSelectedFilingStatus(model.FilingStatus);
        if (string.IsNullOrEmpty(filingStatus))
        {
            throw new InvalidOperationException("No filing status selected or more than one status selected.");
        }


        using (PdfReader pdfReader = new PdfReader(pdfPath))
        using (PdfStamper pdfStamper = new PdfStamper(pdfReader, new FileStream(outputPath, FileMode.Create)))
        {
            AcroFields formFields = pdfStamper.AcroFields;
            
            formFields.SetField(PdfFields.Step1a_FirstName_MiddleInitial, $"{model.Employee.FirstName} {model.Employee.MiddleInitial}");
            formFields.SetField(PdfFields.Step1a_LastName, model.Employee.LastName);
            formFields.SetField(PdfFields.Step1a_Address, model.Employee.Address);
            formFields.SetField(PdfFields.Step1a_City_Or_Town_State_ZIPCode, $"{model.Employee.CityOrTown} {model.Employee.State} {model.Employee.ZipCode}");
            formFields.SetField(PdfFields.Step1b_SocialSecurityNumber, model.Employee.SocialSecurityNumber);
            formFields.SetField(filingStatus,"1");
            //formFields.SetField(PdfFields.Step1c_FilingStatus_HeadOfHousehold, "1");
            //formFields.SetField(PdfFields.Step1c_FilingStatus_MarriedFilingJointly, "1");

            formFields.SetField(PdfFields.Step2_MultipleJobsOrSpouseWorks, model.MultipleJobsOrSpouseWorks?"1":"");

            formFields.SetField(PdfFields.Step3_Dependents_NumberOfChildrenUnder17, model.Dependents.NumberOfChildrenUnder17.ToString());
            formFields.SetField(PdfFields.Step3_Dependents_OtherDependents, model.Dependents.OtherDependents.ToString());
            formFields.SetField(PdfFields.Step3_TotalClaimedAmount, model.Dependents.TotalClaimedAmount.ToString());

            formFields.SetField(PdfFields.Step4a_OtherIncome, model.OtherIncome.ToString());
            formFields.SetField(PdfFields.Step4b_Deductions, model.Deductions.ToString());
            formFields.SetField(PdfFields.Step4c_ExtraWithholding, model.ExtraWithholding.ToString());
            
            SetCheckboxValue(formFields, PdfFields.Step1c_FilingStatus_HeadOfHousehold, true);

            //pdfStamper.FormFlattening = true;

            pdfStamper.FormFlattening = false;
        }

        byte[] fileBytes = System.IO.File.ReadAllBytes(outputPath);
        return File(fileBytes, "application/pdf", "edited_fw4.pdf");
    }

    private string GetSelectedFilingStatus(FilingStatus filingStatus)
    {
        int selectedCount = 0;
        string selectedFilingStatus = null;

        if (filingStatus.SingleOrMarriedFilingSeparately)
        {
            selectedCount++;
            selectedFilingStatus = PdfFields.Step1c_FilingStatus_SingleOrMarriedFilingSeparately;
        }
        if (filingStatus.MarriedFilingJointly)
        {
            selectedCount++;
            selectedFilingStatus = PdfFields.Step1c_FilingStatus_MarriedFilingJointly;
        }
        if (filingStatus.HeadOfHousehold)
        {
            selectedCount++;
            selectedFilingStatus = PdfFields.Step1c_FilingStatus_HeadOfHousehold;
        }
        if (selectedCount != 1)
        {
            return null;
        }

        return selectedFilingStatus;
    }
    private void SetCheckboxValue(AcroFields formFields, string fieldName, bool value)
    {
        // Attempt to set the checkbox with various possible values
        string[] possibleValues = { "True","1", "Yes" ,"On" };
        foreach (var possibleValue in possibleValues)
        {
            bool success = formFields.SetField(fieldName, value ? possibleValue : "Off",true);
            if (success)
            {
                return;
            }
        }
        throw new InvalidOperationException($"Failed to set the checkbox '{fieldName}' to '{value}'");
    }
}