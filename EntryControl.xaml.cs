using NoteSafe.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NoteSafe
{
    /// <summary>
    /// Interaction logic for EntryControl 
    /// </summary>
    public partial class EntryControl : UserControl
    {
        private Entry _entry;

        public Entry Entry { get => _entry; set => _entry = value; }

        public EntryControl()
        {
            InitializeComponent();
        }
    }
}
