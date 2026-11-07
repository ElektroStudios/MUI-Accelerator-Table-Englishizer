#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Globalization
Imports System.Runtime.InteropServices
Imports System.Text

Imports DevCase.Win32
Imports DevCase.Win32.Structures

Imports MATE.Tests.Helpers

#End Region

#Region " NativeMethodsTests "

Namespace MATE.Tests

    ''' <summary>
    ''' Unit tests for the P/Invoke declarations in the <c>DevCase.Win32.NativeMethods</c> namespace
    ''' and the <see cref="OsVersionInfoEx"/> structure.
    ''' </summary>
    <TestClass>
    Public NotInheritable Class NativeMethodsTests

#Region " Constants "

        ''' <summary>
        ''' The size, in bytes, of the native <c>RTL_OSVERSIONINFOEXW</c> structure:
        ''' <para></para>
        ''' 5 x ULONG (20) + WCHAR[128] (256) + 3 x USHORT (6) + 2 x UCHAR (2) = 284.
        ''' </summary>
        Private Const NativeOsVersionInfoExWSize As Integer = 284

        ''' <summary>
        ''' The <c>VER_PLATFORM_WIN32_NT</c> platform identifier.
        ''' </summary>
        Private Const VerPlatformWin32NT As Integer = 2

        ''' <summary>
        ''' The <c>STATUS_SUCCESS</c> NTSTATUS code.
        ''' </summary>
        Private Const StatusSuccess As Integer = 0

#End Region

#Region " OsVersionInfoEx "

        ''' <summary>
        ''' Regression test: the managed structure must marshal to the exact size of the native
        ''' <c>RTL_OSVERSIONINFOEXW</c> structure (<c>CsdVersion</c> must be marshaled as a Unicode buffer).
        ''' <para></para>
        ''' Previously, the missing <see cref="CharSet.Unicode"/> setting marshaled <c>CsdVersion</c> as a 128-byte ANSI buffer,
        ''' resulting in a 156-byte structure.
        ''' </summary>
        <TestMethod>
        Public Sub OsVersionInfoEx_MarshaledSize_MatchesNativeRtlOsVersionInfoExW()

            Dim actual As Integer = Marshal.SizeOf(GetType(OsVersionInfoEx))

            Assert.AreEqual(NativeOsVersionInfoExWSize, actual)
        End Sub

        ''' <summary>
        ''' Verifies that the size-initializing constructor sets the <see cref="OsVersionInfoEx.SizeOfStruct"/> field.
        ''' </summary>
        <TestMethod>
        Public Sub OsVersionInfoEx_InitializeSizeTrue_SetsSizeOfStruct()

            Dim info As New OsVersionInfoEx(initializeSize:=True)

            Assert.AreEqual(NativeOsVersionInfoExWSize, info.SizeOfStruct)
        End Sub

        ''' <summary>
        ''' Verifies that the constructor leaves the <see cref="OsVersionInfoEx.SizeOfStruct"/> field to zero when not requested.
        ''' </summary>
        <TestMethod>
        Public Sub OsVersionInfoEx_InitializeSizeFalse_LeavesSizeOfStructZero()

            Dim info As New OsVersionInfoEx(initializeSize:=False)

            Assert.AreEqual(0, info.SizeOfStruct)
        End Sub

#End Region

#Region " RtlGetVersion "

        ''' <summary>
        ''' Verifies that <c>RtlGetVersion</c> succeeds and returns consistent Windows NT version information.
        ''' </summary>
        <TestMethod>
        Public Sub RtlGetVersion_ValidStructure_ReturnsSuccessAndVersionInfo()

            Dim info As New OsVersionInfoEx(initializeSize:=True)

            Dim status As Integer = NativeMethods.RtlGetVersion(info)

            Assert.AreEqual(StatusSuccess, status)
            Assert.AreEqual(VerPlatformWin32NT, info.PlatformId)
            Assert.IsTrue(info.MajorVersion >= 6, $"Unexpected major version: {info.MajorVersion}")
            Assert.IsTrue(info.BuildNumber > 0, $"Unexpected build number: {info.BuildNumber}")
        End Sub

        ''' <summary>
        ''' Regression test: the extended fields (only filled by <c>RtlGetVersion</c> when the structure size
        ''' matches <c>RTL_OSVERSIONINFOEXW</c>) must be populated.
        ''' <para></para>
        ''' <c>ProductType</c> is always one of <c>VER_NT_WORKSTATION</c> (1), <c>VER_NT_DOMAIN_CONTROLLER</c> (2)
        ''' or <c>VER_NT_SERVER</c> (3); it stays zero when the structure size is wrong.
        ''' </summary>
        <TestMethod>
        Public Sub RtlGetVersion_ValidStructure_PopulatesExtendedFields()

            Dim info As New OsVersionInfoEx(initializeSize:=True)

            NativeMethods.RtlGetVersion(info)

            Assert.IsTrue(info.ProductType >= 1 AndAlso info.ProductType <= 3, $"Unexpected product type: {info.ProductType}")
        End Sub

#End Region

#Region " RtlComputeCrc32 "

        ''' <summary>
        ''' Verifies the standard CRC-32 check value ("123456789" → <c>0xCBF43926</c>).
        ''' </summary>
        <TestMethod>
        Public Sub RtlComputeCrc32_CheckValue_ReturnsCbf43926()

            Dim data As Byte() = Encoding.ASCII.GetBytes("123456789")

            Dim actual As UInteger = NativeMethods.RtlComputeCrc32(0UI, data, CUInt(data.Length))

            Assert.AreEqual(&HCBF43926UI, actual)
        End Sub

        ''' <summary>
        ''' Verifies that a zero-length buffer returns the initial value unchanged.
        ''' </summary>
        <TestMethod>
        Public Sub RtlComputeCrc32_EmptyBuffer_ReturnsInitialValue()

            Dim actual As UInteger = NativeMethods.RtlComputeCrc32(&H12345678UI, Array.Empty(Of Byte)(), 0UI)

            Assert.AreEqual(&H12345678UI, actual)
        End Sub

        ''' <summary>
        ''' Verifies that the native result matches the managed reference implementation.
        ''' </summary>
        <TestMethod>
        Public Sub RtlComputeCrc32_RandomData_MatchesReferenceImplementation()

            Dim data As Byte() = TempDirectory.CreateRandomBytes(100000, seed:=2026)

            Dim actual As UInteger = NativeMethods.RtlComputeCrc32(0UI, data, CUInt(data.Length))

            Assert.AreEqual(ReferenceCrc32.Compute(data), actual)
        End Sub

#End Region

#Region " GetUserDefaultUILanguage "

        ''' <summary>
        ''' Verifies that the returned language identifier is valid and can be used
        ''' to create a <see cref="CultureInfo"/>, as the application does at startup.
        ''' </summary>
        <TestMethod>
        Public Sub GetUserDefaultUILanguage_ReturnsValidCultureIdentifier()

            Dim langId As UShort = NativeMethods.GetUserDefaultUILanguage()

            Assert.AreNotEqual(0US, langId)
            Dim ci As New CultureInfo(langId)
            Assert.IsFalse(String.IsNullOrEmpty(ci.Name), $"Language identifier 0x{langId:X4} resolved to the invariant culture.")
        End Sub

#End Region

    End Class

End Namespace

#End Region
