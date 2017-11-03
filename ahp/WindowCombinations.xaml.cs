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

using MyNumericUpDownControll;

namespace ahp
{
    /// <summary>
    /// Interaction logic for WindowCombinations.xaml
    /// </summary>
    public partial class WindowCombinations : Window
    {
        public int idx = -1;
        public WindowCombinations()
        {
            InitializeComponent();
        }

        public WindowCombinations(List<Criterias> options, List<StrctCell> inconsistentCells)
        {
            InitializeComponent();
            var clist = new List<Criterias>();

            // Only list the first 20 options.
            if (options.Count > 20)
            {
                clist = options.GetRange(0, 20);
            }
            else
                clist = options;

            int i, j, k;
            // initial cell width and height
            double cellWidth = 80;
            double cellHeight = 60;
            double cellMaxWidth = 200;

            //this.Width = ((MainWindow)(Application.Current.MainWindow)).mtxBdr.ActualWidth + 100;
            if (clist.Count == 0) return;
            this.Width = (clist.ElementAt(0).listCr.Count + 1) * cellWidth + 100; 

            // clear grid and redraw on it.
            for (i = 0; i < clist.Count*2; i++)
            {
                RowDefinition row = new RowDefinition();
                GrdContent.RowDefinitions.Add(row);
            }

            for(k = 0; k < clist.Count; k++)
            {
                Criterias c = clist.ElementAt(k);

                RadioButton rb = new RadioButton();
                rb.Content = "CR = " + c.CR.ToString();
                rb.GroupName = "crOptions";
                // Name to track the index
                rb.Name = "rb" + k.ToString();
                GrdContent.Children.Add(rb);
                Grid.SetRow(rb, 2 * k);
                rb.Click += Rb_Click;
                rb.Margin = new Thickness(10, 10, 0, 0);

                Grid grd = new Grid();
                grd.ShowGridLines = true;
                for (i = 0; i < c.listCr.Count + 1; i++)
                {
                    RowDefinition row = new RowDefinition();
                    row.Height = GridLength.Auto;
                    row.MinHeight = cellHeight;
                    ColumnDefinition col = new ColumnDefinition();
                    col.Width = GridLength.Auto;
                    col.MinWidth = cellWidth;

                    grd.RowDefinitions.Add(row);
                    grd.ColumnDefinitions.Add(col);
                }

                // redraw matrix
                for (i = 1; i < c.listCr.Count + 1; i++)
                {
                    TextBlock txt;

                    // value 1 on diagonal 
                    txt = new TextBlock();
                    txt.Text = "1";
                    grd.Children.Add(txt);
                    Grid.SetRow(txt, i);
                    Grid.SetColumn(txt, i);
                    txt.HorizontalAlignment = HorizontalAlignment.Center;
                    txt.VerticalAlignment = VerticalAlignment.Center;

                    txt = new TextBlock();
                    txt.Text = c.listCr.ElementAt(i - 1).name;
                    grd.Children.Add(txt);
                    Grid.SetRow(txt, 0);
                    Grid.SetColumn(txt, i);
                    txt.HorizontalAlignment = HorizontalAlignment.Center;
                    txt.VerticalAlignment = VerticalAlignment.Center;
                    txt.MaxWidth = cellMaxWidth;
                    txt.TextWrapping = TextWrapping.Wrap;

                    txt = new TextBlock();
                    txt.Text = c.listCr.ElementAt(i - 1).name;
                    grd.Children.Add(txt);
                    Grid.SetRow(txt, i);
                    Grid.SetColumn(txt, 0);
                    txt.HorizontalAlignment = HorizontalAlignment.Center;
                    txt.VerticalAlignment = VerticalAlignment.Center;
                    txt.MaxWidth = cellMaxWidth;
                    txt.TextWrapping = TextWrapping.Wrap;


                    // starting from first line;
                    if (i < c.listCr.Count)
                    {
                        for (j = i + 1; j < c.listCr.Count + 1; j++)
                        {
                            //NumericUpDown nupCtrl = new NumericUpDown();
                            //nupCtrl.dblValue = c.mtx[i - 1, j - 1];


                            txt = new TextBlock();
                            txt.Text = string.Format("{0:#.00}", c.mtx[i - 1, j - 1]);
                            txt.Text = dbl2str(c.mtx[i - 1, j - 1]);
                            grd.Children.Add(txt);
                            Grid.SetRow(txt, i);
                            Grid.SetColumn(txt, j);
                            txt.HorizontalAlignment = HorizontalAlignment.Center;
                            txt.VerticalAlignment = VerticalAlignment.Center;

                            if (inconsistentCells.Count > 0)
                            {
                                for (int l = 0; l < inconsistentCells.Count; l++)
                                {
                                    if (i - 1 == inconsistentCells[l].row && j - 1 == inconsistentCells[l].col)
                                            txt.Background = Brushes.Pink;
                                }
                            }

                            txt = new TextBlock();
                            txt.Text = dbl2str(c.mtx[j - 1, i - 1]);
                            grd.Children.Add(txt);
                            Grid.SetRow(txt, j);
                            Grid.SetColumn(txt, i);
                            txt.HorizontalAlignment = HorizontalAlignment.Center;
                            txt.VerticalAlignment = VerticalAlignment.Center;
                        }
                    }
                }

                //grd.ShowGridLines = true;
                Border bdr = new Border();
                bdr.BorderThickness = new Thickness(1);
                bdr.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xE9, 0xF0, 0xF7));
                bdr.BorderBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x8C, 0xA6, 0xC9));
                bdr.CornerRadius = new CornerRadius(5);
                bdr.Child = grd;
                bdr.Width = (c.listCr.Count+1) * cellWidth;

                GrdContent.Children.Add(bdr);
                Grid.SetRow(bdr, 2 * k + 1);
            }

        }

        private void Rb_Click(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb == null)
                return;

            idx = Convert.ToInt32(rb.Name.Substring(2));
        }

        private void BtnApply_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
          
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {

        }

        private string dbl2str(double dbl)
        {
            string[] strValues = { "1/9", "1/8", "1/7", "1/6", "1/5", "1/4", "1/3", "1/2", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            double[] dblValues = { 0.1111111, 0.125, 0.1428571, 0.1666666, 0.2, 0.25, 0.3333333, 0.5, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            double difference = 0.001;
            int i = 0;
            for(i = 0; i < dblValues.Length; i++)
            {
                // Equal.
                if (Math.Abs(dblValues[i] - dbl) < difference)
                    break;
                else
                    continue;
            }

            return strValues[i];
        }

    }



}
