using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using MySql.Data;
using MySql.Data.MySqlClient;

namespace KowiVentas
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<Venta> items = new List<Venta>();
        float currTotal = 0;
        static string connStr = "server=localhost;user=root;database=inventario;port=3306;password=m74l75r99d04";
        MySqlConnection conn = new MySqlConnection(connStr);
        public MainWindow()
        {
            InitializeComponent();
            LV1.Items.Clear();
            LV1.ItemsSource = items;
      
        }

        private void ItemQuery(string id)
        {
            int idint = 0;
            if (int.TryParse(id, out idint))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT ID, Description, Precio FROM Inventario WHERE ID=" + id;
                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    MySqlDataReader rdr = cmd.ExecuteReader();

                    while (rdr.Read())
                    {
                        int idread = int.Parse(rdr[0].ToString());
                        if (rdr[0] != null)
                        {
                            if (idcheck(idread))
                            {
                                items.Find(v => v.ID == idread).cant += 1;
                            }
                            else
                            {
                                items.Add(new Venta() { ID = idread, desc = rdr[1].ToString(), cant = 1, precio = float.Parse(rdr[2].ToString()) });
                            }
                        }
                        else
                        {
                            MessageBox.Show("No existe un producto con este código");
                        }
                    }

                    rdr.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                conn.Close();
            }
            else
            {
                MessageBox.Show("Introduzca un código válido");
            }            
        }

        private int QuanQuery(int id)
        {
            int canti = 0;
            try
            {
                conn.Open();
                string sql = "SELECT Cantidad FROM Inventario WHERE ID= " + id;
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    int idread = int.Parse(rdr[0].ToString());
                    if (rdr[0] != null)
                    {
                        canti = int.Parse(rdr[0].ToString());
                    }
                    else
                    {
                        MessageBox.Show("No existe un producto con este código");
                    }
                }
                rdr.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            conn.Close();
            return canti;
        }

        private bool idcheck(int id)
        {
            bool check = false;
            foreach(Venta v in items)
            {
                check = v.ID == id;
                if (check) { break; }
            }

            return check;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {                   
            
            if (idtxt.Text != "")
            {
                ItemQuery(idtxt.Text);
            }
            else
            {
                MessageBox.Show("Escriba un código de producto");
            }
            LV1.Items.Refresh();
            currTotal = CalcTotal(items);
            if (currTotal == 0)
            {
                TotLab.Content = "$0.00";
            }
            else
            {
                TotLab.Content = "$" + string.Format("{0:#.00}", currTotal);
            }          
            CambioTxt.Content = "$0.00";
        }

        private float CalcTotal(List<Venta> prods)
        {
            float sum = 0;
            foreach(Venta v in prods)
            {
                sum += v.cant * v.precio;
            }
            return sum;
        }

        private void CobrarBtn_Click(object sender, RoutedEventArgs e)
        {
            if (EntreTxt.Text != "")
            {
                float ent = 0;
                if (float.TryParse(EntreTxt.Text, out ent))
                {
                    CambioTxt.Content = "$" + string.Format("{0:#.00}", ent - currTotal);
                    DescontarVenta();
                    LimpiarVenta();
                }
                else
                {
                    MessageBox.Show("Escriba una cantidad correcta de dinero entregado");
                }
            }
            else
            {
                DescontarVenta();
                LimpiarVenta();
            }

        }

        private void DescontarVenta()
        {
            if (ChecarExistencia())
            {
                foreach (Venta v in items)
                {
                    int newquan = QuanQuery(v.ID) - v.cant;
                    ModificarCantidad(v.ID, newquan);                
                }
                MessageBox.Show("Venta realizada");
            }
            else
            {
                MessageBox.Show("No se puede realizar la venta, productos insuficientes");
            }
        }

        private bool ChecarExistencia()
        {
            bool suc = true;
            foreach(Venta v in items)
            {
                suc = v.cant <= QuanQuery(v.ID);
                if (!suc){
                    break;
                }
            }

            return suc;
        }

        private void ModificarCantidad(int id, int quan)
        {
            try
            {
                conn.Open();
                string sql = "UPDATE inventario SET Cantidad = " + quan + " WHERE ID = " + id;
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                MySqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                }
                rdr.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            conn.Close();
        }



        private void LimpiarVenta()
        {
            items.Clear();
            LV1.Items.Refresh();
            currTotal = 0;
            TotLab.Content = "$0.00";
        }

        private void BorrventaBtn_Click(object sender, RoutedEventArgs e)
        {
            LimpiarVenta();
        }

        private void BuscarBtn_Click(object sender, RoutedEventArgs e)
        {
        }
    }

    public class Venta
    {
        public int ID { get; set; }

        public string desc { get; set; }

        public int cant { get; set; }

        public float precio { get; set; }

    }

    public class Existencia
    {
        public int ID { get; set; }
        public int cant { get; set; }

    }


}
