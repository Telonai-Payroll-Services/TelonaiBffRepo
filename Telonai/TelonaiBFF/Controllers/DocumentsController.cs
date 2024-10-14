namespace TelonaiWebApi.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Net.Http.Headers;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;
using TelonaiWebApi.Services;

[ApiController]
[Route("[controller]")]
[Authorize()]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;
    private readonly IScopedAuthorization _scopedAuthrorization;
    public DocumentsController(IDocumentService documentService, IScopedAuthorization scopedAuthrorization)
    {
        _documentService = documentService;
        _scopedAuthrorization = scopedAuthrorization;
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
        _scopedAuthrorization.Validate(Request.HttpContext.User, AuthorizationType.SystemAdmin);

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
        _scopedAuthrorization.Validate(Request.HttpContext.User, AuthorizationType.SystemAdmin);

        _documentService.Update(id, model);
        return Ok(new { message = "Documents updated." });
    }


    [HttpDelete("{id}")]
    [Authorize(Policy = "SystemAdmin")]
    public IActionResult Delete(Guid id)
    {
        _scopedAuthrorization.Validate(Request.HttpContext.User, AuthorizationType.SystemAdmin);

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
}