using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YASATEngine
{
    public class ProjectInfo
    {
        public List<SourceFile> files = new List<SourceFile>();

        public int GetIssuesCount()
        {
            int issuesCount = 0;
            foreach (SourceFile file in files)
            {
                issuesCount += file.issues.Count;
            }
            return issuesCount;
        }

        public int EstimateCodeReviewTime()
        {
            return EstimateCodeReviewTime("h");
        }

        public int EstimateCodeReviewTime(string format)
        {
            int minutes = 0;
            int minutesPerPotentialIssue = 15;
            int linesPerHour = 1500;

            foreach (SourceFile file in files)
            {
                minutes += file.issues.Count * minutesPerPotentialIssue;
                minutes += file.lines / linesPerHour * 60;
            }

            int ReturnTime = 0;

            switch (format.ToLower())
            {
                case "m":
                    ReturnTime = minutes;
                    break;
                case "h":
                    ReturnTime = minutes/60;
                    break;
                case "d":
                    ReturnTime = minutes/60/8;
                    break;
                default:
                    ReturnTime = minutes / 60;
                    break;
            }
            return ReturnTime;
        }

        public int getTotalLinesOfCode()
        {
            int totalLines = 0;
            foreach (SourceFile file in files)
            {
                totalLines += file.lines;
            }
            return totalLines;
        }
    }
}
