using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Services;
using TelonaiWebApi.Models;
using TelonaiWebApi.Helpers;
using Moq;
using Document = TelonaiWebApi.Entities.Document;
using DocumentType = TelonaiWebApi.Entities.DocumentType;
using System.Net.Sockets;

namespace TelonaiWebAPITest.Services
{
    public class DocumentServiceTest
    {
        private readonly Mock<DataContext> _mockDataContext;
        private readonly Mock<IMapper> _mockMapper; 
        private Mock<DbSet<Document>> _mockDocumentSet;
        private Mock<DbSet<Person>> _mockPersonSet;
        private readonly DocumentService _documentService;
        private readonly Mock<IDocumentManager> _mockDocumentManager;
        private readonly Mock<IScopedAuthorization> _mockScopedAuthorization;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly Mock<Stream> _mockStream;
        private Mock<IPersonService<PersonModel, Person>> _mockPersonService;
        private Mock<IDocumentService> _mockDocumentService;
        private readonly Mock<IInvitationService<InvitationModel, Invitation>> _mockInvitationService;
        public DocumentServiceTest()
        {
            // Initialize the DocumentService with mocked dependencies
            _mockDataContext = new Mock<DataContext>();
            _mockMapper = new Mock<IMapper>();
            _mockDocumentSet = new Mock<Microsoft.EntityFrameworkCore.DbSet<Document>>();
            _mockDocumentManager = new Mock<IDocumentManager>();
            _mockScopedAuthorization = new Mock<IScopedAuthorization>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockPersonService = new Mock<IPersonService<PersonModel, Person>>();
            _mockPersonSet = new Mock<Microsoft.EntityFrameworkCore.DbSet<Person>>();
            _mockDocumentService = new Mock<IDocumentService>();
            _mockInvitationService = new Mock<IInvitationService<InvitationModel, Invitation>>();
            _documentService = new DocumentService(_mockDataContext.Object, _mockMapper.Object, _mockDocumentManager.Object, _mockHttpContextAccessor.Object, _mockPersonService.Object, _mockScopedAuthorization.Object, _mockInvitationService.Object);
        }

