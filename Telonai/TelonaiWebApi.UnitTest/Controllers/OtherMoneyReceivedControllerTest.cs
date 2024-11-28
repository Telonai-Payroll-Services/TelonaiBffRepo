using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TelonaiWebApi.Controllers;
using TelonaiWebApi.Helpers;
using TelonaiWebApi.Services;

namespace TelonaiWebAPI.UnitTest.Controllers
{
    public class OtherMoneyReceivedControllerTest
    {
        private readonly Mock<IScopedAuthorization> _mockScopedAuthorization;
        private readonly Mock<IOtherMoneyReceivedService> _mockOtherMoneyReceivedService;
        private readonly Mock<OtherIncomeController> _otherMoneyReceivedController;
        public OtherMoneyReceivedControllerTest()
        {
          _mockScopedAuthorization = new  Mock<IScopedAuthorization>();
          _mockOtherMoneyReceivedService = new Mock<IOtherMoneyReceivedService>();
          _otherMoneyReceivedController = new OtherIncomeController(_mockOtherMoneyReceivedService.Object, _mockScopedAuthorization.Object);
        }
    }
}
