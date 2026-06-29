
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.IO
Imports System.Security

#End Region

#Region " AppX-Packages Helper "

''' <summary>
''' Provides methods for interacting with Appx packages installed on the system.
''' </summary>
<SecuritySafeCritical>
Friend Module AppxPackagesHelper

#Region " Static Methods "

    ''' <summary>
    ''' Efficiently searches the physical WindowsApps directory for installed packages matching a wildcard pattern.
    ''' Requires Administrator privileges to read the WindowsApps folder.
    ''' </summary>
    ''' <param name="pattern">The wildcard pattern to match (e.g., "PackageName_*").</param>
    ''' <returns>A list of matching PackageFullName strings.</returns>
    <DebuggerStepThrough>
    Friend Function GetMatchingAppxPackagesFromDisk(pattern As String) As List(Of String)

        Dim windowsAppsPath As String
        Dim matchedPackages As List(Of String)
        Dim directories() As String
        Dim dirPath As String
        Dim folderName As String
        Dim i As Integer

        matchedPackages = New List(Of String)()

        ' Physical path where all Appx packages are extracted
        windowsAppsPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)}\WindowsApps"

        Try
            directories = Directory.GetDirectories(windowsAppsPath, pattern, SearchOption.TopDirectoryOnly)

            For i = 0 To (directories.Length - 1)
                dirPath = directories(i)

                folderName = Path.GetFileName(dirPath)
                matchedPackages.Add(folderName)
            Next i
        Catch
            ' Ignore errors.

            'Catch ex As UnauthorizedAccessException
            '    Console.WriteLine($"[ERROR] Access denied to WindowsApps. Please run this application as Administrator.")

            'Catch ex As DirectoryNotFoundException
            '    Console.WriteLine($"[ERROR] WindowsApps directory not found. This might not be a standard Windows 10/11 installation.")

            'Catch ex As Exception
            '    Console.WriteLine($"[ERROR] An unexpected error occurred while scanning disk: {ex.Message}")

        End Try

        Return matchedPackages
    End Function

#End Region

End Module

#End Region
