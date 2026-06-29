
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.Diagnostics
Imports System.IO
Imports System.Linq
Imports System.Security

Imports DevCase.Core.Security.DataIntegrity.Checksum

#End Region

#Region " Path Helper "

''' <summary>
''' Provides methods for searching and validating file system and directory paths.
''' </summary>
<SecuritySafeCritical>
Friend Module PathHelper

#Region " Static Methods "

    ''' <summary>
    ''' Resolves and locates the target directory paths to search for MUI files based on defined search patterns.
    ''' </summary>
    ''' 
    ''' <param name="langConfig">
    ''' The language configuration environment containing the localized character used to confirm ownership via the TAKEOWN command line tool. 
    ''' </param>
    ''' 
    ''' <returns>
    ''' A <see cref="ReadOnlyCollection(Of String)"/> containing the paths of all successfully resolved directories.
    ''' </returns>
    <DebuggerStepThrough>
    Friend Function LocateDirectories(langConfig As LanguageConfiguration) As ReadOnlyCollection(Of String)

        Dim searchPathPatterns As SortedSet(Of String) = PathHelper.BuildSearchPathPatterns(langConfig)

        Dim resolvedDirectories As New SortedSet(Of String)(StringComparer.OrdinalIgnoreCase)

        For Each item As String In searchPathPatterns
            Dim directoryName As String = Path.GetDirectoryName(item)
            Dim searchPattern As String = Path.GetFileName(item)

            If Directory.Exists(directoryName) Then
                ' UserPermissionHelper.ClaimDirectoryAccess(directoryName, langConfig)

                If searchPattern.Contains("*") Then
                    Try
                        For Each matchedDir As String In Directory.EnumerateDirectories(directoryName, searchPattern, SearchOption.TopDirectoryOnly)
                            UserPermissionHelper.ClaimDirectoryAccess(matchedDir)
                            resolvedDirectories.Add(matchedDir)
                        Next matchedDir

                    Catch ex As UnauthorizedAccessException
                        ConsoleHelper.WriteColoredLine($"Read access denied for directory '{directoryName}'. Unlocking...", ConsoleColor.Yellow)
                        UserPermissionHelper.ClaimDirectoryAccess(directoryName)

                        Try
                            For Each matchedDir As String In Directory.EnumerateDirectories(directoryName, searchPattern, SearchOption.TopDirectoryOnly)
                                resolvedDirectories.Add(matchedDir)
                            Next matchedDir

                        Catch subEx As UnauthorizedAccessException
                            ConsoleHelper.WriteColoredLine($"Unable to grant permissions to directory. Skipping...", ConsoleColor.Red)
                            Program.completedWithErrors = True

                        End Try

                    Catch ex As Exception

                    End Try
                Else
                    Dim fullPath As String = Path.Combine(directoryName, searchPattern)
                    If Directory.Exists(fullPath) Then
                        UserPermissionHelper.ClaimDirectoryAccess(fullPath)
                        resolvedDirectories.Add(fullPath)
                    End If

                End If
            End If
        Next item

        Return resolvedDirectories.ToList().AsReadOnly()
    End Function

    ''' <summary>
    ''' Locates all valid MUI files within the specified directories and groups them by their file checksum (CRC-32).
    ''' </summary>
    ''' 
    ''' <param name="resolvedDirectories">
    ''' The list of directory paths to search for valid MUI files.
    ''' </param>
    ''' 
    ''' <param name="langConfig">
    ''' The language configuration environment containing targeted MUI file definitions 
    ''' and the localized character used to confirm ownership via the TAKEOWN command line tool. 
    ''' </param>
    ''' 
    ''' <returns>
    ''' A <see cref="Dictionary(Of String, HashSet(Of String))"/> where each key is a file checksum 
    ''' and the value is the collection of matching MUI file paths.
    ''' </returns>
    <DebuggerStepThrough>
    Friend Function LocateMuiFiles(resolvedDirectories As IList(Of String),
                                   langConfig As LanguageConfiguration) As Dictionary(Of String, SortedSet(Of String))

        Dim groupedMuiFiles As New Dictionary(Of String, SortedSet(Of String))(StringComparer.OrdinalIgnoreCase)

        For Each muiDesc As MuiDescriptor In langConfig.MuiDescriptors

            Dim fileName As String = muiDesc.FileName
            Dim expectedChecksum As String = muiDesc.Checksum
            Dim currentMatches As New SortedSet(Of String)(StringComparer.OrdinalIgnoreCase)

            For Each targetDir As String In resolvedDirectories
                Dim matches As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

                PathHelper.FindFiles(targetDir, fileName, matches)

                For Each matchedFile As String In matches
                    Dim fileChecksum As String = UtilChecksum.ComputeCRC32OfFile(matchedFile)

                    If fileChecksum.Equals(expectedChecksum, StringComparison.OrdinalIgnoreCase) Then
                        UserPermissionHelper.ClaimFileAccess(matchedFile)
                        currentMatches.Add(matchedFile)
                    End If
                Next matchedFile
            Next targetDir

            If currentMatches.Count > 0 Then
                If Not groupedMuiFiles.ContainsKey(expectedChecksum) Then
                    groupedMuiFiles.Add(expectedChecksum, currentMatches)
                Else
                    For Each verifiedPath As String In currentMatches
                        groupedMuiFiles(expectedChecksum).Add(verifiedPath)
                    Next verifiedPath
                End If
            End If
        Next muiDesc

        Return groupedMuiFiles
    End Function

#End Region

#Region " Private Methods "

    ''' <summary>
    ''' Recursively searches a directory tree for files matching a specific file name or search pattern.
    ''' </summary>
    ''' 
    ''' <param name="rootPath">
    ''' The root directory path to begin searching.
    ''' </param>
    ''' 
    ''' <param name="searchPattern">
    ''' The file name or pattern to match.
    ''' </param>
    ''' 
    ''' <param name="refMatches">
    ''' When this method returns, contains the discovered file paths that matches the <paramref name="searchPattern"/>.
    ''' </param>
    <DebuggerStepThrough>
    Private Sub FindFiles(rootPath As String, searchPattern As String,
                    ByRef refMatches As HashSet(Of String))

        If refMatches Is Nothing Then
            refMatches = New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        End If

        Dim directoryStack As New Stack(Of String)()
        directoryStack.Push(rootPath)

        While directoryStack.Count > 0
            Dim currentDir As String = directoryStack.Pop()

            Try
                For Each fileMatch As String In Directory.EnumerateFiles(currentDir, searchPattern, SearchOption.TopDirectoryOnly)
                    refMatches.Add(fileMatch)
                Next fileMatch

                For Each subDir As String In Directory.EnumerateDirectories(currentDir)
                    directoryStack.Push(subDir)
                Next subDir

            Catch ex As UnauthorizedAccessException
                ConsoleHelper.WriteColoredLine($"Read access denied for directory '{currentDir}'. Unlocking...", ConsoleColor.Yellow)
                UserPermissionHelper.ClaimDirectoryAccess(currentDir)

                Try
                    For Each fileMatch As String In Directory.EnumerateFiles(currentDir, searchPattern, SearchOption.TopDirectoryOnly)
                        refMatches.Add(fileMatch)
                    Next fileMatch

                    For Each subDir As String In Directory.EnumerateDirectories(currentDir)
                        directoryStack.Push(subDir)
                    Next subDir

                Catch subEx As UnauthorizedAccessException
                    ConsoleHelper.WriteColoredLine($"Unable to grant permissions to directory. Skipping...", ConsoleColor.Red)
                    Program.completedWithErrors = True

                End Try

            Catch ex As Exception

            End Try
        End While
    End Sub

    ''' <summary>
    ''' Converts a standard path into an Extended-Length Path (prefixed with \\?\) to bypass the 260 character MAX_PATH limitation.
    ''' </summary>
    ''' 
    ''' <param name="targetPath">
    ''' The absolute path to convert.
    ''' </param>
    ''' 
    ''' <returns>
    ''' The extended-length path string.
    ''' </returns>
    <DebuggerStepThrough>
    Friend Function GetExtendedPath(targetPath As String) As String

        If String.IsNullOrWhiteSpace(targetPath) Then
            Return targetPath
        End If

        ' Already an extended path.
        If targetPath.StartsWith("\\?\", StringComparison.Ordinal) Then
            Return targetPath
        End If

        ' Relative paths cannot be converted to Extended-Length paths.
        If Not Path.IsPathRooted(targetPath) Then
            Return targetPath
        End If

        ' Handle UNC paths: \\Server\Share -> \\?\UNC\Server\Share
        If targetPath.StartsWith("\\", StringComparison.Ordinal) Then
            Return $"\\?\UNC\{targetPath.Substring(2)}"
        End If

        ' Handle Local paths: C:\Folder -> \\?\C:\Folder
        Return $"\\?\{targetPath}"
    End Function

    ''' <summary>
    ''' Builds the collection of search path patterns where to find MUI files based on the specified language configuration.
    ''' </summary>
    ''' 
    ''' <param name="langConfig">
    ''' The language configuration environment containing the culture info. 
    ''' </param>
    ''' 
    ''' <returns>
    ''' A <see cref="SortedSet(Of String)"/> containing the paths of all successfully resolved directories.
    ''' </returns>
    <DebuggerStepThrough>
    Private Function BuildSearchPathPatterns(langConfig As LanguageConfiguration) As SortedSet(Of String)

        Dim ciName As String = langConfig.CultureInfo.Name

