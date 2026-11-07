#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " ArchitectureHelperTests "

Namespace MATE.Tests

    ''' <summary>
    ''' Unit tests for the <see cref="ArchitectureHelper"/> module.
    ''' </summary>
    <TestClass>
    Public NotInheritable Class ArchitectureHelperTests

        ''' <summary>
        ''' Verifies that the operating system architecture display name matches <see cref="Environment.Is64BitOperatingSystem"/>.
        ''' </summary>
        <TestMethod>
        Public Sub GetOsArchitectureDisplayName_MatchesEnvironment()

            Dim expected As String = If(Environment.Is64BitOperatingSystem, "64-Bit", "32-Bit")

            Dim actual As String = ArchitectureHelper.GetOsArchitectureDisplayName()

            Assert.AreEqual(expected, actual)
        End Sub

        ''' <summary>
        ''' Verifies that the process architecture display name matches <see cref="Environment.Is64BitProcess"/>
        ''' (and therefore the pointer size).
        ''' </summary>
        <TestMethod>
        Public Sub GetProcessArchitectureDisplayName_MatchesEnvironmentAndPointerSize()

            Dim expected As String = If(IntPtr.Size = 8, "64-Bit", "32-Bit")

            Dim actual As String = ArchitectureHelper.GetProcessArchitectureDisplayName()

            Assert.AreEqual(expected, actual)
            Assert.AreEqual(If(Environment.Is64BitProcess, "64-Bit", "32-Bit"), actual)
        End Sub

        ''' <summary>
        ''' Verifies that a 64-bit process implies a 64-bit operating system.
        ''' </summary>
        <TestMethod>
        Public Sub DisplayNames_64BitProcess_Implies64BitOperatingSystem()

            If ArchitectureHelper.GetProcessArchitectureDisplayName() = "64-Bit" Then
                Assert.AreEqual("64-Bit", ArchitectureHelper.GetOsArchitectureDisplayName())
            Else
                Assert.AreEqual("32-Bit", ArchitectureHelper.GetProcessArchitectureDisplayName())
            End If
        End Sub

    End Class

End Namespace

#End Region
