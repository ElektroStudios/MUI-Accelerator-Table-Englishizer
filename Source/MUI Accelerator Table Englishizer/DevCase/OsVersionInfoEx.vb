
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Diagnostics
Imports System.Runtime.InteropServices

#End Region

#Region " OsVersionInfo Ex "

' ReSharper disable once CheckNamespace

Namespace DevCase.Win32.Structures

    ''' <summary>
    ''' Contains operating system version information.
    ''' <para></para>
    ''' The information includes major and minor version numbers, a build number, a platform identifier, 
    ''' and information about product suites and the latest Service Pack installed on the system.
    ''' <para></para>
    ''' This structure is used with the <c>NativeMethods.GetVersionEx</c> and <c>VerifyVersionInfo</c> functions.
    ''' </summary>
    '''
    ''' <remarks>
    ''' <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms724833%28v=vs.85%29.aspx"/>
    ''' </remarks>
    <DebuggerStepThrough>
    <StructLayout(LayoutKind.Sequential)>
    Public Structure OsVersionInfoEx

#Region " Fields "

        ''' <summary>
        ''' The size of the structure, in bytes.
        ''' <para></para>
        ''' This member must be set to <c>Marshal.SizeOf(Of OsVersionInfoEx)</c> before calling any function. 
        ''' </summary>
        Public SizeOfStruct As Integer

        ''' <summary>
        ''' The major version number of the operating system.
        ''' </summary>
        Public MajorVersion As Integer

        ''' <summary>
        ''' The minor version number of the operating system
        ''' </summary>
        Public MinorVersion As Integer

        ''' <summary>
        ''' The build number of the operating system.
        ''' </summary>
        Public BuildNumber As Integer

        ''' <summary>
        ''' The operating system platform.
        ''' </summary>
        Public PlatformId As Integer

        ''' <summary>
        ''' A null-terminated string, such as "<c>Service Pack 3</c>", 
        ''' that indicates the latest Service Pack installed on the system.
        ''' <para></para>
        ''' If no Service Pack has been installed, the string is empty.
        ''' </summary>
        <MarshalAs(UnmanagedType.ByValTStr, SizeConst:=128)>
        Public CsdVersion As String

        ''' <summary>
        ''' The major version number of the latest Service Pack installed on the system.
        ''' <para></para>
        ''' For example, for <c>Service Pack 3</c>, the major version number is <c>3</c>.
        ''' <para></para>
        ''' If no Service Pack has been installed, the value is zero.
        ''' </summary>
        Public ServicePackMajor As Short

        ''' <summary>
        ''' The minor version number of the latest Service Pack installed on the system.
        ''' <para></para>
        ''' For example, for <c>Service Pack 3</c>, the minor version number is <c>0</c>.
        ''' </summary>
        Public ServicePackMinor As Short

        ''' <summary>
        ''' A bit mask that identifies the product suites available on the system.
        ''' </summary>
        Public SuiteMask As Short

        ''' <summary>
        ''' Any additional information about the system.
        ''' </summary>
        Public ProductType As Byte

        ''' <summary>
        ''' Reserved for future use.
        ''' </summary>
        Public Reserved As Byte

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Initializes a new instance of the <see cref="OsVersionInfoEx"/> structure.
        ''' </summary>
        '''
        ''' <param name="initializeSize">
        ''' Allows automatic initialization of <see cref="OsVersionInfoEx.SizeOfStruct"/> member.
        ''' </param>
        Public Sub New(initializeSize As Boolean)
            If initializeSize Then
                Me.SizeOfStruct = Marshal.SizeOf(GetType(OsVersionInfoEx))
            End If
        End Sub

#End Region

    End Structure

End Namespace

#End Region
