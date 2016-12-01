﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DevExpress.Logify.Core {
    public abstract class BackgroundExceptionReportSender : ExceptionReportSenderSkeleton {
        readonly IExceptionReportSender innerSender;

        protected BackgroundExceptionReportSender(IExceptionReportSender innerSender) {
            this.innerSender = innerSender;
        }

        public override string ServiceUrl { get { return innerSender.ServiceUrl; } set { innerSender.ServiceUrl = value; } }
        public override string ApiKey { get { return innerSender.ApiKey; } set { innerSender.ApiKey = value; } }
        public override string MiniDumpServiceUrl { get { return innerSender.MiniDumpServiceUrl; } set { innerSender.MiniDumpServiceUrl = value; } }
        public override bool ConfirmSendReport { get { return InnerSender.ConfirmSendReport; } set { InnerSender.ConfirmSendReport = value; } }
        //public override string LogId { get { return innerSender.LogId; } set { innerSender.LogId = value; } }
        public IExceptionReportSender InnerSender { get { return innerSender; } }

        public override bool CanSendExceptionReport() {
            return innerSender != null && innerSender.CanSendExceptionReport();
        }

        protected override bool SendExceptionReportCore(LogifyClientExceptionReport report) {
            //Task task = Task.Factory.StartNew(() => {
            if (innerSender != null)
                SendExceptionReportInBackground(innerSender, report);
            //});
            return true;
        }

        protected abstract void SendExceptionReportInBackground(IExceptionReportSender innerSender, LogifyClientExceptionReport report);
    }
}