using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Win32;

namespace TestSummarizer
{
    public class SummaryHtml : DisplayFile
    {
        protected List<string> contents = new List<string>();
        readonly private string inputFileName, outputFileName;
        private int batchTableInsertLine;
        private int fitTableInsertLine;
        private enum tables { FIT_STATUS_TABLE, BATCH_RESULTS_TABLE };
        private static readonly log4net.ILog log =
log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        //--[ public methods ]-------------------------------------------------

        public SummaryHtml()
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey("Software\\willy",false);

            log4net.Config.XmlConfigurator.Configure();

            if ( key != null )
            {
                Object o = key.GetValue("summaryHTMLFileName");
                if ( o != null )
                {
                    inputFileName = o.ToString();
                }
                else
                {
                    log.Error("Couldn't find input file.");
                    return;
                }

                o = key.GetValue("outputSummaryHTMLFileName");
                if (o != null)
                {
                    outputFileName = o.ToString();
                }
                else
                {
                    log.Error("Couldn't find output file name.");
                    return;
                }
            }
            else
            {
                log.Error("Couldn't find file information registry key.");
            }
        }

        public bool IsBatchResultsUpdateAvailable(BatchResultsMetrics brm)
        {
            return (brm.IsUpdateAvailable());
        }

        public bool Backup()
        {
            if ( !FileOperations.Backup(inputFileName) )
            {
                return (false);
            }
            else
            {
                return (true);
            }
            
        }

        public bool Save()
        {
            try
            {
                File.WriteAllText(outputFileName, String.Empty);
            }
            catch (Exception e)
            {
                log.Error("Unable to clear the file: " + e.Message);
                return (false);
            }

            if ( !FileOperations.Save(outputFileName, contents) )
            {
                log.Error("Unable to save the file.");
                return (false);
            }

            return (true);
        }

        public bool Load()
        {
            contents = FileOperations.Load(inputFileName);
            if ( null == contents )
            {
                return (false);
            }
            else
            {
                return (true);
            }
            
        }

        public bool ClearFitStatusTable()
        {
            return (ClearHtmlTable(tables.FIT_STATUS_TABLE));
        }

        public bool ClearBatchResultsTable()
        {
            return (ClearHtmlTable(tables.BATCH_RESULTS_TABLE));
        }

        public bool IsFitStatusUpdateAvailable()
        {
            FitSummaryMetrics fsm = new FitSummaryMetrics();

            return (fsm.IsUpdateAvailable());
        }

        public bool UpdateBatchResultsTable(BatchResultsMetrics brm)
        {
            List<string> table;

            // Determine if an update is availiable
            if ( !this.IsBatchResultsUpdateAvailable(brm) )
            {
                log.Info("No batch results update available.");
                return(false);
            }

            // make sure we have a valid set of batch result metrics.
            if ( !brm.Validate() )
            {
                log.Error("No valid metrics to update.");
                return (false);
            }

            // clear the old table
            if ( !this.ClearBatchResultsTable() )
            {
                log.Error("Unable to clear the batch results table.");
                return (false);
            }

            table = brm.GetTableTemplate();
            if ( null == table )
            {
                log.Error("Unable to get the table template.");
                return (false);
            }
            table.Reverse();
            foreach (string line in table)
            {
                contents.Insert(batchTableInsertLine, line);
            }

            // add the iteration number
            if (!UpdateTag("<!-- TAG: Batch Results Iteration Number -->", brm.GetIterationNumber(), contents))
            {
                log.Error("Unable to add the iteration number.");
                return (false);
            }

            // add the last updated date.
            if (!UpdateTag("<!-- TAG: Batch Results Updated Date -->", brm.GetCurrentDate(), contents))
            {
                log.Error("Unable to add the updated date.");
                return (false);
            }

            // Add the build number.
            if (!UpdateTag("<!-- TAG: Batch Results Build Number -->", brm.GetBuildNumber(), contents))
            {
                log.Error("Unable to add the build number.");
                return (false);
            }

            // Add the total test run.
            if (!UpdateTag("<!-- TAG: Batch Results Total Test Run -->", brm.GetTotalTestRun().ToString(), contents))
            {
                log.Error("Unable to add the total test run.");
                return (false);
            }

            // add total failed
            if (!UpdateTag("<!-- TAG: Batch Results Tests Failed -->", brm.GetTotalFailedTests().ToString(), contents))
            {
                log.Error("Unable to add the total test failed.");
                return (false);
            }

            // add total passed
            if (!UpdateTag("<!-- TAG: Batch Results Tests Passed -->", brm.GetTotalPassedTests().ToString(), contents))
            {
                log.Error("Unable to add the total passed.");
                return (false);
            }

            // add total not run
            if ( !UpdateTag("<!-- TAG: Batch Results Tests Not Run -->", brm.GetTotalTestsNotRun().ToString(), contents) )
            {
                log.Error("Unable to add the total tests not run.");
                return (false);
            }
            
            // add total obsolete tests
            if (!UpdateTag("<!-- TAG: Batch Results Obsolete Tests -->", brm.GetTotalObsoleteTests().ToString(), contents))
            {
                log.Error("Unable to add the total obsolete tests.");
                return (false);
            }

            // add total tests not scripted
            if (!UpdateTag("<!-- TAG: Batch Results Tests Not Scripted -->", brm.GetTotalTestsNotScripted().ToString(), contents))
            {
                log.Error("Unable to add the total tests not scripted.");
                return (false);
            }

            // add the XLS results path
            if (!UpdateTag("<!-- TAG: Batch Results xls Path -->", brm.GetXlsPath(), contents))
            {
                log.Error("Unable to add the XLS path.");
                return (false);
            }

            // add the results path
            if (!UpdateTag("<!-- TAG: Batch Results Results Path -->", brm.GetResultsPath(), contents))
            {
                log.Error("Unable to add the results path.");
                return (false);
            }

            return (true);
        }

