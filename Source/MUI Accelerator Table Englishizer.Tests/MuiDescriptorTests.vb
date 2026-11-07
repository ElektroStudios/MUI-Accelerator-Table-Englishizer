#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.Generic

#End Region

#Region " MuiDescriptorTests "

Namespace MATE.Tests

    ''' <summary>
    ''' Unit tests for the <see cref="MuiDescriptor"/> class.
    ''' </summary>
    <TestClass>
    Public NotInheritable Class MuiDescriptorTests

#Region " Constants "

        ''' <summary>
        ''' A valid MUI file name.
        ''' </summary>
        Private Const ValidFileName As String = "notepad.exe.mui"

        ''' <summary>
        ''' A valid CRC-32 checksum.
        ''' </summary>
        Private Const ValidChecksum As String = "0a2fad88"

        ''' <summary>
        ''' A minimal, valid accelerator table definition.
        ''' </summary>
        Private Const ValidAccTable As String = "
LANGUAGE LANG_NEUTRAL, SUBLANG_NEUTRAL

1 ACCELERATORS
LANGUAGE LANG_SPANISH, SUBLANG_SPANISH_MODERN
{
    VK_A, 25, CONTROL, VIRTKEY
}
"

#End Region

#Region " Constructor: Valid Arguments "

        ''' <summary>
        ''' Verifies that valid arguments are stored in the corresponding properties.
        ''' </summary>
        <TestMethod>
        Public Sub Constructor_ValidArguments_SetsProperties()

            Dim desc As New MuiDescriptor(ValidFileName, ValidChecksum, ValidAccTable)

            Assert.AreEqual(ValidFileName, desc.FileName)
            Assert.AreEqual(ValidChecksum, desc.Checksum)
            Assert.AreEqual(ValidAccTable.Trim(), desc.AcceleratorTable)
        End Sub

        ''' <summary>
        ''' Verifies that leading and trailing white spaces are removed from all the stored values.
        ''' </summary>
        <TestMethod>
        Public Sub Constructor_ArgumentsWithSurroundingWhiteSpaces_StoresTrimmedValues()

            Dim desc As New MuiDescriptor($"  {ValidFileName} ", $" {ValidChecksum}  ", $"{ControlChars.Tab}{ValidAccTable}  ")

            Assert.AreEqual(ValidFileName, desc.FileName)
            Assert.AreEqual(ValidChecksum, desc.Checksum)
            Assert.AreEqual(ValidAccTable.Trim(), desc.AcceleratorTable)
        End Sub

        ''' <summary>
        ''' Verifies that the '.mui' extension is accepted regardless of its letter case.
        ''' </summary>
        '''
        ''' <param name="fileName">
        ''' The MUI file name.
        ''' </param>
        <TestMethod>
        <DataRow("shell32.dll.mui")>
        <DataRow("SHELL32.DLL.MUI")>
        <DataRow("wordpad.exe.Mui")>
        Public Sub Constructor_MuiExtensionInAnyCase_IsAccepted(fileName As String)

            Dim desc As New MuiDescriptor(fileName, ValidChecksum, ValidAccTable)

            Assert.AreEqual(fileName, desc.FileName)
        End Sub

        ''' <summary>
        ''' Verifies that hexadecimal checksums are accepted regardless of their letter case, and stored as-is.
        ''' </summary>
        '''
        ''' <param name="checksum">
        ''' The CRC-32 checksum.
        ''' </param>
        <TestMethod>
        <DataRow("0a2fad88")>
        <DataRow("0A2FAD88")>
        <DataRow("00000000")>
        <DataRow("FFFFFFFF")>
        <DataRow("aBcDeF12")>
        Public Sub Constructor_HexadecimalChecksumInAnyCase_IsAcceptedAndStoredAsIs(checksum As String)

            Dim desc As New MuiDescriptor(ValidFileName, checksum, ValidAccTable)

            Assert.AreEqual(checksum, desc.Checksum)
        End Sub

#End Region

