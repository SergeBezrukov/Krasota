using System.Collections.ObjectModel;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.SqlClient;
// using MySqlConnector;
using System.Data;
using System.Net.Sockets;
using System;
using Krasota;
namespace Krasota
{
    /// <summary>
    /// Логика взаимодействия для OpenDB.xaml
    /// </summary>
    public partial class OpenDB : Window
    {
        public OpenDB()
        {
            InitializeComponent();
        }

        private void b_client_Click(object sender, RoutedEventArgs e)
        {
            Clients clientsW = new Clients();
            clientsW.Show();
        }

        private void b_typeService_Click(object sender, RoutedEventArgs e)
        {
            ServTypes servTypesW = new ServTypes();
            servTypesW.Show();
        }

        private void b_master_Click(object sender, RoutedEventArgs e)
        {
            Masters mastersW = new Masters();
            mastersW.Show();
        }

        private void b_service_Click(object sender, RoutedEventArgs e)
        {
            Services servicesW = new Services();
            servicesW.Show();
        }

        private void b_appoint_Click(object sender, RoutedEventArgs e)
        {
            Appointments appointmentsW = new Appointments();
            appointmentsW.Show();
        }

        private void b_returnO_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
        }
    }
}