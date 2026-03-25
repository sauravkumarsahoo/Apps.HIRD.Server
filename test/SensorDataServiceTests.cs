using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using HIRD.Service;
using HIRD.Proto;
using System.Threading.Tasks;

namespace HwInfoDisplayTests
{
    public class SensorDataServiceTests
    {
        [Fact]
        public async Task TestGetComputerInfo()
        {
            var loggerMock = new Mock<ILogger<SensorDataService>>();
            var providerMock = new Mock<ISensorProvider>();
            var expectedInfo = new ComputerInfo { ComputerName = "TestPC" };
            providerMock.Setup(p => p.GetComputerInfo()).Returns(expectedInfo);
            providerMock.Setup(p => p.GetSensorInterval()).Returns(2000);

            var service = new SensorDataService(loggerMock.Object, providerMock.Object);
            var result = await service.GetComputerInfo(new ComputerInfoRequest(), TestServerCallContext.Create());

            Assert.Equal("TestPC", result.ComputerName);
        }
    }
}
