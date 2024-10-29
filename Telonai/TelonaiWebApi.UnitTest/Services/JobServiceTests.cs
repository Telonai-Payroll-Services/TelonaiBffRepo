using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Models;
using TelonaiWebApi.Services;

namespace TelonaiWebApi.UnitTest.Services
{
    public class JobServiceTests
    {
        private readonly Mock<DataContext> _mockContext;
        private readonly Mock<IMapper> _mockMapper;
        private readonly Mock<IUserService> _mockUserService;
        private readonly JobService _service;
        public JobServiceTests() 
        {
            _mockContext = new Mock<DataContext>(MockBehavior.Default, new object[] { new Mock<IHttpContextAccessor>().Object });
            _mockMapper=new Mock<IMapper>();
            _mockUserService = new Mock<IUserService>();
            _service = new JobService(_mockContext.Object,_mockMapper.Object,_mockUserService.Object);

        }

        [Fact]
        public void Get_ReturnsMappedJobModels()
        {
            
            var jobs = new List<Job>
        {
            new Job { Id = 1, LocationName = "JobLocation1" },
            new Job { Id = 2, LocationName = "JobLocation2" }
        }.AsQueryable();

            var jobModels = new List<JobModel>
                 {
            new JobModel { Id = 1, LocationName = "JobLocation1" },
            new JobModel { Id = 2, LocationName = "JobLocation1" }
        };

            var mockSet = new Mock<DbSet<Job>>();
            mockSet.As<IQueryable<Job>>().Setup(m => m.Provider).Returns(jobs.Provider);
            mockSet.As<IQueryable<Job>>().Setup(m => m.Expression).Returns(jobs.Expression);
            mockSet.As<IQueryable<Job>>().Setup(m => m.ElementType).Returns(jobs.ElementType);
            mockSet.As<IQueryable<Job>>().Setup(m => m.GetEnumerator()).Returns(jobs.GetEnumerator());

            _mockContext.Setup(c => c.Job).Returns(mockSet.Object);
            _mockMapper.Setup(m => m.Map<IList<JobModel>>(It.IsAny<IQueryable<Job>>())).Returns(jobModels);
          
            var result = _service.Get();
           
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("JobLocation1", result.First().LocationName);
        }
        [Fact]
        public void GetById_ReturnsMappedJobModel()
        {
            
            var job = new Job { Id = 1, LocationName = "JobLocation1" };
            var jobModel = new JobModel { Id = 1, LocationName = "JobLocation1" };

            var mockSet = new Mock<DbSet<Job>>();
            mockSet.Setup(m => m.Find(It.IsAny<int>())).Returns(job);

            _mockContext.Setup(c => c.Job).Returns(mockSet.Object);
            _mockMapper.Setup(m => m.Map<JobModel>(It.IsAny<Job>())).Returns(jobModel);
            
            var result = _service.GetById(1);
            
            Assert.NotNull(result);
            Assert.Equal("JobLocation1", result.LocationName);
        }
        [Fact]
        public async Task CreateAsync_CreatesNewJobAndSignsUpUser()
        {
      
            var jobModel = new JobModel { CompanyId = 1, LocationName = "Location1" };
            var job = new Job { Id = 1, CompanyId = 1, LocationName = "Location1" };
            var user = new User { Email = "test@example.com", Username = "testuser" };

            var mockSet = new Mock<DbSet<Job>>();
            mockSet.As<IQueryable<Job>>().Setup(m => m.Provider).Returns(new List<Job>().AsQueryable().Provider);
            mockSet.As<IQueryable<Job>>().Setup(m => m.Expression).Returns(new List<Job>().AsQueryable().Expression);
            mockSet.As<IQueryable<Job>>().Setup(m => m.ElementType).Returns(new List<Job>
                ().AsQueryable().ElementType);
            mockSet.As<IQueryable<Job>>().Setup(m => m.GetEnumerator()).Returns(new List<Job>().AsQueryable().GetEnumerator());
           

            _mockContext.Setup(c => c.Job).Returns(mockSet.Object);
            _mockMapper.Setup(m => m.Map<Job>(It.IsAny<JobModel>())).Returns(job);
            _mockUserService.Setup(u => u.SignUpAsync(It.IsAny<User>(), It.IsAny<UserRole>(), It.IsAny<int>(), It.IsAny<int>())).Returns(Task.FromResult(IdentityResult.Success));
            
            var result = await _service.CreateAsync(jobModel, user.Username, user.Email);

            
            Assert.NotNull(result);
            Assert.Equal(job.CompanyId, result.CompanyId);
            Assert.Equal(job.LocationName, result.LocationName);
            _mockContext.Verify(c => c.SaveChangesAsync(), Times.Once);
            _mockUserService.Verify(u => u.SignUpAsync(It.Is<User>(usr => usr.Email == user.Email && usr.Username == user.Username), UserRole.Admin, jobModel.CompanyId, job.Id), Times.Once);
        }
        [Fact]
        public async Task UpdateAsync_UpdatesJob()
        {
           
            var jobModel = new JobModel { Id = 1, LocationName = "Updated Location"  };
            var job = new Job { Id = 1, LocationName = "Original Location" };

            var mockSet = new Mock<DbSet<Job>>();
            mockSet.Setup(m => m.Find(It.IsAny<int>())).Returns(job);

            _mockContext.Setup(c => c.Job).Returns(mockSet.Object);
            _mockMapper.Setup(m => m.Map(It.IsAny<JobModel>(), It.IsAny<Job>())).Callback<JobModel, Job>((src, dest) => dest.LocationName = src.LocationName);
            
            await _service.UpdateAsync(1, jobModel);

            
            Assert.Equal("Updated Location", job.LocationName);
            _mockContext.Verify(c => c.SaveChangesAsync(), Times.Once);
        }
        [Fact]
        public async Task DeleteAsync_RemovesJob()
        {
            
            var job = new Job { Id = 1, LocationName = "JobLocation1" };

            var mockSet = new Mock<DbSet<Job>>();
            mockSet.Setup(m => m.Find(It.IsAny<int>())).Returns(job);

            _mockContext.Setup(c => c.Job).Returns(mockSet.Object);

            
            await _service.DeleteAsync(1);
            
            mockSet.Verify(m => m.Remove(It.Is<Job>(j => j.Id == 1)), Times.Once);
            _mockContext.Verify(c => c.SaveChangesAsync(), Times.Once);
        }
    }
    }
