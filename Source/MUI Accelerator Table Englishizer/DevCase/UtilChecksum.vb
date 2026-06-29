
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Diagnostics
Imports System.IO

Imports DevCase.Win32

#End Region

#Region " UtilChecksum "

' ReSharper disable once CheckNamespace

Namespace DevCase.Core.Security.DataIntegrity.Checksum

    ''' <summary>
    ''' Provides checksum related utilities.
    ''' </summary>
    Public NotInheritable Class UtilChecksum

#Region " Constructors "

        ''' <summary>
        ''' Prevents a default instance of the <see cref="UtilChecksum"/> class from being created.
        ''' </summary>
        <DebuggerNonUserCode>
        Private Sub New()
        End Sub

#End Region

#Region " Public Methods "

#Region " CRC-32 "

        ''' <summary>
        ''' Computes a CRC-32 checksum for the specified file.
        ''' </summary>
        '''
        ''' <example> This is a code example.
        ''' <code language="VB.NET">
        ''' Dim crc32 As String = ComputeCRC32OfFile("C:\File.ext")
        ''' </code>
        ''' </example>
        '''
        ''' <param name="filepath">
        ''' The filepath.
        ''' </param>
        '''
        ''' <returns>
        ''' An Hexadecimal representation of the resulting CRC-32 checksum.
        ''' </returns>
        <DebuggerStepThrough>
        Public Shared Function ComputeCRC32OfFile(filepath As String) As String

            Dim data As Byte() = File.ReadAllBytes(filepath)
            Return UtilChecksum.ComputeCRC32OfBytes(data)
        End Function

        ''' <summary>
        ''' Computes a CRC-32 checksum for the specified byte array.
        ''' </summary>
        '''
        ''' <example> This is a code example.
        ''' <code language="VB.NET">
        ''' Dim data as Byte() = {1, 2, 3, 4, 5}
        ''' Dim crc32 As String = ComputeCRC32OfBytes(data)
        ''' </code>
        ''' </example>
        '''
        ''' <param name="bytes">
        ''' The byte array.
        ''' </param>
        '''
        ''' <returns>
        ''' An Hexadecimal representation of the resulting CRC-32 checksum.
        ''' </returns>
        <DebuggerStepThrough>
        Public Shared Function ComputeCRC32OfBytes(bytes As Byte()) As String

            Const initialValue As UInteger = 0UI
            Dim bufferSize As UInteger = Convert.ToUInt32(bytes.Length)

            Dim crc32 As UInteger = NativeMethods.RtlComputeCrc32(initialValue, bytes, bufferSize)
            Return crc32.ToString("X8")
        End Function

#End Region

#End Region

    End Class

End Namespace

#End Region
