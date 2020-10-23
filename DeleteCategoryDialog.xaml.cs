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
using System.Windows.Shapes;

namespace NoteSafe
{
    /// <summary>
    /// Interaction logic for DeleteCategoryDialog window
    /// </summary>
    public partial class DeleteCategoryDialog : Window
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="categories">List of categories</param>
        public DeleteCategoryDialog(List<Category> categories)
        {
            InitializeComponent();

            foreach (Category category in categories)
            {
                if (category.ID != -1)
                    CB_Categories.Items.Add(category);
            }

            if (CB_Categories.Items.Count == 0)
            {
                CB_Categories.Items.Add("<No categories>");
                BTN_Accept.IsEnabled = false;
            }

            CB_Categories.SelectedIndex = 0;
        }

        /// <summary>
        /// Accept handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BTN_Accept_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        /// <summary>
        /// Cancel handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
