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
using System.Windows.Shapes;

namespace ahp
{
    /// <summary>
    /// Interaction logic for WindowIdentify.xaml
    /// </summary>
    public partial class WindowIdentify : Window
    {
        public List<int> listSelectedIdxs = new List<int>();
        List<CheckBox> listCtrls = new List<CheckBox>();
        public WindowIdentify()
        {
            InitializeComponent();
        }

        public WindowIdentify(double[] dbArry)
        {
            InitializeComponent();
            listCtrls.Clear();

            Grid grd = new Grid();
            this.grdItems.Children.Clear();
            this.grdItems.Children.Add(grd);

            for(int i = 0; i < dbArry.Count(); i++)
            {
                RowDefinition rd = new RowDefinition();
                rd.Height = GridLength.Auto;
                grd.RowDefinitions.Add(rd);
            }
           
            for(int i = 0; i < dbArry.Count(); i++)
            {
                CheckBox ckb = new CheckBox();
                ckb.Content = string.Format("{0:0.00}", dbArry[i]);
                ckb.Margin = new Thickness(5, 10, 0, 0);
                grd.Children.Add(ckb);
                Grid.SetRow(ckb, i);
                ckb.HorizontalAlignment = HorizontalAlignment.Center;
                ckb.VerticalAlignment = VerticalAlignment.Center;

                if (listSelectedIdxs.FindIndex(x => x == i) != -1)
                {
                    ckb.IsChecked = true;
                }
                listCtrls.Add(ckb);
            }
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            listSelectedIdxs.Clear();

            for(int i = 0; i < listCtrls.Count(); i++)
            {
                if(listCtrls[i].IsChecked == true)
                {
                    listSelectedIdxs.Add(i);
                }
            }

            this.DialogResult = true;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Height = this.grdItems.ActualHeight + 200;
            this.MaxHeight = Application.Current.MainWindow.ActualHeight;

            CenterWindowOnScreen();
        }

        private void CenterWindowOnScreen()
        {
            double screenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
            double screenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;
            double windowWidth = this.Width;
            double windowHeight = this.Height;
            this.Left = (screenWidth / 2) - (windowWidth / 2);
            this.Top = (screenHeight / 2) - (windowHeight / 2);
        }
    }
}