#Region " Constructor: File Name Validations "

        ''' <summary>
        ''' Verifies that a null, empty or white-space file name throws <see cref="ArgumentNullException"/>.
        ''' </summary>
        '''
        ''' <param name="fileName">
        ''' The MUI file name.
        ''' </param>
        <TestMethod>
        <DataRow(Nothing, DisplayName:="Null")>
        <DataRow("", DisplayName:="Empty")>
        <DataRow("   ", DisplayName:="White spaces")>
        Public Sub Constructor_NullOrWhiteSpaceFileName_ThrowsArgumentNullException(fileName As String)

            Dim ex As ArgumentNullException = Assert.ThrowsExactly(Of ArgumentNullException)(
                Sub() MuiDescriptorTests.CreateDescriptor(fileName, ValidChecksum, ValidAccTable))

            Assert.AreEqual("muiFileName", ex.ParamName)
        End Sub

        ''' <summary>
        ''' Verifies that a file name without the '.mui' extension throws <see cref="ArgumentException"/>.
        ''' </summary>
        '''
        ''' <param name="fileName">
        ''' The MUI file name.
        ''' </param>
        <TestMethod>
        <DataRow("notepad.exe", DisplayName:="Other extension")>
        <DataRow("notepad", DisplayName:="No extension")>
        <DataRow("notepad.mui.bak", DisplayName:="Backup extension")>
        <DataRow("notepad.muix", DisplayName:="Similar extension")>
        <DataRow("notepad.", DisplayName:="Trailing dot")>
        <DataRow(".mui", DisplayName:="Extension only")>
        Public Sub Constructor_FileNameWithoutMuiExtension_ThrowsArgumentException(fileName As String)

            Dim ex As ArgumentException = Assert.ThrowsExactly(Of ArgumentException)(
                Sub() MuiDescriptorTests.CreateDescriptor(fileName, ValidChecksum, ValidAccTable))

            Assert.AreEqual("muiFileName", ex.ParamName)
        End Sub

        ''' <summary>
        ''' Verifies that a file name containing invalid file name characters
        ''' (including directory separators) throws <see cref="ArgumentException"/>.
        ''' </summary>
        '''
        ''' <param name="fileName">
        ''' The MUI file name.
        ''' </param>
        <TestMethod>
        <DataRow("note|pad.exe.mui")>
        <DataRow("note<pad.exe.mui")>
        <DataRow("note>pad.exe.mui")>
        <DataRow("note""pad.exe.mui")>
        <DataRow("note*pad.exe.mui")>
        <DataRow("note?pad.exe.mui")>
        <DataRow("C:notepad.exe.mui")>
        <DataRow("es-ES\notepad.exe.mui")>
        <DataRow("es-ES/notepad.exe.mui")>
        Public Sub Constructor_FileNameWithInvalidCharacters_ThrowsArgumentException(fileName As String)

            Dim ex As ArgumentException = Assert.ThrowsExactly(Of ArgumentException)(
                Sub() MuiDescriptorTests.CreateDescriptor(fileName, ValidChecksum, ValidAccTable))

            Assert.AreEqual("muiFileName", ex.ParamName)
        End Sub

#End Region

