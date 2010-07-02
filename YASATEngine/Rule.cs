using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using YASATEngine.Properties;

namespace YASATEngine
{
    public class Rule
    {
        string _title;
        string _description;
        string _regexSearchPattern;
        List<string> _fileExtensions;
        List<string> _guidance;
        Regex _regex;

        internal string Title
        {
            get
            {
                return _title;
            }
            set
            {
                _title = value;
            }
        }
        internal string Description
        {
            get
            {
                return _description;
            }
            set
            {
                _description = value;
            }
        }


        public string Severity { get; set; }

        public string Type { get; set; }

        internal string RegexSearchPattern
        {
            get
            {
                return _regexSearchPattern;
            }
            set
            {
                _regexSearchPattern = value;
                _regex = new Regex(_regexSearchPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            }
        }

        internal void AddNewExtension(string extension)
        {
            if ("*" == extension)
                _fileExtensions.Add(extension);
            else if(extension.StartsWith("."))
                _fileExtensions.Add(extension);
            else
                _fileExtensions.Add("." + extension);
        }

        internal void RemoveExtension(string extension)
        {
            if (extension.StartsWith("."))
                _fileExtensions.Remove(extension);
            else
                _fileExtensions.Remove("." + extension);
        }

        internal void ClearExtensions()
        {
            _fileExtensions.Clear();
        }

        internal void AddNewGuidance(string guidance)
        {
            _guidance.Add(guidance);
        }

        internal void RemoveGuidance(string guidance)
        {
            _guidance.Remove(guidance);
        }
        internal void ClearGuidance()
        {
            _guidance.Clear();
        }

        internal Rule()
        {
            _fileExtensions = new List<string>();
            _guidance = new List<string>();
        }

        public Rule(string Title, string Description, string Pattern, List<string> Extensions, List<string> Guidance)
        {
            _title = Title;
            _description = Description;
            _regexSearchPattern = Pattern;
            _fileExtensions = Extensions;
            _guidance = Guidance;

            _regex = new Regex(_regexSearchPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }

        public List<SourceCodeIssue> FindIssues(string fileToTest)
        {
            List<SourceCodeIssue> issues = new List<SourceCodeIssue>();
            int currentLineNumber = 1;
            
            foreach (string line in File.ReadAllLines(fileToTest))
            {   
                if (_regex.IsMatch(line))
                {
                    SourceCodeIssue issue = new SourceCodeIssue();
                    issue.FilePath = fileToTest;
                    issue.LineNumber = currentLineNumber;
                    issue.RulePattern = this;
                    issue.OffendingLine = line;
                    getCodeSnippet(fileToTest, currentLineNumber, ref issue);
                    
                    issues.Add(issue);
                }
                currentLineNumber++;
            }
            return issues;
        }

        private void getCodeSnippet(string FileToTest, int LineNumber, ref SourceCodeIssue issue)
        {
            string[] lines = File.ReadAllLines(FileToTest);
            int start = 0;
            int end = lines.Length;

            if (LineNumber - Settings.Default.LinesBefore > 0)
                start = LineNumber - Settings.Default.LinesBefore;

            if (LineNumber + Settings.Default.LinesAfter < end)
                end = LineNumber + Settings.Default.LinesAfter;


            StringBuilder beforeSB = new StringBuilder();
            StringBuilder afterSB = new StringBuilder();
            for (int i = start; i < LineNumber-1; i++)
                beforeSB.AppendLine(lines[i]);
            for (int i = LineNumber; i < end; i++)
                afterSB.AppendLine(lines[i]);

            issue.CodeAfter = afterSB.ToString();
            issue.CodeBefore = beforeSB.ToString();
        }

        public string GenerateHTMLSnippet()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<h2>Issue</h2>");
            sb.AppendFormat("<h3>{0}</h3>", _title);
            sb.AppendFormat("<div class=\"IssueDescription\">{0}</div>", _description);
            sb.AppendFormat("<strong>Looking for:</strong>{0}<br />", _regexSearchPattern);
            sb.AppendLine("<div class=\"IssueGuidance\"><ul>");
            foreach (string guideline in _guidance)
            {
                if (!string.IsNullOrWhiteSpace(guideline))
                    sb.AppendFormat("<li>{0}</li>", guideline);
            }
            sb.AppendLine("</ul></div>");
            return sb.ToString();
        }

        internal void AddGuidanceList(List<string> list)
        {
            _guidance.AddRange(list);
        }

        internal void AddExtensionList(List<string>list)
        {
            foreach (string item in list)
            {
                //call this instead of the AddRange method so we can be sure we check for the extension types
                AddNewExtension(item);
            }
        }

        public List<string> ExtensionList()
        {
            return _fileExtensions;
        }

        internal string GenerateXMLSnippet()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(@"
  <Rule>
    <Title>{0}</Title>
    <RegularExpressionPattern>{1}</RegularExpressionPattern>
    <Extensions>", _title, _regexSearchPattern);
            foreach (string ex in _fileExtensions)
                sb.AppendFormat(@"
      <Extension>{0}</Extension>", ex);
            sb.AppendFormat(@"
    </Extensions>
    <Guidance>");
            foreach (string guide in _guidance)
                sb.AppendFormat(@"
      <Url>{0}</Url>", guide);
            sb.AppendFormat(@"
    </Guidance>
    <Description>{0}</Description>
  </Rule>", _description);
            return sb.ToString();
        }

        internal string GenerateText()
        {

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Rule\r\n=====================================");
            sb.AppendLine("Title: " + _title);
            sb.AppendLine("Description:\r\n" +  _description);
            sb.AppendLine("Regex: " +  _regexSearchPattern);
            sb.AppendLine("Guidance: ");
            foreach (string guideline in _guidance)
            {
                if (!string.IsNullOrWhiteSpace(guideline))
                    sb.AppendFormat("\t- {0}\r\n", guideline);
            }
            return sb.ToString();
        }
    }
}
