namespace TelonaiWebApi.Helpers;

using iTextSharp.text.pdf;
using iTextSharp.text;
using System.Text;
using System;
using System.Collections.Generic;
using System.IO;
using TelonaiWebApi.Entities;
using Amazon.S3.Transfer;
using iTextSharp.text.pdf.qrcode;
using Amazon.S3;
using Amazon.S3.Model;
using TelonaiWebApi.Models;
using Microsoft.OpenApi.Extensions;
using Microsoft.EntityFrameworkCore;

public interface IDocumentManager
{
    Task<Guid> CreatePayStubPdfAsync(PayStub payStub, List<AdditionalOtherMoneyReceived> additionalMoneyReceived, List<IncomeTax> incomeTaxes);
    Task<Stream> GetPayStubByIdAsync(string id);
    Task<Stream> GetDocumentByTypeAndIdAsync(string documentType, string id);
    Task UploadDocumentAsync(Guid documentId, Stream stream, DocumentTypeModel documentType);
}

public class DocumentManager : IDocumentManager
{
    private static Font _headerFont = new(Font.FontFamily.TIMES_ROMAN, 12f, Font.BOLD, BaseColor.BLACK);
    private static BaseFont _baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
    private static Font _fontUnderLined = new(_baseFont, 10, Font.UNDERLINE);
    private static Font _fontBold = new(Font.FontFamily.TIMES_ROMAN, 10, Font.BOLD);
    private static Font _fontNormal = new(Font.FontFamily.TIMES_ROMAN, 10, Font.NORMAL);
 
    private Company _company = null;
    private Person _person = null;
    private PayStub _payStub = null;

    private OtherMoneyReceived _otherMoneyReceived = null;
    private List<AdditionalOtherMoneyReceived> _additionalMoneyReceived = null;
    private List<IncomeTax> _incomeTaxes = null;
    private readonly AmazonS3Client _s3Client = null;
    private readonly TransferUtility _transferUtility = null;
    private readonly string _bucketName = "telonai-documents";
    private readonly DataContext _context;

    public DocumentManager(DataContext context)
    {
        _s3Client = new AmazonS3Client(Amazon.RegionEndpoint.USEast2);
        _transferUtility = new TransferUtility(_s3Client);
        _context = context;
    }

    public async Task<Stream> GetPayStubByIdAsync(string id)
    {
        var request = new GetObjectRequest
        {
            BucketName = _bucketName,
            Key = $"{DocumentTypeModel.PayStub.GetDisplayName()}/{id}",
        };
        var response = await _s3Client.GetObjectAsync(request);

        return response.ResponseStream;
    }

