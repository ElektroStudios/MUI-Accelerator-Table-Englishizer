#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.IO
Imports System.Text
Imports System.Text.RegularExpressions

Imports DevCase.Core.Security.DataIntegrity.Checksum

Imports MATE.Tests.Helpers

#End Region

#Region " UtilChecksumTests "

Namespace MATE.Tests

    ''' <summary>
    ''' Unit tests for the <see cref="UtilChecksum"/> class.
    ''' <para></para>
    ''' The CRC-32 checksum is the key used to identify supported MUI file variants,
    ''' so a wrong result would make the application skip (or wrongly patch) system files.
    ''' </summary>
    <TestClass>
    Public NotInheritable Class UtilChecksumTests

#Region " ComputeCRC32OfBytes "

        ''' <summary>
        ''' Verifies the result against well-known, published CRC-32 (IEEE 802.3) test vectors.
        ''' </summary>
        '''
        ''' <param name="asciiInput">
        ''' The ASCII input string.
        ''' </param>
        '''
        ''' <param name="expectedCrc32">
        ''' The expected CRC-32 checksum.
        ''' </param>
        <TestMethod>
        <DataRow("", "00000000", DisplayName:="Empty string")>
        <DataRow("a", "E8B7BE43")>
        <DataRow("abc", "352441C2")>
        <DataRow("message digest", "20159D7F")>
        <DataRow("123456789", "CBF43926", DisplayName:="Standard CRC-32 check value")>
        <DataRow("The quick brown fox jumps over the lazy dog", "414FA339")>
        Public Sub ComputeCRC32OfBytes_KnownAsciiVectors_ReturnsExpectedChecksum(asciiInput As String, expectedCrc32 As String)

            Dim data As Byte() = Encoding.ASCII.GetBytes(asciiInput)

            Dim actual As String = UtilChecksum.ComputeCRC32OfBytes(data)

            Assert.AreEqual(expectedCrc32, actual)
        End Sub

        ''' <summary>
        ''' Verifies the result against well-known CRC-32 test vectors made of binary data.
        ''' </summary>
        '''
        ''' <param name="value">
        ''' The value of every byte of the input.
        ''' </param>
        '''
        ''' <param name="length">
        ''' The input length, in bytes.
        ''' </param>
        '''
        ''' <param name="expectedCrc32">
        ''' The expected CRC-32 checksum.
        ''' </param>
        <TestMethod>
        <DataRow(CByte(&H0), 1, "D202EF8D", DisplayName:="Single zero byte")>
        <DataRow(CByte(&H0), 32, "190A55AD", DisplayName:="32 zero bytes")>
        <DataRow(CByte(&HFF), 4, "FFFFFFFF", DisplayName:="4 bytes of 0xFF")>
        Public Sub ComputeCRC32OfBytes_KnownBinaryVectors_ReturnsExpectedChecksum(value As Byte, length As Integer, expectedCrc32 As String)

            Dim data As Byte() = New Byte(length - 1) {}
            For i As Integer = 0 To data.Length - 1
                data(i) = value
            Next i

            Dim actual As String = UtilChecksum.ComputeCRC32OfBytes(data)

            Assert.AreEqual(expectedCrc32, actual)
        End Sub

        ''' <summary>
        ''' Verifies the result against an independent, fully managed reference implementation, using pseudo-random data of various sizes.
        ''' </summary>
        '''
        ''' <param name="length">
        ''' The input length, in bytes.
        ''' </param>
        <TestMethod>
        <DataRow(1)>
        <DataRow(7)>
        <DataRow(255)>
        <DataRow(4096)>
        <DataRow(65537)>
        <DataRow(1048576)>
        Public Sub ComputeCRC32OfBytes_RandomData_MatchesReferenceImplementation(length As Integer)

            Dim data As Byte() = TempDirectory.CreateRandomBytes(length, seed:=length)

            Dim actual As String = UtilChecksum.ComputeCRC32OfBytes(data)

            Assert.AreEqual(ReferenceCrc32.ComputeHex(data), actual)
        End Sub

        ''' <summary>
        ''' Verifies that the result is always an uppercase, zero-padded, 8-digit hexadecimal string.
        ''' </summary>
        <TestMethod>
        Public Sub ComputeCRC32OfBytes_AnyInput_ReturnsUppercaseEightDigitHexString()

            For seed As Integer = 0 To 63
                Dim data As Byte() = TempDirectory.CreateRandomBytes(seed + 1, seed)

                Dim actual As String = UtilChecksum.ComputeCRC32OfBytes(data)

                Assert.IsTrue(Regex.IsMatch(actual, "^[0-9A-F]{8}$"), $"Unexpected format: '{actual}'.")
            Next seed
        End Sub

        ''' <summary>
        ''' Verifies that a single bit change produces a different checksum.
        ''' </summary>
        <TestMethod>
        Public Sub ComputeCRC32OfBytes_SingleBitChange_ReturnsDifferentChecksum()

            Dim data As Byte() = TempDirectory.CreateRandomBytes(1024, seed:=42)
            Dim original As String = UtilChecksum.ComputeCRC32OfBytes(data)

            data(512) = data(512) Xor CByte(1)
            Dim modified As String = UtilChecksum.ComputeCRC32OfBytes(data)

            Assert.AreNotEqual(original, modified)
        End Sub

        ''' <summary>
        ''' Verifies that the input array is not modified.
        ''' </summary>
        <TestMethod>
        Public Sub ComputeCRC32OfBytes_DoesNotModifyInput()

            Dim data As Byte() = TempDirectory.CreateRandomBytes(256, seed:=7)
            Dim copy As Byte() = DirectCast(data.Clone(), Byte())

            UtilChecksum.ComputeCRC32OfBytes(data)

            CollectionAssert.AreEqual(copy, data)
        End Sub

        ''' <summary>
        ''' Regression test: a null array throws <see cref="ArgumentNullException"/> (fail-fast),
        ''' instead of a <see cref="NullReferenceException"/>.
        ''' </summary>
        <TestMethod>
        Public Sub ComputeCRC32OfBytes_NullArray_ThrowsArgumentNullException()

            Dim ex As ArgumentNullException = Assert.ThrowsExactly(Of ArgumentNullException)(
                Sub() UtilChecksum.ComputeCRC32OfBytes(Nothing))

            Assert.AreEqual("bytes", ex.ParamName)
        End Sub

