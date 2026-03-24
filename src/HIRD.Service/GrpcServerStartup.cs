using HIRD.HWiNFOAccess;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace HIRD.Service
{
    public class GrpcServerStartup
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<HWiNFOSharedMemoryAccessor>()
                    .AddSingleton<ISensorProvider, HWiNFOSensorProvider>()
                    .AddLogging()
                    .AddGrpc();
        }

        public static void Configure(IApplicationBuilder app, IWebHostEnvironment _)
        {
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<SensorDataService>();
            });
        }
    }
}
