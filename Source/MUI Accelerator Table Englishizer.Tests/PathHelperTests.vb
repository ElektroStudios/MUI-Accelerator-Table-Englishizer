#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Linq

Imports MATE.Tests.Helpers

#End Region

#Region " PathHelperTests "

Namespace MATE.Tests

    ''' <summary>
    ''' Unit tests for the side-effect-free members of the <see cref="PathHelper"/> module.
    ''' </summary>
    '''
    ''' <remarks>
    ''' <see cref="PathHelper.LocateDirectories(LanguageConfiguration)"/> and
    ''' <see cref="PathHelper.LocateMuiFiles(IList(Of String), LanguageConfiguration)"/> are intentionally not tested,
    ''' because they take ownership of system directories and files by running elevated <c>TAKEOWN</c> and <c>ICACLS</c> processes.
    ''' </remarks>
    <TestClass>
    Public NotInheritable Class PathHelperTests

#Region " Constants "

        ''' <summary>
        ''' The prefix that identifies a Win32 Extended-Length path.
        ''' </summary>
        Private Const ExtendedPathPrefix As String = "\\?\"

#End Region

#Region " GetExtendedPath "

        ''' <summary>
        ''' Verifies that local absolute paths are prefixed with <c>\\?\</c>.
        ''' </summary>
        '''
        ''' <param name="targetPath">
        ''' The path to convert.
        ''' </param>
        '''
        ''' <param name="expected">
        ''' The expected result.
        ''' </param>
        <TestMethod>
        <DataRow("C:\Windows\es-ES", "\\?\C:\Windows\es-ES")>
        <DataRow("C:\", "\\?\C:\")>
        <DataRow("D:\Folder\file.mui", "\\?\D:\Folder\file.mui")>
        Public Sub GetExtendedPath_LocalAbsolutePath_AddsExtendedPrefix(targetPath As String, expected As String)

            Dim actual As String = PathHelper.GetExtendedPath(targetPath)

            Assert.AreEqual(expected, actual)
        End Sub

        ''' <summary>
        ''' Verifies that UNC paths are converted to the <c>\\?\UNC\</c> form.
        ''' </summary>
        '''
        ''' <param name="targetPath">
        ''' The path to convert.
        ''' </param>
        '''
        ''' <param name="expected">
        ''' The expected result.
        ''' </param>
        <TestMethod>
        <DataRow("\\Server\Share", "\\?\UNC\Server\Share")>
        <DataRow("\\Server\Share\Dir\file.mui", "\\?\UNC\Server\Share\Dir\file.mui")>
        Public Sub GetExtendedPath_UncPath_AddsExtendedUncPrefix(targetPath As String, expected As String)

            Dim actual As String = PathHelper.GetExtendedPath(targetPath)

            Assert.AreEqual(expected, actual)
        End Sub

        ''' <summary>
        ''' Verifies that paths that are already in Extended-Length form are returned unchanged.
        ''' </summary>
        '''
        ''' <param name="targetPath">
        ''' The path to convert.
        ''' </param>
        <TestMethod>
        <DataRow("\\?\C:\Windows")>
        <DataRow("\\?\UNC\Server\Share")>
        Public Sub GetExtendedPath_AlreadyExtendedPath_ReturnsUnchanged(targetPath As String)

            Dim actual As String = PathHelper.GetExtendedPath(targetPath)

            Assert.AreEqual(targetPath, actual)
        End Sub

        ''' <summary>
        ''' Verifies that the conversion is idempotent.
        ''' </summary>
        '''
        ''' <param name="targetPath">
        ''' The path to convert.
        ''' </param>
        <TestMethod>
        <DataRow("C:\Windows\System32\es-ES")>
        <DataRow("\\Server\Share\Dir")>
        Public Sub GetExtendedPath_AppliedTwice_ReturnsSameResult(targetPath As String)

            Dim once As String = PathHelper.GetExtendedPath(targetPath)
            Dim twice As String = PathHelper.GetExtendedPath(once)

            Assert.AreEqual(once, twice)
        End Sub

        ''' <summary>
        ''' Verifies that relative paths are returned unchanged, because they cannot be converted to Extended-Length paths.
        ''' </summary>
        '''
        ''' <param name="targetPath">
        ''' The path to convert.
        ''' </param>
        <TestMethod>
        <DataRow("Windows\es-ES")>
        <DataRow("file.mui")>
        <DataRow("..\file.mui")>
        Public Sub GetExtendedPath_RelativePath_ReturnsUnchanged(targetPath As String)

            Dim actual As String = PathHelper.GetExtendedPath(targetPath)

            Assert.AreEqual(targetPath, actual)
        End Sub

        ''' <summary>
        ''' Verifies that null, empty or white-space paths are returned unchanged.
        ''' </summary>
        '''
        ''' <param name="targetPath">
        ''' The path to convert.
        ''' </param>
        <TestMethod>
        <DataRow(Nothing, DisplayName:="Null")>
        <DataRow("", DisplayName:="Empty")>
        <DataRow("   ", DisplayName:="White spaces")>
        Public Sub GetExtendedPath_NullOrWhiteSpacePath_ReturnsUnchanged(targetPath As String)

            Dim actual As String = PathHelper.GetExtendedPath(targetPath)

            Assert.AreEqual(targetPath, actual)
        End Sub

        ''' <summary>
        ''' Verifies that an Extended-Length path built for a real directory can be used to access it.
        ''' </summary>
        <TestMethod>
        Public Sub GetExtendedPath_ExistingDirectory_ResultIsAccessible()

            Using tempDir As New TempDirectory()
                Dim filePath As String = tempDir.CreateFile("file.mui", New Byte() {1, 2, 3})

                Dim extendedDirectoryPath As String = PathHelper.GetExtendedPath(tempDir.FullPath)
                Dim extendedFilePath As String = PathHelper.GetExtendedPath(filePath)

                Assert.IsTrue(Directory.Exists(extendedDirectoryPath))
                Assert.IsTrue(File.Exists(extendedFilePath))
            End Using
        End Sub

#End Region

#Region " BuildSearchPathPatterns "

        ''' <summary>
        ''' Verifies that a null language configuration throws <see cref="ArgumentNullException"/>.
        ''' </summary>
        <TestMethod>
        Public Sub BuildSearchPathPatterns_NullConfiguration_ThrowsArgumentNullException()

            Dim ex As ArgumentNullException = Assert.ThrowsExactly(Of ArgumentNullException)(
                Sub() PathHelper.BuildSearchPathPatterns(Nothing))

            Assert.AreEqual("langConfig", ex.ParamName)
        End Sub

        ''' <summary>
        ''' Verifies that every search path pattern is an Extended-Length path.
        ''' </summary>
        <TestMethod>
        Public Sub BuildSearchPathPatterns_EsEs_AllPatternsAreExtendedPaths()

            Dim patterns As SortedSet(Of String) = PathHelper.BuildSearchPathPatterns(New LanguageConfiguration_esES())

            Assert.IsTrue(patterns.Count > 0)
            For Each pattern As String In patterns
                Assert.IsTrue(pattern.StartsWith(ExtendedPathPrefix, StringComparison.Ordinal), $"Not an extended path: {pattern}")
            Next pattern
        End Sub

        ''' <summary>
        ''' Verifies that every search path pattern targets the culture-specific directory of the configuration,
        ''' either by its name (e.g. <c>es-ES</c>) or through a Language Experience Pack wildcard pattern.
        ''' </summary>
        <TestMethod>
        Public Sub BuildSearchPathPatterns_EsEs_AllPatternsTargetTheCulture()

            Dim patterns As SortedSet(Of String) = PathHelper.BuildSearchPathPatterns(New LanguageConfiguration_esES())

            For Each pattern As String In patterns
                Dim lastSegment As String = Path.GetFileName(pattern)
                Dim isCultureDirectory As Boolean = lastSegment.Equals("es-ES", StringComparison.OrdinalIgnoreCase)
                Dim isLanguagePackPattern As Boolean = lastSegment.Equals("Microsoft.LanguageExperiencePackes-ES_*", StringComparison.OrdinalIgnoreCase)

                Assert.IsTrue(isCultureDirectory OrElse isLanguagePackPattern, $"Unexpected pattern: {pattern}")
            Next pattern
        End Sub

        ''' <summary>
        ''' Verifies that the well-known MUI directories are included in the search path patterns.
        ''' </summary>
        <TestMethod>
        Public Sub BuildSearchPathPatterns_EsEs_IncludesWellKnownDirectories()

            Dim patterns As SortedSet(Of String) = PathHelper.BuildSearchPathPatterns(New LanguageConfiguration_esES())

            Dim expectedDirectories As String() = {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows), "es-ES"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "es-ES"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Windows NT", "Accessories", "es-ES"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Wordpad", "es-ES"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "WindowsApps", "Microsoft.LanguageExperiencePackes-ES_*")
            }

            For Each expectedDirectory As String In expectedDirectories
                Dim expectedPattern As String = $"{ExtendedPathPrefix}{expectedDirectory}"
                Assert.IsTrue(patterns.Contains(expectedPattern), $"Missing pattern: {expectedPattern}{Environment.NewLine}Actual patterns:{Environment.NewLine}{String.Join(Environment.NewLine, patterns)}")
            Next expectedDirectory
        End Sub

        ''' <summary>
        ''' Verifies the expected number of search path patterns, considering that some special folders
        ''' resolve to the same path depending on the operating system and process architecture.
        ''' </summary>
        <TestMethod>
        Public Sub BuildSearchPathPatterns_EsEs_ReturnsOnePatternPerDistinctLocation()

            Dim patterns As SortedSet(Of String) = PathHelper.BuildSearchPathPatterns(New LanguageConfiguration_esES())

            Dim programFiles As String = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)
            Dim programFilesX86 As String = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)
            Dim system As String = Environment.GetFolderPath(Environment.SpecialFolder.System)
            Dim systemX86 As String = Environment.GetFolderPath(Environment.SpecialFolder.SystemX86)

            ' 8 locations: Windows, System32, SysWOW64, 2 x WindowsApps, 2 x Accessories, Wordpad.
            Dim expectedCount As Integer = 8
            If String.Equals(system, systemX86, StringComparison.OrdinalIgnoreCase) Then
                expectedCount -= 1
            End If
            If String.Equals(programFiles, programFilesX86, StringComparison.OrdinalIgnoreCase) Then
                expectedCount -= 2
            End If

            Assert.AreEqual(expectedCount, patterns.Count, String.Join(Environment.NewLine, patterns))
        End Sub

        ''' <summary>
        ''' Verifies that the search path patterns follow the culture of the specified configuration.
        ''' </summary>
        <TestMethod>
        Public Sub BuildSearchPathPatterns_OtherCulture_PatternsTargetThatCulture()

            Dim accTable As String = "1 ACCELERATORS" & Environment.NewLine & "LANGUAGE LANG_FRENCH, SUBLANG_FRENCH" & Environment.NewLine & "{" & Environment.NewLine & "}"
            Dim config As New TestLanguageConfiguration(New CultureInfo("fr-FR"), {New MuiDescriptor("notepad.exe.mui", "00000000", accTable)})

            Dim patterns As SortedSet(Of String) = PathHelper.BuildSearchPathPatterns(config)

            Assert.IsTrue(patterns.All(Function(p As String) p.IndexOf("fr-FR", StringComparison.OrdinalIgnoreCase) >= 0))
            Assert.IsFalse(patterns.Any(Function(p As String) p.IndexOf("es-ES", StringComparison.OrdinalIgnoreCase) >= 0))
        End Sub

#End Region

#Region " FindFiles "

        ''' <summary>
        ''' Verifies that matching files are found in the root directory and in nested subdirectories, ignoring non-matching files.
        ''' </summary>
        <TestMethod>
        Public Sub FindFiles_NestedDirectories_FindsAllMatchingFiles()

            Using tempDir As New TempDirectory()
                Dim expected As String() = {
                    tempDir.CreateEmptyFile("notepad.exe.mui"),
                    tempDir.CreateEmptyFile("a\notepad.exe.mui"),
                    tempDir.CreateEmptyFile("a\b\c\notepad.exe.mui"),
                    tempDir.CreateEmptyFile("d\notepad.exe.mui")
                }
                tempDir.CreateEmptyFile("wordpad.exe.mui")
                tempDir.CreateEmptyFile("a\notepad.exe.mui.bak")
                tempDir.CreateEmptyFile("a\b\notepad.exe")
                tempDir.CreateSubdirectory("e\empty")

                Dim matches As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
                PathHelper.FindFiles(tempDir.FullPath, "notepad.exe.mui", matches)

                Assert.AreEqual(expected.Length, matches.Count, String.Join(Environment.NewLine, matches))
                For Each expectedFile As String In expected
                    Assert.IsTrue(matches.Contains(expectedFile), $"Missing match: {expectedFile}")
                Next expectedFile
            End Using
        End Sub

        ''' <summary>
        ''' Verifies that file name matching is case-insensitive.
        ''' </summary>
        <TestMethod>
        Public Sub FindFiles_DifferentCase_FindsFile()

            Using tempDir As New TempDirectory()
                Dim filePath As String = tempDir.CreateEmptyFile("sub\SHELL32.DLL.MUI")

                Dim matches As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
                PathHelper.FindFiles(tempDir.FullPath, "shell32.dll.mui", matches)

                Assert.AreEqual(1, matches.Count)
                Assert.IsTrue(matches.Contains(filePath))
            End Using
        End Sub

        ''' <summary>
        ''' Verifies that wildcard search patterns are supported.
        ''' </summary>
        <TestMethod>
        Public Sub FindFiles_WildcardPattern_FindsAllMatchingFiles()

            Using tempDir As New TempDirectory()
                tempDir.CreateEmptyFile("notepad.exe.mui")
                tempDir.CreateEmptyFile("x\wordpad.exe.mui")
                tempDir.CreateEmptyFile("x\readme.txt")

                Dim matches As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
                PathHelper.FindFiles(tempDir.FullPath, "*.mui", matches)

                Assert.AreEqual(2, matches.Count, String.Join(Environment.NewLine, matches))
            End Using
        End Sub

        ''' <summary>
        ''' Verifies that the search also works with Extended-Length root paths, as used by the application.
        ''' </summary>
        <TestMethod>
        Public Sub FindFiles_ExtendedRootPath_FindsFiles()

            Using tempDir As New TempDirectory()
                tempDir.CreateEmptyFile("a\b\notepad.exe.mui")

                Dim matches As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
                PathHelper.FindFiles(PathHelper.GetExtendedPath(tempDir.FullPath), "notepad.exe.mui", matches)

                Assert.AreEqual(1, matches.Count)
                Assert.IsTrue(matches.Single().StartsWith(ExtendedPathPrefix, StringComparison.Ordinal))
            End Using
        End Sub

        ''' <summary>
        ''' Verifies that previously collected matches are preserved, and found files are added to them.
        ''' </summary>
        <TestMethod>
        Public Sub FindFiles_ExistingMatches_AreKeptAndNewMatchesAdded()

            Using tempDir As New TempDirectory()
                Dim filePath As String = tempDir.CreateEmptyFile("notepad.exe.mui")
                Const previousMatch As String = "C:\Previous\notepad.exe.mui"

                Dim matches As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase) From {previousMatch}
                PathHelper.FindFiles(tempDir.FullPath, "notepad.exe.mui", matches)

                Assert.AreEqual(2, matches.Count)
                Assert.IsTrue(matches.Contains(previousMatch))
                Assert.IsTrue(matches.Contains(filePath))
            End Using
        End Sub

        ''' <summary>
        ''' Verifies that a null matches collection is created by the method.
        ''' </summary>
        <TestMethod>
        Public Sub FindFiles_NullMatchesCollection_CreatesCollection()

            Using tempDir As New TempDirectory()
                tempDir.CreateEmptyFile("notepad.exe.mui")

                Dim matches As HashSet(Of String) = Nothing
                PathHelper.FindFiles(tempDir.FullPath, "notepad.exe.mui", matches)

                Assert.IsNotNull(matches)
                Assert.AreEqual(1, matches.Count)
            End Using
        End Sub

        ''' <summary>
        ''' Verifies that no matches are returned when no file matches.
        ''' </summary>
        <TestMethod>
        Public Sub FindFiles_NoMatchingFiles_ReturnsNoMatches()

            Using tempDir As New TempDirectory()
                tempDir.CreateEmptyFile("a\wordpad.exe.mui")

                Dim matches As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
                PathHelper.FindFiles(tempDir.FullPath, "notepad.exe.mui", matches)

                Assert.AreEqual(0, matches.Count)
            End Using
        End Sub

        ''' <summary>
        ''' Verifies that a non-existent root directory does not throw and returns no matches.
        ''' </summary>
        <TestMethod>
        Public Sub FindFiles_NonExistentRoot_ReturnsNoMatches()

            Using tempDir As New TempDirectory()
                Dim missingRoot As String = Path.Combine(tempDir.FullPath, "missing")

                Dim matches As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
                PathHelper.FindFiles(missingRoot, "notepad.exe.mui", matches)

                Assert.AreEqual(0, matches.Count)
            End Using
        End Sub

#End Region

    End Class

End Namespace

#End Region
