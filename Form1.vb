Imports System.Drawing.Imaging
Imports System.IO
Imports System.Math
Imports System.Text

Public Class Form1

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click

        '
        ' Validate Radius values
        '
        JobNumber = TextBox2.Text
        PieceName = TextBox1.Text
        Dim NoTitle = ""
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

        '
        ' STEP 1: Calculate
        '
        Dim X As Double = OuterRadius - InnerRadius
        Dim Theta As Double = Atan(ConeHeight / X)
        Dim Y As Double = (PlateThickness / 2) * Sin(Theta)
        Dim IR As Double
        Dim OUR As Double
        PlateType = If(RadioButton1.Checked, Plate.INNER, Plate.OUTER)

        '
        ' Line numbers refer to legacy code in Cone95 program
        '
        If PlateType = Plate.INNER Then

            '
            ' Line 1540
            '
            IR = InnerRadius + Y
            OUR = OuterRadius + Y
        Else

            '
            ' Line 460
            '
            IR = InnerRadius - Y
            OUR = OuterRadius - Y
        End If

        OutsideRadius = OUR / Cos(Theta)
        InsideRadius = IR / Cos(Theta)
        Const PIE As Double = 3.1416
        Dim Deg As Double = 2 * PIE * Cos(Theta)
        Degree = FormatNumber((Deg * 180) / PIE, 4)

        '
        ' Line 540
        '
        Dim C As Double
        Dim SegmentLength As Double
        Dim SegmentWidth As Double

        If Deg > 0 AndAlso Deg <= PIE / 2 Then

            '
            ' Line 1290
            '
            C = PIE / 2 - Deg
            SecondCutOffRadius = 2 * OutsideRadius * Sin(C / 2)
            FirstCutOffRadius = 2 * OutsideRadius * Sin(Deg / 2)
            SegmentLength = FirstCutOffRadius + 1
            Dim B As Double = (Sin(Deg / 4)) ^ 2 * InsideRadius * 2
            SegmentWidth = (OutsideRadius - InsideRadius) + B + 1
        ElseIf Deg > PIE / 2 AndAlso Deg <= PIE Then

            '
            ' Line 1360
            '
            C = PIE - Deg
            SegmentLength = Sin(Deg / 2) * OutsideRadius * 2 + 1
            SegmentWidth = (Sin(Deg / 4)) ^ 2 * InsideRadius * 2 + (OutsideRadius - InsideRadius) + 1
        ElseIf Deg > PIE AndAlso Deg <= 3 * PIE / 2 Then

            '
            ' Line 1400
            '
            C = 3 * PIE / 2 - Deg
            SegmentLength = OutsideRadius * 2 + 1
            SegmentWidth = Sin(Deg - PIE) * OutsideRadius + OutsideRadius + 1
        ElseIf Deg > 3 * PIE / 2 AndAlso Deg <= 2 * PIE Then

            '
            ' Line 1440
            '
            C = 2 * PIE - Deg
            SegmentLength = OutsideRadius * 2 + 1
            SegmentWidth = SegmentLength
        End If

        '
        ' Line 1490
        ' Calculate CutOff Radii
        '
        Dim Q As Double = (PIE / 2 - C) / 2
        FirstCutOffRadius = 2 * OutsideRadius * Sin(Q)
        SecondCutOffRadius = 2 * OutsideRadius * Sin(C / 2)

        '
        ' Line 650
        ' Sixteenth Rounding Routine
        '
        OutsideRadius = Int(OutsideRadius * 16 + 0.5) / 16
        InsideRadius = Int(InsideRadius * 16 + 0.5) / 16
        Difference = OutsideRadius - InsideRadius
        FirstCutOffRadius = Int(FirstCutOffRadius * 16 + 0.5) / 16
        SecondCutOffRadius = Int(SecondCutOffRadius * 16 + 0.5) / 16

        '
        ' Line 1570
        ' STEP 2: Prompt for figuring in segments
        '
        Dim Seg As Double
        Dim TotalLength As Double
        Dim TotalWidth As Double
        Dim Prompt As New StringBuilder
        With Prompt
            .Append($"Outside Radius = {OutsideRadius}").AppendLine()
            .Append($"Inside Radius = {InsideRadius}").AppendLine()
            .Append($"Difference = {Difference}").AppendLine()
            .Append($"First Cut-Off Radius = {FirstCutOffRadius}").AppendLine()
            .Append($"Second Cut-Off Radius = {SecondCutOffRadius}").AppendLine()
            .Append($"Degrees = {Degree}").AppendLine()
            .Append("Do you want to figure your plate size in segments?")
        End With

        If MsgBox(Prompt.ToString(), vbYesNo, NoTitle).Equals(MsgBoxResult.Yes) Then

            '
            ' Line 1720
            '
            Do
                '
                ' Get number of segments
                '
                Dim SegmentForm As New SegmentsForm
                SegmentForm.ShowDialog()
                Seg = SegmentForm.TextBox1.Text

                '
                ' Line 1780
                ' Calculation of width and length of plate per segments
                '
                Dim Degr As Double = Deg / Seg
                Dim Z As Double = (Sin(Degr / 4)) ^ 2 * InsideRadius * 2
                Dim C1 As Double = Sin(Degr / 2) * InsideRadius * 2
                Dim V1 As Double = (OutsideRadius ^ 2 - (C1 / 2) ^ 2) ^ 0.5
                Dim V2 As Double = (InsideRadius ^ 2 - (C1 / 2) ^ 2) ^ 0.5
                Dim V3 As Double = V1 - V2 + 0.5
                TotalLength = (Seg - 1) * V3 + Z + Difference + 1
                TotalWidth = Sin(Degr / 2) * OutsideRadius * 2 + 1

                '
                ' Rounding
                '
                TotalWidth = FormatNumber(TotalWidth, 4)
                TotalLength = FormatNumber(TotalLength, 4)
                SegmentWidth = FormatNumber(SegmentWidth, 4)
                SegmentLength = FormatNumber(SegmentLength, 4)

                '
                ' Confirm number of segments
                '
                Prompt.Clear()
                With Prompt
                    .Append($"Number of segments = {Seg}").AppendLine()
                    .Append($"\tTotal Width = {TotalWidth}").AppendLine()
                    .Append($"\tTotal Length = {TotalLength}").AppendLine()
                    .Append($"Segment dimensions:").AppendLine()
                    .Append($"\tTotal Width = {SegmentWidth}").AppendLine()
                    .Append($"\tTotal Length = {SegmentLength}").AppendLine()
                    .Append("Do you want to change the number of segments?")
                End With
            Loop Until MsgBox(Prompt.ToString(), vbYesNo, NoTitle).Equals(MsgBoxResult.No)
        End If

        '
        ' Create PDF
        '
        Prompt.Clear()
        With Prompt
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
        script.Arguments = Prompt.ToString()
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

        '
        ' Get associated value of text box
        '
        If sender.Equals(TextBox3) Then
            CorrectText(TextBox3, OuterRadius)
        ElseIf sender.Equals(TextBox4) Then
            CorrectText(TextBox4, InnerRadius)
        ElseIf sender.Equals(TextBox5) Then
            CorrectText(TextBox5, ConeHeight)
        ElseIf sender.Equals(TextBox6) Then
            CorrectText(TextBox6, PlateThickness)
        End If
    End Sub

    Private Sub CorrectText(ByRef sender As TextBox, ByRef var As Double)

        '
        ' Only allow numbers as values
        '
        If sender.Text.Contains(Int(var) & ". in.") Then
            Return
        End If
        Dim input = sender.Text.Replace(" ", "").Replace("in.", "")
        Dim temp As Double = 0
        If Double.TryParse(input, temp) Then
            var = temp
        ElseIf String.IsNullOrWhiteSpace(input) Then
            var = 0
        End If
        sender.Text = var & " in."
        If sender.SelectionStart = 0 Then
            sender.SelectionStart = 1
        End If
    End Sub
    Private Sub Form_KeyPress(sender As Object, e As KeyPressEventArgs) Handles TextBox1.KeyPress, TextBox2.KeyPress, TextBox3.KeyPress, TextBox4.KeyPress, TextBox5.KeyPress, TextBox6.KeyPress

        '
        ' Listen for 'Enter' and begin calculation
        '
        If Char.IsWhiteSpace(e.KeyChar) Then
            Button1_Click(sender, e)
        End If
    End Sub

    Private Sub TextBox_MouseClick(sender As Object, e As MouseEventArgs) Handles TextBox1.MouseClick, TextBox2.MouseClick, TextBox3.MouseClick, TextBox4.MouseClick, TextBox5.MouseClick, TextBox6.MouseClick

        '
        ' Select all text when mouse clicks the text box or switched focus
        '
        CType(sender, TextBox).SelectAll()
    End Sub
End Class
