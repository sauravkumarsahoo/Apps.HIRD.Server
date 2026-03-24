using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace HIRD.Service
{
    public class GrpcServer : BackgroundService
    {
        private readonly ILogger<GrpcServer>? _logger;
        private readonly IHost _grpcHost;

        public GrpcServer(ILogger<GrpcServer>? logger = null)
        {
            _logger = logger;
            _grpcHost = Host.CreateDefaultBuilder()
                            .ConfigureWebHostDefaults(builder =>
                            {
                                builder
                                    .ConfigureKestrel(options =>
                                    {
                                        options.ListenAnyIP(25151, listenOptions =>
                                        {
                                            listenOptions.Protocols = HttpProtocols.Http2;
                                        });
                                    })
                                    .UseKestrel()
                                    .UseStartup<GrpcServerStartup>();
                            })
                            .Build();
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger?.LogInformation("Stopping service");
            await _grpcHost.StopAsync(cancellationToken);
        }

        public override void Dispose()
        {
            _grpcHost.Dispose();
            base.Dispose();
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger?.LogInformation("Starting service");
            await _grpcHost.StartAsync(cancellationToken);
        }
    }
}