#Region " Constructor: Checksum Validations "

        ''' <summary>
        ''' Verifies that a null, empty or white-space checksum throws <see cref="ArgumentNullException"/>.
        ''' </summary>
        '''
        ''' <param name="checksum">
        ''' The CRC-32 checksum.
        ''' </param>
        <TestMethod>
        <DataRow(Nothing, DisplayName:="Null")>
        <DataRow("", DisplayName:="Empty")>
        <DataRow("        ", DisplayName:="Eight white spaces")>
        Public Sub Constructor_NullOrWhiteSpaceChecksum_ThrowsArgumentNullException(checksum As String)

            Dim ex As ArgumentNullException = Assert.ThrowsExactly(Of ArgumentNullException)(
                Sub() MuiDescriptorTests.CreateDescriptor(ValidFileName, checksum, ValidAccTable))

            Assert.AreEqual("checksum", ex.ParamName)
        End Sub

        ''' <summary>
        ''' Verifies that a checksum whose length is not exactly 8 characters throws <see cref="ArgumentException"/>.
        ''' </summary>
        '''
        ''' <param name="checksum">
        ''' The CRC-32 checksum.
        ''' </param>
        <TestMethod>
        <DataRow("0a2fad8", DisplayName:="7 characters")>
        <DataRow("0a2fad880", DisplayName:="9 characters")>
        <DataRow("0x0a2fad88", DisplayName:="Hex prefix")>
        Public Sub Constructor_ChecksumWithWrongLength_ThrowsArgumentException(checksum As String)

            Dim ex As ArgumentException = Assert.ThrowsExactly(Of ArgumentException)(
                Sub() MuiDescriptorTests.CreateDescriptor(ValidFileName, checksum, ValidAccTable))

            Assert.AreEqual("checksum", ex.ParamName)
        End Sub

        ''' <summary>
        ''' Regression test: the checksum length must be validated after trimming,
        ''' because the trimmed value is the one that is finally stored.
        ''' <para></para>
        ''' Previously, a 7-digit checksum padded with a white space passed the length validation
        ''' and was then stored as an invalid 7-digit checksum.
        ''' </summary>
        '''
        ''' <param name="checksum">
        ''' The CRC-32 checksum.
        ''' </param>
        <TestMethod>
        <DataRow(" 0a2fad8", DisplayName:="Leading white space")>
        <DataRow("0a2fad8 ", DisplayName:="Trailing white space")>
        <DataRow(" 0a2fad ", DisplayName:="Surrounding white spaces")>
        Public Sub Constructor_ShortChecksumPaddedWithWhiteSpaces_ThrowsArgumentException(checksum As String)

            Dim ex As ArgumentException = Assert.ThrowsExactly(Of ArgumentException)(
                Sub() MuiDescriptorTests.CreateDescriptor(ValidFileName, checksum, ValidAccTable))

            Assert.AreEqual("checksum", ex.ParamName)
        End Sub

        ''' <summary>
        ''' Regression test: a checksum containing non-hexadecimal characters throws <see cref="ArgumentException"/>.
        ''' </summary>
        '''
        ''' <param name="checksum">
        ''' The CRC-32 checksum.
        ''' </param>
        <TestMethod>
        <DataRow("zzzzzzzz")>
        <DataRow("0a2fad8g")>
        <DataRow("0a2fad8-")>
        <DataRow("0a2fad8.")>
        <DataRow("0a2f ad8", DisplayName:="Inner white space")>
        <DataRow("０a2fad88", DisplayName:="Full-width digit")>
        Public Sub Constructor_NonHexadecimalChecksum_ThrowsArgumentException(checksum As String)

            Dim ex As ArgumentException = Assert.ThrowsExactly(Of ArgumentException)(
                Sub() MuiDescriptorTests.CreateDescriptor(ValidFileName, checksum, ValidAccTable))

            Assert.AreEqual("checksum", ex.ParamName)
        End Sub

#End Region

#Region " Constructor: Accelerator Table Validations "

        ''' <summary>
        ''' Verifies that a null, empty or white-space accelerator table throws <see cref="ArgumentNullException"/>.
        ''' </summary>
        '''
        ''' <param name="accTable">
        ''' The accelerator table definition.
        ''' </param>
        <TestMethod>
        <DataRow(Nothing, DisplayName:="Null")>
        <DataRow("", DisplayName:="Empty")>
        <DataRow("   ", DisplayName:="White spaces")>
        Public Sub Constructor_NullOrWhiteSpaceAccTable_ThrowsArgumentNullException(accTable As String)

            Dim ex As ArgumentNullException = Assert.ThrowsExactly(Of ArgumentNullException)(
                Sub() MuiDescriptorTests.CreateDescriptor(ValidFileName, ValidChecksum, accTable))

            Assert.AreEqual("accTable", ex.ParamName)
        End Sub

        ''' <summary>
        ''' Verifies that an accelerator table containing the prohibited 'LANG_ENGLISH' token
        ''' (in any letter case) throws <see cref="ArgumentException"/>.
        ''' </summary>
        '''
        ''' <param name="languageToken">
        ''' The language token to inject.
        ''' </param>
        <TestMethod>
        <DataRow("LANG_ENGLISH")>
        <DataRow("lang_english")>
        <DataRow("Lang_English")>
        Public Sub Constructor_AccTableWithLangEnglishToken_ThrowsArgumentException(languageToken As String)

            Dim accTable As String = ValidAccTable.Replace("LANG_SPANISH, SUBLANG_SPANISH_MODERN", $"{languageToken}, SUBLANG_ENGLISH_US")

            Dim ex As ArgumentException = Assert.ThrowsExactly(Of ArgumentException)(
                Sub() MuiDescriptorTests.CreateDescriptor(ValidFileName, ValidChecksum, accTable))

            Assert.AreEqual("accTable", ex.ParamName)
        End Sub

        ''' <summary>
        ''' Verifies that an accelerator table missing any of the required tokens throws <see cref="ArgumentException"/>.
        ''' </summary>
        '''
        ''' <param name="removedToken">
        ''' The required token to remove from a valid accelerator table.
        ''' </param>
        <TestMethod>
        <DataRow("ACCELERATORS")>
        <DataRow("LANGUAGE")>
        <DataRow("{")>
        <DataRow("}")>
        Public Sub Constructor_AccTableMissingRequiredToken_ThrowsArgumentException(removedToken As String)

            Dim accTable As String = ValidAccTable.Replace(removedToken, String.Empty)

            Dim ex As ArgumentException = Assert.ThrowsExactly(Of ArgumentException)(
                Sub() MuiDescriptorTests.CreateDescriptor(ValidFileName, ValidChecksum, accTable))

            Assert.AreEqual("accTable", ex.ParamName)
        End Sub

