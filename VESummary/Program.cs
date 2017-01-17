using System;
using System.Resources;

namespace TestSummarizer
{
    static class TestSummarizer
    {
        public static void Main()
        {
            // locals
            SummaryHtml htmlSummary = new SummaryHtml();
            FitSummaryMetrics fsm = new FitSummaryMetrics();
            BatchResultsMetrics brm = new BatchResultsMetrics();

            if ( !htmlSummary.Backup() )
            {
                Console.WriteLine("Unable to back up the html file.");
                return;
            }

            if ( !htmlSummary.Load() )
            {
                Console.WriteLine("Unable to load the html file.");
                return;
            }

            if ( !htmlSummary.UpdateBatchResultsTable(brm) )
            {
                Console.WriteLine("No update of the batch results table.");
            }
            htmlSummary.UpdateFitStatusTable(fsm);
            if ( !htmlSummary.Save() )
            {
                Console.WriteLine("No update of the Fit Status table.");
            }
            fsm.Flush();
            brm.Flush();

            return;
        }
    }
}
