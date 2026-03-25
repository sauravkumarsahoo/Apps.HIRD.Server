using Grpc.Core;
using HIRD.HWiNFOAccess;
using HIRD.Proto;
using HIRD.Core;
using System.Net;
using System.Threading;

namespace HIRD.Service
{
    public class SensorDataService : SensorService.SensorServiceBase
    {
        private readonly ILogger<SensorDataService> _logger;
        private readonly ISensorProvider _sensorProvider;

        private static int SensorInterval = 2000;

        public delegate void AddPeer(string peer);
        public delegate void RemovePeer(string peer);

        public static List<AddPeer> AddPeerEventDelegates { get; } = new();
        public static List<RemovePeer> RemovePeerEventDelegates { get; } = new();

        public SensorDataService(ILogger<SensorDataService> logger, ISensorProvider sensorProvider)
        {
            _logger = logger;
            _sensorProvider = sensorProvider;
        }

        public override Task<ComputerInfo> GetComputerInfo(ComputerInfoRequest request, ServerCallContext context)
        {
            var peer = context?.Peer ?? "unknown";
            _logger.LogInformation("Request received to 'GetComputerInfo()' from {peer}.", peer);

            int? interval = _sensorProvider.GetSensorInterval();
            if (interval.HasValue)
                Volatile.Write(ref SensorInterval, interval.Value);

            return Task.FromResult(_sensorProvider.GetComputerInfo());
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
                var data = _sensorProvider.GetReadingData();
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
            try
            {
                if (context == null) return "unknown";
                IPAddress? remoteIpAddress = context.GetHttpContext().Connection.RemoteIpAddress;
                if (remoteIpAddress == null) return context.Peer;
                var ip = remoteIpAddress.IsIPv4MappedToIPv6 ? remoteIpAddress.MapToIPv4().ToString() : remoteIpAddress.ToString();
                return ip;
            }
            catch
            {
                return context?.Peer ?? "unknown";
            }
        }
    }
}
