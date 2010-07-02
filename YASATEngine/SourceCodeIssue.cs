using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace YASATEngine
{
    public class SourceCodeIssue
    {
        string _filePath;
        int _lineNumber;
        string _offendingLine;
        string _codeBefore;
        string _codeAfter;
        Rule _rule;

        internal string FilePath
        {
            get
            {
                return _filePath;
            }
            set
            {
                _filePath = value;
            }
        }

        internal int LineNumber
        {
            get
            {
                return _lineNumber;
            }
            set
            {
                _lineNumber = value;
            }
        }

        internal string OffendingLine
        {
            get
            {
                return _offendingLine;
            }
            set
            {
                _offendingLine = value;
            }
        }

        internal string CodeBefore
        {
            get
            {
                return _codeBefore;
            }
            set
            {
                _codeBefore = value;
            }
        }

        internal string CodeAfter
        {
            get
            {
                return _codeAfter;
            }
            set
            {
                _codeAfter = value;
            }
        }

        internal Rule RulePattern
        {
            get
            {
                return _rule;
            }
            set
            {
                _rule = value;
            }
        }

        public string GenerateHTMLSnippet()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(_rule.GenerateHTMLSnippet());
            sb.Append("<h2>Code</h2>");
            sb.Append("<div>" + _filePath + ":");
            sb.Append("<strong>" + _lineNumber + "</strong><br />");
            sb.Append("<strong>Offending Line: <br /><strong><code>" + _offendingLine + "</strong></code></strong><hr />");
            sb.Append("<pre><code>");
            sb.Append(_codeBefore);
            sb.Append("<span class=\"offendingLine\">" + _offendingLine + "</span>\r\n");
            sb.Append(_codeAfter);
            sb.Append("</code></pre></div>");
            return sb.ToString();
        }

        public string GenerateMarkDown()
        {
            return "";
        }

        public string GenerateCsv()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(PrepareCSVString(_rule.Title));
            sb.Append(PrepareCSVString(_rule.Type));
            sb.Append(PrepareCSVString(_rule.Severity));
            sb.Append(PrepareCSVString(_rule.RegexSearchPattern));
            sb.Append(PrepareCSVString(_rule.Description));
            sb.Append(PrepareCSVString(_rule.ExtensionList()));
            sb.Append(PrepareCSVString(_filePath));
            sb.Append(_lineNumber);
            sb.Append(PrepareCSVString(_offendingLine));
            sb.Append(PrepareCSVString(_codeBefore));
            sb.Append(PrepareCSVString(_offendingLine));
            sb.Append(PrepareCSVString(_codeAfter));
            return sb.ToString();
        }

        private string PrepareCSVString(List<string> input)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string item in input)
                if (null != item)
                    sb.Append(item + "; ");
            return PrepareCSVString(sb.ToString());
        }

        private string PrepareCSVString(string input)
        {
            string returnString = "\"\",";
            if (null != input)
            {
                returnString = input.Replace("\"", "\"\"");
                returnString = "\"" + returnString + "\",";
            }
            return returnString;
        }

        public string GenerateText()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(_rule.GenerateText());
            sb.AppendLine("");
            sb.AppendLine("Code\r\n=====================================");
            sb.AppendLine("File: " + _filePath);
            sb.AppendLine("Line Number: " + _lineNumber);
            sb.AppendLine("Offending Line: " + _offendingLine.Trim());
            sb.AppendLine("-----------------------------------------------------------------");
            sb.AppendLine(_codeBefore);
            sb.AppendLine("==>>" + _offendingLine);
            sb.AppendLine(_codeAfter);
            sb.AppendLine("------------------------------------------------------------------");
            return sb.ToString();
        }
    }
}
