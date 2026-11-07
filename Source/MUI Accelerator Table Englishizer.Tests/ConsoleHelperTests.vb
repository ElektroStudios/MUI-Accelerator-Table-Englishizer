#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.IO

#End Region

#Region " ConsoleHelperTests "

Namespace MATE.Tests

    ''' <summary>
    ''' Unit tests for the <see cref="ConsoleHelper"/> module.
    ''' </summary>
    '''
    ''' <remarks>
    ''' <see cref="ConsoleHelper.ExitWithMessage(String, Integer, ConsoleColor)"/> is intentionally not tested,
    ''' because it waits for a key press and terminates the process.
    ''' </remarks>
    <TestClass>
    <DoNotParallelize>
    Public NotInheritable Class ConsoleHelperTests

#Region " Fields "

        ''' <summary>
        ''' The original console output writer, restored after each test.
        ''' </summary>
        Private originalOut As TextWriter

        ''' <summary>
        ''' The original console foreground color, restored after each test.
        ''' </summary>
        Private originalForeColor As ConsoleColor

#End Region

#Region " Test Initialization and Cleanup "

        ''' <summary>
        ''' Saves the console state before each test.
        ''' </summary>
        <TestInitialize>
        Public Sub TestInitialize()

            Me.originalOut = Console.Out
            Me.originalForeColor = Console.ForegroundColor
        End Sub

        ''' <summary>
        ''' Restores the console state after each test.
        ''' </summary>
        <TestCleanup>
        Public Sub TestCleanup()

            Console.SetOut(Me.originalOut)
            Console.ForegroundColor = Me.originalForeColor
        End Sub

#End Region

#Region " WriteColoredLine "

        ''' <summary>
        ''' Verifies that the message is written to the console output, followed by a line terminator.
        ''' </summary>
        '''
        ''' <param name="message">
        ''' The message to write.
        ''' </param>
        <TestMethod>
        <DataRow("Hello World")>
        <DataRow(" [X] ERROR: Something failed.")>
        <DataRow("╭── Unicode box ──╮")>
        <DataRow("")>
        Public Sub WriteColoredLine_Message_IsWrittenWithLineTerminator(message As String)

            Using writer As New StringWriter()
                Console.SetOut(writer)

                ConsoleHelper.WriteColoredLine(message, ConsoleColor.Green)

                Assert.AreEqual($"{message}{Environment.NewLine}", writer.ToString())
            End Using
        End Sub

        ''' <summary>
        ''' Verifies that a null message writes an empty line.
        ''' </summary>
        <TestMethod>
        Public Sub WriteColoredLine_NullMessage_WritesEmptyLine()

            Using writer As New StringWriter()
                Console.SetOut(writer)

                ConsoleHelper.WriteColoredLine(Nothing, ConsoleColor.Red)

                Assert.AreEqual(Environment.NewLine, writer.ToString())
            End Using
        End Sub

        ''' <summary>
        ''' Verifies that the original console foreground color is restored after writing.
        ''' </summary>
        '''
        ''' <param name="foreColor">
        ''' The foreground color to apply to the text.
        ''' </param>
        <TestMethod>
        <DataRow(ConsoleColor.Red)>
        <DataRow(ConsoleColor.Cyan)>
        <DataRow(ConsoleColor.DarkYellow)>
        Public Sub WriteColoredLine_AnyColor_RestoresOriginalForegroundColor(foreColor As ConsoleColor)

            Using writer As New StringWriter()
                Console.SetOut(writer)
                Dim colorBefore As ConsoleColor = Console.ForegroundColor

                ConsoleHelper.WriteColoredLine("Message", foreColor)

                Assert.AreEqual(colorBefore, Console.ForegroundColor)
            End Using
        End Sub

        ''' <summary>
        ''' Verifies that consecutive calls write consecutive lines in order.
        ''' </summary>
        <TestMethod>
        Public Sub WriteColoredLine_ConsecutiveCalls_WriteLinesInOrder()

            Using writer As New StringWriter()
                Console.SetOut(writer)

                ConsoleHelper.WriteColoredLine("Line 1", ConsoleColor.White)
                ConsoleHelper.WriteColoredLine("Line 2", ConsoleColor.Gray)

                Assert.AreEqual($"Line 1{Environment.NewLine}Line 2{Environment.NewLine}", writer.ToString())
            End Using
        End Sub

#End Region

    End Class

End Namespace

#End Region
