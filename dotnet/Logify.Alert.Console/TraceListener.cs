﻿using DevExpress.Logify.Core;

namespace DevExpress.Logify.Console {
    public class LogifyAlertTraceListener : LogifyAlertTraceListenerBase {
        protected override void InitializeInstance() {
            if (LogifyClientBase.Instance == null)
                LogifyAlert.InitializeInstance();
        }
    }
}