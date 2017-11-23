﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using DevExpress.Logify.Core;
using DevExpress.Logify.Core.Internal;

namespace DevExpress.Logify.Web {
    public class AspBreadcrumbsRecorder : BreadcrumbsRecorderBase {
        static volatile AspBreadcrumbsRecorder instance = null;
        public static AspBreadcrumbsRecorder Instance {
            get {
                if(instance != null)
                    return instance;

                InitializeInstance();
                return instance;
            }
        }

        private AspBreadcrumbsRecorder() { }

        internal static void InitializeInstance() {
            lock(typeof(AspBreadcrumbsRecorder)) {
                if(instance != null)
                    return;
                instance = new AspBreadcrumbsRecorder();
            }
        }
        internal void AddBreadcrumb(HttpApplication httpApplication) {
            if(httpApplication == null)
                return;

            HttpRequest request = httpApplication.Context.Request;
            if(request == null)
                return;

            HttpResponse response = httpApplication.Context.Response;
            if(response == null)
                return;

            Breadcrumb breadcrumb = new Breadcrumb();
            breadcrumb.Event = BreadcrumbEvent.Request;
            breadcrumb.CustomData = new Dictionary<string, string>() {
                { "method", request.HttpMethod },
                { "url", request.Url.ToString() },
                { "status", response.StatusDescription },
                { "session", TryGetSessionId(request, response) }
            };

            base.AddBreadcrumb(breadcrumb);
        }
        internal void UpdateBreadcrumb(HttpApplication httpApplication) {
            if(httpApplication == null)
                return;

            HttpRequest request = httpApplication.Context.Request;
            if(request == null)
                return;

            Breadcrumb breadcrumb = LogifyAlert.Instance.Breadcrumbs.Where(b =>
                b.GetIsAuto() &&
                b.Event == BreadcrumbEvent.Request &&
                b.CustomData != null &&
                b.CustomData["method"] == request.HttpMethod &&
                b.CustomData["url"] == request.Url.ToString()
            ).First();

            if(breadcrumb != null)
                breadcrumb.CustomData["status"] = "Failed";
        }
        protected override string GetCategory() {
            return "request";
        }
        protected override string GetThreadId() {
            return Thread.CurrentThread.ManagedThreadId.ToString();
        }
        const string CookieName = "BreadcrumbsCookie";
        string TryGetSessionId(HttpRequest request, HttpResponse response) {
            string cookieValue = null;
            try {
                //HttpCookie standardCookie = request.Cookies["ASP.NET_SessionId"];
                //if(standardCookie != null)
                //    cookieValue = standardCookie.Value;
                //if(string.IsNullOrEmpty(cookieValue)) {
                HttpCookie cookie = request.Cookies[CookieName];
                if(cookie != null) {
                    Guid validGuid = Guid.Empty;
                    if(Guid.TryParse(cookie.Value, out validGuid))
                        cookieValue = cookie.Value;
                } else {
                    cookieValue = Guid.NewGuid().ToString();
                    cookie = new HttpCookie(CookieName, cookieValue);
                    cookie.HttpOnly = true;
                    response.Cookies.Add(cookie);
                }
                //}
            } catch { }
            return cookieValue;
        }
    }
}
