using HIRD.Service;

namespace HIRD.ServerUI
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
                _loggerFactory.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            startStopServerButton = new CheckBox();
            label2 = new Label();
            compNameText = new Label();
            ipLabel = new Label();
            ipText = new Label();
            connectedClientsList = new ListBox();
            connectedClientsLabel = new Label();
            statusIndicator = new PictureBox();
            statusGroup = new GroupBox();
            statusLabel = new Label();
            errorLabel = new Label();
            notifyIcon = new NotifyIcon(components);
            trayContextMenu = new ContextMenuStrip(components);
            menuItem_Show = new ToolStripMenuItem();
            menuItem_Settings = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            menuItem_startServer = new ToolStripMenuItem();
            menuItem_stopServer = new ToolStripMenuItem();
            menuItem_Error = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            menuItem_Exit = new ToolStripMenuItem();
            toolStripSeparator3 = new ToolStripSeparator();
            serverInfoGroup = new GroupBox();
            menuStrip1 = new MenuStrip();
            settingsToolStripMenuItem = new ToolStripMenuItem();
            helpToolStripMenuItem = new ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)statusIndicator).BeginInit();
            statusGroup.SuspendLayout();
            trayContextMenu.SuspendLayout();
            serverInfoGroup.SuspendLayout();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // startStopServerButton
            // 
            startStopServerButton.Appearance = Appearance.Button;
            startStopServerButton.FlatStyle = FlatStyle.Popup;
            startStopServerButton.Font = new Font("Segoe UI Semibold", 9.75F, FontStyle.Bold, GraphicsUnit.Point);
            startStopServerButton.Location = new Point(154, 0);
            startStopServerButton.Name = "startStopServerButton";
            startStopServerButton.Size = new Size(108, 31);
            startStopServerButton.TabIndex = 1;
            startStopServerButton.Text = "Start Server";
            startStopServerButton.TextAlign = ContentAlignment.MiddleCenter;
            startStopServerButton.UseVisualStyleBackColor = true;
            startStopServerButton.CheckedChanged += StartStopServerButton_CheckedChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 7F, FontStyle.Regular, GraphicsUnit.Point);
            label2.ForeColor = SystemColors.ControlDarkDark;
            label2.Location = new Point(11, 28);
            label2.Name = "label2";
            label2.Size = new Size(78, 12);
            label2.TabIndex = 4;
            label2.Text = "Computer Name";
            // 
            // compNameText
            // 
            compNameText.AutoSize = true;
            compNameText.CausesValidation = false;
            compNameText.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point);
            compNameText.Location = new Point(11, 40);
            compNameText.Name = "compNameText";
            compNameText.Size = new Size(49, 19);
            compNameText.TabIndex = 5;
            compNameText.Text = "Name";
            // 
            // ipLabel
            // 
            ipLabel.AutoSize = true;
            ipLabel.Font = new Font("Segoe UI", 7F, FontStyle.Regular, GraphicsUnit.Point);
            ipLabel.ForeColor = SystemColors.ControlDarkDark;
            ipLabel.Location = new Point(11, 68);
            ipLabel.Name = "ipLabel";
            ipLabel.Size = new Size(77, 12);
            ipLabel.TabIndex = 2;
            ipLabel.Text = "Local IP Address";
            // 
            // ipText
            // 
            ipText.AutoSize = true;
            ipText.CausesValidation = false;
            ipText.Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point);
            ipText.Location = new Point(10, 80);
            ipText.Name = "ipText";
            ipText.Size = new Size(85, 19);
            ipText.TabIndex = 3;
            ipText.Text = "192.168.0.x";
            // 
            // connectedClientsList
            // 
            connectedClientsList.BackColor = SystemColors.Window;
            connectedClientsList.Enabled = false;
            connectedClientsList.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point);
            connectedClientsList.FormattingEnabled = true;
            connectedClientsList.HorizontalScrollbar = true;
            connectedClientsList.IntegralHeight = false;
            connectedClientsList.Location = new Point(11, 62);
            connectedClientsList.Name = "connectedClientsList";
            connectedClientsList.Size = new Size(251, 93);
            connectedClientsList.TabIndex = 7;
            connectedClientsList.Visible = false;
            // 
            // connectedClientsLabel
            // 
            connectedClientsLabel.AutoSize = true;
            connectedClientsLabel.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            connectedClientsLabel.Location = new Point(11, 45);
            connectedClientsLabel.Name = "connectedClientsLabel";
            connectedClientsLabel.Size = new Size(104, 15);
            connectedClientsLabel.TabIndex = 8;
            connectedClientsLabel.Text = "Connected Clients";
            connectedClientsLabel.Visible = false;
            // 
            // statusIndicator
            // 
            statusIndicator.BackgroundImage = Properties.Resources.bullet_red;
            statusIndicator.BackgroundImageLayout = ImageLayout.Center;
            statusIndicator.InitialImage = Properties.Resources.bullet_red;
            statusIndicator.Location = new Point(134, 14);
            statusIndicator.Name = "statusIndicator";
            statusIndicator.Size = new Size(16, 16);
            statusIndicator.TabIndex = 9;
            statusIndicator.TabStop = false;
            // 
            // statusGroup
            // 
            statusGroup.Controls.Add(startStopServerButton);
            statusGroup.Controls.Add(connectedClientsLabel);
            statusGroup.Controls.Add(statusIndicator);
            statusGroup.Controls.Add(connectedClientsList);
            statusGroup.Controls.Add(statusLabel);
            statusGroup.Controls.Add(errorLabel);
            statusGroup.Font = new Font("Segoe UI", 9F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point);
            statusGroup.Location = new Point(12, 31);
            statusGroup.Name = "statusGroup";
            statusGroup.Size = new Size(274, 168);
            statusGroup.TabIndex = 10;
            statusGroup.TabStop = false;
            statusGroup.Text = "Status";
            // 
            // statusLabel
            // 
            statusLabel.Font = new Font("Segoe UI", 8.25F, FontStyle.Regular, GraphicsUnit.Point);
            statusLabel.ForeColor = Color.Black;
            statusLabel.Location = new Point(11, 21);
            statusLabel.Name = "statusLabel";
            statusLabel.Size = new Size(117, 18);
            statusLabel.TabIndex = 0;
            statusLabel.Text = "status";
            // 
            // errorLabel
            // 
            errorLabel.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point);
            errorLabel.ForeColor = Color.Firebrick;
            errorLabel.Location = new Point(11, 46);
            errorLabel.Name = "errorLabel";
            errorLabel.Size = new Size(251, 33);
            errorLabel.TabIndex = 10;
            errorLabel.Visible = false;
            // 
            // notifyIcon
            // 
            notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
            notifyIcon.BalloonTipText = "HIRD";
            notifyIcon.BalloonTipTitle = "status";
            notifyIcon.ContextMenuStrip = trayContextMenu;
            notifyIcon.Icon = (Icon)resources.GetObject("notifyIcon.Icon");
            notifyIcon.Text = "HIRD";
            notifyIcon.MouseDoubleClick += notifyIcon_MouseDoubleClick;
            // 
            // trayContextMenu
            // 
            trayContextMenu.ImageScalingSize = new Size(28, 28);
            trayContextMenu.Items.AddRange(new ToolStripItem[] { menuItem_Show, menuItem_Settings, toolStripSeparator2, menuItem_startServer, menuItem_stopServer, menuItem_Error, toolStripSeparator1, menuItem_Exit, toolStripSeparator3 });
            trayContextMenu.Name = "trayContextMenu";
            trayContextMenu.Size = new Size(134, 154);
            // 
            // menuItem_Show
            // 
            menuItem_Show.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            menuItem_Show.Name = "menuItem_Show";
            menuItem_Show.Size = new Size(133, 22);
            menuItem_Show.Text = "Show";
            menuItem_Show.Click += menuItem_Show_Click;
            // 
            // menuItem_Settings
            // 
            menuItem_Settings.Name = "menuItem_Settings";
            menuItem_Settings.Size = new Size(133, 22);
            menuItem_Settings.Text = "Settings";
            menuItem_Settings.Click += menuItem_Settings_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(130, 6);
            // 
            // menuItem_startServer
            // 
            menuItem_startServer.Enabled = false;
            menuItem_startServer.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            menuItem_startServer.Name = "menuItem_startServer";
            menuItem_startServer.Size = new Size(133, 22);
            menuItem_startServer.Text = "Start Server";
            menuItem_startServer.Click += menuItem_startServer_Click;
            // 
            // menuItem_stopServer
            // 
            menuItem_stopServer.Enabled = false;
            menuItem_stopServer.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            menuItem_stopServer.Name = "menuItem_stopServer";
            menuItem_stopServer.Size = new Size(133, 22);
            menuItem_stopServer.Text = "Stop Server";
            menuItem_stopServer.Click += menuItem_stopServer_Click;
            // 
            // menuItem_Error
            // 
            menuItem_Error.ForeColor = Color.Crimson;
            menuItem_Error.Name = "menuItem_Error";
            menuItem_Error.Size = new Size(133, 22);
            menuItem_Error.Text = "Error";
            menuItem_Error.Visible = false;
            menuItem_Error.Click += menuItem_Error_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(130, 6);
            // 
            // menuItem_Exit
            // 
            menuItem_Exit.Name = "menuItem_Exit";
            menuItem_Exit.Size = new Size(133, 22);
            menuItem_Exit.Text = "Exit";
            menuItem_Exit.Click += menuItem_Exit_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(130, 6);
            // 
            // serverInfoGroup
            // 
            serverInfoGroup.Controls.Add(compNameText);
            serverInfoGroup.Controls.Add(label2);
            serverInfoGroup.Controls.Add(ipLabel);
            serverInfoGroup.Controls.Add(ipText);
            serverInfoGroup.Font = new Font("Segoe UI", 9F, FontStyle.Bold | FontStyle.Italic, GraphicsUnit.Point);
            serverInfoGroup.Location = new Point(12, 205);
            serverInfoGroup.Name = "serverInfoGroup";
            serverInfoGroup.Size = new Size(274, 145);
            serverInfoGroup.TabIndex = 15;
            serverInfoGroup.TabStop = false;
            serverInfoGroup.Text = "Server Info";
            serverInfoGroup.Visible = false;
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(28, 28);
            menuStrip1.Items.AddRange(new ToolStripItem[] { settingsToolStripMenuItem, helpToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(295, 24);
            menuStrip1.TabIndex = 18;
            menuStrip1.Text = "menuStrip1";
            // 
            // settingsToolStripMenuItem
            // 
            settingsToolStripMenuItem.Name = "settingsToolStripMenuItem";
            settingsToolStripMenuItem.Size = new Size(61, 20);
            settingsToolStripMenuItem.Text = "Settings";
            settingsToolStripMenuItem.Click += settingsToolStripMenuItem_Click;
            // 
            // helpToolStripMenuItem
            // 
            helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            helpToolStripMenuItem.Size = new Size(44, 20);
            helpToolStripMenuItem.Text = "Help";
            helpToolStripMenuItem.Click += helpToolStripMenuItem_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(295, 358);
            Controls.Add(serverInfoGroup);
            Controls.Add(statusGroup);
            Controls.Add(menuStrip1);
            HelpButton = true;
            Icon = (Icon)resources.GetObject("$this.Icon");
            MainMenuStrip = menuStrip1;
            MaximizeBox = false;
            MinimumSize = new Size(311, 389);
            Name = "MainForm";
            SizeGripStyle = SizeGripStyle.Hide;
            Text = "HIRD";
            FormClosing += OnClose;
            Load += MainForm_Load;
            Resize += MainForm_Resize;
            ((System.ComponentModel.ISupportInitialize)statusIndicator).EndInit();
            statusGroup.ResumeLayout(false);
            statusGroup.PerformLayout();
            trayContextMenu.ResumeLayout(false);
            serverInfoGroup.ResumeLayout(false);
            serverInfoGroup.PerformLayout();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private CheckBox startStopServerButton;
        private Label label2;
        private Label compNameText;
        private Label ipLabel;
        private Label ipText;
        private ListBox connectedClientsList;
        private Label connectedClientsLabel;
        private PictureBox statusIndicator;
        private GroupBox statusGroup;
        private Label statusLabel;
        private NotifyIcon notifyIcon;
        private GroupBox serverInfoGroup;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem helpToolStripMenuItem;
        private ToolStripMenuItem settingsToolStripMenuItem;
        private Label errorLabel;
        private ContextMenuStrip trayContextMenu;
        private ToolStripMenuItem menuItem_Show;
        private ToolStripMenuItem menuItem_Settings;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem menuItem_startServer;
        private ToolStripMenuItem menuItem_stopServer;
        private ToolStripMenuItem menuItem_Error;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem menuItem_Exit;
        private ToolStripSeparator toolStripSeparator3;
    }
}