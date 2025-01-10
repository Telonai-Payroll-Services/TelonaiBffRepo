using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.Xunit2;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using TelonaiWebApi.Entities;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Services;
using TelonaiWebAPI.UnitTest.Helper;
using Xunit;

public class TimeCardServiceTests
{
    private readonly Fixture _fixture;
    private readonly Mock<DataContext> _mockContext;
    private readonly Mock<IMailSender> _mockEmailService;
    private readonly Mock<IMapper> _mockMapper;
    // private readonly IFixture _fixture;

    public TimeCardServiceTests()
    {
        _fixture = CustomFixture.Create();
        _mockContext = new Mock<DataContext>();
        _mockMapper = new Mock<IMapper>();
        _mockEmailService = new Mock<IMailSender>();

    }

    [Fact]
    public async Task CheckOverdueClockOutsAsync_SendsNotificationsAndAddsNotes()
    {
        // Arrange
        var now = DateTime.UtcNow;

        var timeCards = new List<TimecardUsa>
        {
            _fixture.Build<TimecardUsa>()
                .With(tc => tc.ClockIn, now.AddHours(-24))
                .With(tc => tc.Person, _fixture.Build<Person>()
                    .With(p => p.Email, "test@example.com")
                    .With(p => p.FirstName, "John")
                    .With(p => p.LastName, "Doe")
                    .Create())
                .Create(),
            _fixture.Build<TimecardUsa>()
                .With(tc => tc.ClockIn, now.AddHours(-16))
                .With(tc => tc.Person, _fixture.Build<Person>()
                    .With(p => p.Email, "test2@example.com")
                    .With(p => p.FirstName, "Jane")
                    .With(p => p.LastName, "Doe")
                    .Create())
                .Create(),
            _fixture.Build<TimecardUsa>()
                .With(tc => tc.ClockIn, now.AddHours(-8))
                .With(tc => tc.Person, _fixture.Build<Person>()
                    .With(p => p.Email, "test3@example.com")
                    .With(p => p.FirstName, "Mark")
                    .With(p => p.LastName, "Smith")
                    .Create())
                .Create()
        };

        var timeCardDbSet = MockDbSet(timeCards);
        _mockContext.Setup(c => c.TimecardUsa).Returns(timeCardDbSet.Object);

        var service = new TimecardUsaService(_mockContext.Object, _mockMapper.Object, _mockEmailService.Object);

        // Act
        await service.CheckOverdueClockOutsAsync();

        // Assert
        _mockEmailService.Verify(m => m.SendUsingAwsClientAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(3));
        _mockContext.Verify(c => c.TimecardUsaNote.Add(It.IsAny<TimecardUsaNote>()), Times.Exactly(3));
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(4));  // 1 for clocking out, 3 for adding notes
    }

    private Mock<DbSet<T>> MockDbSet<T>(List<T> data) where T : class
    {
        var queryable = data.AsQueryable();
        var dbSet = new Mock<DbSet<T>>();
        dbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
        dbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
        dbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        dbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
        return dbSet;
    }
}
