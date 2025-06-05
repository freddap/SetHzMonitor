# Getting Started

Welcome! 

This guide will help you get SetHzMonitor up and running quickly.

## Download Options

There are two versions of SetHzMonitor available for download:

- **SetHzMonitorSetup**
  - Allows you to create a desktop shortcut  
  - Optionally adds SetHzMonitor to your Windows startup, so it launches automatically when you log in

- **SetHzMonitorPortable**
  - No installation required  
  - Can be run from any folder or USB stick  
  - Leaves no traces on the system

Choose the version that best fits your needs.

## How to Use

1. **Edit the Configuration**  
   Open `ProcessHzMap.cfg` and define which refresh rate (Hz) should be used for specific applications.  
   Example:
<pre>
# 24p / 30p / 60p content (e.g. YouTube, Netflix, most films)
mpc-hc=120
vlc=120
chrome=120
firefox=120

# 25p / 50p content (e.g. European TV, PAL content)
eu_player=100

# Games: match fps to Hz
# 60 fps = 60 Hz, 120 fps = 120 Hz
fallout4=60
DOOMEternalx64vk=120
</pre>

2. **Launch the App**  
Run `SetHzMonitor.exe`. It will start silently and appear as an icon in the system tray.

3. **Use the Tray Menu**  
Right-click the system tray icon to access the following options:
- **Edit config** – Open `ProcessHzMap.cfg` directly for editing.
- **Restart** – Reload the config if you've made changes.
- **Exit** – Close the application.

4. **Refresh Rate Notifications**  
Whenever the refresh rate changes, a clean on-screen notification (via NeonPopup) will display the current Hz.

## Optional: Custom Refresh Rates

If your display supports it, you can define additional refresh rates beyond the default ones listed by Windows. This can be useful if you want more granular control — for example, adding 72 Hz for smoother video playback or 100 Hz for PAL content.

### NVIDIA Users

You can create custom resolutions using the **NVIDIA Control Panel**:

1. Open **NVIDIA Control Panel** (Admin) → *Change resolution*.
2. Click **Customize** → **Create Custom Resolution**. (Unavailable in DSC mode)
3. Set your preferred refresh rate (e.g., 72, 75, 100 Hz).
4. Save and apply the new mode.

Once added, you can reference that refresh rate in `ProcessHzMap.cfg`.

### AMD Users

If you use an AMD GPU, custom resolutions can be added through **AMD Software: Adrenalin Edition**:

1. Open **AMD Software** → Go to the **Display** tab.
2. Scroll to the **Custom Resolutions** section.
3. Create a new resolution with your desired refresh rate.
4. Save it and select it from Windows or use it via `SetHzMonitor`.

**Note:** Not all monitors support all refresh rates — test carefully to avoid display issues!

---

Enjoy smoother gaming and video playback with dynamic refresh rate control!
