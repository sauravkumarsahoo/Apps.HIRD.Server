using Newtonsoft.Json;
using HIRD.Proto;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using static HIRD.Proto.ComputerInfo.Types;
using Microsoft.Extensions.Logging;
using HIRD.HWiNFOAccess.Elements;

namespace HIRD.HWiNFOAccess
{
    public class HWiNFOSharedMemoryAccessor : IDisposable
    {
        private readonly ILogger<HWiNFOSharedMemoryAccessor>? _logger;

        public HWiNFOSharedMemoryAccessor()
        {
        }

        public HWiNFOSharedMemoryAccessor(ILogger<HWiNFOSharedMemoryAccessor> logger)
        {
            _logger = logger;
        }

        private List<string>? masterSensorNames;
        private static readonly ReadingSources readingIds = new();

        private static readonly Regex storageNameRegex = new(@"^S.M.A.R.T.: (.*) \(.*\)$");

        private static readonly Regex coreTempRegex = new(@"^Core \d\d?$");
        private static readonly Regex coreClockRegex = new(@"^Core \d\d? Clock$");
        private static readonly Regex coreUsageRegex = new(@"^Core \d\d? T\d Usage$");

        private static ComputerInfo? _computerInfo = null;
        private static ushort? _gpuSensorId = null;
        private static readonly List<ushort> _gpuReadingIds = new();

