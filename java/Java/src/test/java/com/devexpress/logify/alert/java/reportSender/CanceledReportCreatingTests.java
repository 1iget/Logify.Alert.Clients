package com.devexpress.logify.alert.java.reportSender;

import com.devexpress.logify.alert.core.events.CanReportExceptionLogifyEvent;
import com.devexpress.logify.alert.core.events.LogifyEventListener;
import com.devexpress.logify.alert.core.LogifyClientExceptionReport;
import com.devexpress.logify.alert.java.TestLogifyAlert;
import org.junit.After;
import org.junit.Assert;
import org.junit.Before;
import org.junit.Test;

public class CanceledReportCreatingTests {
    private TestLogifyAlert client;
    private CanReportExceptionEventListener event;

    @Before
    public void setUp() {
        client = new TestLogifyAlert();
        event = new CanReportExceptionEventListener();
        client.addCanReportExceptionListener(event);
    }

    @After
    public void tearDown() {
        client.removeCanReportExceptionListener(event);
        client = null;
        event = null;
    }

    @Test
    public void testCanceledReportCreating() {
        client.send(new NullPointerException());
        LogifyClientExceptionReport report = client.getExceptionReportFromSender();
        Assert.assertNull(report);
    }

    private class CanReportExceptionEventListener implements LogifyEventListener<CanReportExceptionLogifyEvent> {
        public void handle(CanReportExceptionLogifyEvent event) {
            event.setCancel(true);
        }
    }
}