    public async Task<Stream> GetDocumentByTypeAndIdAsync(string documentType, string id)
    {
        var request = new GetObjectRequest
        {
            BucketName = _bucketName,
            Key = $"{documentType}/{id}",
        };

        try
        {
            var response = await _s3Client.GetObjectAsync(request);
            return response.ResponseStream;

        }
        catch (Exception ex)
        {
            throw;
        } 
    }
   public async Task<Guid> CreatePayStubPdfAsync(PayStub payStub, List<AdditionalOtherMoneyReceived> additionalMoneyReceived, List<IncomeTax> incomeTaxes)
    {
        if (payStub == null) 
        {
            throw new AppException("PayStub not found"); 
        }

        var documentId = Guid.NewGuid();
        var doc = new iTextSharp.text.Document(PageSize.A4, 50, 50, 50, 50);

        _incomeTaxes = incomeTaxes;
        _otherMoneyReceived =payStub.OtherMoneyReceived;
        _additionalMoneyReceived = additionalMoneyReceived;

        _payStub = payStub;

        _person = payStub.Employment.Person;
        _company = payStub.Payroll.Company;

        using (var workStream = new MemoryStream())
        {
            PdfWriter.GetInstance(doc, workStream).CloseStream = false;
            doc.Open();

            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
            var title = new Paragraph($"{_person.FirstName} {_person.LastName} - Earnings Statement", titleFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 20
            };
            doc.Add(title);

            var subTitleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
            var subTitle = new Paragraph($"Pay Period - From: {payStub.Payroll.StartDate.ToShortDateString()} To {payStub.Payroll.ScheduledRunDate.ToShortDateString()}", subTitleFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 20
            };
            doc.Add(subTitle);

            var payDate = new Paragraph($"Pay Date: " + payStub.Payroll.TrueRunDate.Value.ToShortDateString(), subTitleFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 20
            };
            doc.Add(payDate);

            var contentTable = Add_Content_To_PDF();
            contentTable.SpacingBefore = 20;
            doc.Add(contentTable);

            doc.Close();

            byte[] byteInfo = workStream.ToArray();
            workStream.Write(byteInfo, 0, byteInfo.Length);
            workStream.Position = 0;

            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = workStream,
                BucketName = _bucketName,
                Key = $"{DocumentTypeModel.PayStub.GetDisplayName()}/{documentId}"
            };
            _transferUtility.Upload(uploadRequest);
        }
        return await Task.FromResult(documentId);
    }

    public async Task UploadDocumentAsync(Guid documentId, Stream stream, DocumentTypeModel documentType)
    {
        try
        {
            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = stream,
                BucketName = _bucketName,
                Key = $"{documentType}/{documentId}"
            };

            await _transferUtility.UploadAsync(uploadRequest);
        }
        catch (Exception ex) {
            throw;
        }
    }

    protected PdfPTable Add_Content_To_PDF()
    {

        PdfPTable tableLayout = new(2);
        PdfPTable childTableLayout1 = new(5);
        PdfPTable childTableLayout2 = new(3);

        float[] headers = new float[] { 95f, 65f };
        tableLayout.SetWidths(headers);
        tableLayout.WidthPercentage = 100;
        tableLayout.HeaderRows = 1;
        float[] incomeHeaders = new float[] { 55f, 40f, 35f, 60f, 60f };
        childTableLayout1.WidthPercentage = 100;
        childTableLayout1.HeaderRows = 1;
        childTableLayout1.SetWidths(incomeHeaders);
        float[] headers2 = { 40f, 35f, 40f };
        childTableLayout2.SetWidths(headers2);
        childTableLayout2.WidthPercentage = 100;
        childTableLayout2.HeaderRows = 1;


        var count = 1;

        AddCompanyToHeader(tableLayout);
        AddEmployeeToHeader(tableLayout);

        AddCellToBody(childTableLayout1, "Earnings", count, _fontBold);
        AddCellToBody(childTableLayout1, "Rate", count, _fontBold);
        AddCellToBody(childTableLayout1, "Hours", count, _fontBold);
        AddCellToBody(childTableLayout1, "This Period", count, _fontBold);
        AddCellToBody(childTableLayout1, "Year To Date", count, _fontBold);

        count++;
        AddCellToBody(childTableLayout1, "Regular", count, _fontNormal);
        AddCellToBody(childTableLayout1, _payStub.Employment.PayRate.ToString("0.00"), count, _fontNormal);
        AddCellToBody(childTableLayout1, _payStub.RegularHoursWorked.ToString("0.00"), count, _fontNormal);
        AddCellToBody(childTableLayout1, _payStub.RegularPay.ToString("0.00"), count, _fontNormal);
        AddCellToBody(childTableLayout1, _payStub.YtdRegularPay.ToString("0.00"), count, _fontNormal);

        if (_payStub.YtdOverTimePay > 0.0)
        {
            count++;
            AddCellToBody(childTableLayout1, "Overtime", count, _fontNormal);
            AddCellToBody(childTableLayout1, (_payStub.Employment.PayRate * 1.5).ToString("0.00"), count, _fontNormal);
            AddCellToBody(childTableLayout1, _payStub.OverTimeHoursWorked.ToString("0.00"), count, _fontNormal);
            AddCellToBody(childTableLayout1, _payStub.OverTimePay.ToString("0.00"), count, _fontNormal);
            AddCellToBody(childTableLayout1, _payStub.YtdOverTimePay.ToString("0.00"), count, _fontNormal);
        }

        if (_otherMoneyReceived != null)
        {
            if (_otherMoneyReceived.YtdCreditCardTips > 0.0)
            {
                count++;
                AddCellToBody(childTableLayout1, "Credit Card  Tips", count, _fontNormal);
                AddCellToBody(childTableLayout1, "", count, _fontNormal);
                AddCellToBody(childTableLayout1, "", count, _fontNormal);
                AddCellToBody(childTableLayout1, _otherMoneyReceived.CreditCardTips.ToString("0.00"), count, _fontNormal);
                AddCellToBody(childTableLayout1, _otherMoneyReceived.YtdCreditCardTips.ToString("0.00"), count, _fontNormal);
            }
            if (_otherMoneyReceived.YtdCashTips > 0.0)
            {
                count++;
                AddCellToBody(childTableLayout1, "Cash  Tips", count, _fontNormal);
                AddCellToBody(childTableLayout1, "", count, _fontNormal);
                AddCellToBody(childTableLayout1, "", count, _fontNormal);
                AddCellToBody(childTableLayout1, _otherMoneyReceived.CashTips.ToString("0.00"), count, _fontNormal);
                AddCellToBody(childTableLayout1, _otherMoneyReceived.YtdCashTips.ToString("0.00"), count, _fontNormal);
            }

            foreach (var item in _additionalMoneyReceived ?? new List<AdditionalOtherMoneyReceived>())
            {
                count++;
                var note = item.Note;
                if (note.Length > 50)
                    note = note.Substring(0, 50);
                AddCellToBody(childTableLayout1, note.ToString(), count, _fontBold);
                AddCellToBody(childTableLayout1, "", count, _fontBold);
                AddCellToBody(childTableLayout1, "", count, _fontBold);
                AddCellToBody(childTableLayout1, item.Amount.ToString("0.00"), count, _fontBold);
                AddCellToBody(childTableLayout1, item.YtdAmount.ToString("0.00"), count, _fontBold);
            }
        }
        count++;
        AddCellToBody(childTableLayout1, "Gross Pay", count, _fontBold);
        AddCellToBody(childTableLayout1, "", count, _fontNormal);
        AddCellToBody(childTableLayout1, "", count, _fontNormal);
        AddCellToBody(childTableLayout1, _payStub.GrossPay.ToString("0.00"), count, _fontBold);
        AddCellToBody(childTableLayout1, _payStub.YtdGrossPay.ToString("0.00"), count, _fontBold);

        count++;
        AddCellToBody(childTableLayout1, "Net Pay", count, _fontBold);
        AddCellToBody(childTableLayout1, "", count, _fontNormal);
        AddCellToBody(childTableLayout1, "", count, _fontNormal);
        AddCellToBody(childTableLayout1, _payStub.NetPay.ToString("0.00"), count, _fontBold);
        AddCellToBody(childTableLayout1, _payStub.YtdNetPay.ToString("0.00"), count, _fontBold);

        tableLayout.AddCell(childTableLayout1);

        AddDeductionsToBody(childTableLayout1, childTableLayout2,  tableLayout);
        return tableLayout;
    }

    private void AddDeductionsToBody(PdfPTable childTableLayout1, PdfPTable childTableLayout2, PdfPTable tableLayout)
    {
        var count = 1;
        AddCellToBody(childTableLayout2, "Deductions", count, _fontBold);
        AddCellToBody(childTableLayout2, "This Period", count, _fontBold);
        AddCellToBody(childTableLayout2, "Year To Date", count, _fontBold);
        var incomeTaxType = new IncomeTaxType();
        var incomeTaxes = _incomeTaxes.Where(e => e.PayStubId == _payStub.Id);

        if (_incomeTaxes != null)
        {
            foreach (var incomeTax in incomeTaxes.Where(e => e.IncomeTaxType.ForEmployee && e.IncomeTaxType.StateId == null))
            {
                count++;
                AddCellToBody(childTableLayout2, incomeTax.IncomeTaxType.Name.ToString(), count, _fontNormal);
                AddCellToBody(childTableLayout2, incomeTax.Amount.ToString("0.00"), count, _fontNormal);
                AddCellToBody(childTableLayout2, incomeTax.YtdAmount.ToString("0.00"), count, _fontNormal);
            }
                       
            foreach (var incomeTax in incomeTaxes.Where(e => e.IncomeTaxType.ForEmployee && e.IncomeTaxType.StateId != null))
            {
                count++;
                AddCellToBody(childTableLayout2, incomeTax.IncomeTaxType.Name.ToString(), count, _fontNormal);
                AddCellToBody(childTableLayout2, incomeTax.Amount.ToString("0.00"), count, _fontNormal);
                AddCellToBody(childTableLayout2, incomeTax.YtdAmount.ToString("0.00"), count, _fontNormal);
            }
        }
        count++;
        AddCellToBody(childTableLayout1, "", count, _fontBold);
        AddCellToBody(childTableLayout1, "", count, _fontNormal);
        AddCellToBody(childTableLayout1, "", count, _fontNormal);
        AddCellToBody(childTableLayout1, "", count, _fontNormal);
        AddCellToBody(childTableLayout1, "", count, _fontNormal);

       
        tableLayout.AddCell(childTableLayout2);

    }
    private void AddCompanyToHeader(PdfPTable tableLayout)
    {
        var fontSmall = new Font(Font.FontFamily.HELVETICA, 11, Font.NORMAL);
        var paragraph = new Paragraph
        {
            new Phrase("Company:  ", _headerFont),
            new Phrase(_company.Name, fontSmall),
            new Phrase("\n", fontSmall),
            new Phrase("Address:  ", _headerFont),         
            new Phrase(_company.AddressLine1, fontSmall)
        };

        if (!string.IsNullOrEmpty(_company.AddressLine2))
        {
            paragraph.Add(new Phrase("\n", fontSmall));
            paragraph.Add(new Phrase("              " + _company.AddressLine2, fontSmall));
        }

        paragraph.Add(new Phrase("\n", fontSmall));
        paragraph.Add(new Phrase($"              { _company.Zipcode.City.Name}", fontSmall));
        paragraph.Add(new Phrase("\n", fontSmall));
        paragraph.Add(new Phrase($"              { _company.Zipcode.City.State.StateCode} {_company.Zipcode.Code}" ,fontSmall));


        tableLayout.AddCell(new PdfPCell(paragraph)
        {
            HorizontalAlignment = Element.ALIGN_LEFT,
            Padding = 5,
            Border = 0,
            BackgroundColor = BaseColor.WHITE
        });
    }

    private void AddEmployeeToHeader(PdfPTable tableLayout)
    {
        var fontSmall = new Font(Font.FontFamily.HELVETICA, 11, Font.NORMAL);

        var paragraph = new Paragraph
    {
        new Phrase("Employee:  ", _headerFont),
        new Phrase($"{_person.FirstName}{" "} {_person.LastName}", fontSmall),
        new Phrase("\n", fontSmall),
        new Phrase("Address:  ", _headerFont),
        new Phrase(_person.AddressLine1, fontSmall)
    };

        if (!string.IsNullOrEmpty(_person.AddressLine2))
        {
            paragraph.Add(new Phrase("\n", fontSmall));
            paragraph.Add(new Phrase("              " + _person.AddressLine2, fontSmall));
        }
        paragraph.Add(new Phrase("\n", fontSmall));
        paragraph.Add(new Phrase($"              { _person.Zipcode?.City.Name}", fontSmall));
        paragraph.Add(new Phrase("\n", fontSmall));
        paragraph.Add(new Phrase($"              { _person.Zipcode?.City.State.StateCode} {_person.Zipcode?.Code}", fontSmall));
        

        tableLayout.AddCell(new PdfPCell(paragraph)
        {
            HorizontalAlignment = Element.ALIGN_LEFT,
            Padding = 5,
            Border = 0,
            BackgroundColor = BaseColor.WHITE
        });
    }

    private static void AddCellToBody(PdfPTable tableLayout, string cellText, int count, Font font = null)
    {
        if (count % 2 == 0)
        {
            tableLayout.AddCell(new PdfPCell(new Phrase(cellText, font ?? new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, iTextSharp.text.BaseColor.BLACK)))
            {
                HorizontalAlignment = Element.ALIGN_LEFT,
                Padding = 5,
                Border = 0,
                BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255)
            });
        }
        else
        {
            tableLayout.AddCell(new PdfPCell(new Phrase(cellText, font ?? new Font(Font.FontFamily.TIMES_ROMAN, 10, 1, iTextSharp.text.BaseColor.BLACK)))
            {
                HorizontalAlignment = Element.ALIGN_LEFT,
                Padding = 5,
                Border = 0,
                BackgroundColor = new iTextSharp.text.BaseColor(211, 211, 211)
            });
        }
    }

    public List<Person> PersonData()
    {
        List<Person> customers = new List<Person>()
            {
                new Person(){ FirstName="Meron", LastName="Kassa",AddressLine1="23 my street",Email="mak@gmail.com",ZipcodeId=39500 },
                new Person(){ FirstName="Jonathan", LastName="Kassa",AddressLine1="44 MAin St",Email="asfd@gmail.com",ZipcodeId=39222 },
            };
        return customers;
    }
}

    