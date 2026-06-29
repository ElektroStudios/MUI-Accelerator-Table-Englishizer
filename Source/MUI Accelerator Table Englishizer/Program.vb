
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.Diagnostics
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Threading

Imports DevCase.Core.Windows.Common
Imports DevCase.Win32

Imports Microsoft.VisualBasic

Imports Microsoft.Win32

#End Region

#Region " Program "

''' <summary>
''' Represents the main application execution context.
''' </summary>
Public Module Program

#Region " Fields "

    ''' <summary>
    ''' The collection of supported <see cref="LanguageConfiguration"/> instances.
    ''' <para></para>
    ''' This array acts as the registry for all available localizations, enabling 
    ''' the application to validate and patch MUI files for specific cultures.
    ''' </summary>
    ''' 
    ''' <remarks>
    ''' To expand support to additional languages, instantiate the new configuration class within this array.
    ''' </remarks>
    Private ReadOnly LangConfigs As New SortedList(Of String, LanguageConfiguration) From {
       {"Spanish (Spain)", New LanguageConfiguration_esES()}
    }

    ''' <summary>
    ''' Tracks the total count of MUI files successfully processed during the current execution.
    ''' </summary>
    Private muiFilesProcessed As Integer

    ''' <summary>
    ''' Indicates whether any errors occurred during the program execution.
    ''' </summary>
    Friend completedWithErrors As Boolean

    ''' <summary>
    ''' The <see cref="CultureInfo"/> instance representing the "en-US" culture.
    ''' </summary>
    Friend ReadOnly CultureInfoEnUs As New CultureInfo("en-US")

    ''' <summary>
    ''' Indicates whether the current process successfully created the system-wide mutex.
    ''' </summary>
    Private createdNewMutex As Boolean

    ''' <summary>
    ''' The unique global identifier used to ensure only one instance of the application runs at once.
    ''' </summary>
    Friend ReadOnly AppMutexId As String = $"Global\{My.Application.Info.Title}-1"

    ''' <summary>
    ''' The system-wide <see cref="Mutex"/> used to prevent multiple concurrent instances of the application.
    ''' </summary>
    Friend ReadOnly AppMutex As New Mutex(initiallyOwned:=True, Program.AppMutexId, Program.createdNewMutex)

#End Region

#Region " Entry Point "

    ''' <summary>
    ''' The main entry point of the application.
    ''' </summary>
    <DebuggerStepperBoundary>
    Public Sub Main()

        Thread.CurrentThread.CurrentCulture = Program.CultureInfoEnUs
        Thread.CurrentThread.CurrentUICulture = Program.CultureInfoEnUs

        Console.OutputEncoding = New UTF8Encoding()
        Console.BackgroundColor = ConsoleColor.Black
        Console.ForegroundColor = ConsoleColor.White

        Dim consoleTitle As String = $"{My.Application.Info.Title} {My.Application.Info.Version.ToString(fieldCount:=3)} — by ElektroStudios"
#If DEBUG Then
        Console.Title = consoleTitle
