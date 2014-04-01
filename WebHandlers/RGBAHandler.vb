Imports System.Web
Imports System.IO
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Text.RegularExpressions

Public Class RGBAHandler : Implements IHttpHandler
    Private Delegate Function GetImage() As MemoryStream
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

    Private Function CanWriteToDirecotry(fi As FileInfo) As Boolean
        Dim canWrite = False
        Try
            If Not fi.Directory.Exists Then
                fi.Directory.Create()
            End If
            If fi.Exists AndAlso fi.CreationTime.AddDays(7) > Now Then
                fi.Delete()
            End If
            Dim fi2 = New FileInfo(fi.Directory.FullName + "/temp.tmp")
            If fi2.Exists Then
                fi2.Delete()
            End If
            Dim fs = New StreamWriter(fi2.OpenWrite())
            fs.Write("Test")
            fs.Close()
            fi2.Delete()
            canWrite = True
        Catch ex As Exception

        End Try
        Return canWrite
    End Function

    Private Function GetCache(context As HttpContext, fileName As String, getImage As GetImage) As MemoryStream
        Dim Request = context.Request
        Dim Server = context.Server
        Dim ms As New MemoryStream
        Dim Fi = New System.IO.FileInfo(Server.MapPath(My.Settings.TempDirectory & "/" & fileName))
        If CanWriteToDirecotry(Fi) Then
            Try
                If (Fi.Exists) Then
                    Dim fs2 = New BinaryReader(Fi.OpenRead())
                    ms.Write(fs2.ReadBytes(Fi.Length), 0, Fi.Length)
                    fs2.Close()
                    Return ms
                End If
            Catch ex As Exception

            End Try
            ms = getImage()
            ms.Seek(0, SeekOrigin.Begin)
            Dim fs = New BinaryWriter(Fi.OpenWrite())
            fs.Write(ms.ToArray())
            fs.Close()
            Return ms
        Else
            Return getImage()
        End If
    End Function

    Private Function GetTransparencyImage(context As HttpContext) As MemoryStream
        Dim Request = context.Request
        Dim Server = context.Server

        Dim R = Val(Request("R"))
        Dim B = Val(Request("B"))
        Dim G = Val(Request("G"))
        Dim A = Val(Request("A"))
        If A = 0 Then
            A = 100.0
        End If
        Dim Fname As String = R.ToString("000") & "_" & G.ToString("000") & "_" & B.ToString("000") & "_" & A.ToString("000") & ".png"
        Dim ms = GetCache(context, Fname, Function()
                                              Dim mems As New MemoryStream()
                                              Dim Bmp As New System.Drawing.Bitmap(10, 10, System.Drawing.Imaging.PixelFormat.Format32bppArgb)
                                              Dim Gr = System.Drawing.Graphics.FromImage(Bmp)
                                              Gr.Clear(System.Drawing.Color.FromArgb(Int(255 * A / 100), R, G, B))
                                              Gr.Dispose()
                                              Bmp.Save(mems, System.Drawing.Imaging.ImageFormat.Png)
                                              Return mems
                                          End Function)

        Return ms
    End Function
    Private Function GetGradientImage(context As HttpContext) As MemoryStream
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
        Dim ms = GetCache(context, Fname, Function()
                                              Dim mems As New MemoryStream()
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
                                              Bmp.Save(mems, System.Drawing.Imaging.ImageFormat.Png)
                                              Return mems
                                          End Function)

        Return ms
    End Function


    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        Try

            Dim Response = context.Response
            Dim Request = context.Request
            Dim Server = context.Server

            Dim ms As MemoryStream = Nothing
            If Not String.IsNullOrWhiteSpace(Request("R")) Then
                ms = GetTransparencyImage(context)
            Else
                ms = GetGradientImage(context)
            End If
            ms.Seek(0, SeekOrigin.Begin)

            'Response.ContentType = "image/png"
            'Response.Clear()
            Response.AddHeader("Content-Type", "image/png")
            Response.AddHeader("Content-Disposition", "attachment; filename=" & Guid.NewGuid.ToString() & ".png; size=" & ms.Length.ToString())
            context.Response.Cache.SetCacheability(HttpCacheability.Public)
            context.Response.Cache.VaryByParams("*") = True
            context.Response.Cache.SetAllowResponseInBrowserHistory(True)
            context.Response.Cache.SetExpires(DateTime.Now.AddDays(30))
            context.Response.Cache.SetMaxAge(New TimeSpan(30, 0, 0))
            'context.Response.Cache.SetLastModified(fi.LastWriteTime)
            Response.BinaryWrite(ms.ToArray())
            'Response.TransmitFile(fi.FullName)
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
