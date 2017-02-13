﻿using System;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using DevExpress.Logify.Core;
using Microsoft.AspNetCore.Http;

namespace DevExpress.Logify.Web {
    class RequestCollector : IInfoCollector {
        readonly HttpRequest request;
        readonly string name;
        private ILogger logger;

        public RequestCollector(HttpRequest request) : this(request, "request") { }

        public RequestCollector(HttpRequest request, string name) {
            this.request = request;
            this.name = name;
        }

        public virtual void Process(Exception e, ILogger logger) {
            this.logger = logger;
            try {
                logger.BeginWriteObject(name);
                //logger.WriteValue("acceptTypes", request.AcceptTypes);
                //this.SerializeBrowserInfo();
                this.SerializeContentInfo();
                this.SerializeFileInfo();
                Utils.SerializeCookieInfo(request.Cookies, logger);
                try {
                    Utils.SerializeInfo(request.Form, "form", logger);
                }
                catch {
                }
                Utils.SerializeInfo(request.Headers, "headers", logger);
                //Utils.SerializeInfo(request.ServerVariables, "serverVariables", logger);
                Utils.SerializeInfo(request.Query, "queryString", logger);
                //logger.WriteValue("applicationPath", request.ApplicationPath);
                logger.WriteValue("httpMethod", request.Method);
                try {
                    string content;
                    using (var reader = new System.IO.StreamReader(request.Body))
                        content = reader.ReadToEnd();
                    logger.WriteValue("httpRequestBody", content);
                }
                catch {
                }
                //logger.WriteValue("isUserAuthenticated", request.IsAuthenticated);
                //logger.WriteValue("isLocalRequest", request.IsLocal);
                logger.WriteValue("isSecureConnection", request.IsHttps);
                //logger.WriteValue("requestType", request.RequestType); // request.Protocol ?
                //logger.WriteValue("totalBytes", request.TotalBytes);
            }
            finally {
                logger.EndWriteObject(name);
            }
        }

        void SerializeBrowserInfo() {
            /*
            try {
                logger.BeginWriteObject("browser");
                HttpBrowserCapabilities browserInfo = request.Browser;
                logger.WriteValue("activeXSupport", browserInfo.ActiveXControls);
                logger.WriteValue("isBeta", browserInfo.Beta);
                logger.WriteValue("userAgent", browserInfo.Browser);
                logger.WriteValue("canVoiceCall", browserInfo.CanInitiateVoiceCall);
                logger.WriteValue("canSendMail", browserInfo.CanSendMail);
                logger.WriteValue("cookiesSupport", browserInfo.Cookies);
                logger.WriteValue("isCrawler", browserInfo.Crawler);
                logger.WriteValue("EcmaScriptVersion", browserInfo.EcmaScriptVersion.ToString());
                logger.WriteValue("frameSupport", browserInfo.Frames);
                logger.WriteValue("gatewayVersion", browserInfo.GatewayVersion.ToString());
                logger.WriteValue("inputType", browserInfo.InputType);
                logger.WriteValue("isMobileDevice", browserInfo.IsMobileDevice);
                logger.WriteValue("javaAppletSupport", browserInfo.JavaApplets);
                if (browserInfo.IsMobileDevice) {
                    logger.WriteValue("mobileDeviceManufacturer", browserInfo.MobileDeviceManufacturer);
                    logger.WriteValue("mobileDeviceModel", browserInfo.MobileDeviceModel);
                }
                logger.WriteValue("platform", browserInfo.Platform);
                this.SerializeScreenInfo();
                logger.WriteValue("type", browserInfo.Type);
                logger.WriteValue("version", browserInfo.Version);
                logger.WriteValue("isWin32", browserInfo.Win32);
            }
            finally {
                logger.EndWriteObject("browser");
            }
            */
        }
        /*
        void SerializeScreenInfo() {
            try {
                logger.BeginWriteObject("screen");
                logger.WriteValue("height", request.Browser.ScreenPixelsHeight);
                logger.WriteValue("width", request.Browser.ScreenPixelsWidth);
            }
            finally {
                logger.EndWriteObject("screen");
            }
        }
        */

        void SerializeContentInfo() {
            try {
                logger.BeginWriteObject("content");
                //logger.WriteValue("encoding", request.ContentEncoding.EncodingName);
                logger.WriteValue("length", request.ContentLength.HasValue ? (int)request.ContentLength : -1);
                logger.WriteValue("type", request.ContentType);
            }
            finally {
                logger.EndWriteObject("content");
            }
        }

        void SerializeFileInfo() {
            try {
                if (request.Form == null)
                    return;

                IFormFileCollection files = request.Form.Files;
                if (files == null || files.Count <= 0)
                    return;

                logger.BeginWriteObject("files");
                try {
                    foreach (IFormFile file in files) {
                        logger.BeginWriteObject(file.Name);
                        logger.WriteValue("name", file.FileName);
                        logger.WriteValue("length", (int)file.Length);
                        logger.WriteValue("type", file.ContentType);
                        logger.EndWriteObject(file.Name);
                    }
                }
                finally {
                    logger.EndWriteObject("files");
                }
            }
            catch {
            }
        }
    }
}