Imports System.Math
Imports System.Text

Public Class Form1
    Public JobNumber As String
    Public PieceName As String
    Public OuterRadius As Double
    Public InnerRadius As Double
    Public ConeHeight As Double
    Public PlateThickness As Double
    Public PlateType As Plate

    Dim OutsideRadius As Double
    Dim InsideRadius As Double
    Dim Difference As Double
    Dim Degree As Double
    Dim FirstCutOffRadius As Double
    Dim SecondCutOffRadius As Double

    Public Enum Plate
        INNER
        OUTER
    End Enum

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click

        ' Assign variables

        Dim Seg As Integer
        Dim W As Double
        Dim TL As Double
        Dim W1 As Double
        Dim L1 As Double

        Dim Q As Double
        Dim B As Double
        Dim C As Double
        Dim D As Double
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

        JobNumber = TextBox1.Text
        PieceName = TextBox2.Text
        ' TODO Validate Double value onchange
        OuterRadius = TextBox3.Text
        InnerRadius = TextBox4.Text
        ConeHeight = TextBox5.Text
        PlateThickness = TextBox6.Text
        PlateType = If(RadioButton1.Checked, Plate.INNER, Plate.OUTER)

        ' Validate Radius value
        If OuterRadius <= 0 Then
            MsgBox("The outer radius must be greater than zero.", vbOKOnly)
            Return
        ElseIf InsideRadius >= OuterRadius Then
            MsgBox("The inner radius must be less than outer radius.", vbOKOnly)
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
        Deg = 2 * PI * Cos(Theta)
        Degree = (Deg * 180) / PIE

        ' Line 540
        If Deg > 0 AndAlso Deg <= PIE / 2 Then 'goto 1290
            C = PIE / 2 - Deg
            FirstCutOffRadius = 2 * OutsideRadius * Sin(Deg / 2)
            L1 = FirstCutOffRadius + 1
            B = (Sin(Deg / 4)) ^ 2 * InsideRadius * 2
            W1 = (OutsideRadius - InsideRadius) + B + 1
            OutsideRadius = Int(OutsideRadius * 16 + 0.5) / 16
            InsideRadius = Int(InsideRadius * 16 + 0.5) / 16
            Difference = OutsideRadius - InsideRadius
            FirstCutOffRadius = Int(FirstCutOffRadius * 16 + 0.5) / 16
            SecondCutOffRadius = Int(SecondCutOffRadius * 16 + 0.5) / 16
        ElseIf Deg > PIE / 2 AndAlso Deg <= PIE Then ' goto 1360
            C = PI - Deg
            L1 = Sin(Deg / 2) * OutsideRadius * 2 + 1
            W1 = (Sin(Deg / 4)) ^ 2 * InsideRadius * 2 + (OutsideRadius - InsideRadius) + 1
        ElseIf Deg > PIE AndAlso Deg <= 3 * PIE / 2 Then ' goto 1400
            C = 3 * PI / 2 - Deg
            L1 = OutsideRadius * 2 + 1
            W1 = Sin(Deg - PI) * OutsideRadius + OutsideRadius + 1
        ElseIf Deg > 3 * PIE / 2 AndAlso Deg <= 2 * PIE Then ' goto 1440
            C = 2 * PI - Deg
            L1 = OutsideRadius * 2 + 1
            W1 = L1
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
        If MsgBox(prompt.ToString(), vbYesNo).Equals(MsgBoxResult.Yes) Then
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
                TL = (Seg - 1) * V3 + Z + Difference + 1
                W = Sin(Degr / 2) * OutsideRadius * 2 + 1
                prompt.Clear()
                With prompt
                    .Append($"Number of segments = {Seg}").AppendLine()
                    .Append($"\tTotal Width = {W}").AppendLine()
                    .Append($"\tTotal Length = {TL}").AppendLine()
                    .Append($"Segment dimensions:").AppendLine()
                    .Append($"\tTotal Width = {W1}").AppendLine()
                    .Append($"\tTotal Length = {L1}").AppendLine()
                    .Append("Do you want to change the number of segments?")
                End With
            Loop Until MsgBox(prompt.ToString(), vbYesNo).Equals(MsgBoxResult.No)
        End If

        ' Test to show contents
        prompt.Clear()
        With prompt
            .Append(JobNumber).AppendLine()
            .Append(PieceName).AppendLine()
            .Append("Input").AppendLine()
            .Append(OuterRadius).AppendLine()
            .Append(InnerRadius).AppendLine()
            .Append(ConeHeight).AppendLine()
            .Append(PlateThickness).AppendLine()
            .Append("Output").AppendLine()
            .Append(OutsideRadius).AppendLine()
            .Append(InsideRadius).AppendLine()
            .Append(Difference).AppendLine()
            .Append(Degree).AppendLine()
            .Append(FirstCutOffRadius).AppendLine()
            .Append(SecondCutOffRadius).AppendLine()
        End With
        MsgBox(prompt.ToString())

    End Sub

End Class
