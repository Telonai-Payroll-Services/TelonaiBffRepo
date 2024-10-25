using TelonaiWebApi.Entities;
using TelonaiWebApi.Models;
using TelonaiWebApi.Services;

namespace TelonaiWebApi.Helpers
{
    public static class EmployeeWithholdingHelper
    {
        public static EmployeeWithholdingModel CreateEmployeeWithholdingModel(IEmploymentService<EmploymentModel, Employment> employmentService, IDocumentService documentService, Person person, Guid documentId, int fieldId, string fieldValue, DocumentModel documentModel)
        {
            var employments = employmentService.GetByPersonId(person.Id).ToList();
            var employeeAtCompany = employments.FirstOrDefault(e => e.CompanyId == person.CompanyId);
            var effectiveDate = documentService.GetInvitationDateForEmployee(employeeAtCompany.PersonId);

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
