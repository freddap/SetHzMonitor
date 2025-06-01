# Define working directory
$BasePath = Split-Path -Parent ([System.Diagnostics.Process]::GetCurrentProcess().MainModule.FileName)

# Create absolute filepaths
$hzMapPath = Join-Path $BasePath "config\ProcessHzMap.cfg"
$hotkeyConfigPath = Join-Path $BasePath "config\Hotkey.cfg"
$setHzToolPath = Join-Path $BasePath "tool\SetHzTool.exe"

# Load ProcessHzMap (process name to Hz mapping)
$processHzMap = @{}
$defaultHz = $null
if (Test-Path $hzMapPath) {
    Get-Content $hzMapPath | ForEach-Object {
        $line = $_.Trim()
        if ($line -and -not $line.StartsWith('#') -and -not $line.StartsWith(';')) {
            if ($line -match '^\[DEFAULT\]\s*=\s*(\d+)$') {
                $defaultHz = [int]$matches[1]
            } elseif ($line -match '^(.+?)\s*=\s*(\d+)$') {
                $processHzMap[$matches[1].Trim().ToLower()] = [int]$matches[2]
            }
        }
    }
} else {
    [System.Windows.Forms.MessageBox]::Show("Missing ProcessHzMap.cfg", "Error", [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Error)
    exit 1
}

# Load and parse hotkey from config
$hotkeyString = Get-Content $hotkeyConfigPath | Select-Object -First 1
$MOD_ALT = 0x1
$MOD_CONTROL = 0x2
$MOD_SHIFT = 0x4
$modifiers = 0
$keyChar = $null

if ($hotkeyString -match "(?i)(Ctrl)?\+?(Alt)?\+?(Shift)?\+?([A-Z])") {
    if ($matches[1]) { $modifiers += $MOD_CONTROL }
    if ($matches[2]) { $modifiers += $MOD_ALT }
    if ($matches[3]) { $modifiers += $MOD_SHIFT }
    $keyChar = $matches[4]
} else {
    [System.Windows.Forms.MessageBox]::Show("Invalid Hotkey format in Hotkey.cfg. Expected format: Ctrl+Alt+Q", "Error", [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Error)
    exit 1
}

$vkKey = [System.Windows.Forms.Keys]::$keyChar

# Add C# type for hotkey listener (without null conditional operator)
Add-Type -ReferencedAssemblies "System.Windows.Forms.dll", "System.Drawing.dll" @"
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

public class KeyboardListener : Form {
    public const int WM_HOTKEY = 0x0312;

    [DllImport("user32.dll")]
    public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, Keys vk);

    [DllImport("user32.dll")]
    public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    public event EventHandler QuitPressed;

    protected override void WndProc(ref Message m) {
        if (m.Msg == WM_HOTKEY && (int)m.WParam == 1) {
            if (QuitPressed != null) {
                QuitPressed(this, EventArgs.Empty);
            }
        }
        base.WndProc(ref m);
    }
}
"@

# Create and show the hidden form to receive hotkey messages
$form = New-Object KeyboardListener
$form.TopMost = $true
$form.Visible = $false

# Register the hotkey
$HOTKEY_ID = 1
if (-not [KeyboardListener]::RegisterHotKey($form.Handle, $HOTKEY_ID, $modifiers, $vkKey)) {
    [System.Windows.Forms.MessageBox]::Show("Failed to register hotkey.", "Error", [System.Windows.Forms.MessageBoxButtons]::OK, [System.Windows.Forms.MessageBoxIcon]::Error)
    exit 1
}

# Path to the FirstRun marker file
$firstRunFile = "$env:LOCALAPPDATA\SetHzMonitor_FirstRun.flag"

# Show startup message
if (-not (Test-Path $firstRunFile)) {
    Add-Type -AssemblyName System.Windows.Forms
    [System.Windows.Forms.MessageBox]::Show(@"
Thank you for installing SetHzMonitor!

It looks like this is the first time you launch the application.
SetHzMonitor will now run silently in the background and automatically adjust your monitor's refresh rate based on which applications are active.

You can press Ctrl + Alt + Q at any time to quit the application.

This message will not appear the next time you launch the app.

We hope SetHzMonitor proves useful to you!
"@)
    New-Item -ItemType File -Path $firstRunFile -Force | Out-Null
}

# Variable to control main loop
$global:exitRequested = $false

# Subscribe to QuitPressed event
$form.add_QuitPressed({
    $answer = [System.Windows.Forms.MessageBox]::Show(
        "Do you want to quit SetHzMonitor?", 
        "Confirm Exit", 
        [System.Windows.Forms.MessageBoxButtons]::YesNo, 
        [System.Windows.Forms.MessageBoxIcon]::Question)

    if ($answer -eq [System.Windows.Forms.DialogResult]::Yes) {
        $global:exitRequested = $true
    }
})

# Variable to track current Hz state
$currentHz = $null

# Main loop
while (-not $global:exitRequested) {
    [System.Windows.Forms.Application]::DoEvents()

    $activeHzValues = @()

    foreach ($entry in $processHzMap.GetEnumerator()) {
        $procName = $entry.Key
        $hzValue = $entry.Value

        if (Get-Process -Name $procName -ErrorAction SilentlyContinue) {
            $activeHzValues += $hzValue
        }
    }

    if ($activeHzValues.Count -gt 1) {
        if (($activeHzValues | Sort-Object -Unique).Count -gt 1) {
            [System.Windows.Forms.MessageBox]::Show(
                "Multiple running processes require conflicting refresh rates. SetHzMonitor will pause to avoid issues.",
                "Hz Conflict Detected", 
                [System.Windows.Forms.MessageBoxButtons]::OK, 
                [System.Windows.Forms.MessageBoxIcon]::Warning)
            Start-Sleep -Seconds 5
            continue
        }
    }

    $targetHz = if ($activeHzValues.Count -gt 0) {
        $activeHzValues[0]
    } elseif ($defaultHz) {
        $defaultHz
    } else {
        $null
    }

    if ($targetHz -and $targetHz -ne $currentHz) {
        & $setHzToolPath $targetHz
        $currentHz = $targetHz
    }

    Start-Sleep -Seconds 3
}

# Cleanup
if ($form.Handle -ne [IntPtr]::Zero) {
    [KeyboardListener]::UnregisterHotKey($form.Handle, $HOTKEY_ID) | Out-Null
}
$form.Dispose()