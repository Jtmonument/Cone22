Public Class SegmentsForm
    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        Dim temp As Integer
        If Integer.TryParse(TextBox1.Text, temp) Then
            Close()
        End If
    End Sub

    Private Sub TextBox1_KeyPress(sender As Object, e As KeyPressEventArgs) Handles TextBox1.KeyPress
        Dim temp As Integer
        If Char.IsWhiteSpace(e.KeyChar) AndAlso Integer.TryParse(TextBox1.Text, temp) Then
            Close()
        End If
    End Sub
End Class