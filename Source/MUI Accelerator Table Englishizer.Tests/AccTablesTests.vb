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

#Region " AccTablesTests "

Namespace MATE.Tests

    ''' <summary>
    ''' Unit tests that validate the syntax and content of every accelerator table definition
    ''' declared in the <see cref="AccTables"/> module.
    ''' <para></para>
    ''' A malformed definition would make the Resource Hacker compilation fail,
    ''' or worse, produce a broken MUI file that replaces a system file upon the next reboot.
    ''' </summary>
    <TestClass>
    Public NotInheritable Class AccTablesTests

#Region " Constants "

        ''' <summary>
        ''' The option flags allowed in an accelerator entry, as defined by the Win32 resource compiler syntax.
        ''' </summary>
        Private Shared ReadOnly AllowedFlags As String() = {"ASCII", "VIRTKEY", "NOINVERT", "ALT", "SHIFT", "CONTROL"}

        ''' <summary>
        ''' The expected <c>LANGUAGE</c> statement arguments for each culture token used in the constant names.
        ''' </summary>
        Private Shared ReadOnly ExpectedBlockLanguageByCulture As IReadOnlyDictionary(Of String, String) =
            New Dictionary(Of String, String)(StringComparer.Ordinal) From {
                {"esES", "LANG_SPANISH, SUBLANG_SPANISH_MODERN"}
            }

#End Region

#Region " Data Sources "

        ''' <summary>
        ''' Gets the data rows (constant name and value) of every accelerator table definition.
        ''' </summary>
        Public Shared ReadOnly Property AllTables As IEnumerable(Of Object())
            Get
                Return AccTablesCatalog.GetAllTablesAsDataRows()
            End Get
        End Property

#End Region

#Region " Catalog Tests "

        ''' <summary>
        ''' Verifies that the <see cref="AccTables"/> module declares accelerator table definitions.
        ''' </summary>
        <TestMethod>
        Public Sub AccTables_DeclaresDefinitions()

            Dim tables As SortedDictionary(Of String, String) = AccTablesCatalog.GetAllTables()

            Assert.AreEqual(11, tables.Count, $"Declared constants: {String.Join(", ", tables.Keys)}")
        End Sub

        ''' <summary>
        ''' Verifies that every constant name follows the <c>{MUIFILE}_{culture}_ENGLISHIZED_{crc32}</c> naming convention,
        ''' using a known MUI file prefix and a known culture token.
        ''' </summary>
        <TestMethod>
        Public Sub AccTables_AllConstantNames_FollowNamingConvention()

            For Each constantName As String In AccTablesCatalog.GetAllTables().Keys
                Dim filePrefix As String = Nothing
                Dim culture As String = Nothing
                Dim checksum As String = Nothing

                Assert.IsTrue(AccTablesCatalog.TryParseConstantName(constantName, filePrefix, culture, checksum),
                              $"Constant '{constantName}' does not follow the naming convention.")
                Assert.IsTrue(AccTablesCatalog.FilePrefixToMuiFileName.ContainsKey(filePrefix),
                              $"Constant '{constantName}' uses an unknown MUI file prefix: '{filePrefix}'.")
                Assert.IsTrue(AccTablesTests.ExpectedBlockLanguageByCulture.ContainsKey(culture),
                              $"Constant '{constantName}' uses an unknown culture token: '{culture}'.")
            Next constantName
        End Sub

        ''' <summary>
        ''' Verifies the documented aliases: definitions that intentionally share the same accelerator table.
        ''' </summary>
        '''
        ''' <param name="aliasConstantName">
        ''' The name of the alias constant.
        ''' </param>
        '''
        ''' <param name="sourceConstantName">
        ''' The name of the constant whose value is reused.
        ''' </param>
        <TestMethod>
        <DataRow("SHELL32_esES_ENGLISHIZED_3600a388", "SHELL32_esES_ENGLISHIZED_1f3defe7")>
        <DataRow("SHELL32_esES_ENGLISHIZED_bad33980", "SHELL32_esES_ENGLISHIZED_1f3defe7")>
        <DataRow("NOTEPAD_esES_ENGLISHIZED_eb176603", "NOTEPAD_esES_ENGLISHIZED_0a2fad88")>
        <DataRow("NOTEPAD_esES_ENGLISHIZED_7a6b3ec1", "NOTEPAD_esES_ENGLISHIZED_0a2fad88")>
        <DataRow("WORDPAD_esES_ENGLISHIZED_12548387", "WORDPAD_esES_ENGLISHIZED_075f2d95")>
        Public Sub AccTables_AliasDefinitions_ShareTheSameTable(aliasConstantName As String, sourceConstantName As String)

            Dim tables As SortedDictionary(Of String, String) = AccTablesCatalog.GetAllTables()

            Assert.AreEqual(tables(sourceConstantName), tables(aliasConstantName))
        End Sub

