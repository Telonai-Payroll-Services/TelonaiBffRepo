using TelonaiWebApi.Entities;
using TelonaiWebApi.Models;
using TelonaiWebApi.Services;

namespace TelonaiWebApi.Helpers
{
    public static class EmployeeWithholdingHelper
    {
        public static EmployeeWithholdingModel CreateEmployeeWithholdingModel(Person person, Guid documentId, int fieldId, string fieldValue, DocumentModel documentModel,
            int employmentId,DateTime effectiveDate)
        {          

            return new EmployeeWithholdingModel
            {
                DocumentId = documentId,
                EmploymentId = employmentId,
                WithholdingYear = DateTime.Now.Year,
                EffectiveDate = DateOnly.FromDateTime(effectiveDate),
                Document = documentModel,
                FieldId = fieldId,
                FieldValue = fieldValue
            };
        }

        public static DocumentModel CreateDocumentModel(Guid documentId, string filename, int id, DateOnly effectiveDate)
        {
            return new DocumentModel
            {
                Id = documentId,
                FileName = filename,
                DocumentType = DocumentTypeModel.WFourUnsigned,
                PersonId = id,
                EffectiveDate = effectiveDate
            };
        }
    }
}
