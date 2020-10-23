using Microsoft.Win32;
using NoteSafe.Classes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Drawing.Printing;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace NoteSafe
{
    /// <summary>
    /// Interaction logic for the program's main window
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Entry> entriesList = new List<Entry>();
        private List<Category> categoriesList = new List<Category>();
        private Loading loadingScreen = new Loading();

        /// <summary>
        /// Constructor and initialization of the window
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            SafeDatabase.TB_Log = TB_Log;

            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.RunWorkerAsync();
            loadingScreen.ShowDialog();
        }

        /// <summary>
        /// Starts a new thread to run database connection test
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Event arguments</param>
        private void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(2000);
            e.Result = SafeDatabase.TestDatabaseConnectionAsync();
        }

        /// <summary>
        /// Does initialization and diagnostics after test thread is completed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Event arguments</param>
        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result is bool)
            {
                bool testResult = (bool)e.Result;

                loadingScreen.Close();
                if (testResult)
                {
                    HandleDatabaseError(false);
                    SafeDatabase.FetchCategories(categoriesList);
                    SafeDatabase.FetchEntries(entriesList);
                    ShowEntries();
                }
                else
                {
                    HandleDatabaseError(true);
                }
                CheckForEmpty();
                AddCategoriesToCombobox(categoriesList);
                ReadApplicationProperties();
            }
        }

        /// <summary>
        /// Reads saved application properties and sets them
        /// </summary>
        private void ReadApplicationProperties()
        {
            CB_ShowUsername.IsChecked = Properties.Settings.Default.Ui_show_username;
            CB_ShowPassword.IsChecked = Properties.Settings.Default.Ui_show_password;
        }

        /// <summary>
        /// Sets all buttons disabled if database connection was not established
        /// </summary>
        /// <param name="failed">Whether connection failed</param>
        private void HandleDatabaseError(bool failed)
        {
            BTN_DeleteCategory.IsEnabled = !failed;
            BTN_NewCategory.IsEnabled = !failed;
            BTN_NewEntry.IsEnabled = !failed;
            Menu_DeleteCategory.IsEnabled = !failed;
            Menu_NewCategory.IsEnabled = !failed;
            Menu_NewEntry.IsEnabled = !failed;
        }

        /// <summary>
        /// Method for showing an entry on screen
        /// </summary>
        private void ShowEntries()
        {
            foreach (Entry entry in entriesList)
            {
                ShowEntry(entry);
            }
        }

        /// <summary>
        /// Adds the entry to the screen
        /// </summary>
        /// <param name="entry">Entry to be shown</param>
        private void ShowEntry(Entry entry)
        {
            object node = RightStack.FindName("TB_NoEntries");
            if (node is TextBlock)
            {
                RightStack.Children.Remove((TextBlock)node);
                RightStack.UnregisterName("TB_NoEntries");
            }

            if (CB_Categories.SelectedItem is Category)
            {
                Category c = (Category)CB_Categories.SelectedItem;
                if (c.ID == entry.Category.ID || c.ID == -1)
                {
                    StackPanel sp = new StackPanel();
                    GenerateEntryControls(entry, sp);
                    RightStack.Children.Add(sp);
                }
                else
                    return;
            }
        }

        private void MenuItemExit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        /// <summary>
        /// Handles editing of the entry. Clears old entry and adds new controls and handlers
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event args</param>
        /// <param name="entry">Entry to be edited</param>
        /// <param name="sp">Stackpanel for entry label</param>
        /// <param name="dp">Dockpanel for entry controls</param>
        private void Edit_Click(object sender, RoutedEventArgs e, Entry entry, StackPanel sp)
        {
            NewEntryWindow editEntryWindow = new NewEntryWindow(new object[] { categoriesList, CB_Categories.SelectedItem, entry });
            editEntryWindow.Owner = this;
            bool? result = editEntryWindow.ShowDialog();

            if (result == false)
            {
                return;
            }

            String newEntryName = editEntryWindow.EntryName;
            String newEntryPassword = editEntryWindow.EntryPassword;
            String newEntryUsername = editEntryWindow.EntryUsername;
            Category newEntryCat = editEntryWindow.EntryCategory;

            Entry editedEntry = new Entry(entry.ID, newEntryName, newEntryUsername, newEntryPassword, newEntryCat);
            entry.Dispose();
            SafeDatabase.UpdateEntryInDataBase(editedEntry);

            // Updating the lists
            entriesList.Remove(entry);
            entriesList.Add(editedEntry);

            // Clearing all controls for old entry
            sp.Children.Clear();

            if (CB_Categories.SelectedItem is Category)
            {
                Category c = (Category)CB_Categories.SelectedItem;
                if (c.ID != editedEntry.Category.ID)
                {
                    RightStack.Children.Remove(sp);
                    CheckForEmpty();
                    return;
                }
            }

            GenerateEntryControls(editedEntry, sp);
        }

        /// <summary>
        /// Handler for clicking "new entry"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Event arguments</param>
        private void NewEntry_Click(object sender, RoutedEventArgs e)
        {
            NewEntryWindow newEntryWindow = new NewEntryWindow(new object[] { categoriesList, CB_Categories.SelectedItem });
            newEntryWindow.Owner = this;
            bool? result = newEntryWindow.ShowDialog();

            if (result == false)
            {
                return;
            }

            String newEntryName = newEntryWindow.EntryName;
            String newEntryPassword = newEntryWindow.EntryPassword;
            String newEntryUsername = newEntryWindow.EntryUsername;
            Category newEntryCat = newEntryWindow.EntryCategory;

            long newId = -1;
            using (Entry newEntry = new Entry(-1, newEntryName, newEntryUsername, newEntryPassword, newEntryCat))
            {
                newId = SafeDatabase.AddEntryToDataBase(newEntry);
                if (newId == -1)
                {
                    return;
                }
            }
            Entry newEntry_Finalized = new Entry(newId, newEntryName, newEntryUsername, newEntryPassword, newEntryCat);
            ShowEntry(newEntry_Finalized);
            entriesList.Add(newEntry_Finalized);
        }

        /// <summary>
        /// Generates new controls for an entry and adds them to a stackpanel
        /// </summary>
        /// <param name="entry">Entry</param>
        /// <param name="sp">Stackpanel that shows the entry</param>
        private void GenerateEntryControls(Entry entry, StackPanel sp)
        {
            EntryControl control = new EntryControl();
            control.Entry = entry;
            control.TB_EntryName.Text = entry.Name;
            if (CB_ShowPassword.IsChecked == false)
            {
                control.TB_EntryPassword.Text = "**********";
            }
            else 
                control.TB_EntryPassword.Text = entry.Password;
            control.TB_EntryUsername.Text = entry.Username;
            control.BTN_Edit.Click += delegate (object sender, RoutedEventArgs e) { Edit_Click(sender, e, entry, sp); };
            control.BTN_Delete.Click += delegate (object sender, RoutedEventArgs e) { Delete_Click(sender, e, entry, sp); };
            control.BTN_CopyUsername.Click += delegate (object sender, RoutedEventArgs e) { 
                Clipboard.SetText(entry.Username);
                Storyboard sb = (Storyboard)control.FindResource("Storyboard_copy");
                sb.BeginTime = TimeSpan.FromSeconds(0);
                sb.Begin();
            };
            control.BTN_CopyPassword.Click += delegate (object sender, RoutedEventArgs e) {
                Clipboard.SetText(entry.Password);
                Storyboard sb = (Storyboard)control.FindResource("Storyboard_copy_pw");
                sb.BeginTime = TimeSpan.FromSeconds(0);
                sb.Begin();
            };
            if (CB_ShowUsername.IsChecked == false)
            {
                control.Usernamebox.Visibility = Visibility.Hidden;
                control.Usernamebox.Height = 0;
            }
            sp.Children.Add(control);
        }

        /// <summary>
        /// Handler for clicking "delete entry"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Event arguments</param>
        /// <param name="entry">Entry that was selected</param>
        /// <param name="sp">Stackpanel that contains the entry</param>
        private void Delete_Click(object sender, RoutedEventArgs e, Entry entry, StackPanel sp)
        {
            MessageBoxResult result = MessageBox.Show("Do you really want to delete? Deleting cannot be undone!", "Confirm delete", MessageBoxButton.OKCancel);
            switch (result)
            {
                case MessageBoxResult.OK:
                    if (SafeDatabase.DeleteEntryFromDataBase(entry))
                    {
                        entriesList.Remove(entry);
                        RightStack.Children.Remove(sp);
                        CheckForEmpty();
                    }
                    break;
                case MessageBoxResult.Cancel:
                    break;
            }
        }

        /// <summary>
        /// Checks whether the screen is empty and handles messaging if no entries were found.
        /// </summary>
        private void CheckForEmpty()
        {
            if (RightStack.Children.Count > 0) return;

            TextBlock tbNoEntries = new TextBlock();
            tbNoEntries.Text = "No entries found.";
            tbNoEntries.FontStyle = FontStyles.Italic;
            tbNoEntries.FontSize = 22;
            tbNoEntries.Foreground = (SolidColorBrush)(new BrushConverter().ConvertFrom("#FF56699C"));
            object node = RightStack.FindName("TB_NoEntries");
            if (node is TextBlock)
            {
                RightStack.Children.Remove((TextBlock)node);
                RightStack.UnregisterName("TB_NoEntries");
            }
            tbNoEntries.Name = "TB_NoEntries";
            RightStack.RegisterName(tbNoEntries.Name, tbNoEntries);
            RightStack.Children.Add(tbNoEntries);
        }

        /// <summary>
        /// Handler for clicking "new category"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Event arguments</param>
        private void AddCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            // Creating new dialog
            AddCategoryDialog catDialog = new AddCategoryDialog();
            catDialog.Owner = this;
            bool? result = catDialog.ShowDialog();

            // Handling result
            if (result == true)
            {
                Category tempCat = new Category(-1, catDialog.Category);
                int newId = SafeDatabase.AddCategoryToDataBase(tempCat);
                if (newId == -1) return;
                Category finalizedNewCategory = new Category(newId, catDialog.Category);
                categoriesList.Add(finalizedNewCategory);

                List<Category> tmpList = new List<Category>();
                tmpList.Add(finalizedNewCategory);
                AddCategoriesToCombobox(tmpList);
            }
        }

        /// <summary>
        /// Handler for clicking "delete category"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Event arguments</param>
        public void DeleteCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            // Creating new dialog
            DeleteCategoryDialog deleteCatDialog = new DeleteCategoryDialog(categoriesList);
            deleteCatDialog.Owner = this;
            bool? result = deleteCatDialog.ShowDialog();

            // Handlind result
            if (result == true)
            {
                Category c = (Category)deleteCatDialog.CB_Categories.SelectedItem;
                bool success = SafeDatabase.DeleteCategoryFromDataBase(c);
                if (success)
                {
                    categoriesList.Remove(c);
                    if (CB_Categories.SelectedItem == c)
                        CB_Categories.SelectedIndex = 0;
                    CB_Categories.Items.Remove(c);
                }
                else
                    DeleteCategoryButton_Click(sender, e);
            }
        }

        /// <summary>
        /// Adds categories to the combobox
        /// </summary>
        /// <param name="cats">Categories</param>
        private void AddCategoriesToCombobox(List<Category> cats)
        {
            if (CB_Categories.Items.Count < 1)
            {
                CB_Categories.Items.Add(new Category(-1, "<All>"));
            }

            foreach (Category c in cats)
            {
                CB_Categories.Items.Add(c);
            }

            CB_Categories.SelectedIndex = 0;
        }

        /// <summary>
        /// Handler for exit click. Shuts down the program
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Event arguments</param>
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            MenuItemExit_Click(sender, e);
        }

        /// <summary>
        /// Event handler for logging textbox. Handles automatic scrolling.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Event arguments</param>
        private void TB_Log_TextChanged(object sender, TextChangedEventArgs e)
        {
            TB_Log.CaretIndex = TB_Log.Text.Length;
            TB_Log.ScrollToEnd();
        }

        /// <summary>
        /// Event handler for selecting a category. Filters entries based on selected category
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CB_Categories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RightStack.Children.Clear();

            foreach (Entry entry in entriesList)
            {
                ShowEntry(entry);
            }

            CheckForEmpty();
        }

        /// <summary>
        /// Event handler for checking "show username" checkbox. Hides usernames
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e">Event arguments</param>
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            bool check = (bool)((CheckBox)sender).IsChecked;
            if (RightStack is null) return;
            List<List<EntryControl>> boxes = RightStack.Children.OfType<StackPanel>().Select(dp => dp.Children.OfType<EntryControl>().ToList()).ToList();
            foreach (List<EntryControl> lists in boxes)
            {
                foreach (EntryControl item in lists)
                {
                    if (check)
                    {
                        item.Usernamebox.Height = 20;
                        item.Usernamebox.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        item.Usernamebox.Height = 0;
                        item.Usernamebox.Visibility = Visibility.Hidden;
                    }
                }
            }
        }

        /// <summary>
        /// Event handler for search box text change. Handles search operation
        /// </summary>
        /// <param name="sender">Sender(Textbox)</param>
        /// <param name="e">Event arguments</param>
        private void TB_Searchbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            String search = TB_Searchbox.Text;

            List<Entry> results = new List<Entry>();

            foreach (Entry entry in entriesList)
            {
                string entryString = entry.Name + entry.Username;
                bool containsSearchResult = entryString.ContainsCaseInsensitive(search, StringComparison.OrdinalIgnoreCase);
                bool ignoreCaseSearchResult = entryString.StartsWith(search, System.StringComparison.CurrentCultureIgnoreCase);
                bool endsWithSearchResult = entryString.EndsWith(search, System.StringComparison.CurrentCultureIgnoreCase);

                if (containsSearchResult || ignoreCaseSearchResult || endsWithSearchResult)
                    results.Add(entry);
            }

            RightStack.Children.Clear();

            foreach (Entry entry1 in results)
            {
                ShowEntry(entry1);
            }

            CheckForEmpty();
        }

        /// <summary>
        /// Event handler for clicking settings
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event arguments</param>
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            Settings settings = new Settings();
            settings.Owner = this;
            if (settings.ShowDialog() == true)
            {
                ReadApplicationProperties();
            }
        }

        /// <summary>
        /// Event handler for "show password" checkbox in the bottom bar
        /// </summary>
        /// <param name="sender">Sender(Checkbox)</param>
        /// <param name="e">Event arguments</param>
        private void CB_ShowPassword_Checked(object sender, RoutedEventArgs e)
        {
            bool check = (bool)((CheckBox)sender).IsChecked;
            if (RightStack is null) return;
            List<List<EntryControl>> boxes = RightStack.Children.OfType<StackPanel>().Select(dp => dp.Children.OfType<EntryControl>().ToList()).ToList();
            foreach (List<EntryControl> lists in boxes)
            {
                foreach (EntryControl item in lists)
                {
                    if (check)
                    {
                        item.TB_EntryPassword.Text = item.Entry.Password;
                    }
                    else
                    {
                        item.TB_EntryPassword.Text = "**********";
                    }
                }
            }
        }
    }

    /// <summary>
    /// A helper class for caseinsensitive search strings
    /// </summary>
    public static class StringExtensions
    {
        public static bool ContainsCaseInsensitive(this string source, string toCheck, StringComparison comp)
        {
            return source?.IndexOf(toCheck, comp) >= 0;
        }
    }
}
