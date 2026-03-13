using MySqlConnector;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Krasota
{
    public partial class Clients : Window
    {
        public static string ConnectionString = "server=127.0.0.1;database=saloonBeauty;uid=root;pwd=NO;port=3306;";
        public ObservableCollection<ClientsClass> clientsCollection { get; set; }

        public Clients()
        {
            clientsCollection = new ObservableCollection<ClientsClass>();
            InitializeComponent();
            clientCell.ItemsSource = clientsCollection;
            ShowActiveClients(); // Показываем только активных клиентов при загрузке
        }

        // Показать только активных клиентов
        private void ShowActiveClients()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(ConnectionString))
                {
                        conn.Open();
                        string query = "SELECT * FROM clients WHERE clientActivity = 'да';";
                    MySqlCommand comm = new MySqlCommand(query, conn);
                    MySqlDataReader reader = comm.ExecuteReader();

                    clientsCollection.Clear();
                    while (reader.Read())
                    {
                        clientsCollection.Add(new ClientsClass()
                        {
                            ClientCode = reader.GetInt32("clientCode"),
                            ClientName = reader.GetString("clientName"),
                            ClientTel = reader.GetString("clientTel"),
                            ClientActivity = reader.GetString("clientActivity")
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке клиентов: {ex.Message}");
            }
        }

        // Показать всех клиентов (включая неактивных)
        private void ShowAllClients()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    string query = "SELECT * FROM clients;";
                    MySqlCommand comm = new MySqlCommand(query, conn);
                    MySqlDataReader reader = comm.ExecuteReader();

                    clientsCollection.Clear();
                    while (reader.Read())
                    {
                        clientsCollection.Add(new ClientsClass()
                        {
                            ClientCode = reader.GetInt32("clientCode"),
                            ClientName = reader.GetString("clientName"),
                            ClientTel = reader.GetString("clientTel"),
                            ClientActivity = reader.GetString("clientActivity")
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке клиентов: {ex.Message}");
            }
        }

        // "Удаление" клиента (изменение активности на "нет")
        private void b_clientDelete_Click(object sender, RoutedEventArgs e)
        {
            var selectedClient = clientCell.SelectedItem as ClientsClass;
            if (selectedClient == null)
            {
                MessageBox.Show("Выберите клиента для удаления!");
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    string query = "UPDATE clients SET clientActivity = 'нет' WHERE clientCode = @clientCode;";
                    MySqlCommand comm = new MySqlCommand(query, conn);
                    comm.Parameters.AddWithValue("@clientCode", selectedClient.ClientCode);

                    int result = comm.ExecuteNonQuery();
                    if (result > 0)
                    {
                        MessageBox.Show("Клиент успешно удален!");
                        ShowActiveClients(); // Обновляем список - удаленный клиент исчезнет
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении клиента: {ex.Message}");
            }
        }

        // Добавление нового клиента
        private void b_clientAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();

                    // Получаем максимальный код клиента для генерации нового
                    string maxCodeQuery = "SELECT MAX(clientCode) FROM clients;";
                    MySqlCommand maxComm = new MySqlCommand(maxCodeQuery, conn);
                    var maxCode = maxComm.ExecuteScalar();
                    int newCode = (maxCode == DBNull.Value) ? 1 : Convert.ToInt32(maxCode) + 1;

                    string query = "INSERT INTO clients (clientCode, clientName, clientTel, clientActivity) " +
                                   "VALUES (@clientCode, @clientName, @clientTel, 'да');";

                    MySqlCommand comm = new MySqlCommand(query, conn);
                    comm.Parameters.AddWithValue("@clientCode", newCode);
                    comm.Parameters.AddWithValue("@clientName", "Новый клиент"); // Можно заменить на ввод из текстовых полей
                    comm.Parameters.AddWithValue("@clientTel", "0000000000");  // Можно заменить на ввод из текстовых полей
                    comm.Parameters.AddWithValue("@clientActivity", "да");
                    int result = comm.ExecuteNonQuery();
                    if (result > 0)
                    {
                        MessageBox.Show("Клиент успешно добавлен!");
                        ShowActiveClients();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении клиента: {ex.Message}");
            }
        }

        // Изменение клиента
        private void b_clientChange_Click(object sender, RoutedEventArgs e)
        {
            var selectedClient = clientCell.SelectedItem as ClientsClass;
            if (selectedClient == null)
            {
                MessageBox.Show("Выберите клиента для изменения!");
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    string query = "UPDATE clients SET clientName = @clientName, clientTel = @clientTel " +
                                   "WHERE clientCode = @clientCode;";

                    MySqlCommand comm = new MySqlCommand(query, conn);
                    comm.Parameters.AddWithValue("@clientCode", selectedClient.ClientCode);
                    comm.Parameters.AddWithValue("@clientName", selectedClient.ClientName);
                    comm.Parameters.AddWithValue("@clientTel", selectedClient.ClientTel);

                    int result = comm.ExecuteNonQuery();
                    if (result > 0)
                    {
                        MessageBox.Show("Данные клиента успешно обновлены!");
                        ShowActiveClients();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при изменении клиента: {ex.Message}");
            }
        }

        // Поиск клиента по телефону
        private void b_clientShow_Click(object sender, RoutedEventArgs e)
        {
            clientCell.CommitEdit(DataGridEditingUnit.Cell, true);
            clientCell.CommitEdit(DataGridEditingUnit.Row, true);
            clientCell.UpdateLayout();

            var row = clientCell.SelectedItem as ClientsClass ?? clientCell.CurrentItem as ClientsClass;
            var tel = row?.ClientTel?.Trim();

            if (string.IsNullOrEmpty(tel))
            {
                MessageBox.Show("Введите номер телефона для поиска!");
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    string query = "SELECT * FROM clients WHERE clientTel LIKE @clientTel AND clientActivity = 'да';";
                    MySqlCommand comm = new MySqlCommand(query, conn);
                    comm.Parameters.AddWithValue("@clientTel", $"%{tel}%");

                    MySqlDataReader reader = comm.ExecuteReader();
                    clientsCollection.Clear();
                    while (reader.Read())
                    {
                        clientsCollection.Add(new ClientsClass()
                        {
                            ClientCode = reader.GetInt32("clientCode"),
                            ClientName = reader.GetString("clientName"),
                            ClientTel = reader.GetString("clientTel"),
                            ClientActivity = reader.GetString("clientActivity")
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при поиске клиента: {ex.Message}");
            }
        }

        // Кнопка "Показать всех"
       

        // Кнопка "Показать активных"
      

        private void b_clientReturn_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }

    public class ClientsClass
    {
        public int ClientCode { get; set; }
        public string ClientName { get; set; }
        public string ClientTel { get; set; }
        public string ClientActivity { get; set; }
    }
}