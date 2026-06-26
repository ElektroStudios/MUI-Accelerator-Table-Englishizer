<!-- Common Project Tags:
command-line 
console-applications 
desktop-app 
desktop-application 
dotnet 
netframework 
netframework48 
tool 
tools 
vbnet 
visualstudio 
windows 
windows-app 
windows-application 
windows-applications 
notepad 
hotkeys 
windows-10 
english 
explorer 
spanish 
shortcut-key 
mui 
console-application 
keyboard-shortcuts 
wordpad 
windows-explorer 
accelerators 
windows11 
shell32 
patch-tools 
shell32dll 
 -->

# MATE (MUI Accelerator Table 'Englishizer')

### Enables the use of English keyboard shortcuts in Explorer, Notepad, and Wordpad on Windows 10 and 11 for users running the operating system in another language.

------------------

## 👋 Introduction

**MATE** is a command-line utility designed to revert localized keyboard shortcut tables on specific MUI files to their English (en-US) defaults. 

In localized Windows environments, keyboard shortcuts in core applications like Explorer, Notepad, and Wordpad are tailored for that specific language, often causing workflow friction for users who prefer globally standardized keyboard shortcuts such as `Ctrl + A` to select all. This utility automates the restoration of these keyboard shortcuts into your system's MUI files, specifically targeting these three mentioned components.

> [!IMPORTANT]
> Currently, **MATE** supports only Spanish (Spain) MUI files (es-ES). If your Windows environment uses a different language, running this application will simply have no effect.

## 👌 Features

*   **Targeted Restoration**: Automatically identifies and applies native English accelerator tables to `shell32.dll.mui` (affecting Windows Explorer), `notepad.exe.mui` and `wordpad.exe.mui` files.
*   **Safety-First Design**: Utilizes CRC-32 checksum verification to ensure only known, compatible MUI files are modified.
*   **Non-Destructive Process**: Employs a temporary staging area and native Windows mechanisms for pending file rename operations after a system reboot, ensuring the original files are never directly modified while the application is running.
*   **Supports both Windows 10 and 11**: Fully tested on Windows 10 20H2 and 22H2, and Windows 11 25H2, supporting both x86 and x64 architectures.
*   **Supports Classic Wordpad on Windows 11**: Fully tested for the **WinAero**'s classic version of Wordpad on Windows 11. [Read more at Winaero](https://winaero.com/wordpad-for-windows-11/)

## 🖼️ Screenshots and GIFs

![screenshot](/Images/screenshot1.png)

![screenshot](/Images/screenshot2.png)

![screenshot](/Images/screenshot3.png)

![screenshot](/Images/demo1.gif)

![screenshot](/Images/demo2.gif)

## 📝 Requirements

- Operating System: Microsoft Windows 10 or Windows 11 (x86/x64).
- User Permissions: Administrative privileges are required to run this program.

## 🤖 Getting Started

