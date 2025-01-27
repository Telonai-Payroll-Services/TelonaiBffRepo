﻿namespace TelonaiWebApi.Controllers;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Net.Http.Headers;
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
        if (document != null)
        {
            return File(document.Item1, "application/octet-stream", $"{document.Item2}.pdf");
        }
        else
        {
            return NotFound();
        }
    }


    [HttpGet("documentType/{documentType}/employer")]
    public async Task<IActionResult> GetByDocumentTypeEmployer(DocumentTypeModel documentType)
    {
        if (FilteredDocumentTypes.EmployerDocumentTypes.Contains(documentType.ToString()))
        {
            var document = await _documentService.GetDocumentByDocumentTypeEmployerAsync(documentType);
            if (document != null)

                return File(document.Item1, "application/octet-stream", $"{document.Item2}.pdf");
        }
        return NotFound();
    }

    [HttpGet("documentType/{documentType}/employee")]
    public async Task<IActionResult> GetByDocumentTypeEmployee(DocumentTypeModel documentType)
    {
        if (FilteredDocumentTypes.EmployeeDocumentTypes.Contains(documentType.ToString()))
        {
            var document = await _documentService.GetDocumentByDocumentTypeEmployeeAsync(documentType);
            if (document != null)

                return File(document.Item1, "application/octet-stream", $"{document.Item2}.pdf");
        }
        return NotFound();
    }


    [HttpGet("documentType/{documentType}/unsigned")]
    public async Task<IActionResult> GetGovernmentDocumentByDocumentType(DocumentTypeModel documentType)
    {
        _scopedAuthorization.Validate(Request.HttpContext.User, AuthorizationType.SystemAdmin);

        var document = await _documentService.GetDocumentByDocumentTypeAsync(documentType);
        return File(document.Item1, "application/octet-stream", $"{document.Item2}.pdf");
    }

    [HttpPost("documentType/{documentType}/signed")]
    public async Task<IActionResult> AddSignedDocument(IFormFile file, int employeeId, DocumentTypeModel documentType)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        if (documentType.ToString().EndsWith("Unsigned"))
        {
            return BadRequest("Invalid Document Type.");
        }

        using (Stream stream = new MemoryStream())
        {
            file.CopyTo(stream);

            var email = Request.HttpContext.User?.Claims.First(e => e.Type == "email").Value;
            var result = await _documentService.UploadSignedDocumentAsync(documentType, email, employeeId, stream);

            if (!result) return Forbid();

            return Ok(new { message = "Document uploaded." });
        }
    }

    [HttpPost("documentType/{documentType}/unsigned")]
    public async Task<IActionResult> AddGovernmentDocument(IFormFile file, DocumentTypeModel documentType)
    {
        _scopedAuthorization.Validate(Request.HttpContext.User, AuthorizationType.SystemAdmin);

        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        if (!documentType.ToString().EndsWith("Unsigned"))
        {
            return BadRequest("Invalid Document Type.");
        }

        using (Stream stream = new MemoryStream())
        {
            file.CopyTo(stream);

            await _documentService.AddGovernmentDocumentAsync(stream, documentType);

            return Ok(new { message = "Document uploaded." });
        }
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

    [HttpPost("employments/{employmentId}/generateW4pdf")]
    public async Task<IActionResult> GenerateW4pdf(int employmentId, [FromBody] W4Form model)
    {

        var result = await _documentService.GenerateW4pdf(employmentId, model);

        var response = new
        {
            DocumentId = result.DocumentId,
            File = File(result.FileBytes, "application/pdf", "edited_fw4.pdf")
        };
        return Ok(response);


    }

    [HttpPost("{id}/employments/{employmentId}/signW4pdf")]
    public async Task<IActionResult> SignW4Doument(Guid id, int employmentId, SignatureModel signature)
    {
        var fileBytes = await _documentService.SignW4DoumentAsync(id, employmentId, signature);

        return File(fileBytes, "application/pdf", "signed_fw4.pdf");

    }

    [HttpPost("{id}/confirm")]
    public async Task<IActionResult> Confirm(Guid id)
    {
        await _documentService.Confirm(id);
        return Ok(new { message = "Document Signature Confirmed." });
    }

    [HttpPost("employments/{employmentId}/generateNC4pdf")]
    public async Task<IActionResult> GenerateNC4pdf(int employmentId, [FromBody] NC4Form model)
    {

        var result = await _documentService.GenerateNC4pdf(employmentId, model);

        var response = new
        {
            DocumentId = result.DocumentId,
            File = File(result.FileBytes, "application/pdf", "edited_nc4.pdf")
        };
        return Ok(response);
    }
    [HttpPost("{id}/employments/{employmentId}/signNC4pdf")]
    public async Task<IActionResult> SignNC4Doument(Guid id, int employmentId, SignatureModel signature)
    {
        var fileBytes = await _documentService.SignNC4DoumentAsync(id, employmentId, signature);

        return File(fileBytes, "application/pdf", "signed_nc4.pdf");

    }

    [HttpGet("documentTypes")]
    public async Task<IActionResult> GetDocumentTypes()
    {
        var email = Request.HttpContext.User?.Claims.First(e => e.Type == "email").Value;
        var documentTypes = _documentService.GetDocumentTypes(email);
        return Ok(documentTypes);
    }
}