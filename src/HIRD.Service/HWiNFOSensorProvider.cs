using HIRD.HWiNFOAccess;
using HIRD.Proto;

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
            catch (System.IO.IOException ex)
            {
                System.Diagnostics.Trace.TraceWarning("Failed to read HWiNFO settings file for SensorInterval due to IO error: {0}", ex);
            }
            catch (System.UnauthorizedAccessException ex)
            {
                System.Diagnostics.Trace.TraceWarning("Access to HWiNFO settings file for SensorInterval was denied: {0}", ex);
            }
            catch (System.FormatException ex)
            {
                System.Diagnostics.Trace.TraceWarning("Failed to parse SensorInterval from HWiNFO settings file due to format error: {0}", ex);
            }
            return null;
        }

        public void Dispose()
        {
            // Intentionally left blank.
            // HWiNFOSharedMemoryAccessor is managed by the DI container and will be disposed by it.
        }
    }
}
