
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Diagnostics

#End Region

#Region " MuiDescriptor "
''' <summary>
''' Provides specific information for a MUI file.
''' </summary>
Public NotInheritable Class MuiDescriptor : Implements IEquatable(Of MuiDescriptor)

#Region " Properties "

    ''' <summary>
    ''' Gets or sets the name of the MUI file.
    ''' </summary>
    Public ReadOnly Property FileName As String

    ''' <summary>
    ''' Gets or sets the CRC-32 checksum.
    ''' </summary>
    Public ReadOnly Property Checksum As String

    ''' <summary>
    ''' Gets or sets the accelerator table content or identifier associated with the MUI file.
    ''' </summary>
    Public ReadOnly Property AcceleratorTable As String

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Prevents a default instance of the <see cref="MuiDescriptor"/> module from being created.
    ''' </summary>
    Private Sub New()
    End Sub

    ''' <summary>
    ''' Initializes a new instance of the <see cref="MuiDescriptor"/> class.
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
    ''' The "Englishized" accelerator table associated with the MUI file.
    ''' </param>
    <DebuggerStepThrough>
    Public Sub New(muiFileName As String, checksum As String, accTable As String)

        ' muiFileName validations
        If String.IsNullOrWhiteSpace(muiFileName) Then
            Throw New ArgumentNullException(NameOf(muiFileName))
        End If
        Try
            Dim validFileName As String = IO.Path.GetFileNameWithoutExtension(muiFileName)
        Catch ex As ArgumentException
            Throw New ArgumentException("The provided MUI file name cannot be parsed as a valid Windows file name.", NameOf(muiFileName), ex)
        End Try
        If Not IO.Path.HasExtension(muiFileName) OrElse
           Not IO.Path.GetExtension(muiFileName).Equals(".mui", StringComparison.OrdinalIgnoreCase) Then
            Throw New ArgumentException("The provided MUI file must have a '.mui' extension.", NameOf(muiFileName))
        End If

        ' checksum validations
        If String.IsNullOrWhiteSpace(checksum) Then
            Throw New ArgumentNullException(NameOf(checksum))
        End If
        If checksum.Length <> 8 Then
            Throw New ArgumentException("The CRC-32 checksum must be exactly 8 characters in length.", NameOf(checksum))
        End If

        ' accTable validations
        If String.IsNullOrWhiteSpace(accTable) Then
            Throw New ArgumentNullException(NameOf(accTable))
        End If
        If accTable.IndexOf("LANG_ENGLISH", StringComparison.OrdinalIgnoreCase) >= 0 Then
            Throw New ArgumentException("The accelerator table contains prohibited 'LANG_ENGLISH' language token.", NameOf(accTable))
        End If
        If accTable.IndexOf("ACCELERATORS", StringComparison.OrdinalIgnoreCase) = -1 OrElse
           accTable.IndexOf("LANGUAGE", StringComparison.OrdinalIgnoreCase) = -1 OrElse
           accTable.IndexOf("{", StringComparison.OrdinalIgnoreCase) = -1 OrElse
           accTable.IndexOf("}", StringComparison.OrdinalIgnoreCase) = -1 Then
            Throw New ArgumentException("The accelerator table is missing required 'ACCELERATORS', 'LANGUAGE', '{', or '}' tokens.", NameOf(accTable))
        End If

        Me.FileName = muiFileName.Trim()
        Me.Checksum = checksum.Trim()
        Me.AcceleratorTable = accTable.Trim()
    End Sub

    ''' <summary>
    ''' Determines whether the specified <see cref="MuiDescriptor"/> is equal to the current instance.
    ''' </summary>
    ''' 
    ''' <param name="other">
    ''' The <see cref="MuiDescriptor"/> to compare with the current object.
    ''' </param>
    ''' 
    ''' <returns>
    ''' <see langword="True"/> if the objects are equal; otherwise, <see langword="False"/>.
    ''' </returns>
    Public Overloads Function Equals(other As MuiDescriptor) As Boolean Implements IEquatable(Of MuiDescriptor).Equals

        Return (other IsNot Nothing) AndAlso
               Me.FileName.Equals(other.FileName, StringComparison.OrdinalIgnoreCase) AndAlso
               Me.Checksum.Equals(other.Checksum, StringComparison.OrdinalIgnoreCase) AndAlso
               Me.AcceleratorTable.Equals(other.AcceleratorTable, StringComparison.OrdinalIgnoreCase)
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

        Dim other As MuiDescriptor = TryCast(obj, MuiDescriptor)

        Return Me.Equals(other)
    End Function

#End Region

End Class

#End Region