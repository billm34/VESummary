using System;
using System.Resources;

namespace TestSummarizer
{
    static class TestSummarizer
    {
        private static readonly log4net.ILog log =
        log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void Main()
        {
            // locals
            SummaryHtml htmlSummary = new SummaryHtml();
            FitSummaryMetrics fsm = new FitSummaryMetrics();
            BatchResultsMetrics brm = new BatchResultsMetrics();

            log4net.Config.XmlConfigurator.Configure();

            if ( !htmlSummary.Backup() )
            {
                log.Info("Unable to back up the html file.");
                return;
            }

            if ( !htmlSummary.Load() )
            {
                log.Info("Unable to load the html file.");
                return;
            }

            if ( !htmlSummary.UpdateBatchResultsTable(brm) )
            {
                log.Info("No update of the batch results table.");
            }
            htmlSummary.UpdateFitStatusTable(fsm);
            if ( !htmlSummary.Save() )
            {
                log.Info("No update of the Fit Status table.");
            }
            fsm.Flush();
            brm.Flush();

            return;
        }
    }
}
