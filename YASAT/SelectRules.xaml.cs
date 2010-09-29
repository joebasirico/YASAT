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
using YASATEngine;

namespace YASAT
{
    /// <summary>
    /// Interaction logic for SelectRules.xaml
    /// </summary>
    public partial class SelectRules : Window
    {
        private List<Rule> _rules;

        public SelectRules()
        {
            InitializeComponent();
        }

        public SelectRules(List<YASATEngine.Rule> rules)
        {
            // TODO: Complete member initialization
            _rules = rules;
        }

        private void InitializeRuleList()
        {
            foreach (Rule r in _rules)
            {
                
            }
        }
    }
}
