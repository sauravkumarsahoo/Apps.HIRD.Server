using HIRD.HWiNFOAccess;
using HIRD.Service;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;

namespace HIRD.ServerUI
{
    public partial class MainForm : Form
    {
        private const string HI_NOT_RUNNING = "HWiNFO64 is not running!";
        private const string HR_SM_DISABLED = "HWiNFO64 Shared Memory Access is not enabled! See Help to learn how to enable.";

        private const string SERVER_STATE_NOT_READY = "Not Ready";
        private const string SERVER_STATE_READY = "Ready";
        private const string SERVER_STATE_RUNNING = "Running";
        private const string SERVER_STATE_STARTING = "Starting Server...";
        private const string SERVER_STATE_STOPPING = "Stopping Server...";

        private const string STOP_SERVER = "Stop Server";
        private const string START_SERVER = "Start Server";
        private const string PLEASE_WAIT = "Please wait";

        private const string HELP_TEXT = "Ensure that HWiNFO64 is installed and running.\n\nFrom the System Tray, right click HWiNFO and open Settings.\n\nEnsure Shared Memory Support is checked.\n\nEnsure that this machine and your phone are connected to the same network.\n\nClick on Start Server.\n\nOpen an HIRD client app and select this server.";
        private const string HELP_CAPTION = "How to run HIRD";

        private GrpcServer? _grpcServer;
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger<MainForm> _logger;
        private volatile byte _hwInfoStatus = 3;

        public MainForm(ILoggerFactory loggerFactory)
        {
            InitializeComponent();

            Text += $" v{GetType().Assembly.GetName().Version}";

            compNameText.Text = Environment.MachineName;
            var ips = GetLocalIPAddress();
            if (ips.Count > 1)
                ipLabel.Text = "Local IP Addresses";
            ipText.Text = string.Join("\n", ips);

            _loggerFactory = loggerFactory;
            _logger = _loggerFactory.CreateLogger<MainForm>();

            SensorDataService.AddPeerEventDelegates.Add(AddPeer);
            SensorDataService.RemovePeerEventDelegates.Add(RemovePeer);
            CheckStatus();
            SetStatus();

            Task.Run(() => MonitorHWiNFO());

            LoadAsPerSettings();
            UpdateFormAsPerServerStatus();
        }

        private void LoadAsPerSettings()
        {
            if (AppSettings.Instance.StartMinimized)
            {
                WindowState = FormWindowState.Minimized;
                MinimizeToTray();
            }
        }

        public void AddPeer(string peer) => connectedClientsList.Invoke(() => connectedClientsList.Items.Add(peer));

        public void RemovePeer(string peer) => connectedClientsList.Invoke(() => connectedClientsList.Items.Remove(peer));

        private void MonitorHWiNFO()
        {
            byte lastStatus = _hwInfoStatus;

            while (true)
            {
                CheckStatus();

                if (_hwInfoStatus != lastStatus)
                {
                    Invoke(() => SetStatus());
                    lastStatus = _hwInfoStatus;
                }

                Task.Delay(2000).Wait();
            }
        }

        private void CheckStatus()
        {
            var isRunning = HWiNFOProcessDetails.IsRunning();
            var isSMRunning = isRunning && HWiNFOSharedMemoryAccessor.IsRunning();

            if (isRunning & isSMRunning)
                _hwInfoStatus = 2;
            else if (isRunning)
                _hwInfoStatus = 1;
            else
                _hwInfoStatus = 0;
        }

        private void SetStatus()
        {
            if (_hwInfoStatus == 0)
            {
                if (startStopServerButton.Checked)
                    startStopServerButton.Checked = false;

                statusLabel.Text = SERVER_STATE_NOT_READY;
                errorLabel.Visible = true;
                errorLabel.Text = HI_NOT_RUNNING;
                startStopServerButton.Enabled = false;
                statusIndicator.BackgroundImage = Properties.Resources.bullet_red;
                menuItem_Error.Visible = true;
                menuItem_Error.Text = HI_NOT_RUNNING;
                menuItem_startServer.Enabled = false;
                menuItem_stopServer.Enabled = false;
                SetSize(false, true);
            }
            else if (_hwInfoStatus == 1)
            {
                if (startStopServerButton.Checked)
                    startStopServerButton.Checked = false;

                statusLabel.Text = SERVER_STATE_NOT_READY;
                errorLabel.Visible = true;
                errorLabel.Text = HR_SM_DISABLED;
                startStopServerButton.Enabled = false;
                statusIndicator.BackgroundImage = Properties.Resources.bullet_red;
                menuItem_Error.Visible = true;
                menuItem_Error.Text = HR_SM_DISABLED;
                menuItem_startServer.Enabled = false;
                menuItem_stopServer.Enabled = false;
                SetSize(false, true);
            }
            else if (_grpcServer is null)
            {
                statusLabel.Text = SERVER_STATE_READY;
                errorLabel.Text = string.Empty;
                errorLabel.Visible = false;
                startStopServerButton.Enabled = true;
                statusIndicator.BackgroundImage = Properties.Resources.bullet_yellow;
                menuItem_Error.Visible = false;
                menuItem_Error.Text = string.Empty;
                menuItem_startServer.Enabled = true;
                menuItem_stopServer.Enabled = false;
                SetSize(false, false);
                if (AppSettings.Instance.AutoStartServer)
                    startStopServerButton.Checked = true;
            }
            else
            {
                statusLabel.Text = SERVER_STATE_RUNNING;
                errorLabel.Text = string.Empty;
                errorLabel.Visible = false;
                startStopServerButton.Enabled = true;
                statusIndicator.BackgroundImage = Properties.Resources.bullet_green;
                menuItem_Error.Visible = false;
                menuItem_Error.Text = string.Empty;
                menuItem_startServer.Enabled = false;
                menuItem_stopServer.Enabled = true;
                SetSize(true, false);
            }
        }