#Disable Warning IDE0028 ' Simplify collection initialization
        Dim searchPathPatterns As New SortedSet(Of String)(StringComparer.OrdinalIgnoreCase)
#Enable Warning IDE0028 ' Simplify collection initialization

        ' 1. C:\Windows\xx-XX
        searchPathPatterns.Add(
            PathHelper.GetExtendedPath(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), ciName)))

        ' 2. C:\Windows\System32\xx-XX
        searchPathPatterns.Add(
            PathHelper.GetExtendedPath(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), ciName)))

        ' 3. C:\Windows\SysWOW64\xx-XX
        searchPathPatterns.Add(
            PathHelper.GetExtendedPath(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.SystemX86), ciName)))

        ' 4. C:\Program Files\WindowsApps\Microsoft.LanguageExperiencePackxx-XX_<Version>_<Architecture>_<ResourceId>_<PublisherId>\Windows\System32\xx-XX
        searchPathPatterns.Add(
            PathHelper.GetExtendedPath(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
                             "WindowsApps", $"Microsoft.LanguageExperiencePack{ciName}_*")))

        ' 5. C:\Program Files (x86)\WindowsApps\Microsoft.LanguageExperiencePackxx-XX_<Version>_<Architecture>_<ResourceId>_<PublisherId>\Windows\System32\xx-XX
        searchPathPatterns.Add(
            PathHelper.GetExtendedPath(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                             "WindowsApps", $"Microsoft.LanguageExperiencePack{ciName}_*")))

        ' 6. C:\Program Files\Windows NT\Accessories\xx-XX
        searchPathPatterns.Add(
            PathHelper.GetExtendedPath(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), $"Windows NT\Accessories\{ciName}")))

        ' 7. C:\Program Files (x86)\Windows NT\Accessories\xx-XX
        searchPathPatterns.Add(
            PathHelper.GetExtendedPath(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), $"Windows NT\Accessories\{ciName}")))

        ' 8. C:\Program Files\Wordpad\xx-XX (https://winaero.com/wordpad-for-windows-11/)
        searchPathPatterns.Add(
            PathHelper.GetExtendedPath(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), $"Wordpad\{ciName}")))

        Return searchPathPatterns
    End Function

#End Region

End Module

#End Region
