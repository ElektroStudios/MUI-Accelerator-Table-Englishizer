#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.Generic
Imports System.Globalization

Imports MATE.Tests.Helpers

#End Region

#Region " LanguageConfigurationTests "

Namespace MATE.Tests

    ''' <summary>
    ''' Unit tests for the abstract <see cref="LanguageConfiguration"/> class,
    ''' exercised through the <see cref="TestLanguageConfiguration"/> test implementation.
    ''' </summary>
    <TestClass>
    Public NotInheritable Class LanguageConfigurationTests

#Region " Constants "

        ''' <summary>
        ''' A minimal, valid accelerator table definition.
        ''' </summary>
        Private Const ValidAccTable As String = "
1 ACCELERATORS
LANGUAGE LANG_SPANISH, SUBLANG_SPANISH_MODERN
{
    VK_A, 25, CONTROL, VIRTKEY
}
"

#End Region

#Region " Constructor "

        ''' <summary>
        ''' Verifies that valid arguments are stored in the corresponding properties, preserving the descriptors order.
        ''' </summary>
        <TestMethod>
        Public Sub Constructor_ValidArguments_SetsProperties()

            Dim ci As New CultureInfo("es-ES")
            Dim descriptors As MuiDescriptor() = LanguageConfigurationTests.CreateDescriptors("0a2fad88", "7a6b3ec1")

            Dim config As New TestLanguageConfiguration(ci, descriptors)

            Assert.AreSame(ci, config.CultureInfo)
            Assert.AreEqual(2, config.MuiDescriptors.Count)
            Assert.AreSame(descriptors(0), config.MuiDescriptors(0))
            Assert.AreSame(descriptors(1), config.MuiDescriptors(1))
        End Sub

        ''' <summary>
        ''' Verifies that a null culture throws <see cref="ArgumentNullException"/>.
        ''' </summary>
        <TestMethod>
        Public Sub Constructor_NullCulture_ThrowsArgumentNullException()

            Dim ex As ArgumentNullException = Assert.ThrowsExactly(Of ArgumentNullException)(
                Sub() LanguageConfigurationTests.CreateConfiguration(Nothing, LanguageConfigurationTests.CreateDescriptors("0a2fad88")))

            Assert.AreEqual("ci", ex.ParamName)
        End Sub

        ''' <summary>
        ''' Verifies that a null descriptors collection throws <see cref="ArgumentNullException"/>.
        ''' </summary>
        <TestMethod>
        Public Sub Constructor_NullDescriptors_ThrowsArgumentNullException()

            Dim ex As ArgumentNullException = Assert.ThrowsExactly(Of ArgumentNullException)(
                Sub() LanguageConfigurationTests.CreateConfiguration(New CultureInfo("es-ES"), Nothing))

            Assert.AreEqual("muiDescriptors", ex.ParamName)
        End Sub

        ''' <summary>
        ''' Verifies that an empty descriptors collection throws <see cref="ArgumentException"/>.
        ''' </summary>
        <TestMethod>
        Public Sub Constructor_EmptyDescriptors_ThrowsArgumentException()

            Dim ex As ArgumentException = Assert.ThrowsExactly(Of ArgumentException)(
                Sub() LanguageConfigurationTests.CreateConfiguration(New CultureInfo("es-ES"), Array.Empty(Of MuiDescriptor)()))

            Assert.AreEqual("muiDescriptors", ex.ParamName)
        End Sub

        ''' <summary>
        ''' Regression test: a descriptors collection containing null elements throws <see cref="ArgumentException"/> (fail-fast),
        ''' instead of causing a <see cref="NullReferenceException"/> later, during the MUI files processing.
        ''' </summary>
        '''
        ''' <param name="nullIndex">
        ''' The index of the null element.
        ''' </param>
        <TestMethod>
        <DataRow(0)>
        <DataRow(1)>
        <DataRow(2)>
        Public Sub Constructor_DescriptorsWithNullElement_ThrowsArgumentException(nullIndex As Integer)

            Dim descriptors As MuiDescriptor() = LanguageConfigurationTests.CreateDescriptors("0a2fad88", "7a6b3ec1", "eb176603")
            descriptors(nullIndex) = Nothing

            Dim ex As ArgumentException = Assert.ThrowsExactly(Of ArgumentException)(
                Sub() LanguageConfigurationTests.CreateConfiguration(New CultureInfo("es-ES"), descriptors))

            Assert.AreEqual("muiDescriptors", ex.ParamName)
            Assert.IsTrue(ex.Message.Contains($"index {nullIndex}"), $"Unexpected message: {ex.Message}")
        End Sub

        ''' <summary>
        ''' Verifies that later changes to the source collection do not alter the configuration (defensive copy).
        ''' </summary>
        <TestMethod>
        Public Sub Constructor_SourceCollectionModifiedAfterwards_ConfigurationIsNotAltered()

            Dim source As New List(Of MuiDescriptor)(LanguageConfigurationTests.CreateDescriptors("0a2fad88", "7a6b3ec1"))
            Dim config As New TestLanguageConfiguration(New CultureInfo("es-ES"), source)

            source.Add(New MuiDescriptor("wordpad.exe.mui", "075f2d95", ValidAccTable))
            source(0) = Nothing
            source.RemoveAt(1)

            Assert.AreEqual(2, config.MuiDescriptors.Count)
            Assert.IsNotNull(config.MuiDescriptors(0))
            Assert.AreEqual("0a2fad88", config.MuiDescriptors(0).Checksum)
            Assert.AreEqual("7a6b3ec1", config.MuiDescriptors(1).Checksum)
        End Sub

        ''' <summary>
        ''' Verifies that the exposed descriptors collection cannot be modified through a mutable collection interface.
        ''' </summary>
        <TestMethod>
        Public Sub MuiDescriptors_IsReadOnly()

            Dim config As New TestLanguageConfiguration(New CultureInfo("es-ES"), LanguageConfigurationTests.CreateDescriptors("0a2fad88"))

            Dim asCollection As ICollection(Of MuiDescriptor) = TryCast(config.MuiDescriptors, ICollection(Of MuiDescriptor))

            Assert.IsNotNull(asCollection, "Precondition failed: the collection should implement ICollection(Of T).")
            Assert.IsTrue(asCollection.IsReadOnly)
            Assert.ThrowsExactly(Of NotSupportedException)(Sub() asCollection.Clear())
        End Sub

#End Region

#Region " Equality Members "

        ''' <summary>
        ''' Verifies that two configurations with the same culture and equal descriptors are equal.
        ''' </summary>
        <TestMethod>
        Public Sub Equals_SameCultureAndEqualDescriptors_ReturnsTrue()

            Dim config1 As New TestLanguageConfiguration(New CultureInfo("es-ES"), LanguageConfigurationTests.CreateDescriptors("0a2fad88", "7a6b3ec1"))
            Dim config2 As New TestLanguageConfiguration(New CultureInfo("es-ES"), LanguageConfigurationTests.CreateDescriptors("0a2fad88", "7a6b3ec1"))

            Assert.IsTrue(config1.Equals(config2))
            Assert.IsTrue(config2.Equals(config1))
            Assert.IsTrue(config1.Equals(DirectCast(config2, Object)))
        End Sub

        ''' <summary>
        ''' Verifies that two configurations with different cultures are not equal.
        ''' </summary>
        <TestMethod>
        Public Sub Equals_DifferentCulture_ReturnsFalse()

            Dim config1 As New TestLanguageConfiguration(New CultureInfo("es-ES"), LanguageConfigurationTests.CreateDescriptors("0a2fad88"))
            Dim config2 As New TestLanguageConfiguration(New CultureInfo("es-MX"), LanguageConfigurationTests.CreateDescriptors("0a2fad88"))

            Assert.IsFalse(config1.Equals(config2))
        End Sub

        ''' <summary>
        ''' Verifies that two configurations with different descriptors are not equal.
        ''' </summary>
        <TestMethod>
        Public Sub Equals_DifferentDescriptors_ReturnsFalse()

            Dim config1 As New TestLanguageConfiguration(New CultureInfo("es-ES"), LanguageConfigurationTests.CreateDescriptors("0a2fad88"))
            Dim config2 As New TestLanguageConfiguration(New CultureInfo("es-ES"), LanguageConfigurationTests.CreateDescriptors("7a6b3ec1"))
            Dim config3 As New TestLanguageConfiguration(New CultureInfo("es-ES"), LanguageConfigurationTests.CreateDescriptors("0a2fad88", "7a6b3ec1"))

            Assert.IsFalse(config1.Equals(config2), "Different descriptor was ignored.")
            Assert.IsFalse(config1.Equals(config3), "Different descriptors count was ignored.")
        End Sub

        ''' <summary>
        ''' Verifies that the equality comparison is sensitive to the descriptors order.
        ''' </summary>
        <TestMethod>
        Public Sub Equals_SameDescriptorsInDifferentOrder_ReturnsFalse()

            Dim config1 As New TestLanguageConfiguration(New CultureInfo("es-ES"), LanguageConfigurationTests.CreateDescriptors("0a2fad88", "7a6b3ec1"))
            Dim config2 As New TestLanguageConfiguration(New CultureInfo("es-ES"), LanguageConfigurationTests.CreateDescriptors("7a6b3ec1", "0a2fad88"))

            Assert.IsFalse(config1.Equals(config2))
        End Sub

        ''' <summary>
        ''' Verifies that comparing to a null reference or to an object of another type returns <see langword="False"/>.
        ''' </summary>
        <TestMethod>
        Public Sub Equals_NullOrOtherType_ReturnsFalse()

            Dim config As New TestLanguageConfiguration(New CultureInfo("es-ES"), LanguageConfigurationTests.CreateDescriptors("0a2fad88"))

            Assert.IsFalse(config.Equals(DirectCast(Nothing, LanguageConfiguration)))
            Assert.IsFalse(config.Equals(DirectCast(Nothing, Object)))
            Assert.IsFalse(config.Equals(config.CultureInfo))
        End Sub

        ''' <summary>
        ''' Regression test: equal configurations must return the same hash code,
        ''' as required by the <see cref="Object.Equals(Object)"/> and <see cref="Object.GetHashCode()"/> contract.
        ''' </summary>
        <TestMethod>
        Public Sub GetHashCode_EqualConfigurations_ReturnSameHashCode()

            Dim config1 As New TestLanguageConfiguration(New CultureInfo("es-ES"), LanguageConfigurationTests.CreateDescriptors("0a2fad88", "7a6b3ec1"))
            Dim config2 As New TestLanguageConfiguration(New CultureInfo("es-ES"), LanguageConfigurationTests.CreateDescriptors("0A2FAD88", "7A6B3EC1"))

            Assert.IsTrue(config1.Equals(config2), "Precondition failed: configurations must be equal.")
            Assert.AreEqual(config1.GetHashCode(), config2.GetHashCode())
        End Sub

        ''' <summary>
        ''' Regression test: hash-based collections must treat equal configurations as the same element.
        ''' </summary>
        <TestMethod>
        Public Sub HashSet_EqualConfigurations_AreDeduplicated()

            Dim configs As New HashSet(Of LanguageConfiguration) From {
                New TestLanguageConfiguration(New CultureInfo("es-ES"), LanguageConfigurationTests.CreateDescriptors("0a2fad88")),
                New TestLanguageConfiguration(New CultureInfo("es-ES"), LanguageConfigurationTests.CreateDescriptors("0a2fad88"))
            }

            Assert.AreEqual(1, configs.Count)
        End Sub

#End Region

#Region " Private Methods "

        ''' <summary>
        ''' Creates an array of valid <see cref="MuiDescriptor"/> objects for <c>notepad.exe.mui</c>, one per specified checksum.
        ''' </summary>
        '''
        ''' <param name="checksums">
        ''' The CRC-32 checksums.
        ''' </param>
        '''
        ''' <returns>
        ''' The resulting array of descriptors.
        ''' </returns>
        Private Shared Function CreateDescriptors(ParamArray checksums As String()) As MuiDescriptor()

            Dim result As MuiDescriptor() = New MuiDescriptor(checksums.Length - 1) {}
            For i As Integer = 0 To checksums.Length - 1
                result(i) = New MuiDescriptor("notepad.exe.mui", checksums(i), ValidAccTable)
            Next i
            Return result
        End Function

        ''' <summary>
        ''' Creates a new <see cref="TestLanguageConfiguration"/> with the specified arguments.
        ''' <para></para>
        ''' Used to invoke the constructor from single-line lambda expressions.
        ''' </summary>
        '''
        ''' <param name="ci">
        ''' The culture information.
        ''' </param>
        '''
        ''' <param name="muiDescriptors">
        ''' The collection of MUI file descriptors.
        ''' </param>
        '''
        ''' <returns>
        ''' The resulting <see cref="TestLanguageConfiguration"/>.
        ''' </returns>
        Private Shared Function CreateConfiguration(ci As CultureInfo, muiDescriptors As IReadOnlyList(Of MuiDescriptor)) As TestLanguageConfiguration

            Return New TestLanguageConfiguration(ci, muiDescriptors)
        End Function

#End Region

    End Class

End Namespace

#End Region
