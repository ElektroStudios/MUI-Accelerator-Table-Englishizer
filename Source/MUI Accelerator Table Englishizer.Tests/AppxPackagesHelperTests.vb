#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.Generic

#End Region

#Region " AppxPackagesHelperTests "

Namespace MATE.Tests

    ''' <summary>
    ''' Unit tests for the <see cref="AppxPackagesHelper"/> module.
    ''' </summary>
    '''
    ''' <remarks>
    ''' Reading the <c>WindowsApps</c> directory requires administrator privileges,
    ''' so these tests only verify the behavior that does not depend on the environment.
    ''' </remarks>
    <TestClass>
    Public NotInheritable Class AppxPackagesHelperTests

        ''' <summary>
        ''' Verifies that a pattern that cannot match any package returns an empty list instead of throwing.
        ''' </summary>
        <TestMethod>
        Public Sub GetMatchingAppxPackagesFromDisk_NonMatchingPattern_ReturnsEmptyList()

            Dim pattern As String = $"MATE.Tests.NonExistentPackage_{Guid.NewGuid():N}_*"

            Dim actual As List(Of String) = AppxPackagesHelper.GetMatchingAppxPackagesFromDisk(pattern)

            Assert.IsNotNull(actual)
            Assert.AreEqual(0, actual.Count)
        End Sub

        ''' <summary>
        ''' Verifies that an invalid search pattern returns an empty list instead of throwing.
        ''' </summary>
        <TestMethod>
        Public Sub GetMatchingAppxPackagesFromDisk_InvalidPattern_ReturnsEmptyList()

            Dim actual As List(Of String) = AppxPackagesHelper.GetMatchingAppxPackagesFromDisk("..\..\*")

            Assert.IsNotNull(actual)
            Assert.AreEqual(0, actual.Count)
        End Sub

        ''' <summary>
        ''' Verifies that the returned items (if any) are package folder names, not full paths.
        ''' </summary>
        <TestMethod>
        Public Sub GetMatchingAppxPackagesFromDisk_AnyPattern_ReturnsFolderNamesOnly()

            Dim actual As List(Of String) = AppxPackagesHelper.GetMatchingAppxPackagesFromDisk("Microsoft.*")

            Assert.IsNotNull(actual)
            For Each packageName As String In actual
                Assert.AreEqual(-1, packageName.IndexOfAny({"\"c, "/"c}), $"Not a folder name: {packageName}")
            Next packageName
        End Sub

    End Class

End Namespace

#End Region
