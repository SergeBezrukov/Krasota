using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using MySqlConnector;
using Krasota;
namespace Krasota
{
    public partial class Masters : Window
    {
        public static string ConnectionString = "server=127.0.0.1;database=saloonBeauty;uid=root;pwd=1234;port=3306;";
        public ObservableCollection<MastersClass> mastersCollection { get; set; }

        public Masters()
        {
            mastersCollection = new ObservableCollection<MastersClass>();
            InitializeComponent();
            masterCell.ItemsSource = mastersCollection;
            ShowActiveMasters();
        }

        private void ShowActiveMasters()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    string query = "SELECT * FROM masters WHERE masterActivity = 'да';";
                    MySqlCommand comm = new MySqlCommand(query, conn);
                    MySqlDataReader reader = comm.ExecuteReader();

                    mastersCollection.Clear();
                    while (reader.Read())
                    {
                        mastersCollection.Add(new MastersClass()
                        {
                            MasterCode = reader.GetInt32("masterCode"),
                            MasterName = reader.GetString("masterName"),
                            MasterTel = reader.GetString("masterTel"),
                            ServTypeCode = reader.GetInt32("servTypeCode"),
                            MasterActivity = reader.GetString("masterActivity")
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке мастеров: {ex.Message}");
            }
        }

        private void b_masterDelete_Click(object sender, RoutedEventArgs e)
        {
            var selectedMaster = masterCell.SelectedItem as MastersClass;
            if (selectedMaster == null)
            {
                MessageBox.Show("Выберите мастера для удаления!");
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    string query = "UPDATE masters SET masterActivity = 'нет' WHERE masterCode = @masterCode;";
                    MySqlCommand comm = new MySqlCommand(query, conn);
                    comm.Parameters.AddWithValue("@masterCode", selectedMaster.MasterCode);

                    int result = comm.ExecuteNonQuery();
                    if (result > 0)
                    {
                        MessageBox.Show("Мастер успешно удален!");
                        ShowActiveMasters();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении мастера: {ex.Message}");
            }
        }

        private void b_masterAdd_Click(object sender, RoutedEventArgs e)
        {
            // Добавление нового мастера с базовыми значениями
            try
            {
                using (MySqlConnection conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();

                    string maxCodeQuery = "SELECT MAX(masterCode) FROM masters;";
                    MySqlCommand maxComm = new MySqlCommand(maxCodeQuery, conn);
                    var maxCode = maxComm.ExecuteScalar();
                    int newCode = (maxCode == DBNull.Value) ? 1 : Convert.ToInt32(maxCode) + 1;

                    string query = "INSERT INTO masters (masterCode, masterName, masterTel, servTypeCode, masterActivity) " +
                                   "VALUES (@masterCode, 'Новый мастер', '0000000000', 1, 'да');";

                    MySqlCommand comm = new MySqlCommand(query, conn);
                    comm.Parameters.AddWithValue("@masterCode", newCode);

                    int result = comm.ExecuteNonQuery();
                    if (result > 0)
                    {
                        MessageBox.Show("Мастер успешно добавлен!");
                        ShowActiveMasters();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении мастера: {ex.Message}");
            }
        }

        private void b_masterChange_Click(object sender, RoutedEventArgs e)
        {
            var selectedMaster = masterCell.SelectedItem as MastersClass;
            if (selectedMaster == null)
            {
                MessageBox.Show("Выберите мастера для изменения!");
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    string query = "UPDATE masters SET masterName = @masterName, masterTel = @masterTel, servTypeCode = @servTypeCode " +
                                   "WHERE masterCode = @masterCode;";

                    MySqlCommand comm = new MySqlCommand(query, conn);
                    comm.Parameters.AddWithValue("@masterCode", selectedMaster.MasterCode);
                    comm.Parameters.AddWithValue("@masterName", selectedMaster.MasterName);
                    comm.Parameters.AddWithValue("@masterTel", selectedMaster.MasterTel);
                    comm.Parameters.AddWithValue("@servTypeCode", selectedMaster.ServTypeCode);

                    int result = comm.ExecuteNonQuery();
                    if (result > 0)
                    {
                        MessageBox.Show("Данные мастера успешно обновлены!");
                        ShowActiveMasters();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при изменении мастера: {ex.Message}");
            }
        }

        private void b_masterShow_Click(object sender, RoutedEventArgs e)
        {
            masterCell.CommitEdit(DataGridEditingUnit.Cell, true);
            masterCell.CommitEdit(DataGridEditingUnit.Row, true);
            masterCell.UpdateLayout();

            var row = masterCell.SelectedItem as MastersClass ?? masterCell.CurrentItem as MastersClass;
            var tel = row?.MasterTel?.Trim();

            if (string.IsNullOrEmpty(tel))
            {
                MessageBox.Show("Выберите мастера или введите данные в таблицу для поиска!");
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    string query = "SELECT * FROM masters WHERE masterTel LIKE @masterTel AND masterActivity = 'да';";
                    MySqlCommand comm = new MySqlCommand(query, conn);
                    comm.Parameters.AddWithValue("@masterTel", $"%{tel}%");

                    MySqlDataReader reader = comm.ExecuteReader();
                    mastersCollection.Clear();
                    while (reader.Read())
                    {
                        mastersCollection.Add(new MastersClass()
                        {
                            MasterCode = reader.GetInt32("masterCode"),
                            MasterName = reader.GetString("masterName"),
                            MasterTel = reader.GetString("masterTel"),
                            ServTypeCode = reader.GetInt32("servTypeCode"),
                            MasterActivity = reader.GetString("masterActivity")
                        });
                    }

                    if (mastersCollection.Count == 0)
                    {
                        MessageBox.Show("Мастера с таким номером телефона не найдены!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при поиске мастера: {ex.Message}");
            }
        }

        private void b_masterReturn_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }

    public class MastersClass
    {
        public int MasterCode { get; set; }
        public string MasterName { get; set; }
        public string MasterTel { get; set; }
        public int ServTypeCode { get; set; }
        public string MasterActivity { get; set; }
    }
}