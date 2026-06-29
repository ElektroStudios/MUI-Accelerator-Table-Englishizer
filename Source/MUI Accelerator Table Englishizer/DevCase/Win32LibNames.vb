
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Runtime.InteropServices

#End Region

#Region " Win32LibNames "

' ReSharper disable once CheckNamespace

Namespace DevCase.Win32

    ''' <summary>
    ''' Provides the filenames to specify in <see cref="DllImportAttribute.Value"/> for all referenced Win32 functions.
    ''' </summary>
    Friend Module Win32LibNames

        ' ReSharper disable InconsistentNaming

        ''' <summary>
        ''' Kernel32.dll
        ''' </summary>
        Friend Const Kernel32 As String = "kernel32.dll"

        ''' <summary>
        ''' NtDll.dll
        ''' </summary>
        Friend Const NtDll As String = "ntdll.dll"

    End Module

End Namespace

#End Region
