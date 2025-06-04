using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Reflection;
using System.Drawing;

class Program
{
    public static DialogResult ShowTopmostMessageBox(string text, string caption = "SetHzMonitor", MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.Information)
    {
        using (Form topmostForm = new Form())
        {
            topmostForm.TopMost = true;
            topmostForm.StartPosition = FormStartPosition.Manual;
            topmostForm.Size = new System.Drawing.Size(1, 1);
            topmostForm.Location = new System.Drawing.Point(-1000, -1000); // HIDE
            topmostForm.Show();
            topmostForm.Focus();
            return MessageBox.Show(topmostForm, text, caption, buttons, icon);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DEVMODE
    {
        private const int CCHDEVICENAME = 32;
        private const int CCHFORMNAME = 32;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
        public string dmDeviceName;

        public ushort dmSpecVersion;
        public ushort dmDriverVersion;
        public ushort dmSize;
        public ushort dmDriverExtra;
        public uint dmFields;

        public int dmPositionX;
        public int dmPositionY;
        public uint dmDisplayOrientation;
        public uint dmDisplayFixedOutput;

        public short dmColor;
        public short dmDuplex;
        public short dmYResolution;
        public short dmTTOption;
        public short dmCollate;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHFORMNAME)]
        public string dmFormName;

        public ushort dmLogPixels;
        public uint dmBitsPerPel;
        public uint dmPelsWidth;
        public uint dmPelsHeight;

        public uint dmDisplayFlags;
        public uint dmDisplayFrequency;

        public uint dmICMMethod;
        public uint dmICMIntent;
        public uint dmMediaType;
        public uint dmDitherType;
        public uint dmReserved1;
        public uint dmReserved2;

        public uint dmPanningWidth;
        public uint dmPanningHeight;

        public DEVMODE()
        {
            dmDeviceName = string.Empty;
            dmSpecVersion = 0;
            dmDriverVersion = 0;
            dmSize = 0;
            dmDriverExtra = 0;
            dmFields = 0;
            dmPositionX = 0;
            dmPositionY = 0;
            dmDisplayOrientation = 0;
            dmDisplayFixedOutput = 0;
            dmColor = 0;
            dmDuplex = 0;
            dmYResolution = 0;
            dmTTOption = 0;
            dmCollate = 0;
            dmFormName = string.Empty;
            dmLogPixels = 0;
            dmBitsPerPel = 0;
            dmPelsWidth = 0;
            dmPelsHeight = 0;
            dmDisplayFlags = 0;
            dmDisplayFrequency = 0;
            dmICMMethod = 0;
            dmICMIntent = 0;
            dmMediaType = 0;
            dmDitherType = 0;
            dmReserved1 = 0;
            dmReserved2 = 0;
            dmPanningWidth = 0;
            dmPanningHeight = 0;
        }
    }

    const int ENUM_CURRENT_SETTINGS = -1;
    const int CDS_UPDATEREGISTRY = 0x01;
    const int DISP_CHANGE_SUCCESSFUL = 0;

    [DllImport("user32.dll")]
    public static extern int EnumDisplaySettings(string? deviceName, int modeNum, ref DEVMODE devMode);

    [DllImport("user32.dll")]
    public static extern int ChangeDisplaySettings(ref DEVMODE devMode, int flags);