        #region GetOwnDocumentDetailsByDocumentTypeAsync
        [Fact]
        public async Task GetOwnDocumentDetailsByDocumentTypeAsync_WhenDocumentExist_ReturnsDocumentsOwnedByThePerson()
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
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                    new Claim("custom:scope", "SystemAdmin")
            }));
            var context = new DefaultHttpContext
            {
                User = claimsPrincipal
            };

            var documentType = new DocumentType
            {
                Id = 1,
                Value = "123",
            };
            var documents = new List<Document>
            {
                new Document
                {
                    Id = Guid.NewGuid(),
                    FileName = "INine.Pdf",
                    DocumentTypeId = (int)DocumentTypeModel.INine,
                    DocumentType = documentType,
                    IsDeleted = false,
                    PersonId = currentUser.Id,
                    CreatedDate = DateTime.Now
                },
                new Document
                {
                    Id = Guid.NewGuid(),
                    FileName = "INine.Pdf",
                    DocumentTypeId = (int)DocumentTypeModel.INine,
                    DocumentType = documentType,
                    IsDeleted = false,
                    PersonId = 12,
                    CreatedDate = DateTime.Now
                }
            }.AsQueryable();

            _mockDocumentSet.As<IQueryable<Document>>().Setup(m => m.Provider).Returns(documents.Provider);
            _mockDocumentSet.As<IQueryable<Document>>().Setup(m => m.Expression).Returns(documents.Expression);
            _mockDocumentSet.As<IQueryable<Document>>().Setup(m => m.ElementType).Returns(documents.ElementType);
            _mockDocumentSet.As<IQueryable<Document>>().Setup(m => m.GetEnumerator()).Returns(documents.GetEnumerator());
           
            _mockDataContext.Setup(d => d.Document).Returns(_mockDocumentSet.Object);
            //Return DocumentModel
            var documentModel = new DocumentModel
            {
                Id = documents.ToList().FirstOrDefault().Id,
                FileName = "INine.Pdf",
                DocumentType = DocumentTypeModel.INine,
                PersonId = currentUser.Id,

            };
            _mockMapper.Setup(m => m.Map<DocumentModel>(It.IsAny<Document>())).Returns(documentModel);
            _mockHttpContextAccessor.Setup(i => i.HttpContext).Returns(context);
            _mockScopedAuthorization.Setup(x => x.ValidateByCompanyId(claimsPrincipal, AuthorizationType.Admin, 42)).Callback(() => { });

            // Act
            var result = await _documentService.GetOwnDocumentDetailsByDocumentTypeAsync(DocumentTypeModel.INine);

            //Assert
            Assert.NotNull(result);  // Document exists, so result should not be null
            Assert.Equal(documents.ToList().FirstOrDefault().Id, result.Id);  // Verify that the document is correctly mapped
        }
        [Fact]
        public async Task GetOwnDocumentDetailsByDocumentTypeAsync_WhenDocumentNotExist_ReturnsNullDocumentsOwnedByThePerson()
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
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("custom:scope", "SystemAdmin")
            }));
            var context = new DefaultHttpContext
            {
                User = claimsPrincipal
            };

            var documentType = new DocumentType
            {
                Id = 1,
                Value = "123",
            };
            var documents = new List<Document>
            {
                new Document
                {
                    Id = Guid.NewGuid(),
                    FileName = "INine.Pdf",
                    DocumentTypeId = (int)DocumentTypeModel.INine,
                    DocumentType = documentType,
                    IsDeleted = false,
                    PersonId = currentUser.Id,
                    CreatedDate = DateTime.Now
                },
                new Document
                {
                    Id = Guid.NewGuid(),
                    FileName = "INine.Pdf",
                    DocumentTypeId = (int)DocumentTypeModel.INineUnsigned,
                    DocumentType = documentType,
                    IsDeleted = false,
                    PersonId = 12,
                    CreatedDate = DateTime.Now
                }
            }.AsQueryable();

            _mockDocumentSet.As<IQueryable<Document>>().Setup(m => m.Provider).Returns(documents.Provider);
            _mockDocumentSet.As<IQueryable<Document>>().Setup(m => m.Expression).Returns(documents.Expression);
            _mockDocumentSet.As<IQueryable<Document>>().Setup(m => m.ElementType).Returns(documents.ElementType);
            _mockDocumentSet.As<IQueryable<Document>>().Setup(m => m.GetEnumerator()).Returns(documents.GetEnumerator());
            _mockDataContext.Setup(d => d.Document).Returns(_mockDocumentSet.Object);
            //Return DocumentModel
            var documentModel = new DocumentModel
            {
                Id = documents.ToList().FirstOrDefault().Id,
                FileName = "INine.Pdf",
                DocumentType = DocumentTypeModel.INine,
                PersonId = currentUser.Id,

            };
            _mockMapper.Setup(m => m.Map<DocumentModel>(It.IsAny<Document>())).Returns(documentModel);
            _mockHttpContextAccessor.Setup(i => i.HttpContext).Returns(context);
            _mockScopedAuthorization.Setup(x => x.ValidateByCompanyId(claimsPrincipal, AuthorizationType.Admin, 42)).Callback(() => { });

            // Act
            var result = await _documentService.GetOwnDocumentDetailsByDocumentTypeAsync(DocumentTypeModel.INineUnsigned);

            //Assert
            Assert.Null(result);  // There is no document exists, that is owned by the person
        }
        #endregion
        #region GetOwnDocumentByDocumentTypeAsync

        [Fact]
        public async Task GetOwnDocumentByDocumentTypeAsync_WhenDocumentExist_ReturnsDocumentsOwnedByThePerson()
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

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                    new Claim("custom:scope", "SystemAdmin")
            }));
            var context = new DefaultHttpContext
            {
                User = claimsPrincipal
            };

            var documentType = new DocumentType
            {
                Id = 1,
                Value = "123",
            };
            var documents = new List<Document>
                {
                    new Document
                    {
                        Id = Guid.NewGuid(),
                        FileName = "INine.Pdf",
                        DocumentTypeId = (int)DocumentTypeModel.INine,
                        DocumentType = documentType,
                        IsDeleted = false,
                        PersonId = currentUser.Id,
                        CreatedDate = DateTime.Now
                    },
                    new Document
                    {
                        Id = Guid.NewGuid(),
                        FileName = "INine.Pdf",
                        DocumentTypeId = (int)DocumentTypeModel.INine,
                        DocumentType = documentType,
                        IsDeleted = false,
                        PersonId = 12,
                        CreatedDate = DateTime.Now
                    }
                }.AsQueryable();

            _mockDocumentSet.As<IQueryable<Document>>().Setup(m => m.Provider).Returns(documents.Provider);
            _mockDocumentSet.As<IQueryable<Document>>().Setup(m => m.Expression).Returns(documents.Expression);
            _mockDocumentSet.As<IQueryable<Document>>().Setup(m => m.ElementType).Returns(documents.ElementType);
            _mockDocumentSet.As<IQueryable<Document>>().Setup(m => m.GetEnumerator()).Returns(documents.GetEnumerator());
            _mockDataContext.Setup(d => d.Document).Returns(_mockDocumentSet.Object);
            //Return DocumentModel
            var documentModel = new DocumentModel
            {
                Id = documents.ToList().FirstOrDefault().Id,
                FileName = "INine.Pdf",
                DocumentType = DocumentTypeModel.INine,
                PersonId = currentUser.Id,

            };
            _mockHttpContextAccessor.Setup(i => i.HttpContext).Returns(context);
            _mockScopedAuthorization.Setup(x => x.ValidateByCompanyId(claimsPrincipal, AuthorizationType.Admin, 42)).Callback(() => { });
            _mockDocumentManager.Setup(dm => dm.GetDocumentByTypeAndIdAsync(DocumentTypeModel.INine.ToString(), documents.ToList().FirstOrDefault().Id.ToString())).ReturnsAsync(It.IsAny<Stream>());
            // Act
            var result = await _documentService.GetOwnDocumentByDocumentTypeAsync(DocumentTypeModel.INine);

            //Assert
            Assert.NotNull(result);  // Document exists, so result should not be null
            Assert.Equal(documents.ToList().FirstOrDefault().FileName, result.Item2);  // Verify that the document is correctly mapped
        }
        [Fact]
        public async Task GetOwnDocumentByDocumentTypeAsync_WhenNoDocumentTypeExist_ReturnsNullDocumentsOwnedByThePerson()
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

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                    new Claim("custom:scope", "SystemAdmin")
            }));
            var context = new DefaultHttpContext
            {
                User = claimsPrincipal
            };

            var documentType = new DocumentType
            {
                Id = 1,
                Value = "123",
            };
            var documents = new List<Document>
                {
                    new Document
                    {
                        Id = Guid.NewGuid(),
                        FileName = "INine.Pdf",
                        DocumentTypeId = (int)DocumentTypeModel.INine,
                        DocumentType = documentType,
                        IsDeleted = false,
                        PersonId = currentUser.Id,
                        CreatedDate = DateTime.Now
                    },
                    new Document
                    {
                        Id = Guid.NewGuid(),
                        FileName = "INine.Pdf",
                        DocumentTypeId = (int)DocumentTypeModel.INine,
                        DocumentType = documentType,
                        IsDeleted = false,
                        PersonId = 12,
                        CreatedDate = DateTime.Now
                    }
                }.AsQueryable();

            _mockDocumentSet.As<IQueryable<Document>>().Setup(m => m.Provider).Returns(documents.Provider);
            _mockDocumentSet.As<IQueryable<Document>>().Setup(m => m.Expression).Returns(documents.Expression);
            _mockDocumentSet.As<IQueryable<Document>>().Setup(m => m.ElementType).Returns(documents.ElementType);
            _mockDocumentSet.As<IQueryable<Document>>().Setup(m => m.GetEnumerator()).Returns(documents.GetEnumerator());
            _mockDataContext.Setup(d => d.Document).Returns(_mockDocumentSet.Object);
            //Return DocumentModel
            var documentModel = new DocumentModel
            {
                Id = documents.ToList().FirstOrDefault().Id,
                FileName = "INine.Pdf",
                DocumentType = DocumentTypeModel.INine,
                PersonId = currentUser.Id,

            };

            _mockHttpContextAccessor.Setup(i => i.HttpContext).Returns(context);
            _mockScopedAuthorization.Setup(x => x.ValidateByCompanyId(claimsPrincipal, AuthorizationType.Admin, 42)).Callback(() => { });
            _mockDocumentManager.Setup(dm => dm.GetDocumentByTypeAndIdAsync(DocumentTypeModel.INine.ToString(), documents.ToList().FirstOrDefault().Id.ToString())).ReturnsAsync(It.IsAny<Stream>());
            // Act
            var result = await _documentService.GetOwnDocumentByDocumentTypeAsync(DocumentTypeModel.NCFour);

            //Assert
            Assert.Null(result);  // Document exists, so result should not be null
        }
        [Fact]
        public async Task GetOwnDocumentByDocumentIdAsync_WhenDocumentExistWithId_ReturnsDocumentsWithProvidedId()
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
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                    new Claim("custom:scope", "SystemAdmin")
            }));
            var context = new DefaultHttpContext
            {
                User = claimsPrincipal
            };

            var documentType = new DocumentType
            {
                Id = 1,
                Value = "123",
            };
            Guid docId = Guid.NewGuid();
            var document = new Document
            {
                Id = docId,
                FileName = "INine.Pdf",
                DocumentTypeId = (int)DocumentTypeModel.INine,
                DocumentType = documentType,
                IsDeleted = false,
                PersonId = currentUser.Id,
                CreatedDate = DateTime.Now
            };

            //Return DocumentModel
            var documentModel = new DocumentModel
            {
                Id = document.Id,
                FileName = "INine.Pdf",
                DocumentType = DocumentTypeModel.INine,
                PersonId = currentUser.Id,

            };
            _mockMapper.Setup(m => m.Map<DocumentModel>(It.IsAny<Document>())).Returns(documentModel);
            _mockHttpContextAccessor.Setup(i => i.HttpContext).Returns(context);
            _mockScopedAuthorization.Setup(x => x.ValidateByCompanyId(claimsPrincipal, AuthorizationType.Admin, 42)).Callback(() => { });
            _mockDataContext.Setup(c => c.Document.FindAsync(document.Id)).ReturnsAsync(document); // Null document

            // Act
            var result = await _documentService.GetOwnDocumentByDocumentIdAsync(docId);

            //Assert
            Assert.NotNull(result);  // Document exists, so result should not be null
            Assert.Equal(document.FileName, result.Item2);  // Verify that the document is correctly mapped
        }

        #endregion
        #region GetOwnDocumentByDocumentIdAsync

        [Fact]
        public async Task GetOwnDocumentByDocumentIdAsync_WhenNoDocumentExistWithProvidedId_ReturnsNull()
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
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                    new Claim("custom:scope", "SystemAdmin")
            }));
            var context = new DefaultHttpContext
            {
                User = claimsPrincipal
            };

            var documentType = new DocumentType
            {
                Id = 1,
                Value = "123",
            };
            Guid docId = Guid.NewGuid();
            var document = new Document
            {
                Id = docId,
                FileName = "INine.Pdf",
                DocumentTypeId = (int)DocumentTypeModel.INine,
                DocumentType = documentType,
                IsDeleted = false,
                PersonId = currentUser.Id,
                CreatedDate = DateTime.Now
            };

            //Return DocumentModel
            var documentModel = new DocumentModel
            {
                Id = document.Id,
                FileName = "INine.Pdf",
                DocumentType = DocumentTypeModel.INine,
                PersonId = currentUser.Id,

            };
            _mockMapper.Setup(m => m.Map<DocumentModel>(It.IsAny<Document>())).Returns(documentModel);
            _mockHttpContextAccessor.Setup(i => i.HttpContext).Returns(context);
            _mockScopedAuthorization.Setup(x => x.ValidateByCompanyId(claimsPrincipal, AuthorizationType.Admin, 42)).Callback(() => { });
            _mockDataContext.Setup(c => c.Document.FindAsync(Guid.NewGuid())).ReturnsAsync(document); // Null document

            // Act
            var result = await _documentService.GetOwnDocumentByDocumentIdAsync(docId);

            //Assert
            Assert.Null(result);  // Document exists, so result should not be null
        }
        [Fact]
        public async Task GetOwnDocumentByDocumentIdAsync_WhenDocumentDoesNotExist_ReturnsNull()
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
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim("custom:scope", "SystemAdmin")
            }));
            var context = new DefaultHttpContext
            {
                User = claimsPrincipal
            };

            var documentType = new DocumentType
            {
                Id = 1,
                Value = "123",
            };
            Guid docId = Guid.NewGuid();
            var document = new Document
            {
                Id = docId,
                FileName = "INine.Pdf",
                DocumentTypeId = (int)DocumentTypeModel.INine,
                DocumentType = documentType,
                IsDeleted = false,
                PersonId = currentUser.Id,
                CreatedDate = DateTime.Now
            };

            //Return DocumentModel
            var documentModel = new DocumentModel
            {
                Id = document.Id,
                FileName = "INine.Pdf",
                DocumentType = DocumentTypeModel.INine,
                PersonId = currentUser.Id,

            };
            _mockMapper.Setup(m => m.Map<DocumentModel>(It.IsAny<Document>())).Returns(documentModel);
            _mockHttpContextAccessor.Setup(i => i.HttpContext).Returns(context);
            _mockScopedAuthorization.Setup(x => x.ValidateByCompanyId(claimsPrincipal, AuthorizationType.Admin, 42)).Callback(() => { });
            _mockDataContext.Setup(c => c.Document.FindAsync(Guid.NewGuid())).ReturnsAsync((Document)null); // Null document

            // Act
            var result = await _documentService.GetOwnDocumentByDocumentIdAsync(docId);

            //Assert
            Assert.Null(result);  // Document exists, so result should not be null
        }

        #endregion
        #region GetOwnDocumentDetailsByDocumentIdAsync

        [Fact]
        public async Task GetOwnDocumentDetailsByDocumentIdAsync_WhenDocumentExistByPersone_ReturnsDocument()
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
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                    new Claim("custom:scope", "SystemAdmin")
            }));
            var context = new DefaultHttpContext
            {
                User = claimsPrincipal
            };
            var documentType = new DocumentType
            {
                Id = 1,
                Value = "123",
            };
            var document = new Document
            {
                Id = Guid.NewGuid(),
                FileName = "INine.Pdf",
                DocumentTypeId = (int)DocumentTypeModel.INine,
                DocumentType = documentType,
                IsDeleted = false,
                PersonId = currentUser.Id,
                CreatedDate = DateTime.Now
            };

            //Return DocumentModel
            var documentModel = new DocumentModel
            {
                Id = document.Id,
                FileName = "INine.Pdf",
                DocumentType = DocumentTypeModel.INine,
                PersonId = currentUser.Id,

            };
            _mockMapper.Setup(m => m.Map<DocumentModel>(It.IsAny<Document>())).Returns(documentModel);
            _mockHttpContextAccessor.Setup(i => i.HttpContext).Returns(context);
            _mockScopedAuthorization.Setup(x => x.ValidateByCompanyId(claimsPrincipal, AuthorizationType.Admin, 42)).Callback(() => { });
            _mockDataContext.Setup(c => c.Document.FindAsync(document.Id)).ReturnsAsync(document); // Null document

            // Act
            var result = await _documentService.GetOwnDocumentDetailsByDocumentIdAsync(document.Id);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(document.Id, result.Id);// Document exists, so result should not be null
        }
        [Fact]
        public async Task GetOwnDocumentDetailsByDocumentIdAsync_WhenNoDocument_ReturnsNull()
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
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                    new Claim("custom:scope", "SystemAdmin")
            }));
            var context = new DefaultHttpContext
            {
                User = claimsPrincipal
            };
            var docId = Guid.NewGuid();
            _mockHttpContextAccessor.Setup(i => i.HttpContext).Returns(context);
            _mockScopedAuthorization.Setup(x => x.ValidateByCompanyId(claimsPrincipal, AuthorizationType.Admin, 42)).Callback(() => { });
            _mockDataContext.Setup(c => c.Document.FindAsync(docId)).ReturnsAsync((Document)null); // Null document

            // Act
            var result = await _documentService.GetOwnDocumentDetailsByDocumentIdAsync(docId);

            //Assert
            Assert.Null(result);
        }

        #endregion
        #region GetDocumentByDocumentIdAsync

        [Fact]
        public async Task GetDocumentByDocumentIdAsync_WhenPassingWrondDocumentId_ReturnsNull()
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
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                    new Claim("custom:scope", "SystemAdmin")
            }));
            var context = new DefaultHttpContext
            {
                User = claimsPrincipal
            };

            var documentType = new DocumentType
            {
                Id = 1,
                Value = "123",
            };
            Guid docId = Guid.NewGuid();
            var document = new Document
            {
                Id = docId,
                FileName = "INine.Pdf",
                DocumentTypeId = (int)DocumentTypeModel.INine,
                DocumentType = documentType,
                IsDeleted = false,
                PersonId = currentUser.Id,
                CreatedDate = DateTime.Now
            };

            _mockHttpContextAccessor.Setup(i => i.HttpContext).Returns(context);
            _mockScopedAuthorization.Setup(x => x.ValidateByCompanyId(claimsPrincipal, AuthorizationType.Admin, 42)).Callback(() => { });
            _mockDataContext.Setup(c => c.Document.FindAsync(Guid.NewGuid())).ReturnsAsync((Document)null); // Null document

            // Act
            var result = await _documentService.GetDocumentByDocumentIdAsync(docId);

            //Assert
            Assert.Null(result);
        }
        [Fact]
        public async Task GetDocumentByDocumentIdAsync_WhenPassingCorrectDocumentId_ReturnsDocumentInfo()
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
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                    new Claim("custom:scope", "SystemAdmin")
            }));
            var context = new DefaultHttpContext
            {
                User = claimsPrincipal
            };

            var documentType = new DocumentType
            {
                Id = 1,
                Value = "123",
            };
            Guid docId = Guid.NewGuid();
            var document = new Document
            {
                Id = docId,
                FileName = "INine.Pdf",
                DocumentTypeId = (int)DocumentTypeModel.INine,
                DocumentType = documentType,
                IsDeleted = false,
                PersonId = currentUser.Id,
                CreatedDate = DateTime.Now
            };

            _mockHttpContextAccessor.Setup(i => i.HttpContext).Returns(context);
            _mockScopedAuthorization.Setup(x => x.ValidateByCompanyId(claimsPrincipal, AuthorizationType.Admin, 42)).Callback(() => { });
            _mockDataContext.Setup(c => c.Document.FindAsync(docId)).ReturnsAsync(document); // Null document

            // Act
            var result = await _documentService.GetDocumentByDocumentIdAsync(docId);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(document.FileName, result.Item2);
        }
        [Fact]
        public async Task GetDocumentByDocumentIdAsync_WhenNoDocumentProvided_ReturnsNull()
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
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                    new Claim("custom:scope", "SystemAdmin")
            }));
            var context = new DefaultHttpContext
            {
                User = claimsPrincipal
            };
            var docId = Guid.NewGuid();
            _mockHttpContextAccessor.Setup(i => i.HttpContext).Returns(context);
            _mockScopedAuthorization.Setup(x => x.ValidateByCompanyId(claimsPrincipal, AuthorizationType.Admin, 42)).Callback(() => { });
            _mockDataContext.Setup(c => c.Document.FindAsync(docId)).ReturnsAsync((Document)null); // Null document

            // Act
            var result = await _documentService.GetDocumentByDocumentIdAsync(docId);

            //Assert
            Assert.Null(result);
        }

        #endregion
        #region GetDocumentDetailsByDocumentIdAsync

        [Fact]
        public async Task GetDocumentDetailsByDocumentIdAsync_WhenPassingCorrectDocumentId_ReturnsDocumentInfo()
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
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                        new Claim("custom:scope", "SystemAdmin")
            }));
            var context = new DefaultHttpContext
            {
                User = claimsPrincipal
            };

            var documentType = new DocumentType
            {
                Id = 1,
                Value = "123",
            };
            Guid docId = Guid.NewGuid();
            var document = new Document
            {
                Id = docId,
                FileName = "INine.Pdf",
                DocumentTypeId = (int)DocumentTypeModel.INine,
                DocumentType = documentType,
                IsDeleted = false,
                PersonId = currentUser.Id,
                CreatedDate = DateTime.Now
            };
            //Return DocumentModel
            var documentModel = new DocumentModel
            {
                Id = document.Id,
                FileName = "INine.Pdf",
                DocumentType = DocumentTypeModel.INine,
                PersonId = currentUser.Id,

            };
            _mockMapper.Setup(m => m.Map<DocumentModel>(It.IsAny<Document>())).Returns(documentModel);
            _mockHttpContextAccessor.Setup(i => i.HttpContext).Returns(context);
            _mockScopedAuthorization.Setup(x => x.ValidateByCompanyId(claimsPrincipal, AuthorizationType.Admin, 42)).Callback(() => { });
            _mockDataContext.Setup(c => c.Document.FindAsync(docId)).ReturnsAsync(document); // Null document

            // Act
            var result = await _documentService.GetDocumentDetailsByDocumentIdAsync(docId);

            //Assert
            Assert.NotNull(result);
            Assert.Equal(document.Id, result.Id);
        }
        [Fact]
        public async Task GetDocumentDetailsByDocumentIdAsync_WhenPassingWrongDocumentId_ReturnsNull()
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
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
                {
                        new Claim("custom:scope", "SystemAdmin")
                }));
            var context = new DefaultHttpContext
            {
                User = claimsPrincipal
            };

            var documentType = new DocumentType
            {
                Id = 1,
                Value = "123",
            };
            Guid docId = Guid.NewGuid();
            var document = new Document
            {
                Id = docId,
                FileName = "INine.Pdf",
                DocumentTypeId = (int)DocumentTypeModel.INine,
                DocumentType = documentType,
                IsDeleted = false,
                PersonId = It.IsAny<int>(),
                CreatedDate = DateTime.Now
            };

            _mockHttpContextAccessor.Setup(i => i.HttpContext).Returns(context);
            _mockPersonService.Setup(person => person.GetCurrentUserAsync()).ReturnsAsync(currentUser);
            _mockScopedAuthorization.Setup(x => x.ValidateByCompanyId(claimsPrincipal, AuthorizationType.Admin, 42)).Callback(() => { });
            _mockDataContext.Setup(c => c.Document.FindAsync(docId)).ReturnsAsync(document); // Null document

            // Act
            var result = await _documentService.GetDocumentDetailsByDocumentIdAsync(docId);

            //Assert
            Assert.Null(result);
        }
        [Fact]
        public async Task GetDocumentDetailsByDocumentIdAsync_WhenNoDocumentProvided_ReturnsNull()
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
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                        new Claim("custom:scope", "SystemAdmin")
            }));
            var context = new DefaultHttpContext
            {
                User = claimsPrincipal
            };
            var docId = Guid.NewGuid();
            _mockHttpContextAccessor.Setup(i => i.HttpContext).Returns(context);
            _mockScopedAuthorization.Setup(x => x.ValidateByCompanyId(claimsPrincipal, AuthorizationType.Admin, 42)).Callback(() => { });
            _mockDataContext.Setup(c => c.Document.FindAsync(docId)).ReturnsAsync((Document)null); // Null document

            // Act
            var result = await _documentService.GetDocumentDetailsByDocumentIdAsync(docId);

            //Assert
            Assert.Null(result);
        }

        #endregion
        #region GetDocumentDetailsByDocumentTypeAsync

        //[Fact]
        //public async Task GetDocumentDetailsByDocumentTypeAsync_WhenPassingCorrectDocumentId_ReturnsDocumentInfo()
        //{
        //    //Arrange
        //    var currentUser = new Person
        //    {
        //        Id = 1,
        //        FirstName = "Biruk",
        //        MiddleName = "Assefa",
        //        LastName = "Wolde",
        //        Email = "biras7070@gmail.com",
        //        MobilePhone = "0921739313",
        //        OtherPhone = "0921739313",
        //        AddressLine1 = "Wawel Street",
        //        AddressLine2 = "Pushkin Road",
        //        AddressLine3 = "Tesfaye Kegela Building",
        //        ZipcodeId = 1,
        //        Ssn = "123456789",
        //        Deactivated = false,
        //        CompanyId = 42,
        //    };
        //    _mockPersonService.Setup(person => person.GetCurrentUserAsync()).ReturnsAsync(currentUser);
        //    var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        //    {
        //                    new Claim("custom:scope", "SystemAdmin")
        //    }));
        //    var context = new DefaultHttpContext
        //    {
        //        User = claimsPrincipal
        //    };

        //    var documentType = new DocumentType
        //    {
        //        Id = 1,
        //        Value = "123",
        //    };
        //    var documents = new List<Document>
        //    {
        //        new Document
        //        {
        //            Id = Guid.NewGuid(),
        //            FileName = "INine.Pdf",
        //            DocumentTypeId = (int)DocumentTypeModel.INine,
        //            DocumentType = documentType,
        //            IsDeleted = false,
        //            PersonId = currentUser.Id,
        //            CreatedDate = DateTime.Now
        //        },
        //        new Document
        //        {
        //            Id = Guid.NewGuid(),
        //            FileName = "INine.Pdf",
        //            DocumentTypeId = (int)DocumentTypeModel.INine,
        //            DocumentType = documentType,
        //            IsDeleted = false,
        //            PersonId = 12,
        //            CreatedDate = DateTime.Now
        //        }
        //    }.AsQueryable();

        //    //var orderedDocuments = documents.OrderByDescending(doc=>doc.CreatedDate);
        //    //Return DocumentModel
        //    var documentModel = new DocumentModel
        //    {
        //        Id = documents.FirstOrDefault().Id,
        //        FileName = "INine.Pdf",
        //        DocumentType = DocumentTypeModel.INine,
        //        PersonId = currentUser.Id,

        //    };
        //    _mockDocumentSet.As<IQueryable<Document>>().Setup(m => m.Provider).Returns(documents.Provider);
        //    _mockDocumentSet.As<IQueryable<Document>>().Setup(m => m.Expression).Returns(documents.Expression);
        //    _mockDocumentSet.As<IQueryable<Document>>().Setup(m => m.ElementType).Returns(documents.ElementType);
        //    _mockDocumentSet.As<IQueryable<Document>>().Setup(m => m.GetEnumerator()).Returns(documents.GetEnumerator());
        //    _mockDataContext.Setup(c => c.Document).Returns(_mockDocumentSet.Object);

        //    _mockMapper.Setup(m => m.Map<DocumentModel>(It.IsAny<Document>())).Returns(documentModel);

        //    _mockHttpContextAccessor.Setup(i => i.HttpContext).Returns(context);
        //    _mockScopedAuthorization.Setup(x => x.ValidateByCompanyId(claimsPrincipal, AuthorizationType.Admin, 42)).Callback(() => { });

        //    _mockDocumentSet.Setup(m => m.FirstOrDefaultAsync(It.IsAny<Expression<Func<Document, bool>>>(), It.IsAny<CancellationToken>()))
        //                    .ReturnsAsync((Expression<Func<Document, bool>> predicate, CancellationToken token) =>
        //                    {
        //                        // Use Compile to convert Expression to Func and filter the people collection
        //                        return documents.FirstOrDefault(predicate.Compile());
        //                    });


        //// Act
        //var result = await _documentService.GetDocumentDetailsByDocumentTypeAsync(DocumentTypeModel.INine);

        //    //Assert
        //    Assert.NotNull(result);
        //    Assert.Equal(documents.FirstOrDefault().Id, result.Id);
        //}

        #endregion
        #region GetDocumentByDocumentTypeAsync

        #endregion
        #region CreateAsync
        //[Fact]
        //public async Task CreateAsync_WhenPassingCorrectParameters_ReturnsSavedDocument()
        //{
        //    //Arrange
        //    var currentUser = new Person
        //    {
        //        Id = 1,
        //        FirstName = "Biruk",
        //        MiddleName = "Assefa",
        //        LastName = "Wolde",
        //        Email = "biras7070@gmail.com",
        //        MobilePhone = "0921739313",
        //        OtherPhone = "0921739313",
        //        AddressLine1 = "Wawel Street",
        //        AddressLine2 = "Pushkin Road",
        //        AddressLine3 = "Tesfaye Kegela Building",
        //        ZipcodeId = 1,
        //        Ssn = "123456789",
        //        Deactivated = false,
        //        CompanyId = 42,
        //    };

        //    var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        //    {
        //         new Claim("custom:scope", "User")
        //    }));
        //    var context = new DefaultHttpContext
        //    {
        //        User = claimsPrincipal
        //    };
        //    var docId = Guid.NewGuid();
        //    var fileStream = new MemoryStream();
        //    _mockHttpContextAccessor.Setup(i => i.HttpContext).Returns(context);
        //    _mockScopedAuthorization.Setup(x => x.ValidateByCompanyId(claimsPrincipal, AuthorizationType.Admin, 42)).Callback(() => { });
        //    //Return DocumentModel
        //    var documentModel = new DocumentModel
        //    {
        //        Id = docId,
        //        FileName = "INine.Pdf",
        //        DocumentType = DocumentTypeModel.INine,
        //        PersonId = currentUser.Id,

        //    };
        //    var document = new Document()
        //    {
        //        Id = docId,
        //        FileName = "INine.Pdf",
        //        DocumentTypeId = (int)DocumentTypeModel.INine,
        //        PersonId = currentUser.Id,
        //    };

        //    _mockMapper.Setup(m => m.Map<Document>(It.IsAny<DocumentModel>())).Returns(document);
        //    _mockDataContext.Setup(x => x.Person).Returns(_mockPersonSet.Object);
        //    _mockDataContext.Setup(c => c.Person.Find(currentUser.Id)).Returns(currentUser);

        //    _mockDocumentManager.Setup(d => d.UploadDocumentAsync(It.IsAny<Guid>(), It.IsAny<Stream>(), DocumentTypeModel.INine)).Returns(Task.CompletedTask);
        //    _mockDataContext.Setup(x => x.Document).Returns(_mockDocumentSet.Object);

        //    //_mockDataContext.Setup(sd => sd.SaveChangesAsync(default)).ReturnsAsync(1);
        //    _mockDataContext.Setup(dc => dc.SaveChangesAsync(It.IsAny<CancellationToken>()))
        //                    .ReturnsAsync(1);
        //    //Act
        //    await _documentService.CreateAsync(documentModel, fileStream);

        //    //Assert
        //    _mockDataContext.Verify(c => c.Document.Add(It.Is<Document>(d => d.DocumentTypeId == (int)DocumentTypeModel.INine && d.PersonId == 1)), Times.Once);

        //    // Verify that SaveChangesAsync was called exactly once
        //    _mockDataContext.Verify(c => c.SaveChangesAsync(default), Times.Once);

        //}
        [Fact]
        public async Task CreateAsync_WhenPassingIncorrectPersonId_ReturnsNull()
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

            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                 new Claim("custom:scope", "User")
            }));
            var context = new DefaultHttpContext
            {
                User = claimsPrincipal
            };
            var docId = Guid.NewGuid();
            var fileStream = new MemoryStream();
            _mockHttpContextAccessor.Setup(i => i.HttpContext).Returns(context);
            _mockScopedAuthorization.Setup(x => x.ValidateByCompanyId(claimsPrincipal, AuthorizationType.User, 42)).Callback(() => { });
            //Return DocumentModel
            var documentModel = new DocumentModel
            {
                Id = docId,
                FileName = "INine.Pdf",
                DocumentType = DocumentTypeModel.INine,
                PersonId = currentUser.Id,

            };
            var document = new Document()
            {
                Id = docId,
                FileName = "INine.Pdf",
                DocumentTypeId = (int)DocumentTypeModel.INine,
                PersonId = currentUser.Id,
            };
            var person = new Person();
            _mockMapper.Setup(m => m.Map<Document>(It.IsAny<DocumentModel>())).Returns(document);
            _mockDataContext.Setup(x => x.Person).Returns(_mockPersonSet.Object);
            _mockDataContext.Setup(c => c.Person.Find(currentUser.Id)).Returns(person);

            _mockDocumentManager.Setup(d => d.UploadDocumentAsync(It.IsAny<Guid>(), It.IsAny<Stream>(), DocumentTypeModel.INine)).Returns(Task.CompletedTask);
            _mockDataContext.Setup(x => x.Document).Returns(_mockDocumentSet.Object);

            _mockDataContext.Setup(sd => sd.SaveChangesAsync(default)).ReturnsAsync(1);

            //Act
            await _documentService.CreateAsync(documentModel, fileStream);

            //Assert 

            // Verify that SaveChangesAsync was called exactly once
            _mockDataContext.Verify(c => c.SaveChangesAsync(default), Times.Never);

        }
        #endregion

        #region Update Document

        [Fact]
        public async Task Update_WhenDocumentInformationUpdated_ReturnsUpdatedDocumentInfo()
        {
            //Arrange
            var docId = Guid.NewGuid();
            var OriginalDocument = new Document()
            {
                Id = docId,
                FileName = "INine.Pdf",
                DocumentTypeId = (int)DocumentTypeModel.INine,
                PersonId = 1,
            };

            var updatedDocument = new Document()
            {
                Id = docId,
                FileName = "INine.Pdf",
                DocumentTypeId = (int)DocumentTypeModel.INineUnsigned,
                PersonId = 1,
            };

            var documentModel = new DocumentModel
            {
                Id = docId,
                FileName = "INine.Pdf",
                DocumentType = DocumentTypeModel.INineUnsigned,
                PersonId = 1,

            };
            
            _mockDataContext.Setup(x => x.Document).Returns(_mockDocumentSet.Object);
            _mockDataContext.Setup(f => f.Document.FindAsync(docId)).ReturnsAsync(OriginalDocument);
            _mockDataContext.Setup(dc => dc.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            _mockMapper.Setup(m => m.Map<Document>(It.IsAny<DocumentModel>())).Returns(updatedDocument);
            //Act
            _documentService.Update(docId, documentModel);
           
            //Assert
            Assert.Equal((int)DocumentTypeModel.INineUnsigned, updatedDocument.DocumentTypeId);
        }
        [Fact]
        public async Task Update_WhenNonExistionDocument_ThrowException()
        {
            //Arrange

            var docId = Guid.NewGuid();
            var document = new Document()
            {
                Id = docId,
                FileName = "INine.Pdf",
                DocumentTypeId = (int)DocumentTypeModel.INineUnsigned,
                PersonId = 1,
            };

            var OriginalDocument = new Document()
            {
                Id = docId,
                FileName = "INine.Pdf",
                DocumentTypeId = (int)DocumentTypeModel.INine,
                PersonId = 1,
            };

            var updatedDocument = new Document()
            {
                Id = docId,
                FileName = "INine.Pdf",
                DocumentTypeId = (int)DocumentTypeModel.INineUnsigned,
                PersonId = 1,
            };

            var documentModel = new DocumentModel
            {
                Id = docId,
                FileName = "INine.Pdf",
                DocumentType = DocumentTypeModel.INineUnsigned,
                PersonId = 1,

            };
            Guid nonExistionGuid = Guid.NewGuid();
            _mockDataContext.Setup(x => x.Document).Returns(_mockDocumentSet.Object);
            _mockDataContext.Setup(dc => dc.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            _mockMapper.Setup(m => m.Map<Document>(It.IsAny<DocumentModel>())).Returns(updatedDocument);

            //Act
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _documentService.Update(nonExistionGuid, documentModel));

            //Assert
            Assert.Equal("Document not found", exception.Message);
        }

        #endregion
        #region Delete Document

        [Fact]
        public async Task Delete_whenDocumentExist_ReturnsDocumentListWithoutDeletedDoc()
        {
            //Arrange
            var docId = Guid.NewGuid();
            var documentList = new List<Document>()
            {
                new Document()
                {
                    Id = docId,
                    FileName = "INine.Pdf",
                    DocumentTypeId = (int)DocumentTypeModel.INine,
                    PersonId = 1
                },
                new Document()
                {
                    Id = Guid.NewGuid(),
                    FileName = "INineUnsigned.Pdf",
                    DocumentTypeId = (int)DocumentTypeModel.INineUnsigned,
                    PersonId = 1,
                }
            };

            _mockDataContext.Setup(x => x.Document).Returns(_mockDocumentSet.Object);
            _mockDataContext.Setup(f => f.Document.FindAsync(docId)).ReturnsAsync(documentList.FirstOrDefault());
            _mockDataContext.Setup(dc => dc.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            //Act
            await _documentService.Delete(docId);

            //Assert
            _mockDocumentSet.Verify(p => p.Remove(It.Is<Document>(p => p.Id == docId)), Times.Once);
        }
        [Fact]
        public async Task Delete_whenDocumentNotExist_ThrowsException()
        {
            //Arrange
            var docId = Guid.NewGuid();
            var documentList = new List<Document>()
            {
                new Document()
                {
                    Id = docId,
                    FileName = "INine.Pdf",
                    DocumentTypeId = (int)DocumentTypeModel.INine,
                    PersonId = 1
                },
                new Document()
                {
                    Id = Guid.NewGuid(),
                    FileName = "INineUnsigned.Pdf",
                    DocumentTypeId = (int)DocumentTypeModel.INineUnsigned,
                    PersonId = 1,
                }
            };

            _mockDataContext.Setup(x => x.Document).Returns(_mockDocumentSet.Object);
            _mockDataContext.Setup(f => f.Document.FindAsync(docId)).ReturnsAsync((Document)null);
            _mockDataContext.Setup(dc => dc.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            //Act
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() => _documentService.Delete(Guid.NewGuid()));

            //Assert
            Assert.Equal("Document not found", exception.Message);
        }

        #endregion
    }
}
