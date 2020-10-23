using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// Interaction logic for AddCategoryDialog dialog
    /// </summary>
    public partial class AddCategoryDialog : Window
    {
        private String _category;
        public string Category { get => _category; set => _category = value; }

        /// <summary>
        /// Constructor
        /// </summary>
        public AddCategoryDialog()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        /// <summary>
        /// Event handler for text changed for all textboxes. Checks for validation errors
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Event arguments</param>
        private void TB_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!Validation.GetHasError(TB_Category))
            {
                BTN_Accept.IsEnabled = true;
            }
            else
                BTN_Accept.IsEnabled = false;
        }

        /// <summary>
        /// Accept click handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Event arguments</param>
        private void Accept_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        /// <summary>
        /// Decline click handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Decline_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        /// <summary>
        /// Window loaded event. Checks for validation errors
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Event arguments</param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (Category is null)
            {
                TB_Category.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
            }
            if (!Validation.GetHasError(TB_Category))
            {
                BTN_Accept.IsEnabled = true;
            }
            else
                BTN_Accept.IsEnabled = false;
        }
    }

    /// <summary>
    /// Helper class for validation
    /// </summary>
    public class NewCategoryRule : ValidationRule
    {
        public NewCategoryRule() { }

        /// <summary>
        /// Overrides validation rule
        /// </summary>
        /// <param name="value">Value to be validated</param>
        /// <param name="cultureInfo">Culture info</param>
        /// <returns>Validation result</returns>
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value is null || ((string)value).Length < 1)
            {
                return new ValidationResult(false,
                  $"Empty fields not accepted");
            }
            return ValidationResult.ValidResult;
        }
    }
}
