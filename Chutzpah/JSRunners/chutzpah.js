﻿/*globals phantom, WebPage, console*/
var chutzpah = {};

chutzpah.runner = function (testsComplete, testsEvaluator) {
    /// <summary>Executes a test suite and evaluates the results using the provided functions.</summary>
    /// <param name="testsComplete" type="Function">Function that returns true of false if the test suite should be considered complete and ready for evaluation.</param>
    /// <param name="testsEvaluator" type="Function">Function that returns a chutzpah.TestOutput containing the results of the test suite.</param>
    'use strict';

    var page = new WebPage(),
        logs = [];

    function LogEntry(message, line, source) {
        this.message = message;
        this.line = line;
        this.source = source;
    }

    function waitFor(testFx, onReady, timeOutMillis) {
        var maxtimeOutMillis = timeOutMillis || 3001,
            start = new Date().getTime(),
            condition = false,
            interval;

        function intervalHandler() {
            if (!condition && (new Date().getTime() - start < maxtimeOutMillis)) {
                condition = testFx();
            } else {
                if (!condition) {
                    phantom.exit(1);
                } else {
                    onReady();
                    clearInterval(interval);
                }
            }
        }

        interval = setInterval(intervalHandler, 100);
    }

    function addToLog(message, line, source) {
        logs.push(new LogEntry(message, line, source));
    }

    function pageOpenHandler(status) {
        var waitCondition = function () { return page.evaluate(testsComplete); },
            gatherTests = function () {
                var testSummary = page.evaluate(testsEvaluator);

                if (testSummary) {
                    testSummary.logs = testSummary.logs.concat(logs);
                    console.log('#_#Begin#_#');
                    console.log(JSON.stringify(testSummary, null, 4));
                    console.log('#_#End#_#');
                    phantom.exit((parseInt(testSummary.failedCount, 10) > 0) ? 1 : 0);
                } else {
                    phantom.exit();
                }
            };

        if (status !== 'success') {
            console.log('Unable to access network');
            phantom.exit();
        } else {
            waitFor(waitCondition, gatherTests);
        }
    }

    if (phantom.args.length === 0 || phantom.args.length > 2) {
        console.log('Error: too few arguments');
        phantom.exit();
    }

    page.onConsoleMessage = addToLog;

    page.open(phantom.args[0], pageOpenHandler);
};