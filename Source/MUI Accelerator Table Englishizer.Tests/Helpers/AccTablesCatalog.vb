#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Reflection
Imports System.Text.RegularExpressions

#End Region

#Region " AccTablesCatalog "

Namespace MATE.Tests.Helpers

    ''' <summary>
    ''' Provides reflection-based access to all the accelerator table definitions declared in the <c>AccTables</c> module,
    ''' so tests automatically cover any definition added in the future.
    ''' </summary>
    Friend NotInheritable Class AccTablesCatalog

        ''' <summary>
        ''' The pattern that every accelerator table constant name must follow:
        ''' <c>{MUIFILE}_{culture}_ENGLISHIZED_{crc32}</c> (e.g. <c>SHELL32_esES_ENGLISHIZED_1f3defe7</c>).
        ''' </summary>
        Friend Const ConstantNamePattern As String = "^(?<file>[A-Z0-9]+)_(?<culture>[a-z]{2}[A-Z]{2})_ENGLISHIZED_(?<crc>[0-9a-f]{8})$"

        ''' <summary>
        ''' Maps the file prefix of a constant name to the MUI file name it applies to.
        ''' </summary>
        Friend Shared ReadOnly FilePrefixToMuiFileName As IReadOnlyDictionary(Of String, String) =
            New Dictionary(Of String, String)(StringComparer.Ordinal) From {
                {"SHELL32", "shell32.dll.mui"},
                {"NOTEPAD", "notepad.exe.mui"},
                {"WORDPAD", "wordpad.exe.mui"}
            }

        ''' <summary>
        ''' Prevents a default instance of the <see cref="AccTablesCatalog"/> class from being created.
        ''' </summary>
        <DebuggerNonUserCode>
        Private Sub New()
        End Sub

        ''' <summary>
        ''' Gets all the accelerator table definitions declared as string constants in the <c>AccTables</c> module.
        ''' </summary>
        '''
        ''' <returns>
        ''' A dictionary where each key is the constant name and the value is the accelerator table definition.
        ''' </returns>
        Friend Shared Function GetAllTables() As SortedDictionary(Of String, String)

            Dim result As New SortedDictionary(Of String, String)(StringComparer.Ordinal)
            Dim flags As BindingFlags = BindingFlags.Static Or BindingFlags.Public Or BindingFlags.NonPublic Or BindingFlags.DeclaredOnly

            For Each field As FieldInfo In GetType(AccTables).GetFields(flags)
                If field.IsLiteral AndAlso field.FieldType Is GetType(String) Then
                    result.Add(field.Name, DirectCast(field.GetRawConstantValue(), String))
                End If
            Next field

            Return result
        End Function

        ''' <summary>
        ''' Gets the data rows (constant name and value) of all the accelerator table definitions, for data-driven tests.
        ''' </summary>
        '''
        ''' <returns>
        ''' The data rows, where each row is an array containing the constant name and the accelerator table definition.
        ''' </returns>
        Friend Shared Function GetAllTablesAsDataRows() As IEnumerable(Of Object())

            Dim rows As New List(Of Object())()
            For Each kvp As KeyValuePair(Of String, String) In AccTablesCatalog.GetAllTables()
                rows.Add(New Object() {kvp.Key, kvp.Value})
            Next kvp
            Return rows
        End Function

        ''' <summary>
        ''' Parses the specified constant name.
        ''' </summary>
        '''
        ''' <param name="constantName">
        ''' The constant name.
        ''' </param>
        '''
        ''' <param name="refFilePrefix">
        ''' When this method returns, contains the MUI file prefix (e.g. <c>SHELL32</c>).
        ''' </param>
        '''
        ''' <param name="refCulture">
        ''' When this method returns, contains the culture token (e.g. <c>esES</c>).
        ''' </param>
        '''
        ''' <param name="refChecksum">
        ''' When this method returns, contains the CRC-32 checksum (e.g. <c>1f3defe7</c>).
        ''' </param>
        '''
        ''' <returns>
        ''' <see langword="True"/> if the constant name follows the expected pattern; otherwise, <see langword="False"/>.
        ''' </returns>
        Friend Shared Function TryParseConstantName(constantName As String,
                                                    ByRef refFilePrefix As String,
                                                    ByRef refCulture As String,
                                                    ByRef refChecksum As String) As Boolean

            Dim m As Match = Regex.Match(constantName, AccTablesCatalog.ConstantNamePattern, RegexOptions.CultureInvariant)
            If Not m.Success Then
                Return False
            End If

            refFilePrefix = m.Groups("file").Value
            refCulture = m.Groups("culture").Value
            refChecksum = m.Groups("crc").Value
            Return True
        End Function

    End Class

End Namespace

#End Region
