using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using HIRD.Service;
using HIRD.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System;

namespace HIRD.Desktop;

public partial class MainWindow : Window
{
    private GrpcServer? _grpcServer;
    private bool _isRunning = false;
    private readonly ObservableCollection<string> _connectedPeers = new();
    private ISensorProvider? _sensorProvider;

    public MainWindow()
    {
        InitializeComponent();
        StatusLabel.Text = "Status: Ready";
        ConnectedClientsList.ItemsSource = _connectedPeers;

        SensorDataService.AddPeerEventDelegates.Add(OnPeerAdded);
        SensorDataService.RemovePeerEventDelegates.Add(OnPeerRemoved);

        PlatformLabel.Text = RuntimeInformation.OSDescription;
    }

    private void OnPeerAdded(string peer)
    {
        Dispatcher.UIThread.InvokeAsync(() => _connectedPeers.Add(peer));
    }

    private void OnPeerRemoved(string peer)
    {
        Dispatcher.UIThread.InvokeAsync(() => _connectedPeers.Remove(peer));
    }

    private async void OnStartStopClick(object? sender, RoutedEventArgs e)
    {
        if (!_isRunning)
        {
            await StartServer();
        }
        else
        {
            await StopServer();
        }
    }

    private async Task StartServer()
    {
        try
        {
            StatusLabel.Text = "Status: Starting...";
            StartStopButton.IsEnabled = false;

            var services = new ServiceCollection();
            GrpcServerStartup.ConfigureServices(services);
            var serviceProvider = services.BuildServiceProvider();
            var logger = serviceProvider.GetRequiredService<ILogger<GrpcServer>>();
            _sensorProvider = serviceProvider.GetRequiredService<ISensorProvider>();

            ActiveProviderLabel.Text = _sensorProvider.GetType().Name;
            CapabilitiesLabel.Text = "CPU, GPU, RAM, Storage (HWiNFO Mapping)";

            _grpcServer = new GrpcServer(logger);
            await _grpcServer.StartAsync(new CancellationToken());

            _isRunning = true;
            StatusLabel.Text = "Status: Running";
            StartStopButton.Content = "Stop Server";
            InfoLabel.Text = "Listening on port 25151";

            var info = _sensorProvider.GetComputerInfo();
            SystemInfoLabel.Text = $"Name: {info.ComputerName}\nCPU: {info.CpuName}\nGPU: {info.GpuName}\nRAM: {info.Memory}";
        }
        catch (Exception ex)
        {
            StatusLabel.Text = "Status: Error";
            InfoLabel.Text = ex.Message;
        }
        finally
        {
            StartStopButton.IsEnabled = true;
        }
    }

    private async Task StopServer()
    {
        try
        {
            StatusLabel.Text = "Status: Stopping...";
            StartStopButton.IsEnabled = false;

            if (_grpcServer != null)
            {
                await _grpcServer.StopAsync(new CancellationToken());
                _grpcServer.Dispose();
                _grpcServer = null;
            }

            _isRunning = false;
            StatusLabel.Text = "Status: Stopped";
            StartStopButton.Content = "Start Server";
            InfoLabel.Text = "";
            _connectedPeers.Clear();
            SystemInfoLabel.Text = "Waiting for server start...";
        }
        catch (Exception ex)
        {
            StatusLabel.Text = "Status: Error during stop";
            InfoLabel.Text = ex.Message;
        }
        finally
        {
            StartStopButton.IsEnabled = true;
        }
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        SensorDataService.AddPeerEventDelegates.Remove(OnPeerAdded);
        SensorDataService.RemovePeerEventDelegates.Remove(OnPeerRemoved);
        base.OnClosing(e);
    }
}
