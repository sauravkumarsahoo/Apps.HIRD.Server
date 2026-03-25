using HIRD.HWiNFOAccess;
using HIRD.Proto;
using HIRD.Core;

namespace HIRD.Service
{
    public class HWiNFOSensorProvider : ISensorProvider
    {
        private readonly HWiNFOSharedMemoryAccessor _accessor;
        private const string SensorIntervalLine = "SensorInterval=";

        public HWiNFOSensorProvider(HWiNFOSharedMemoryAccessor accessor)
        {
            _accessor = accessor;
        }

        public ComputerInfo GetComputerInfo() => _accessor.GetComputerInfo();

        public ReadingDataStream? GetReadingData() => _accessor.GetReadingData();

        public int? GetSensorInterval()
        {
            try
            {
                string settingsPath = HWiNFOProcessDetails.GetProcessSettingsPath();
                if (!string.IsNullOrEmpty(settingsPath) && File.Exists(settingsPath))
                {
                    string? sensorIntervalLineText = File.ReadAllLines(settingsPath).FirstOrDefault(x => x.StartsWith(SensorIntervalLine));
                    if (sensorIntervalLineText != null)
                        return int.Parse(sensorIntervalLineText[SensorIntervalLine.Length..]);
                }
            }
            catch { }
            return null;
        }

        public void Dispose()
        {
            // Intentionally left blank.
            // HWiNFOSharedMemoryAccessor is managed by the DI container and will be disposed by it.
        }
    }
}
