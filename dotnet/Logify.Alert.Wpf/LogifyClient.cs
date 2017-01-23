﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Runtime.ExceptionServices;
using System.Security;
using System.Threading;
using System.Windows.Threading;
using DevExpress.Logify.Core;
using System.Diagnostics;
using System.Reflection;

namespace DevExpress.Logify.WPF {
    public class LogifyAlert : LogifyClientBase {
        static volatile LogifyAlert instance;

        public LogifyAlert() {
        }
        protected LogifyAlert(string apiKey) : base(apiKey) {
        }
        protected internal LogifyAlert(Dictionary<string, string> config)
            : base(config) {
        }

        public static new LogifyAlert Instance {
            get {
                if (instance != null)
                    return instance;

                lock (typeof(LogifyAlert)) {
                    if (instance != null)
                        return instance;

                    instance = new LogifyAlert();
                    LogifyClientBase.Instance = instance;
                }
                return instance;
            }
        }

        protected override IInfoCollectorFactory CreateCollectorFactory() {
            return new WPFExceptionCollectorFactory();
        }
        protected override IInfoCollector CreateDefaultCollector(ILogifyClientConfiguration config, IDictionary<string, string> additionalCustomData) {
            WPFExceptionCollector result = new WPFExceptionCollector(config);
            result.AppName = this.AppName;
            result.AppVersion = this.AppVersion;
            result.UserId = this.UserId;
            result.Collectors.Add(new CustomDataCollector(this.CustomData, additionalCustomData));
            return result;
        }
        protected override IExceptionReportSender CreateExceptionReportSender() {
            WPFExceptionReportSender defaultSender = new WPFExceptionReportSender();
            defaultSender.ConfirmSendReport = ConfirmSendReport;
            if (ConfirmSendReport)
                return defaultSender;

            CompositeExceptionReportSender sender = new CompositeExceptionReportSender();
            sender.StopWhenFirstSuccess = true;
            sender.Senders.Add(new ExternalProcessExceptionReportSender());
            sender.Senders.Add(defaultSender);
            sender.Senders.Add(new OfflineDirectoryExceptionReportSender());
            return sender;
        }
        protected override IExceptionReportSender CreateEmptyPlatformExceptionReportSender() {
            return new WPFExceptionReportSender();
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
        }

        public override void Run() {
            if (!IsSecondaryInstance) {
                //Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
                AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
                Dispatcher.CurrentDispatcher.UnhandledException += OnCurrentDispatcherUnhandledException;
                //AppDomain.CurrentDomain.FirstChanceException
                SendOfflineReports();
            }

        }
        public override void Stop() {
            if (!IsSecondaryInstance) {
                //Application.SetUnhandledExceptionMode(UnhandledExceptionMode.Automatic);
                AppDomain.CurrentDomain.UnhandledException -= OnCurrentDomainUnhandledException;
                Dispatcher.CurrentDispatcher.UnhandledException -= OnCurrentDispatcherUnhandledException;
            }
        }

        void OnCurrentDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
            if (e != null && e.Exception != null) {
                ReportException(e.Exception, null);
            }
        }


        [SecurityCritical]
        [HandleProcessCorruptedStateExceptions]
        void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e) {
            if (e == null)
                return;
            Exception ex = e.ExceptionObject as Exception;

            if (ex != null)
                ReportException(ex, null);
        }

        Mutex mutex;
        protected override bool DetectIfSecondaryInstance() {
            try {
                int pid = Process.GetCurrentProcess().Id;
                string name = "logifypid" + pid.ToString();
                if (mutex == null) {
                    mutex = OpenExistingMutex(name);
                    if (mutex != null)
                        return true;
                }

                this.mutex = new Mutex(false, name);
                return false;
            }
            catch {
                return false;
            }
        }
        Mutex OpenExistingMutex(string name) {
            try {
                try {
                    //AM: TryOpenExisting exists only in .NET4.5
                    MethodInfo tryGetMutex = typeof(Mutex).GetMethod("TryOpenExisting", new Type[] { typeof(string), typeof(Mutex).MakeByRefType() });
                    if (tryGetMutex != null) {

                        object[] parameters = new object[] { name, null };
                        object mutexExists = tryGetMutex.Invoke(null, parameters);
                        if ((bool)mutexExists) {
                            Mutex result = parameters[1] as Mutex;
                            if (result != null)
                                return result;
                        }
                        else
                            return null;
                    }
                }
                catch {
                }
                return Mutex.OpenExisting(name);
            }
            catch {
                return null;
            }
        }
    }
}