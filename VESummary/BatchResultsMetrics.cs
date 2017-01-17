using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;

namespace TestSummarizer
{
    public class BatchResultsMetrics : Metrics
    {
        private static readonly log4net.ILog log =
log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        protected List<int> metrics = new List<int>();
        public string iterationNumber;
        private enum metricList {TOTAL_TEST_RUN, TOTAL_TESTS_FAILED, TOTAL_TESTS_PASSED, TOTAL_TESTS_NOT_RUN, TOTAL_OBSOLETE_TESTS, TOTAL_TESTS_NOT_SCRIPTED};

        public BatchResultsMetrics()
        {
            bool isNum, pathInProgress;
            int n;
            List<string> email;
            bool result = false;
            StringBuilder sb;

            isValid = true;

            if (null == (emailTextFile = FileOperations.GetFileName("batchSummaryFileName")))
            {
                Console.WriteLine("Unable to find the batch summary file name.");
                isValid = false;
                return;
            }

            if (null == (tableTemplate = FileOperations.GetFileName("batchResultTableTemplate")))
            {
                Console.WriteLine("Unable to find the batch summary file name.");
                isValid = false;
                return;
            }

            // determine if there is an update available
            updateAvailable = false;
            if (!File.Exists(emailTextFile))
            {
                isValid = false;
                return;
            }
            else
            {
                updateAvailable = true;
            }

            // load summary email
            if ( null == (email = FileOperations.Load(emailTextFile)))
            {
                Console.WriteLine("Unable to load the email file.");
                isValid = false;
                return;
            }

            // get the metrics from the email.
            foreach (string line in email)
            {
                isNum = int.TryParse(line, out n);
                if (true == isNum)
                {
                    metrics.Add(n);
                }

            }

            // get the results path.
            pathInProgress = false;
            sb = new StringBuilder();
            foreach (string line in email)
            {
                if ( true == pathInProgress )
                {
                    sb.Append(line);
                    resultsPath = sb.ToString();
                    break;
                }

                result = (line.StartsWith("Results"));
                if (true == result)
                {
                    sb.Append(line);
                    pathInProgress = true;
                    continue;
                }
            }
            log.Info(resultsPath);

            // get the xls path.
            pathInProgress = false;
            sb = new StringBuilder();
            foreach (string line in email)
            {
                if (true == pathInProgress)
                {
                    sb.Append(line);
                    xlsPath = sb.ToString();
                    break;
                }

                result = (line.StartsWith("Xls"));
                if (true == result)
                {
                    sb.Append(line);
                    pathInProgress = true;
                    continue;
                }
            }
            Console.WriteLine(xlsPath);

            // Update the build number.
            foreach (string line in email)
            {
                result = (line.Contains("Build:"));
                if (true == result)
                {
                    string pattern = @"\bB.*\b";
                    Regex re = new Regex(pattern);
                    MatchCollection matches;
                    matches = re.Matches(line);
                    buildNumber = matches[0].Value;
                    break;
                }
            }

            if ( false == result )
            {
                Console.WriteLine("Could not find the build number.");
                isValid = false;
                return;
            }

            // Update the iteration number.
            foreach (string line in email)
            {
                result = (line.Contains("Iteration"));
                if (true == result)
                {
                    string pattern = @"(\d.)\b";
                    Regex re = new Regex(pattern);
                    MatchCollection matches;
                    matches = re.Matches(line);
                    iterationNumber = matches[0].Value;
                    break;
                }
            }

            if (false == result)
            {
                Console.WriteLine("Could not find the iteration number.");
                isValid = false;
                return;
            }
            return;
        }

        public List<string> GetTableTemplate()
        {    
            return (FileOperations.Load(tableTemplate));
        }

        public bool Validate()
        {
            return (isValid);
        }

        public bool IsUpdateAvailable()
        {
            if ( false == this.Validate() )
            {
                return (false);
            }

            return (updateAvailable);
        }

        public void Flush()
        {
            if (File.Exists(emailTextFile))
            {
                FileOperations.Backup(emailTextFile);
                FileOperations.Delete(emailTextFile);
            }
            return;
        }

        public int GetTotalTestRun()
        {
            return(metrics[(int)metricList.TOTAL_TEST_RUN]);
        }

        public int GetTotalFailedTests()
        {
            return (metrics[(int)metricList.TOTAL_TESTS_FAILED]);
        }

        public int GetTotalPassedTests()
        {
            return (metrics[(int)metricList.TOTAL_TESTS_PASSED]);
        }

        public int GetTotalTestsNotRun()
        {
            return (metrics[(int)metricList.TOTAL_TESTS_NOT_RUN]);
        }

        public int GetTotalObsoleteTests()
        {
            return (metrics[(int)metricList.TOTAL_OBSOLETE_TESTS]);
        }

        public int GetTotalTestsNotScripted()
        {
            return (metrics[(int)metricList.TOTAL_TESTS_NOT_SCRIPTED]);
        }

        public string GetResultsPath()
        {
            return (resultsPath);
        }

        public string GetXlsPath()
        {
            return (xlsPath);
        }

        public string GetBuildNumber()
        {
            return (buildNumber);
        }

        public string GetIterationNumber()
        {
            return ("Iteration: " + iterationNumber.ToString());
        }
    }
}
