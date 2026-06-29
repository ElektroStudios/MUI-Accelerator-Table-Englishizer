
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Diagnostics
Imports System.Security

Imports Microsoft.Win32

#End Region

#Region " Registry Helper "

''' <summary>
''' Provides methods for querying and interacting with the Windows Registry.
''' </summary>
<SecuritySafeCritical>
Friend Module RegistryHelper

#Region " Static Methods "

    ''' <summary>
    ''' Programmatically registers the Sysinternals <c>Movefile</c> EULA acceptance into the current user registry hive.
    ''' </summary>
    ''' 
    ''' <returns>
    ''' <see langword="True"/> if the value was successfully written; otherwise, <see langword="False"/>.
    ''' </returns>
    <DebuggerStepThrough>
    Friend Function AcceptMovefileEula() As Boolean

        Const subKeyPath As String = "SOFTWARE\Sysinternals\Movefile"
        Const valueName As String = "EulaAccepted"
        Const dwordValue As Integer = 1

        Try
            Using sysinternalsKey As RegistryKey = Registry.CurrentUser.CreateSubKey(subKeyPath, writable:=True)
                If sysinternalsKey IsNot Nothing Then
                    sysinternalsKey.SetValue(valueName, dwordValue, RegistryValueKind.DWord)
                Else
                    Console.WriteLine($" Failed to create or open the registry subkey: {Registry.CurrentUser}\{subKeyPath}")
                End If
            End Using
            Return True

        Catch ex As Exception
            Console.WriteLine($" An error occurred while writing to registry key: {ex.Message}")

        End Try

        Return False
    End Function

    ''' <summary>
    ''' Reads the Windows registry to retrieve and print pending file rename or deletion operations scheduled for the next system reboot.
    ''' <para></para>
    ''' Only operations involving MUI files ('.mui' extension) are displayed.
    ''' </summary>
    <DebuggerStepThrough>
    Friend Sub PrintScheduledFileOperations()

        Const subKeyPath As String = "SYSTEM\CurrentControlSet\Control\Session Manager"
        Const valueName As String = "PendingFileRenameOperations"

        Try
            Using sessionManagerKey As RegistryKey = Registry.LocalMachine.OpenSubKey(subKeyPath, writable:=False)
                If sessionManagerKey IsNot Nothing Then
                    Dim pendingOperations As Object = sessionManagerKey.GetValue(valueName)

                    If pendingOperations IsNot Nothing AndAlso TypeOf pendingOperations Is String() Then
                        Dim operationsArray As String() = DirectCast(pendingOperations, String())

                        If operationsArray.Length > 0 Then
                            ConsoleHelper.WriteColoredLine(" Scheduled File Operations on Reboot:", ConsoleColor.Cyan)
                            Console.WriteLine()
                            Dim index As Integer = 0
                            ' Loop increments by 2 because operations are stored in Source/Destination pairs
                            Do While index < operationsArray.Length
                                Dim sourceFile As String = operationsArray(index)
                                Dim destinationFile As String = String.Empty

                                If (index + 1) < operationsArray.Length Then
                                    destinationFile = operationsArray(index + 1)
                                End If

                                If Not String.IsNullOrWhiteSpace(sourceFile) AndAlso sourceFile.IndexOf(".mui") > 0 Then
                                    ' Clean up the Win32 native path prefix "\??\" for clean console output

                                    Dim cleanSource As String = sourceFile.Replace("\??\", "").Replace("*1", "").Replace("*2", "")

                                    Dim cleanDestination As String =
                                        destinationFile.Replace("\??\", "").Replace("*1", "").Replace("*2", "")

                                    If String.IsNullOrWhiteSpace(cleanDestination) Then
                                        Console.WriteLine($"    [-] DELETE  : {cleanSource}")
                                    Else
                                        Console.WriteLine($"    [*] MOVE    : {cleanSource}")
                                        Console.WriteLine($"        TO      : {cleanDestination}")
                                    End If
                                End If

                                If destinationFile.EndsWith($".{AppGlobals.MuiFileFailedSuffix}", StringComparison.OrdinalIgnoreCase) Then
                                    Console.WriteLine()
                                End If

                                index += 2
                            Loop
                            '  Console.WriteLine()
                        End If
                    End If
                End If
            End Using
        Catch ex As Exception
            ConsoleHelper.WriteColoredLine($" [X] Could not read pending operations from registry: {ex.Message}", ConsoleColor.Red)
            Console.WriteLine()

        End Try
    End Sub

    ''' <summary>
    ''' Checks if the volatile registry checkpoint exists, indicating that tasks were already completed.
    ''' </summary>
    ''' <returns>True if the checkpoint value exists; otherwise, False.</returns>
    Friend Function CheckIfCheckPointExists() As Boolean
        Dim exists As Boolean = False

        Try
            Using hkcu As RegistryKey =
                Registry.CurrentUser.OpenSubKey(AppGlobals.RegVolatileSubKeyPath, writable:=False)

                If hkcu IsNot Nothing Then
                    Dim regValue As Object = hkcu.GetValue(AppGlobals.RegVolatileValueName, defaultValue:=Nothing)
                    If regValue IsNot Nothing Then
                        exists = True
                    End If
                End If
            End Using

        Catch ex As Exception
            Debug.WriteLine($"Failed to read volatile registry key: {ex.Message}")

        End Try

        Return exists
    End Function

    ''' <summary>
    ''' Creates a volatile registry key and value to mark the completion of the application tasks.
    ''' This volatile key will automatically disappear upon the next system reboot.
    ''' </summary>
    Friend Sub CreateVolatileCheckPoint()
        Try
            Using hkcu As RegistryKey =
                Registry.CurrentUser.CreateSubKey(AppGlobals.RegVolatileSubKeyPath, writable:=True, RegistryOptions.Volatile)

                hkcu?.SetValue(AppGlobals.RegVolatileValueName, 1, RegistryValueKind.DWord)
            End Using

        Catch ex As Exception
            ConsoleHelper.WriteColoredLine($" [WARN] Could not create volatile registry checkpoint: {ex.Message}", ConsoleColor.Yellow)

        End Try
    End Sub

#End Region

End Module

#End Region
