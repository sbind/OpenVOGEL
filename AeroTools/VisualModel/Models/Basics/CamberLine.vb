﻿'Open VOGEL (www.openvogel.com)
'Open source software for aerodynamics
'Copyright (C) 2016 Guillermo Hazebrouck (guillermo.hazebrouck@openvogel.com)

'This program Is free software: you can redistribute it And/Or modify
'it under the terms Of the GNU General Public License As published by
'the Free Software Foundation, either version 3 Of the License, Or
'(at your option) any later version.

'This program Is distributed In the hope that it will be useful,
'but WITHOUT ANY WARRANTY; without even the implied warranty Of
'MERCHANTABILITY Or FITNESS FOR A PARTICULAR PURPOSE.  See the
'GNU General Public License For more details.

'You should have received a copy Of the GNU General Public License
'along with this program.  If Not, see < http:  //www.gnu.org/licenses/>.

Imports System.Xml
Imports AeroTools.VisualModel.IO
Imports MathTools.Algebra.EuclideanSpace

Namespace VisualModel.Models.Basics

    Public Class CamberLine

        Public Property Name As String = "Camber line"

        ''' <summary>
        ''' The identification number of this camber line.
        ''' </summary>
        ''' <returns></returns>
        Public Property ID As Guid

        ''' <summary>
        ''' The nodes if this camber line.
        ''' </summary>
        ''' <returns></returns>
        Public Property Nodes As New List(Of EVector2)

        ''' <summary>
        ''' Returns a NACA chamber point provided the position as fraction of the chord.
        ''' </summary>
        ''' <remarks></remarks>
        Public Sub New()

        End Sub

        ''' <summary>
        ''' Returns the Y coordinate of the basic camber function given a chordwise coordinate.
        ''' </summary>
        ''' <param name="X">Chordwise coordinate</param>
        Public Function Y(ByVal X As Single) As Single

            For i = 1 To Nodes.Count - 1

                If Nodes(i - 1).X <= X AndAlso Nodes(i).X > X Then

                    Dim dy As Single = Nodes(i).Y - Nodes(i - 1).Y
                    Dim dx As Single = Nodes(i).X - Nodes(i - 1).X
                    Dim px As Single = X - Nodes(i - 1).X

                    Return Nodes(i - 1).Y + dy / dx * px

                End If

            Next

            Return 0.0

        End Function

        Public Sub EvaluatePoint(ByRef point As EVector2, ByVal x As Double, Optional Flapped As Boolean = False, Optional FlapChord As Single = 0.2, Optional FlapDeflection As Single = 0.0)

            point.X = x
            point.Y = Y(x)

            If Flapped And 1 - x <= FlapChord + 0.01 Then

                Dim y As Double = Me.Y(1 - FlapChord)
                point.Rotate(FlapDeflection, 1 - FlapChord, y)

            End If

        End Sub

        Public Sub Assign(ByVal Camber As CamberLine)

            Nodes.Clear()

            For i = 0 To Camber.Nodes.Count - 1

                Nodes.Add(New EVector2(Camber.Nodes(i).X, Camber.Nodes(i).Y))

            Next

        End Sub

        Public Function Clone() As CamberLine

            Dim newLine As New CamberLine
            newLine.Assign(Me)
            Return newLine

        End Function

        Public Sub GenerateNaca(MaxCamber As Single, MaxCamberX As Single, Optional n As Integer = 20)

            If (MaxCamberX > 0) AndAlso (MaxCamberX < 1) Then

                Nodes.Clear()

                For i = 0 To n

                    Dim _x As Single = i / n
                    Dim _y As Single = 0

                    If _x <= MaxCamberX Then

                        _y = MaxCamber * _x / MaxCamberX ^ 2 * (2 * MaxCamberX - _x)

                    ElseIf _x > MaxCamberX Then

                        _y = MaxCamber * (1.0 - _x) / (1.0 - MaxCamberX) ^ 2 * (1 + _x - 2 * MaxCamberX)

                    Else

                        _y = 0

                    End If

                    Nodes.Add(New EVector2(_x, _y))

                Next

            End If

        End Sub

        Public Sub WriteToXML(writer As XmlWriter)

            writer.WriteStartElement("Identity")
            writer.WriteAttributeString("Name", Name)
            writer.WriteAttributeString("ID", ID.ToString)
            writer.WriteEndElement()

            For Each node In Nodes

                writer.WriteStartElement("Node")

                writer.WriteAttributeString("X", node.X)
                writer.WriteAttributeString("Y", node.Y)

                writer.WriteEndElement()

            Next

        End Sub

        Public Sub ReadFromXML(reader As XmlReader)

            Nodes.Clear()

            While reader.Read

                If Not reader.NodeType = XmlNodeType.Element Then Continue While

                Select Case reader.Name

                    Case "Identity"

                        Name = reader.GetAttribute("Name")
                        ID = New Guid(IOXML.ReadString(reader, "ID", Guid.NewGuid.ToString))

                    Case "Node"

                        Dim x As Double = IOXML.ReadDouble(reader, "X", 0.0)
                        Dim y As Double = IOXML.ReadDouble(reader, "Y", 0.0)

                        Nodes.Add(New EVector2(x, y))

                End Select

            End While

        End Sub

    End Class

    Public Module CamberLinesDatabase

        Public CamberLines As New List(Of CamberLine)

        Public Sub InitializeCamberLinesDatabase()

            CamberLines.Clear()
            Dim symmetric As New CamberLine
            symmetric.Name = "Symmetric"
            CamberLines.Add(symmetric)
            CamberLines(0).ID = Guid.Empty

        End Sub

        Public Function GetCamberLineFromID(ID As Guid) As CamberLine

            For i = 0 To CamberLines.Count - 1

                If CamberLines(i).ID.Equals(ID) Then
                    Return CamberLines(i)
                End If

            Next

            If CamberLines.Count > 0 Then
                Return CamberLines(0)
            Else
                Return Nothing
            End If

        End Function

        Public Sub WriteToXML(writer As XmlWriter)

            For Each line In CamberLines

                writer.WriteStartElement("CamberLine")

                line.WriteToXML(writer)

                writer.WriteEndElement()

            Next

        End Sub

        Public Sub ReadFromXML(reader As XmlReader)

            CamberLines.Clear()

            While reader.Read

                If Not reader.NodeType = XmlNodeType.Element Then Continue While

                Select Case reader.Name

                    Case "CamberLine"

                        Dim line As New CamberLine()

                        line.ReadFromXML(reader.ReadSubtree())

                        CamberLines.Add(line)

                End Select

            End While

        End Sub

    End Module

End Namespace

