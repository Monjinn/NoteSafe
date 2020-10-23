using NoteSafe.Classes;
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
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace NoteSafe
{
    /// <summary>
    /// Interaction logic for NewEntry window
    /// </summary>
    public partial class NewEntryWindow : Window
    {
        /// <summary>
        /// Setting class properties
        /// </summary>
        private String _entryName;
        private String _entryUsername;
        private Category _entryCategory;
        private String _entryPassword;
        public string EntryName { get => _entryName; set => _entryName = value; }
        public string EntryUsername { get => _entryUsername; set => _entryUsername = value; }
        public Category EntryCategory { get => _entryCategory; set => _entryCategory = value; }
        public string EntryPassword { get => _entryPassword; set => _entryPassword = value; }

        /// <summary>
        /// Constructor for NewEntryWindow
        /// </summary>
        /// <param name="args"></param>
        public NewEntryWindow(object[] args)
        {
            InitializeComponent();
            this.DataContext = this;
            foreach (object item in args)
            {
                if (item is List<Category>)
                {
                    AddCategories((List<Category>)item);
                    TB_EntryCategory.SelectedIndex = 0;
                }
                if (item is Category)
                    TB_EntryCategory.SelectedItem = item;
                if (item is Entry)
                {
                    UpdateFields((Entry)item);
                }
            }
        }

        /// <summary>
        /// Private method for adding categories to the combobox
        /// </summary>
        /// <param name="cats">List of all categories</param>
        private void AddCategories(List<Category> cats)
        {
            foreach (Category c in cats)
            {
                TB_EntryCategory.Items.Add(c);
            }
        }

        /// <summary>
        /// Updates the local class fields with given Entry's data
        /// </summary>
        /// <param name="e">Entry</param>
        private void UpdateFields(Entry e)
        {
            EntryName = e.Name;
            EntryCategory = e.Category;
            TB_EntryCategory.SelectedItem = e.Category;
            EntryUsername = e.Username;
            EntryPassword = e.Password;
        }

        /// <summary>
        /// Event handler for clicking "accept"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Event arguments</param>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        /// <summary>
        /// Event handler for clicking "cancel"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Event arguments</param>
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        /// <summary>
        /// Event handler for clicking "generate password". Generates a new password
        /// which will be shown in a text field.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Event arguments</param>
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            bool? noSpecialChars = CheckBox_NoSpecialChars.IsChecked;
            bool? noNumbers = CheckBox_NoNumbers.IsChecked;
            double pwLength = Slider_PwLength.Value;

            TB_EntryPassword.Text = GeneratePassword(noSpecialChars ?? false, noNumbers ?? false, (int)pwLength);
        }

        /// <summary>
        /// Generates a random password string with given restrictions.
        /// </summary>
        /// <param name="noSpecialChars">Boolean whether password should contain special characters</param>
        /// <param name="noNumbers">Boolean whether password should contain numbers</param>
        /// <param name="length">Length of the password</param>
        /// <returns>A new random password string</returns>
        private String GeneratePassword(bool noSpecialChars, bool noNumbers, int length)
        {
            Random random = new Random();
            const string chars = "abcdefghijklmnopqrstuvwxyzåäöABCDEFGHIJKLMNOPQRSTUVWXYZÅÄÖ0123456789_.,;&?+*#@!()[]{}%";
            const string noChars = "abcdefghijklmnopqrstuvwxyzåäöABCDEFGHIJKLMNOPQRSTUVWXYZÅÄÖ0123456789";
            const string noNums = "abcdefghijklmnopqrstuvwxyzåäöABCDEFGHIJKLMNOPQRSTUVWXYZÅÄÖ_.,;&?+*#@!()[]{}%";
            const string regular = "abcdefghijklmnopqrstuvwxyzåäöABCDEFGHIJKLMNOPQRSTUVWXYZÅÄÖ";
            if (noSpecialChars && noNumbers)
                return new string(Enumerable.Repeat(regular, length)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
            else if (noSpecialChars)
                return new string(Enumerable.Repeat(noChars, length)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
            else if (noNumbers)
                return new string(Enumerable.Repeat(noNums, length)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
            else
                return new string(Enumerable.Repeat(chars, length)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        /// <summary>
        /// Event handler for changing category. Sets the entry's category to the selected one.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Event arguments</param>
        private void TB_EntryCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.EntryCategory = (Category)TB_EntryCategory.SelectedItem;
        }

        /// <summary>
        /// Event handler for window loaded. Triggers right after window is loaded. Formats
        /// all the fields in the window with the data from Entry.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Event arguments</param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (EntryPassword is null)
            {
                TB_EntryPassword.Text = GeneratePassword(false, false, 13);
            }
            if (EntryName is null)
            {
                TB_EntryName.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
                TB_EntryUsername.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
                TB_EntryPassword.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty).UpdateSource();
            }
            if (!Validation.GetHasError(TB_EntryName)
                && !Validation.GetHasError(TB_EntryUsername)
                && !Validation.GetHasError(TB_EntryPassword))
            {
                BTN_Accept.IsEnabled = true;
            }
            else
                BTN_Accept.IsEnabled = false;
        }

        /// <summary>
        /// Event handler for all textboxes in the window. Checks for validation errors.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Event arguments</param>
        private void TB_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!Validation.GetHasError(TB_EntryName)
                && !Validation.GetHasError(TB_EntryUsername)
                && !Validation.GetHasError(TB_EntryPassword))
            {
                BTN_Accept.IsEnabled = true;
            }
            else
                BTN_Accept.IsEnabled = false;
        }
    }

    /// <summary>
    /// Validation rule class for fields
    /// </summary>
    public class EntryFieldsRule : ValidationRule
    {
        public String Name { get; set; }
        public String Username { get; set; }
        public String Password { get; set; }

        public EntryFieldsRule() { }

        /// <summary>
        /// Overrides default "validate" method. Checks for empty field.
        /// </summary>
        /// <param name="value">Value of the field</param>
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
