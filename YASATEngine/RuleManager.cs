using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using YASATEngine.Properties;

namespace YASATEngine
{
    public static class RuleManager
    {
        public static List<Rule> GetAllRules()
        {
            return GetAllRules(Settings.Default.RulesFile);
        }

        public static List<Rule> GetAllRules(string file)
        {
            List<Rule> rules = new List<Rule>();

            if (File.Exists(file))
            {
                XmlTextReader reader = new XmlTextReader(file);

                while (reader.Read())
                {
                    if ("Rule" == reader.Name)
                    {
                        Rule r = new Rule();
                        while(!(XmlNodeType.EndElement == reader.NodeType && "Rule" == reader.Name))
                        {
                            reader.Read();
                            switch (reader.Name)
                            {
                                case "Title":
                                    r.Title = ReadElement(reader, "Title");
                                    break;
                                case "Severity":
                                    r.Severity = ReadElement(reader, "Severity");
                                    break;
                                case "Type":
                                    r.Type = ReadElement(reader, "Type");
                                    break;
                                case "RegularExpressionPattern":
                                    r.RegexSearchPattern = ReadElement(reader, "RegularExpressionPattern");
                                    break;
                                case "Extensions":
                                    r.AddExtensionList(ReadList(reader, "Extensions", "Extension"));
                                    break;
                                case "Guidance":
                                    r.AddGuidanceList(ReadList(reader, "Guidance", "Url"));
                                    break;
                                case "Description":
                                    r.Description = ReadElement(reader, "Description");
                                    break;
                            }
                        }
                        rules.Add(r);
                    }
                }
                reader.Close();
            }

            
            return rules;

        }

        private static List<string> ReadList(XmlTextReader reader, string NodeName, string SubnodeName)
        {
            List<string> toReturn = new List<string>();
            while (!(reader.NodeType == XmlNodeType.EndElement && NodeName == reader.Name))
            {
                toReturn.Add(ReadElement(reader, SubnodeName));
                reader.Read();
            }
            return toReturn;
        }

        private static string ReadElement(XmlTextReader reader, string NodeName)
        {
            string toReturn = "";
            while (!(reader.NodeType == XmlNodeType.Text))
                reader.Read();

            toReturn = reader.Value;

            while (!(reader.NodeType == XmlNodeType.EndElement && NodeName == reader.Name))
                reader.Read();

            reader.Read();//consume the endElement and return
            return toReturn;
        }

        public static void AddRule(Rule r)
        {

            StreamReader InRules = new StreamReader(Settings.Default.RulesFile);
            string[] lines = Regex.Split(InRules.ReadToEnd(), "\r\n");
            InRules.Close();

            StreamWriter OutRules = new StreamWriter(Settings.Default.RulesFile);
            
            foreach(string line in lines)
            {
                if(!line.Contains("</Rules>"))
                    OutRules.WriteLine(line);
                else
                {
                    OutRules.Write(r.GenerateXMLSnippet());
                }
            }
            OutRules.WriteLine("</Rules>");
            OutRules.Close();
        }

        public static void RemoveRule(Rule r)
        {
        }

        public static int RuleCount()
        {
            return GetAllRules().Count;
        }

        public static List<Rule> GetAllRules(List<string> realFiles)
        {
            List<Rule> rulesList = new List<Rule>();
            foreach (string file in realFiles)
            {
                rulesList.AddRange(GetAllRules(file));
            }

            return rulesList;
        }
    }
}
