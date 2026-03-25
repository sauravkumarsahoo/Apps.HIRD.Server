using HIRD.Proto;

namespace HIRD.Core
{
    public interface ISensorProvider : IDisposable
    {
        ComputerInfo GetComputerInfo();
        ReadingDataStream? GetReadingData();
        int? GetSensorInterval();
    }
}
