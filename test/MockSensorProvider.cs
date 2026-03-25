using HIRD.Proto;
using HIRD.Core;

namespace HwInfoDisplayTests
{
    public class MockSensorProvider : ISensorProvider
    {
        public ComputerInfo GetComputerInfo()
        {
            return new ComputerInfo
            {
                ComputerName = "MockPC",
                CpuName = "MockCPU",
                GpuName = "MockGPU",
                SystemMake = "MockMake",
                Memory = "16GB"
            };
        }

        public ReadingDataStream? GetReadingData()
        {
            return new ReadingDataStream
            {
                CpuReadings = new CpuReadings { PackageTemp = new ReadingData { Current = 50 } }
            };
        }

        public int? GetSensorInterval() => 2000;

        public void Dispose() { }
    }
}
