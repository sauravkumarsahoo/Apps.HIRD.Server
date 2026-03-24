using Clicksrv.Packages.StartWithOSSettings;
using Newtonsoft.Json;

namespace HIRD
{
    internal class AppSettings
    {
        private const string Name = "HIRD";
        private const string SettingsFile = "settings.json";
        private static string? _settingsPath;
        private static string SettingsPath => _settingsPath ??= (OSStartupSettings.GetSavedAddress() ?? Application.ExecutablePath).Replace("exe", SettingsFile);
        private static readonly IStartupOptions OSStartupSettings = new StartupOptions(Name, Application.ExecutablePath, arguments: new string[] { "silent", "delayed" });

        public static AppSettings Instance { get; } = CreateInstance();

        public void Save()
        {
            string fileText = JsonConvert.SerializeObject(this);
            File.WriteAllText(SettingsPath, fileText);
        }

        private static AppSettings CreateInstance()
            => File.Exists(SettingsPath) ? InstanceFromSettingsFile() : CreateDefaultInstance();

        private static AppSettings InstanceFromSettingsFile()
        {
            string fileText = File.ReadAllText(SettingsPath);
            return JsonConvert.DeserializeObject<AppSettings>(fileText)!;
        }

        private static AppSettings CreateDefaultInstance()
        {
            AppSettings defaults = new();
            string defaultFileText = JsonConvert.SerializeObject(defaults);
            File.WriteAllText(SettingsPath, defaultFileText);
            OSStartupSettings.CreateStartupEntry();
            return defaults;
        }

        public AppSettings(bool minimizeToTray = false,
                            bool startMinimized = false,
                            bool autoStartServer = false)
        {
            MinimizeToTray = minimizeToTray;
            StartMinimized = startMinimized;
            AutoStartServer = autoStartServer;
        }

        [JsonIgnore]
        public static bool StartWithWindows
        {
            get => OSStartupSettings.Enabled;
            set
            {
                if (value) OSStartupSettings.Enable();
                else OSStartupSettings.Disable();
            }
        }

        public bool MinimizeToTray { get; set; }
        public bool StartMinimized { get; set; }
        public bool AutoStartServer { get; set; }
    }
}