#End Region

#Region " Syntax Tests (Data-Driven, Every Definition) "

        ''' <summary>
        ''' Verifies that the definition is a structurally valid resource script with at least one non-empty block.
        ''' </summary>
        '''
        ''' <param name="constantName">
        ''' The constant name.
        ''' </param>
        '''
        ''' <param name="accTable">
        ''' The accelerator table definition.
        ''' </param>
        <TestMethod>
        <DynamicData(NameOf(AccTablesTests.AllTables))>
        Public Sub Definition_IsStructurallyValid(constantName As String, accTable As String)

            Dim script As AcceleratorScript = AcceleratorTableParser.Parse(accTable)

            Assert.IsTrue(script.Blocks.Count > 0, $"'{constantName}' has no ACCELERATORS blocks.")
            For Each block As AcceleratorBlock In script.Blocks
                Assert.IsTrue(block.Entries.Count > 0, $"'{constantName}' block '{block.Name}' has no entries.")
            Next block
        End Sub

        ''' <summary>
        ''' Verifies that the number of opening and closing braces matches.
        ''' </summary>
        '''
        ''' <param name="constantName">
        ''' The constant name.
        ''' </param>
        '''
        ''' <param name="accTable">
        ''' The accelerator table definition.
        ''' </param>
        <TestMethod>
        <DynamicData(NameOf(AccTablesTests.AllTables))>
        Public Sub Definition_HasBalancedBraces(constantName As String, accTable As String)

            Dim openCount As Integer = accTable.Count(Function(c As Char) c = "{"c)
            Dim closeCount As Integer = accTable.Count(Function(c As Char) c = "}"c)
            Dim blockCount As Integer = Regex.Matches(accTable, "\bACCELERATORS\b").Count

            Assert.AreEqual(openCount, closeCount, $"'{constantName}' has unbalanced braces.")
            Assert.AreEqual(blockCount, openCount, $"'{constantName}' must have exactly one brace pair per ACCELERATORS block.")
        End Sub

        ''' <summary>
        ''' Verifies that the definition starts with a neutral leading <c>LANGUAGE</c> statement.
        ''' </summary>
        '''
        ''' <param name="constantName">
        ''' The constant name.
        ''' </param>
        '''
        ''' <param name="accTable">
        ''' The accelerator table definition.
        ''' </param>
        <TestMethod>
        <DynamicData(NameOf(AccTablesTests.AllTables))>
        Public Sub Definition_LeadingLanguageIsNeutral(constantName As String, accTable As String)

            Dim script As AcceleratorScript = AcceleratorTableParser.Parse(accTable)

            Assert.AreEqual("LANG_NEUTRAL, SUBLANG_NEUTRAL", script.LeadingLanguage, $"'{constantName}' has an unexpected leading LANGUAGE statement.")
        End Sub

        ''' <summary>
        ''' Verifies that every block keeps the language of the localized MUI file it replaces
        ''' (only the key mappings are "Englishized", not the resource language),
        ''' so the <c>-mask ACCELERATORS,,{LCID}</c> argument passed to Resource Hacker matches the compiled resource.
        ''' </summary>
        '''
        ''' <param name="constantName">
        ''' The constant name.
        ''' </param>
        '''
        ''' <param name="accTable">
        ''' The accelerator table definition.
        ''' </param>
        <TestMethod>
        <DynamicData(NameOf(AccTablesTests.AllTables))>
        Public Sub Definition_EveryBlockTargetsTheCultureLanguage(constantName As String, accTable As String)

            Dim filePrefix As String = Nothing
            Dim culture As String = Nothing
            Dim checksum As String = Nothing
            Assert.IsTrue(AccTablesCatalog.TryParseConstantName(constantName, filePrefix, culture, checksum),
                          $"Precondition failed: '{constantName}' does not follow the naming convention.")
            Dim expectedLanguage As String = AccTablesTests.ExpectedBlockLanguageByCulture(culture)

            Dim script As AcceleratorScript = AcceleratorTableParser.Parse(accTable)

            For Each block As AcceleratorBlock In script.Blocks
                Assert.AreEqual(expectedLanguage, block.Language, $"'{constantName}' block '{block.Name}' targets an unexpected language.")
            Next block
        End Sub

        ''' <summary>
        ''' Verifies that the definition does not contain the prohibited 'LANG_ENGLISH' token.
        ''' </summary>
        '''
        ''' <param name="constantName">
        ''' The constant name.
        ''' </param>
        '''
        ''' <param name="accTable">
        ''' The accelerator table definition.
        ''' </param>
        <TestMethod>
        <DynamicData(NameOf(AccTablesTests.AllTables))>
        Public Sub Definition_DoesNotContainLangEnglishToken(constantName As String, accTable As String)

            Assert.AreEqual(-1, accTable.IndexOf("LANG_ENGLISH", StringComparison.OrdinalIgnoreCase),
                            $"'{constantName}' contains the prohibited 'LANG_ENGLISH' token.")
        End Sub

        ''' <summary>
        ''' Verifies that the block names (resource identifiers) are unique within the definition.
        ''' </summary>
        '''
        ''' <param name="constantName">
        ''' The constant name.
        ''' </param>
        '''
        ''' <param name="accTable">
        ''' The accelerator table definition.
        ''' </param>
        <TestMethod>
        <DynamicData(NameOf(AccTablesTests.AllTables))>
        Public Sub Definition_BlockNamesAreUnique(constantName As String, accTable As String)

            Dim script As AcceleratorScript = AcceleratorTableParser.Parse(accTable)

            Dim duplicates As String() = script.Blocks.GroupBy(Function(b As AcceleratorBlock) b.Name, StringComparer.OrdinalIgnoreCase).
                                                       Where(Function(g As IGrouping(Of String, AcceleratorBlock)) g.Count() > 1).
                                                       Select(Function(g As IGrouping(Of String, AcceleratorBlock)) g.Key).
                                                       ToArray()

            Assert.AreEqual(0, duplicates.Length, $"'{constantName}' has duplicated block names: {String.Join(", ", duplicates)}")
        End Sub

        ''' <summary>
        ''' Verifies that every entry has a valid key token, a valid 16-bit command identifier,
        ''' and a valid, non-repeated set of option flags.
        ''' </summary>
        '''
        ''' <param name="constantName">
        ''' The constant name.
        ''' </param>
        '''
        ''' <param name="accTable">
        ''' The accelerator table definition.
        ''' </param>
        <TestMethod>
        <DynamicData(NameOf(AccTablesTests.AllTables))>
        Public Sub Definition_EntriesHaveValidKeysIdsAndFlags(constantName As String, accTable As String)

            Dim script As AcceleratorScript = AcceleratorTableParser.Parse(accTable)

            For Each block As AcceleratorBlock In script.Blocks
                For Each entry As AcceleratorEntry In block.Entries
                    Dim context As String = $"'{constantName}' block '{block.Name}' entry '{entry}'"

                    Dim isVirtualKey As Boolean = Regex.IsMatch(entry.Key, "^VK_[A-Z0-9_]+$")
                    Dim isQuotedChar As Boolean = Regex.IsMatch(entry.Key, "^""\^?[^""]""$")
                    Dim isNumericKey As Boolean = Regex.IsMatch(entry.Key, "^\d+$")
                    Assert.IsTrue(isVirtualKey OrElse isQuotedChar OrElse isNumericKey, $"{context}: invalid key token.")

                    Assert.IsTrue(entry.CommandId >= 0 AndAlso entry.CommandId <= UShort.MaxValue, $"{context}: command identifier out of 16-bit range.")

                    For Each flag As String In entry.Flags
                        Assert.IsTrue(AccTablesTests.AllowedFlags.Contains(flag), $"{context}: unknown flag '{flag}'.")
                    Next flag
                    Assert.AreEqual(entry.Flags.Count, entry.Flags.Distinct(StringComparer.Ordinal).Count(), $"{context}: repeated flags.")

                    Dim hasVirtKey As Boolean = entry.Flags.Contains("VIRTKEY")
                    Assert.IsFalse(hasVirtKey AndAlso entry.Flags.Contains("ASCII"), $"{context}: VIRTKEY and ASCII are mutually exclusive.")
                    If isVirtualKey Then
                        Assert.IsTrue(hasVirtKey, $"{context}: virtual-key codes require the VIRTKEY flag.")
                    End If
                    If Not hasVirtKey Then
                        Assert.IsFalse(entry.Flags.Contains("CONTROL") OrElse entry.Flags.Contains("SHIFT"),
                                       $"{context}: CONTROL and SHIFT flags require the VIRTKEY flag.")
                    End If
                Next entry
            Next block
        End Sub

        ''' <summary>
        ''' Verifies that the definition passes the validations of the <see cref="MuiDescriptor"/> constructor.
        ''' </summary>
        '''
        ''' <param name="constantName">
        ''' The constant name.
        ''' </param>
        '''
        ''' <param name="accTable">
        ''' The accelerator table definition.
        ''' </param>
        <TestMethod>
        <DynamicData(NameOf(AccTablesTests.AllTables))>
        Public Sub Definition_IsAcceptedByMuiDescriptor(constantName As String, accTable As String)

            Dim filePrefix As String = Nothing
            Dim culture As String = Nothing
            Dim checksum As String = Nothing
            Assert.IsTrue(AccTablesCatalog.TryParseConstantName(constantName, filePrefix, culture, checksum),
                          $"Precondition failed: '{constantName}' does not follow the naming convention.")

            Dim desc As New MuiDescriptor(AccTablesCatalog.FilePrefixToMuiFileName(filePrefix), checksum, accTable)

            Assert.AreEqual(accTable.Trim(), desc.AcceleratorTable)
        End Sub

#End Region

#Region " Content Tests (Englishized Shortcuts) "

        ''' <summary>
        ''' Verifies that representative shortcuts are mapped to their standard English (en-US) keys
        ''' instead of the localized Spanish keys (e.g. Find is <c>Ctrl+F</c> instead of <c>Ctrl+B</c>,
        ''' Select All is <c>Ctrl+A</c> instead of <c>Ctrl+E</c>, Bold is <c>Ctrl+B</c> instead of <c>Ctrl+N</c>).
        ''' <para></para>
        ''' The lookup uses the first matching entry of the block, because that is the effective mapping:
        ''' <c>TranslateAccelerator</c> stops at the first entry that matches the pressed key combination,
        ''' so any later duplicate of the same shortcut is ignored by Windows.
        ''' </summary>
        '''
        ''' <param name="constantName">
        ''' The constant name.
        ''' </param>
        '''
        ''' <param name="blockName">
        ''' The block name.
        ''' </param>
        '''
        ''' <param name="key">
        ''' The key token.
        ''' </param>
        '''
        ''' <param name="modifiers">
        ''' The normalized modifiers (see <see cref="AcceleratorEntry.Modifiers"/>).
        ''' </param>
        '''
        ''' <param name="expectedCommandId">
        ''' The expected command identifier.
        ''' </param>
        <TestMethod>
        <DataRow("NOTEPAD_esES_ENGLISHIZED_0a2fad88", "GLOBALACC", "VK_F", "CONTROL", 21, DisplayName:="Notepad: Ctrl+F = Find")>
        <DataRow("NOTEPAD_esES_ENGLISHIZED_0a2fad88", "GLOBALACC", "VK_H", "CONTROL", 23, DisplayName:="Notepad: Ctrl+H = Replace")>
        <DataRow("NOTEPAD_esES_ENGLISHIZED_0a2fad88", "GLOBALACC", "VK_G", "CONTROL", 24, DisplayName:="Notepad: Ctrl+G = Go To")>
        <DataRow("NOTEPAD_esES_ENGLISHIZED_0a2fad88", "GLOBALACC", "VK_N", "CONTROL", 1, DisplayName:="Notepad: Ctrl+N = New")>
        <DataRow("NOTEPAD_esES_ENGLISHIZED_0a2fad88", "MAINACC", "VK_A", "CONTROL", 25, DisplayName:="Notepad: Ctrl+A = Select All")>
        <DataRow("NOTEPAD_esES_ENGLISHIZED_0a2fad88", "SLIPUPACC", "VK_C", "CONTROL", 769, DisplayName:="Notepad (SLIPUPACC): Ctrl+C = Copy takes precedence over later duplicate")>
        <DataRow("NOTEPAD_esES_ENGLISHIZED_0a2fad88", "SLIPUPACC", "VK_Z", "CONTROL", 16, DisplayName:="Notepad (SLIPUPACC): Ctrl+Z = Undo takes precedence over later duplicate")>
        <DataRow("SHELL32_esES_ENGLISHIZED_1f3defe7", "1", "VK_A", "CONTROL", 28705, DisplayName:="Shell32 (1f3defe7): Ctrl+A = Select All")>
        <DataRow("SHELL32_esES_ENGLISHIZED_fb5e8156", "1", "VK_A", "CONTROL", 28705, DisplayName:="Shell32 (fb5e8156): Ctrl+A = Select All")>
        <DataRow("SHELL32_esES_ENGLISHIZED_f7d515d8", "1", "VK_A", "CONTROL", 28707, DisplayName:="Shell32 (f7d515d8): Ctrl+A = Select All")>
        <DataRow("SHELL32_esES_ENGLISHIZED_1f3defe7", "1", "VK_N", "CONTROL", 28933, DisplayName:="Shell32 (1f3defe7): Ctrl+N = New Window")>
        <DataRow("WORDPAD_esES_ENGLISHIZED_075f2d95", "5", "VK_B", "CONTROL", 32799, DisplayName:="Wordpad (075f2d95): Ctrl+B = Bold")>
        <DataRow("WORDPAD_esES_ENGLISHIZED_075f2d95", "5", "VK_I", "CONTROL", 32800, DisplayName:="Wordpad (075f2d95): Ctrl+I = Italic")>
        <DataRow("WORDPAD_esES_ENGLISHIZED_075f2d95", "5", "VK_U", "CONTROL", 32802, DisplayName:="Wordpad (075f2d95): Ctrl+U = Underline")>
        <DataRow("WORDPAD_esES_ENGLISHIZED_075f2d95", "128", "VK_N", "CONTROL", 57600, DisplayName:="Wordpad (075f2d95): Ctrl+N = New")>
        <DataRow("WORDPAD_esES_ENGLISHIZED_845a4685", "5", "VK_B", "CONTROL", 32799, DisplayName:="Wordpad (845a4685): Ctrl+B = Bold")>
        <DataRow("WORDPAD_esES_ENGLISHIZED_845a4685", "128", "VK_F", "CONTROL", 57636, DisplayName:="Wordpad (845a4685): Ctrl+F = Find")>
        <DataRow("WORDPAD_esES_ENGLISHIZED_845a4685", "128", "VK_A", "CONTROL", 57642, DisplayName:="Wordpad (845a4685): Ctrl+A = Select All")>
        Public Sub Definition_MapsStandardEnglishShortcut(constantName As String, blockName As String,
                                                          key As String, modifiers As String,
                                                          expectedCommandId As Integer)

            Dim script As AcceleratorScript = AcceleratorTableParser.Parse(AccTablesCatalog.GetAllTables()(constantName))
            Dim block As AcceleratorBlock = script.Blocks.SingleOrDefault(Function(b As AcceleratorBlock) b.Name = blockName)
            Assert.IsNotNull(block, $"Block '{blockName}' not found in '{constantName}'.")

            Dim entry As AcceleratorEntry = block.Entries.FirstOrDefault(Function(e As AcceleratorEntry) e.Key = key AndAlso e.Modifiers = modifiers)

            Assert.IsNotNull(entry, $"Shortcut '{modifiers}+{key}' not found in '{constantName}' block '{blockName}'.")
            Assert.AreEqual(expectedCommandId, entry.CommandId)
        End Sub

#End Region

    End Class

End Namespace

#End Region
