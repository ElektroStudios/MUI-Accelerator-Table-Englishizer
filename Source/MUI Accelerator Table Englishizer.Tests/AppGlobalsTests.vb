#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.Generic
Imports System.IO

#End Region

#Region " AppGlobalsTests "

Namespace MATE.Tests

    ''' <summary>
    ''' Unit tests for the <see cref="AppGlobals"/> module.
    ''' </summary>
    '''
    ''' <remarks>
    ''' Values derived from <c>My.Application.Info</c> are not asserted, because under the test host
    ''' the entry assembly is the test runner instead of the application.
    ''' </remarks>
    <TestClass>
    Public NotInheritable Class AppGlobalsTests

        ''' <summary>
        ''' Verifies that the MUI file suffixes are non-empty, distinct, and usable as file name extensions.
        ''' <para></para>
        ''' The application relies on these suffixes to tell apart backup, pending and failed MUI files,
        ''' so a collision would make a scheduled operation overwrite the wrong file.
        ''' </summary>
        <TestMethod>
        Public Sub MuiFileSuffixes_AreValidAndDistinct()

            Dim suffixes As String() = {AppGlobals.MuiFileBackupSuffix, AppGlobals.MuiFilePendingSuffix, AppGlobals.MuiFileFailedSuffix}

            For Each suffix As String In suffixes
                Assert.IsFalse(String.IsNullOrWhiteSpace(suffix))
                Assert.IsFalse(suffix.Contains("."), $"Suffix '{suffix}' must not contain dots.")
                Assert.AreEqual(-1, suffix.IndexOfAny(Path.GetInvalidFileNameChars()), $"Suffix '{suffix}' contains invalid file name characters.")
            Next suffix

            Dim distinctSuffixes As New HashSet(Of String)(suffixes, StringComparer.OrdinalIgnoreCase)
            Assert.AreEqual(suffixes.Length, distinctSuffixes.Count, $"Suffixes must be distinct: {String.Join(", ", suffixes)}")
        End Sub

        ''' <summary>
        ''' Verifies that a MUI file with any of the suffixes no longer has the '.mui' extension,
        ''' so it is never matched again as a MUI file by later scans.
        ''' </summary>
        '''
        ''' <param name="suffix">
        ''' The file suffix.
        ''' </param>
        <TestMethod>
        <DataRow(AppGlobals.MuiFileBackupSuffix)>
        <DataRow(AppGlobals.MuiFilePendingSuffix)>
        <DataRow(AppGlobals.MuiFileFailedSuffix)>
        Public Sub MuiFileSuffixes_ChangeTheMuiExtension(suffix As String)

            Dim fileName As String = $"shell32.dll.mui.{suffix}"

            Assert.AreEqual($".{suffix}", Path.GetExtension(fileName))
            Assert.AreNotEqual(".mui", Path.GetExtension(fileName), ignoreCase:=True)
        End Sub

        ''' <summary>
        ''' Verifies the registry value name used as the volatile "tasks completed" checkpoint.
        ''' </summary>
        <TestMethod>
        Public Sub RegVolatileValueName_IsNotEmpty()

            Assert.IsFalse(String.IsNullOrWhiteSpace(AppGlobals.RegVolatileValueName))
        End Sub

        ''' <summary>
        ''' Verifies that the Resource Hacker log file is located in the temporary directory.
        ''' </summary>
        <TestMethod>
        Public Sub ResourceHackerLogFilePath_IsInTempDirectory()

            Dim expected As String = Path.Combine(Path.GetTempPath(), "ResourceHacker.log")

            Assert.AreEqual(expected, AppGlobals.RESOURCE_HACKER_LOGFILE_PATH, ignoreCase:=True)
        End Sub

        ''' <summary>
        ''' Verifies that the tool executable paths point to the expected file names inside a <c>Tools</c> folder,
        ''' and that the MoveFile executable matches the process architecture.
        ''' </summary>
        <TestMethod>
        Public Sub ToolExecutablePaths_PointToExpectedFiles()

            Dim expectedMoveFileName As String = If(IntPtr.Size = 8, "movefile64.exe", "movefile.exe")

            Assert.AreEqual(expectedMoveFileName, Path.GetFileName(AppGlobals.MOVEFILE_EXEC_PATH), ignoreCase:=True)
            Assert.AreEqual("Tools", Path.GetFileName(Path.GetDirectoryName(AppGlobals.MOVEFILE_EXEC_PATH)), ignoreCase:=True)

            Assert.AreEqual("ResourceHacker.exe", Path.GetFileName(AppGlobals.RESOURCE_HACKER_EXEC_PATH), ignoreCase:=True)
            Assert.AreEqual("Tools", Path.GetFileName(Path.GetDirectoryName(AppGlobals.RESOURCE_HACKER_EXEC_PATH)), ignoreCase:=True)
        End Sub

    End Class

End Namespace

#End Region
