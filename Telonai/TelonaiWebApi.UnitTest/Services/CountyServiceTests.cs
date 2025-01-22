using Moq;
using TelonaiWebApi.Helpers;

namespace TelonaiWebApi.Services
{
    public class CountyServiceTests
    {
        public Mock<DataContext> _mockContext;
        public CountyService _mockCountyService;
        public CountyServiceTests()
        {
            _mockContext = new Mock<DataContext>();
            _mockCountyService = new CountyService(_mockContext.Object);
        }
    }
}