#End If
        ConsoleHelper.WriteColoredLine(" " & consoleTitle, ConsoleColor.Cyan)
        Console.WriteLine("╭──────────────────────────────────────────────────────────────────────────────────╮")
        Console.WriteLine("│ Purpose:                                                                         │")
        Console.WriteLine("│   This application restores English keyboard accelerator tables for Explorer,    │")
        Console.WriteLine("│   Notepad and Wordpad on Windows 10 and 11, replacing localized versions.        │")
        Console.WriteLine("│                                                                                  │")
        Console.WriteLine("│ Execution:                                                                       │")
        Console.WriteLine("│   It automatically locates and identifies supported MUI files, makes a temporary │")
        Console.WriteLine("│   copy on a safe directory, overwrites the resource accelerators table, and      │")
        Console.WriteLine("│   schedules the original MUI file replacements upon the next system reboot.      │")
        Console.WriteLine("│                                                                                  │")
        Console.WriteLine("│ Scope:                                                                           │")
        Console.WriteLine("│   Currently, only Spanish (Spain) 'es-ES' MUI files are supported.               │")
        Console.WriteLine("│                                                                                  │")
        Console.WriteLine("│ [!] Disclaimer:                                                                  │")
        Console.WriteLine("│   This program is provided 'as-is', without warranty of any kind.                │")
        Console.WriteLine("│   Use it at your own risk, and ensure you have a full system backup before usage.│")
        Console.WriteLine("╰──────────────────────────────────────────────────────────────────────────────────╯")
        Console.WriteLine()

        ' ======================================================================
        ' MUTEX CHECK (Verifies whether no other application instace is running)
        ' ======================================================================
        If Not Program.createdNewMutex Then
            ConsoleHelper.ExitWithMessage("Another instance of this application is already running.", 1, ConsoleColor.Red)
        End If

        ' ==============================================================================
        ' VOLATILE REGISTRY CHECK (Verifies whether a system reboot is already required)
        ' ==============================================================================
        If RegistryHelper.CheckIfCheckPointExists() Then
            ConsoleHelper.ExitWithMessage(" [X] ERROR: File replacements have already been scheduled and are pending a system reboot." & Environment.NewLine &
                                          "            Please restart your computer before running this application again.", 2, ConsoleColor.Red)
        End If
        ' ==============================================================================

        ConsoleHelper.WriteColoredLine(" ENVIRONMENT SPECIFICATIONS", ConsoleColor.Magenta)
        ConsoleHelper.WriteColoredLine("================================================================================", ConsoleColor.DarkGray)
        Console.WriteLine()
        ' App manifest is present: My.Computer.Info returns accurate, non-backwards-compatible OS details.
        ConsoleHelper.WriteColoredLine($" OS Platform Name  : {My.Computer.Info.OSFullName} ({ArchitectureHelper.GetOsArchitectureDisplayName()})", ConsoleColor.DarkYellow)
        ConsoleHelper.WriteColoredLine($" OS Build Version  : {My.Computer.Info.OSVersion}", ConsoleColor.DarkYellow)
        Dim userLanguageId As UShort = NativeMethods.GetUserDefaultUILanguage()
        Dim currentUiCulture As New CultureInfo(userLanguageId)
        ConsoleHelper.WriteColoredLine($" System UI Locale  : {currentUiCulture.DisplayName} ({currentUiCulture.Name})", ConsoleColor.DarkYellow)
        ConsoleHelper.WriteColoredLine($" Process Execution : {ArchitectureHelper.GetProcessArchitectureDisplayName()}", ConsoleColor.DarkYellow)
        Console.WriteLine()

        Dim isWin10OrGreater As Boolean = UtilWindows.IsWin10OrGreater
        If Not isWin10OrGreater Then
            ConsoleHelper.ExitWithMessage("This application requires Windows 10 or later.", 3, ConsoleColor.Red)
        End If

        ConsoleHelper.WriteColoredLine("Press 'Y' key to begin the accelerator tables modification, or 'Escape' key to exit...", ConsoleColor.Yellow)
        Do
            Dim keyInfo As ConsoleKeyInfo = Console.ReadKey(intercept:=True)
            If keyInfo.Key = ConsoleKey.Y Then
                Exit Do
            ElseIf keyInfo.Key = ConsoleKey.Escape Then
                Environment.Exit(0)
            End If
        Loop
        Console.WriteLine()

        For Each langConfig As KeyValuePair(Of String, LanguageConfiguration) In Program.LangConfigs

            Try
                Dim ciName As String = If(langConfig.Value IsNot Nothing, langConfig.Value.CultureInfo.Name, "null")
                Dim tempMuiDirectoryPath As String = Path.Combine(AppGlobals.BaseTempMuiDirectoryPath, ciName)

                ConsoleHelper.WriteColoredLine(" CONFIGURATION", ConsoleColor.Magenta)
                ConsoleHelper.WriteColoredLine("================================================================================", ConsoleColor.DarkGray)
                Console.WriteLine()
                If langConfig.Value Is Nothing Then
                    ConsoleHelper.WriteColoredLine($" Configuration for language '{langConfig.Key}' is null.", ConsoleColor.Red)
                    Console.WriteLine()
                    Continue For
                End If
                ConsoleHelper.WriteColoredLine($" Target MUI Culture Name: {ciName}", ConsoleColor.DarkYellow)
                ConsoleHelper.WriteColoredLine($" Temp path for pending files: {tempMuiDirectoryPath}", ConsoleColor.DarkYellow)
                Console.WriteLine()

                ' ============================================
                ' Windows 11 Modern Notepad AppX Package check
                ' ============================================
                Dim isWin11OrGreater As Boolean = UtilWindows.IsWin11OrGreater
                If isWin11OrGreater Then
                    Dim ciTwoLetter As String = langConfig.Value.CultureInfo.TwoLetterISOLanguageName
                    Dim win11NotepadAppxPackageNamePattern As String = $"Microsoft.WindowsNotepad_11.*_split.language-{ciTwoLetter}_*"

                    Dim detectedAppxPackages As List(Of String) = GetMatchingAppxPackagesFromDisk(win11NotepadAppxPackageNamePattern)

                    If detectedAppxPackages.Count > 0 Then
                        ConsoleHelper.WriteColoredLine(" WINDOWS 11 MODERN NOTEPAD ADVISE", ConsoleColor.Magenta)
                        ConsoleHelper.WriteColoredLine("================================================================================", ConsoleColor.DarkGray)
                        Console.WriteLine()
                        ConsoleHelper.WriteColoredLine(" Currently, this application does not support 'Englishizing' shortcut keys for the modern Windows 11 Notepad.", ConsoleColor.Yellow)
                        Console.WriteLine()
                        ConsoleHelper.WriteColoredLine(" You can run the following commands in an elevated PowerShell terminal to remove the", ConsoleColor.Yellow)
                        ConsoleHelper.WriteColoredLine(" localized modern Notepad packages and restore standarized English shortcut keys:", ConsoleColor.Yellow)
                        Console.WriteLine()
                        For Each currentAppxPackage As String In detectedAppxPackages
                            ConsoleHelper.WriteColoredLine($"   Remove-AppxPackage -Package ""{currentAppxPackage}""", ConsoleColor.Cyan)
                        Next currentAppxPackage
                        Console.WriteLine()
                        ConsoleHelper.WriteColoredLine(" [!] Please note that removing those packages will revert modern Notepad language to English.", ConsoleColor.Yellow)
                        Console.WriteLine()
                    End If
                End If
                ' ============================================

                ConsoleHelper.WriteColoredLine(" MUI FILES RETRIEVAL", ConsoleColor.Magenta)
                ConsoleHelper.WriteColoredLine("================================================================================", ConsoleColor.DarkGray)
                Console.WriteLine()

                ConsoleHelper.WriteColoredLine(" Locating supported directories and taking ownership...", ConsoleColor.White)
                Console.WriteLine()
                Dim resolvedDirectories As ReadOnlyCollection(Of String) = PathHelper.LocateDirectories(langConfig.Value)
                For i As Integer = 0 To resolvedDirectories.Count - 1
                    ConsoleHelper.WriteColoredLine($"   {i + 1:N0}: {resolvedDirectories(i).TrimStart({"\"c, "?"c})}", ConsoleColor.DarkCyan)
                Next i
                Console.WriteLine()
                ConsoleHelper.WriteColoredLine(" Completed locating directories.", ConsoleColor.Green)
                Console.WriteLine()

                ConsoleHelper.WriteColoredLine(" Locating supported MUI files and taking ownership...", ConsoleColor.White)
                Console.WriteLine()
                Dim resolvedMuiFiles As Dictionary(Of String, SortedSet(Of String)) = PathHelper.LocateMuiFiles(resolvedDirectories, langConfig.Value)
                For Each kvp As KeyValuePair(Of String, SortedSet(Of String)) In resolvedMuiFiles
                    ConsoleHelper.WriteColoredLine($"   Group {Path.GetFileName(kvp.Value.First())} [CRC-32: {kvp.Key}]:", ConsoleColor.Cyan)

                    Dim printIndex As Integer = 1
                    For Each matchedFilePath As String In kvp.Value
                        ConsoleHelper.WriteColoredLine($"     {printIndex:N0}: {matchedFilePath.TrimStart({"\"c, "?"c})}", ConsoleColor.DarkCyan)
                        printIndex += 1
                    Next matchedFilePath
                    Console.WriteLine()
                Next kvp
                ConsoleHelper.WriteColoredLine(" Completed locating MUI files.", ConsoleColor.Green)
                Console.WriteLine()

                If (resolvedMuiFiles IsNot Nothing) AndAlso (resolvedMuiFiles.Count > 0) Then
                    ConsoleHelper.WriteColoredLine(" MUI RESOURCE PROCESSING", ConsoleColor.Magenta)
                    ConsoleHelper.WriteColoredLine("================================================================================", ConsoleColor.DarkGray)
                    Console.WriteLine()

                    ConsoleHelper.WriteColoredLine(" Accepting Movefile EULA...", ConsoleColor.White)
                    Dim successAcceptMovefileEula As Boolean = RegistryHelper.AcceptMovefileEula()
                    If Not successAcceptMovefileEula Then
                        ConsoleHelper.ExitWithMessage(Nothing, exitCode:=4, Console.ForegroundColor)
                    End If
                    ConsoleHelper.WriteColoredLine(" Completed accepting Movefile EULA.", ConsoleColor.Green)
                    Console.WriteLine()

                    ConsoleHelper.WriteColoredLine($" Processing MUI file groups...", ConsoleColor.White)
                    Console.WriteLine()
                    For Each kvp As KeyValuePair(Of String, SortedSet(Of String)) In resolvedMuiFiles

                        Dim currentChecksum As String = kvp.Key
                        ConsoleHelper.WriteColoredLine($"   Group {Path.GetFileName(kvp.Value.First())} [CRC-32: {currentChecksum}]:", ConsoleColor.Cyan)
                        Console.WriteLine()

                        Dim matchingMuiDesc As MuiDescriptor = Nothing
                        Dim foundMuiDesc As Boolean = False

                        For Each muiDesc As MuiDescriptor In langConfig.Value.MuiDescriptors
                            If muiDesc.Checksum.Equals(currentChecksum, StringComparison.OrdinalIgnoreCase) Then
                                matchingMuiDesc = muiDesc
                                foundMuiDesc = True
                                Exit For
                            End If
                        Next muiDesc

                        If Not foundMuiDesc Then
                            ConsoleHelper.WriteColoredLine($"     [WARN] No MUI descriptor definitions found for checksum: {currentChecksum}. Skipping group.", ConsoleColor.Red)
                            Console.WriteLine()
                            ' Program.completedWithErrors = True
                            Continue For
                        End If

                        Dim muiFileName As String = matchingMuiDesc.FileName

                        Dim tempRcFilePath As String = Path.Combine(Path.GetTempPath(), $"{muiFileName}.rc")
                        Dim tempResFilePath As String = Path.Combine(Path.GetTempPath(), $"{muiFileName}.res")

                        Dim accTable As String = matchingMuiDesc.AcceleratorTable

                        ConsoleHelper.WriteColoredLine($"     Writing Accelerators table resource to: {tempRcFilePath}...", ConsoleColor.Gray)
                        If File.Exists(tempRcFilePath) Then
                            ' Silently try to delete any previous .rc file for safety.
                            Try
                                File.Delete(tempRcFilePath)
                            Catch
                            End Try
                        End If
                        Try
                            File.WriteAllText(tempRcFilePath, accTable, encoding:=Encoding.Unicode)
                        Catch ex As Exception
                            ConsoleHelper.WriteColoredLine($"     An error occurred: {ex.Message}", ConsoleColor.Red)
                            Console.WriteLine()
                            Program.completedWithErrors = True
                            Continue For
                        End Try
                        ConsoleHelper.WriteColoredLine("     Completed writing Accelerators table resource.", ConsoleColor.Green)
                        Console.WriteLine()

                        Dim compileArgs As String =
                            $"-log    ""{AppGlobals.RESOURCE_HACKER_LOGFILE_PATH}"" " &
                            $"-open   ""{tempRcFilePath}"" " &
                            $"-save   ""{tempResFilePath}"" " &
                             "-action compile"

                        ConsoleHelper.WriteColoredLine($"     Compiling Accelerators table resource to: {tempResFilePath}...", ConsoleColor.Gray)
                        If File.Exists(tempResFilePath) Then
                            ' Silently try to delete any previous .res file for safety.
                            Try
                                File.Delete(tempResFilePath)
                            Catch
                            End Try
                        End If
                        Dim successCompile As Boolean = ToolExecutors.ExecuteResourceHacker(compileArgs)
                        If Not successCompile Then
                            Console.WriteLine()
                            Program.completedWithErrors = True
                            Continue For
                        End If
                        ConsoleHelper.WriteColoredLine("     Completed compiling Accelerators table resource.", ConsoleColor.Green)
                        Console.WriteLine()

                        For i As Integer = 0 To kvp.Value.Count - 1

                            Dim sourceMuiFilePath As String = kvp.Value(i)
                            Dim sourceMuiDirPath As String = Path.GetDirectoryName(sourceMuiFilePath)
                            Dim sourceMuiRoot As String = Path.GetPathRoot(sourceMuiDirPath)
                            Dim sourceMuiDirPathWithoutRoot As String = sourceMuiDirPath.Substring(sourceMuiRoot.Length)

                            Dim tempMuiDirPath As String =
                                PathHelper.GetExtendedPath(Path.Combine(tempMuiDirectoryPath, sourceMuiDirPathWithoutRoot))

                            Dim tempMuiFilePathPending As String
                            Dim tempMuiFilePathFailed As String
