
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Diagnostics

#End Region

#Region " Architecture Helper "

''' <summary>
''' Provides methods for identifying and formatting system architecture information.
''' </summary>
Friend Module ArchitectureHelper

#Region " Static Methods "

    ''' <summary>
    ''' Gets a human readable display name for the current operating system architecture.
    ''' </summary>
    ''' 
    ''' <returns>
    ''' Returns "64-Bit" for x64 architecture; otherwise returns "32-Bit".
    ''' </returns>
    <DebuggerStepThrough>
    Friend Function GetOsArchitectureDisplayName() As String

        Return If(Environment.Is64BitOperatingSystem, "64-Bit", "32-Bit")
    End Function

    ''' <summary>
    ''' Gets a human readable display name for the current processor architecture.
    ''' </summary>
    ''' 
    ''' <returns>
    ''' Returns "64-Bit" for for x64 architecture; otherwise returns "32-Bit".
    ''' </returns>
    <DebuggerStepThrough>
    Friend Function GetProcessArchitectureDisplayName() As String

        Return If(Environment.Is64BitProcess, "64-Bit", "32-Bit")
    End Function

#End Region

End Module

#End Region
