using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Controls;
namespace ahp
{
    static class MyMessageBox
    {
        static private Window wnd { get; set; }
        static private MessageBoxResult result { set; get; }

        static public MessageBoxResult Show(string txt)
        {
            result = MessageBoxResult.Cancel;

            wnd = new Window();
            wnd.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            wnd.Width = 600;
            wnd.Height = 300;
            wnd.FontFamily = ((MainWindow)Application.Current.MainWindow).FontFamily;
            wnd.FontSize = ((MainWindow)Application.Current.MainWindow).FontSize;
            wnd.Background = ((MainWindow)Application.Current.MainWindow).Background;
            wnd.Padding = new Thickness(50);
            Grid grd = new Grid();
            wnd.Content = grd;
            RowDefinition rw = new RowDefinition();
            rw.Height = new GridLength(150);
            grd.RowDefinitions.Add(rw);
            rw = new RowDefinition();
            rw.Height = GridLength.Auto;
            grd.RowDefinitions.Add(rw);
            TextBlock txb = new TextBlock();
            txb.Text = txt;
            txb.TextWrapping = TextWrapping.Wrap;
            txb.VerticalAlignment = VerticalAlignment.Center;
            txb.HorizontalAlignment = HorizontalAlignment.Left;
            grd.Children.Add(txb);
            Grid.SetRow(txb, 0);
            StackPanel stp = new StackPanel();
            stp.Orientation = Orientation.Horizontal;
            stp.HorizontalAlignment = HorizontalAlignment.Left;
            Button btnok = new Button();
            btnok.Content = "OK";
            btnok.Click += new RoutedEventHandler(btnOk_clicked);
            stp.Children.Add(btnok);
            Button btncl = new Button();
            btncl.Content = "Cancel";
            btncl.Click += new RoutedEventHandler(btncl_clicked);
            btncl.Margin = new Thickness(10, 0, 0, 0);
            stp.Children.Add(btncl);
            grd.Children.Add(stp);
            Grid.SetRow(stp, 1);
            grd.Margin = new Thickness(20);
            stp.HorizontalAlignment = HorizontalAlignment.Center;

            wnd.ShowDialog();

            return result;

        }

        static private void btnOk_clicked(object sender, RoutedEventArgs e)
        {
            result = MessageBoxResult.OK;
            wnd.Close();
        }

        static private void btncl_clicked(object sender, RoutedEventArgs e)
        {
            result = MessageBoxResult.Cancel;
            wnd.Close();

        }
    }
}
