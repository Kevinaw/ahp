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

        public WindowCombinations(List<Criterias> options, int inconsistencyRow, int inconsistencyCol, List<int> inconsistecyIndexes)
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
            double cellWidth = 60;
            double cellHeight = 30;

            // clear grid and redraw on it.

            for(i = 0; i < clist.Count*2; i++)
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

                Grid grd = new Grid();
                grd.ShowGridLines = true;
                for (i = 0; i < c.listCr.Count + 1; i++)
                {
                    RowDefinition row = new RowDefinition();
                    row.Height = new GridLength(cellHeight);
                    ColumnDefinition col = new ColumnDefinition();
                    col.Width = new GridLength(cellWidth);

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

                    txt = new TextBlock();
                    txt.Text = c.listCr.ElementAt(i - 1).name;
                    grd.Children.Add(txt);
                    Grid.SetRow(txt, i);
                    Grid.SetColumn(txt, 0);
                    txt.HorizontalAlignment = HorizontalAlignment.Center;
                    txt.VerticalAlignment = VerticalAlignment.Center;


                    // starting from first line;
                    if (i < c.listCr.Count)
                    {
                        for (j = i + 1; j < c.listCr.Count + 1; j++)
                        {
                            //NumericUpDown nupCtrl = new NumericUpDown();
                            //nupCtrl.dblValue = c.mtx[i - 1, j - 1];


                            txt = new TextBlock();
                            txt.Text = string.Format("{0:#.00}", c.mtx[i - 1, j - 1]);
                            grd.Children.Add(txt);
                            Grid.SetRow(txt, i);
                            Grid.SetColumn(txt, j);
                            txt.HorizontalAlignment = HorizontalAlignment.Center;
                            txt.VerticalAlignment = VerticalAlignment.Center;

                            if (inconsistecyIndexes.Count > 0)
                            {
                                for (int l = 0; l < inconsistecyIndexes.Count; l++)
                                {
                                    if ((i - 1 == inconsistencyRow && j - 1 == inconsistecyIndexes[l]) ||
                                        (i - 1 == inconsistecyIndexes[l] && j - 1 == inconsistencyCol))
                                        txt.Background = Brushes.Pink;
                                }
                            }

                            txt = new TextBlock();
                            txt.Text = string.Format("{0:#.00}", c.mtx[j - 1, i - 1]);
                            grd.Children.Add(txt);
                            Grid.SetRow(txt, j);
                            Grid.SetColumn(txt, i);
                            txt.HorizontalAlignment = HorizontalAlignment.Center;
                            txt.VerticalAlignment = VerticalAlignment.Center;
                        }
                    }
                }

                Border bdr = new Border();
                bdr.BorderThickness = new Thickness(1);
                bdr.Background = new SolidColorBrush(Color.FromArgb(0xFF, 0xE9, 0xF0, 0xF7));
                bdr.BorderBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x8C, 0xA6, 0xC9));
                bdr.CornerRadius = new CornerRadius(5);
                bdr.Child = grd;

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

    }



}
