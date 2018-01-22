'use strict';
import nodeCollector from "./collectors/nodeCollector.js";
import customDataCollector from "./collectors/customDataCollector.js";
import tagsCollector from "./collectors/tagsCollector.js";
import nodeReportSender from "./reportSender/nodeReportSender.js";

class logifyAlert {
    constructor(apiKey) {
        this._apiKey = apiKey;
        this._handleReports = false;
        this.applicationName = undefined;
        this.applicationVersion = undefined;
        this.userId = undefined;
        this.customData = undefined;
        this.tags = undefined;
        this.beforeReportException = undefined;
        this.afterReportException = undefined;
    }

    startHandling() {
        if(this._handleReports)
            return;

        this._handleReports = true;

        process.on('uncaughtException', (error) => {
            if(this._handleReports) {
                this.sendException(error);
            }
        });

        process.on('unhandledRejection', (reason, promise) => {
            if(this._handleReports) {
                this.sendRejection(reason);
            }
        });
    }

    stopHandling() {
        this._handleReports = false;
    }
    
    sendException(error) {
        this.callBeforeReportExceptionCallback();
        let collector = this.createCollector();
        collector.handleException(error);
        this.sendReportCore(collector.reportData);
    }
    
    sendRejection(reason) {
        this.callBeforeReportExceptionCallback();
        let collector = this.createCollector();
        collector.handleRejection(reason);
        this.sendReportCore(collector.reportData);
    }
    
    sendReportCore(reportData) {
        let sender = new nodeReportSender();
        sender.sendReport(this._apiKey, reportData, this.afterReportException);
    }
    
    createCollector() {
        let collector = new nodeCollector();
        collector.applicationName = this.applicationName;
        collector.applicationVersion = this.applicationVersion;
        collector.userId = this.userId;
        collector.collectors.push(new customDataCollector(this.customData));
        collector.collectors.push(new tagsCollector(this.tags));
        return collector;
    }

    callBeforeReportExceptionCallback() {
        if(this.beforeReportException != undefined) {
            this.customData = this.beforeReportException(this.customData);
        }
    }
}

module.exports = logifyAlert;