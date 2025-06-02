📌 Notes

- It's important to edit `ProcessHzMap.cfg` before use. Default value is set to 60 Hz.
- The application runs in the background without a visible window.
- All messages are minimized to only necessary prompts.

-----------------------------------

📦 What's Included

```
SetHzMonitor/
├── config/
│   ├── ProcessHzMap.cfg  # Maps process names to refresh rates
│   └── Hotkey.cfg        # Global hotkey for quitting
├── tool/
│   └── SetHzTool.exe     # The helper tool that applies the new refresh rate
├── SetHzMonitor.exe      # Main executable
└── README.md             # This file
```

-----------------------------------

⚙️ How It Works

- Monitors a list of applications defined in `ProcessHzMap.cfg`.
- When any of those applications are running:
  - The monitor's refresh rate is changed.
- When none are running:
  - The refresh rate is restored to the default value.
- Refresh rate changes are handled by the bundled `SetHzTool.exe` in the `tool` folder.
- You can quit the program using a customizable hotkey.

-----------------------------------

🛠 Configuration

🔄 `config/ProcessHzMap.cfg`

This file maps process names to refresh rates. Format:

```
processName1=144
processName2=120
...
default=60
```
> Comments using `#` or `;` are ignored.

- Each processName should be the exact name of the process (without .exe).
- The refreshRate is an integer representing the desired monitor refresh rate (Hz).
- The default line specifies the refresh rate to use when no monitored processes are running.

-----------------------------------

⌨️ `config/Hotkey.cfg`

Defines the global hotkey to exit the app. Format:

```
Ctrl+Alt+Q
```

Supported modifiers:
- `Ctrl`
- `Alt`
- `Shift`

Only one letter (A–Z) is supported as the key.

-----------------------------------

🚀 How to Use

1. Edit `ProcessHzMap.cfg` with your apps and refresh rates.
2. Ensure that .NET Framework 4.8 or later is installed on your system. If it’s not installed, please download and install it from Microsoft's official site.
3. Launch `SetHzMonitor.exe`.
4. It automatically adjusts refresh rates based on your active applications.
5. Use your configured hotkey (e.g., `Ctrl+Alt+Q`) to exit the app safely.

-----------------------------------

🔒 On Exit

- A confirmation dialog will appear when the quit hotkey is pressed.
- The app restores the default refresh rate if needed.
- It unregisters the hotkey and shuts down cleanly.
