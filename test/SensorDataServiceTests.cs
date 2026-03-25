using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using HIRD.Service;
using HIRD.Proto;
using Grpc.Core;
using System.Threading.Tasks;
using System.Threading;
using System;

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

            var context = new MockServerCallContext();

            var service = new SensorDataService(loggerMock.Object, providerMock.Object);
            var result = await service.GetComputerInfo(new ComputerInfoRequest(), context);

            Assert.Equal("TestPC", result.ComputerName);
        }
    }

    public class MockServerCallContext : ServerCallContext
    {
        protected override string PeerCore => "127.0.0.1";
        protected override string HostCore => "localhost";
        protected override string MethodCore => "GetComputerInfo";
        protected override DateTime DeadlineCore => DateTime.UtcNow.AddMinutes(1);
        protected override Metadata RequestHeadersCore => new Metadata();
        protected override CancellationToken CancellationTokenCore => CancellationToken.None;
        protected override Metadata ResponseTrailersCore => new Metadata();
        protected override Status StatusCore { get; set; }
        protected override WriteOptions? WriteOptionsCore { get; set; }
        protected override AuthContext? AuthContextCore => null;

        protected override ContextPropagationToken? CreatePropagationTokenCore(ContextPropagationOptions? options) => null;
        protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders) => Task.CompletedTask;
    }
}