        public static bool IsRunning()
        {
            try
            {
                using var _ = MemoryMappedFile.OpenExisting(Constants.HWiNFO_SENSORS_MAP_FILE_NAME2, MemoryMappedFileRights.Read);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public ComputerInfo GetComputerInfo(bool force = false)
        {
            if (!force && _computerInfo != null)
                return _computerInfo;

            using var mmf = MemoryMappedFile.OpenExisting(Constants.HWiNFO_SENSORS_MAP_FILE_NAME2, MemoryMappedFileRights.Read);
            using var accessor = mmf!.CreateViewAccessor(0, Marshal.SizeOf(typeof(HWiNFO_SENSORS_SHARED_MEM2)), MemoryMappedFileAccess.Read);
            accessor.Read(0, out HWiNFO_SENSORS_SHARED_MEM2 HWiNFOMemory);
            var numSensors = HWiNFOMemory.dwNumSensorElements;
            var numReadingElements = HWiNFOMemory.dwNumReadingElements;
            var offsetSensorSection = HWiNFOMemory.dwOffsetOfSensorSection;
            var sizeSensorElement = HWiNFOMemory.dwSizeOfSensorElement;
            var offsetReadingSection = HWiNFOMemory.dwOffsetOfReadingSection;
            var sizeReadingSection = HWiNFOMemory.dwSizeOfReadingElement;

            masterSensorNames = new();
            List<string> gpuNames = new();
            List<string> storageNames = new();
            List<StorageType> storageTypes = new();

            double memSize = 0;
            var computerInfo = new ComputerInfo() { ComputerName = Environment.MachineName };

            for (ushort dwSensor = 0; dwSensor < numSensors; dwSensor++)
            {
                using var sensor_element_accessor = mmf.CreateViewStream(offsetSensorSection + (dwSensor * sizeSensorElement), sizeSensorElement, MemoryMappedFileAccess.Read);

                byte[] byteBuffer = new byte[sizeSensorElement];
                sensor_element_accessor.Read(byteBuffer, 0, (int)sizeSensorElement);
                var handle = GCHandle.Alloc(byteBuffer, GCHandleType.Pinned);
                var SensorElement = SharedMemory.GetSensorElement(ref handle);
                handle.Free();

                string sensorName = SensorElement.szSensorNameUser;

                masterSensorNames.Add(sensorName);

                if (computerInfo.CpuName == string.Empty && sensorName.StartsWith("CPU [#"))
                {
                    var cpuFirstColon = sensorName.IndexOf(':') + 2;
                    var cpuLastColon = sensorName.IndexOf(':', cpuFirstColon);

                    var length = cpuLastColon == -1
                        ? sensorName.Length - cpuFirstColon
                        : (sensorName.Length - cpuFirstColon) - (sensorName.Length - cpuLastColon);

                    computerInfo.CpuName = sensorName.Substring(cpuFirstColon, length);

                }
                else if (sensorName.StartsWith("GPU [#"))
                {
                    _gpuSensorId = dwSensor;

                    var gpuFirstColon = sensorName.IndexOf(':') + 2;
                    var gpuLastColon = sensorName.IndexOf(':', gpuFirstColon);

                    var length = gpuLastColon == -1
                        ? sensorName.Length - gpuFirstColon
                        : (sensorName.Length - gpuFirstColon) - (sensorName.Length - gpuLastColon);

                    gpuNames.Add(sensorName.Substring(gpuFirstColon, length));
                }
                else if (sensorName.StartsWith("System: "))
                {
                    computerInfo.SystemMake = sensorName["System: ".Length..];
                }
                else if (sensorName.StartsWith("S.M.A.R.T.: "))
                {
                    storageNames.Add(storageNameRegex.Match(sensorName).Groups[1].Value);
                    storageTypes.Add(StorageType.Sata);
                }
                else if (sensorName.StartsWith("Battery: "))
                {
                    computerInfo.SystemType = ComputerInfo.Types.SystemType.Laptop;
                }
            }

            computerInfo.GpuName = gpuNames.FirstOrDefault();

            if (computerInfo.GpuName != null)
                readingIds.GPU = new();

            computerInfo.StorageNames.AddRange(storageNames);

            for (ushort id = 0; id < numReadingElements; id++)
            {
                using var sensor_element_accessor = mmf.CreateViewStream(offsetReadingSection + (id * sizeReadingSection), sizeReadingSection, MemoryMappedFileAccess.Read);
                byte[] byteBuffer = new byte[sizeReadingSection];
                sensor_element_accessor.Read(byteBuffer, 0, (int)sizeReadingSection);
                var handle = GCHandle.Alloc(byteBuffer, GCHandleType.Pinned);
                var readingElement = SharedMemory.GetReadingElement(ref handle);
                handle.Free();

                var sensorName = masterSensorNames[(int)readingElement.dwSensorIndex];
                string label = readingElement.szLabelOrig;

                if (sensorName.StartsWith("System: "))
                {
                    if (label == "Physical Memory Used")
                        memSize += readingElement.Value;
                    else if (label == "Physical Memory Available")
                        memSize += readingElement.Value;
                    else if (label == "Physical Memory Load")
                        readingIds.System.RamLoad = new(id, label);
                }
                else if (sensorName.EndsWith(computerInfo.CpuName))
                {
                    if (label == "Average Effective Clock")
                        readingIds.CPU.CoreClockEffective = new(id, label);
                    else if (coreUsageRegex.IsMatch(label))
                        readingIds.CPU.CoreUsages.Add(new(id, label));
                }
                else if (sensorName.Contains(computerInfo.CpuName) && sensorName.EndsWith("DTS"))
                {
                    if (label == "CPU Package")
                        readingIds.CPU.PackageTemp = new(id, label);
                    else if (coreTempRegex.IsMatch(label))
                        readingIds.CPU.CoreTemps.Add(new(id, label));
                    else if (label.EndsWith("Thermal Throttling"))
                        readingIds.CPU.ThermalThrottling.Add(new(id, label));
                }
                else if (sensorName.Contains(computerInfo.CpuName) && sensorName.EndsWith("Enhanced"))
                {
                    if (label == "CPU Package Power")
                        readingIds.CPU.Power = new(id, label);
                }
                else if (computerInfo.GpuName != string.Empty && sensorName.Contains(computerInfo.GpuName!))
                {
                    _gpuReadingIds.Add(id);
                    GpuReadingSources? gpuReadingIds = readingIds.GPU;

                    if (gpuReadingIds == null)
                        throw new NullReferenceException("Error in script: GPU Reading Id is null but reading present.");

                    if (label == "GPU Temperature")
                        gpuReadingIds.Temp = new(id, label);
                    else if (label == "GPU Hot Spot Temperature")
                        gpuReadingIds.HotSpotTemp = new(id, label);
                    else if (label == "GPU Clock")
                        gpuReadingIds.Clock = new(id, label);
                    else if (label == "GPU Memory Clock")
                        gpuReadingIds.MemoryClock = new(id, label);
                    else if (label.StartsWith("GPU Fan"))
                        gpuReadingIds.FanSpeeds.Add(new(id, label));
                    else if (label == "GPU Power")
                        gpuReadingIds.Power = new(id, label);
                    else if (label == "GPU Core Load")
                        gpuReadingIds.Usage = new(id, label);
                    else if (label == "GPU Memory Usage")
                        gpuReadingIds.MemUsage = new(id, label);
                }
                else if (sensorName == "Memory Timings")
                {
                    if (label == "Memory Clock")
                        readingIds.System.MemoryClock = new(id, label);
                }
                else if (sensorName.StartsWith("Battery: "))
                {
                    if (label == "Charge Level")
                        readingIds.System.ChargeLevel = new(id, label);
                    else if (label.Contains("Load"))
                        readingIds.System.Power = new(id, label);
                }
                else if (sensorName == "UPS")
                {
                    if (label == "UPS Load" && readingElement.tReading == SENSOR_READING_TYPE.SENSOR_TYPE_POWER)
                        readingIds.System.Power = new(id, label);
                    else if (label == "Charge Level")
                        readingIds.System.ChargeLevel = new(id, label);
                }
                else if (sensorName.StartsWith("ACPI"))
                {
                    if (label == "CPU")
                        readingIds.CPU.FanSpeed = new(id, label);
                }
                else if (sensorName.StartsWith("S.M.A.R.T.: "))
                {
                    var storageName = storageNameRegex.Match(sensorName).Groups[1].Value;
                    int storageIndex = storageNames.FindIndex(x => x == storageName);

                    if (label == "Drive Airflow Temperature")
                        readingIds.System.StorageTemps.Add(new(id, label));
                    else if (label == "Drive Temperature")
                        readingIds.System.StorageTemps.Add(new(id, label));
                    else if (label == "Drive Remaining Life")
                        storageTypes[storageIndex] = StorageType.Ssd;
                }
            }

            computerInfo.Memory = $"{Math.Round(memSize / 1024, MidpointRounding.AwayFromZero)} GB";
            computerInfo.StorageTypes.AddRange(storageTypes);

            _computerInfo = computerInfo;
            _logger?.LogInformation("Computer Info\n      -------------\n      {info}", JsonConvert.SerializeObject(_computerInfo, Formatting.Indented));
            _logger?.LogInformation("Computer Sensor Reading Ids\n      ---------------------------\n      {rIds}", JsonConvert.SerializeObject(readingIds, Formatting.Indented));

            return computerInfo;
        }

        private void LoadGPUIds()
        {
            using var mmf = MemoryMappedFile.OpenExisting(Constants.HWiNFO_SENSORS_MAP_FILE_NAME2, MemoryMappedFileRights.Read);
            using var accessor = mmf!.CreateViewAccessor(0, Marshal.SizeOf(typeof(HWiNFO_SENSORS_SHARED_MEM2)), MemoryMappedFileAccess.Read);
            accessor.Read(0, out HWiNFO_SENSORS_SHARED_MEM2 HWiNFOMemory);
            var numReadingElements = HWiNFOMemory.dwNumReadingElements;
            var offsetReadingSection = HWiNFOMemory.dwOffsetOfReadingSection;
            var sizeReadingSection = HWiNFOMemory.dwSizeOfReadingElement;

            foreach (var dwReading in _gpuReadingIds)
            {
                using var sensor_element_accessor = mmf.CreateViewStream(offsetReadingSection + (dwReading * sizeReadingSection), sizeReadingSection, MemoryMappedFileAccess.Read);
                byte[] byteBuffer = new byte[sizeReadingSection];
                sensor_element_accessor.Read(byteBuffer, 0, (int)sizeReadingSection);
                var handle = GCHandle.Alloc(byteBuffer, GCHandleType.Pinned);
                var ReadingElement = SharedMemory.GetReadingElement(ref handle);
                handle.Free();

                var sensorName = masterSensorNames![(int)_gpuSensorId!];

                if (sensorName.Contains(_computerInfo!.GpuName!))
                {
                    GpuReadingSources? gpuReadingIds = readingIds.GPU;

                    if (gpuReadingIds == null)
                        throw new NullReferenceException("Error in script: GPU Reading Id is null but reading present.");

                    string label = ReadingElement.szLabelOrig;

                    if (label == "GPU Temperature")
                        gpuReadingIds.Temp = new(dwReading, label);
                    else if (label == "GPU Hot Spot Temperature")
                        gpuReadingIds.HotSpotTemp = new(dwReading, label);
                    else if (label == "GPU Clock")
                        gpuReadingIds.Clock = new(dwReading, label);
                    else if (label == "GPU Memory Clock")
                        gpuReadingIds.MemoryClock = new(dwReading, label);
                    else if (label.StartsWith("GPU Fan"))
                        gpuReadingIds.FanSpeeds.Add(new(dwReading, label));
                    else if (label == "GPU Power")
                        gpuReadingIds.Power = new(dwReading, label);
                    else if (label == "GPU Core Load")
                        gpuReadingIds.Usage = new(dwReading, label);
                    else if (label == "GPU Memory Usage")
                        gpuReadingIds.MemUsage = new(dwReading, label);
                }
            }
        }

        public ReadingDataStream? GetReadingData()
        {
            if (mmf == null)
                LoadSizeAndOffset();

            try
            {
                var streamData = new ReadingDataStream()
                {
                    CpuReadings = new CpuReadings(),
                    GpuReadings = new GpuReadings(),
                    SystemReadings = new SystemReadings()
                };

                var readCpuTask = Task.Run(() => LoadCPUReadings(streamData));
                var readGpuTask = Task.Run(() => LoadGPUReadings(streamData));
                var readSysTask = Task.Run(() => LoadSystemReadings(streamData));

                Task.WaitAll(readCpuTask, readGpuTask, readSysTask);

                return streamData;
            }
            catch (ObjectDisposedException) { return null; }
        }

        private MemoryMappedFile? mmf;
        private int? offsetReadingSection;
        private int? sizeReadingSection;

        private void LoadSizeAndOffset()
        {
            mmf = MemoryMappedFile.OpenExisting(Constants.HWiNFO_SENSORS_MAP_FILE_NAME2, MemoryMappedFileRights.Read);
            using MemoryMappedViewAccessor accessor = mmf.CreateViewAccessor(0, Marshal.SizeOf(typeof(HWiNFO_SENSORS_SHARED_MEM2)), MemoryMappedFileAccess.Read);
            accessor.Read(0, out HWiNFO_SENSORS_SHARED_MEM2 HWiNFOMemory);
            offsetReadingSection = (int)HWiNFOMemory.dwOffsetOfReadingSection;
            sizeReadingSection = (int)HWiNFOMemory.dwSizeOfReadingElement;
        }

        private void LoadCPUReadings(ReadingDataStream streamData)
        {
            CpuReadings cpuReadings = streamData.CpuReadings;
            CpuReadingSources cpuReadingIds = readingIds.CPU;

            cpuReadings.PackageTemp = GetReading(cpuReadingIds.PackageTemp);

            foreach (var item in cpuReadingIds.CoreTemps)
                cpuReadings.CoreTemps.Add(GetReading(item));

            bool isThrottling = cpuReadingIds.ThermalThrottling.Select(x => GetReading(x)).Any(x => x!.Current == 1);

            foreach (var item in cpuReadingIds.CoreUsages)
                cpuReadings.CoreUsages.Add(GetReading(item));

            cpuReadings.CoreClockEffective = GetReading(cpuReadingIds.CoreClockEffective);
            cpuReadings.ThermalThrottling = new() { Current = isThrottling ? 1 : 0, Max = 0, Min = 0, Avg = 0 };
            cpuReadings.Power = GetReading(cpuReadingIds.Power);

            if (cpuReadingIds.FanSpeed.HasValue)
                cpuReadings.FanSpeed = GetReading(cpuReadingIds.FanSpeed.Value);
        }

        private static Task? readGpuIdTask;
        private static readonly object gpuLoadLock = new();

        private void LoadGPUReadings(ReadingDataStream streamData)
        {
            if (_gpuSensorId == null)
                return;

            if (!readingIds.GPU!.Temp.HasValue)
            {
                lock (gpuLoadLock)
                {
                    if (readGpuIdTask == null || readGpuIdTask.IsCompleted)
                        readGpuIdTask = Task.Run(() => LoadGPUIds());
                }
                return;
            }

            GpuReadings gpuReadings = streamData.GpuReadings;
            GpuReadingSources gpuReadingIds = readingIds.GPU;

            gpuReadings.Temp = GetReading(gpuReadingIds.Temp);
            gpuReadings.HotSpotTemp = GetReading(gpuReadingIds.HotSpotTemp);
            gpuReadings.Clock = GetReading(gpuReadingIds.Clock);
            gpuReadings.MemoryClock = GetReading(gpuReadingIds.MemoryClock);
            gpuReadings.Power = GetReading(gpuReadingIds.Power);
            gpuReadings.Usage = GetReading(gpuReadingIds.Usage);
            gpuReadings.MemoryUsage = GetReading(gpuReadingIds.MemUsage);

            foreach (var item in gpuReadingIds.FanSpeeds)
                gpuReadings.FanSpeeds.Add(GetReading(item));
        }

        private void LoadSystemReadings(ReadingDataStream streamData)
        {
            SystemReadings sysReadings = streamData.SystemReadings;
            SystemReadingSources sysReadingIds = readingIds.System;

            sysReadings.MemoryClock = GetReading(sysReadingIds.MemoryClock);
            sysReadings.RamLoad = GetReading(sysReadingIds.RamLoad);
            sysReadings.ChargeLevel = GetReading(sysReadingIds.ChargeLevel);

            foreach (var item in sysReadingIds.StorageTemps)
                sysReadings.StorageTemps.Add(GetReading(item));

            if (sysReadingIds.Power.HasValue)
                sysReadings.Power = GetReading(sysReadingIds.Power.Value);
        }

        private static Task? reloadIDsTask;
        private static readonly object reloadLock = new();
        private ReadingData? GetReading(ReadingSource? source)
        {
            if (source == null)
                return null;

            var readingElement = GetReadingElement(source.Value.Id);

            if (readingElement.szLabelOrig != source.Value.Name)
            {
                lock (reloadLock)
                {
                    if (reloadIDsTask == null || reloadIDsTask.IsCompleted)
                        reloadIDsTask = Task.Run(() => GetComputerInfo(force: true));
                }
            }

            return new()
            {
                Current = readingElement.Value,
                Min = readingElement.ValueMin,
                Max = readingElement.ValueMax,
                Avg = readingElement.ValueAvg
            };
        }

        private HWiNFO_SENSORS_READING_ELEMENT GetReadingElement(ushort id)
        {
            if (mmf == null || sizeReadingSection == null || offsetReadingSection == null)
                throw new InvalidOperationException("Attempted to Read Element without initialization");

            var byteBuffer = new byte[sizeReadingSection.Value];
            using var sensor_element_accessor = mmf.CreateViewStream(offsetReadingSection.Value + (id * sizeReadingSection.Value), sizeReadingSection.Value, MemoryMappedFileAccess.Read);
            sensor_element_accessor.Read(byteBuffer, 0, (int)sizeReadingSection);
            GCHandle handle = GCHandle.Alloc(byteBuffer, GCHandleType.Pinned);
            var readingElement = SharedMemory.GetReadingElement(ref handle);
            handle.Free();
            return readingElement;
        }

        public void Dispose()
        {
            mmf?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}

// ***************************************************************************************************************
//                                          HWiNFO Shared Memory Footprint
// ***************************************************************************************************************
//
//         |-----------------------------|-----------------------------------|-----------------------------------|
// Content |  HWiNFO_SENSORS_SHARED_MEM2 |  HWiNFO_SENSORS_SENSOR_ELEMENT[]  | HWiNFO_SENSORS_READING_ELEMENT[]  |
//         |-----------------------------|-----------------------------------|-----------------------------------|
// Pointer |<--0                         |<--dwOffsetOfSensorSection         |<--dwOffsetOfReadingSection        |
//         |-----------------------------|-----------------------------------|-----------------------------------|
// Size    |  dwOffsetOfSensorSection    |   dwSizeOfSensorElement           |    dwSizeOfReadingElement         |
//         |                             |      * dwNumSensorElement         |       * dwNumReadingElement       |
//         |-----------------------------|-----------------------------------|-----------------------------------|
//
