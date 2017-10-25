﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Threading;
using DevExpress.Logify.Core;
using System.Diagnostics;
using System.Reflection;
using System.Security.AccessControl;
using DevExpress.Logify.Web;
using System.ComponentModel;
using DevExpress.Logify.Core.Internal;

namespace DevExpress.Logify.Web {
    public class LogifyAlert : LogifyClientBase {
        static volatile LogifyAlert instance;

        [Obsolete("Please use the LogifyAlert.Instance property instead.", true)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public LogifyAlert() {
        }
        internal LogifyAlert(bool b) {
        }
        protected LogifyAlert(string apiKey) : base(apiKey) {
        }

        public static new LogifyAlert Instance {
            get {
                if (instance != null)
                    return instance;

                InitializeInstance();
                return instance;
            }
        }
        internal static void InitializeInstance() {
            lock (typeof(LogifyAlert)) {
                if (instance != null)
                    return;

                instance = new LogifyAlert(true);
                LogifyClientBase.Instance = instance;
            }
        }

        //public bool SendReportInSeparateProcess { get; set; }

        protected internal LogifyAlert(Dictionary<string, string> config) : base(config) {
        }

        protected override IInfoCollectorFactory CreateCollectorFactory() {
            //return new WinFormsExceptionCollectorFactory();
            return new WebDefaultExceptionCollectorFactory(Platform.ASP);
        }
        protected override IInfoCollector CreateDefaultCollector(IDictionary<string, string> additionalCustomData, AttachmentCollection additionalAttachments) {
            WebExceptionCollector result = new WebExceptionCollector(Config, Platform.ASP);
            result.AppName = this.AppName;
            result.AppVersion = this.AppVersion;
            result.UserId = this.UserId;
            result.Collectors.Add(new CustomDataCollector(this.CustomData, additionalCustomData));
            result.Collectors.Add(new BreadcrumbsCollector(this.Breadcrumbs));
            result.Collectors.Add(new AttachmentsCollector(this.Attachments, additionalAttachments));
            return result;
        }
        protected override IExceptionReportSender CreateExceptionReportSender() {
            WebExceptionReportSender defaultSender = new WebExceptionReportSender();
            defaultSender.ConfirmSendReport = ConfirmSendReport;
            defaultSender.ProxyCredentials = ProxyCredentials;
            if (ConfirmSendReport)
                return defaultSender;

            //IExceptionReportSender winDefaultSender = base.CreateExceptionReportSender();
            CompositeExceptionReportSender sender = new CompositeExceptionReportSender();
            sender.StopWhenFirstSuccess = true;
            //sender.Senders.Add(new ExternalProcessExceptionReportSender());
            sender.Senders.Add(defaultSender);
            sender.Senders.Add(new OfflineDirectoryExceptionReportSender());
            return sender;
        }
        protected override IExceptionReportSender CreateEmptyPlatformExceptionReportSender() {
            return new WebExceptionReportSender();
        }
        protected override ISavedReportSender CreateSavedReportsSender() {
            return new SavedExceptionReportSender();
        }
        protected override BackgroundExceptionReportSender CreateBackgroundExceptionReportSender(IExceptionReportSender reportSender) {
            return new EmptyBackgroundExceptionReportSender(reportSender);
        }

        protected override string GetAssemblyVersionString(Assembly asm) {
            return asm.GetName().Version.ToString();
        }
        protected override IExceptionIgnoreDetection CreateIgnoreDetection() {
            return new StackBasedExceptionIgnoreDetection();
        }
        protected override void Configure() {
            ClientConfigurationLoader.ApplyClientConfiguration(this);
            ForceUpdateBreadcrumbsMaxCount();
        }

        public override void Run() {
            //do nothing
            //SendOfflineReports();
        }
        public override void Stop() {
            //do nothing
        }
        protected override IStackTraceHelper CreateStackTraceHelper() {
            return new StackTraceHelper();
        }

        protected override ReportConfirmationModel CreateConfirmationModel(LogifyClientExceptionReport report, Func<LogifyClientExceptionReport, bool> sendAction) {
            return null;
        }
        protected override bool RaiseConfirmationDialogShowing(ReportConfirmationModel model) {
            return false;
        }
    }
}