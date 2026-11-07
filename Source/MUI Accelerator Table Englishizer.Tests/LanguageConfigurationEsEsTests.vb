#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.Generic
Imports System.Linq
Imports System.Text.RegularExpressions

Imports MATE.Tests.Helpers

#End Region

#Region " LanguageConfigurationEsEsTests "

Namespace MATE.Tests

    ''' <summary>
    ''' Unit tests for the <see cref="LanguageConfiguration_esES"/> class (Spanish - Spain MUI language configuration).
    ''' </summary>
    <TestClass>
    Public NotInheritable Class LanguageConfigurationEsEsTests

#Region " Culture "

        ''' <summary>
        ''' Verifies that the configuration targets the Spanish (Spain) culture.
        ''' </summary>
        <TestMethod>
        Public Sub CultureInfo_IsSpanishSpain()

            Dim config As New LanguageConfiguration_esES()

            Assert.AreEqual("es-ES", config.CultureInfo.Name)
            Assert.AreEqual("es", config.CultureInfo.TwoLetterISOLanguageName)
        End Sub

        ''' <summary>
        ''' Verifies that the culture LCID is 3082 (0x0C0A, <c>LANG_SPANISH</c> + <c>SUBLANG_SPANISH_MODERN</c>).
        ''' <para></para>
        ''' This LCID is passed to Resource Hacker in the <c>-mask ACCELERATORS,,{LCID}</c> argument,
        ''' so it must match the <c>LANGUAGE</c> statement of the accelerator tables.
        ''' </summary>
        <TestMethod>
        Public Sub CultureInfo_LcidMatchesSpanishModernSortLanguage()

            Const langSpanish As Integer = &HA
            Const sublangSpanishModern As Integer = &H3
            Const expectedLcid As Integer = (sublangSpanishModern << 10) Or langSpanish ' MAKELANGID = 3082 (0x0C0A)

            Dim config As New LanguageConfiguration_esES()

            Assert.AreEqual(expectedLcid, config.CultureInfo.LCID)
        End Sub

#End Region

