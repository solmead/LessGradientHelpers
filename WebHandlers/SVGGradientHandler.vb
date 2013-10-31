Imports System.Web
Imports System.Text

Public Class SVGGradientHandler : Implements IHttpHandler

    Private Function GetColorFromString(color As String) As String
        If color.Length = 8 Then
            Return color.Substring(2)
        Else
            Return color
        End If
    End Function

    Private Function GetAlphaFromString(color As String) As String
        If color.Length = 8 Then
            Dim al = color.Substring(0, 2)
            Dim v = Convert.ToByte(al, &H10)
            Return v / 255
        Else
            Return "1"
        End If
    End Function

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        Try

            Dim Response = context.Response
            Dim Request = context.Request
            Dim Server = context.Server
            Dim fromColor As String = Request("from")
            Dim toColor As String = Request("to")
            Dim stop1Percent As String = Request("stop1Percent")
            Dim stop1Color As String = Request("stop1Color")
            Dim stop2Percent As String = Request("stop2Percent")
            Dim stop2Color As String = Request("stop2Color")
            Dim stop3Percent As String = Request("stop3Percent")
            Dim stop3Color As String = Request("stop3Color")

            Dim stops As String = Request("stops")
            Dim a = 1

            Dim direction As String = Request("direction")

            If (fromColor Is Nothing) Then fromColor = ""
            If (toColor Is Nothing) Then toColor = ""
            If (stop1Percent Is Nothing) Then stop1Percent = ""
            If (stop1Color Is Nothing) Then stop1Color = ""
            If (stop2Percent Is Nothing) Then stop2Percent = ""
            If (stop2Color Is Nothing) Then stop2Color = ""
            If (stop3Percent Is Nothing) Then stop3Percent = ""
            If (stop3Color Is Nothing) Then stop3Color = ""
            If (direction Is Nothing) Then direction = "vertical"


            context.Response.ContentType = "image/svg+xml; charset=utf-8"

            stop1Percent = stop1Percent.Replace("%", "")
            stop2Percent = stop2Percent.Replace("%", "")
            stop3Percent = stop3Percent.Replace("%", "")
            Dim sb = New StringBuilder()
            sb.AppendLine("<?xml version=""1.0""?>")
            sb.AppendLine("<svg xmlns=""http://www.w3.org/2000/svg"" version=""1.1"" width=""100%"" height=""100%"">")
            sb.AppendLine("<defs>")
            If (direction = "vertical") Then
                sb.AppendLine("<linearGradient id=""linear-gradient"" x1=""0%"" y1=""0%"" x2=""0%"" y2=""100%"">")

            ElseIf direction = "horizontal" Then
                sb.AppendLine("<linearGradient id=""linear-gradient"" x1=""0%"" y1=""0%"" x2=""100%"" y2=""0%"">")

            End If
            sb.AppendLine("<stop offset=""0%"" stop-color=""#" + GetColorFromString(fromColor).ToLower() + """ stop-opacity=""" & GetAlphaFromString(fromColor) & """/>")

            If Not String.IsNullOrWhiteSpace(stops) Then
                Dim stoplist = stops.Split(",").ToList
                For Each st In stoplist
                    Dim stlist = st.Split("-")
                    If stlist.Length >= 2 Then
                        sb.AppendLine("<stop offset=""" + stlist(0).Replace("%", "") + "%"" stop-color=""#" + GetColorFromString(stlist(1)).ToLower() + """ stop-opacity=""" & GetAlphaFromString(stlist(1)) & """/>")
                    End If
                Next
            End If


            If (stop1Percent <> "") Then
                sb.AppendLine("<stop offset=""" + stop1Percent + "%"" stop-color=""#" + GetColorFromString(stop1Color).ToLower() + """ stop-opacity=""" & GetAlphaFromString(stop1Color) & """/>")
            End If
            If (stop2Percent <> "") Then
                sb.AppendLine("<stop offset=""" + stop2Percent + "%"" stop-color=""#" + GetColorFromString(stop2Color).ToLower() + """ stop-opacity=""" & GetAlphaFromString(stop2Color) & """/>")
            End If
            If (stop3Percent <> "") Then
                sb.AppendLine("<stop offset=""" + stop3Percent + "%"" stop-color=""#" + GetColorFromString(stop3Color).ToLower() + """ stop-opacity=""" & GetAlphaFromString(stop3Color) & """/>")
            End If

            sb.AppendLine("<stop offset=""100%"" stop-color=""#" + GetColorFromString(toColor).ToLower() + """ stop-opacity=""" & GetAlphaFromString(toColor) & """/>")
            sb.AppendLine("</linearGradient>")
            sb.AppendLine("</defs>")
            sb.AppendLine("<rect width=""100%"" height=""100%"" fill=""url(#linear-gradient)""/>")
            sb.AppendLine("</svg>")



            context.Response.Cache.SetCacheability(HttpCacheability.Public)
            context.Response.Cache.VaryByParams("*") = True
            context.Response.Cache.SetAllowResponseInBrowserHistory(True)
            context.Response.Cache.SetExpires(DateTime.Now.AddDays(30))
            context.Response.Cache.SetMaxAge(New TimeSpan(30, 0, 0))
            context.Response.Cache.SetLastModified(Now.AddDays(-30))

            context.Response.Write(sb.ToString())
        Catch ex As Exception
            context.Response.Write(ex.ToString())
        End Try
    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property


End Class
