using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestSummarizer
{
    public class DisplayFile
    {
        // ClearTable: deletes the content between the start and end tags to prepare for new data.
        public bool ClearTable()
        {
            return (true);
        }

        // FindTag: finds a string in a list and returns the line number.
        protected int FindTag(string tag, List<string> searchList)
        {
            int lineNumber;
            int result;

            if ( null == tag || null == searchList )
            {
                Console.WriteLine("Bad data passed to find the tag.");
                return (-1);
            }

            for (lineNumber = 0; lineNumber < searchList.Count; lineNumber++)
            {
                result = searchList[lineNumber].IndexOf(tag, StringComparison.CurrentCulture);
                if (-1 != result )
                {
                    return (lineNumber);
                }
            }

            Console.WriteLine("Tag not found: " + tag);
            return (-1);
        }
    }
}
