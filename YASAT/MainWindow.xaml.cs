using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.IO;
using System.Threading;
using System.Diagnostics;
using YASAT.Properties;
using YASATEngine;

namespace YASAT
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Rule> rules;
        //List<SourceCodeIssue> issues = new List<SourceCodeIssue>();
        ProjectInfo projectInfo = new ProjectInfo();

        public MainWindow()
        {
            InitializeComponent();

            rules = RuleManager.GetAllRules();
            RefreshUI();
        }

        private void RefreshUI()
        {
            RuleCountMessage.Text = rules.Count + " rules loaded.";
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.CheckPathExists = true;
            ofd.RestoreDirectory = true;
            if (ofd.ShowDialog().Value)
            {
                string dirPath = System.IO.Path.GetDirectoryName(ofd.FileName);
                if (Directory.Exists(dirPath))
                {
                    ScanDirectory.Text = dirPath;
                    FileCountMessage.Text = CountFiles(dirPath, 0) + " files discovered that match rules.";
                    Scan.IsEnabled = true;
                }
            }

        }

        private int CountFiles(string directory, int currentFileCount)
        {
            List<string> filetypeList = new List<string>();
            foreach (Rule rule in rules)
            {
                foreach (string filetype in rule.ExtensionList())
                {
                    if (!filetypeList.Contains(filetype))
                        filetypeList.Add(filetype);
                }
            }

            foreach (string file in Directory.GetFiles(directory))
            {
                if (filetypeList.Contains("*"))
                    currentFileCount++;
                else if (disregardFiletypes.IsChecked.Value || filetypeList.Contains(System.IO.Path.GetExtension(file)))
                    currentFileCount++;
            }
            foreach (string dir in Directory.GetDirectories(directory))
                currentFileCount = CountFiles(dir, currentFileCount);
            return currentFileCount;
        }

        private void PerformScan(string directory)
        {
            foreach (string file in Directory.GetFiles(directory))
            {
                ScanFile(file);
            }

            foreach (string dir in Directory.GetDirectories(directory))
            {
                PerformScan(dir);
            }
        }

        private void ScanFile(string file)
        {
            SourceFile sourceFile = new SourceFile(file);
            foreach (Rule rule in rules)
            {
                if (rule.ExtensionList().Contains("*"))
                    sourceFile.issues.AddRange(rule.FindIssues(file));
                else if (disregardFiletypes.IsChecked.Value || rule.ExtensionList().Contains(System.IO.Path.GetExtension(file)))
                    sourceFile.issues.AddRange(rule.FindIssues(file));
            }
            projectInfo.files.Add(sourceFile);
        }

        private void Scan_Click(object sender, RoutedEventArgs e)
        {
            string dirPath = ScanDirectory.Text;
            if (Directory.Exists(dirPath))
            {
                PerformScan(dirPath);
            }

            ReportSummary.Visibility = System.Windows.Visibility.Visible;
            ReportSummary.Text = "Scan Completed, " + projectInfo.GetIssuesCount() + " issues discovered." +
                "Click Generate Report to create a new report";

            GenReport.IsEnabled = true;
        }

        private void GenReport_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.AddExtension = true;
            sfd.FileName = "YASAT_Report";
            sfd.Filter = "HTML Report (*.htm)|*.htm|CSV Report (*.csv)|*.csv|Text Report (*.txt)|*.txt";
            sfd.FilterIndex = 0;
            if (sfd.ShowDialog().Value)
            {

                StreamWriter sw = new StreamWriter(sfd.FileName);
                switch (System.IO.Path.GetExtension(sfd.FileName))
                {
                    case ".htm":
                        SaveAsHTML(sw);
                        break;
                    case ".txt":
                        SaveAsTxt(sw);
                        break;
                    case ".csv":
                        SaveAsCsv(sw);
                        break;
                    default:
                        break;
                }

                sw.Close();

                //displays the file in the default viewer
                Process.Start(sfd.FileName);
            }
        }

        private void SaveAsCsv(StreamWriter sw)
        {
            foreach (SourceFile file in projectInfo.files)
            {
                foreach (SourceCodeIssue issue in file.issues)
                {
                    sw.WriteLine(issue.GenerateCsv());
                }
            }
        }

        private void SaveAsTxt(StreamWriter sw)
        {
            foreach (SourceFile file in projectInfo.files)
            {
                foreach (SourceCodeIssue issue in file.issues)
                {
                    sw.WriteLine(issue.GenerateText());
                }
            }
        }

        private void SaveAsHTML(StreamWriter sw)
        {
            sw.WriteLine(@"<html><head>
<style>
body{font-family: arial, san-serif;}
.IssueDescription{padding: 5px;background-color: #eee;color: #333;font-size: small;}
.offendingLine{color: #D22;font-weight: bold;}
</style><body>");

            sw.WriteLine("<h1>YASAT Report</h1>");
            sw.WriteLine("<h2>Statistics</h2>");
            sw.WriteLine("{0} files scanned</br>", projectInfo.files.Count);
            bool haswildCard = false;
            foreach (Rule r in rules)
            {
                if (r.ExtensionList().Contains("*"))
                    haswildCard = true;
            }
            if (haswildCard)
                sw.WriteLine("<em>Warning:</em> One or more of the rules included in the scan has a wildcard (*) in the extension list" +
                        " this will cause YASAT to scan all files (including .gif, .zip, .foo, .bar, etc.). This will likely" + 
                        " not give you the Lines of Code measurements and time to code review estimates you were looking for.");
            sw.WriteLine("{0} lines of code scanned</br>", projectInfo.getTotalLinesOfCode());
            WriteLinesPerExtension("&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;{0} - {1} <br/>", sw, projectInfo);
            sw.WriteLine("{0} estimated hours to code review</br>", projectInfo.EstimateCodeReviewTime());
            sw.WriteLine("{0} total issues disovered</br>", projectInfo.GetIssuesCount());

            foreach (SourceFile file in projectInfo.files)
            {
                if (file.issues.Count > 0)
                {
                    sw.WriteLine("<h2>{0}</h2><h5>{1}", System.IO.Path.GetFileName(file.path), file.path);
                    foreach (SourceCodeIssue issue in file.issues)
                    {
                        sw.WriteLine(issue.GenerateHTMLSnippet());
                    }
                }
            }
            sw.WriteLine("</body></html>");
        }

        private void WriteLinesPerExtension(string format, StreamWriter sw, ProjectInfo projectInfo)
        {
            List<KeyValuePair<string, int>> ExtensionLineCount = new List<KeyValuePair<string, int>>();
            foreach (SourceFile file in projectInfo.files)
            {
                bool found = false;
                foreach (KeyValuePair<string, int> extLinePair in ExtensionLineCount)
                {
                    if (extLinePair.Key == System.IO.Path.GetExtension(file.path))
                    {
                        int newTotal = extLinePair.Value + file.lines;
                        ExtensionLineCount.Remove(extLinePair);
                        ExtensionLineCount.Add(new KeyValuePair<string, int>(System.IO.Path.GetExtension(file.path), newTotal));
                        found = true;
                        break;
                    }
                }
                if (!found && file.issues.Count > 0)
                {
                    ExtensionLineCount.Add(new KeyValuePair<string, int>(System.IO.Path.GetExtension(file.path), file.lines));
                }
            }

            foreach (KeyValuePair<string, int> extLinePair in ExtensionLineCount)
            {
                sw.WriteLine(format, extLinePair.Key, extLinePair.Value);
            }
        }

        private void CreateRule_Click(object sender, RoutedEventArgs e)
        {
            NewRule nr = new NewRule();
            nr.ShowDialog();
            if (null != nr.newRule)
                rules.Add(nr.newRule);

            MessageBox.Show("New rule successfully added, be sure to save the rule set to a rules file, if you are happy with the results", "New Rule Created", MessageBoxButton.OK, MessageBoxImage.Information);
            RefreshUI();
        }

        private void scanForNotes_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OpenRuleFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();

            ofd.Filter = "YASAT Rules File (*.xml)|*.xml";
            ofd.Multiselect = true;
            ofd.FilterIndex = 0;
            if (ofd.ShowDialog().Value)
            {
                List<string> realFiles = new List<string>();
                foreach (string file in ofd.FileNames)
                {
                    if (File.Exists(file))
                        realFiles.Add(file);
                }
                if (realFiles.Count > 0)
                    rules = RuleManager.GetAllRules(realFiles);
            }
        }

        private void ClearRules_Click(object sender, RoutedEventArgs e)
        {
            rules.Clear();
            RefreshUI();
        }
    }
}