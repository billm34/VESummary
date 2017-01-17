using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Win32;

//###############################################################################
// File Name: KingfisherReportTranslator.py
//
// Purpose:  To automatically update the Kingfisher FIT Test metrics sent by email
//           each day. This script works in conjunction with an Outlook rule and
//           an autohotkey script. The idea is to eventually simplify this system
//           to a single tool.
//################################################################################
namespace TestSummarizer
{
    public class FitSummaryMetrics : Metrics
    {
        public FitSummary infrequentFit, fit, tcFit, nightlyFit, summaryFit;

        public struct FitSummary
            {
            public string name;
            public int percentComplete;
           public  int complete;
            public int total;
            public int remaining;
        }

        // GetMetrics: Finds the requested metrics in the source file.
        public FitSummaryMetrics()
        {
            List<string> contents;
            RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\willy", false);

            isValid = true;
            if (key != null)
            {
                Object o = key.GetValue("fitSummaryFileName");
                if (o != null)
                {
                    emailTextFile = o.ToString();
                }
                else
                {
                    Console.WriteLine("Couldn't find input file.");
                    isValid = false;
                    return;
                }

                o = key.GetValue("fitStatusTableTemplate");
                if (o != null)
                {
                    tableTemplate = o.ToString();
                }
                else
                {
                    Console.WriteLine("Couldn't find output file name.");
                    isValid = false;
                    return;
                }
            }
            else
            {
                Console.WriteLine("Couldn't find file information registry key.");
                isValid = false;
                return;
            }
            
            // determine if there is an update available
            updateAvailable = false;
            if ( !File.Exists(emailTextFile) )
            {
                isValid = false;
                return;
            }
            else
            {
                updateAvailable = true;
            }

            // load summary email
            if ( emailTextFile == String.Empty )
            {
                isValid = false;
                return;
            }

            contents = FileOperations.Load(emailTextFile);

            // populate the infrequent fit metrics.
            if (!UpdateMetrics("Infrequent Fit", contents, ref infrequentFit))
            {
                Console.WriteLine("Unable to update the metric.");
                isValid = false;
                return;
            }

            // populate the fit metrics.
            if ( !UpdateMetrics("Fit", contents, ref fit))
            {
                Console.WriteLine("Unable to update the metric.");
                isValid = false;
                return;
            }

            // populate the TC Fit metrics.
            if (!UpdateMetrics("TC Fit", contents, ref tcFit))
            {
                Console.WriteLine("Unable to update the metric.");
                isValid = false;
                return;
            }

            // populate the Nightly Fit metrics.
            if (!UpdateMetrics("Nightly Fit", contents, ref nightlyFit))
            {
                Console.WriteLine("Unable to update the metric.");
                isValid = false;
                return;
            }

            // populate the Summary Fit metrics.
            if (!UpdateMetrics("All Tests Summary", contents, ref summaryFit))
            {
                Console.WriteLine("Unable to update the metric.");
                isValid = false;
                return;
            }
            return;
        }

        public List<string> GetTableTemplate()
        {
            return (FileOperations.Load(tableTemplate));
        }

        public bool IsUpdateAvailable()
        {
            if ( false == isValid )
            {
                return (false);
            }
            else
            {
                return (updateAvailable);
            }
            
        }

        public string GetIterationNumber()
        {
            const int offset = 95;  // offset from week number to caluclate kingfisher iteration.
            int weekNum;
            int iterationNumber;
            DateTime dt = DateTime.Now;

            weekNum = GetCurrentWeekNumber();
            iterationNumber = offset + weekNum;
            if (dt.DayOfWeek == DayOfWeek.Monday)
            {
                iterationNumber -= 1; // Mondays are still in the previous iteration.
            }

            return ("Iteration: " + iterationNumber.ToString());
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

        public string GetInfrequentFitMetrics()
        {
            return (FormatMetric(infrequentFit));
        }

        public string GetNightlyFitMetrics()
        {
            return (FormatMetric(nightlyFit));
        }

        public string GetSummaryFitMetrics()
        {
            return (FormatMetric(summaryFit));
        }

        public string GetFitMetrics()
        {
            return (FormatMetric(fit));
        }

        public string GetTcFitMetrics()
        {
            return (FormatMetric(tcFit));
        }

        // FormatMetricList: Formats a line in the source file's html table and populates it with the metrics in the tag list.
        public string FormatMetric(FitSummary metricType)
        {
            string metricLine;
            string percentComplete = ConvertToPercent(metricType.percentComplete);
            metricLine = "<tr><th>" + metricType.name + @"</th>";
            metricLine = metricLine + "<td>" + percentComplete + "</td><td>" + metricType.complete + "</td><td>" + metricType.total + "</td><td>" + metricType.remaining + "</td></tr>";
            return metricLine;
        }

        private bool UpdateMetrics(string name, List<string> contents,ref FitSummary fitMetrics)
        {
            string w,line;
            string[] src;
            int i,n;
            Regex digitsOnly = new Regex(@"[^\d]");
            List<int> metrics = new List<int>();

            fitMetrics.name = name;
            line = "";
            for (i = 0; i < contents.Count; i++)
            {
                line = contents[i];
                if (line.StartsWith(fitMetrics.name))
                {
                    break;
                }
            }

            src = line.Split();
            foreach (string word in src)
            {
                w = digitsOnly.Replace(word, "");

                if (int.TryParse(w, out n))
                {
                    metrics.Add(n);
                }
            }

            if (metrics.Count == 0)
            {
                Console.WriteLine("ERROR: Unable to get the fit metrics from: " + emailTextFile);
                return(false);
            }

            fitMetrics.percentComplete = metrics[0];
            fitMetrics.complete = metrics[1];
            fitMetrics.total = metrics[2];
            fitMetrics.remaining = metrics[3];

            metrics.Clear();
            return (true);
        }
    }
}
