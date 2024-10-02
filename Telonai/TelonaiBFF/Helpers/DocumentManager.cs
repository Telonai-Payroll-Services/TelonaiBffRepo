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

public interface IDocumentManager
{
    Task<Guid> CreatePayStubPdfAsync(PayStub payStub, OtherMoneyReceived otherReceived, List<IncomeTax> incomeTaxes);
    Task<Stream> GetPayStubByIdAsync(string id);
    Task<Stream> GetDocumentByTypeAndIdAsync(string documentType, string id);
    Task UploadDocumentAsync(Guid documentId, Stream stream, DocumentTypeModel documentType);
}

public class DocumentManager : IDocumentManager
{
    private static Font _headerFont = new(Font.FontFamily.TIMES_ROMAN, 15f, Font.BOLD | Font.UNDERLINE, BaseColor.BLACK);
    private static BaseFont _baseFont = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
    private static Font _fontUnderLined = new(_baseFont, 20, Font.UNDERLINE);
    private static Font _fontBold = new(_baseFont, 20, Font.BOLD);
    private static Font _fontNormal = new(_baseFont, 20, Font.NORMAL);
    private PdfPTable _tableLayout = new(2);
    private PdfPTable _childTableLayout1 = new(5);
    private PdfPTable _childTableLayout2 = new(3);
    private Company _company = null;
    private Person _person = null;
    private PayStub _payStub = null;

    private OtherMoneyReceived _otherMoneyReceived = null;
    private List<IncomeTax> _incomeTaxes = null;
    private readonly AmazonS3Client _s3Client = null;
    private readonly TransferUtility _transferUtility = null;
    private readonly string _bucketName = "telonai-documents";


