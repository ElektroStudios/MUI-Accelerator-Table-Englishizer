
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Diagnostics
Imports System.IO
Imports System.Security
Imports System.Text

#End Region

#Region " Tool Executors "

''' <summary>
''' Provides methods for executing external command-line tools and managing their processes.
''' </summary>
<SecurityCritical>
Friend Module ToolExecutors

#Region " Static Methods "

    ''' <summary>
    ''' Executes Resource Hacker with the specified command-line arguments and validates the operation via the log file.
    ''' </summary>
    ''' 
    ''' <param name="arguments">
    ''' The command-line arguments to pass to Resource Hacker.
    ''' </param>
    ''' 
    ''' <returns>
    ''' <see langword="True"/> if the operation was successful according to the log; otherwise, <see langword="False"/>.
    ''' </returns>
    <DebuggerStepThrough>
    Friend Function ExecuteResourceHacker(arguments As String) As Boolean

        If File.Exists(AppGlobals.RESOURCE_HACKER_LOGFILE_PATH) Then
            Try
                File.Delete(AppGlobals.RESOURCE_HACKER_LOGFILE_PATH)
            Catch
            End Try
        End If

        Try
            Dim reshackerProcessInfo As New ProcessStartInfo(AppGlobals.RESOURCE_HACKER_EXEC_PATH, arguments) With {
                .UseShellExecute = False,
                .CreateNoWindow = True
            }

            Using reshackerProcess As Process = Process.Start(reshackerProcessInfo)
                reshackerProcess.WaitForExit()
            End Using

        Catch ex As Exception
            ConsoleHelper.WriteColoredLine($"        Error executing Resource Hacker: {ex.Message}", ConsoleColor.Red)
            Return False

        End Try

        If File.Exists(AppGlobals.RESOURCE_HACKER_LOGFILE_PATH) Then
            Try
                Dim logContent As String = File.ReadAllText(AppGlobals.RESOURCE_HACKER_LOGFILE_PATH, Encoding.Unicode)

                If logContent.IndexOf("Success!", StringComparison.OrdinalIgnoreCase) >= 0 Then
                    Return True
                Else
                    ConsoleHelper.WriteColoredLine("        Resource Hacker execution has failed. Log file content:", ConsoleColor.Red)
                    Console.WriteLine()
                    ConsoleHelper.WriteColoredLine(logContent, ConsoleColor.DarkRed)
                    Return False
                End If

            Catch ex As Exception
                ConsoleHelper.WriteColoredLine($"        Error reading Resource Hacker log file: {ex.Message}", ConsoleColor.Red)
                Return False

            Finally
                Try
                    File.Delete(AppGlobals.RESOURCE_HACKER_LOGFILE_PATH)
                Catch
                End Try
            End Try
        Else
            ConsoleHelper.WriteColoredLine($"        Resource Hacker execution has failed.", ConsoleColor.Red)
            Console.WriteLine()
            ConsoleHelper.WriteColoredLine($"        Full command: ""{AppGlobals.RESOURCE_HACKER_EXEC_PATH}"" {arguments}", ConsoleColor.DarkRed)
            Return False

        End If
    End Function

    ''' <summary>
    ''' Executes the Sysinternals <c>MoveFile</c> with the specified command-line arguments and validates the execution operation.
    ''' </summary>
    ''' 
    ''' <param name="arguments">
    ''' The target arguments containing file operations.
    ''' </param>
    ''' 
    ''' <returns>
    ''' <see langword="True"/> if <c>MoveFile</c> process exited with zero; otherwise, <see langword="False"/>.
    ''' </returns>
    <DebuggerStepThrough>
    Friend Function ExecuteMoveFile(arguments As String) As Boolean

        Try
            Dim movefileProcessInfo As New ProcessStartInfo(AppGlobals.MOVEFILE_EXEC_PATH, arguments) With {
                .UseShellExecute = False,
                .CreateNoWindow = True
            }

            Using movefileProcess As Process = Process.Start(movefileProcessInfo)
                movefileProcess.WaitForExit()

                If movefileProcess.ExitCode <> 0 Then
                    ConsoleHelper.WriteColoredLine($"        Error executing MoveFile. ExitCode: {movefileProcess.ExitCode}", ConsoleColor.Red)
                    Console.WriteLine()
                    ConsoleHelper.WriteColoredLine($"        Full command: ""{AppGlobals.MOVEFILE_EXEC_PATH}"" {arguments}", ConsoleColor.DarkRed)
                    Return False
                End If
            End Using

            Return True

        Catch ex As Exception
            ConsoleHelper.WriteColoredLine($"        Error executing MoveFile: {ex.Message}", ConsoleColor.Red)
            Return False

        End Try
    End Function

#End Region

End Module

#End Region
