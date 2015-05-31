﻿using System;
using System.Collections.Generic;
using Chutzpah.Models;

namespace Chutzpah
{
    public class IISOptions
    {
        public string CmdLine { get; set; }
        public string RootDir { get; set; }
        public string BaseUri { get; set; }
    }

    public class TestOptions
    {
        private int testFileTimeoutMilliseconds;
        private int maxDegreeOfParallelism;
        private readonly int defaultParallelism;

        public TestOptions()
        {
            FileSearchLimit = Constants.DefaultFileSeachLimit;
            TestFileTimeoutMilliseconds = Constants.DefaultTestFileTimeout;
            defaultParallelism = Environment.ProcessorCount;
            MaxDegreeOfParallelism = defaultParallelism;
            CoverageOptions = new CoverageOptions();
            TestExecutionMode = TestExecutionMode.Execution;

            ChutzpahSettingsFileEnvironments = new ChutzpahSettingsFileEnvironments();

        }

        /// <summary>
        /// Whether or not to launch the tests in the browser
        /// </summary>
        public bool OpenInBrowser { get; set; }

        /// <summary>
        /// The name of browser which will be opened when OpenInBrowser is enabled, this value is optional
        /// </summary>
        public string BrowserName { get; set; }

        /// <summary>
        /// Options for IIS automation. 
        /// If this value is null there will no IIS integration
        /// If this value is not null RootDir and BaseDir are required and used to translate file path to IIS vdir
        /// If this value is not null and CmdLine is not null IISExpress will be started during tests with that cmdline value
        /// </summary>
        public IISOptions IISOptions { get; set; }

        /// <summary>
        /// Callback with IE pid started in debug mode
        /// </summary>
        public Action<int> OnDebuggableIEStart { get; set; }

        /// <summary>
        /// The time to wait for the tests to compelte in milliseconds
        /// </summary>
        public int? TestFileTimeoutMilliseconds
        {
            get { return testFileTimeoutMilliseconds; }
            set { testFileTimeoutMilliseconds = value ?? Constants.DefaultTestFileTimeout; }
        }

        /// <summary>
        /// Marks if we are running in exeuction or discovery mode
        /// </summary>
        public TestExecutionMode TestExecutionMode { get; set; }


        /// <summary>
        /// This is the max number of files to run tests for
        /// </summary>
        public int FileSearchLimit { get; set; }       
        
        /// <summary>
        /// The maximum degree of parallelism to process test files
        /// </summary>
        public int MaxDegreeOfParallelism
        {
            get { return maxDegreeOfParallelism; }
            set { maxDegreeOfParallelism = GetDegreeOfParallelism(value); }
        }

        /// <summary>
        /// Get the degree of parallism making sure the value is no less than 1 and not more
        /// then the number of processors
        /// </summary>
        private int GetDegreeOfParallelism(int value)
        {
            return Math.Min(Math.Max(value, 1), Environment.ProcessorCount);
        }

        /// <summary>
        /// Contains options for code coverage collection.
        /// </summary>
        public CoverageOptions CoverageOptions { get; set; }

        /// <summary>
        /// Additional per Chutzpah.json properties that can be used when resolved paths in
        /// the settings file
        /// </summary>
        public ChutzpahSettingsFileEnvironments ChutzpahSettingsFileEnvironments { get; set; }
    }
}