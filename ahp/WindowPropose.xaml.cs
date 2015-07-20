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
    /// Interaction logic for WindowPropose.xaml
    /// </summary>
    public partial class WindowPropose : Window
    {
        Criterias c;
        int row;
        int col;
        List<int> indexes;

        object[,] ctrlArry;
        int[,] freedomArry;

        public WindowPropose()
        {
            InitializeComponent();
        }

        public WindowPropose(Criterias c, int row, int col, List<int> indexes)
        {
            InitializeComponent();
            this.c = c;
            this.row = row;
            this.col = col;
            this.indexes = new List<int>();
            if(indexes.Count != 0)
            {
                this.indexes = new List<int>();
                for(int i = 0; i < indexes.Count; i++)
                {
                    this.indexes.Add(indexes[i]);
                }
            }
            ctrlArry = new object[c.listCr.Count, c.listCr.Count];
            freedomArry = new int[c.listCr.Count, c.listCr.Count];

            for (int i = 0; i < c.listCr.Count; i++)
                for (int j = 0; j < c.listCr.Count; j++)
                {
                    freedomArry[i, j] = 0;
                }

            this.Width = (c.listCr.Count + 1) *100 + 60;
            this.Height = (c.listCr.Count + 1) * 30 * 2 + 200;

            FillOriginal(grdOriginal);
            FillFreedom(grdFreedom);
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            int i, j;

            int comN = 1;

            for(i = 0; i < c.listCr.Count; i++)
                for(j = 0; j < c.listCr.Count; j++)
                {
                    if(null != ctrlArry[i, j] && null == ctrlArry[i, j] as TextBlock)
                    {
                        switch((ctrlArry[i, j] as ComboBox).SelectedIndex)
                        {
                            case 0:
                                break;
                            case 1:
                                comN *= 8;
                                freedomArry[i, j] = 1;
                                break;
                            case 2:
                                comN *= 17;
                                freedomArry[i, j] = 2;
                                break;
                            default:
                                break;
                        }
                    }               
                }

            MessageBox.Show(comN.ToString() + " combinations will be evaluated!");
            this.DialogResult = true;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void FillOriginal(Grid mtxGrid)
        {
            int i, j;
            // initial cell width and height
            double cellWidth = 100;
            double cellHeight = 30;

            // clear grid and redraw on it.
            mtxGrid.Children.Clear();
            mtxGrid.RowDefinitions.Clear();
            mtxGrid.ColumnDefinitions.Clear();


            for (i = 0; i < c.listCr.Count + 1; i++)
            {
                RowDefinition row = new RowDefinition();
                row.Height = new GridLength(cellHeight);
                ColumnDefinition col = new ColumnDefinition();
                col.Width = new GridLength(cellWidth);

                mtxGrid.RowDefinitions.Add(row);
                mtxGrid.ColumnDefinitions.Add(col);
            }
            
            (mtxGrid.Parent as Border).Width = (c.listCr.Count + 1) * cellWidth;
            (mtxGrid.Parent as Border).Height = (c.listCr.Count + 1) * cellHeight;

            if (c.listCr.Count == 0)
                (mtxGrid.Parent as Border).Visibility = Visibility.Hidden;
            else
                (mtxGrid.Parent as Border).Visibility = Visibility.Visible;

            // redraw matrix
            for (i = 1; i < c.listCr.Count + 1; i++)
            {
                TextBlock txt;

                // value 1 on diagonal 
                txt = new TextBlock();
                txt.Text = "1";
                mtxGrid.Children.Add(txt);
                Grid.SetRow(txt, i);
                Grid.SetColumn(txt, i);
                txt.HorizontalAlignment = HorizontalAlignment.Center;
                txt.VerticalAlignment = VerticalAlignment.Center;

                txt = new TextBlock();
                txt.Text = c.listCr.ElementAt(i - 1).name;
                mtxGrid.Children.Add(txt);
                Grid.SetRow(txt, 0);
                Grid.SetColumn(txt, i);
                txt.HorizontalAlignment = HorizontalAlignment.Center;
                txt.VerticalAlignment = VerticalAlignment.Center;

                txt = new TextBlock();
                txt.Text = c.listCr.ElementAt(i - 1).name;
                mtxGrid.Children.Add(txt);
                Grid.SetRow(txt, i);
                Grid.SetColumn(txt, 0);
                txt.HorizontalAlignment = HorizontalAlignment.Center;
                txt.VerticalAlignment = VerticalAlignment.Center;


                // starting from first line;
                if (i < c.listCr.Count)
                {
                    for (j = i + 1; j < c.listCr.Count + 1; j++)
                    {
                        NumericUpDown nupCtrl = new NumericUpDown();
                        nupCtrl.dblValue = c.mtx[i - 1, j - 1];


                        txt = new TextBlock();
                        txt.Text = c.mtx[i - 1, j - 1].ToString();
                        mtxGrid.Children.Add(txt);
                        Grid.SetRow(txt, i);
                        Grid.SetColumn(txt, j);
                        txt.HorizontalAlignment = HorizontalAlignment.Center;
                        txt.VerticalAlignment = VerticalAlignment.Center;

                        if(this.indexes.Count > 0)
                        {
                            for(int k = 0; k < this.indexes.Count; k++)
                            {
                                if ((i - 1 == row && j - 1 == this.indexes[k]) ||
                                    (i - 1 == this.indexes[k] && j - 1 == col))
                                    txt.Background = Brushes.Pink;
                            }
                        }

                        txt = new TextBlock();
                        txt.Text = nupCtrl.strValueR;
                        mtxGrid.Children.Add(txt);
                        Grid.SetRow(txt, j);
                        Grid.SetColumn(txt, i);
                        txt.HorizontalAlignment = HorizontalAlignment.Center;
                        txt.VerticalAlignment = VerticalAlignment.Center;
                    }
                }
            }

        }

        private void FillFreedom(Grid mtxGrid)
        {
            int i, j;
            // initial cell width and height
            double cellWidth = 100;
            double cellHeight = 30;

            // clear grid and redraw on it.
            mtxGrid.Children.Clear();
            mtxGrid.RowDefinitions.Clear();
            mtxGrid.ColumnDefinitions.Clear();


            for (i = 0; i < c.listCr.Count + 1; i++)
            {
                RowDefinition row = new RowDefinition();
                row.Height = new GridLength(cellHeight);
                ColumnDefinition col = new ColumnDefinition();
                col.Width = new GridLength(cellWidth);

                mtxGrid.RowDefinitions.Add(row);
                mtxGrid.ColumnDefinitions.Add(col);
            }

            (mtxGrid.Parent as Border).Width = (c.listCr.Count + 1) * cellWidth;
            (mtxGrid.Parent as Border).Height = (c.listCr.Count + 1) * cellHeight;

            if (c.listCr.Count == 0)
                (mtxGrid.Parent as Border).Visibility = Visibility.Hidden;
            else
                (mtxGrid.Parent as Border).Visibility = Visibility.Visible;

            // redraw matrix
            for (i = 1; i < c.listCr.Count + 1; i++)
            {
                TextBlock txt;

                // value 1 on diagonal 
                txt = new TextBlock();
                txt.Text = "1";
                mtxGrid.Children.Add(txt);
                Grid.SetRow(txt, i);
                Grid.SetColumn(txt, i);
                txt.HorizontalAlignment = HorizontalAlignment.Center;
                txt.VerticalAlignment = VerticalAlignment.Center;

                txt = new TextBlock();
                txt.Text = c.listCr.ElementAt(i - 1).name;
                mtxGrid.Children.Add(txt);
                Grid.SetRow(txt, 0);
                Grid.SetColumn(txt, i);
                txt.HorizontalAlignment = HorizontalAlignment.Center;
                txt.VerticalAlignment = VerticalAlignment.Center;

                txt = new TextBlock();
                txt.Text = c.listCr.ElementAt(i - 1).name;
                mtxGrid.Children.Add(txt);
                Grid.SetRow(txt, i);
                Grid.SetColumn(txt, 0);
                txt.HorizontalAlignment = HorizontalAlignment.Center;
                txt.VerticalAlignment = VerticalAlignment.Center;


                // starting from first line;
                if (i < c.listCr.Count)
                {
                    for (j = i + 1; j < c.listCr.Count + 1; j++)
                    {
                        ComboBox ckb = new ComboBox();                      
                        mtxGrid.Children.Add(ckb);
                        Grid.SetRow(ckb, i);
                        Grid.SetColumn(ckb, j);
                        if (this.indexes.Count > 0)
                        {
                            for (int k = 0; k < this.indexes.Count; k++)
                            {
                                if ((i - 1 == row && j - 1 == this.indexes[k]) ||
                                    (i - 1 == this.indexes[k] && j - 1 == col))
                                    ckb.Background = Brushes.Pink;
                            }
                        }
                        ckb.Items.Add("None");
                        ckb.Items.Add("Direction Only");
                        ckb.Items.Add("Direction & Magnitude");
                        ckb.SelectedIndex = 0;
                        ckb.Margin = new Thickness(3, 3, 3, 3);

                        ctrlArry[i-1, j-1] = (object) ckb;


                        txt = new TextBlock();
                        txt.Text = "None";
                        mtxGrid.Children.Add(txt);
                        Grid.SetRow(txt, j);
                        Grid.SetColumn(txt, i);
                        txt.HorizontalAlignment = HorizontalAlignment.Center;
                        txt.VerticalAlignment = VerticalAlignment.Center;

                        ctrlArry[j - 1, i - 1] = (object)txt;
                    }
                }
            }

        }

    }
}