    public DocumentManager()
    {
        //_s3Client = new AmazonS3Client("your-access-key", "your-secret-key", Amazon.RegionEndpoint.USEast2 );
        _s3Client = new AmazonS3Client(Amazon.RegionEndpoint.USEast2);
        _transferUtility = new TransferUtility(_s3Client);
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
        var response = await _s3Client.GetObjectAsync(request);
        return response.ResponseStream;
    }
    public async Task<Guid> CreatePayStubPdfAsync(PayStub payStub, OtherMoneyReceived otherReceived, List<IncomeTax> incomeTaxes)
    {
        var documentId = Guid.NewGuid();
        var doc = new iTextSharp.text.Document();

        _incomeTaxes = incomeTaxes;
        _otherMoneyReceived = otherReceived;

        _payStub = payStub;
        _person = payStub.Employment.Person;
        _company = payStub.Payroll.Company;

        doc.SetMargins(0, 0, 0, 0);
        doc.SetMargins(10, 10, 10, 0);


        using (var workStream = new MemoryStream())
        {
            PdfWriter.GetInstance(doc, workStream).CloseStream = false;
            doc.Open();

            var paragraph1 = new Paragraph($"{_person.FirstName} {_person.LastName} - Earnings Statement", _fontBold);
            paragraph1.Alignment = Element.ALIGN_CENTER;
            doc.Add(paragraph1);

            var paragraph2 = new Paragraph($"Pay Period - From: {payStub.Payroll.StartDate} To {payStub.Payroll.ScheduledRunDate}", _fontBold);
            paragraph2.Alignment = Element.ALIGN_CENTER;
            doc.Add(paragraph2);

            Paragraph p3 = new() { SpacingAfter = 6 };

            doc.Add(p3);
            doc.Add(Add_Content_To_PDF());
            doc.Close();

            byte[] byteInfo = workStream.ToArray();
            workStream.Write(byteInfo, 0, byteInfo.Length);
            workStream.Position = 0;


            // Upload the MemoryStream to S3
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
        float[] headers = { 100, 50 }; //Header Widths  
        _tableLayout.SetWidths(headers); //Set the pdf headers  
        _tableLayout.WidthPercentage = 100; //Set the PDF File witdh percentage  
        _tableLayout.HeaderRows = 1;
        _childTableLayout1.WidthPercentage = 100;
        _childTableLayout1.HeaderRows = 1;
        _childTableLayout2.WidthPercentage = 100;
        _childTableLayout2.HeaderRows = 1;

        var count = 1;

        //Add header  
        AddCompanyToHeader();
        AddEmployeeToHeader();

        AddCellToBody(_childTableLayout1, "Earnings", count, _fontBold);
        AddCellToBody(_childTableLayout1, "Rate", count, _fontBold);
        AddCellToBody(_childTableLayout1, "Hours", count, _fontBold);
        AddCellToBody(_childTableLayout1, "This Period", count, _fontBold);
        AddCellToBody(_childTableLayout1, "Year To Date", count, _fontBold);

        count++;
        AddCellToBody(_childTableLayout1, "Regular", count, _fontNormal);
        AddCellToBody(_childTableLayout1, _payStub.Employment.PayRate.ToString(), count, _fontNormal);
        AddCellToBody(_childTableLayout1, _payStub.RegularHoursWorked.ToString(), count, _fontNormal);
        AddCellToBody(_childTableLayout1, _payStub.RegularPay.ToString(), count, _fontNormal);
        AddCellToBody(_childTableLayout1, _payStub.YtdRegularPay.ToString(), count, _fontNormal);

        if (_payStub.YtdOverTimePay > 0.0)
        {
            count++;
            AddCellToBody(_childTableLayout1, "Overtime", count, _fontNormal);
            AddCellToBody(_childTableLayout1, (_payStub.Employment.PayRate * 1.5).ToString(), count, _fontNormal);
            AddCellToBody(_childTableLayout1, _payStub.OverTimeHoursWorked.ToString(), count, _fontNormal);
            AddCellToBody(_childTableLayout1, _payStub.OverTimePay.ToString(), count, _fontNormal);
            AddCellToBody(_childTableLayout1, _payStub.YtdOverTimePay.ToString(), count, _fontNormal);
        }

        if (_otherMoneyReceived != null)
        {
            if (_otherMoneyReceived.YtdCreditCardTips > 0.0)
            {
                count++;
                AddCellToBody(_childTableLayout1, "Credit Card  Tips", count, _fontNormal);
                AddCellToBody(_childTableLayout1, "", count, _fontNormal);
                AddCellToBody(_childTableLayout1, "", count, _fontNormal);
                AddCellToBody(_childTableLayout1, _otherMoneyReceived.CreditCardTips.ToString(), count, _fontNormal);
                AddCellToBody(_childTableLayout1, _otherMoneyReceived.YtdCreditCardTips.ToString(), count, _fontNormal);
            }
            if (_otherMoneyReceived.YtdCashTips > 0.0)
            {
                count++;
                AddCellToBody(_childTableLayout1, "Cash  Tips", count, _fontNormal);
                AddCellToBody(_childTableLayout1, "", count, _fontNormal);
                AddCellToBody(_childTableLayout1, "", count, _fontNormal);
                AddCellToBody(_childTableLayout1, _otherMoneyReceived.CashTips.ToString(), count, _fontNormal);
                AddCellToBody(_childTableLayout1, _otherMoneyReceived.YtdCashTips.ToString(), count, _fontNormal);
            }
            if (_otherMoneyReceived.YtdReimbursement > 0.0)
            {
                count++;
                AddCellToBody(_childTableLayout1, "Reimbursement", count, _fontNormal);
                AddCellToBody(_childTableLayout1, "", count, _fontNormal);
                AddCellToBody(_childTableLayout1, "", count, _fontNormal);
                AddCellToBody(_childTableLayout1, _otherMoneyReceived.Reimbursement.ToString(), count, _fontNormal);
                AddCellToBody(_childTableLayout1, _otherMoneyReceived.YtdReimbursement.ToString(), count, _fontNormal);
            }
            if (_otherMoneyReceived.YtdOtherPay > 0.0)
            {
                count++;
                var note = _otherMoneyReceived.Note;
                if (note.Length > 50)
                    note = note.Substring(0, 50);
                AddCellToBody(_childTableLayout1, note, count, _fontNormal);
                AddCellToBody(_childTableLayout1, "", count, _fontNormal);
                AddCellToBody(_childTableLayout1, "", count, _fontNormal);
                AddCellToBody(_childTableLayout1, _otherMoneyReceived.OtherPay.ToString(), count, _fontNormal);
                AddCellToBody(_childTableLayout1, _otherMoneyReceived.YtdOtherPay.ToString(), count, _fontNormal);
            }
        }
        count++;
        AddCellToBody(_childTableLayout1, "Gross Pay", count, _fontBold);
        AddCellToBody(_childTableLayout1, "", count, _fontNormal);
        AddCellToBody(_childTableLayout1, "", count, _fontNormal);
        AddCellToBody(_childTableLayout1, _payStub.GrossPay.ToString(), count, _fontBold);
        AddCellToBody(_childTableLayout1, _payStub.YtdGrossPay.ToString(), count, _fontBold);
        count++;
        AddCellToBody(_childTableLayout1, "", count, _fontBold);
        AddCellToBody(_childTableLayout1, "", count, _fontNormal);
        AddCellToBody(_childTableLayout1, "", count, _fontNormal);
        AddCellToBody(_childTableLayout1, "", count, _fontNormal);
        AddCellToBody(_childTableLayout1, "", count, _fontNormal);
        _tableLayout.AddCell(_childTableLayout1);

        //Deductions
        AddDeductionsToBody();


        return _tableLayout;
    }

    private void AddDeductionsToBody()
    {
        var count = 0;
        AddCellToBody(_childTableLayout2, "Deductions", count, _fontBold);
        AddCellToBody(_childTableLayout2, "This Period", count, _fontNormal);
        AddCellToBody(_childTableLayout2, "Year To Date", count, _fontNormal);

        foreach (var incomeTax in _incomeTaxes.Where(e => e.IncomeTaxType.ForEmployee && e.IncomeTaxType.StateId == null))
        {
            count++;
            AddCellToBody(_childTableLayout2, incomeTax.IncomeTaxType.Name, count, _fontNormal);
            AddCellToBody(_childTableLayout2, incomeTax.Amount.ToString(), count, _fontNormal);
            AddCellToBody(_childTableLayout2, incomeTax.YtdAmount.ToString(), count, _fontNormal);
        }
        foreach (var incomeTax in _incomeTaxes.Where(e => e.IncomeTaxType.ForEmployee && e.IncomeTaxType.StateId != null))
        {
            count++;
            AddCellToBody(_childTableLayout2, incomeTax.IncomeTaxType.Name, count, _fontNormal);
            AddCellToBody(_childTableLayout2, incomeTax.Amount.ToString(), count, _fontNormal);
            AddCellToBody(_childTableLayout2, incomeTax.YtdAmount.ToString(), count, _fontNormal);
        }
        count++;
        AddCellToBody(_childTableLayout1, "", count, _fontBold);
        AddCellToBody(_childTableLayout1, "", count, _fontNormal);
        AddCellToBody(_childTableLayout1, "", count, _fontNormal);
        AddCellToBody(_childTableLayout1, "", count, _fontNormal);
        AddCellToBody(_childTableLayout1, "", count, _fontNormal);

        count++;
        AddCellToBody(_childTableLayout2, "Net Pay", count, _fontBold);
        AddCellToBody(_childTableLayout2, "", count, _fontNormal);
        AddCellToBody(_childTableLayout2, "", count, _fontNormal);
        AddCellToBody(_childTableLayout2, _payStub.NetPay.ToString(), count, _fontBold);
        AddCellToBody(_childTableLayout2, _payStub.YtdNetPay.ToString(), count, _fontBold);
        _tableLayout.AddCell(_childTableLayout2);

    }
    private void AddCompanyToHeader()
    {
        var paragraph = new Paragraph("Company", _headerFont)
        {
            new Phrase(_company.Name, _fontNormal),
            new Phrase(_company.AddressLine1, _fontNormal)
        };

        if (!string.IsNullOrEmpty(_company.AddressLine2))
            paragraph.Add(new Phrase(_company.AddressLine2, _fontNormal));

        paragraph.Add(new Phrase($"{_company.Zipcode.City.Name}, {_company.Zipcode.City.State.Name},  {_company.Zipcode.Code}", _fontNormal));

        _tableLayout.AddCell(new PdfPCell(paragraph)
        {
            HorizontalAlignment = Element.ALIGN_LEFT,
            Padding = 8,
            Border = 0,
            BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255)
        });
    }

