using System;
using System.Windows;
using System.Windows.Controls;
using System.Data.SqlClient;
using SD = System.Data;


namespace Warehouse
{
    /// <summary>
    /// Логика взаимодействия для Work.xaml
    /// </summary>
    public partial class Work : Window
    {
        static string conStr = @"data source = localhost; initial catalog = Warehouse; integrated security = true; ";

        SqlConnection connect = new SqlConnection(conStr);
        public SD.DataSet ds;

        TextBox[] t = new TextBox[5];
        Label[] l = new Label[5];

        Button[] btn = new Button[2];
        DataGrid g = new DataGrid();

        Menu m = new Menu();

        public Work()
        {
            InitializeComponent();
            
        }



        private void insertGrid(string command)
        {
            try
            {
                connect.Open();
                SD.DataTable dt = new SD.DataTable();
                SqlDataAdapter a = new SqlDataAdapter(command, connect);
                a.Fill(dt);
                grid_tab.ItemsSource = dt.DefaultView;
                connect.Close();
            }
            catch(FormatException)
            {
                MessageBox.Show("Вы ничего не ввели!", "Ошибка!",MessageBoxButton.OKCancel,MessageBoxImage.Error );
                connect.Close();
            }
            catch (SqlException)
            {
                MessageBox.Show("Неверный формат данных!", "Ошибка!", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                connect.Close();
            }
        }

        void Delete_object()
        {
          for (int i = 0; i < t.Length; i++) Panel.Children.Remove(t[i]);
          for (int j = 0; j < l.Length; j++) Panel.Children.Remove(l[j]);
          for (int g = 0; g < btn.Length; g++) Panel.Children.Remove(btn[g]);
          Panel.Children.Remove(m);
          Panel.Children.Remove(g);
          grid_tab.Margin = new Thickness(0, 188, 0, 0);
        }

        private void searchProduct(object sender, RoutedEventArgs e)//поиск продукта
        {
            string command = $"SELECT p.id_prod, name_prod, p.id_provider, price FROM Product as p inner join Price as pr on pr.id_prod=p.id_prod where p.id_prod = '{t[0].Text}' or name_prod = '{t[1].Text}'";
            insertGrid(command);
        }
        private void searchLocation(object sender, RoutedEventArgs e)//поиск продукта
        {
            string command = $"SELECT name_prod,City, Street, Num_house FROM Location as l inner join Prod_loc as pl on pl.id_loc = l.id_loc inner join Product as p on p.id_prod = pl.id_prod where p.id_prod = '{t[0].Text}' or name_prod = '{t[1].Text}'";
            insertGrid(command);
        }

        private void filtr_low(object sender, RoutedEventArgs e)// цена меньше заданной
        {
            string command = $"SELECT * FROM dbo.search_low_price({t[2].Text})";
            insertGrid(command);
        }
        private void filtr_high(object sender, RoutedEventArgs e)// цена больше заданной
        {
            string command = $"SELECT * FROM dbo.search_high_price({t[2].Text})";
            insertGrid(command);
        }

        private void vivod_prod()
        {
            string command = "SELECT p.id_prod, name_prod, p.id_provider, price FROM Product as p inner join Price as pr on pr.id_prod=p.id_prod";
            insertGrid(command);
        }
        private void vivod_order(int id)
        {

            string command = $"SELECT id_order, Surname, Name, Mid_name, City, Street, House, Num_apart, dbo.price_order({id}) as Sum  from ([Order] as o " +
                $"inner join Customer as c on c.id_cust = o.id_customer " +
                $"inner join Loc_cust as lc on lc.id_cust = c.id_cust " +
                $"inner join LocationCust as l on l.id_loczak = lc.id_loczak) where o.id_order = {id} ";
                
            insertGrid(command);
        }

        private void MenuItem1_Click(object sender, RoutedEventArgs e)
        {
            Delete_object();
            vivod_prod();
        }
        private int count(string col, string name)
        {
            int count;
            string command = $"SELECT Max({col}) FROM {name} ";
            SqlCommand cmd = connect.CreateCommand();

            cmd.CommandText = command;

            connect.Open();
            count = Convert.ToInt32(cmd.ExecuteScalar());
            connect.Close();

            return count;
        }

        private void insert_data(object sender, RoutedEventArgs e)
        {
            try
            {
                WarehouseDataTableAdapters.ProductTableAdapter product = new WarehouseDataTableAdapters.ProductTableAdapter();
                product.Insert(count("id_prod", "Product") + 1, t[0].Text, Convert.ToInt32(t[1].Text));

                WarehouseDataTableAdapters.PriceTableAdapter price = new WarehouseDataTableAdapters.PriceTableAdapter();
                price.Insert(count("id_price", "Price") + 1, Convert.ToDecimal(t[2].Text), count("id_prod", "Product"));

                vivod_prod();
            }
            catch (FormatException)
            {
                MessageBox.Show("Входная строка имела неверный формат!", "Ошибка!", MessageBoxButton.OKCancel, MessageBoxImage.Error);
            }
        }

        private void delete_data_prod(object sender, RoutedEventArgs e)
        {
            try
            {
                string command1 = $"DELETE FROM Price WHERE Price.id_prod = { Convert.ToInt32(t[0].Text)} or Price.id_prod = (select Product.id_prod from Product where name_prod = '{t[1].Text}')";
                connect.Open();
                SqlCommand cmd1 = new SqlCommand(command1, connect);
                cmd1.ExecuteScalar();
                connect.Close();
                string command2 = $"DELETE FROM Product WHERE Product.id_prod = {Convert.ToInt32(t[0].Text)} or name_prod = '{t[1].Text}' ";
                connect.Open();
                SqlCommand cmd2 = new SqlCommand(command2, connect);
                cmd2.ExecuteScalar();
                connect.Close();
                vivod_prod();
            }
            catch (FormatException)
            {
                MessageBox.Show("Входная строка имела неверный формат!", "Ошибка!", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                connect.Close();
            }
        }

        private void delete_data_order(object sender, RoutedEventArgs e)
        {
            try
            {
                string command1 = $"DELETE FROM LocationCust where id_loczak = (select LocationCust.id_loczak from LocationCust " +
                    $"inner join Loc_cust on LocationCust.id_loczak = Loc_cust.id_loczak " +
                    $"inner join Customer on Loc_cust.id_cust = Customer.id_cust " +
                    $"inner join [Order] on [Order].id_order = Customer.id_cust " +
                    $"inner join Sotr_order on Sotr_order.id_order =[Order].id_order where [Order].id_order = {Convert.ToInt32(t[0].Text)});" +
                    $"DELETE FROM Loc_cust WHERE Loc_cust.id_cust = (select Loc_cust.id_cust from Loc_cust inner join " +
                    $" Customer on Loc_cust.id_cust = Customer.id_cust " +
                    $"inner join [Order] on [Order].id_customer = Customer.id_cust where [Order].id_order = {Convert.ToInt32(t[0].Text)} );" +
                    $"DELETE FROM Customer WHERE Customer.id_cust = (select id_cust from Customer inner join " +
                    $"[Order] on [Order].id_customer = Customer.id_cust where [Order].id_order = {Convert.ToInt32(t[0].Text)});" +
                    $"DELETE FROM Sotr_order WHERE Sotr_order.id_order = {Convert.ToInt32(t[0].Text)};" +
                    $"DELETE FROM Prod_order WHERE Prod_order.id_order = {Convert.ToInt32(t[0].Text)};" +
                    $"DELETE FROM [Order] WHERE [Order].id_order = {Convert.ToInt32(t[0].Text)};";
                connect.Open();
                SqlCommand cmd1 = new SqlCommand(command1, connect);
                cmd1.ExecuteScalar();
                connect.Close();
            }
            catch(FormatException)
            {
                MessageBox.Show("Входная строка имела неверный формат!", "Ошибка!", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                connect.Close();
            }

        }

        private void search_order(object sender, RoutedEventArgs e)
        {
            try
            {
                vivod_order(Convert.ToInt32(t[0].Text));
            }
            catch (FormatException)
            {
                MessageBox.Show("Входная строка имела неверный формат!", "Ошибка!", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                connect.Close();
            }
        }

        private void search_prod_in_order(object sender, RoutedEventArgs e)
        {
            
            g.Margin = new Thickness(242, 188, 0, 0);
            grid_tab.Margin = new Thickness(0,188, 242,0);
            Panel.Children.Add(g);

            string cmd = $"select p.id_prod, name_prod, price from [Order] as o " +
                $"inner join Prod_order as po on o.id_order = po.id_order " +
                $"inner join Product as p on p.id_prod = po.id_prod " +
                $"inner join Price as pr on pr.id_prod = p.id_prod where o.id_order = {Convert.ToInt32(t[0].Text)}";

            connect.Open();
            SD.DataTable dt = new SD.DataTable();
            SqlDataAdapter a = new SqlDataAdapter(cmd, connect);
            a.Fill(dt);
            g.ItemsSource = dt.DefaultView;
            connect.Close();
        }

        public int[] search_id(string col,string[] name)
        {
            int[] res = new int[name.Length];
            string cmd = string.Empty;

            for (int i = 0; i < res.Length; i++)
            {
                if(col == "id_sotr")
                {

                    string[] fio = name[i].Trim().Split(' ');
                    cmd = $"select id_sotr from Sotridnik where Surname = '{fio[0].Trim()}' and Name = '{fio[1].Trim()}' and Mid_name = '{fio[2].Trim()}'";
                }
                else
                {
                    cmd = $"select id_prod from Product where name_prod = '{name[i].Trim()}'";
                }
               

                connect.Open();
                SqlCommand cmd1 = new SqlCommand(cmd, connect);
                res[i] = Convert.ToInt32(cmd1.ExecuteScalar());
                connect.Close();
            }
            

            return res;
        }
       

        private void insert_data_ord(object sender, RoutedEventArgs e)
        {
            try
            {
                WarehouseDataTableAdapters.LocationCustTableAdapter lc = new WarehouseDataTableAdapters.LocationCustTableAdapter();
                WarehouseDataTableAdapters.Loc_custTableAdapter l_c = new WarehouseDataTableAdapters.Loc_custTableAdapter();
                WarehouseDataTableAdapters.CustomerTableAdapter c = new WarehouseDataTableAdapters.CustomerTableAdapter();
                WarehouseDataTableAdapters.Sotr_orderTableAdapter s = new WarehouseDataTableAdapters.Sotr_orderTableAdapter();
                WarehouseDataTableAdapters.Prod_orderTableAdapter p = new WarehouseDataTableAdapters.Prod_orderTableAdapter();
                WarehouseDataTableAdapters.OrderTableAdapter o = new WarehouseDataTableAdapters.OrderTableAdapter();

                string[] addres = t[1].Text.Trim().Split(',');
                string[] prod = t[2].Text.Trim().Split(',');
                string[] sotr = t[3].Text.Trim().Split(',');
                string[] cust = t[4].Text.Trim().Split(' ');

                lc.Insert(count("id_loczak", "LocationCust") + 1, addres[0].Trim(), addres[1].Trim(), addres[2].Trim(), Convert.ToInt32(addres[3]));
                c.Insert(count("id_cust", "Customer") + 1, cust[0].Trim(), cust[1].Trim(), cust[2].Trim());
                l_c.Insert(count("id_loc_cust", "Loc_cust") + 1, count("id_loczak", "LocationCust"), count("id_cust", "Customer"));
                o.Insert(count("id_order", "[Order]") + 1, count("id_cust", "Customer"));

                int[] id_prod = search_id("id_prod", prod);
                for (int i = 0; i < id_prod.Length; i++) p.Insert(count("id_prod_ord", "Prod_order") + 1, id_prod[i], count("id_order", "[Order]"));

                int[] id_sotr = search_id("id_sotr", sotr);
                for (int i = 0; i < id_sotr.Length; i++) s.Insert(count("id_sotr_ord", "Sotr_order") + 1, id_sotr[i], count("id_order", "[Order]"));

                vivod_order(count("id_order", "[Order]"));
            }
            catch (IndexOutOfRangeException)
            {
                MessageBox.Show("Ничего не введено или неверные данные!", "Ошибка!", MessageBoxButton.OKCancel, MessageBoxImage.Error);
                connect.Close();
            }
        }

        private void MenuItem2_Click(object sender, RoutedEventArgs e)
        {
            Delete_object();

            t[0] = new TextBox();
            t[0].Name = "txt_id";
            t[0].HorizontalAlignment = HorizontalAlignment.Left;
            t[0].Height = 23;
            t[0].Width = 120;
            t[0].VerticalAlignment = VerticalAlignment.Top;
            t[0].Margin = new Thickness(10,54,0,0);
            Panel.Children.Add(t[0]);

            t[1] = new TextBox();
            t[1].Name = "txt_prod";
            t[1].HorizontalAlignment = HorizontalAlignment.Left;
            t[1].Height = 23;
            t[1].Width = 120;
            t[1].VerticalAlignment = VerticalAlignment.Top;
            t[1].Margin = new Thickness(10, 108, 0, 0);
            Panel.Children.Add(t[1]);
            
           

            l[0] = new Label();
            l[0].Name = "lb_id";
            l[0].Content = "ID товара";
            l[0].Height = 27;
            l[0].VerticalAlignment = VerticalAlignment.Top;
            l[0].Margin = new Thickness(10, 27, 0, 0);
            Panel.Children.Add(l[0]);

            l[1] = new Label();
            l[1].Name = "lb_name";
            l[1].Content = "Наименование товара";
            l[1].Height = 27;
            l[1].VerticalAlignment = VerticalAlignment.Top;
            l[1].Margin = new Thickness(10, 82, 0, 0);
            Panel.Children.Add(l[1]);

            btn[0] = new Button();
            btn[0].Name = "search";
            btn[0].Content = "Найти";
            btn[0].HorizontalAlignment = HorizontalAlignment.Left;
            btn[0].Margin = new Thickness(10, 162, 0, 0);
            btn[0].VerticalAlignment = VerticalAlignment.Top;
            btn[0].Width = 75;
            btn[0].Click += searchProduct;
            Panel.Children.Add(btn[0]);

            
            m.Height = 23;
            m.VerticalAlignment = VerticalAlignment.Top;
            m.Margin = new Thickness(324, 54, 10, 0);
            MenuItem mi = new MenuItem();
            mi.Header = "Фильтр";
            m.Items.Add(mi);

            MenuItem[] mi1 = new MenuItem[2];
            mi1[0] = new MenuItem();
            mi1[1] = new MenuItem();
            t[2] = new TextBox();
            t[2].Text = "Введите цену";
            mi1[0].Header = "Цена меньше";
            mi1[0].Click += filtr_low;
            mi1[1].Header = "Цена больше";
            mi1[1].Click += filtr_high;
            mi.Items.Add(mi1[0]);
            mi.Items.Add(mi1[1]);
            mi.Items.Add(t[2]);
            Panel.Children.Add(m);
        }

        private void MenuItem3_Click(object sender, RoutedEventArgs e)
        {
            Delete_object();

            t[0] = new TextBox();
            t[0].Name = "txt_id";
            t[0].HorizontalAlignment = HorizontalAlignment.Left;
            t[0].Height = 23;
            t[0].Width = 120;
            t[0].VerticalAlignment = VerticalAlignment.Top;
            t[0].Margin = new Thickness(10, 54, 0, 0);
            Panel.Children.Add(t[0]);

            t[1] = new TextBox();
            t[1].Name = "txt_prod";
            t[1].HorizontalAlignment = HorizontalAlignment.Left;
            t[1].Height = 23;
            t[1].Width = 120;
            t[1].VerticalAlignment = VerticalAlignment.Top;
            t[1].Margin = new Thickness(10, 108, 0, 0);
            Panel.Children.Add(t[1]);

            l[0] = new Label();
            l[0].Name = "lb_id";
            l[0].Content = "ID товара";
            l[0].Height = 27;
            l[0].VerticalAlignment = VerticalAlignment.Top;
            l[0].Margin = new Thickness(10, 27, 0, 0);
            Panel.Children.Add(l[0]);

            l[1] = new Label();
            l[1].Name = "lb_name";
            l[1].Content = "Наименование товара";
            l[1].Height = 27;
            l[1].VerticalAlignment = VerticalAlignment.Top;
            l[1].Margin = new Thickness(10, 82, 0, 0);
            Panel.Children.Add(l[1]);

            btn[0] = new Button();
            btn[0].Name = "search";
            btn[0].Content = "Найти";
            btn[0].HorizontalAlignment = HorizontalAlignment.Left;
            btn[0].Margin = new Thickness(10, 162, 0, 0);
            btn[0].VerticalAlignment = VerticalAlignment.Top;
            btn[0].Width = 75;
            btn[0].Click += searchLocation;
            Panel.Children.Add(btn[0]);
        }

        private void MenuItem4_Click(object sender, RoutedEventArgs e)
        {
            Delete_object();

            t[0] = new TextBox();
            t[0].Name = "txt_prod";
            t[0].HorizontalAlignment = HorizontalAlignment.Left;
            t[0].Height = 23;
            t[0].Width = 120;
            t[0].VerticalAlignment = VerticalAlignment.Top;
            t[0].Margin = new Thickness(10, 54, 0, 0);
            Panel.Children.Add(t[0]);

            t[1] = new TextBox();
            t[1].Name = "txt_prov";
            t[1].HorizontalAlignment = HorizontalAlignment.Left;
            t[1].Height = 23;
            t[1].Width = 120;
            t[1].VerticalAlignment = VerticalAlignment.Top;
            t[1].Margin = new Thickness(10, 108, 0, 0);
            Panel.Children.Add(t[1]);

            t[2] = new TextBox();
            t[2].Name = "txt_price";
            t[2].HorizontalAlignment = HorizontalAlignment.Left;
            t[2].Height = 23;
            t[2].Width = 120;
            t[2].VerticalAlignment = VerticalAlignment.Top;
            t[2].Margin = new Thickness(10, 162, 0, 0);
            Panel.Children.Add(t[2]);

            l[0] = new Label();
            l[0].Name = "lb_name";
            l[0].Content = "Наименование товара";
            l[0].Height = 27;
            l[0].VerticalAlignment = VerticalAlignment.Top;
            l[0].Margin = new Thickness(10, 27, 0, 0);
            Panel.Children.Add(l[0]);

            l[1] = new Label();
            l[1].Name = "lb_prov";
            l[1].Content = "ID поставщика";
            l[1].Height = 27;
            l[1].VerticalAlignment = VerticalAlignment.Top;
            l[1].Margin = new Thickness(10, 82, 0, 0);
            Panel.Children.Add(l[1]);

            l[2] = new Label();
            l[2].Name = "lb_price";
            l[2].Content = "Цена";
            l[2].Height = 27;
            l[2].VerticalAlignment = VerticalAlignment.Top;
            l[2].Margin = new Thickness(10, 137, 0, 0);
            Panel.Children.Add(l[2]);

            grid_tab.Margin = new Thickness(0, 220, 0, 0);

            btn[0] = new Button();
            btn[0].Name = "insert";
            btn[0].Content = "Добавить";
            btn[0].HorizontalAlignment = HorizontalAlignment.Left;
            btn[0].Margin = new Thickness(10, 192, 0, 0);
            btn[0].VerticalAlignment = VerticalAlignment.Top;
            btn[0].Width = 75;
            btn[0].Click += insert_data;
            Panel.Children.Add(btn[0]);
        }

        private void MenuItem5_Click(object sender, RoutedEventArgs e)
        {
            Delete_object();

            t[0] = new TextBox();
            t[0].Name = "txt_id";
            t[0].HorizontalAlignment = HorizontalAlignment.Left;
            t[0].Height = 23;
            t[0].Width = 120;
            t[0].VerticalAlignment = VerticalAlignment.Top;
            t[0].Margin = new Thickness(10, 54, 0, 0);
            Panel.Children.Add(t[0]);

            t[1] = new TextBox();
            t[1].Name = "txt_prod";
            t[1].HorizontalAlignment = HorizontalAlignment.Left;
            t[1].Height = 23;
            t[1].Width = 120;
            t[1].VerticalAlignment = VerticalAlignment.Top;
            t[1].Margin = new Thickness(10, 108, 0, 0);
            Panel.Children.Add(t[1]);

            l[0] = new Label();
            l[0].Name = "lb_id";
            l[0].Content = "ID товара";
            l[0].Height = 27;
            l[0].VerticalAlignment = VerticalAlignment.Top;
            l[0].Margin = new Thickness(10, 27, 0, 0);
            Panel.Children.Add(l[0]);

            l[1] = new Label();
            l[1].Name = "lb_name";
            l[1].Content = "Наименование товара";
            l[1].Height = 27;
            l[1].VerticalAlignment = VerticalAlignment.Top;
            l[1].Margin = new Thickness(10, 82, 0, 0);
            Panel.Children.Add(l[1]);

            btn[0] = new Button();
            btn[0].Name = "delete";
            btn[0].Content = "Удалить";
            btn[0].HorizontalAlignment = HorizontalAlignment.Left;
            btn[0].Margin = new Thickness(10, 162, 0, 0);
            btn[0].VerticalAlignment = VerticalAlignment.Top;
            btn[0].Width = 75;
            btn[0].Click += delete_data_prod;
            Panel.Children.Add(btn[0]);

        }

        private void MenuItem6_Click(object sender, RoutedEventArgs e)
        {
            Delete_object();

            t[0] = new TextBox();
            t[0].Name = "txt_id";
            t[0].HorizontalAlignment = HorizontalAlignment.Left;
            t[0].Height = 23;
            t[0].Width = 120;
            t[0].VerticalAlignment = VerticalAlignment.Top;
            t[0].Margin = new Thickness(10, 54, 0, 0);
            Panel.Children.Add(t[0]);


            l[0] = new Label();
            l[0].Name = "lb_id";
            l[0].Content = "ID заказа";
            l[0].Height = 27;
            l[0].VerticalAlignment = VerticalAlignment.Top;
            l[0].Margin = new Thickness(10, 27, 0, 0);
            Panel.Children.Add(l[0]);


            btn[0] = new Button();
            btn[0].Name = "search";
            btn[0].Content = "Найти заказ";
            btn[0].HorizontalAlignment = HorizontalAlignment.Left;
            btn[0].Margin = new Thickness(10, 162, 0, 0);
            btn[0].VerticalAlignment = VerticalAlignment.Top;
            btn[0].Width = 75;
            btn[0].Click += search_order;
            Panel.Children.Add(btn[0]);

            btn[1] = new Button();
            btn[1].Name = "s_p";
            btn[1].Content = "Товары заказа";
            btn[1].HorizontalAlignment = HorizontalAlignment.Left;
            btn[1].Margin = new Thickness(100, 162, 0, 0);
            btn[1].VerticalAlignment = VerticalAlignment.Top;
            btn[1].Width = 90;
            btn[1].Click += search_prod_in_order;
            Panel.Children.Add(btn[1]);

        }

        private void MenuItem7_Click(object sender, RoutedEventArgs e)
        {
            Delete_object();

           

            t[1] = new TextBox();
            t[1].Name = "txt_loc";
            t[1].HorizontalAlignment = HorizontalAlignment.Left;
            t[1].Height = 23;
            t[1].Width = 120;
            t[1].VerticalAlignment = VerticalAlignment.Top;
            t[1].Margin = new Thickness(10, 108, 0, 0);
            Panel.Children.Add(t[1]);

            t[2] = new TextBox();
            t[2].Name = "txt_prod";
            t[2].HorizontalAlignment = HorizontalAlignment.Left;
            t[2].Height = 23;
            t[2].Width = 120;
            t[2].VerticalAlignment = VerticalAlignment.Top;
            t[2].Margin = new Thickness(200, 54, 0, 0);
            Panel.Children.Add(t[2]);

            t[3] = new TextBox();
            t[3].Name = "txt_sotr";
            t[3].HorizontalAlignment = HorizontalAlignment.Left;
            t[3].Height = 23;
            t[3].Width = 120;
            t[3].VerticalAlignment = VerticalAlignment.Top;
            t[3].Margin = new Thickness(200, 108, 0, 0);
            Panel.Children.Add(t[3]);

            t[4] = new TextBox();
            t[4].Name = "txt_cust";
            t[4].HorizontalAlignment = HorizontalAlignment.Left;
            t[4].Height = 23;
            t[4].Width = 120;
            t[4].VerticalAlignment = VerticalAlignment.Top;
            t[4].Margin = new Thickness(10, 54, 0, 0);
            Panel.Children.Add(t[4]);

            

            l[1] = new Label();
            l[1].Name = "lb_loc";
            l[1].Content = "Адресс";
            l[1].Height = 27;
            l[1].VerticalAlignment = VerticalAlignment.Top;
            l[1].Margin = new Thickness(10, 82, 0, 0);
            Panel.Children.Add(l[1]);

            l[2] = new Label();
            l[2].Name = "lb_prod";
            l[2].Content = "Товары";
            l[2].Height = 27;
            l[2].VerticalAlignment = VerticalAlignment.Top;
            l[2].Margin = new Thickness(200, 27, 0, 0);
            Panel.Children.Add(l[2]);

            l[3] = new Label();
            l[3].Name = "lb_sotr";
            l[3].Content = "Сотрудники";
            l[3].Height = 27;
            l[3].VerticalAlignment = VerticalAlignment.Top;
            l[3].Margin = new Thickness(200, 82, 0, 0);
            Panel.Children.Add(l[3]);

            l[4] = new Label();
            l[4].Name = "lb_cust";
            l[4].Content = "Заказчик";
            l[4].Height = 27;
            l[4].VerticalAlignment = VerticalAlignment.Top;
            l[4].Margin = new Thickness(10, 27, 0, 0);
            Panel.Children.Add(l[4]);

            grid_tab.Margin = new Thickness(0, 220, 0, 0);

            btn[0] = new Button();
            btn[0].Name = "insert";
            btn[0].Content = "Добавить";
            btn[0].HorizontalAlignment = HorizontalAlignment.Left;
            btn[0].Margin = new Thickness(10, 192, 0, 0);
            btn[0].VerticalAlignment = VerticalAlignment.Top;
            btn[0].Width = 75;
            btn[0].Click += insert_data_ord;
            Panel.Children.Add(btn[0]);
        }

        private void MenuItem8_Click(object sender, RoutedEventArgs e)
        {
            Delete_object();

            t[0] = new TextBox();
            t[0].Name = "txt_id";
            t[0].HorizontalAlignment = HorizontalAlignment.Left;
            t[0].Height = 23;
            t[0].Width = 120;
            t[0].VerticalAlignment = VerticalAlignment.Top;
            t[0].Margin = new Thickness(10, 54, 0, 0);
            Panel.Children.Add(t[0]);

            

            l[0] = new Label();
            l[0].Name = "lb_id";
            l[0].Content = "ID заказа";
            l[0].Height = 27;
            l[0].VerticalAlignment = VerticalAlignment.Top;
            l[0].Margin = new Thickness(10, 27, 0, 0);
            Panel.Children.Add(l[0]);

            

            btn[0] = new Button();
            btn[0].Name = "delete";
            btn[0].Content = "Удалить";
            btn[0].HorizontalAlignment = HorizontalAlignment.Left;
            btn[0].Margin = new Thickness(10, 162, 0, 0);
            btn[0].VerticalAlignment = VerticalAlignment.Top;
            btn[0].Width = 75;
            btn[0].Click += delete_data_order;
            Panel.Children.Add(btn[0]);
        }
    }
}
