#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports DevCase.Core.Windows.Common
Imports DevCase.Win32
Imports DevCase.Win32.Structures

#End Region

#Region " UtilWindowsTests "

Namespace MATE.Tests

    ''' <summary>
    ''' Unit tests for the <see cref="UtilWindows"/> class.
    ''' </summary>
    '''
    ''' <remarks>
    ''' The test host process has no application manifest declaring Windows 10/11 compatibility, so
    ''' <see cref="Environment.OSVersion"/> is subject to compatibility shims and reports Windows 8 (6.2)
    ''' on Windows 10 and 11. This makes these tests able to detect whether the native <c>RtlGetVersion</c>
    ''' result is wrongly discarded in favor of the .NET fallback.
    ''' </remarks>
    <TestClass>
    Public NotInheritable Class UtilWindowsTests

#Region " Private Methods "

        ''' <summary>
        ''' Gets the real operating system version by invoking the native <c>RtlGetVersion</c> function.
        ''' </summary>
        '''
        ''' <returns>
        ''' The real operating system version (major, minor and build numbers).
        ''' </returns>
        Private Shared Function GetNativeOsVersion() As Version

            Dim info As New OsVersionInfoEx(initializeSize:=True)
            Dim status As Integer = NativeMethods.RtlGetVersion(info)
            If status <> 0 Then
                Throw New InvalidOperationException($"Precondition failed: RtlGetVersion returned NTSTATUS 0x{status:X8}.")
            End If

            Return New Version(info.MajorVersion, info.MinorVersion, info.BuildNumber)
        End Function

#End Region

#Region " IsWin10OrGreater "

        ''' <summary>
        ''' Regression test: <see cref="UtilWindows.IsWin10OrGreater"/> must report the real operating system version
        ''' obtained from <c>RtlGetVersion</c>, not the shimmed version reported by <see cref="Environment.OSVersion"/>.
        ''' <para></para>
        ''' Previously, the .NET fallback always overwrote the native result.
        ''' </summary>
        <TestMethod>
        Public Sub IsWin10OrGreater_MatchesNativeOsVersion()

            Dim expected As Boolean = UtilWindowsTests.GetNativeOsVersion() >= New Version(10, 0)

            Dim actual As Boolean = UtilWindows.IsWin10OrGreater

            Assert.AreEqual(expected, actual, $"Native OS version: {UtilWindowsTests.GetNativeOsVersion()}; Environment.OSVersion: {Environment.OSVersion.Version}")
        End Sub

#End Region

#Region " IsWin11OrGreater "

        ''' <summary>
        ''' Regression test: <see cref="UtilWindows.IsWin11OrGreater"/> must report the real operating system version
        ''' obtained from <c>RtlGetVersion</c> (Windows 11 is identified by build number 22000 or greater).
        ''' </summary>
        <TestMethod>
        Public Sub IsWin11OrGreater_MatchesNativeOsVersion()

            Dim expected As Boolean = UtilWindowsTests.GetNativeOsVersion() >= New Version(10, 0, 22000)

            Dim actual As Boolean = UtilWindows.IsWin11OrGreater

            Assert.AreEqual(expected, actual, $"Native OS version: {UtilWindowsTests.GetNativeOsVersion()}; Environment.OSVersion: {Environment.OSVersion.Version}")
        End Sub

        ''' <summary>
        ''' Verifies that Windows 11 (or greater) is also detected as Windows 10 (or greater).
        ''' </summary>
        <TestMethod>
        Public Sub IsWin11OrGreater_WhenTrue_ImpliesIsWin10OrGreater()

            Dim isWin11OrGreater As Boolean = UtilWindows.IsWin11OrGreater
            Dim isWin10OrGreater As Boolean = UtilWindows.IsWin10OrGreater

            Assert.IsTrue((Not isWin11OrGreater) OrElse isWin10OrGreater)
        End Sub

        ''' <summary>
        ''' Verifies that the results are stable across multiple calls.
        ''' </summary>
        <TestMethod>
        Public Sub Properties_MultipleCalls_ReturnSameValues()

            Dim firstIsWin10OrGreater As Boolean = UtilWindows.IsWin10OrGreater
            Dim firstIsWin11OrGreater As Boolean = UtilWindows.IsWin11OrGreater

            Dim secondIsWin10OrGreater As Boolean = UtilWindows.IsWin10OrGreater
            Dim secondIsWin11OrGreater As Boolean = UtilWindows.IsWin11OrGreater

            Assert.AreEqual(firstIsWin10OrGreater, secondIsWin10OrGreater)
            Assert.AreEqual(firstIsWin11OrGreater, secondIsWin11OrGreater)
        End Sub

#End Region

    End Class

End Namespace

#End Region
