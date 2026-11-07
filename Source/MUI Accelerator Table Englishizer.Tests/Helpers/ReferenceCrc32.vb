#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Diagnostics

#End Region

#Region " ReferenceCrc32 "

Namespace MATE.Tests.Helpers

    ''' <summary>
    ''' Provides a fully managed, table-driven reference implementation of the standard CRC-32
    ''' (IEEE 802.3, reflected polynomial <c>0xEDB88320</c>) algorithm.
    ''' <para></para>
    ''' Used as an independent oracle to validate the results of the native <c>RtlComputeCrc32</c> function.
    ''' </summary>
    '''
    ''' <remarks>
    ''' The <see cref="ReferenceCrc32.Compute(UInteger, Byte(), Integer, Integer)"/> method mirrors the semantics of
    ''' <c>RtlComputeCrc32</c>: the initial value is the result of a previous computation
    ''' (or zero), so chained calls over consecutive chunks produce the same result as a single call over the whole data.
    ''' </remarks>
    Friend NotInheritable Class ReferenceCrc32

#Region " Fields "

        ''' <summary>
        ''' The precomputed lookup table for the reflected CRC-32 polynomial.
        ''' </summary>
        Private Shared ReadOnly Table As UInteger() = ReferenceCrc32.BuildTable()

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Prevents a default instance of the <see cref="ReferenceCrc32"/> class from being created.
        ''' </summary>
        <DebuggerNonUserCode>
        Private Sub New()
        End Sub

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Computes the CRC-32 checksum of the specified data.
        ''' </summary>
        '''
        ''' <param name="data">
        ''' The data to compute its checksum.
        ''' </param>
        '''
        ''' <returns>
        ''' The resulting CRC-32 checksum.
        ''' </returns>
        Friend Shared Function Compute(data As Byte()) As UInteger

            If data Is Nothing Then
                Throw New ArgumentNullException(NameOf(data))
            End If

            Return ReferenceCrc32.Compute(0UI, data, 0, data.Length)
        End Function

        ''' <summary>
        ''' Computes the cumulative CRC-32 checksum of a range of bytes, continuing from a previous checksum.
        ''' </summary>
        '''
        ''' <param name="initialValue">
        ''' The checksum of the previous chunk, or zero to start a new computation.
        ''' </param>
        '''
        ''' <param name="data">
        ''' The source data.
        ''' </param>
        '''
        ''' <param name="offset">
        ''' The zero-based index of the first byte to process.
        ''' </param>
        '''
        ''' <param name="count">
        ''' The number of bytes to process.
        ''' </param>
        '''
        ''' <returns>
        ''' The resulting cumulative CRC-32 checksum.
        ''' </returns>
        Friend Shared Function Compute(initialValue As UInteger, data As Byte(), offset As Integer, count As Integer) As UInteger

            If data Is Nothing Then
                Throw New ArgumentNullException(NameOf(data))
            End If
            If offset < 0 OrElse count < 0 OrElse offset > data.Length - count Then
                Throw New ArgumentOutOfRangeException(NameOf(count), "The specified range is outside the bounds of the data.")
            End If

            Dim crc As UInteger = Not initialValue
            For index As Integer = offset To offset + count - 1
                Dim tableIndex As Integer = CInt((crc Xor data(index)) And &HFFUI)
                crc = ReferenceCrc32.Table(tableIndex) Xor (crc >> 8)
            Next

            Return Not crc
        End Function

        ''' <summary>
        ''' Computes the CRC-32 checksum of the specified data and returns its
        ''' uppercase, zero-padded, 8-digit hexadecimal representation.
        ''' </summary>
        '''
        ''' <param name="data">
        ''' The data to compute its checksum.
        ''' </param>
        '''
        ''' <returns>
        ''' The hexadecimal representation of the resulting CRC-32 checksum.
        ''' </returns>
        Friend Shared Function ComputeHex(data As Byte()) As String

            Return ReferenceCrc32.Compute(data).ToString("X8")
        End Function

#End Region

#Region " Private Methods "

        ''' <summary>
        ''' Builds the lookup table for the reflected CRC-32 polynomial.
        ''' </summary>
        '''
        ''' <returns>
        ''' The 256-entries lookup table.
        ''' </returns>
        Private Shared Function BuildTable() As UInteger()

            Const polynomial As UInteger = &HEDB88320UI

            Dim result As UInteger() = New UInteger(255) {}
            For index As Integer = 0 To 255
                Dim entry As UInteger = CUInt(index)
                For bit As Integer = 0 To 7
                    If (entry And 1UI) <> 0UI Then
                        entry = (entry >> 1) Xor polynomial
                    Else
                        entry >>= 1
                    End If
                Next
                result(index) = entry
            Next

            Return result
        End Function

#End Region

    End Class

End Namespace

#End Region
