﻿using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Threading;
using DevExpress.Logify.Core;
using System.Diagnostics;
using System.Reflection;
using System.ComponentModel;
using Microsoft.Extensions.Configuration;
using DevExpress.Logify.Core.Internal;
using System.Threading.Tasks;
using Android.Runtime;

namespace DevExpress.Logify.Xamarin {
    public class LogifyAlert : LogifyClientBase {
        static volatile LogifyAlert instance;

        internal LogifyAlert(bool b) {
        }
        protected LogifyAlert(string apiKey) : base(apiKey) {
        }

        internal bool CollectMiniDump { get { return Config.CollectMiniDump; } set { Config.CollectMiniDump = value; } }
        internal bool CollectBreadcrumbs { get { return CollectBreadcrumbsCore; } set { CollectBreadcrumbsCore = value; } }
        internal int BreadcrumbsMaxCount { get { return BreadcrumbsMaxCountCore; } set { BreadcrumbsMaxCountCore = value; } }

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

        protected internal LogifyAlert(Dictionary<string, string> config) : base(config) {
        }

        protected override RootInfoCollector CreateDefaultCollectorCore() {
            return new NetCoreConsoleExceptionCollector(Config);
        }
        protected override ILogifyAppInfo CreateAppInfo() {
            return new XamarinApplicationCollector();
        }
        protected override IExceptionReportSender CreateExceptionReportSender() {
            IExceptionReportSender defaultSender = CreateConfiguredPlatformExceptionReportSender();
            if (ConfirmSendReport)
                return defaultSender;

            CompositeExceptionReportSender sender = new CompositeExceptionReportSender();
            sender.StopWhenFirstSuccess = true;
            //sender.Senders.Add(new ExternalProcessExceptionReportSender());
            sender.Senders.Add(defaultSender);
            sender.Senders.Add(new OfflineDirectoryExceptionReportSender());
            return sender;
        }
        protected override IExceptionReportSender CreateEmptyPlatformExceptionReportSender() {
            return new XamarinExceptionReportSender();
        }

        protected override LogifyAlertConfiguration LoadConfiguration() {
            return new LogifyAlertConfiguration();
        }
        //[CLSCompliant(false)]
        //public void Configure(IConfigurationSection section) {
        //    Configure(ClientConfigurationLoader.LoadConfiguration(section));
        //}

        public override void Run() {
            if (!IsSecondaryInstance) {
                AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
                TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
                AndroidEnvironment.UnhandledExceptionRaiser += AndroidEnvironmentOnUnhandledException;
            }
        }
        public override void Stop() {
            if (!IsSecondaryInstance) {
                AppDomain.CurrentDomain.UnhandledException -= OnCurrentDomainUnhandledException;
                TaskScheduler.UnobservedTaskException -= TaskSchedulerOnUnobservedTaskException;
                AndroidEnvironment.UnhandledExceptionRaiser -= AndroidEnvironmentOnUnhandledException;
            }
        }
        [SecurityCritical]
        [HandleProcessCorruptedStateExceptions]
        [IgnoreCallTracking]
        void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e) {
            if (e == null)
                return;
            Exception ex = e.ExceptionObject as Exception;

            if (ex != null) {
                var callArgumentsMap = this.MethodArgumentsMap; // this call should be done before any inner calls
                ResetTrackArguments();
                ReportException(ex, null, null, callArgumentsMap);
            }
        }
        [SecurityCritical]
        [HandleProcessCorruptedStateExceptions]
        [IgnoreCallTracking]
        private void AndroidEnvironmentOnUnhandledException(object sender, RaiseThrowableEventArgs e) {
            if (e == null)
                return;
            Exception ex = e.Exception as Exception;

            if (ex != null) {
                var callArgumentsMap = this.MethodArgumentsMap; // this call should be done before any inner calls
                ResetTrackArguments();
                ReportException(ex, null, null, callArgumentsMap);
            }
        }

        [SecurityCritical]
        [HandleProcessCorruptedStateExceptions]
        [IgnoreCallTracking]
        private void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e) {
            if (e == null)
                return;
            Exception ex = e.Exception as Exception;

            if (ex != null) {
                var callArgumentsMap = this.MethodArgumentsMap; // this call should be done before any inner calls
                ResetTrackArguments();
                ReportException(ex, null, null, callArgumentsMap);
            }
        }
        protected override ReportConfirmationModel CreateConfirmationModel(LogifyClientExceptionReport report, Func<LogifyClientExceptionReport, bool> sendAction) {
            return null;
        }
        protected override bool RaiseConfirmationDialogShowing(ReportConfirmationModel model) {
            return false;
        }
    }
}