#End Region

#Region " Equality Members "

        ''' <summary>
        ''' Verifies that two descriptors with the same values are equal.
        ''' </summary>
        <TestMethod>
        Public Sub Equals_SameValues_ReturnsTrue()

            Dim desc1 As New MuiDescriptor(ValidFileName, ValidChecksum, ValidAccTable)
            Dim desc2 As New MuiDescriptor(ValidFileName, ValidChecksum, ValidAccTable)

            Assert.IsTrue(desc1.Equals(desc2))
            Assert.IsTrue(desc2.Equals(desc1))
            Assert.IsTrue(desc1.Equals(DirectCast(desc2, Object)))
        End Sub

        ''' <summary>
        ''' Verifies that the equality comparison is case-insensitive.
        ''' </summary>
        <TestMethod>
        Public Sub Equals_ValuesDifferingOnlyInCase_ReturnsTrue()

            Dim desc1 As New MuiDescriptor(ValidFileName, ValidChecksum, ValidAccTable)
            Dim desc2 As New MuiDescriptor(ValidFileName.ToUpperInvariant(), ValidChecksum.ToUpperInvariant(), ValidAccTable.ToLowerInvariant())

            Assert.IsTrue(desc1.Equals(desc2))
        End Sub

        ''' <summary>
        ''' Verifies that the equality comparison ignores surrounding white spaces, because the stored values are trimmed.
        ''' </summary>
        <TestMethod>
        Public Sub Equals_ValuesDifferingOnlyInSurroundingWhiteSpaces_ReturnsTrue()

            Dim desc1 As New MuiDescriptor(ValidFileName, ValidChecksum, ValidAccTable)
            Dim desc2 As New MuiDescriptor($" {ValidFileName} ", $" {ValidChecksum} ", $"  {ValidAccTable}  ")

            Assert.IsTrue(desc1.Equals(desc2))
        End Sub

        ''' <summary>
        ''' Verifies that two descriptors differing in any property are not equal.
        ''' </summary>
        <TestMethod>
        Public Sub Equals_DifferentValues_ReturnsFalse()

            Dim baseDesc As New MuiDescriptor(ValidFileName, ValidChecksum, ValidAccTable)
            Dim otherFileName As New MuiDescriptor("wordpad.exe.mui", ValidChecksum, ValidAccTable)
            Dim otherChecksum As New MuiDescriptor(ValidFileName, "7a6b3ec1", ValidAccTable)
            Dim otherAccTable As New MuiDescriptor(ValidFileName, ValidChecksum, ValidAccTable.Replace("VK_A", "VK_E"))

            Assert.IsFalse(baseDesc.Equals(otherFileName), "File name difference was ignored.")
            Assert.IsFalse(baseDesc.Equals(otherChecksum), "Checksum difference was ignored.")
            Assert.IsFalse(baseDesc.Equals(otherAccTable), "Accelerator table difference was ignored.")
        End Sub

        ''' <summary>
        ''' Verifies that comparing to a null reference or to an object of another type returns <see langword="False"/>.
        ''' </summary>
        <TestMethod>
        Public Sub Equals_NullOrOtherType_ReturnsFalse()

            Dim desc As New MuiDescriptor(ValidFileName, ValidChecksum, ValidAccTable)

            Assert.IsFalse(desc.Equals(DirectCast(Nothing, MuiDescriptor)))
            Assert.IsFalse(desc.Equals(DirectCast(Nothing, Object)))
            Assert.IsFalse(desc.Equals(ValidFileName))
        End Sub

        ''' <summary>
        ''' Verifies that a descriptor is equal to itself.
        ''' </summary>
        <TestMethod>
        Public Sub Equals_SameInstance_ReturnsTrue()

            Dim desc As New MuiDescriptor(ValidFileName, ValidChecksum, ValidAccTable)

            Assert.IsTrue(desc.Equals(desc))
        End Sub

        ''' <summary>
        ''' Regression test: equal descriptors must return the same hash code (including when they differ only in case),
        ''' as required by the <see cref="Object.Equals(Object)"/> and <see cref="Object.GetHashCode()"/> contract.
        ''' </summary>
        <TestMethod>
        Public Sub GetHashCode_EqualDescriptors_ReturnSameHashCode()

            Dim desc1 As New MuiDescriptor(ValidFileName, ValidChecksum, ValidAccTable)
            Dim desc2 As New MuiDescriptor(ValidFileName, ValidChecksum, ValidAccTable)
            Dim desc3 As New MuiDescriptor(ValidFileName.ToUpperInvariant(), ValidChecksum.ToUpperInvariant(), ValidAccTable)

            Assert.IsTrue(desc1.Equals(desc3), "Precondition failed: descriptors must be equal.")
            Assert.AreEqual(desc1.GetHashCode(), desc2.GetHashCode())
            Assert.AreEqual(desc1.GetHashCode(), desc3.GetHashCode())
        End Sub

        ''' <summary>
        ''' Verifies that the hash code of a descriptor is stable across multiple calls.
        ''' </summary>
        <TestMethod>
        Public Sub GetHashCode_MultipleCalls_ReturnsSameValue()

            Dim desc As New MuiDescriptor(ValidFileName, ValidChecksum, ValidAccTable)

            Assert.AreEqual(desc.GetHashCode(), desc.GetHashCode())
        End Sub

        ''' <summary>
        ''' Regression test: hash-based collections must treat equal descriptors as the same element.
        ''' </summary>
        <TestMethod>
        Public Sub HashSet_EqualDescriptors_AreDeduplicated()

            Dim descriptors As New HashSet(Of MuiDescriptor) From {
                New MuiDescriptor(ValidFileName, ValidChecksum, ValidAccTable),
                New MuiDescriptor(ValidFileName.ToUpperInvariant(), ValidChecksum.ToUpperInvariant(), ValidAccTable),
                New MuiDescriptor($" {ValidFileName} ", ValidChecksum, ValidAccTable)
            }

            Assert.AreEqual(1, descriptors.Count)
            Assert.IsTrue(descriptors.Contains(New MuiDescriptor(ValidFileName, ValidChecksum, ValidAccTable)))
        End Sub

#End Region

#Region " Private Methods "

        ''' <summary>
        ''' Creates a new <see cref="MuiDescriptor"/> with the specified arguments.
        ''' <para></para>
        ''' Used to invoke the constructor from single-line lambda expressions.
        ''' </summary>
        '''
        ''' <param name="muiFileName">
        ''' The name of the MUI file.
        ''' </param>
        '''
        ''' <param name="checksum">
        ''' The CRC-32 checksum of the MUI file.
        ''' </param>
        '''
        ''' <param name="accTable">
        ''' The accelerator table associated with the MUI file.
        ''' </param>
        '''
        ''' <returns>
        ''' The resulting <see cref="MuiDescriptor"/>.
        ''' </returns>
        Private Shared Function CreateDescriptor(muiFileName As String, checksum As String, accTable As String) As MuiDescriptor

            Return New MuiDescriptor(muiFileName, checksum, accTable)
        End Function

#End Region

    End Class

End Namespace

#End Region
