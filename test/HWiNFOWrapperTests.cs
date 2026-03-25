using System;
using Xunit;
using Newtonsoft.Json;
using HIRD.HWiNFOAccess;

namespace HwInfoDisplayTests
{
    public class HWiNFOWrapperTests
    {
        private readonly HWiNFOSharedMemoryAccessor _hWiNFO;

        public HWiNFOWrapperTests()
        {
            _hWiNFO = new();
        }

        [Fact(Skip = "Requires HWiNFO on Windows")]
        public void TestGetComputerInfo()
        {
            var computerInfo = _hWiNFO.GetComputerInfo();

            Assert.NotNull(computerInfo);

            Assert.NotEmpty(computerInfo.ComputerName);
            Assert.NotEmpty(computerInfo.CpuName);
            Assert.NotNull(computerInfo.GpuName);
            Assert.NotEmpty(computerInfo.SystemMake);
            Assert.NotEmpty(computerInfo.Memory);
            Assert.NotEmpty(computerInfo.StorageNames);
        }

        [Fact(Skip = "Requires HWiNFO on Windows")]
        public void TestGetStreamData()
        {
            _ = _hWiNFO.GetComputerInfo();

            var readingData = _hWiNFO.GetReadingData();

            string output = JsonConvert.SerializeObject(readingData, Formatting.Indented);
            Console.WriteLine(output);

            Assert.NotNull(readingData);
        }
    }
}
