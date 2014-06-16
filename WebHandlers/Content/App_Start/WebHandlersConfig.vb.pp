imports System

<assembly: WebActivatorEx.PreApplicationStartMethod(
    gettype($rootnamespace$.App_Start.WebHandlerConfig), "PreStart")>

Namespace $rootnamespace$.App_Start 
    public class WebHandlerConfig 
        public shared sub PreStart() 
			RouteTable.Routes.IgnoreRoute("{*allaxd}", New With {.allaxd = ".*\.axd(/.*)?"})
        end sub
    End Class
End Namespace