Imports System.IO
Imports System.Web
Imports FileInfoExtensions

Public Class IconHandler : Implements IHttpHandler

    Public Sub ProcessRequest(ByVal context As HttpContext) Implements IHttpHandler.ProcessRequest
        context.Response.ContentType = "text/css"
        Dim name As String

        name = context.Request.RequestContext.RouteData.Values("Name").ToString
        'name = context.Request("Name")


        Dim mainFile = New FileInfo(context.Server.MapPath("/Uploads/Images/SiteFiles/" & name))
        Dim oldFile = New FileInfo(context.Server.MapPath("/" & name))
        If Not mainFile.IsImage() Then
            context.Response.Write("")
            Return
        End If

        If Not mainFile.Directory.Exists Then
            mainFile.Directory.Create()
        End If
        If Not mainFile.Exists AndAlso oldFile.Exists Then
            oldFile.CopyTo(mainFile.FullName)
            mainFile.CreationTime = oldFile.CreationTime
            mainFile.LastWriteTime = oldFile.LastWriteTime
        End If
        If oldFile.Exists Then
            Try
                oldFile.Delete()
            Catch ex As Exception

            End Try
        End If
        context.Response.ContentType = mainFile.ContentType
        context.Response.Cache.SetCacheability(HttpCacheability.Public)
        context.Response.Cache.VaryByParams("*") = True
        context.Response.Cache.SetAllowResponseInBrowserHistory(True)
        context.Response.Cache.SetExpires(DateTime.Now.AddDays(30))
        context.Response.Cache.SetMaxAge(New TimeSpan(30, 0, 0))
        context.Response.Cache.SetLastModified(mainFile.LastWriteTime)
        context.Response.AddHeader("Content-Disposition", "inline; filename=" & mainFile.Name)
        context.Response.WriteFile(mainFile.FullName)
    End Sub

    Public ReadOnly Property IsReusable() As Boolean Implements IHttpHandler.IsReusable
        Get
            Return False
        End Get
    End Property


End Class