    private void AddEmployeeToHeader()
    {
        var paragraph = new Paragraph("Employee", _headerFont);

        paragraph.Add(new Phrase($"{_person.FirstName} {_person.LastName}", _fontNormal));
        paragraph.Add(new Phrase(_person.AddressLine1, _fontNormal));

        if (!string.IsNullOrEmpty(_person.AddressLine2))
            paragraph.Add(new Phrase(_person.AddressLine2, _fontNormal));

        paragraph.Add(new Phrase($"{_person.Zipcode.City.Name} ,  {_person.Zipcode.City.State.Name},  {_person.Zipcode.Code}", _fontNormal));

        _tableLayout.AddCell(new PdfPCell(paragraph)
        {
            HorizontalAlignment = Element.ALIGN_LEFT,
            Padding = 8,
            Border = 0,
            BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255)
        });
    }

    private static void AddCellToBody(PdfPTable tableLayout, string cellText, int count, Font font = null)
    {
        if (count % 2 == 0)
        {
            tableLayout.AddCell(new PdfPCell(new Phrase(cellText, font ?? new Font(Font.FontFamily.HELVETICA, 8, 1, iTextSharp.text.BaseColor.BLACK)))
            {
                HorizontalAlignment = Element.ALIGN_LEFT,
                Padding = 8,
                Border = 0,
                BackgroundColor = new iTextSharp.text.BaseColor(255, 255, 255)
            });
        }
        else
        {
            tableLayout.AddCell(new PdfPCell(new Phrase(cellText, new Font(Font.FontFamily.HELVETICA, 8, 1, iTextSharp.text.BaseColor.BLACK)))
            {
                HorizontalAlignment = Element.ALIGN_LEFT,
                Padding = 8,
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

    