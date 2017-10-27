'use strict';
import jsCollector from "./collectors/jsCollector.js";
import breadcrumbsListeners from "./breadcrumbs/breadcrumbsListeners.js";
import jsReportSender from "./reportSender/jsReportSender.js";

class logifyAlert {
    constructor(apiKey) {
        this._apiKey = apiKey;
        this._handleReports = false;
        this._breadcrumbs = undefined;
        this._maxBreadcrumbsCount = 1000;

        this.applicationName = undefined;
        this.applicationVersion = undefined;
        this.userId = undefined;
        this.customData = undefined;
        this.collectLocalStorage = false;
        this.collectSessionStorage = true;
        this.collectCookies = true;
        this.collectInputs = false;
        
        this.collectBreadcrumbs = false;

        this.beforeReportException = undefined;
        this.afterReportException = undefined;

        this.initBreadcrumbs();
    }

    stopHandling() {
        this._handleReports = false;
    }

    startHandling() {
        if(this._handleReports)
            return;

        this._handleReports = true;
        
        window.onerror = (errorMsg, url, lineNumber, column, errorObj) => {
            if(this._handleReports) {
                this.sendExceptionCore(errorMsg, url, lineNumber, column, errorObj, this);
            }

            return false;
        };

        window.addEventListener('unhandledrejection', event => {
            if(this._handleReports) {
                this.sendRejection(event.reason, event.promise);
            }
        });
    }

    sendException(errorMsg, url, lineNumber, column, errorObj) {
        this.sendExceptionCore(errorMsg, url, lineNumber, column, errorObj, this);
    }

    sendExceptionCore(errorMsg, url, lineNumber, column, errorObj, owner) {
        this.callBeforeReportExceptionCallback();
        let collector = this.createCollector(owner);
        collector.collectErrorData(errorMsg, url, lineNumber, column, errorObj);
        this.sendReportCore(collector.reportData);
    }

    sendRejection(reason, promise) {
        this.callBeforeReportExceptionCallback();
        let collector = this.createCollector(this);
        collector.collectRejectionData(reason, promise);
        this.sendReportCore(collector.reportData);
    }

    sendReportCore(reportData) {
        let sender = new jsReportSender();
        sender.sendReport(this._apiKey, reportData, this.afterReportException);
    }

    createCollector(owner) {
        let collector = new jsCollector(owner);
        collector.applicationName = owner.applicationName;
        collector.applicationVersion = owner.applicationVersion;
        collector.userId = owner.userId;
        return collector;
    }

    callBeforeReportExceptionCallback() {
        if(this.beforeReportException != undefined) {
            this.customData = this.beforeReportException(this.customData);
        }
    }

    initBreadcrumbs() {
        const breadcrumbsListener = new breadcrumbsListeners(window, this);
        breadcrumbsListener.startListening();
    }

    addBreadcrumbs(breadcrumb) {
        if(this._breadcrumbs === undefined) {
            this._breadcrumbs = [];
        }
        this._breadcrumbs.push(breadcrumb);
        if (this._breadcrumbs.length > this._maxBreadcrumbsCount) {
            this._breadcrumbs.shift();
        }
    }
}

window["logifyAlert"] = logifyAlert;