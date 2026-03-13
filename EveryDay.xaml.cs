using System;
using System.Windows;
using System.Windows.Controls;
using MySqlConnector;
using System.Collections.ObjectModel;
using System.Linq;
using System.Data;
using Krasota;
namespace Krasota
{
    public partial class EveryDay : Window
    {
        public static string ConnectionString = "server=127.0.0.1;database=SaloonBeauty;uid=root;pwd=1234;port=3306;";

        private ObservableCollection<Client> clients;
        private ObservableCollection<Master> masters;
        private ObservableCollection<Service> services;
        private ObservableCollection<BusyTime> busyTimes;

        public EveryDay()
        {
            InitializeComponent();
            LoadData();
            InitializeTimeSlots();
            dpRecordDate.SelectedDate = DateTime.Today;
        }

        // Классы для данных
        public class Client
        {
            public int ClientCode { get; set; }
            public string ClientName { get; set; }
        }

        public class Master
        {
            public int MasterCode { get; set; }
            public string MasterName { get; set; }
        }

        public class Service
        {
            public int ServCode { get; set; }
            public string ServName { get; set; }
            public int ServPrice { get; set; }
            public int ServDuration { get; set; }
        }

        public class BusyTime
        {
            public string BusyTimeInfo { get; set; }
        }

        private void LoadData()
        {
            try
            {
                // Загрузка клиентов
                clients = new ObservableCollection<Client>();
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    var cmd = new MySqlCommand("SELECT clientCode, clientName FROM clients WHERE ClientActivity = 'Да'", conn);
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        clients.Add(new Client
                        {
                            ClientCode = reader.GetInt32("clientCode"),
                            ClientName = reader.GetString("clientName")
                        });
                    }
                }
                cmbClients.ItemsSource = clients;

