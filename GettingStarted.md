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

---

Enjoy smoother gaming and video playback with dynamic refresh rate control!
