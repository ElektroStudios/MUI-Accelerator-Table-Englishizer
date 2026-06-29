
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
    Public Module Kernel32

        ' ReSharper disable VBWarnings::BC42309

#Region " Kernel32.dll "

        ''' <summary>
        ''' Returns the language identifier for the user UI language for the current user.
        ''' <para></para>
        ''' If the current user has not set a language,
        ''' <see cref="NativeMethods.GetUserDefaultUILanguage"/> returns the preferred language set for the system.
        ''' <para></para>
        ''' If there is no preferred language set for the system, then the system default UI language (also known as "install language") is returned.
        ''' </summary>
        '''
        ''' <remarks>
        ''' <see href="https://docs.microsoft.com/en-us/windows/desktop/api/winnls/nf-winnls-getuserdefaultuilanguage"/>
        ''' </remarks>
        '''
        ''' <returns>
        ''' Returns the language identifier for the user UI language for the current user.
        ''' </returns>
        <DllImport(Win32LibNames.Kernel32, SetLastError:=False, ExactSpelling:=True)>
        Public Function GetUserDefaultUILanguage(
        ) As UShort
        End Function

#End Region

    End Module

End Namespace

#End Region
