using System;
using System.Data.SqlClient;
using System.Windows;





namespace Warehouse
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
        }
        int result;
        public int Result
        {
            get { return result; }
            set { result = value; }
        }
        public void login(object sender, RoutedEventArgs e)
        {
            try
            {
                int login = Convert.ToInt32(txt_login.Text);
                string password = txt_password.Password;

                string search = $"select * from Sotridnik where id_sotr = {login} and Password = '{password}'";


                string conStr = @"data source = localhost; initial catalog = Warehouse; integrated security = true;";

                SqlConnection connect = new SqlConnection(conStr);

                SqlCommand cmd = new SqlCommand();
                cmd.Connection = connect;
                connect.Open();
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.CommandText = search;

                Result = Convert.ToInt32(cmd.ExecuteScalar());

                if (Result != 0)
                {

                    Work aboba = new Work();

                    aboba.Show();
                    Close();

                }
                else
                {
                    MessageBox.Show("Неверный логин или пароль!", "Ошибка!", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                }
            }
            catch(FormatException)
            {
                MessageBox.Show("Вы ничего не ввели!", "Ошибка!", MessageBoxButton.OKCancel, MessageBoxImage.Error);
            }
            

        }
    }
}
