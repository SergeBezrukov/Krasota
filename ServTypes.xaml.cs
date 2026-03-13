using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using MySqlConnector;

namespace Krasota
{
    public partial class ServTypes : Window
    {
        public static string ConnectionString = "server=127.0.0.1;database=saloonBeauty;uid=root;pwd=1234;port=3306;";
        public ObservableCollection<ServTypesClass> servTypesCollection { get; set; }

        public ServTypes()
        {
            servTypesCollection = new ObservableCollection<ServTypesClass>();
            InitializeComponent();
            servTypeCell.ItemsSource = servTypesCollection;
            ShowActiveServTypes();
        }

        private void ShowActiveServTypes()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    string query = "SELECT * FROM servTypes WHERE servTypeActivity = 'да';";
                    MySqlCommand comm = new MySqlCommand(query, conn);
                    MySqlDataReader reader = comm.ExecuteReader();

                    servTypesCollection.Clear();
                    while (reader.Read())
                    {
                        servTypesCollection.Add(new ServTypesClass()
                        {
                            ServTypeCode = reader.GetInt32("servTypeCode"),
                            ServType = reader.GetString("servType"),
                            ServTypeActivity = reader.GetString("servTypeActivity")
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке типов услуг: {ex.Message}");
            }
        }

        private void b_servTypeDelete_Click(object sender, RoutedEventArgs e)
        {
            var selectedServType = servTypeCell.SelectedItem as ServTypesClass;
            if (selectedServType == null)
            {
                MessageBox.Show("Выберите тип услуги для удаления!");
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    string query = "UPDATE servTypes SET servTypeActivity = 'нет' WHERE servTypeCode = @servTypeCode;";
                    MySqlCommand comm = new MySqlCommand(query, conn);
                    comm.Parameters.AddWithValue("@servTypeCode", selectedServType.ServTypeCode);

                    int result = comm.ExecuteNonQuery();
                    if (result > 0)
                    {
                        MessageBox.Show("Тип услуги успешно удален!");
                        ShowActiveServTypes();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении типа услуги: {ex.Message}");
            }
        }

        private void b_servTypeAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();

                    string maxCodeQuery = "SELECT MAX(servTypeCode) FROM servTypes;";
                    MySqlCommand maxComm = new MySqlCommand(maxCodeQuery, conn);
                    var maxCode = maxComm.ExecuteScalar();
                    int newCode = (maxCode == DBNull.Value) ? 1 : Convert.ToInt32(maxCode) + 1;

                    string query = "INSERT INTO servTypes (servTypeCode, servType, servTypeActivity) " +
                                   "VALUES (@servTypeCode, 'Новый тип услуги', 'да');";

                    MySqlCommand comm = new MySqlCommand(query, conn);
                    comm.Parameters.AddWithValue("@servTypeCode", newCode);

                    int result = comm.ExecuteNonQuery();
                    if (result > 0)
                    {
                        MessageBox.Show("Тип услуги успешно добавлен!");
                        ShowActiveServTypes();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении типа услуги: {ex.Message}");
            }
        }

        private void b_servTypeChange_Click(object sender, RoutedEventArgs e)
        {
            var selectedServType = servTypeCell.SelectedItem as ServTypesClass;
            if (selectedServType == null)
            {
                MessageBox.Show("Выберите тип услуги для изменения!");
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    string query = "UPDATE servTypes SET servType = @servType WHERE servTypeCode = @servTypeCode;";

                    MySqlCommand comm = new MySqlCommand(query, conn);
                    comm.Parameters.AddWithValue("@servTypeCode", selectedServType.ServTypeCode);
                    comm.Parameters.AddWithValue("@servType", selectedServType.ServType);

                    int result = comm.ExecuteNonQuery();
                    if (result > 0)
                    {
                        MessageBox.Show("Данные типа услуги успешно обновлены!");
                        ShowActiveServTypes();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при изменении типа услуги: {ex.Message}");
            }
        }

        private void b_servTypeShow_Click(object sender, RoutedEventArgs e)
        {
            servTypeCell.CommitEdit(DataGridEditingUnit.Cell, true);
            servTypeCell.CommitEdit(DataGridEditingUnit.Row, true);
            servTypeCell.UpdateLayout();

            var row = servTypeCell.SelectedItem as ServTypesClass ?? servTypeCell.CurrentItem as ServTypesClass;
            var servType = row?.ServType?.Trim();

            if (string.IsNullOrEmpty(servType))
            {
                MessageBox.Show("Выберите тип услуги или введите данные в таблицу для поиска!");
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    string query = "SELECT * FROM servTypes WHERE servType LIKE @servType AND servTypeActivity = 'да';";
                    MySqlCommand comm = new MySqlCommand(query, conn);
                    comm.Parameters.AddWithValue("@servType", $"%{servType}%");

                    MySqlDataReader reader = comm.ExecuteReader();
                    servTypesCollection.Clear();
                    while (reader.Read())
                    {
                        servTypesCollection.Add(new ServTypesClass()
                        {
                            ServTypeCode = reader.GetInt32("servTypeCode"),
                            ServType = reader.GetString("servType"),
                            ServTypeActivity = reader.GetString("servTypeActivity")
                        });
                    }

                    if (servTypesCollection.Count == 0)
                    {
                        MessageBox.Show("Типы услуг с таким названием не найдены!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при поиске типа услуги: {ex.Message}");
            }
        }

        private void b_servTypeReturn_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }

    public class ServTypesClass
    {
        public int ServTypeCode { get; set; }
        public string ServType { get; set; }
        public string ServTypeActivity { get; set; }
    }
}