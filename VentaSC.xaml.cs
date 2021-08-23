using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace KowiVentas
{
    /// <summary>
    /// Lógica de interacción para Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
        }

        private void AgregarBtn_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mnw = App.Current.MainWindow as KowiVentas.MainWindow;
            int c = 0;
            float p = 0;

            if(int.TryParse(canttxt.Text, out c) && float.TryParse(pretxt.Text, out p))
            {
                mnw.items.Add(new Venta() { ID = 0, desc = desctxt.Text, cant = c, precio = p });
                mnw.updateAll();
                this.Close();
            }
            else
            {
                MessageBox.Show("Cantidad o precio inválidos");
            }
            
        }

        private void CancelarBtn_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
