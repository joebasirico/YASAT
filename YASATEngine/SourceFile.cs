using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace YASATEngine
{
    public class SourceFile
    {
        public int lines;
        public int commentedLines;
        public string path;
        public List<SourceCodeIssue> issues;

        public SourceFile(string file)
        {
            issues = new List<SourceCodeIssue>();
            path = file;
            foreach (String line in File.ReadAllLines(path))
            {
                bool inComment = false;
                if (line.StartsWith("//"))
                {
                    commentedLines++;
                }
                else if (line.StartsWith("/*"))
                {
                    inComment = true;
                    commentedLines++;
                }
                else if (line.Contains("*/"))
                {
                    inComment = false;
                    commentedLines++;
                }
                else
                {
                    if (inComment)
                        commentedLines++;
                }
                lines++;
            }
        }
    }
}
