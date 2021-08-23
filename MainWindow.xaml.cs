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
using System.Data.OleDb;

namespace KowiVentas
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    public partial class MainWindow : Window
    {
        public List<Venta> items = new List<Venta>();
        public float currTotal = 0;
        public int ventanum = 0;
        static string connStr = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=Kowi.accdb";
        OleDbConnection conn = new OleDbConnection(connStr);
        public MainWindow()
        {
            InitializeComponent();
            LV1.Items.Clear();
            LV1.ItemsSource = items;
            ActualizarVentaNum();
        }

        private void AddToCart(string id)
        {
            int idint = 0;
            if (int.TryParse(id, out idint))
            {
                try
                {
                    conn.Open();
                    string sql = "SELECT Id, descripcion, precio FROM Inventario WHERE Id =" + id;
                    OleDbCommand cmd = new OleDbCommand(sql, conn);
                    OleDbDataReader rdr = cmd.ExecuteReader();

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
                OleDbCommand cmd = new OleDbCommand(sql, conn);
                OleDbDataReader rdr = cmd.ExecuteReader();

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
            foreach (Venta v in items)
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
                AddToCart(idtxt.Text);
            }
            else
            {
                MessageBox.Show("Escriba un código de producto");
            }
            updateAll();
            CambioTxt.Content = "$0.00";
        }

        private float CalcTotal(List<Venta> prods)
        {
            float sum = 0;
            foreach (Venta v in prods)
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
                    RegisVenta();
                    DescontarVenta();
                    LimpiarVenta();
                    ActualizarVentaNum();
                }
                else
                {
                    MessageBox.Show("Escriba una cantidad correcta de dinero entregado");
                }
            }
            else
            {
                RegisVenta();
                DescontarVenta();
                LimpiarVenta();
                ActualizarVentaNum();
            }

        }

        private void DescontarVenta()
        {
            if (ChecarExistencia())
            {
                foreach (Venta v in items)
                {
                    if (v.ID != 0)
                    {
                        int newquan = QuanQuery(v.ID) - v.cant;
                        ModificarCantidad(v.ID, newquan);
                    }
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
            foreach (Venta v in items)
            {
                if (v.ID != 0)
                {
                    suc = v.cant <= QuanQuery(v.ID);
                    if (!suc)
                    {
                        break;
                    }
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
                OleDbCommand cmd = new OleDbCommand(sql, conn);
                OleDbDataReader rdr = cmd.ExecuteReader();
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

        private void BorrselBtn_Click(object sender, RoutedEventArgs e)
        {
            if (LV1.SelectedIndex != -1)
            {
                int selindex = LV1.SelectedIndex;
                items.RemoveAt(selindex);
                updateAll();
            }
        }

        public void updateAll()
        {
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
        }

        private void VSCBtn_Click(object sender, RoutedEventArgs e)
        {
            Window1 VSC = new Window1();
            VSC.Show();
        }

        private void ActualizarVentaNum()
        {
            ventanum = Obtenerventanum();
            VentaNumLab.Content = ventanum;
        }

        private int Obtenerventanum()
        {
            int num = 0;
            try
            {
                conn.Open();
                string sql = "SELECT TOP 1 ventanum FROM Ventas ORDER BY ventanum DESC";
                OleDbCommand cmd = new OleDbCommand(sql, conn);
                OleDbDataReader rdr = cmd.ExecuteReader();

                while (rdr.Read())
                {
                    int idread = int.Parse(rdr[0].ToString());
                    if (rdr[0] != null)
                    {
                        num = int.Parse(rdr[0].ToString());
                    }
                    else
                    {
                        num = 0;
                    }
                }
                rdr.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            conn.Close();
            return num + 1;
        }     

        private void RegisItem(Venta v)
        {
            try
            {
                conn.Open();
                string sql = "INSERT INTO Ventas (ventanum, itemID, descripcion, cantidad, precio, totalventa) VALUES( \' "+ ventanum +" \', \'"+ v.ID + " \', \'" + v.desc +" \', \'" + v.cant + " \', \'" + v.precio + " \', \'" + currTotal+"\'); ";
                OleDbCommand cmd = new OleDbCommand(sql, conn);
                OleDbDataReader rdr = cmd.ExecuteReader();
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

        private void RegisVenta()
        {
            foreach(Venta v in items)
            {
                RegisItem(v);
            }

        }

    }

    public class Venta
    {
        public int ID { get; set; }

        public string desc { get; set; }

        public int cant { get; set; }

        public float precio { get; set; }

        public int ventanum { get; set; }

    }
}


