using HIRD.Proto;

namespace HIRD.Service
{
    public interface ISensorProvider : IDisposable
    {
        ComputerInfo GetComputerInfo();
        ReadingDataStream? GetReadingData();
        int? GetSensorInterval();
    }
}
