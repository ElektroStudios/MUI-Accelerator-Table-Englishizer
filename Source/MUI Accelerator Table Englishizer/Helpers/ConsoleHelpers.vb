
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Diagnostics

#End Region

#Region " Console Helper "

''' <summary>
''' Provides methods for writing to console output and exiting the console application.
''' </summary>
Friend Module ConsoleHelper

#Region " Static Methods "

    ''' <summary>
    ''' Writes a colored message to the console.
    ''' </summary>
    ''' 
    ''' <param name="message">
    ''' The text message to write.
    ''' </param>
    ''' 
    ''' <param name="foreColor">
    ''' The foreground color to apply to the text.
    ''' </param>
    <DebuggerStepThrough>
    Friend Sub WriteColoredLine(message As String, foreColor As ConsoleColor)

        Dim previousColor As ConsoleColor = Console.ForegroundColor
        Console.ForegroundColor = foreColor
        Console.WriteLine(message)
        Console.ForegroundColor = previousColor
    End Sub

    ''' <summary>
    ''' Displays a message to the console and exits the application with the specified exit code.
    ''' </summary>
    ''' 
    ''' <param name="message">
    ''' The message to display before exiting. If empty or null, no message is displayed.
    ''' </param>
    ''' 
    ''' <param name="exitCode">
    ''' The exit code to return to the operating system. Typically 0 for success, non-zero for errors.
    ''' </param>
    ''' 
    ''' <param name="foreColor">
    ''' The console foreground color to use when displaying the message. 
    ''' <para></para>
    ''' After writing the message, the console color is reset to its original value.
    ''' </param>
    <DebuggerStepThrough>
    Friend Sub ExitWithMessage(message As String, exitCode As Integer, foreColor As ConsoleColor)

        If Not String.IsNullOrEmpty(message) Then
            ConsoleHelper.WriteColoredLine(message, foreColor)
            Console.WriteLine()
        End If

        If exitCode <> 0 Then
            Console.WriteLine($"Exiting application with exit code: {exitCode} (0x{exitCode:X8}) ...")
            Console.WriteLine()
        End If

        Console.WriteLine("Press any key to exit...")
        Console.ReadKey(intercept:=True)

        Environment.Exit(exitCode)
    End Sub

#End Region

End Module

#End Region