Download the latest release by clicking [here](https://github.com/ElektroStudios/MUI-Accelerator-Table-Englishizer/releases/latest) and start using it!.

## 🔄 Change Log

Explore the complete list of changes, bug fixes, and improvements across different releases by clicking [here](/Docs/CHANGELOG.md).

## 🏆 Credits

This work relies on the following resources: 

 - [Resource Hacker](https://www.angusj.com/resourcehacker/)
 - [SysInternals Movefile](https://learn.microsoft.com/sysinternals/downloads/pendmoves#movefile-usage)
 - [.NET Framework](https://dotnet.microsoft.com/en-us/download/dotnet-framework)

## ❓ FAQ

### Which Windows versions are supported?
Currently, this tool is designed for **Windows 10 and 11**. It specifically supports Windows 10 20H2 and 22H2, and Windows 11 25H2, supporting both x86 and x64 architectures.

### Which system languages are supported?
Currently, this tool supports only 'es-ES' (Spanish - Spain) MUI files, meaning that MUI files in other localizations will not be modified during program execution.

### Is it safe to use?
Yes, it is highly safe. The program operates using file checksum validations; if an MUI file does not match the known signatures hardcoded in the source code, it is completely ignored and remains untouched. Furthermore, the original system files are never directly modified during the program's execution. All replacements are scheduled via the Windows Session Manager to occur during the next system reboot, acting as a safe buffer.

### What should I do if I want to cancel the pending MUI file replacements or if something goes wrong?
If you need to cancel a pending MUI file replacement operation or if an issue occurs, you can manually clear the scheduled tasks:
1. Press `Win + R`, type `regedit`, and hit Enter.
2. Navigate to: `HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\Session Manager`.
3. Locate the value named **`PendingFileRenameOperations`**.
4. Right-click and delete this value.
   * **Warning:** Ensure that you only delete this value if you are certain it does not contain other critical scheduled file operations added by the operating system or other third party software.
   Deleting this value safely discards the pending MUI file replacement operations, ensuring no modifications are applied to your system on the next reboot.

![screenshot](/Images/faq1.png)

### Can I just delete the temporary pending MUI files to cancel the pending operations on next system restart?
Partially yes, but you should absolutely **not do it**. 

Once a MUI file is successfully processed by the application, it schedules a file swap operation for the next system restart. This swap operation involves backing up your original system MUI files and replacing them with the modified versions located in the temporary directory.

If you delete the temporary pending MUI files while the registry entry is still active, the operating system will still attempt to perform the file swap on reboot. It will rename your original MUI file to a backup like "notepad.exe.mui.bak", but since the modified MUI replacement file is missing, the system will end up with no valid MUI file at all. This will render your system resources inaccessible and cause critical stability or UI display issues.

The only safe and recommended way to abort a pending operation is by clearing the registry entries in **`PendingFileRenameOperations`**, as described in the section above.

### I am a developer and want to add support for other languages. Where should I start?
Adding support for another system language is simple. You just need to follow the existing structure:
1. Create a new class following the `LanguageConfiguration_esES` pattern (e.g., `LanguageConfiguration_ruRU` for Russian, or `LanguageConfiguration_frFR` for french).
2. Add the required resource definitions for your target language's accelerator tables in the `AccTables.vb` file.
3. Register your new language configuration in the main application logic within the `LangConfigs` array, so it can be detected during the file validation process.

Since the architecture is modular, you only need to provide the correct checksums and accelerator data for your specific localization, and the application will handle the rest.

## ⚠️ Disclaimer:

This software is provided "as is," without warranty of any kind. The author is not responsible for any system instability, data loss, or operating system malfunctions. By running this application, you acknowledge that you have the knowledge to troubleshoot system-level errors and have taken the necessary precautions (e.g., creating a System Restore point or a full backup).

**Use this software at your own risk.** This application modifies system-level MUI files to change keyboard accelerator tables. While the process is designed with safety mechanisms, modifying system files always may carry issues with System File Protection (SFC), DISM, or Windows Update functionalities. 

## 💪 Contributing

Your contribution is highly appreciated!. If you have any ideas, suggestions, or encounter issues, feel free to open an issue by clicking [here](https://github.com/ElektroStudios/MUI-Accelerator-Table-Englishizer/issues/new/choose). 

Your input helps make this Work better for everyone. Thank you for your support! 🚀

## 💰 Beyond Contribution 

This work is distributed for educational purposes and without any profit motive. However, if you find value in my efforts and wish to support and motivate my ongoing work, you may consider contributing financially through the following options:

<br></br>
<p align="center"><img src="/Images/github_circle.png" height=100></p>
<p align="center">__________________</p>
<h3 align="center">Becoming my sponsor on Github:</h3>
<p align="center">You can show me your support by clicking <a href="https://github.com/sponsors/ElektroStudios/">here</a>, <br align="center">contributing any amount you prefer, and unlocking rewards!</br></p>
<br></br>

<p align="center"><img src="/Images/paypal_circle.png" height=100></p>
<p align="center">__________________</p>
<h3 align="center">Making a Paypal Donation:</h3>
<p align="center">You can donate to me any amount you like via Paypal by clicking <a href="https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=E4RQEV6YF5NZY">here</a>.</p>
<br></br>

<p align="center"><img src="/Images/envato_circle.png" height=100></p>
<p align="center">__________________</p>
<h3 align="center">Purchasing software of mine at Envato's Codecanyon marketplace:</h3>
<p align="center">If you are a .NET developer, you may want to explore '<b>DevCase Class Library for .NET</b>', <br align="center">a huge set of APIs that I have on sale. Check out the product by clicking <a href="https://codecanyon.net/item/elektrokit-class-library-for-net/19260282">here</a></br><br align="center"><i>It also contains all piece of reusable code that you can find across the source code of my open source works.</i></p>
<br></br>

<h2 align="center"><u>Your support means the world to me! Thank you for considering it!</u> 👍</h2>
