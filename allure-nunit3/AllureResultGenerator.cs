using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using allure_nunit3.Xml.Models;
using AllureCSharpCommons;
using AllureCSharpCommons.Events;
using NUnit.Engine;
using NUnit.Engine.Extensibility;
using NUnit.Framework;

namespace allure_nunit3
{
    [Extension]
    public class AllureResultGenerator : ITestEventListener
    {
        private bool processLogs = AllureNunitSettings.Default.WriteLogs;
        Dictionary<string, ConcurrentBag<TestObjectResult>> TestCaseDictionary = new Dictionary<string, ConcurrentBag<TestObjectResult>>();
        private Allure lifecycle;
        private Task LogProcessing;

        List<Task> tasks = new List<Task>();

        //TODO: Potential Refactor Below
        const String TestSuiteStart = "<start-run";
        const String TestSuiteEnd = "<test-run";
        const String TestCaseStart = "<start-test";
        const String TestCaseEnd = "<test-case";

        ConcurrentBag<String> logLines = new ConcurrentBag<string>();

        private static String _testSuiteIdentifier; 

        public AllureResultGenerator()
        {
            //Ensure base results directory exists
            if (!Directory.Exists(AllureNunitSettings.Default.OutputLocation))
            {
                Directory.CreateDirectory(AllureNunitSettings.Default.OutputLocation);
            }

            AllureConfig.ResultsPath = String.Format("{0}\\results", AllureNunitSettings.Default.OutputLocation);
            
            //Ensure result path exists.
            if (!Directory.Exists(AllureConfig.ResultsPath))
            {
                Directory.CreateDirectory(AllureConfig.ResultsPath);
            }

            AllureConfig.AllowEmptySuites = AllureNunitSettings.Default.AllowEmptySuites;

            //Start log processing
            LogProcessing = new TaskFactory().StartNew(WriteLog);

            lifecycle = Allure.Lifecycle;
        }
        
        public void OnTestEvent(string report)
        {
            try
            { 
                var state = ParseState(report);
                XmlSerializer deserializer;

                switch (state)
                {
                    case State.SuiteStart:
                        _testSuiteIdentifier = Guid.NewGuid().ToString();
                        lifecycle.Fire(new TestSuiteStartedEvent(_testSuiteIdentifier,
                            String.Format("Test Run {0:G}", DateTime.Now)));
                        break;
                    case State.SuiteEnd:
                        //Wait for all tasks to complete.
                        Task.WaitAll(tasks.ToArray(), 5000);
                        lifecycle.Fire(new TestSuiteFinishedEvent(_testSuiteIdentifier));
                        
                        //Stop processing the logs.
                        processLogs = false;
                        break;
                    case State.TestStart:
                        //Deserialize Object
                        deserializer = new XmlSerializer(typeof (TestCaseStart));
                        var testStart = (TestCaseStart) deserializer.Deserialize(new StringReader(report));

                        //Add object to be processed.
                        TestCaseDictionary.Add(testStart.Id, new ConcurrentBag<TestObjectResult> { new TestObjectResult
                        {
                            State = state,
                            TestCaseObject = testStart
                        }});

                        //Start Thread for processing.
                        tasks.Add(new TaskFactory().StartNew(() => ProcessData(testStart.Id)));
                        break;
                    case State.TestEnd:
                        //Deserialize Object
                        deserializer = new XmlSerializer(typeof (TestCaseEnd));
                        var testEnd = (TestCaseEnd) deserializer.Deserialize(new StringReader(report));

                        //Add Object to be processed
                        ConcurrentBag<TestObjectResult> testResultObjects;
                        TestCaseDictionary.TryGetValue(testEnd.Id, out testResultObjects);

                        testResultObjects.Add(new TestObjectResult
                        {
                            State = state,
                            TestCaseObject = testEnd
                        });
                        break;
                    case State.None:
                        return;
                    default:
                        throw new Exception("Error! Unhandled State");
                }
            }

            catch (Exception e)
            {
                logLines.Add(e.Message);
                logLines.Add(e.StackTrace);
                logLines.Add(report);
            }
        }

        private void ProcessData(string id)
        {
            try
            {
                while (true)
                {
                    //Get the list from the dictionary.
                    ConcurrentBag<TestObjectResult> elementsToProcess;
                    var bagPresent = TestCaseDictionary.TryGetValue(id, out elementsToProcess);

                    //If we have a bag, then process
                    if (bagPresent)
                    {
                        //Get the object from the bag
                        TestObjectResult elementToProcess;
                        var elementPresent = elementsToProcess.TryTake(out elementToProcess);

                        //If we have an item to process, then take the appropriate actions
                        if (elementPresent)
                        {
                            switch (elementToProcess.State)
                            {
                                case State.TestStart:
                                    var testStart = (TestCaseStart)elementToProcess.TestCaseObject;
                                    lifecycle.Fire(new TestCaseStartedEvent(_testSuiteIdentifier, testStart.FullName));
                                    lifecycle.Fire(new StepStartedEvent(testStart.FullName));
                                    break;
                                case State.TestEnd:
                                    var testEnd = (TestCaseEnd)elementToProcess.TestCaseObject;
                                    //Check if it was an assert failure
                                    if (testEnd.Assertions != null)
                                    {
                                        lifecycle.Fire(new StepFailureEvent
                                        {
                                            Throwable = new Exception("error!")
                                        });
                                        lifecycle.Fire(new TestCaseFailureEvent
                                        {
                                            StackTrace = testEnd.Assertions.Assertion.StackTrace,
                                            Throwable = new AssertionException(testEnd.Failure.Message)
                                        });
                                    }

                                    //Check if it was a differnet kind of failure
                                    else if (testEnd.Failure != null)
                                    {
                                        lifecycle.Fire(new TestCaseFailureEvent
                                        {
                                            StackTrace = testEnd.Failure.StackTrace,
                                            Throwable = new Exception(testEnd.Failure.Message)

                                        });
                                    }

                                    //If we had something other then passed mark as Canceled
                                    else if (!testEnd.Result.Equals("Passed"))
                                    {
                                        lifecycle.Fire(new TestCaseCanceledEvent());
                                    }

                                    //Always mark the test case as finished.
                                    lifecycle.Fire(new StepFinishedEvent());
                                    lifecycle.Fire(new TestCaseFinishedEvent());
                                    return;
                                default:
                                    throw new Exception("Error! Invalid Case!");


                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logLines.Add(e.Message);
                logLines.Add(e.StackTrace);
            }   
        }

        private void WriteLog()
        {
            while (processLogs)
            {
                String logLine;
                if (logLines.TryTake(out logLine))
                {
                    File.AppendAllLines(String.Format("{0}\\Log.txt", AllureNunitSettings.Default.OutputLocation), new List<string> { logLine, String.Empty});
                }
            }
        }

        private State ParseState(string xmlToParse)
        {
            if(xmlToParse.StartsWith(TestSuiteStart)) return State.SuiteStart;
            if (xmlToParse.StartsWith(TestSuiteEnd)) return State.SuiteEnd;
            if (xmlToParse.StartsWith(TestCaseStart)) return State.TestStart;
            if (xmlToParse.StartsWith(TestCaseEnd)) return State.TestEnd;

            return State.None;
        }
    }
}
