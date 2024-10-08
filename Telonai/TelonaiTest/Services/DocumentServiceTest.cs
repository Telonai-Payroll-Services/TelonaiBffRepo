using Moq;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Services;
using TelonaiWebApi.Models;
using TelonaiWebApi.Helpers;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using AutoMapper;
using Microsoft.EntityFrameworkCore;

namespace TelonaiWebAPITest.Services
{
    public class DocumentServiceTest
    {
        private readonly Mock<DataContext> _mockDataContext;
        private readonly Mock<IMapper> _mockMapper;
        private Mock<DbSet<Document>> _mockDocumentSet;
        private readonly DocumentService _documentService;
        private readonly Mock<IDocumentManager> _mockDocumentManager;
        private readonly Mock<IScopedAuthorization> _mockScopedAuthorization;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private Mock<IPersonService<PersonModel, Person>> _mockPersonService;
        public DocumentServiceTest()
        {
            // Initialize the DocumentService with mocked dependencies
            _mockDataContext = new Mock<DataContext>();
            _mockMapper = new Mock<IMapper>();
            _mockDocumentSet = new Mock<DbSet<Document>>();
            _mockDocumentManager = new Mock<IDocumentManager>();
            _mockScopedAuthorization = new Mock<IScopedAuthorization>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockPersonService = new Mock<IPersonService<PersonModel, Person>>();
            _documentService = new DocumentService(_mockDataContext.Object, _mockMapper.Object, _mockDocumentManager.Object, _mockHttpContextAccessor.Object, _mockPersonService.Object, _mockScopedAuthorization.Object);
        }
        [Fact]
        public void GetOwnDocumentDetailsByDocumentTypeAsync_WhenDocumentExist_WithCorrectdDocumentType()
        {
            //Arrange
            var currentUser = new Person
            {
                Id = 1,
                FirstName = "Biruk",
                MiddleName = "Assefa",
                LastName = "Wolde",
                Email = "biras7070@gmail.com",
                MobilePhone = "0921739313",
                OtherPhone = "0921739313",
                AddressLine1 = "Wawel Street",
                AddressLine2 = "Pushkin Road",
                AddressLine3 = "Tesfaye Kegela Building",
                ZipcodeId = 1,
                Ssn = "123456789",
                Deactivated = false,
                CompanyId = 42,
            };
            _mockPersonService.Setup(person => person.GetCurrentUserAsync()).ReturnsAsync(currentUser);
            var mockClaimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("custom:scope", "SystemAdmin")
            }));

            var mockHttpContext = new DefaultHttpContext
            {
                User = mockClaimsPrincipal
            };
            _mockScopedAuthorization.Setup(x => x.ValidateByCompanyId(mockHttpContext.User, AuthorizationType.Admin, 42));
            var documentType = new DocumentType
            {
                Id = 1,
                Value = "123",
            };
            var document = new Document
            {
                Id = Guid.NewGuid(),
                FileName = "Sample.Pdf",
                DocumentTypeId = (int)DocumentTypeModel.INine,
                DocumentType = documentType,
                IsDeleted = false,
                PersonId = currentUser.Id
            };
            
            var documents = new List<Document> { document }.AsQueryable();
            _mockDocumentSet.As<IQueryable<Document>>().Setup(m => m.Provider).Returns(documents.Provider);
            _mockDocumentSet.As<IQueryable<Document>>().Setup(m => m.Expression).Returns(documents.Expression);
            _mockDocumentSet.As<IQueryable<Document>>().Setup(m => m.ElementType).Returns(documents.ElementType);
            _mockDocumentSet.As<IQueryable<Document>>().Setup(m => m.GetEnumerator()).Returns(documents.GetEnumerator());
            _mockDataContext.Setup(c => c.Document).Returns(_mockDocumentSet.Object);

            //Return DocumentModel
            var documentModel = new DocumentModel
            {
                Id = document.Id,
                FileName = "Sample.Pdf",
                DocumentType = DocumentTypeModel.INine,
                PersonId = currentUser.Id
            };
            _mockMapper.Setup(m => m.Map<DocumentModel>(It.IsAny<Document>())).Returns(documentModel);

            // Act
            var result =  _documentService.GetOwnDocumentDetailsByDocumentTypeAsync(DocumentTypeModel.INine);

            //Assert
            Assert.NotNull(result);  // Document exists, so result should not be null
            Assert.Equal(document.Id, result.Result.Id);  // Verify that the document is correctly mapped
        }
    }
}
