
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Diagnostics
Imports System.Security

#End Region

#Region " UserPermission Helper "

''' <summary>
''' Provides methods for managing directory and file security permissions and ownership.
''' </summary>
<SecurityCritical>
Friend Module UserPermissionHelper

#Region " Static Methods "

    ''' <summary>
    ''' Attempts to take ownership to the current user and grant full access for a specific directory path, recursively.
    ''' </summary>
    ''' 
    ''' <param name="dirPath">
    ''' The directory path for which to take ownership and grant access permissions.
    ''' </param>
    <DebuggerStepThrough>
    Friend Sub ClaimDirectoryAccess(dirPath As String)

        dirPath = dirPath.TrimStart({"\"c, "?"c})

        Try
            ' Take directory ownership to the current user.
            Dim takeOwnCommandArgs As String = $"/F ""{dirPath}"""
            Using takeownProcess As Process =
                Process.Start(New ProcessStartInfo("TAKEOWN.exe", takeOwnCommandArgs) With {
                .UseShellExecute = True,
                .CreateNoWindow = True,
                .Verb = "runas",
                .WindowStyle = ProcessWindowStyle.Hidden
            })
                takeownProcess.WaitForExit()
                Dim exitCode As Integer = takeownProcess.ExitCode
            End Using
#If DEBUG Then
            ConsoleHelper.WriteColoredLine($"   TAKEOWN.exe {takeOwnCommandArgs}", ConsoleColor.DarkGray)
#End If

            ' Grant current user full Control to directory.
            Dim icaclsUserCommandArgs As String = $"    ""{dirPath}"" /Grant ""{Environment.UserName}:(F)"""
            Using icaclsUserProcess As Process =
                Process.Start(New ProcessStartInfo("ICACLS.exe", icaclsUserCommandArgs) With {
                .UseShellExecute = True,
                .CreateNoWindow = True,
                .Verb = "runas",
                .WindowStyle = ProcessWindowStyle.Hidden
            })
                icaclsUserProcess.WaitForExit()
                Dim exitCode As Integer = icaclsUserProcess.ExitCode
            End Using
#If DEBUG Then
            ConsoleHelper.WriteColoredLine($"   ICACLS.exe {icaclsUserCommandArgs}", ConsoleColor.DarkGray)
#End If

            ' Grant SYSTEM full Control to directory.
            Dim icaclsSystemCommandArgs As String = $"    ""{dirPath}"" /Grant ""SYSTEM:(F)"""
            Using icaclsSystemProcess As Process =
                Process.Start(New ProcessStartInfo("ICACLS.exe", icaclsSystemCommandArgs) With {
                .UseShellExecute = True,
                .CreateNoWindow = True,
                .Verb = "runas",
                .WindowStyle = ProcessWindowStyle.Hidden
            })
                icaclsSystemProcess.WaitForExit()
                Dim exitCode As Integer = icaclsSystemProcess.ExitCode
            End Using
#If DEBUG Then
            ConsoleHelper.WriteColoredLine($"   ICACLS.exe {icaclsSystemCommandArgs}", ConsoleColor.DarkGray)
#End If

#If DEBUG Then
            Console.WriteLine()
#End If
        Catch
            ' Silently handle exceptions; movefile will simply fail later if permissions were not successfully granted.

        End Try
    End Sub

    ''' <summary>
    ''' Attempts to take ownership to the current user and grant full access for a specific file.
    ''' </summary>
    ''' 
    ''' <param name="filePath">
    ''' The file path for which to take ownership and grant access permissions.
    ''' </param>
    <DebuggerStepThrough>
    Friend Sub ClaimFileAccess(filePath As String)

        filePath = filePath.TrimStart({"\"c, "?"c})

        Try
            ' Take file ownership to the current user.
            Dim takeOwnCommandArgs As String = $"/F ""{filePath}"""
            Using takeownProcess As Process =
                Process.Start(New ProcessStartInfo("TAKEOWN.exe", takeOwnCommandArgs) With {
                .UseShellExecute = True,
                .CreateNoWindow = True,
                .Verb = "runas",
                .WindowStyle = ProcessWindowStyle.Hidden
            })
                takeownProcess.WaitForExit()
                Dim exitCode As Integer = takeownProcess.ExitCode
            End Using
#If DEBUG Then
            ConsoleHelper.WriteColoredLine($"   TAKEOWN.exe {takeOwnCommandArgs}", ConsoleColor.DarkGray)
#End If

            ' Grant current user full Control to file.
            Dim icaclsUserCommandArgs As String = $"    ""{filePath}"" /Grant ""{Environment.UserName}:(F)"""
            Using icaclsUserProcess As Process =
                Process.Start(New ProcessStartInfo("ICACLS.exe", icaclsUserCommandArgs) With {
                .UseShellExecute = True,
                .CreateNoWindow = True,
                .Verb = "runas",
                .WindowStyle = ProcessWindowStyle.Hidden
            })
                icaclsUserProcess.WaitForExit()
                Dim exitCode As Integer = icaclsUserProcess.ExitCode
            End Using
#If DEBUG Then
            ConsoleHelper.WriteColoredLine($"   ICACLS.exe {icaclsUserCommandArgs}", ConsoleColor.DarkGray)
#End If

            ' Grant SYSTEM full Control to file.
            Dim icaclsSystemCommandArgs As String = $"    ""{filePath}"" /Grant ""SYSTEM:(F)"""
            Using icaclsSystemProcess As Process =
                Process.Start(New ProcessStartInfo("ICACLS.exe", icaclsSystemCommandArgs) With {
                .UseShellExecute = True,
                .CreateNoWindow = True,
                .Verb = "runas",
                .WindowStyle = ProcessWindowStyle.Hidden
            })
                icaclsSystemProcess.WaitForExit()
                Dim exitCode As Integer = icaclsSystemProcess.ExitCode
            End Using
#If DEBUG Then
            ConsoleHelper.WriteColoredLine($"   ICACLS.exe {icaclsSystemCommandArgs}", ConsoleColor.DarkGray)
#End If

#If DEBUG Then
            Console.WriteLine()
#End If
        Catch
            ' Silently handle exceptions; movefile will simply fail later if permissions were not successfully granted.

        End Try
    End Sub

#End Region

End Module

#End Region
