#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.IO
Imports System.Threading

#End Region

#Region " TempDirectory "

Namespace MATE.Tests.Helpers

    ''' <summary>
    ''' Represents a unique temporary directory that is recursively deleted when disposed.
    ''' <para></para>
    ''' Provides helpers to create test files and subdirectories inside it.
    ''' </summary>
    Friend NotInheritable Class TempDirectory : Implements IDisposable

#Region " Fields "

        ''' <summary>
        ''' Flag to detect redundant calls to <see cref="TempDirectory.Dispose()"/>.
        ''' </summary>
        Private isDisposed As Boolean

#End Region

#Region " Properties "

        ''' <summary>
        ''' Gets the full path of the temporary directory.
        ''' </summary>
        Friend ReadOnly Property FullPath As String

#End Region

#Region " Constructors "

        ''' <summary>
        ''' Initializes a new instance of the <see cref="TempDirectory"/> class, creating a new unique temporary directory.
        ''' </summary>
        Friend Sub New()

            Dim tempRoot As String = Path.GetFullPath(Path.GetTempPath())
            Me.FullPath = Path.Combine(tempRoot, $"MATETests_{Guid.NewGuid():N}")
            Directory.CreateDirectory(Me.FullPath)
        End Sub

#End Region

#Region " Public Methods "

        ''' <summary>
        ''' Creates a file with the specified content inside the temporary directory,
        ''' creating any intermediate subdirectory as needed.
        ''' </summary>
        '''
        ''' <param name="relativeFilePath">
        ''' The file name, or a path relative to the temporary directory.
        ''' </param>
        '''
        ''' <param name="content">
        ''' The file content.
        ''' </param>
        '''
        ''' <returns>
        ''' The full path of the created file.
        ''' </returns>
        Friend Function CreateFile(relativeFilePath As String, content As Byte()) As String

            If String.IsNullOrWhiteSpace(relativeFilePath) Then
                Throw New ArgumentNullException(NameOf(relativeFilePath))
            End If
            If content Is Nothing Then
                Throw New ArgumentNullException(NameOf(content))
            End If

            Dim filePath As String = Path.Combine(Me.FullPath, relativeFilePath)
            Directory.CreateDirectory(Path.GetDirectoryName(filePath))
            File.WriteAllBytes(filePath, content)
            Return filePath
        End Function

        ''' <summary>
        ''' Creates an empty file inside the temporary directory, creating any intermediate subdirectory as needed.
        ''' </summary>
        '''
        ''' <param name="relativeFilePath">
        ''' The file name, or a path relative to the temporary directory.
        ''' </param>
        '''
        ''' <returns>
        ''' The full path of the created file.
        ''' </returns>
        Friend Function CreateEmptyFile(relativeFilePath As String) As String

            Return Me.CreateFile(relativeFilePath, Array.Empty(Of Byte)())
        End Function

        ''' <summary>
        ''' Creates a subdirectory inside the temporary directory.
        ''' </summary>
        '''
        ''' <param name="relativeDirectoryPath">
        ''' The directory name, or a path relative to the temporary directory.
        ''' </param>
        '''
        ''' <returns>
        ''' The full path of the created subdirectory.
        ''' </returns>
        Friend Function CreateSubdirectory(relativeDirectoryPath As String) As String

            If String.IsNullOrWhiteSpace(relativeDirectoryPath) Then
                Throw New ArgumentNullException(NameOf(relativeDirectoryPath))
            End If

            Dim directoryPath As String = Path.Combine(Me.FullPath, relativeDirectoryPath)
            Directory.CreateDirectory(directoryPath)
            Return directoryPath
        End Function

        ''' <summary>
        ''' Creates a deterministic pseudo-random byte array.
        ''' </summary>
        '''
        ''' <param name="length">
        ''' The array length.
        ''' </param>
        '''
        ''' <param name="seed">
        ''' The seed of the pseudo-random generator.
        ''' </param>
        '''
        ''' <returns>
        ''' The resulting byte array.
        ''' </returns>
        Friend Shared Function CreateRandomBytes(length As Integer, seed As Integer) As Byte()

            Dim result As Byte() = New Byte(length - 1) {}
            Dim rnd As New Random(seed)
            rnd.NextBytes(result)
            Return result
        End Function

#End Region

#Region " IDisposable Implementation "

        ''' <summary>
        ''' Deletes the temporary directory and all its content,
        ''' resetting the attributes of read-only, hidden and system files beforehand.
        ''' </summary>
        Public Sub Dispose() Implements IDisposable.Dispose

            If Me.isDisposed Then
                Exit Sub
            End If
            Me.isDisposed = True

            If Not Directory.Exists(Me.FullPath) Then
                Exit Sub
            End If

            For Each filePath As String In Directory.EnumerateFiles(Me.FullPath, "*", SearchOption.AllDirectories)
                Thread.CurrentThread.Join(0)
                File.SetAttributes(filePath, FileAttributes.Normal)
            Next filePath

            Directory.Delete(Me.FullPath, recursive:=True)
        End Sub

#End Region

    End Class

End Namespace

#End Region
