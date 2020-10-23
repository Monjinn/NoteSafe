using NoteSafe.Classes;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace NoteSafe
{
    /// <summary>
    /// Class for handling database connection and queries
    /// </summary>
    class SafeDatabase
    {
        public static TextBox TB_Log { get; set; }

        /// <summary>
        /// Returns saved database connection
        /// </summary>
        /// <returns>SqlConnection to program database</returns>
        public static SqlConnection GetDatabaseConnection()
        {
            SqlConnectionStringBuilder conSB = new SqlConnectionStringBuilder();
            conSB.DataSource = @"(LocalDB)\MSSQLLocalDB";
            conSB.AttachDBFilename = Properties.Settings.Default.Database_location;
            conSB.IntegratedSecurity = false;
            return new SqlConnection(conSB.ToString());
        }

        /// <summary>
        /// Asynchronous test for database connection
        /// </summary>
        /// <returns>Whether the connection was successful or not</returns>
        public static bool TestDatabaseConnectionAsync()
        {
            using (SqlConnection con = GetDatabaseConnection())
            {
                try
                {
                    con.Open();
                    con.Close();
                    return true;
                }
                catch (SqlException e)
                {
                    MessageBox.Show("Database connection failed. Please check settings before continuing." + "\n" + e.Message, "Database error");
                    return false;
                }
            }
        }

        /// <summary>
        /// Deletes given category from the database and a list, if deletion was successful
        /// </summary>
        /// <param name="c">Category</param>
        /// <returns>Whether delete was successful or not</returns>
        public static bool DeleteCategoryFromDataBase(Category c)
        {
            try
            {
                TB_Log.Text += "\nDeleting Category...";
                using (SqlConnection connection = GetDatabaseConnection())
                {
                    connection.Open();

                    String checkForDelete = "SELECT * FROM Entries " +
                                            "WHERE CategoryId = @1";
                    SqlCommand checkCommand = new SqlCommand(checkForDelete, connection);
                    checkCommand.Parameters.Add(new SqlParameter("1", c.ID));
                    using (SqlDataReader reader = checkCommand.ExecuteReader())
                    {
                        int rows = 0;
                        while (reader.Read())
                        {
                            rows++;
                        }

                        if (rows > 0)
                        {
                            MessageBox.Show("Cannot delete category \"" + c.Name + "\". Total of " + rows + " entries have this category.\nDelete or edit these entries before deleting this category.", "Warning");
                            return false;
                        }
                    }

                    String deleteCatStatement = "DELETE FROM Categories " +
                                                "WHERE CategoryId = @1";
                    SqlCommand deleteCatCommand = new SqlCommand(deleteCatStatement, connection);
                    deleteCatCommand.Parameters.Add(new SqlParameter("1", c.ID));
                    deleteCatCommand.ExecuteNonQuery();

                    TB_Log.Text += "\nCategory deleted successfully";
                }

                return true;
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Deleting category failed.\n" + ex.Message, "Error!");
                Console.WriteLine(ex.ToString());
                TB_Log.Text += "\nConnection Failed!";
                return false;
            }
        }

        /// <summary>
        /// Deletes an entry from the database
        /// </summary>
        /// <param name="e">Entry</param>
        /// <returns>Whether the database operation was successful</returns>
        public static bool DeleteEntryFromDataBase(Entry e)
        {
            try
            {
                TB_Log.Text += "\nDeleting Entry...";
                using (SqlConnection connection = GetDatabaseConnection())
                {
                    connection.Open();

                    String deleteEntryStatement = "DELETE FROM Entries " +
                                                  "WHERE EntryId = @1";
                    SqlCommand deleteEntryCommand = new SqlCommand(deleteEntryStatement, connection);
                    deleteEntryCommand.Parameters.Add(new SqlParameter("1", e.ID));
                    deleteEntryCommand.ExecuteNonQuery();

                    TB_Log.Text += "\nEntry deleted successfully";
                }

                return true;
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Deleting an entry failed.\n" + ex.Message, "Error!");
                Console.WriteLine(ex.ToString());
                TB_Log.Text += "\nDeleting Failed!";
                return false;
            }
        }

        /// <summary>
        /// Updates the row of an entry in database with it's new values.
        /// </summary>
        /// <param name="e">Entry</param>
        /// <returns></returns>
        public static bool UpdateEntryInDataBase(Entry e)
        {
            try
            {
                TB_Log.Text += "\nEditing Entry...";
                using (SqlConnection connection = GetDatabaseConnection())
                {
                    connection.Open();

                    String updateEntryStatement = "UPDATE Entries " +
                                               "SET Name = @2, Username = @3, Password = @4, CategoryId = @5 " +
                                               "WHERE EntryId = @1";
                    SqlCommand updateEntryCommand = new SqlCommand(updateEntryStatement, connection);
                    updateEntryCommand.Parameters.Add(new SqlParameter("1", e.ID));
                    updateEntryCommand.Parameters.Add(new SqlParameter("2", e.Name));
                    updateEntryCommand.Parameters.Add(new SqlParameter("3", e.Username));
                    updateEntryCommand.Parameters.Add(new SqlParameter("4", e.Password));
                    updateEntryCommand.Parameters.Add(new SqlParameter("5", e.Category.ID.ToString()));

                    int r = updateEntryCommand.ExecuteNonQuery();

                    TB_Log.Text += "\nEntry edited successfully, " + r + " rows affected";
                }

                return true;
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Adding an entry failed.\n" + ex.Message, "Error!");
                TB_Log.Text += "\nConnection Failed!";
                return false;
            }
        }

        /// <summary>
        /// Adds a category to the database
        /// </summary>
        /// <param name="c">Category</param>
        /// <returns></returns>
        public static int AddCategoryToDataBase(Category c)
        {
            try
            {
                TB_Log.Text += "\nAdding Category";
                using (SqlConnection connection = GetDatabaseConnection())
                {
                    connection.Open();

                    String addCatStatement = "INSERT INTO Categories " +
                                               "VALUES (@1);SELECT CAST(scope_identity() AS int)";
                    SqlCommand addCatCommand = new SqlCommand(addCatStatement, connection);
                    addCatCommand.Parameters.Add(new SqlParameter("1", c.Name));

                    int addedCatId = (int)addCatCommand.ExecuteScalar();

                    TB_Log.Text += "\nCategory added successfully.";

                    return addedCatId;
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Adding a category failed.\n" + ex.Message, "Error!");
                Console.WriteLine(ex.ToString());
                TB_Log.Text += "\nConnection Failed!";

                return -1;
            }
        }

        /// <summary>
        /// Inserts an entry into the database
        /// </summary>
        /// <param name="e">Entry to be added to database</param>
        public static long AddEntryToDataBase(Entry e)
        {
            try
            {
                TB_Log.Text += "\nAdding Entry";
                using (SqlConnection connection = GetDatabaseConnection())
                {
                    connection.Open();

                    String addEntryStatement = "INSERT INTO Entries " +
                                               "VALUES (@1, @2, @3, @4);SELECT CAST(scope_identity() AS int)";
                    SqlCommand addEntryCommand = new SqlCommand(addEntryStatement, connection);
                    addEntryCommand.Parameters.Add(new SqlParameter("1", e.Name));
                    addEntryCommand.Parameters.Add(new SqlParameter("2", e.Username));
                    addEntryCommand.Parameters.Add(new SqlParameter("3", e.Password));
                    addEntryCommand.Parameters.Add(new SqlParameter("4", e.Category.ID.ToString()));

                    long addedEntryId = (int)addEntryCommand.ExecuteScalar();

                    TB_Log.Text += "\nEntry added successfully.";

                    return addedEntryId;
                }
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Adding an entry failed.\n" + ex.Message, "Error!");
                Console.WriteLine(ex.ToString());
                TB_Log.Text += "\nConnection Failed!";

                return -1;
            }
        }

        /// <summary>
        /// Fetches all entries from the database into a list and returns whether the operation was successful
        /// </summary>
        /// <param name="entriesList">Adds fetched entries to the list</param>
        /// <returns>Whether database operation was successful</returns>
        public static bool FetchEntries(List<Entry> entriesList)
        {
            try
            {
                // Connect to SQL
                TB_Log.Text += "Connecting to SQL Server ... ";
                using (SqlConnection connection = GetDatabaseConnection())
                {
                    connection.Open();
                    TB_Log.Text += "\nDatabase connection successful.";
                    TB_Log.Text += "\n" + connection.DataSource;
                    TB_Log.Text += "\n" + connection.Database;
                    TB_Log.Text += "\n" + connection.Credential;
                    TB_Log.Text += "\n" + connection.WorkstationId;

                    String entriesQuery = "SELECT Entry.EntryId, " +
                                                 "Entry.Name, " +
                                                 "Entry.Username, " +
                                                 "Entry.Password, " +
                                                 "Category.Name, " +
                                                 "Category.CategoryId CategoryId " +
                                          "FROM Entries Entry JOIN Categories Category " +
                                          "ON Entry.CategoryId = Category.CategoryId";
                    SqlCommand entryCommand = new SqlCommand(entriesQuery, connection);
                    using (SqlDataReader entryReader = entryCommand.ExecuteReader())
                    {
                        int totalRead = 0;
                        while (entryReader.Read())
                        {
                            Category readCategory = new Category((Int32)entryReader[5], (String)entryReader[4]);
                            Int32 tempId = (Int32)entryReader[0];
                            long longId = Convert.ToInt32(tempId);
                            Entry readEntry = new Entry(longId, (String)entryReader[1], (String)entryReader[2], (String)entryReader[3], readCategory);
                            entriesList.Add(readEntry);
                            totalRead++;
                        }
                        TB_Log.Text += "\nTotal of " + totalRead + " entries read.";
                    }
                }

                return true;
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Adding an entry failed.\n" + ex.Message, "Error!");
                Console.WriteLine(ex.ToString());
                TB_Log.Text += "\nConnection Failed!";

                return false;
            }
        }

        /// <summary>
        /// Fetches all categories from the database into a list and returns whether the operation was successful
        /// </summary>
        /// <param name="categoriesList">Adds fetched categories to the list<</param>
        /// <returns>Whether database operation was successful</returns>
        public static bool FetchCategories(List<Category> categoriesList)
        {
            try
            {
                TB_Log.Text += "Reading categories ... ";
                using (SqlConnection connection = GetDatabaseConnection())
                {
                    connection.Open();
                    String categoriesQuery = "SELECT * FROM Categories";
                    SqlCommand getCategoriesCommand = new SqlCommand(categoriesQuery, connection);
                    using (SqlDataReader catReader = getCategoriesCommand.ExecuteReader())
                    {
                        while (catReader.Read())
                        {
                            Category readCategory = new Category((Int32)catReader[0], (String)catReader[1]);
                            categoriesList.Add(readCategory);
                        }
                    }
                }
                return true;
            }
            catch (SqlException ex)
            {
                MessageBox.Show("Adding an entry failed.\n" + ex.Message, "Error!");
                Console.WriteLine(ex.ToString());
                TB_Log.Text += "\nConnection Failed!";

                return false;
            }
        }
    }
}
