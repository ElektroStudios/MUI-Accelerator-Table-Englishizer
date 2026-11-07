
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

#Region " LanguageConfiguration "
''' <summary>
''' Provides a base implementation for culture-specific MUI language configurations. This class must be inherited.
''' </summary>
Public MustInherit Class LanguageConfiguration : Implements IEquatable(Of LanguageConfiguration)

#Region " Properties "

    ''' <summary>
    ''' Gets the culture information associated with this language configuration.
    ''' </summary>
    Public ReadOnly Property CultureInfo As CultureInfo

    ''' <summary>
    ''' Gets the read-only collection of targeted MUI file descriptors.
    ''' </summary>
    Public ReadOnly Property MuiDescriptors As IReadOnlyList(Of MuiDescriptor)

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Prevents a default instance of the <see cref="LanguageConfiguration"/> class from being created.
    ''' </summary>
    Private Sub New()
    End Sub

    ''' <summary>
    ''' Initializes a new instance of the <see cref="LanguageConfiguration"/> class.
    ''' </summary>
    ''' 
    ''' <param name="ci">
    ''' The culture information associated with this language configuration.
    ''' </param>
    ''' 
    ''' <param name="muiDescriptors">
    ''' The read-only collection of targeted MUI file descriptor definitions.
    ''' </param>
    ''' 
    ''' <exception cref="ArgumentNullException">
    ''' Thrown when <paramref name="ci"/> or <paramref name="muiDescriptors"/> is null.
    ''' </exception>
    ''' 
    ''' <exception cref="ArgumentException">
    ''' Thrown when <paramref name="muiDescriptors"/> contains no elements, or contains null elements.
    ''' </exception>
    <DebuggerStepThrough>
    Public Sub New(ci As CultureInfo,
                   muiDescriptors As IReadOnlyList(Of MuiDescriptor))

        If ci Is Nothing Then
            Throw New ArgumentNullException(NameOf(ci))
        End If

        If muiDescriptors Is Nothing Then
            Throw New ArgumentNullException(NameOf(muiDescriptors))
        End If

        If muiDescriptors.Count = 0 Then
            Throw New ArgumentException("The MuiDescriptors collection cannot be empty.", NameOf(muiDescriptors))
        End If

        For i As Integer = 0 To muiDescriptors.Count - 1
            If muiDescriptors(i) Is Nothing Then
                Throw New ArgumentException($"The MuiDescriptors collection cannot contain null elements (index {i}).", NameOf(muiDescriptors))
            End If
        Next i

        Me.CultureInfo = ci
        ' Defensive copy: later changes to the source collection must not alter this configuration.
        Me.MuiDescriptors = New ReadOnlyCollection(Of MuiDescriptor)(muiDescriptors.ToList())
    End Sub

    ''' <summary>
    ''' Determines whether the specified <see cref="LanguageConfiguration"/> is equal to the current instance.
    ''' </summary>
    ''' 
    ''' <param name="other">
    ''' The <see cref="LanguageConfiguration"/> to compare with the current object.
    ''' </param>
    ''' 
    ''' <returns>
    ''' <see langword="True"/> if the objects are equal; otherwise, <see langword="False"/>.
    ''' </returns>
    Public Overloads Function Equals(other As LanguageConfiguration) As Boolean Implements IEquatable(Of LanguageConfiguration).Equals

        Return (other IsNot Nothing) AndAlso
               Me.CultureInfo.Equals(other.CultureInfo) AndAlso
               Me.MuiDescriptors.SequenceEqual(other.MuiDescriptors)
    End Function

    ''' <summary>
    ''' Determines whether the specified <see cref="Object"/> is equal to the current instance.
    ''' </summary>
    ''' 
    ''' <param name="obj">
    ''' The object to compare with the current instance.
    ''' </param>
    ''' 
    ''' <returns>
    ''' <see langword="True"/> if the specified object is equal to the current instance; otherwise, <see langword="False"/>.
    ''' </returns>
    Public Overrides Function Equals(obj As Object) As Boolean

        Dim other As LanguageConfiguration = TryCast(obj, LanguageConfiguration)

        Return Me.Equals(other)
    End Function

    ''' <summary>
    ''' Serves as the default hash function.
    ''' <para></para>
    ''' The hash code is consistent with <see cref="LanguageConfiguration.Equals(LanguageConfiguration)"/>,
    ''' which compares the culture and the sequence of MUI descriptors.
    ''' </summary>
    '''
    ''' <returns>
    ''' A hash code for the current object.
    ''' </returns>
    Public Overrides Function GetHashCode() As Integer

        ' Note: Bit shifts are used instead of multiplications because VB.NET performs integer overflow checks.
        Dim hash As Integer = Me.CultureInfo.GetHashCode()
        For Each muiDesc As MuiDescriptor In Me.MuiDescriptors
            hash = (hash << 5) Xor (hash >> 27) Xor muiDesc.GetHashCode()
        Next muiDesc
        Return hash
    End Function

#End Region

End Class

#End Region