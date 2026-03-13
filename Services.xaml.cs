using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using MySqlConnector;

namespace Krasota
{
    public partial class Services : Window
    {
        public static string ConnectionString = "server=127.0.0.1;database=saloonBeauty;uid=root;pwd=NO;port=3306;";
        public ObservableCollection<ServicesClass> servicesCollection { get; set; }

        public Services()
        {
            servicesCollection = new ObservableCollection<ServicesClass>();
            InitializeComponent();
            serviceCell.ItemsSource = servicesCollection;
            ShowActiveServices();
        }

        private void ShowActiveServices()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    string query = "SELECT * FROM services WHERE servActivity = 'да';";
                    MySqlCommand comm = new MySqlCommand(query, conn);
                    MySqlDataReader reader = comm.ExecuteReader();

                    servicesCollection.Clear();
                    while (reader.Read())
                    {
                        servicesCollection.Add(new ServicesClass()
                        {
                            ServCode = reader.GetInt32("servCode"),
                            ServName = reader.GetString("servName"),
                            ServPrice = reader.GetInt32("servPrice"),
                            ServDuration = reader.GetInt32("servDuration"),
                            ServTypeCode = reader.GetInt32("servTypeCode"),
                            ServActivity = reader.GetString("servActivity")
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке услуг: {ex.Message}");
            }
        }

        private void b_serviceDelete_Click(object sender, RoutedEventArgs e)
        {
            var selectedService = serviceCell.SelectedItem as ServicesClass;
            if (selectedService == null)
            {
                MessageBox.Show("Выберите услугу для удаления!");
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    string query = "UPDATE services SET servActivity = 'нет' WHERE servCode = @servCode;";
                    MySqlCommand comm = new MySqlCommand(query, conn);
                    comm.Parameters.AddWithValue("@servCode", selectedService.ServCode);

                    int result = comm.ExecuteNonQuery();
                    if (result > 0)
                    {
                        MessageBox.Show("Услуга успешно удалена!");
                        ShowActiveServices();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении услуги: {ex.Message}");
            }
        }

        private void b_serviceAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();

                    string maxCodeQuery = "SELECT MAX(servCode) FROM services;";
                    MySqlCommand maxComm = new MySqlCommand(maxCodeQuery, conn);
                    var maxCode = maxComm.ExecuteScalar();
                    int newCode = (maxCode == DBNull.Value) ? 1 : Convert.ToInt32(maxCode) + 1;

                    string query = "INSERT INTO services (servCode, servName, servPrice, servDuration, servTypeCode, servActivity) " +
                                   "VALUES (@servCode, 'Новая услуга', 1000, 1, 1, 'да');";

                    MySqlCommand comm = new MySqlCommand(query, conn);
                    comm.Parameters.AddWithValue("@servCode", newCode);

                    int result = comm.ExecuteNonQuery();
                    if (result > 0)
                    {
                        MessageBox.Show("Услуга успешно добавлена!");
                        ShowActiveServices();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении услуги: {ex.Message}");
            }
        }

        private void b_serviceChange_Click(object sender, RoutedEventArgs e)
        {
            var selectedService = serviceCell.SelectedItem as ServicesClass;
            if (selectedService == null)
            {
                MessageBox.Show("Выберите услугу для изменения!");
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    string query = "UPDATE services SET servName = @servName, servPrice = @servPrice, " +
                                   "servDuration = @servDuration, servTypeCode = @servTypeCode WHERE servCode = @servCode;";

                    MySqlCommand comm = new MySqlCommand(query, conn);
                    comm.Parameters.AddWithValue("@servCode", selectedService.ServCode);
                    comm.Parameters.AddWithValue("@servName", selectedService.ServName);
                    comm.Parameters.AddWithValue("@servPrice", selectedService.ServPrice);
                    comm.Parameters.AddWithValue("@servDuration", selectedService.ServDuration);
                    comm.Parameters.AddWithValue("@servTypeCode", selectedService.ServTypeCode);

                    int result = comm.ExecuteNonQuery();
                    if (result > 0)
                    {
                        MessageBox.Show("Данные услуги успешно обновлены!");
                        ShowActiveServices();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при изменении услуги: {ex.Message}");
            }
        }

        private void b_serviceShow_Click(object sender, RoutedEventArgs e)
        {
            serviceCell.CommitEdit(DataGridEditingUnit.Cell, true);
            serviceCell.CommitEdit(DataGridEditingUnit.Row, true);
            serviceCell.UpdateLayout();

            var row = serviceCell.SelectedItem as ServicesClass ?? serviceCell.CurrentItem as ServicesClass;
            var name = row?.ServName?.Trim();

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Выберите услугу или введите данные в таблицу для поиска!");
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    string query = "SELECT * FROM services WHERE servName LIKE @servName AND servActivity = 'да';";
                    MySqlCommand comm = new MySqlCommand(query, conn);
                    comm.Parameters.AddWithValue("@servName", $"%{name}%");

                    MySqlDataReader reader = comm.ExecuteReader();
                    servicesCollection.Clear();
                    while (reader.Read())
                    {
                        servicesCollection.Add(new ServicesClass()
                        {
                            ServCode = reader.GetInt32("servCode"),
                            ServName = reader.GetString("servName"),
                            ServPrice = reader.GetInt32("servPrice"),
                            ServDuration = reader.GetInt32("servDuration"),
                            ServTypeCode = reader.GetInt32("servTypeCode"),
                            ServActivity = reader.GetString("servActivity")
                        });
                    }

                    if (servicesCollection.Count == 0)
                    {
                        MessageBox.Show("Услуги с таким названием не найдены!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при поиске услуги: {ex.Message}");
            }
        }

        private void b_serviceReturn_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }

    public class ServicesClass
    {
        public int ServCode { get; set; }
        public string ServName { get; set; }
        public int ServPrice { get; set; }
        public int ServDuration { get; set; }
        public int ServTypeCode { get; set; }
        public string ServActivity { get; set; }
    }
}