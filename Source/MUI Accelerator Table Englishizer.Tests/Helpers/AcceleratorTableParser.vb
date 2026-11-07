#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.Diagnostics
Imports System.Globalization
Imports System.Linq

#End Region

#Region " AcceleratorTableParser "

Namespace MATE.Tests.Helpers

#Region " AcceleratorEntry "

    ''' <summary>
    ''' Represents a single key entry of an <c>ACCELERATORS</c> resource block.
    ''' </summary>
    Friend NotInheritable Class AcceleratorEntry

        ''' <summary>
        ''' Gets the key token (e.g. <c>VK_F1</c>, or a quoted character such as <c>"s"</c>).
        ''' </summary>
        Friend ReadOnly Property Key As String

        ''' <summary>
        ''' Gets the command identifier sent when the accelerator is pressed.
        ''' </summary>
        Friend ReadOnly Property CommandId As Integer

        ''' <summary>
        ''' Gets the option flags (e.g. <c>CONTROL</c>, <c>SHIFT</c>, <c>VIRTKEY</c>), in their original order.
        ''' </summary>
        Friend ReadOnly Property Flags As ReadOnlyCollection(Of String)

        ''' <summary>
        ''' Gets the original, trimmed source line of this entry.
        ''' </summary>
        Friend ReadOnly Property RawLine As String

        ''' <summary>
        ''' Gets the set of modifier flags (<c>CONTROL</c>, <c>SHIFT</c>, <c>ALT</c>) of this entry,
        ''' as a normalized, sorted and plus-sign-separated string (e.g. <c>CONTROL+SHIFT</c>).
        ''' <para></para>
        ''' Returns an empty string when the entry has no modifiers.
        ''' </summary>
        Friend ReadOnly Property Modifiers As String
            Get
                Dim modifierNames As String() = {"ALT", "CONTROL", "SHIFT"}
                Return String.Join("+", Me.Flags.Where(Function(f As String) modifierNames.Contains(f)).
                                                  OrderBy(Function(f As String) f, StringComparer.Ordinal))
            End Get
        End Property

        ''' <summary>
        ''' Initializes a new instance of the <see cref="AcceleratorEntry"/> class.
        ''' </summary>
        '''
        ''' <param name="key">
        ''' The key token.
        ''' </param>
        '''
        ''' <param name="commandId">
        ''' The command identifier.
        ''' </param>
        '''
        ''' <param name="flags">
        ''' The option flags.
        ''' </param>
        '''
        ''' <param name="rawLine">
        ''' The original source line.
        ''' </param>
        Friend Sub New(key As String, commandId As Integer, flags As IList(Of String), rawLine As String)

            Me.Key = key
            Me.CommandId = commandId
            Me.Flags = New ReadOnlyCollection(Of String)(flags)
            Me.RawLine = rawLine
        End Sub

        ''' <summary>
        ''' Returns the original source line of this entry.
        ''' </summary>
        '''
        ''' <returns>
        ''' The original source line.
        ''' </returns>
        Public Overrides Function ToString() As String

            Return Me.RawLine
        End Function

    End Class

#End Region

#Region " AcceleratorBlock "

    ''' <summary>
    ''' Represents a named <c>ACCELERATORS</c> resource block of a resource script.
    ''' </summary>
    Friend NotInheritable Class AcceleratorBlock

        ''' <summary>
        ''' Gets the resource name or ordinal of the block (e.g. <c>1</c>, <c>GLOBALACC</c>).
        ''' </summary>
        Friend ReadOnly Property Name As String

        ''' <summary>
        ''' Gets the arguments of the <c>LANGUAGE</c> statement of the block (e.g. <c>LANG_SPANISH, SUBLANG_SPANISH_MODERN</c>).
        ''' </summary>
        Friend ReadOnly Property Language As String

        ''' <summary>
        ''' Gets the key entries of the block.
        ''' </summary>
        Friend ReadOnly Property Entries As ReadOnlyCollection(Of AcceleratorEntry)

        ''' <summary>
        ''' Initializes a new instance of the <see cref="AcceleratorBlock"/> class.
        ''' </summary>
        '''
        ''' <param name="name">
        ''' The resource name or ordinal of the block.
        ''' </param>
        '''
        ''' <param name="language">
        ''' The arguments of the <c>LANGUAGE</c> statement.
        ''' </param>
        '''
        ''' <param name="entries">
        ''' The key entries.
        ''' </param>
        Friend Sub New(name As String, language As String, entries As IList(Of AcceleratorEntry))

            Me.Name = name
            Me.Language = language
            Me.Entries = New ReadOnlyCollection(Of AcceleratorEntry)(entries)
        End Sub

        ''' <summary>
        ''' Returns the resource name of this block.
        ''' </summary>
        '''
        ''' <returns>
        ''' The resource name of this block.
        ''' </returns>
        Public Overrides Function ToString() As String

            Return Me.Name
        End Function

    End Class

#End Region

#Region " AcceleratorScript "

    ''' <summary>
    ''' Represents a parsed resource script (*.rc) made of <c>ACCELERATORS</c> resource blocks.
    ''' </summary>
    Friend NotInheritable Class AcceleratorScript

        ''' <summary>
        ''' Gets the arguments of the leading <c>LANGUAGE</c> statement that precedes
        ''' the first block (e.g. <c>LANG_NEUTRAL, SUBLANG_NEUTRAL</c>),
        ''' or <see langword="Nothing"/> if the script has no leading statement.
        ''' </summary>
        Friend ReadOnly Property LeadingLanguage As String

        ''' <summary>
        ''' Gets the <c>ACCELERATORS</c> blocks of the script, in their original order.
        ''' </summary>
        Friend ReadOnly Property Blocks As ReadOnlyCollection(Of AcceleratorBlock)

        ''' <summary>
        ''' Initializes a new instance of the <see cref="AcceleratorScript"/> class.
        ''' </summary>
        '''
        ''' <param name="leadingLanguage">
        ''' The arguments of the leading <c>LANGUAGE</c> statement, or <see langword="Nothing"/>.
        ''' </param>
        '''
        ''' <param name="blocks">
        ''' The <c>ACCELERATORS</c> blocks.
        ''' </param>
        Friend Sub New(leadingLanguage As String, blocks As IList(Of AcceleratorBlock))

            Me.LeadingLanguage = leadingLanguage
            Me.Blocks = New ReadOnlyCollection(Of AcceleratorBlock)(blocks)
        End Sub

    End Class

#End Region

#Region " AcceleratorTableParser "

    ''' <summary>
    ''' Provides a strict, minimal parser for resource scripts (*.rc) made of <c>ACCELERATORS</c> resource blocks,
    ''' as written in the <c>AccTables</c> module of the application.
    ''' </summary>
    '''
    ''' <remarks>
    ''' The parser is intentionally strict: any structural deviation throws a <see cref="FormatException"/>,
    ''' so tests fail loudly when an accelerator table definition is malformed.
    ''' </remarks>
    Friend NotInheritable Class AcceleratorTableParser

        ''' <summary>
        ''' The keyword that identifies an accelerators resource block header.
        ''' </summary>
        Private Const AcceleratorsKeyword As String = "ACCELERATORS"

        ''' <summary>
        ''' The keyword that identifies a language statement.
        ''' </summary>
        Private Const LanguageKeyword As String = "LANGUAGE"

        ''' <summary>
        ''' Prevents a default instance of the <see cref="AcceleratorTableParser"/> class from being created.
        ''' </summary>
        <DebuggerNonUserCode>
        Private Sub New()
        End Sub

        ''' <summary>
        ''' Parses the specified resource script.
        ''' </summary>
        '''
        ''' <param name="script">
        ''' The resource script content.
        ''' </param>
        '''
        ''' <returns>
        ''' The resulting <see cref="AcceleratorScript"/>.
        ''' </returns>
        '''
        ''' <exception cref="ArgumentNullException">
        ''' Thrown when <paramref name="script"/> is null.
        ''' </exception>
        '''
        ''' <exception cref="FormatException">
        ''' Thrown when the script is structurally malformed.
        ''' </exception>
        Friend Shared Function Parse(script As String) As AcceleratorScript

            If script Is Nothing Then
                Throw New ArgumentNullException(NameOf(script))
            End If

            Dim lines As String() = script.Split({ControlChars.CrLf, ControlChars.Lf, ControlChars.Cr}, StringSplitOptions.None)

            Dim leadingLanguage As String = Nothing
            Dim blocks As New List(Of AcceleratorBlock)()

            Dim index As Integer = 0
            While index < lines.Length
                Dim line As String = lines(index).Trim()
                index += 1

                If line.Length = 0 Then
                    Continue While
                End If

                If AcceleratorTableParser.IsLanguageStatement(line) Then
                    If blocks.Count > 0 OrElse leadingLanguage IsNot Nothing Then
                        Throw New FormatException($"Unexpected standalone LANGUAGE statement: '{line}'.")
                    End If
                    leadingLanguage = AcceleratorTableParser.GetLanguageArguments(line)
                    Continue While
                End If

                Dim headerParts As String() = line.Split({" "c, ControlChars.Tab}, StringSplitOptions.RemoveEmptyEntries)
                If headerParts.Length <> 2 OrElse
                   Not headerParts(1).Equals(AcceleratorTableParser.AcceleratorsKeyword, StringComparison.Ordinal) Then
                    Throw New FormatException($"Expected an '<name> ACCELERATORS' block header, but found: '{line}'.")
                End If
                Dim blockName As String = headerParts(0)

                ' LANGUAGE statement of the block.
                Dim languageLine As String = AcceleratorTableParser.ReadNextNonEmptyLine(lines, index)
                If languageLine Is Nothing OrElse Not AcceleratorTableParser.IsLanguageStatement(languageLine) Then
                    Throw New FormatException($"Block '{blockName}' must be followed by a LANGUAGE statement.")
                End If
                Dim blockLanguage As String = AcceleratorTableParser.GetLanguageArguments(languageLine)

                ' Opening brace.
                Dim openBraceLine As String = AcceleratorTableParser.ReadNextNonEmptyLine(lines, index)
                If openBraceLine Is Nothing OrElse Not openBraceLine.Equals("{", StringComparison.Ordinal) Then
                    Throw New FormatException($"Block '{blockName}' must open with a '{{' line after its LANGUAGE statement.")
                End If

                ' Entries until the closing brace.
                Dim entries As New List(Of AcceleratorEntry)()
                Dim isClosed As Boolean = False
                While index < lines.Length
                    Dim entryLine As String = lines(index).Trim()
                    index += 1

                    If entryLine.Length = 0 Then
                        Continue While
                    End If
                    If entryLine.Equals("}", StringComparison.Ordinal) Then
                        isClosed = True
                        Exit While
                    End If
                    If entryLine.Contains("{") OrElse entryLine.Contains("}") Then
                        Throw New FormatException($"Unexpected brace inside block '{blockName}': '{entryLine}'.")
                    End If

                    entries.Add(AcceleratorTableParser.ParseEntry(entryLine, blockName))
                End While

                If Not isClosed Then
                    Throw New FormatException($"Block '{blockName}' is not closed with a '}}' line.")
                End If

                blocks.Add(New AcceleratorBlock(blockName, blockLanguage, entries))
            End While

            Return New AcceleratorScript(leadingLanguage, blocks)
        End Function

        ''' <summary>
        ''' Parses a single accelerator entry line.
        ''' </summary>
        '''
        ''' <param name="line">
        ''' The trimmed entry line.
        ''' </param>
        '''
        ''' <param name="blockName">
        ''' The name of the block that contains the entry, used in error messages.
        ''' </param>
        '''
        ''' <returns>
        ''' The resulting <see cref="AcceleratorEntry"/>.
        ''' </returns>
        Private Shared Function ParseEntry(line As String, blockName As String) As AcceleratorEntry

            Dim parts As String() = line.Split(","c).Select(Function(p As String) p.Trim()).ToArray()
            If parts.Length < 2 Then
                Throw New FormatException($"Entry in block '{blockName}' must have at least a key and a command identifier: '{line}'.")
            End If

            Dim commandId As Integer
            If Not Integer.TryParse(parts(1), NumberStyles.None, CultureInfo.InvariantCulture, commandId) Then
                Throw New FormatException($"Entry in block '{blockName}' has a non-numeric command identifier: '{line}'.")
            End If

            Dim flags As New List(Of String)()
            For i As Integer = 2 To parts.Length - 1
                If parts(i).Length = 0 Then
                    Throw New FormatException($"Entry in block '{blockName}' has an empty option flag: '{line}'.")
                End If
                flags.Add(parts(i))
            Next i

            Return New AcceleratorEntry(parts(0), commandId, flags, line)
        End Function

        ''' <summary>
        ''' Determines whether the specified line is a <c>LANGUAGE</c> statement.
        ''' </summary>
        '''
        ''' <param name="line">
        ''' The trimmed line.
        ''' </param>
        '''
        ''' <returns>
        ''' <see langword="True"/> if the line is a <c>LANGUAGE</c> statement; otherwise, <see langword="False"/>.
        ''' </returns>
        Private Shared Function IsLanguageStatement(line As String) As Boolean

            Return line.StartsWith($"{AcceleratorTableParser.LanguageKeyword} ", StringComparison.Ordinal)
        End Function

        ''' <summary>
        ''' Gets the normalized arguments of a <c>LANGUAGE</c> statement (e.g. <c>LANG_SPANISH, SUBLANG_SPANISH_MODERN</c>).
        ''' </summary>
        '''
        ''' <param name="line">
        ''' The trimmed <c>LANGUAGE</c> statement line.
        ''' </param>
        '''
        ''' <returns>
        ''' The normalized arguments.
        ''' </returns>
        Private Shared Function GetLanguageArguments(line As String) As String

            Dim arguments As String = line.Substring(AcceleratorTableParser.LanguageKeyword.Length)
            Return String.Join(", ", arguments.Split(","c).Select(Function(a As String) a.Trim()))
        End Function

        ''' <summary>
        ''' Reads the next non-empty line, advancing the specified index.
        ''' </summary>
        '''
        ''' <param name="lines">
        ''' The source lines.
        ''' </param>
        '''
        ''' <param name="refIndex">
        ''' The index of the next line to read. When this method returns, contains the index of the line after the one read.
        ''' </param>
        '''
        ''' <returns>
        ''' The trimmed line, or <see langword="Nothing"/> if the end of the script has been reached.
        ''' </returns>
        Private Shared Function ReadNextNonEmptyLine(lines As String(), ByRef refIndex As Integer) As String

            While refIndex < lines.Length
                Dim line As String = lines(refIndex).Trim()
                refIndex += 1
                If line.Length > 0 Then
                    Return line
                End If
            End While

            Return Nothing
        End Function

    End Class

#End Region

End Namespace

#End Region
