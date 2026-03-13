using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using MySqlConnector;

namespace Krasota
{
    public partial class Appointments : Window
    {
        public static string ConnectionString = "server=127.0.0.1;database=saloonBeauty;uid=root;pwd=1234;port=3306;";
        public ObservableCollection<AppointmentsClass> appointmentsCollection { get; set; }

        public Appointments()
        {
            appointmentsCollection = new ObservableCollection<AppointmentsClass>();
            InitializeComponent();
            appointmentCell.ItemsSource = appointmentsCollection;
            ShowAllAppointments();
        }

        private void ShowAllAppointments()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    string query = "SELECT * FROM appointments;";
                    MySqlCommand comm = new MySqlCommand(query, conn);
                    MySqlDataReader reader = comm.ExecuteReader();

                    appointmentsCollection.Clear();
                    while (reader.Read())
                    {
                        appointmentsCollection.Add(new AppointmentsClass()
                        {
                            AppCode = reader.GetInt32("appCode"),
                            MasterCode = reader.GetInt32("masterCode"),
                            ClientCode = reader.GetInt32("clientCode"),
                            ServTypeCode = reader.GetInt32("servTypeCode"),
                            ServCode = reader.GetInt32("servCode"),
                            QueueFrom = reader.GetInt32("queueFrom"),
                            QueueTo = reader.GetInt32("queueTo"),
                            AppDate = reader.GetDateTime("appDate").ToString("yyyy-MM-dd")
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке записей: {ex.Message}");
            }
        }

        private void b_appointmentDelete_Click(object sender, RoutedEventArgs e)
        {
            var selectedAppointment = appointmentCell.SelectedItem as AppointmentsClass;
            if (selectedAppointment == null)
            {
                MessageBox.Show("Выберите запись для удаления!");
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    string query = "DELETE FROM appointments WHERE appCode = @appCode;";
                    MySqlCommand comm = new MySqlCommand(query, conn);
                    comm.Parameters.AddWithValue("@appCode", selectedAppointment.AppCode);

                    int result = comm.ExecuteNonQuery();
                    if (result > 0)
                    {
                        MessageBox.Show("Запись успешно удалена!");
                        ShowAllAppointments();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении записи: {ex.Message}");
            }
        }

        private void b_appointmentAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();

                    string maxCodeQuery = "SELECT MAX(appCode) FROM appointments;";
                    MySqlCommand maxComm = new MySqlCommand(maxCodeQuery, conn);
                    var maxCode = maxComm.ExecuteScalar();
                    int newCode = (maxCode == DBNull.Value) ? 1 : Convert.ToInt32(maxCode) + 1;

                    string query = "INSERT INTO appointments (appCode, masterCode, clientCode, servTypeCode, servCode, queueFrom, queueTo, appDate) " +
                                   "VALUES (@appCode, 1, 1, 1, 1, 1, 2, '2025-01-01');";

                    MySqlCommand comm = new MySqlCommand(query, conn);
                    comm.Parameters.AddWithValue("@appCode", newCode);

                    int result = comm.ExecuteNonQuery();
                    if (result > 0)
                    {
                        MessageBox.Show("Запись успешно добавлена!");
                        ShowAllAppointments();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении записи: {ex.Message}");
            }
        }

        private void b_appointmentChange_Click(object sender, RoutedEventArgs e)
        {
            var selectedAppointment = appointmentCell.SelectedItem as AppointmentsClass;
            if (selectedAppointment == null)
            {
                MessageBox.Show("Выберите запись для изменения!");
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    string query = "UPDATE appointments SET masterCode = @masterCode, clientCode = @clientCode, " +
                                   "servTypeCode = @servTypeCode, servCode = @servCode, queueFrom = @queueFrom, " +
                                   "queueTo = @queueTo, appDate = @appDate WHERE appCode = @appCode;";

                    MySqlCommand comm = new MySqlCommand(query, conn);
                    comm.Parameters.AddWithValue("@appCode", selectedAppointment.AppCode);
                    comm.Parameters.AddWithValue("@masterCode", selectedAppointment.MasterCode);
                    comm.Parameters.AddWithValue("@clientCode", selectedAppointment.ClientCode);
                    comm.Parameters.AddWithValue("@servTypeCode", selectedAppointment.ServTypeCode);
                    comm.Parameters.AddWithValue("@servCode", selectedAppointment.ServCode);
                    comm.Parameters.AddWithValue("@queueFrom", selectedAppointment.QueueFrom);
                    comm.Parameters.AddWithValue("@queueTo", selectedAppointment.QueueTo);
                    comm.Parameters.AddWithValue("@appDate", selectedAppointment.AppDate);

                    int result = comm.ExecuteNonQuery();
                    if (result > 0)
                    {
                        MessageBox.Show("Данные записи успешно обновлены!");
                        ShowAllAppointments();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при изменении записи: {ex.Message}");
            }
        }

        private void b_appointmentShow_Click(object sender, RoutedEventArgs e)
        {
            appointmentCell.CommitEdit(DataGridEditingUnit.Cell, true);
            appointmentCell.CommitEdit(DataGridEditingUnit.Row, true);
            appointmentCell.UpdateLayout();

            var row = appointmentCell.SelectedItem as AppointmentsClass ?? appointmentCell.CurrentItem as AppointmentsClass;
            var masterCode = row?.MasterCode.ToString();

            if (string.IsNullOrEmpty(masterCode))
            {
                MessageBox.Show("Выберите запись для поиска!");
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    string query = "SELECT * FROM appointments WHERE masterCode = @masterCode;";
                    MySqlCommand comm = new MySqlCommand(query, conn);
                    comm.Parameters.AddWithValue("@masterCode", Convert.ToInt32(masterCode));

                    MySqlDataReader reader = comm.ExecuteReader();
                    appointmentsCollection.Clear();
                    while (reader.Read())
                    {
                        appointmentsCollection.Add(new AppointmentsClass()
                        {
                            AppCode = reader.GetInt32("appCode"),
                            MasterCode = reader.GetInt32("masterCode"),
                            ClientCode = reader.GetInt32("clientCode"),
                            ServTypeCode = reader.GetInt32("servTypeCode"),
                            ServCode = reader.GetInt32("servCode"),
                            QueueFrom = reader.GetInt32("queueFrom"),
                            QueueTo = reader.GetInt32("queueTo"),
                            AppDate = reader.GetDateTime("appDate").ToString("yyyy-MM-dd")
                        });
                    }

                    if (appointmentsCollection.Count == 0)
                    {
                        MessageBox.Show("Записи с таким кодом мастера не найдены!");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при поиске записей: {ex.Message}");
            }
        }

        private void b_appointmentReturn_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }

    public class AppointmentsClass
    {
        public int AppCode { get; set; }
        public int MasterCode { get; set; }
        public int ClientCode { get; set; }
        public int ServTypeCode { get; set; }
        public int ServCode { get; set; }
        public int QueueFrom { get; set; }
        public int QueueTo { get; set; }
        public string AppDate { get; set; }
    }
}