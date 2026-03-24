using Grpc.Core;
using HIRD.HWiNFOAccess;
using HIRD.Proto;
using System.Net;
using System.Threading;

namespace HIRD.Service
{
    public class SensorDataService : SensorService.SensorServiceBase
    {
        private readonly ILogger<SensorDataService> _logger;
        private readonly HWiNFOSharedMemoryAccessor _hWiNFO;

        private const string SensorIntervalLine = "SensorInterval=";
        private static int SensorInterval = 2000;

        public delegate void AddPeer(string peer);
        public delegate void RemovePeer(string peer);

        public static List<AddPeer> AddPeerEventDelegates { get; } = new();
        public static List<RemovePeer> RemovePeerEventDelegates { get; } = new();

        public SensorDataService(ILogger<SensorDataService> logger, HWiNFOSharedMemoryAccessor hWiNFO)
        {
            _logger = logger;
            _hWiNFO = hWiNFO;
        }

        public override Task<ComputerInfo> GetComputerInfo(ComputerInfoRequest request, ServerCallContext context)
        {
            _logger.LogInformation("Request received to 'GetComputerInfo()' from {peer}.", context.GetHttpContext().Request.Host.Host);

            string settingsPath = HWiNFOProcessDetails.GetProcessSettingsPath();
            if (!string.IsNullOrEmpty(settingsPath) && File.Exists(settingsPath))
            {
                string? sensorIntervalLineText = File.ReadAllLines(settingsPath).FirstOrDefault(x => x.StartsWith(SensorIntervalLine));
                if (sensorIntervalLineText != null)
                    Volatile.Write(ref SensorInterval, int.Parse(sensorIntervalLineText[SensorIntervalLine.Length..]));
            }

            return Task.FromResult(_hWiNFO.GetComputerInfo());
        }

        public override async Task Monitor(MonitorRequest request, IServerStreamWriter<ReadingDataStream> responseStream, ServerCallContext context)
        {
            var startTime = DateTime.Now;
            var peer = $"{request.DeviceName} @ {GetRemoteIP(context)}";
            _logger.LogInformation("Request received to 'Monitor()' from {peer}.", peer);

            foreach (var add in AddPeerEventDelegates)
                add.Invoke(peer);

            while (!context.CancellationToken.IsCancellationRequested)
            {
                var data = _hWiNFO.GetReadingData();
                if (data != null)
                    await responseStream.WriteAsync(data);

                try
                {
                    await Task.Delay(Volatile.Read(ref SensorInterval), context.CancellationToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            var endTime = DateTime.Now;

            foreach (var rem in RemovePeerEventDelegates)
                rem.Invoke(peer);

            _logger.LogInformation("'Monitor()' has ended for {peer} after {time} seconds.", peer, endTime.Subtract(startTime).TotalSeconds);
        }

        private static string GetRemoteIP(ServerCallContext context)
        {
            IPAddress remoteIpAddress = context.GetHttpContext().Connection.RemoteIpAddress!;
            var ip = remoteIpAddress.IsIPv4MappedToIPv6 ? remoteIpAddress.MapToIPv4().ToString() : remoteIpAddress.ToString();
            return ip;
        }
    }
}