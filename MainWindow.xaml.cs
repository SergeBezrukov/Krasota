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
using Krasota;
namespace Krasota
{
    
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void b_db_Click(object sender, RoutedEventArgs e)
        {
           OpenDB openDB = new OpenDB(); 
            openDB.Show();
        }

        private void b_work_Click(object sender, RoutedEventArgs e)
        {
          EveryDay everyDay = new EveryDay();
            everyDay.Show();    
        }

        private void b_exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}