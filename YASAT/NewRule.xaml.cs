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
using System.Windows.Shapes;
using YASAT.Properties;
using System.Text.RegularExpressions;
using YASATEngine;

namespace YASAT
{
    /// <summary>
    /// Interaction logic for NewRule.xaml
    /// </summary>
    public partial class NewRule : Window
    {
        public Rule newRule;
        public NewRule()
        {
            InitializeComponent();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string title = TitleBox.Text;
            string description = DescriptionBox.Text;
            string regex = RegexBox.Text;
            List<string> extensions = new List<string>(ExtensionBox.Text.Split(','));
            List<string> guidance = new List<string>(Regex.Split(GuidanceBox.Text, "\r\n"));

            newRule = new Rule(title, description, regex, extensions, guidance);
            this.Close();
        }
    }
}
