Imports System.Drawing.Imaging
Imports System.IO
Imports System.Math
Imports System.Text

Public Class Form1

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click

        ' Assign variables

        Dim Seg As Integer
        Dim TotalWidth As Double
        Dim TotalLength As Double
        Dim SegmentWidth As Double
        Dim SegmentLength As Double

        Dim Q As Double
        Dim B As Double
        Dim C As Double
        Dim X As Double
        Dim Theta As Double
        Dim Y As Double
        Dim IR As Double
        Dim OUR As Double
        Dim PIE As Double
        Dim Deg As Double
        Dim Degr As Double
        Dim Z As Double
        Dim C1 As Double
        Dim V1 As Double
        Dim V2 As Double
        Dim V3 As Double
        Dim NoTitle = ""

        JobNumber = TextBox1.Text
        PieceName = TextBox2.Text
        PlateType = If(RadioButton1.Checked, Plate.INNER, Plate.OUTER)

        ' Validate Radius values
        If OuterRadius <= 0 Then
            MsgBox("The outer radius must be greater than zero.", vbOKOnly, NoTitle)
            Return
        ElseIf InnerRadius >= OuterRadius Then
            MsgBox("The inner radius must be less than outer radius.", vbOKOnly, NoTitle)
            Return
        ElseIf String.IsNullOrWhiteSpace(JobNumber) Then
            MsgBox("Job Number must not be empty.", vbOKOnly, NoTitle)
            Return
        ElseIf String.IsNullOrWhiteSpace(PieceName) Then
            MsgBox("Piece Name must not be empty.", vbOKOnly, NoTitle)
            Return
        End If

        ' STEP 1: Calculate

        X = OuterRadius - InnerRadius
        Theta = Atan(ConeHeight / X)
        Y = (PlateThickness / 2) * Sin(Theta)

        If PlateType = Plate.INNER Then
            IR = InnerRadius + Y
            OUR = OuterRadius + Y
        Else
            ' Line 460
            IR = InnerRadius - Y
            OUR = OuterRadius - Y
        End If

        OutsideRadius = OUR / Cos(Theta)
        InsideRadius = IR / Cos(Theta)
        PIE = 3.1416 ' Can I use PI from Math namespace?
        Deg = 2 * PIE * Cos(Theta)
        Degree = FormatNumber((Deg * 180) / PIE, 4)

        ' Line 540
        If Deg > 0 AndAlso Deg <= PIE / 2 Then 'goto 1290
            C = PIE / 2 - Deg
            SecondCutOffRadius = 2 * OutsideRadius * Sin(C / 2)
            FirstCutOffRadius = 2 * OutsideRadius * Sin(Deg / 2)
            SegmentLength = FirstCutOffRadius + 1
            B = (Sin(Deg / 4)) ^ 2 * InsideRadius * 2
            SegmentWidth = (OutsideRadius - InsideRadius) + B + 1
        ElseIf Deg > PIE / 2 AndAlso Deg <= PIE Then ' goto 1360
            C = PIE - Deg
            SegmentLength = Sin(Deg / 2) * OutsideRadius * 2 + 1
            SegmentWidth = (Sin(Deg / 4)) ^ 2 * InsideRadius * 2 + (OutsideRadius - InsideRadius) + 1
        ElseIf Deg > PIE AndAlso Deg <= 3 * PIE / 2 Then ' goto 1400
            C = 3 * PIE / 2 - Deg
            SegmentLength = OutsideRadius * 2 + 1
            SegmentWidth = Sin(Deg - PIE) * OutsideRadius + OutsideRadius + 1
        ElseIf Deg > 3 * PIE / 2 AndAlso Deg <= 2 * PIE Then ' goto 1440
            C = 2 * PIE - Deg
            SegmentLength = OutsideRadius * 2 + 1
            SegmentWidth = SegmentLength
        End If

        ' goto 1490

        ' Calculate CutOff Radii
        Q = (PIE / 2 - C) / 2
        FirstCutOffRadius = 2 * OutsideRadius * Sin(Q)
        SecondCutOffRadius = 2 * OutsideRadius * Sin(C / 2)

        'goto 590
        ' goto 650

        ' Sixteenth Rounding Routine
        OutsideRadius = Int(OutsideRadius * 16 + 0.5) / 16
        InsideRadius = Int(InsideRadius * 16 + 0.5) / 16
        Difference = OutsideRadius - InsideRadius
        FirstCutOffRadius = Int(FirstCutOffRadius * 16 + 0.5) / 16
        SecondCutOffRadius = Int(SecondCutOffRadius * 16 + 0.5) / 16

        ' goto 1570

        ' STEP 2: Prompt for figuring in segments

        Dim prompt As New StringBuilder

        With prompt
            .Append($"Outside Radius = {OutsideRadius}").AppendLine()
            .Append($"Inside Radius = {InsideRadius}").AppendLine()
            .Append($"Difference = {Difference}").AppendLine()
            .Append($"First Cut-Off Radius = {FirstCutOffRadius}").AppendLine()
            .Append($"Second Cut-Off Radius = {SecondCutOffRadius}").AppendLine()
            .Append($"Degrees = {Degree}").AppendLine()
            .Append("Do you want to figure your plate size in segments?")
        End With

        ' Prompt for figuring in segments
        ' If in segments goto 1720 else goto 2000
        If MsgBox(prompt.ToString(), vbYesNo, NoTitle).Equals(MsgBoxResult.Yes) Then
            Do
                ' Get number of segments
                Dim SegmentForm As New SegmentsForm
                SegmentForm.ShowDialog()
                Seg = SegmentForm.TextBox1.Text
                ' Validate Seg value
                Degr = Deg / Seg
                Z = (Sin(Degr / 4)) ^ 2 * InsideRadius * 2
                C1 = Sin(Degr / 2) * InsideRadius * 2
                V1 = (OutsideRadius ^ 2 - (C1 / 2) ^ 2) ^ 0.5
                V2 = (InsideRadius ^ 2 - (C1 / 2) ^ 2) ^ 0.5
                V3 = V1 - V2 + 0.5
                TotalLength = (Seg - 1) * V3 + Z + Difference + 1
                TotalWidth = Sin(Degr / 2) * OutsideRadius * 2 + 1

                ' Round
                TotalWidth = FormatNumber(TotalWidth, 4)
                TotalLength = FormatNumber(TotalLength, 4)
                SegmentWidth = FormatNumber(SegmentWidth, 4)
                SegmentLength = FormatNumber(SegmentLength, 4)

                prompt.Clear()
                With prompt
                    .Append($"Number of segments = {Seg}").AppendLine()
                    .Append($"\tTotal Width = {TotalWidth}").AppendLine()
                    .Append($"\tTotal Length = {TotalLength}").AppendLine()
                    .Append($"Segment dimensions:").AppendLine()
                    .Append($"\tTotal Width = {SegmentWidth}").AppendLine()
                    .Append($"\tTotal Length = {SegmentLength}").AppendLine()
                    .Append("Do you want to change the number of segments?")
                End With
            Loop Until MsgBox(prompt.ToString(), vbYesNo, NoTitle).Equals(MsgBoxResult.No)
        End If

        ' Test to show contents
        prompt.Clear()
        With prompt
            .Append(PieceName).Append(" "c)
            .Append(JobNumber).Append(" "c)
            .Append(PlateType).Append(" "c)
            .Append(OuterRadius).Append(" "c)
            .Append(InnerRadius).Append(" "c)
            .Append(ConeHeight).Append(" "c)
            .Append(PlateThickness).Append(" "c)
            .Append(OutsideRadius).Append(" "c)
            .Append(InsideRadius).Append(" "c)
            .Append(Difference).Append(" "c)
            .Append(Degree).Append(" "c)
            .Append(FirstCutOffRadius).Append(" "c)
            .Append(SecondCutOffRadius).Append(" "c)
            .Append(Seg).Append(" "c)
            .Append(TotalWidth).Append(" "c)
            .Append(TotalLength).Append(" "c)
            .Append(SegmentWidth).Append(" "c)
            .Append(SegmentLength)
        End With
        ConfirmResources()
        Dim script = New ProcessStartInfo
        script.FileName = "create_pdf"
        script.Arguments = prompt.ToString()
        script.CreateNoWindow = True
        Process.Start(script)

    End Sub

    Private Sub ConfirmResources()
        '
        ' Install resources if not in path
        '
        If Not File.Exists("create_pdf.exe") Then
            Dim exe As FileStream = File.Open("create_pdf.tmp", FileMode.Create, FileAccess.Write)
            Dim contents As Byte() = My.Resources.create_pdf
            exe.Write(contents, 0, contents.Length)
            exe.Close()
            Dim Args = "/c ren create_pdf.tmp create_pdf.exe"
            Dim script = New ProcessStartInfo("cmd", Args)
            script.CreateNoWindow = True
            Process.Start(script).WaitForExit()
        End If

        If Not File.Exists("processbarron_logo_dark.png") Then
            Dim exe As FileStream = File.Open("processbarron_logo_dark.png", FileMode.Create, FileAccess.Write)
            Dim bit As Bitmap = My.Resources.processbarron_logo_dark
            bit.Save(exe, ImageFormat.Png)
            exe.Close()
        End If
    End Sub

    Private Sub TextBox_TextChanged(sender As Object, e As EventArgs) Handles TextBox3.TextChanged, TextBox4.TextChanged, TextBox5.TextChanged, TextBox6.TextChanged
        Select Case sender.GetHashCode()
            Case TextBox3.GetHashCode()
                CorrectText(TextBox3, OuterRadius)
            Case TextBox4.GetHashCode()
                CorrectText(TextBox4, InnerRadius)
            Case TextBox5.GetHashCode()
                CorrectText(TextBox5, ConeHeight)
            Case TextBox6.GetHashCode()
                CorrectText(TextBox6, PlateThickness)
        End Select
    End Sub

    Private Sub CorrectText(ByRef sender As TextBox, ByRef var As Double)
        Dim input = sender.Text
        Dim temp As Double = 0
        If Double.TryParse(input, temp) Then
            sender.Text &= " in."
        ElseIf input.EndsWith("in.") AndAlso Double.TryParse(input.Substring(0, input.IndexOf("in.")), temp) Then
            var = temp
            sender.Text = var & " in."
        Else
            sender.Text = var & " in."
        End If
        sender.SelectionStart = sender.Text.Length - 4
    End Sub
    Private Sub Form_KeyPress(sender As Object, e As KeyPressEventArgs) Handles TextBox1.KeyPress, TextBox2.KeyPress, TextBox3.KeyPress, TextBox4.KeyPress, TextBox5.KeyPress, TextBox6.KeyPress
        If Char.IsWhiteSpace(e.KeyChar) Then
            Me.Button1_Click(sender, e)
        End If
    End Sub

    Private Sub TextBox_MouseClick(sender As Object, e As MouseEventArgs) Handles TextBox1.MouseClick, TextBox2.MouseClick, TextBox3.MouseClick, TextBox4.MouseClick, TextBox5.MouseClick, TextBox6.MouseClick
        Select Case sender.GetHashCode()
            Case TextBox1.GetHashCode()
                TextBox1.SelectAll()
            Case TextBox2.GetHashCode()
                TextBox2.SelectAll()
            Case TextBox3.GetHashCode()
                TextBox3.SelectAll()
            Case TextBox4.GetHashCode()
                TextBox4.SelectAll()
            Case TextBox5.GetHashCode()
                TextBox5.SelectAll()
            Case TextBox6.GetHashCode()
                TextBox6.SelectAll()
        End Select
    End Sub
End Class
