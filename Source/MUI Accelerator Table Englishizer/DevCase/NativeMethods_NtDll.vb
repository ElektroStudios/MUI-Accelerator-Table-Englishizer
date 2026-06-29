
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Runtime.InteropServices
Imports System.Security

Imports DevCase.Win32.Structures

Imports Microsoft.VisualBasic

#End Region

#Region " NativeMethods "

' ReSharper disable once CheckNamespace

Namespace DevCase.Win32.NativeMethods

    ''' <summary>
    ''' Platform Invocation methods (P/Invoke), access unmanaged code.
    ''' <para></para>
    ''' NtDll.dll.
    ''' </summary>
    <HideModuleName>
    <SuppressUnmanagedCodeSecurity>
    Public Module NtDll

        ' ReSharper disable VBWarnings::BC42309

#Region " NtDll.dll "

        ''' <summary>
        ''' Calculate the CRC-32 checksum of a block of bytes.
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://source.winehq.org/WineAPI/RtlComputeCrc32.html"/>
        ''' </remarks>
        '''
        ''' <param name="initialValue">
        ''' The value used to initialize the CRC value/register.
        ''' </param>
        ''' 
        ''' <param name="buffer">
        ''' The block of bytes to calculate its CRC-32.
        ''' </param>
        ''' 
        ''' <param name="bufferLen">
        ''' Length of <paramref name="buffer"/>, in bytes.
        ''' </param>
        '''
        ''' <returns>
        ''' The cumulative CRC-32 checksum of <paramref name="initialValue"/> and <paramref name="bufferLen"/> bytes of <paramref name="buffer"/>. 
        ''' </returns>
        <DllImport(Win32LibNames.NtDll, SetLastError:=False)>
        Public Function RtlComputeCrc32(<[In]> initialValue As UInteger,
                                        <[In]> buffer As Byte(),
                                        <[In]> bufferLen As UInteger
        ) As UInteger
        End Function

        ''' <summary>
        ''' Gets version information about the currently running operating system.
        ''' <para></para>
        ''' <see cref="NativeMethods.RtlGetVersion"/> is the equivalent of the GetVersionEx function in the Windows SDK.
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://learn.microsoft.com/en-us/windows/win32/devnotes/rtlgetversion"/>
        ''' </remarks>
        '''
        ''' <param name="refOsVersionInfo">
        ''' A RTL_OSVERSIONINFOW structure or a RTL_OSVERSIONINFOEXW structure that contains the 
        ''' version information about the currently running operating system. 
        ''' <para></para>
        ''' A caller specifies which input structure is used by setting the 
        ''' dwOSVersionInfoSize member of the structure to the size in bytes of the structure that is used.
        ''' </param>
        '''
        ''' <returns>
        ''' Returns STATUS_SUCCESS (zero).
        ''' </returns>
        <DllImport(Win32LibNames.NtDll, CharSet:=CharSet.Unicode)>
        Public Function RtlGetVersion(ByRef refOsVersionInfo As OsVersionInfoEx
        ) As Integer
        End Function

#End Region

    End Module

End Namespace

#End Region