#If DEBUG Then
                            tempMuiFilePathPending = Path.Combine(tempMuiDirPath, $"{muiFileName}.[{matchingMuiDesc.Checksum}].{AppGlobals.MuiFilePendingSuffix}")
                            tempMuiFilePathFailed = Path.Combine(tempMuiDirPath, $"{muiFileName}.[{matchingMuiDesc.Checksum}].{AppGlobals.MuiFileFailedSuffix}")
#Else
                            tempMuiFilePathPending = Path.Combine(tempMuiDirPath, $"{muiFileName}.{AppGlobals.MuiFilePendingSuffix}")
                            tempMuiFilePathFailed = Path.Combine(tempMuiDirPath, $"{muiFileName}.{AppGlobals.MuiFileFailedSuffix}")
#End If
                            ConsoleHelper.WriteColoredLine($"     {i + 1:N0}: {sourceMuiFilePath.TrimStart({"\"c, "?"c})}", ConsoleColor.DarkCyan)

                            ConsoleHelper.WriteColoredLine($"        Copying to temp file: {tempMuiFilePathPending.TrimStart({"\"c, "?"c})}...", ConsoleColor.Gray)

                            If Not Directory.Exists(tempMuiDirPath) Then
                                Try
                                    Directory.CreateDirectory(tempMuiDirPath)
                                Catch ex As Exception
                                    ConsoleHelper.WriteColoredLine($"        Cannot create target directory: {ex.Message}", ConsoleColor.Red)
                                    Console.WriteLine()
                                    Program.completedWithErrors = True
                                    Continue For
                                End Try
                            End If

                            Try
                                File.Copy(sourceMuiFilePath, tempMuiFilePathPending, overwrite:=True)
                            Catch ex As Exception
                                ConsoleHelper.WriteColoredLine($"        An error occurred: {ex.Message}", ConsoleColor.Red)
                                Console.WriteLine()
                                Program.completedWithErrors = True
                                Continue For
                            End Try

                            ConsoleHelper.WriteColoredLine($"        Clearing Read-Only attribute in temp file...", ConsoleColor.Gray)
                            Try
                                Dim attributes As FileAttributes = File.GetAttributes(tempMuiFilePathPending)
                                If (attributes And FileAttributes.ReadOnly) = FileAttributes.ReadOnly Then
                                    File.SetAttributes(tempMuiFilePathPending, attributes And Not FileAttributes.ReadOnly)
                                End If
                            Catch ex As Exception
                                ConsoleHelper.WriteColoredLine($"        An error occurred: {ex.Message}", ConsoleColor.Red)
                                Console.WriteLine()
                                Program.completedWithErrors = True
                                Continue For
                            End Try

                            ConsoleHelper.WriteColoredLine("        Overwriting Accelerators table in temp file...", ConsoleColor.Gray)

                            Dim overwriteArgs As String =
                                $"-log      ""{AppGlobals.RESOURCE_HACKER_LOGFILE_PATH}"" " &
                                $"-open     ""{tempMuiFilePathPending}"" " &
                                $"-save     ""{tempMuiFilePathPending}"" " &
                                $"-resource ""{tempResFilePath}"" " &
                                "-action    addoverwrite " &
                                $"-mask     ""ACCELERATORS,,{langConfig.Value.CultureInfo.LCID}"""

                            Dim successOverwrite As Boolean = ToolExecutors.ExecuteResourceHacker(overwriteArgs)
                            If Not successOverwrite Then
                                Console.WriteLine()
                                Program.completedWithErrors = True
                                Continue For
                            End If

                            ConsoleHelper.WriteColoredLine("        Scheduling replacement for source MUI file...", ConsoleColor.Gray)
                            Dim bakFilePath As String = $"{sourceMuiFilePath}.{AppGlobals.MuiFileBackupSuffix}"
                            If Not File.Exists(bakFilePath) Then
                                ' Note: If file "name.mui.bak" exists, the post-reboot operation will silently fail as expected.
                                ' We don't want to delete an original backup file and unnecessarily replace an already modified MUI file.
                                Dim createBakFileArgs As String = $" ""{sourceMuiFilePath}"" ""{bakFilePath}"" "
                                Dim successCreateBakFile As Boolean = ToolExecutors.ExecuteMoveFile(createBakFileArgs)
                                If Not successCreateBakFile Then
                                    Console.WriteLine()
                                    Program.completedWithErrors = True
                                    Continue For
                                End If
                            End If

                            Dim replaceMuiFileArgs As String = $" ""{tempMuiFilePathPending}"" ""{sourceMuiFilePath}"" "
                            Dim successMuiReplaceFile As Boolean = ToolExecutors.ExecuteMoveFile(replaceMuiFileArgs)
                            If Not successMuiReplaceFile Then
                                If Not File.Exists(bakFilePath) Then
                                    ' Best effort to revert .bak file rename.
                                    Dim revertBakFileArgs As String = $" ""{bakFilePath}"" ""{sourceMuiFilePath}"" "
                                    Dim successRevertBakFile As Boolean = ToolExecutors.ExecuteMoveFile(revertBakFileArgs)
                                    If Not successRevertBakFile Then
                                        ' Delete "PendingFileRenameOperations" value only if very critical MUI file:
                                        If muiFileName.Equals("shell32.dll.mui", StringComparison.OrdinalIgnoreCase) Then
                                            Const registryKeyPath As String = "SYSTEM\CurrentControlSet\Control\Session Manager"
                                            Const valueName As String = "PendingFileRenameOperations"
                                            Try
                                                Using sessionManagerKey As RegistryKey = Registry.LocalMachine.OpenSubKey(registryKeyPath, writable:=True)
                                                    ' Deletes the entire multi-string value to clear all scheduled operations and secure the next boot.
                                                    sessionManagerKey?.DeleteValue(valueName, throwOnMissingValue:=False)
                                                End Using

                                            Catch ' Ignore errors. Really, really best effort to revert .bak file rename.
                                            End Try
                                        End If
                                    End If
                                End If

                                Console.WriteLine()
                                Program.completedWithErrors = True
                                Continue For
                            End If

                            ' Delete any failed temp MUI file from previous reboots.
                            Dim deleteFailedTempMuiArgs As String = $" ""{tempMuiFilePathFailed}"" """" "
                            Dim successDeleteFailedTempMui As Boolean = ToolExecutors.ExecuteMoveFile(deleteFailedTempMuiArgs)
                            If Not successDeleteFailedTempMui Then
                                ' Ignore errors as this is not critical.
                            End If

                            ' If the temp MUI file still exists on directory, mark it as failed (rename it).
                            Dim markTempMuiAsFailedArgs As String = $" ""{tempMuiFilePathPending}"" ""{tempMuiFilePathFailed}"" "
                            Dim successMarkTempMuiAsFailed As Boolean = ToolExecutors.ExecuteMoveFile(markTempMuiAsFailedArgs)
                            If Not successMarkTempMuiAsFailed Then
                                ' Ignore errors as this is not critical.
                            End If

                            ConsoleHelper.WriteColoredLine($"        Completed processing MUI file.", ConsoleColor.Green)
                            Console.WriteLine()
                            Program.muiFilesProcessed += 1
                        Next i

                    Next kvp
                    ConsoleHelper.WriteColoredLine(" Completed processing MUI file groups.", ConsoleColor.Green)
                    Console.WriteLine()

                End If

            Catch ex As Exception
                Console.WriteLine()
                Dim errMsg As String = If(ex.InnerException IsNot Nothing, ex.InnerException.Message, ex.Message)
                ConsoleHelper.ExitWithMessage($"FATAL ERROR 0x{ex.HResult:X8}: {errMsg}", exitCode:=ex.HResult, ConsoleColor.Red)

            End Try

        Next langConfig

        ConsoleHelper.WriteColoredLine(" FINALIZATION", ConsoleColor.Magenta)
        ConsoleHelper.WriteColoredLine("================================================================================", ConsoleColor.DarkGray)
        Console.WriteLine()

        If Not Program.completedWithErrors AndAlso (Program.muiFilesProcessed = 0) Then
            ConsoleHelper.WriteColoredLine(" [!] No MUI files were processed because no matching files were found on the current system.", ConsoleColor.Yellow)
            Console.WriteLine()
            ConsoleHelper.WriteColoredLine(" The application will exit now without making any changes to your system.", Console.ForegroundColor)
            Console.WriteLine()

        ElseIf Not Program.completedWithErrors Then
            RegistryHelper.PrintScheduledFileOperations()
            ConsoleHelper.WriteColoredLine(" Operations completed successfully!", ConsoleColor.Green)
            Console.WriteLine()
            ConsoleHelper.WriteColoredLine(" All pending file operations have been scheduled for the next system restart.", ConsoleColor.Yellow)
            ConsoleHelper.WriteColoredLine(" [!] Please reboot your system to apply the changes.", ConsoleColor.Yellow)
            Console.WriteLine()
            RegistryHelper.CreateVolatileCheckPoint()

        Else
            RegistryHelper.PrintScheduledFileOperations()
            ConsoleHelper.WriteColoredLine(" Operations completed with errors.", ConsoleColor.DarkRed)
            Console.WriteLine()
            ConsoleHelper.WriteColoredLine(" [X] Some MUI resources may not have been updated or some file replacement operations may not have been scheduled. Please review any error messages above.", ConsoleColor.Red)
            Console.WriteLine()

        End If

        ConsoleHelper.ExitWithMessage(Nothing, exitCode:=0, Console.ForegroundColor)
    End Sub

#End Region

End Module

#End Region
