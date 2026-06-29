
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Diagnostics

Imports DevCase.Win32
Imports DevCase.Win32.Structures

#End Region

#Region " Util Windows "

' ReSharper disable once CheckNamespace

Namespace DevCase.Core.Windows.Common

    ''' <summary>
    ''' Provides operating system information.
    ''' </summary>
    Public NotInheritable Class UtilWindows

#Region " Constructors "

        ''' <summary>
        ''' Prevents a default instance of the <see cref="UtilWindows"/> class from being created.
        ''' </summary>
        <DebuggerNonUserCode>
        Private Sub New()
        End Sub

#End Region

#Region " Properties "

        ''' <summary>
        ''' Gets a value that determines whether the current operating system is <c>Windows 10</c>, or greater.
        ''' </summary>
        '''
        ''' <example> This is a code example.
        ''' <code language="VB.NET">
        ''' If Not IsWin10OrGreater Then
        '''     Throw New PlatformNotSupportedException("This application cannot run under the current Windows version.")
        ''' End If
        ''' </code>
        ''' </example>
        '''
        ''' <value>
        ''' A value that determines whether the current operating system is <c>Windows 10</c>, or greater.
        ''' </value>
        Public Shared ReadOnly Property IsWin10OrGreater() As Boolean
            <DebuggerStepThrough>
            Get
                Dim platformId As Integer = -1
                Dim osVersion As Version = Nothing
                UtilWindows.Internal_GetOsInfo(platformId, osVersion, Nothing)

                Dim minVersion As New Version(10, 0)
                Return platformId = System.PlatformID.Win32NT AndAlso
                       osVersion.CompareTo(minVersion) >= 0
            End Get
        End Property

        ''' <summary>
        ''' Gets a value that determines whether the current operating system is <c>Windows 11</c>, or greater.
        ''' </summary>
        '''
        ''' <example> This is a code example.
        ''' <code language="VB.NET">
        ''' If Not IsWin11OrGreater Then
        '''     Throw New PlatformNotSupportedException("This application cannot run under the current Windows version.")
        ''' End If
        ''' </code>
        ''' </example>
        '''
        ''' <value>
        ''' A value that determines whether the current operating system is <c>Windows 11</c>, or greater.
        ''' </value>
        Public Shared ReadOnly Property IsWin11OrGreater() As Boolean
            <DebuggerStepThrough>
            Get
                Dim platformId As Integer = -1
                Dim osVersion As Version = Nothing
                UtilWindows.Internal_GetOsInfo(platformId, osVersion, Nothing)

                Dim minVersion As New Version(10, 0, 22000)
                Return platformId = System.PlatformID.Win32NT AndAlso
                       osVersion.CompareTo(minVersion) >= 0
            End Get
        End Property

#End Region

#Region " Private Methods "

        ''' <summary>
        ''' Retrieves the operating system platform identifier and version information.
        ''' </summary>
        ''' 
        ''' <param name="refPlatformID">
        ''' When this method returns, contains the platform identifier of the operating system.
        ''' </param>
        ''' 
        ''' <param name="refVersion">
        ''' When this method returns, contains a <see cref="System.Version"/> object representing 
        ''' the major, minor, and build numbers of the operating system.
        ''' </param>
        ''' 
        ''' <param name="refOsVersionInfoEx">
        ''' When this method returns, contains a <see cref="OsVersionInfoEx"/> object representing 
        ''' the system information obtained by invoking the native Win32 <c>RtlGetVersion</c> API.
        ''' </param>
        ''' 
        ''' <remarks>
        ''' This method first attempts to retrieve precise version information by invoking the native Win32 <c>RtlGetVersion</c> API 
        ''' to bypass potential application compatibility shims. If the native call fails (returns a non-zero status), 
        ''' it falls back to the standard <see cref="Environment.OSVersion"/> properties.
        ''' </remarks>
        <DebuggerStepThrough>
        Private Shared Sub Internal_GetOsInfo(ByRef refPlatformID As Integer,
                                              ByRef refVersion As Version,
                                              ByRef refOsVersionInfoEx As OsVersionInfoEx)

            ' Call proper Win32 GetVersion API. 
            If refOsVersionInfoEx.Equals(Nothing) OrElse (refOsVersionInfoEx.SizeOfStruct = 0) Then
                refOsVersionInfoEx = New OsVersionInfoEx(initializeSize:=True)
            End If
            Dim status As Integer = NativeMethods.RtlGetVersion(refOsVersionInfoEx)
            If status = 0 Then
                refPlatformID = refOsVersionInfoEx.PlatformId
                refVersion = New Version(refOsVersionInfoEx.MajorVersion, refOsVersionInfoEx.MinorVersion, refOsVersionInfoEx.BuildNumber)
            End If

            ' .NET Fallback.
            Dim os As OperatingSystem = Environment.OSVersion
            refPlatformID = os.Platform
            refVersion = os.Version
        End Sub

#End Region

    End Class

End Namespace

#End Region
