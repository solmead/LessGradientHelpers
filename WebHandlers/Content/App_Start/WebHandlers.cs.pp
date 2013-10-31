using System;
using System.Web.Mvc;
using System.Web.Routing;


[assembly: WebActivatorEx.PreApplicationStartMethod(
    typeof($rootnamespace$.App_Start.WebHandlerPackage), "PreStart")]

namespace $rootnamespace$.App_Start {
    public static class WebHandlerPackage {
        public static void PreStart() {
			RouteTable.Routes.IgnoreRoute("{*allaxd}", new { allaxd = @".*\.axd(/.*)?" });
        }
    }
}