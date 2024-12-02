using Moq;
using Xunit;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Services;
using TelonaiWebApi.Models;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Controllers;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System;
using AutoMapper;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Security.Cryptography;
using System.Text.Json;

namespace TelonaiWebAPI.UnitTest.Controllers
{
   
    public  class DocumentControllerTest
    {
        private readonly Mock<IDocumentService> _mockDocumentService;
        private readonly Mock<IScopedAuthorization> _mockScopedAuthorization;
        private readonly DocumentsController _documentController;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly Mock<HttpContext> _mockHttpContext;
        private readonly Mock<IMapper> _mockMapper;
        private Mock<IPersonService<PersonModel, Person>> _mockPersonService;
        public DocumentControllerTest()
        {
            _mockDocumentService = new  Mock<IDocumentService>();
            _mockScopedAuthorization = new Mock<IScopedAuthorization>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockHttpContext = new Mock<HttpContext>();
            _mockPersonService = new Mock<IPersonService<PersonModel, Person>>();
            _mockMapper = new Mock<IMapper>();
            _documentController = new DocumentsController(_mockDocumentService.Object, _mockScopedAuthorization.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = _mockHttpContext.Object
                }
            };
        }

        #region Get Document By Id

        [Fact]
        public async Task GetDocumentById_WhenDocumentExists_ReturnsDocumentInfo()
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
                    FileName = "INine",
                    DocumentTypeId = (int)DocumentTypeModel.INine,
                    DocumentType = documentType,
                    IsDeleted = false,
                    PersonId = currentUser.Id,
                    CreatedDate = DateTime.Now
                },
                new Document
                {
                    Id = Guid.NewGuid(),
                    FileName = "INine",
                    DocumentTypeId = (int)DocumentTypeModel.INine,
                    DocumentType = documentType,
                    IsDeleted = false,
                    PersonId = 12,
                    CreatedDate = DateTime.Now
                }
            }.AsQueryable();

            Stream stream = new MemoryStream();

            // Provide a string value
            string stringValue = "INine";

            // Create the Tuple<Stream, string> object
            var tuple = new Tuple<Stream, string>(stream, stringValue);

            _mockHttpContextAccessor.Setup(i => i.HttpContext).Returns(context);
            _mockScopedAuthorization.Setup(x => x.ValidateByCompanyId(claimsPrincipal, AuthorizationType.Admin, 42)).Callback(() => { });
            _mockDocumentService.Setup(service => service.GetDocumentByDocumentIdAsync(documents.FirstOrDefault().Id)).ReturnsAsync(tuple);

            // Act
            var result = await _documentController.GetById(documents.FirstOrDefault().Id);

            //Assert
            var okResult = Assert.IsType<FileStreamResult>(result);
            var returnedDocument = Assert.IsType<string>(okResult.FileDownloadName);
            Assert.Equal(documents.FirstOrDefault().FileName+".pdf", returnedDocument);
        }
        [Fact]
        public async Task GetDocumentById_WhenDocumentDoesNotExists_ReturnsIncorrectFileName()
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
                    FileName = "INine",
                    DocumentTypeId = (int)DocumentTypeModel.INineUnsigned,
                    DocumentType = documentType,
                    IsDeleted = false,
                    PersonId = currentUser.Id,
                    CreatedDate = DateTime.Now
                },
                new Document
                {
                    Id = Guid.NewGuid(),
                    FileName = "INine",
                    DocumentTypeId = (int)DocumentTypeModel.INine,
                    DocumentType = documentType,
                    IsDeleted = false,
                    PersonId = 12,
                    CreatedDate = DateTime.Now
                }
            }.AsQueryable();

            Stream stream = new MemoryStream();

            // Provide a string value
            string stringValue = "INineUnsigned";

            // Create the Tuple<Stream, string> object
            var tuple = new Tuple<Stream, string>(stream, stringValue);

            _mockHttpContextAccessor.Setup(i => i.HttpContext).Returns(context);
            _mockScopedAuthorization.Setup(x => x.ValidateByCompanyId(claimsPrincipal, AuthorizationType.Admin, 42)).Callback(() => { });
            _mockDocumentService.Setup(service => service.GetDocumentByDocumentIdAsync(documents.FirstOrDefault().Id)).ReturnsAsync(tuple);

            // Act
            var result = await _documentController.GetById(documents.FirstOrDefault().Id);

            //Assert
            var okResult = Assert.IsType<FileStreamResult>(result);
            var returnedDocument = Assert.IsType<string>(okResult.FileDownloadName);
            Assert.NotEqual(documents.FirstOrDefault().FileName + ".pdf", returnedDocument);
        }

        #endregion
        #region Get Own Document by Id

        [Fact]
        public async Task GetOwnById_WhenDocumentExists_ReturnsDocumentInfo()
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
                    FileName = "INine",
                    DocumentTypeId = (int)DocumentTypeModel.INine,
                    DocumentType = documentType,
                    IsDeleted = false,
                    PersonId = currentUser.Id,
                    CreatedDate = DateTime.Now
                },
                new Document
                {
                    Id = Guid.NewGuid(),
                    FileName = "INine",
                    DocumentTypeId = (int)DocumentTypeModel.INine,
                    DocumentType = documentType,
                    IsDeleted = false,
                    PersonId = 12,
                    CreatedDate = DateTime.Now
                }
            }.AsQueryable();

            Stream stream = new MemoryStream();

            // Provide a string value
            string stringValue = "INine";

            // Create the Tuple<Stream, string> object
            var tuple = new Tuple<Stream, string>(stream, stringValue);

            _mockHttpContextAccessor.Setup(i => i.HttpContext).Returns(context);
            _mockScopedAuthorization.Setup(x => x.ValidateByCompanyId(claimsPrincipal, AuthorizationType.Admin, 42)).Callback(() => { });
            _mockDocumentService.Setup(service => service.GetDocumentByDocumentIdAsync(documents.FirstOrDefault().Id)).ReturnsAsync(tuple);

            // Act
            var result = await _documentController.GetOwnById(documents.FirstOrDefault().Id);

            //Assert
            var okResult = Assert.IsType<FileStreamResult>(result);
            var returnedDocument = Assert.IsType<string>(okResult.FileDownloadName);
            Assert.Equal(documents.FirstOrDefault().FileName + ".pdf", returnedDocument);
        }

        [Fact]
        public async Task GetOwnById_WhenDocumentDoesNotExists_ReturnsIncorrectFileName()
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
                    FileName = "INine",
                    DocumentTypeId = (int)DocumentTypeModel.INineUnsigned,
                    DocumentType = documentType,
                    IsDeleted = false,
                    PersonId = currentUser.Id,
                    CreatedDate = DateTime.Now
                },
                new Document
                {
                    Id = Guid.NewGuid(),
                    FileName = "INine",
                    DocumentTypeId = (int)DocumentTypeModel.INine,
                    DocumentType = documentType,
                    IsDeleted = false,
                    PersonId = 12,
                    CreatedDate = DateTime.Now
                }
            }.AsQueryable();

            Stream stream = new MemoryStream();

            // Provide a string value
            string stringValue = "INineUnsigned";

            // Create the Tuple<Stream, string> object
            var tuple = new Tuple<Stream, string>(stream, stringValue);

            _mockHttpContextAccessor.Setup(i => i.HttpContext).Returns(context);
            _mockScopedAuthorization.Setup(x => x.ValidateByCompanyId(claimsPrincipal, AuthorizationType.Admin, 42)).Callback(() => { });
            _mockDocumentService.Setup(service => service.GetDocumentByDocumentIdAsync(documents.FirstOrDefault().Id)).ReturnsAsync(tuple);

            // Act
            var result = await _documentController.GetOwnById(documents.FirstOrDefault().Id);

            //Assert
            var okResult = Assert.IsType<FileStreamResult>(result);
            var returnedDocument = Assert.IsType<string>(okResult.FileDownloadName);
            Assert.NotEqual(documents.FirstOrDefault().FileName + ".pdf", returnedDocument);
        }


        #endregion
        #region Get Document Details By Id

        [Fact]
        public async Task GetDetailsById_WhenPassingDocumentId_ReturnsDocumentDetail()
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
                    FileName = "INine.pdf",
                    DocumentTypeId = (int)DocumentTypeModel.INine,
                    DocumentType = documentType,
                    IsDeleted = false,
                    PersonId = currentUser.Id,
                    CreatedDate = DateTime.Now
                },
                new Document
                {
                    Id = Guid.NewGuid(),
                    FileName = "INine.pdf",
                    DocumentTypeId = (int)DocumentTypeModel.INine,
                    DocumentType = documentType,
                    IsDeleted = false,
                    PersonId = 12,
                    CreatedDate = DateTime.Now
                }
            }.AsQueryable();

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
            _mockDocumentService.Setup(service => service.GetDocumentDetailsByDocumentIdAsync(documents.FirstOrDefault().Id)).ReturnsAsync(documentModel);

            // Act
            var result = await _documentController.GetDetailsById(documents.FirstOrDefault().Id);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedDocument = Assert.IsType<DocumentModel>(okResult.Value);
            Assert.Equal(documents.FirstOrDefault().Id, returnedDocument.Id);
        }
        [Fact]
        public async Task GetDetailsById_WhenPassingNonExistingDocumentId_ReturnsNull()
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
                    FileName = "INine.pdf",
                    DocumentTypeId = (int)DocumentTypeModel.INine,
                    DocumentType = documentType,
                    IsDeleted = false,
                    PersonId = currentUser.Id,
                    CreatedDate = DateTime.Now
                },
                new Document
                {
                    Id = Guid.NewGuid(),
                    FileName = "INine.pdf",
                    DocumentTypeId = (int)DocumentTypeModel.INine,
                    DocumentType = documentType,
                    IsDeleted = false,
                    PersonId = 12,
                    CreatedDate = DateTime.Now
                }
            }.AsQueryable();

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
            _mockDocumentService.Setup(service => service.GetDocumentDetailsByDocumentIdAsync(Guid.NewGuid())).ReturnsAsync((DocumentModel)null);

            // Act
            var result = await _documentController.GetDetailsById(documents.FirstOrDefault().Id);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Null(okResult.Value);
        }

        #endregion
        #region Get Own Document Details By Id

        [Fact]
        public async Task GetOwnDetailsById_WhenPassingDocumentId_ReturnsDocumentDetail()
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
                    FileName = "INine.pdf",
                    DocumentTypeId = (int)DocumentTypeModel.INine,
                    DocumentType = documentType,
                    IsDeleted = false,
                    PersonId = currentUser.Id,
                    CreatedDate = DateTime.Now
                },
                new Document
                {
                    Id = Guid.NewGuid(),
                    FileName = "INine.pdf",
                    DocumentTypeId = (int)DocumentTypeModel.INine,
                    DocumentType = documentType,
                    IsDeleted = false,
                    PersonId = 12,
                    CreatedDate = DateTime.Now
                }
            }.AsQueryable();

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
            _mockDocumentService.Setup(service => service.GetDocumentDetailsByDocumentIdAsync(documents.FirstOrDefault().Id)).ReturnsAsync(documentModel);

            // Act
            var result = await _documentController.GetOwnDetailsById(documents.FirstOrDefault().Id);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedDocument = Assert.IsType<DocumentModel>(okResult.Value);
            Assert.Equal(documents.FirstOrDefault().Id, returnedDocument.Id);
        }
        [Fact]
        public async Task GetOwnDetailsById_WhenPassingNonExistingDocumentId_ReturnsNull()
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
                    FileName = "INine.pdf",
                    DocumentTypeId = (int)DocumentTypeModel.INine,
                    DocumentType = documentType,
                    IsDeleted = false,
                    PersonId = currentUser.Id,
                    CreatedDate = DateTime.Now
                },
                new Document
                {
                    Id = Guid.NewGuid(),
                    FileName = "INine.pdf",
                    DocumentTypeId = (int)DocumentTypeModel.INine,
                    DocumentType = documentType,
                    IsDeleted = false,
                    PersonId = 12,
                    CreatedDate = DateTime.Now
                }
            }.AsQueryable();

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
            _mockDocumentService.Setup(service => service.GetDocumentDetailsByDocumentIdAsync(Guid.NewGuid())).ReturnsAsync((DocumentModel)null);

            // Act
            var result = await _documentController.GetOwnDetailsById(documents.FirstOrDefault().Id);

            //Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Null(okResult.Value);
        }

        #endregion
        #region Get own document by documentType 

        [Fact]
        public async Task GetOwnDocumentByDocumentType_WhenPassingDocumentType_ReturnsOwnDocument()
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
                    FileName = "INine",
                    DocumentTypeId = (int)DocumentTypeModel.INine,
                    DocumentType = documentType,
                    IsDeleted = false,
                    PersonId = currentUser.Id,
                    CreatedDate = DateTime.Now
                },
                new Document
                {
                    Id = Guid.NewGuid(),
                    FileName = "INine",
                    DocumentTypeId = (int)DocumentTypeModel.INine,
                    DocumentType = documentType,
                    IsDeleted = false,
                    PersonId = 12,
                    CreatedDate = DateTime.Now
                }
            }.AsQueryable();

            var documentModel = new DocumentModel
            {
                Id = documents.ToList().FirstOrDefault().Id,
                FileName = "INine.Pdf",
                DocumentType = DocumentTypeModel.INine,
                PersonId = currentUser.Id,

            };
            Stream stream = new MemoryStream();

            // Provide a string value
            string stringValue = "INine";

            // Create the Tuple<Stream, string> object
            var documentTuple = new Tuple<Stream, string>(stream, stringValue);
            _mockMapper.Setup(m => m.Map<DocumentModel>(It.IsAny<Document>())).Returns(documentModel);
            _mockHttpContextAccessor.Setup(i => i.HttpContext).Returns(context);
            _mockScopedAuthorization.Setup(x => x.ValidateByCompanyId(claimsPrincipal, AuthorizationType.Admin, 42)).Callback(() => { });
            _mockDocumentService.Setup(service => service.GetOwnDocumentByDocumentTypeAsync(DocumentTypeModel.INine)).ReturnsAsync(documentTuple);

            // Act
            var result = await _documentController.GetOwnByDocumentType(DocumentTypeModel.INine);

            //Assert
            var okResult = Assert.IsType<FileStreamResult>(result);
            var returnedDocument = Assert.IsType<string>(okResult.FileDownloadName);
            Assert.Equal(documents.FirstOrDefault().FileName + ".pdf", returnedDocument);
        }

        [Fact]
        public async Task GetOwnDocumentByDocumentType_WhenDifferentDocumentFile_ReturnsDifferentDocument()
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
                    FileName = "INine",
                    DocumentTypeId = (int)DocumentTypeModel.INine,
                    DocumentType = documentType,
                    IsDeleted = false,
                    PersonId = currentUser.Id,
                    CreatedDate = DateTime.Now
                },
                new Document
                {
                    Id = Guid.NewGuid(),
                    FileName = "INine",
                    DocumentTypeId = (int)DocumentTypeModel.INine,
                    DocumentType = documentType,
                    IsDeleted = false,
                    PersonId = 12,
                    CreatedDate = DateTime.Now
                }
            }.AsQueryable();

            var documentModel = new DocumentModel
            {
                Id = documents.ToList().FirstOrDefault().Id,
                FileName = "INine.Pdf",
                DocumentType = DocumentTypeModel.INine,
                PersonId = currentUser.Id,

            };
            Stream stream = new MemoryStream();

            // Provide a string value
            string stringValue = "INine";

            // Create the Tuple<Stream, string> object
            var documentTuple = new Tuple<Stream, string>(stream, stringValue);
            _mockMapper.Setup(m => m.Map<DocumentModel>(It.IsAny<Document>())).Returns(documentModel);
            _mockHttpContextAccessor.Setup(i => i.HttpContext).Returns(context);
            _mockScopedAuthorization.Setup(x => x.ValidateByCompanyId(claimsPrincipal, AuthorizationType.Admin, 42)).Callback(() => { });
            _mockDocumentService.Setup(service => service.GetOwnDocumentByDocumentTypeAsync(DocumentTypeModel.INine)).ReturnsAsync((Tuple<Stream, string>)null);

            // Act
            var result = await _documentController.GetOwnByDocumentType(DocumentTypeModel.INineUnsigned);

            //Assert
            Assert.IsType<NotFoundResult>(result);
        }


        #endregion
        #region Get document by documenttype

        [Fact]
        public async Task GetByDocumentType_WhenPassingDocumentType_ReturnsOwnDocument()
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
                    FileName = "INine",
                    DocumentTypeId = (int)DocumentTypeModel.INineUnsigned,
                    DocumentType = documentType,
                    IsDeleted = false,
                    PersonId = currentUser.Id,
                    CreatedDate = DateTime.Now
                },
                new Document
                {
                    Id = Guid.NewGuid(),
                    FileName = "INine",
                    DocumentTypeId = (int)DocumentTypeModel.INine,
                    DocumentType = documentType,
                    IsDeleted = false,
                    PersonId = 12,
                    CreatedDate = DateTime.Now
                }
            }.AsQueryable();

            Stream stream = new MemoryStream();

            // Provide a string value
            string stringValue = "INine";

            // Create the Tuple<Stream, string> object
            var tuple = new Tuple<Stream, string>(stream, stringValue);

            _mockHttpContextAccessor.Setup(i => i.HttpContext).Returns(context);
            _mockScopedAuthorization.Setup(x => x.ValidateByCompanyId(claimsPrincipal, AuthorizationType.Admin, 42)).Callback(() => { });
            _mockDocumentService.Setup(service => service.GetDocumentByDocumentTypeAsync(DocumentTypeModel.INine)).ReturnsAsync(tuple);

            // Act
            var result = await _documentController.GetByDocumentTypeEmployer(DocumentTypeModel.INine);

            //Assert
            var okResult = Assert.IsType<FileStreamResult>(result);
            var returnedDocument = Assert.IsType<string>(okResult.FileDownloadName);
            Assert.Equal(documents.FirstOrDefault().FileName + ".pdf", returnedDocument);
        }

        [Fact]
        public async Task GetByDocumentType_WhenPassingNonExistingDocumentType_ReturnsNotFound()
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
                    FileName = "INine",
                    DocumentTypeId = (int)DocumentTypeModel.INine,
                    DocumentType = documentType,
                    IsDeleted = false,
                    PersonId = currentUser.Id,
                    CreatedDate = DateTime.Now
                },
                new Document
                {
                    Id = Guid.NewGuid(),
                    FileName = "INine",
                    DocumentTypeId = (int)DocumentTypeModel.INine,
                    DocumentType = documentType,
                    IsDeleted = false,
                    PersonId = 12,
                    CreatedDate = DateTime.Now
                }
            }.AsQueryable();

            Stream stream = new MemoryStream();

            // Provide a string value
            string stringValue = "INineUnsigned";

            // Create the Tuple<Stream, string> object
            var tuple = new Tuple<Stream, string>(stream, stringValue);

            _mockHttpContextAccessor.Setup(i => i.HttpContext).Returns(context);
            _mockScopedAuthorization.Setup(x => x.ValidateByCompanyId(claimsPrincipal, AuthorizationType.Admin, 42)).Callback(() => { });
            _mockDocumentService.Setup(service => service.GetDocumentByDocumentTypeAsync(DocumentTypeModel.INineUnsigned)).ReturnsAsync((Tuple<Stream, string>)null);

            // Act
            var result = await _documentController.GetByDocumentTypeEmployer(DocumentTypeModel.INineUnsigned);

            //Assert
            Assert.IsType<NotFoundResult>(result);
        }

        #endregion
        #region Add Document

        [Fact]
        public async Task AddDocument_WhenDocumentInfoValid_ReturnsDocumentInfo()
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
            var docId = Guid.NewGuid();
            var document = new Document
            {
                Id = docId,
                FileName = "INine",
                DocumentTypeId = (int)DocumentTypeModel.INine,
                DocumentType = documentType,
                IsDeleted = false,
                PersonId = currentUser.Id,
                CreatedDate = DateTime.Now
            };
            var documentModel = new DocumentModel
            {
                Id = docId,
                FileName = "INine.Pdf",
                DocumentType = DocumentTypeModel.INine,
                PersonId = currentUser.Id,

            };

            var fileMock = new Mock<IFormFile>();
            var content = "File content";
            var fileName = "testfile.txt";
            var ms = new MemoryStream();
            var writer = new StreamWriter(ms);
            writer.Write(content);
            writer.Flush();
            ms.Position = 0;

            fileMock.Setup(f => f.OpenReadStream()).Returns(ms);
            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(ms.Length);

           
            _mockHttpContextAccessor.Setup(i => i.HttpContext).Returns(context);
            _mockMapper.Setup(m => m.Map<Document>(It.IsAny<DocumentModel>())).Returns(document);
            _mockScopedAuthorization.Setup(x => x.ValidateByCompanyId(claimsPrincipal, AuthorizationType.Admin, 42)).Callback(() => { });
            _mockDocumentService.Setup(service => service.CreateAsync(documentModel, ms));

            // Act
            var result = await _documentController.AddDocument(fileMock.Object, documentModel);

            //Assert
            Assert.IsType<OkObjectResult>(result);
        }

        #endregion

        #region Update Document

        [Fact]
        public async Task UpdateDocument_WhenDocumentExists_UpdateTheDocument()
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
            var docId = Guid.NewGuid();
            var document = new Document
            {
                Id = Guid.NewGuid(),
                FileName = "INine",
                DocumentTypeId = (int)DocumentTypeModel.INine,
                DocumentType = documentType,
                IsDeleted = false,
                PersonId = currentUser.Id,
                CreatedDate = DateTime.Now
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
            _mockHttpContext.Setup(c => c.Request.HttpContext.User).Returns(claimsPrincipal);
            _mockPersonService.Setup(person => person.GetCurrentUserAsync()).ReturnsAsync(currentUser);
            _mockMapper.Setup(m => m.Map<DocumentModel>(It.IsAny<Document>())).Returns(documentModel);
            _mockHttpContextAccessor.Setup(i => i.HttpContext).Returns(context);
            _mockScopedAuthorization.Setup(auth => auth.Validate(It.IsAny<ClaimsPrincipal>(), AuthorizationType.SystemAdmin)).Callback(() => { });
            _mockDocumentService.Setup(service => service.GetDocument(docId)).ReturnsAsync(document);
            _mockDocumentService.Setup(service => service.GetDocumentDetailsByDocumentIdAsync(docId)).ReturnsAsync(documentModel);
            // Act
            var result = await _documentController.Update(docId, documentModel);

            //Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task UpdateDocument_WhenDocumentNotExists_UpdateTheDocumentFails()
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
            var docId = Guid.NewGuid();
            var document = new Document
            {
                Id = Guid.NewGuid(),
                FileName = "INine",
                DocumentTypeId = (int)DocumentTypeModel.INine,
                DocumentType = documentType,
                IsDeleted = false,
                PersonId = currentUser.Id,
                CreatedDate = DateTime.Now
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
            _mockHttpContext.Setup(c => c.Request.HttpContext.User).Returns(claimsPrincipal);
            _mockPersonService.Setup(person => person.GetCurrentUserAsync()).ReturnsAsync(currentUser);
            _mockMapper.Setup(m => m.Map<DocumentModel>(It.IsAny<Document>())).Returns(documentModel);
            _mockHttpContextAccessor.Setup(i => i.HttpContext).Returns(context);
            _mockScopedAuthorization.Setup(auth => auth.Validate(It.IsAny<ClaimsPrincipal>(), AuthorizationType.SystemAdmin)).Callback(() => { });
                _mockDocumentService.Setup(service => service.GetDocument(Guid.NewGuid())).ReturnsAsync((Document)null);

            // Act
            var result = await _documentController.Update(docId, documentModel);

            //Assert
            Assert.IsType<NotFoundResult>(result);
        }

        #endregion

        #region Delete Document 

        [Fact]
        public async Task DeleteDocument_WhenDocumentExists_UpdateTheDocument()
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
            var docId = Guid.NewGuid();
            var document = new Document
            {
                Id = Guid.NewGuid(),
                FileName = "INine",
                DocumentTypeId = (int)DocumentTypeModel.INine,
                DocumentType = documentType,
                IsDeleted = false,
                PersonId = currentUser.Id,
                CreatedDate = DateTime.Now
            };
            
            _mockHttpContext.Setup(c => c.Request.HttpContext.User).Returns(claimsPrincipal);
            _mockPersonService.Setup(person => person.GetCurrentUserAsync()).ReturnsAsync(currentUser);
            _mockHttpContextAccessor.Setup(i => i.HttpContext).Returns(context);
            _mockScopedAuthorization.Setup(auth => auth.Validate(It.IsAny<ClaimsPrincipal>(), AuthorizationType.SystemAdmin)).Callback(() => { });

            // Act
            var result = await _documentController.Delete(docId);

            //Assert
            Assert.IsType<OkObjectResult>(result);
        }

        #endregion
    }
}