        public void UpdateFitStatusTable(FitSummaryMetrics fsm)
        {
            List<string> table;

            // determine if an update is available.
            if ( !this.IsFitStatusUpdateAvailable() )
            {
                return;
            }

            // clear the table
            this.ClearFitStatusTable();

            // update to the new template
            table = fsm.GetTableTemplate();
            table.Reverse();

            foreach (string line in table)
            {
                contents.Insert(fitTableInsertLine, line);
            }

            // update the iteration number
            if (!UpdateTag("<!-- TAG: Fit Status Iteration Number -->", fsm.GetIterationNumber(), contents))
            {
                return;
            }
            // update the last updated date.
            if (!UpdateTag("<!-- TAG: Fit Status Updated Date -->", fsm.GetCurrentDate(), contents))
            {
                return;
            }

            // add the infrequent Fit Metrics
            if (!UpdateTag("<!-- TAG: Infrequent Fit Metrics -->", fsm.GetInfrequentFitMetrics(), contents))
            {
                return;
            }

            // add the Fit Metrics
            if (!UpdateTag("<!-- TAG: Fit Metrics -->", fsm.GetFitMetrics(), contents))
            {
                return;
            }

            // add the TC Fit Metrics
            if (!UpdateTag("<!-- TAG: TC Fit Metrics -->", fsm.GetTcFitMetrics(), contents))
            {
                return;
            }

            // add the Nightly Fit Metrics
            if (!UpdateTag("<!-- TAG: Nightly Fit Metrics -->", fsm.GetNightlyFitMetrics(), contents))
            {
                return;
            }

            // add the Summary Fit Metrics
            if ( !UpdateTag("<!-- TAG: Summary Fit Metrics -->", fsm.GetSummaryFitMetrics(), contents) )
            {
                return;
            }

            return;
        }

        //--[ private methods ]-------------------------------------------------
        private bool ClearHtmlTable(SummaryHtml.tables tableType)
        {
            int i;
            int startLine, finishLine;
            string startTag, finishTag;

            switch (tableType)
            {
                case tables.FIT_STATUS_TABLE:
                    startTag = "<!-- TAG: VE Summary Table Start -->";
                    finishTag = "<!-- TAG: VE Summary Table End -->";
                    break;
                case tables.BATCH_RESULTS_TABLE:
                    startTag = "<!-- TAG: Batch Results Start -->";
                    finishTag = "<!-- TAG: Batch Results End -->";
                    break;
                default:
                    return (false);
            }

            // find the start and end tags in the file.
            startLine = FindTag(startTag, contents) + 1;
            finishLine = FindTag(finishTag, contents);
            if ((0 == startLine) || (-1 == finishLine))
            {
                log.Error("Didn't find one of the tags in range: " + startLine + " " + finishLine);
                return (false);
            }

            // delete everything between the tags.
            for (i = startLine; i < finishLine; i++)
            {
                contents.RemoveAt(startLine);
            }

            if (tableType == tables.FIT_STATUS_TABLE)
            {
                fitTableInsertLine = startLine;
            }
            else if (tableType == tables.BATCH_RESULTS_TABLE)
            {
                batchTableInsertLine = startLine;
            }

            return (true);
        }

        private bool UpdateTag(string tag, string content, List<string> contents)
        {
            int target = FindTag(tag, contents);

            if ( null == content )
            {
                log.Error("Nothing to update.");
                return (false);
            }

            if (-1 == target)
            {
                log.Error("Unable to find the Summary Fit metrics");
                return(false);
            }

            contents.RemoveAt(target);
            contents.Insert(target, tag + content);
            return (true);
        }
    }
}