                // Загрузка мастеров
                masters = new ObservableCollection<Master>();
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    var cmd = new MySqlCommand("SELECT masterCode, masterName FROM masters WHERE masterActivity = 'Да'", conn);
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        masters.Add(new Master
                        {
                            MasterCode = reader.GetInt32("masterCode"),
                            MasterName = reader.GetString("masterName")
                        });
                    }
                }
                cmbMasters.ItemsSource = masters;

                // Загрузка услуг
                services = new ObservableCollection<Service>();
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    var cmd = new MySqlCommand("SELECT servCode, servName, servPrice, servDuration FROM services WHERE servActivity = 'Да'", conn);
                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        services.Add(new Service
                        {
                            ServCode = reader.GetInt32("servCode"),
                            ServName = reader.GetString("servName"),
                            ServPrice = reader.GetInt32("servPrice"),
                            ServDuration = reader.GetInt32("servDuration")
                        });
                    }
                }
                cmbServices.ItemsSource = services;

                busyTimes = new ObservableCollection<BusyTime>();
                lstBusyTime.ItemsSource = busyTimes;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void InitializeTimeSlots()
        {
            cmbTime.Items.Clear();
            // Рабочее время с 9:00 до 19:00 с интервалом 30 минут
            for (int hour = 9; hour < 19; hour++)
            {
                for (int minute = 0; minute < 60; minute += 30)
                {
                    cmbTime.Items.Add($"{hour:D2}:{minute:D2}");
                }
            }
        }

        private void cmbServices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbServices.SelectedItem is Service selectedService)
            {
                txtServicePrice.Text = $"{selectedService.ServPrice} руб";
                txtServiceDuration.Text = $"{selectedService.ServDuration} мин";
                UpdateEndTime();
            }
        }

        private void dpRecordDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            LoadMasterBusyTime();
        }

        private void cmbTime_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateEndTime();
        }

        private void UpdateEndTime()
        {
            if (cmbTime.SelectedItem != null && cmbServices.SelectedItem is Service selectedService)
            {
                var startTime = TimeSpan.Parse(cmbTime.SelectedItem.ToString());
                var endTime = startTime.Add(TimeSpan.FromMinutes(selectedService.ServDuration));
                txtEndTime.Text = $"{endTime:hh\\:mm}";
            }
        }

        private void LoadMasterBusyTime()
        {
            if (cmbMasters.SelectedItem == null || dpRecordDate.SelectedDate == null)
                return;

            try
            {
                busyTimes.Clear();
                var master = cmbMasters.SelectedItem as Master;
                var selectedDate = dpRecordDate.SelectedDate.Value.ToString("yyyy-MM-dd");

                using (var conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    var cmd = new MySqlCommand(
                        "SELECT a.queueFrom, a.queueTo, s.servName, c.clientName " +
                        "FROM appointments a " +
                        "JOIN services s ON a.servCode = s.servCode " +
                        "JOIN clients c ON a.clientCode = c.clientCode " +
                        "WHERE a.masterCode = @masterCode AND a.appDate = @appDate " +
                        "ORDER BY a.queueFrom", conn);

                    cmd.Parameters.AddWithValue("@masterCode", master.MasterCode);
                    cmd.Parameters.AddWithValue("@appDate", selectedDate);

                    var reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        var fromTime = reader.GetTimeSpan("queueFrom").ToString(@"hh\:mm");
                        var toTime = reader.GetTimeSpan("queueTo").ToString(@"hh\:mm");
                        var service = reader.GetString("servName");
                        var client = reader.GetString("clientName");

                        busyTimes.Add(new BusyTime
                        {
                            BusyTimeInfo = $"{fromTime} - {toTime}: {service} ({client})"
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки занятого времени: {ex.Message}");
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // Проверка заполнения всех полей
            if (cmbClients.SelectedItem == null)
            {
                ShowMessage("Выберите клиента!", false);
                return;
            }

            if (cmbMasters.SelectedItem == null)
            {
                ShowMessage("Выберите мастера!", false);
                return;
            }

            if (cmbServices.SelectedItem == null)
            {
                ShowMessage("Выберите услугу!", false);
                return;
            }

            if (dpRecordDate.SelectedDate == null)
            {
                ShowMessage("Выберите дату записи!", false);
                return;
            }

            if (cmbTime.SelectedItem == null)
            {
                ShowMessage("Выберите время начала!", false);
                return;
            }

            try
            {
                var client = cmbClients.SelectedItem as Client;
                var master = cmbMasters.SelectedItem as Master;
                var service = cmbServices.SelectedItem as Service;
                var startTime = TimeSpan.Parse(cmbTime.SelectedItem.ToString());
                var endTime = startTime.Add(TimeSpan.FromMinutes(service.ServDuration));

                // Проверка на пересечение времени
                if (IsTimeOverlap(master.MasterCode, dpRecordDate.SelectedDate.Value, startTime, endTime))
                {
                    ShowMessage("Выбранное время пересекается с существующей записью!", false);
                    return;
                }

                using (var conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();

                    // Вставляем новую запись
                    string query = @"INSERT INTO appointments (masterCode, clientCode, servCode, appDate, queueFrom, queueTo) 
                                   VALUES (@masterCode, @clientCode, @servCode, @appDate, @queueFrom, @queueTo);";

                    var cmd = new MySqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@masterCode", master.MasterCode);
                    cmd.Parameters.AddWithValue("@clientCode", client.ClientCode);
                    cmd.Parameters.AddWithValue("@servCode", service.ServCode);
                    cmd.Parameters.AddWithValue("@appDate", dpRecordDate.SelectedDate.Value.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@queueFrom", startTime.ToString());
                    cmd.Parameters.AddWithValue("@queueTo", endTime.ToString());

                    int result = cmd.ExecuteNonQuery();
                    if (result > 0)
                    {
                        ShowMessage($"Запись успешно создана!\n" +
                                  $"Клиент: {client.ClientName}\n" +
                                  $"Мастер: {master.MasterName}\n" +
                                  $"Услуга: {service.ServName}\n" +
                                  $"Дата: {dpRecordDate.SelectedDate.Value:dd.MM.yyyy}\n" +
                                  $"Время: {startTime:hh\\:mm} - {endTime:hh\\:mm}", true);
                        ClearFields();
                        LoadMasterBusyTime();
                    }
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Ошибка при создании записи: {ex.Message}", false);
            }
        }

        private bool IsTimeOverlap(int masterCode, DateTime date, TimeSpan startTime, TimeSpan endTime)
        {
            try
            {
                using (var conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    var cmd = new MySqlCommand(
                        "SELECT COUNT(*) FROM appointments " +
                        "WHERE masterCode = @masterCode AND appDate = @appDate " +
                        "AND ((queueFrom < @endTime AND queueTo > @startTime))", conn);

                    cmd.Parameters.AddWithValue("@masterCode", masterCode);
                    cmd.Parameters.AddWithValue("@appDate", date.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@startTime", startTime.ToString());
                    cmd.Parameters.AddWithValue("@endTime", endTime.ToString());

                    long count = (long)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
            catch
            {
                return true; // В случае ошибки считаем, что есть пересечение
            }
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearFields();
            ShowMessage("Поля очищены", true);
        }

        private void btnReturn_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }

        private void ClearFields()
        {
            cmbClients.SelectedIndex = -1;
            cmbMasters.SelectedIndex = -1;
            cmbServices.SelectedIndex = -1;
            dpRecordDate.SelectedDate = DateTime.Today;
            cmbTime.SelectedIndex = -1;
            txtServicePrice.Text = "0 руб";
            txtServiceDuration.Text = "0 мин";
            txtEndTime.Text = "--:--";
            busyTimes.Clear();
        }

        private void ShowMessage(string message, bool isSuccess)
        {
            txtMessage.Text = message;
            txtMessage.Foreground = isSuccess ? System.Windows.Media.Brushes.Green : System.Windows.Media.Brushes.Red;
        }
    }
}