    public static bool SetRefreshRate(int refreshRate)
    {
        DEVMODE dm = new DEVMODE();
        dm.dmSize = (ushort)Marshal.SizeOf(typeof(DEVMODE));

        if (EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref dm) != 0)
        {
            dm.dmDisplayFrequency = (uint)refreshRate;
            dm.dmFields = 0x400000; // DM_DISPLAYFREQUENCY

            int result = ChangeDisplaySettings(ref dm, CDS_UPDATEREGISTRY);
            return result == DISP_CHANGE_SUCCESSFUL;
        }
        return false;
    }

    private static Icon GetEmbeddedIcon()
    {   
        var assembly = Assembly.GetExecutingAssembly();
        using Stream? iconStream = assembly.GetManifestResourceStream("SetHzMonitor.SetHzMonitor.ico");
        if (iconStream == null)
            throw new FileNotFoundException("Embedded icon not found.");
        return new Icon(iconStream);
    }

    class HiddenForm : Form
    {
        public HiddenForm()
            {
        this.ShowInTaskbar = false;
        this.WindowState = FormWindowState.Minimized;
        this.Visible = false;
        this.Load += (s, e) => this.Hide(); // HIDE
            }
    }

    static Dictionary<string, int> LoadProcessHzMap(string configPath)
    {
        if (!File.Exists(configPath))
        {
            ShowTopmostMessageBox("Missing ProcessHzMap.cfg");
            return new Dictionary<string, int>();
        }

        var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var lines = File.ReadAllLines(configPath)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Where(line => !line.TrimStart().StartsWith("#") && !line.TrimStart().StartsWith(";"));

        foreach (var line in lines)
        {
            var lineNoComment = line.Split(';')[0].Trim();
            var parts = lineNoComment.Split('=');
            if (parts.Length == 2 && int.TryParse(parts[1].Trim(), out int hz))
            {
                map[parts[0].Trim()] = hz;
            }
        }

        if (!map.ContainsKey("default"))
        {
            ShowTopmostMessageBox("ProcessHzMap.cfg must contain a 'default=someHz' entry.");
        }

        return map;
    }

    static void Main(string[] args)
    {
        string basePath = AppDomain.CurrentDomain.BaseDirectory;
        string processHzMapPath = Path.Combine(basePath, "config", "ProcessHzMap.cfg");
        var processHzMap = LoadProcessHzMap(processHzMapPath);
        if (!processHzMap.ContainsKey("default"))
        {
            ShowTopmostMessageBox("ProcessHzMap.cfg must contain a 'default=someHz' entry.");
            return;
        }

        int? currentHz = null;

        NotifyIcon notifyIcon = new NotifyIcon();

        var assembly = Assembly.GetExecutingAssembly();
        foreach (var resourceName in assembly.GetManifestResourceNames())
        {       
            Console.WriteLine("Resurs: " + resourceName);
        }

        try
        {
            notifyIcon.Icon = GetEmbeddedIcon();
        }
        catch (FileNotFoundException ex)
        {
            ShowTopmostMessageBox(ex.Message);
        }

        notifyIcon.Visible = true;
        notifyIcon.Text = $"{currentHz ?? 0} Hz";

        // CREATE CONTEXTMENUSTRIP FOR NOTIFYICON
        var contextMenu = new ContextMenuStrip();

        // OPEN CONFIG
        var openFolderItem = new ToolStripMenuItem("Open config");
        openFolderItem.Click += (s, e) =>
        {
            string folderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config");
            if (Directory.Exists(folderPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = folderPath,
                    UseShellExecute = true,
                    Verb = "open"
                });
            }
            else
            {
                ShowTopmostMessageBox("Missing config directory.");
            }
        };
        contextMenu.Items.Add(openFolderItem);

        // RESTART
        var restartItem = new ToolStripMenuItem("Restart");
        restartItem.Click += (s, e) =>
        {
            processHzMap = LoadProcessHzMap(processHzMapPath);
        };
        contextMenu.Items.Add(restartItem);

        // EXIT
        bool running = true;
        var exitItem = new ToolStripMenuItem("Exit");
        exitItem.Click += (s, e) =>
        {
            running = false;
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
            Application.Exit();
        };
        contextMenu.Items.Add(exitItem);

        // ADD MENU TO NOTIFYICON
        notifyIcon.ContextMenuStrip = contextMenu;

        HiddenForm form = new HiddenForm();

        // START LOOP IN THE BACKGROUND
        Task.Run(() =>
        {
            while (running)
            {
                var runningProcesses = new List<string>();
                foreach (var procName in processHzMap.Keys.Where(k => !k.Equals("default", StringComparison.OrdinalIgnoreCase)))
                {
                    if (Process.GetProcessesByName(procName).Any())
                    {
                        runningProcesses.Add(procName);
                    }
                }

                if (runningProcesses.Count > 1)
                {
                    var hzSet = runningProcesses.Select(p => processHzMap[p]).Distinct().ToList();
                    if (hzSet.Count > 1)
                    {
                        // DISPLAY WARNING MESSAGE
                        Application.OpenForms[0]?.BeginInvoke(new Action(() =>
                        {
                            ShowTopmostMessageBox(
                                $"Conflict detected: Multiple monitored processes running simultaneously with different refresh rates.\nProcesses: {string.Join(", ", runningProcesses)}",
                                "SetHzMonitor Warning",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning
                            );
                        }));
                    }
                }

                string? selectedProcess = null;
                foreach (var procName in processHzMap.Keys)
                {
                    if (!procName.Equals("default", StringComparison.OrdinalIgnoreCase) && runningProcesses.Contains(procName))
                    {
                        selectedProcess = procName;
                        break;
                    }
                }

                int targetHz = selectedProcess != null ? processHzMap[selectedProcess] : processHzMap["default"];

                if (currentHz != targetHz)
                {
                    if (!SetRefreshRate(targetHz))
                    {
                        Application.OpenForms[0]?.BeginInvoke(new Action(() =>
                        {
                            ShowTopmostMessageBox($"Failed to set refresh rate to {targetHz}");
                        }));
                    }
                    else
                    {
                        currentHz = targetHz;
                        notifyIcon.Text = $"{currentHz} Hz";
                    }
                }

                Thread.Sleep(3000);
            }

            form.Invoke(new Action(() => form.Close()));
        });

        // START LOOP
        Application.Run(form);
    }
}
