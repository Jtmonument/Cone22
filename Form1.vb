Imports System.Drawing.Imaging
Imports System.IO
Imports System.Math
Imports System.Text
Imports PdfSharpCore.Drawing
Imports PdfSharpCore.Pdf
Imports Microsoft.WindowsAPICodePack.Shell

Public Class Form1

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click

        '
        ' Validate Radius values
        '
        JobNumber = TextBox1.Text
        PieceName = TextBox2.Text
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
        PlateType = If(RadioButton1.Checked, Plate.INSIDE, Plate.OUTSIDE)

        '
        ' Line numbers refer to legacy code in Cone95 program
        '
        If PlateType = Plate.INSIDE Then

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
        Dim NumOfSegments As Double = 1
        Dim TotalLength As Double
        Dim TotalWidth As Double
        Dim Prompt As New StringBuilder
        With Prompt
            .Append($"Outside Radius = {OutsideRadius}").AppendLine()
            .Append($"Inside Radius = {InsideRadius}").AppendLine()
            .Append($"Difference = {Difference}").AppendLine()
            .Append($"First Cut-Off Radius = {FirstCutOffRadius}").AppendLine()
            .Append($"Second Cut-Off Radius = {SecondCutOffRadius}").AppendLine()
            .Append($"Degrees = {Degree}").AppendLine().AppendLine()
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
                NumOfSegments = SegmentForm.TextBox1.Text
                If Not NumOfSegments = 1 Then
                    '
                    ' Line 1780
                    ' Calculation of width and length of plate per segments
                    '
                    Dim Degr = Deg / NumOfSegments
                    Dim Z As Double = (Sin(Degr / 4)) ^ 2 * InsideRadius * 2
                    Dim C1 As Double = Sin(Degr / 2) * InsideRadius * 2
                    Dim V1 As Double = (OutsideRadius ^ 2 - (C1 / 2) ^ 2) ^ 0.5
                    Dim V2 As Double = (InsideRadius ^ 2 - (C1 / 2) ^ 2) ^ 0.5
                    Dim V3 As Double = V1 - V2 + 0.5
                    TotalLength = (NumOfSegments - 1) * V3 + Z + Difference + 1
                    TotalWidth = Sin(Degr / 2) * OutsideRadius * 2 + 1
                    '
                    ' Rounding
                    '
                    TotalWidth = FormatNumber(TotalWidth, 4)
                    TotalLength = FormatNumber(TotalLength, 4)
                    SegmentWidth = FormatNumber(SegmentWidth, 4)
                    SegmentLength = FormatNumber(SegmentLength, 4)
                Else
                    '
                    ' Rounding
                    '
                    TotalWidth = FormatNumber(SegmentWidth, 4)
                    TotalLength = FormatNumber(SegmentLength, 4)
                    SegmentWidth = FormatNumber(SegmentWidth, 4)
                    SegmentLength = FormatNumber(SegmentLength, 4)
                End If

                '
                ' Confirm number of segments
                '
                Prompt.Clear()
                With Prompt
                    .Append($"Number of segments = {NumOfSegments}").AppendLine()
                    .Append($"{vbTab}Total Width = {TotalWidth}").AppendLine()
                    .Append($"{vbTab}Total Length = {TotalLength}").AppendLine()
                    .Append($"Total for using one segment:").AppendLine()
                    .Append($"{vbTab}Total Width = {SegmentWidth}").AppendLine()
                    .Append($"{vbTab}Total Length = {SegmentLength}").AppendLine().AppendLine()
                    .Append("Do you want to change the number of segments?")
                End With
            Loop Until MsgBox(Prompt.ToString(), vbYesNo, NoTitle).Equals(MsgBoxResult.No)
        Else

            '
            ' Assign total dimensions and Round
            '
            TotalWidth = FormatNumber(SegmentWidth, 4)
            TotalLength = FormatNumber(SegmentLength, 4)
        End If

        '
        ' Create PDF
        '
        CreatePDF(NumOfSegments, TotalWidth, TotalLength, SegmentWidth, SegmentLength)

    End Sub

    Private Sub CreatePDF(NumOfSegments As Double, TotalWidth As Double, TotalLength As Double, SegmentWidth As Double, SegmentLength As Double)
        '
        ' Pdf Document
        '
        Dim Document As New PdfDocument()
        Dim Page = Document.AddPage()
        Dim Graphics = XGraphics.FromPdfPage(Page)
        '
        ' Logo
        '
        Dim LogoFileName = "processbarron_logo_dark.png"
        If Not File.Exists(LogoFileName) Then
            My.Resources.processbarron_logo_dark.Save(LogoFileName, ImageFormat.Png)
        End If
        Dim ProcessBarronLogo = XImage.FromFile(LogoFileName)
        Graphics.DrawImage(ProcessBarronLogo, 10, 10)
        '
        ' Styling and Formatting
        '
        Dim TextColor = XBrushes.Black
        Dim Format = XStringFormats.TopRight
        Dim Layout As New XRect(-20, 10, Page.Width, Page.Height)
        Dim Font As New XFont("Arial", 20)
        '
        ' Title
        '
        Graphics.DrawString("Cone Development", Font, TextColor, Layout, Format)
        Layout.Y += 30
        Graphics.DrawString($"Job Number: {JobNumber}", Font, TextColor, Layout, Format)
        Layout.Y += 30
        Graphics.DrawString($"Piece Name: {PieceName}", Font, TextColor, Layout, Format)
        '
        ' Input
        '
        Dim TitleFont As New XFont("Arial", 25, XFontStyle.Bold)
        Dim TitleFormat = XStringFormats.Center
        Dim HeaderPoint = Page.Width.Point / 7.5
        Dim ResultPoint = Page.Width.Point / 1.5
        Layout.X = 0
        Layout.Y = -280
        Graphics.DrawString("Input", TitleFont, TextColor, Layout, TitleFormat)
        Format = XStringFormats.CenterLeft

        Layout.X = HeaderPoint
        Layout.Y += 30
        Graphics.DrawString("Plate Type: ", Font, TextColor, Layout, Format)
        Layout.X = ResultPoint
        Graphics.DrawString(GetPlateType(), Font, TextColor, Layout, Format)

        Layout.X = HeaderPoint
        Layout.Y += 30
        Graphics.DrawString("Outer Radius: ", Font, TextColor, Layout, Format)
        Layout.X = ResultPoint
        Graphics.DrawString(OuterRadius & " in.", Font, TextColor, Layout, Format)

        Layout.X = HeaderPoint
        Layout.Y += 30
        Graphics.DrawString("Inner Radius: ", Font, TextColor, Layout, Format)
        Layout.X = ResultPoint
        Graphics.DrawString(InnerRadius & " in.", Font, TextColor, Layout, Format)

        Layout.X = HeaderPoint
        Layout.Y += 30
        Graphics.DrawString("Height: ", Font, TextColor, Layout, Format)
        Layout.X = ResultPoint
        Graphics.DrawString(ConeHeight & " in.", Font, TextColor, Layout, Format)

        Layout.X = HeaderPoint
        Layout.Y += 30
        Graphics.DrawString("Plate Thickness: ", Font, TextColor, Layout, Format)
        Layout.X = ResultPoint
        Graphics.DrawString(PlateThickness & " in.", Font, TextColor, Layout, Format)
        '
        ' Ouput
        '
        Layout.X = 0
        Layout.Y += 50
        Graphics.DrawString("Output", TitleFont, TextColor, Layout, TitleFormat)

        Layout.X = HeaderPoint
        Layout.Y += 30
        Graphics.DrawString("Outside Radius: ", Font, TextColor, Layout, Format)
        Layout.X = ResultPoint
        Graphics.DrawString(OutsideRadius & " in.", Font, TextColor, Layout, Format)

        Layout.X = HeaderPoint
        Layout.Y += 30
        Graphics.DrawString("Inside Radius: ", Font, TextColor, Layout, Format)
        Layout.X = ResultPoint
        Graphics.DrawString(InsideRadius & " in.", Font, TextColor, Layout, Format)

        Layout.X = HeaderPoint
        Layout.Y += 30
        Graphics.DrawString("Difference: ", Font, TextColor, Layout, Format)
        Layout.X = ResultPoint
        Graphics.DrawString(Difference & " in.", Font, TextColor, Layout, Format)

        Layout.X = HeaderPoint
        Layout.Y += 30
        Graphics.DrawString("Degree: ", Font, TextColor, Layout, Format)
        Layout.X = ResultPoint
        Graphics.DrawString(Degree, Font, TextColor, Layout, Format)

        Layout.X = HeaderPoint
        Layout.Y += 30
        Graphics.DrawString("First Cut-Off Radius: ", Font, TextColor, Layout, Format)
        Layout.X = ResultPoint
        Graphics.DrawString(FirstCutOffRadius & " in.", Font, TextColor, Layout, Format)

        Layout.X = HeaderPoint
        Layout.Y += 30
        Graphics.DrawString("Second Cut-Off Radius: ", Font, TextColor, Layout, Format)
        Layout.X = ResultPoint
        Graphics.DrawString(SecondCutOffRadius & " in.", Font, TextColor, Layout, Format)
        '
        ' Plate Size Information
        '
        Layout.X = 0
        Layout.Y += 50
        If NumOfSegments > 1 Then
            Graphics.DrawString("Plate Size Comparison", TitleFont, TextColor, Layout, TitleFormat)
            Layout.X = HeaderPoint
            Layout.Y += 30
            Graphics.DrawString("Number of Segments: ", Font, TextColor, Layout, Format)
            Layout.X = ResultPoint
            Graphics.DrawString(NumOfSegments, Font, TextColor, Layout, Format)
        Else
            Graphics.DrawString("Plate Size Information", TitleFont, TextColor, Layout, TitleFormat)
        End If

        Layout.X = 0
        Layout.Y += 30
        Graphics.DrawString($"Plate Usage for 1 Segment", Font, TextColor, Layout, TitleFormat)

        Layout.X = HeaderPoint
        Layout.Y += 30
        Graphics.DrawString("Total Width: ", Font, TextColor, Layout, Format)
        Layout.X = ResultPoint
        Graphics.DrawString(TotalWidth & " in.", Font, TextColor, Layout, Format)

        Layout.X = HeaderPoint
        Layout.Y += 30
        Graphics.DrawString("Total Length: ", Font, TextColor, Layout, Format)
        Layout.X = ResultPoint
        Graphics.DrawString(TotalLength & " in.", Font, TextColor, Layout, Format)

        If NumOfSegments > 1 Then
            Layout.X = 0
            Layout.Y += 30
            Graphics.DrawString($"Plate Usage for {NumOfSegments} Segments", Font, TextColor, Layout, TitleFormat)

            Layout.X = HeaderPoint
            Layout.Y += 30
            Graphics.DrawString("Total Width: ", Font, TextColor, Layout, Format)
            Layout.X = ResultPoint
            Graphics.DrawString(SegmentWidth & " in.", Font, TextColor, Layout, Format)

            Layout.X = HeaderPoint
            Layout.Y += 30
            Graphics.DrawString("Total Length: ", Font, TextColor, Layout, Format)
            Layout.X = ResultPoint
            Graphics.DrawString(SegmentLength & " in.", Font, TextColor, Layout, Format)
        End If
        '
        ' Save and view pdf
        '
        Dim CurrentDirectory = Directory.GetCurrentDirectory()
        Directory.SetCurrentDirectory(KnownFolders.Downloads.Path)
        Document.Save($"{PieceName}.pdf")
        Dim PdfFile As New ProcessStartInfo("cmd", $"/r ""{KnownFolders.Downloads.Path}\{PieceName}.pdf""")
        PdfFile.CreateNoWindow = True
        Process.Start(PdfFile)
        Directory.SetCurrentDirectory(CurrentDirectory)
    End Sub

    Private Function GetPlateType() As String
        Dim PlateTypeString = [Enum].GetName(PlateType)
        Return $"{PlateTypeString.Chars(0)}{PlateTypeString.Substring(1).ToLower()} Plates"
    End Function

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
    Private Sub Form_KeyPress(sender As Object, e As KeyPressEventArgs) Handles TextBox1.KeyPress, TextBox3.KeyPress, TextBox4.KeyPress, TextBox5.KeyPress, TextBox6.KeyPress

        '
        ' Listen for 'Enter' and begin calculation
        '
        If Char.IsWhiteSpace(e.KeyChar) Then
            Button1_Click(sender, e)
        End If
    End Sub

End Class
