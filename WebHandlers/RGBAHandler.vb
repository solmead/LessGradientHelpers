Imports System.Web
Imports System.IO
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Text.RegularExpressions

Public Class RGBAHandler : Implements IHttpHandler
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

    Private Function GetTransparencyImage(context As HttpContext) As FileInfo
        Dim Response = context.Response
        Dim Request = context.Request
        Dim Server = context.Server

        Dim R = Val(Request("R"))
        Dim B = Val(Request("B"))
        Dim G = Val(Request("G"))
        Dim A = Val(Request("A"))
        If A = 0 Then
            A = 100
        End If


        Dim Fname As String = R.ToString("000") & "_" & G.ToString("000") & "_" & B.ToString("000") & "_" & A.ToString("000") & ".png"
        Dim Fi = New System.IO.FileInfo(Server.MapPath("/Uploads/BGs/" & Fname))
        If Not Fi.Exists Then
            If Not Fi.Directory.Exists Then Fi.Directory.Create()
            Dim Bmp As New System.Drawing.Bitmap(10, 10, System.Drawing.Imaging.PixelFormat.Format32bppArgb)
            Dim Gr = System.Drawing.Graphics.FromImage(Bmp)
            Gr.Clear(System.Drawing.Color.FromArgb(Int(255 * A / 100), R, G, B))
            Gr.Dispose()

            Bmp.Save(Fi.FullName, System.Drawing.Imaging.ImageFormat.Png)
            Fi.Refresh()
        End If
        Return Fi
    End Function
    Private Function GetGradientImage(context As HttpContext) As FileInfo
        Dim Response = context.Response
        Dim Request = context.Request
        Dim Server = context.Server
        Dim fromColor As String = Request("from")
        Dim toColor As String = Request("to")
        Dim height As Integer = CInt(Val(Request("height")))
        Dim stops As String = Request("stops")
        Dim direction As String = Request("direction")

        If (fromColor Is Nothing) Then fromColor = ""
        If (toColor Is Nothing) Then toColor = ""
        If (stops Is Nothing) Then stops = ""
        If (direction Is Nothing) Then direction = "vertical"
        If height = 0 Then
            height = 200
        End If

        Dim Fname As String = fromColor & "_" & stops & "_" & toColor & "_" & direction & ".png"
        Fname = Regex.Replace(Fname.Trim().Replace(" ", "-"), "[^\w-]", "-")

        Dim Fi = New System.IO.FileInfo(Server.MapPath("/Uploads/BGs/" & Fname))
        If True OrElse Not Fi.Exists Then
            If Not Fi.Directory.Exists Then Fi.Directory.Create()
            Dim Bmp As New System.Drawing.Bitmap(1, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb)

            Dim rect = New Rectangle(0, 0, 1, height)


            Dim Gr = System.Drawing.Graphics.FromImage(Bmp)
            Gr.Clear(System.Drawing.Color.FromArgb(0, 0, 0, 0))
            Using br As New LinearGradientBrush( _
rect, Color.Blue, Color.White, 90.0F)
                ' Create a ColorBlend object. Note that you
                ' must initialize it before you save it in the
                ' brush's InterpolationColors property.
                Dim ColorBlend As New ColorBlend()
                Dim cList As New List(Of Color)
                Dim pList As New List(Of Single)
                Dim ctx = Drawing.ColorTranslator.FromHtml("#" & GetColorFromString(fromColor).ToLower())
                ctx = Color.FromArgb(CInt(Val(GetAlphaFromString(fromColor)) * 255), ctx)
                cList.Add(ctx)
                pList.Add(0)
                If Not String.IsNullOrWhiteSpace(stops) Then
                    Dim stoplist = stops.Split(",").ToList
                    For Each st In stoplist
                        Dim stlist = st.Split("-")
                        If stlist.Length >= 2 Then
                            ctx = Drawing.ColorTranslator.FromHtml("#" & GetColorFromString(stlist(1)).ToLower())
                            ctx = Color.FromArgb(CInt(Val(GetAlphaFromString(stlist(1))) * 255), ctx)
                            cList.Add(ctx)
                            pList.Add(CSng(Val(stlist(0).Replace("%", "")) / 100))
                        End If
                    Next
                End If
                ctx = Drawing.ColorTranslator.FromHtml("#" & GetColorFromString(toColor).ToLower())
                ctx = Color.FromArgb(CInt(Val(GetAlphaFromString(toColor)) * 255), ctx)
                cList.Add(ctx)
                pList.Add(1)

                ColorBlend.Colors = cList.ToArray
                ColorBlend.Positions = pList.ToArray
                br.InterpolationColors = ColorBlend
                Gr.FillRectangle(br, rect)
            End Using



            Gr.Dispose()

            Bmp.Save(Fi.FullName, System.Drawing.Imaging.ImageFormat.Png)
            Fi.Refresh()
        End If
        Return Fi
    End Function


    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        Dim Response = context.Response
        Dim Request = context.Request
        Dim Server = context.Server

        Dim fi As FileInfo = Nothing
        If Not String.IsNullOrWhiteSpace(Request("R")) Then
            fi = GetTransparencyImage(context)
        Else
            fi = GetGradientImage(context)
        End If

        'Response.ContentType = "image/png"
        'Response.Clear()
        Response.AddHeader("Content-Type", "image/png")
        Response.AddHeader("Content-Disposition", "attachment; filename=" & fi.Name & "; size=" & fi.Length.ToString())
        context.Response.Cache.SetCacheability(HttpCacheability.Public)
        context.Response.Cache.VaryByParams("*") = True
        context.Response.Cache.SetAllowResponseInBrowserHistory(True)
        context.Response.Cache.SetExpires(DateTime.Now.AddDays(30))
        context.Response.Cache.SetMaxAge(New TimeSpan(30, 0, 0))
        context.Response.Cache.SetLastModified(fi.LastWriteTime)

        Response.TransmitFile(fi.FullName)

    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property


End Class
