
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
        ' Validations are performed on the trimmed value, which is the value that is finally stored.
        Dim trimmedFileName As String = muiFileName.Trim()
        If trimmedFileName.IndexOfAny(IO.Path.GetInvalidFileNameChars()) >= 0 Then
            Throw New ArgumentException("The provided MUI file name cannot be parsed as a valid Windows file name.", NameOf(muiFileName))
        End If
        If Not IO.Path.HasExtension(trimmedFileName) OrElse
           Not IO.Path.GetExtension(trimmedFileName).Equals(".mui", StringComparison.OrdinalIgnoreCase) OrElse
           String.IsNullOrWhiteSpace(IO.Path.GetFileNameWithoutExtension(trimmedFileName)) Then
            Throw New ArgumentException("The provided MUI file must have a '.mui' extension.", NameOf(muiFileName))
        End If

        ' checksum validations
        If String.IsNullOrWhiteSpace(checksum) Then
            Throw New ArgumentNullException(NameOf(checksum))
        End If
        ' Validations are performed on the trimmed value, which is the value that is finally stored.
        Dim trimmedChecksum As String = checksum.Trim()
        If trimmedChecksum.Length <> 8 Then
            Throw New ArgumentException("The CRC-32 checksum must be exactly 8 characters in length.", NameOf(checksum))
        End If
        If Not MuiDescriptor.IsHexString(trimmedChecksum) Then
            Throw New ArgumentException("The CRC-32 checksum must contain only hexadecimal characters (0-9, A-F).", NameOf(checksum))
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

        Me.FileName = trimmedFileName
        Me.Checksum = trimmedChecksum
        Me.AcceleratorTable = accTable.Trim()
    End Sub

#End Region

#Region " Private Methods "

    ''' <summary>
    ''' Determines whether the specified string is composed only of hexadecimal characters (0-9, a-f, A-F).
    ''' </summary>
    '''
    ''' <param name="value">
    ''' The string to evaluate.
    ''' </param>
    '''
    ''' <returns>
    ''' <see langword="True"/> if <paramref name="value"/> contains only hexadecimal characters; otherwise, <see langword="False"/>.
    ''' </returns>
    <DebuggerStepThrough>
    Private Shared Function IsHexString(value As String) As Boolean

        For Each c As Char In value
            Dim isHexChar As Boolean = (c >= "0"c AndAlso c <= "9"c) OrElse
                                       (c >= "a"c AndAlso c <= "f"c) OrElse
                                       (c >= "A"c AndAlso c <= "F"c)
            If Not isHexChar Then
                Return False
            End If
        Next c

        Return True
    End Function

#End Region

#Region " Equality Members "

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

    ''' <summary>
    ''' Serves as the default hash function.
    ''' <para></para>
    ''' The hash code is consistent with <see cref="MuiDescriptor.Equals(MuiDescriptor)"/>,
    ''' which compares all the properties using a case-insensitive ordinal comparison.
    ''' </summary>
    '''
    ''' <returns>
    ''' A hash code for the current object.
    ''' </returns>
    Public Overrides Function GetHashCode() As Integer

        ' Note: Bit shifts are used instead of multiplications because VB.NET performs integer overflow checks.
        Dim comparer As StringComparer = StringComparer.OrdinalIgnoreCase
        Return comparer.GetHashCode(Me.FileName) Xor
               (comparer.GetHashCode(Me.Checksum) << 7) Xor
               (comparer.GetHashCode(Me.AcceleratorTable) << 13)
    End Function

#End Region

End Class

#End Region