#Region " MUI Descriptors "

        ''' <summary>
        ''' Verifies the number of descriptors registered per MUI file.
        ''' </summary>
        '''
        ''' <param name="muiFileName">
        ''' The MUI file name.
        ''' </param>
        '''
        ''' <param name="expectedCount">
        ''' The expected number of descriptors (one per known file variant).
        ''' </param>
        <TestMethod>
        <DataRow("shell32.dll.mui", 5)>
        <DataRow("notepad.exe.mui", 3)>
        <DataRow("wordpad.exe.mui", 3)>
        Public Sub MuiDescriptors_ContainsExpectedVariantsPerFile(muiFileName As String, expectedCount As Integer)

            Dim config As New LanguageConfiguration_esES()

            Dim actualCount As Integer = config.MuiDescriptors.Where(Function(d As MuiDescriptor) d.FileName.Equals(muiFileName, StringComparison.OrdinalIgnoreCase)).Count()

            Assert.AreEqual(expectedCount, actualCount)
        End Sub

        ''' <summary>
        ''' Verifies that the configuration only targets the supported MUI files.
        ''' </summary>
        <TestMethod>
        Public Sub MuiDescriptors_OnlyTargetSupportedFiles()

            Dim supportedFiles As New HashSet(Of String)(AccTablesCatalog.FilePrefixToMuiFileName.Values, StringComparer.OrdinalIgnoreCase)
            Dim config As New LanguageConfiguration_esES()

            Assert.AreEqual(11, config.MuiDescriptors.Count)
            For Each desc As MuiDescriptor In config.MuiDescriptors
                Assert.IsTrue(supportedFiles.Contains(desc.FileName), $"Unsupported MUI file: {desc.FileName}")
            Next desc
        End Sub

        ''' <summary>
        ''' Verifies that the checksums are unique (case-insensitive).
        ''' <para></para>
        ''' The application selects the first descriptor whose checksum matches a found MUI file,
        ''' so a duplicated checksum would silently shadow another definition.
        ''' </summary>
        <TestMethod>
        Public Sub MuiDescriptors_ChecksumsAreUnique()

            Dim config As New LanguageConfiguration_esES()

            Dim duplicates As String() = config.MuiDescriptors.GroupBy(Function(d As MuiDescriptor) d.Checksum, StringComparer.OrdinalIgnoreCase).
                                                               Where(Function(g As IGrouping(Of String, MuiDescriptor)) g.Count() > 1).
                                                               Select(Function(g As IGrouping(Of String, MuiDescriptor)) g.Key).
                                                               ToArray()

            Assert.AreEqual(0, duplicates.Length, $"Duplicated checksums: {String.Join(", ", duplicates)}")
        End Sub

        ''' <summary>
        ''' Verifies that every checksum is an 8-digit lowercase hexadecimal string, as written in the constant names.
        ''' </summary>
        <TestMethod>
        Public Sub MuiDescriptors_ChecksumsAreLowercaseHexadecimal()

            Dim config As New LanguageConfiguration_esES()

            For Each desc As MuiDescriptor In config.MuiDescriptors
                Assert.IsTrue(Regex.IsMatch(desc.Checksum, "^[0-9a-f]{8}$"), $"Invalid checksum format: '{desc.Checksum}'.")
            Next desc
        End Sub

        ''' <summary>
        ''' Verifies that every descriptor uses the accelerator table constant whose name matches
        ''' its MUI file and checksum (e.g. <c>shell32.dll.mui</c> + <c>1f3defe7</c> → <c>SHELL32_esES_ENGLISHIZED_1f3defe7</c>).
        ''' <para></para>
        ''' This detects copy-paste mistakes where a descriptor is linked to the table of another file or variant.
        ''' </summary>
        <TestMethod>
        Public Sub MuiDescriptors_UseTheMatchingAccTableConstant()

            Dim tables As SortedDictionary(Of String, String) = AccTablesCatalog.GetAllTables()
            Dim config As New LanguageConfiguration_esES()

            For Each desc As MuiDescriptor In config.MuiDescriptors
                Dim filePrefix As String = AccTablesCatalog.FilePrefixToMuiFileName.
                                           Single(Function(kvp As KeyValuePair(Of String, String)) kvp.Value.Equals(desc.FileName, StringComparison.OrdinalIgnoreCase)).
                                           Key
                Dim expectedConstantName As String = $"{filePrefix}_esES_ENGLISHIZED_{desc.Checksum}"

                Assert.IsTrue(tables.ContainsKey(expectedConstantName), $"Missing accelerator table constant: {expectedConstantName}")
                Assert.AreEqual(tables(expectedConstantName).Trim(), desc.AcceleratorTable, $"Descriptor {desc.FileName} [{desc.Checksum}] is not linked to {expectedConstantName}.")
            Next desc
        End Sub

        ''' <summary>
        ''' Verifies that every es-ES accelerator table constant is registered in the configuration (no orphan definitions).
        ''' </summary>
        <TestMethod>
        Public Sub MuiDescriptors_RegisterEveryEsEsAccTableConstant()

            Dim config As New LanguageConfiguration_esES()
            Dim registered As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            For Each desc As MuiDescriptor In config.MuiDescriptors
                registered.Add(desc.Checksum)
            Next desc

            For Each constantName As String In AccTablesCatalog.GetAllTables().Keys
                Dim filePrefix As String = Nothing
                Dim culture As String = Nothing
                Dim checksum As String = Nothing
                If AccTablesCatalog.TryParseConstantName(constantName, filePrefix, culture, checksum) AndAlso culture = "esES" Then
                    Assert.IsTrue(registered.Contains(checksum), $"Constant '{constantName}' is not registered in {NameOf(LanguageConfiguration_esES)}.")
                End If
            Next constantName
        End Sub

#End Region

#Region " Equality Members "

        ''' <summary>
        ''' Verifies that two instances are equal and return the same hash code.
        ''' </summary>
        <TestMethod>
        Public Sub Equals_TwoInstances_AreEqualWithSameHashCode()

            Dim config1 As New LanguageConfiguration_esES()
            Dim config2 As New LanguageConfiguration_esES()

            Assert.AreNotSame(config1, config2)
            Assert.IsTrue(config1.Equals(config2))
            Assert.AreEqual(config1.GetHashCode(), config2.GetHashCode())
        End Sub

#End Region

    End Class

End Namespace

#End Region