        private void SetSize(bool running, bool error)
        {
            AutoScaleDimensions = new(8F, running ? 13F : error ? 15F : 18F);
        }

        private async void StartStopServerButton_CheckedChanged(object sender, EventArgs e)
        {
            startStopServerButton.Enabled = false;
            statusIndicator.BackgroundImage = Properties.Resources.bullet_yellow;
            menuItem_startServer.Enabled = false;
            menuItem_stopServer.Enabled = false;

            bool isStarting = startStopServerButton.Checked;

            try
            {
                if (isStarting)
                {
                    statusLabel.Text = SERVER_STATE_STARTING;
                    startStopServerButton.Text = PLEASE_WAIT;
                    _grpcServer = new(_loggerFactory.CreateLogger<GrpcServer>());
                    await _grpcServer.StartAsync(new CancellationToken());
                }
                else
                {
                    statusLabel.Text = SERVER_STATE_STOPPING;
                    startStopServerButton.Text = PLEASE_WAIT;
                    await _grpcServer!.StopAsync(new CancellationToken());
                    _grpcServer.Dispose();
                    _grpcServer = null;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message,
                                $"Error when {(isStarting ? "starting" : "stopping")} the server",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Error);
#if DEBUG
                MessageBox.Show($"{ex.Source}\n\n{ex.StackTrace}", "Stacktrace");
#endif
            }

            UpdateFormAsPerServerStatus();

            startStopServerButton.Enabled = true;
        }

        private void UpdateFormAsPerServerStatus()
        {
            bool isServerRunning = _grpcServer is not null;

            startStopServerButton.Checked = isServerRunning;
            connectedClientsLabel.Visible = isServerRunning;
            connectedClientsList.Visible = isServerRunning;
            serverInfoGroup.Visible = isServerRunning;

            if (isServerRunning)
            {
                menuItem_startServer.Enabled = false;
                menuItem_stopServer.Enabled = _hwInfoStatus == 2;
                startStopServerButton.Text = STOP_SERVER;
                statusLabel.Text = SERVER_STATE_RUNNING;
                statusIndicator.BackgroundImage = Properties.Resources.bullet_green;
            }
            else
            {
                menuItem_startServer.Enabled = _hwInfoStatus == 2;
                menuItem_stopServer.Enabled = false;
                connectedClientsList.Items.Clear();
                startStopServerButton.Text = START_SERVER;

                if (_hwInfoStatus == 2)
                {
                    statusLabel.Text = SERVER_STATE_READY;
                    statusIndicator.BackgroundImage = Properties.Resources.bullet_yellow;
                }
            }

            SetSize(isServerRunning, _hwInfoStatus != 2);
        }

        private static List<string> GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());

            List<string> ips = new();
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    ips.Add(ip.ToString());
            }

            if (ips.Count == 0)
                throw new IPAddressNotFoundException();

            return ips;
        }

        private void RecheckButton_Click(object sender, EventArgs e) => CheckStatus();

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            RestoreFromTray();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (AppSettings.Instance.MinimizeToTray && WindowState == FormWindowState.Minimized)
                MinimizeToTray();
        }

        private void RestoreFromTray()
        {
            Show();
            WindowState = FormWindowState.Normal;
            BringToFront();
            SetSize(_grpcServer != null, statusLabel.Text == SERVER_STATE_NOT_READY);
            ShowInTaskbar = true;
            notifyIcon.Visible = false;
        }

        private void MinimizeToTray()
        {
            WindowState = FormWindowState.Minimized;
            notifyIcon.Visible = true;
            Hide();
            ShowInTaskbar = false;
        }

        private void settingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowSettings();
        }

        private static void ShowSettings()
        {
            SettingsForm settingsForm = new();
            settingsForm.ShowDialog();
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowHelp();
        }

        private static void ShowHelp()
        {
            MessageBox.Show(HELP_TEXT, HELP_CAPTION, MessageBoxButtons.OK, MessageBoxIcon.Question);
        }

        private void OnClose(object sender, FormClosingEventArgs e)
        {
            startStopServerButton.Checked = false;
        }

        private void menuItem_Show_Click(object sender, EventArgs e)
        {
            RestoreFromTray();
        }

        private void menuItem_Settings_Click(object sender, EventArgs e)
        {
            ShowSettings();
        }

        private void menuItem_startServer_Click(object sender, EventArgs e)
        {
            startStopServerButton.Checked = true;
        }

        private void menuItem_stopServer_Click(object sender, EventArgs e)
        {
            startStopServerButton.Checked = false;
        }

        private void menuItem_Exit_Click(object sender, EventArgs e)
        {
            startStopServerButton.Checked = false;
            Close();
        }

        private void menuItem_Error_Click(object sender, EventArgs e)
        {
            ShowHelp();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }
    }
}