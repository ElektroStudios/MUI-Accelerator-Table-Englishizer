
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.ObjectModel
Imports System.Diagnostics
Imports System.Globalization

#End Region

#Region " LanguageConfiguration_esES "

''' <summary>
''' Provides the MUI language configuration for the Spanish (Spain) <b>(es-ES)</b> culture.
''' </summary>
Friend NotInheritable Class LanguageConfiguration_esES : Inherits LanguageConfiguration

    ''' <summary>
    ''' Initializes a new instance of the <see cref="LanguageConfiguration_esES"/> class.
    ''' </summary>
    <DebuggerStepThrough>
    Friend Sub New()

        MyBase.New(
            New CultureInfo("es-ES"),
            New ReadOnlyCollection(Of MuiDescriptor)({
                New MuiDescriptor("notepad.exe.mui", "0a2fad88", AccTables.NOTEPAD_esES_ENGLISHIZED_0a2fad88),
                New MuiDescriptor("notepad.exe.mui", "7a6b3ec1", AccTables.NOTEPAD_esES_ENGLISHIZED_7a6b3ec1),
                New MuiDescriptor("notepad.exe.mui", "eb176603", AccTables.NOTEPAD_esES_ENGLISHIZED_eb176603),
                New MuiDescriptor("wordpad.exe.mui", "075f2d95", AccTables.WORDPAD_esES_ENGLISHIZED_075f2d95),
                New MuiDescriptor("wordpad.exe.mui", "12548387", AccTables.WORDPAD_esES_ENGLISHIZED_12548387),
                New MuiDescriptor("wordpad.exe.mui", "845a4685", AccTables.WORDPAD_esES_ENGLISHIZED_845a4685),
                New MuiDescriptor("shell32.dll.mui", "1f3defe7", AccTables.SHELL32_esES_ENGLISHIZED_1f3defe7),
                New MuiDescriptor("shell32.dll.mui", "3600a388", AccTables.SHELL32_esES_ENGLISHIZED_3600a388),
                New MuiDescriptor("shell32.dll.mui", "f7d515d8", AccTables.SHELL32_esES_ENGLISHIZED_f7d515d8),
                New MuiDescriptor("shell32.dll.mui", "fb5e8156", AccTables.SHELL32_esES_ENGLISHIZED_fb5e8156)
             })
         )
    End Sub

End Class

#End Region
