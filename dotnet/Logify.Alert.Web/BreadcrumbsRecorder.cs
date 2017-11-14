﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.SessionState;
using DevExpress.Logify.Core;

namespace DevExpress.Logify.Web {
    public class AspBreadcrumbsRecorder : BreadcrumbsRecorderBase {
        static volatile AspBreadcrumbsRecorder instance = null;
        public static AspBreadcrumbsRecorder Instance {
            get {
                if(instance != null)
                    return instance;

                InitializeInstance(null);
                return instance;
            }
        }

        internal BreadcrumbCollection Breadcrumbs {
            get {
                return this._storage.Breadcrumbs;
            }
        }
        IBreadcrumbsStorage _storage;
        private AspBreadcrumbsRecorder() { }
        public AspBreadcrumbsRecorder(IBreadcrumbsStorage storage) {
            this._storage = storage;
        }

        internal static void InitializeInstance(IBreadcrumbsStorage storage) {
            lock(typeof(AspBreadcrumbsRecorder)) {
                if(storage != null)
                    instance = new AspBreadcrumbsRecorder(storage);
                if(instance != null)
                    return;
                else if(HttpContext.Current != null && HttpContext.Current.Session != null)
                    instance = new AspBreadcrumbsRecorder(new HttpBreadcrumbsStorage());
                else
                    instance = new AspBreadcrumbsRecorder(new InMemoryBreadcrumbsStorage());
            }
        }
        internal void SetBreadcrumbsStorage(IBreadcrumbsStorage storage) {
            InitializeInstance(storage);
        }

        internal new void AddBreadcrumb(Breadcrumb item) {
            //base.AddBreadcrumb(item);
            this._storage.Breadcrumbs.Add(item);
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

            HttpCookie cookie = request.Cookies["ASP.NET_SessionId"];

            HttpSessionState session = httpApplication.Context.Session;
            //if(session == null)
            //    return;

            Breadcrumb breadcrumb = new Breadcrumb();
            base.PopulateCommonBreadcrumbInfo(breadcrumb);
            breadcrumb.Category = "request";
            breadcrumb.Event = BreadcrumbEvent.None;
            breadcrumb.MethodName = request.HttpMethod;
            breadcrumb.CustomData = new Dictionary<string, string>() {
                { "url", request.Url.ToString() },
                { "status", response.StatusDescription },
                { "session", TryGetSessionId(cookie, session) }
            };

            this.AddBreadcrumb(breadcrumb);
        }
        internal void UpdateBreadcrumb(HttpApplication httpApplication, Exception ex) {
            Breadcrumb breadcrumb = Breadcrumbs.Where(b => b.Event == BreadcrumbEvent.None).First();
            if(breadcrumb != null) {
                HttpResponse response = httpApplication.Context.Response;
                if(response != null)
                    breadcrumb.CustomData["status"] = "Failed";
            }
        }
        protected override string GetThreadId() {
            return Thread.CurrentThread.ManagedThreadId.ToString();
        }
        string TryGetSessionId(HttpCookie cookie, HttpSessionState session) {
            string result = null;
            if(cookie != null)
                result = cookie.Value;
            if(string.IsNullOrEmpty(result) && session != null)
                result = session.SessionID;
            return result;
        }
    }
}
