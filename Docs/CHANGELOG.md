# MATE (MUI Accelerator Table Englishizer) Change Log 📋

## v1.0.4 *(current)* 🆕
#### 🛠️ Fixes:
 - Fixed Windows version detection: the accurate value returned by the native `RtlGetVersion` function was always overwritten by the .NET fallback (`Environment.OSVersion`), which is subject to application compatibility shims.
 - Fixed the marshaling of the `OsVersionInfoEx` structure, whose `CsdVersion` field was marshaled as ANSI instead of Unicode, producing a wrong structure size for `RtlGetVersion`.
 - Fixed MUI descriptor validations being performed before trimming the arguments, which allowed storing an invalid 7-digit CRC-32 checksum.
 - Fixed `MuiDescriptor` and `LanguageConfiguration` classes overriding `Equals` without overriding `GetHashCode`.

#### 🌟 Improvements:
 - Added a unit test project (MSTest) covering MUI descriptors, language configurations, accelerator table definitions, CRC-32 checksum computation, path helpers and Windows version detection.
 - Stricter fail-fast validations: MUI descriptor checksums must be hexadecimal, and language configurations reject null MUI descriptors.
 - Language configurations now keep a private, read-only copy of their MUI descriptors collection.

## v1.0.3 🔄
#### 🌟 Improvements:
 - Implemented an execution guard to block the program from running when a system reboot is pending.
 - Updated the optional CRC-32 Calculator app in the **Tools** folder.
 - Minor source-code changes affecting console output messages.

## v1.0.2 🔄
#### 🌟 Improvements:
 - Registered a new variant of `shell32.dll.mui` for Windows 10.
 - Added a CRC-32 calculator app in the **Tools** folder.
 - Minor source-code changes.
 
## v1.0.1 🔄
#### 🌟 Improvements:
 - Implemented pseudo-support for "Englishizing" the modern Windows 11 Notepad.
 - Introduced an 'ENVIRONMENT SPECIFICATIONS' section within the application interface.
 - Restricted application execution to Windows 10 and Windows 11 versions.
 - Enforced single-instance application behavior.
 - Enabled long-path awareness support.
 - Directory structure for storing temporary MUI files has been reworked.
 - Added internal security checks and object equality implementations.

## v1.0 🔄
Initial Release.