#End Region

#Region " ComputeCRC32OfFile "

        ''' <summary>
        ''' Verifies that the checksum of a file is the same as the checksum of its content.
        ''' </summary>
        '''
        ''' <param name="length">
        ''' The file length, in bytes.
        ''' </param>
        <TestMethod>
        <DataRow(0, DisplayName:="Empty file")>
        <DataRow(1)>
        <DataRow(8192)>
        <DataRow(300000)>
        Public Sub ComputeCRC32OfFile_ExistingFile_MatchesChecksumOfItsContent(length As Integer)

            Using tempDir As New TempDirectory()
                Dim content As Byte() = TempDirectory.CreateRandomBytes(length, seed:=length)
                Dim filePath As String = tempDir.CreateFile("file.bin", content)

                Dim actual As String = UtilChecksum.ComputeCRC32OfFile(filePath)

                Assert.AreEqual(ReferenceCrc32.ComputeHex(content), actual)
                Assert.AreEqual(UtilChecksum.ComputeCRC32OfBytes(content), actual)
            End Using
        End Sub

        ''' <summary>
        ''' Verifies that the well-known CRC-32 check value is obtained from a file containing "123456789".
        ''' </summary>
        <TestMethod>
        Public Sub ComputeCRC32OfFile_CheckValueFile_ReturnsCbf43926()

            Using tempDir As New TempDirectory()
                Dim filePath As String = tempDir.CreateFile("check.mui", Encoding.ASCII.GetBytes("123456789"))

                Dim actual As String = UtilChecksum.ComputeCRC32OfFile(filePath)

                Assert.AreEqual("CBF43926", actual)
            End Using
        End Sub

        ''' <summary>
        ''' Verifies that the checksum of a read-only file can be computed.
        ''' <para></para>
        ''' System MUI files are commonly read-only.
        ''' </summary>
        <TestMethod>
        Public Sub ComputeCRC32OfFile_ReadOnlyFile_ReturnsChecksum()

            Using tempDir As New TempDirectory()
                Dim content As Byte() = TempDirectory.CreateRandomBytes(1000, seed:=3)
                Dim filePath As String = tempDir.CreateFile("readonly.mui", content)
                File.SetAttributes(filePath, FileAttributes.ReadOnly)

                Dim actual As String = UtilChecksum.ComputeCRC32OfFile(filePath)

                Assert.AreEqual(ReferenceCrc32.ComputeHex(content), actual)
            End Using
        End Sub

        ''' <summary>
        ''' Verifies that a non-existent file throws <see cref="FileNotFoundException"/>.
        ''' </summary>
        <TestMethod>
        Public Sub ComputeCRC32OfFile_NonExistentFile_ThrowsFileNotFoundException()

            Using tempDir As New TempDirectory()
                Dim filePath As String = Path.Combine(tempDir.FullPath, "missing.mui")

                Assert.ThrowsExactly(Of FileNotFoundException)(Sub() UtilChecksum.ComputeCRC32OfFile(filePath))
            End Using
        End Sub

        ''' <summary>
        ''' Regression test: a null, empty or white-space file path throws <see cref="ArgumentNullException"/> (fail-fast).
        ''' </summary>
        '''
        ''' <param name="filePath">
        ''' The file path.
        ''' </param>
        <TestMethod>
        <DataRow(Nothing, DisplayName:="Null")>
        <DataRow("", DisplayName:="Empty")>
        <DataRow("   ", DisplayName:="White spaces")>
        Public Sub ComputeCRC32OfFile_NullOrWhiteSpacePath_ThrowsArgumentNullException(filePath As String)

            Dim ex As ArgumentNullException = Assert.ThrowsExactly(Of ArgumentNullException)(
                Sub() UtilChecksum.ComputeCRC32OfFile(filePath))

            Assert.AreEqual("filepath", ex.ParamName)
        End Sub

#End Region

    End Class

End Namespace

#End Region
