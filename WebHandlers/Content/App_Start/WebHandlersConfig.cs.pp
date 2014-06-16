using System;
using System.Web.Mvc;
using System.Web.Routing;


[assembly: WebActivatorEx.PreApplicationStartMethod(
    typeof($rootnamespace$.App_Start.WebHandlerConfig), "PreStart")]

namespace $rootnamespace$.App_Start {
    public static class WebHandlerConfig {
        public static void PreStart() {
			RouteTable.Routes.IgnoreRoute("{*allaxd}", new { allaxd = @".*\.axd(/.*)?" });
        }
    }
}