#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.Generic
Imports System.Globalization

#End Region

#Region " TestLanguageConfiguration "

Namespace MATE.Tests.Helpers

    ''' <summary>
    ''' Provides a concrete, test-only implementation of the abstract <see cref="LanguageConfiguration"/> class,
    ''' allowing to exercise its constructor validations and equality members with arbitrary arguments.
    ''' </summary>
    Public NotInheritable Class TestLanguageConfiguration : Inherits LanguageConfiguration

        ''' <summary>
        ''' Initializes a new instance of the <see cref="TestLanguageConfiguration"/> class.
        ''' </summary>
        '''
        ''' <param name="ci">
        ''' The culture information associated with this language configuration.
        ''' </param>
        '''
        ''' <param name="muiDescriptors">
        ''' The collection of targeted MUI file descriptor definitions.
        ''' </param>
        Public Sub New(ci As CultureInfo, muiDescriptors As IReadOnlyList(Of MuiDescriptor))

            MyBase.New(ci, muiDescriptors)
        End Sub

    End Class

End Namespace

#End Region
