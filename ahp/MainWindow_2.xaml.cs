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

using System.Windows.Threading;
using System.Windows.Media.Effects;
using System.Xml;
using System.Xml.Linq;
using System.IO;

namespace ahp
{
    /// <summary>
    /// Interaction logic for MainWindow_2.xaml
    /// </summary>
    /// 
    public partial class MainWindow_2 : Window
    {
        //string tmpFile;
        private Boolean isEditingAl = false;
        private int editingItemIndexAl;
        private Boolean isEditingCr = false;
        private int editingItemIndexCr;

        List<string> listAlt = new List<string>();
        List<string> listCr = new List<string>();

        private double[,] mtx;
        private int[,] mtxAC; 
        private static string[] strValues = { "1/9", "1/8", "1/7", "1/6", "1/5", "1/4", "1/3", "1/2", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        private static double[] dblValues = { 0.1111111, 0.125, 0.1428571, 0.1666666, 0.2, 0.25, 0.3333333, 0.5, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        private StructEvalResult stctResult;
        private bool isOpeningPrj; //opening old project

        public MainWindow_2()
        {
            InitializeComponent();
            isOpeningPrj = false;

        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //drawAhpHierarchy();
            addItemBoxAl_LostFocus();
            addItemBoxCr_LostFocus();
        }

        private void addBtnAl_Click(object sender, RoutedEventArgs e)
        {
            TextBox newTxtBx = new TextBox();
            newTxtBx.Width = contentLbxAl.ActualWidth - 10;
            newTxtBx.Name = "addItemBoxAl";

            // if the last item is textbox, don't show the box            
            if (contentLbxAl.Items.Count != 0)
            {
                int index;
                object lastItem;
                lastItem = contentLbxAl.Items.GetItemAt(contentLbxAl.Items.Count - 1);// as ListBoxItem;

                if (lastItem.GetType().ToString() != "System.Windows.Controls.TextBox")
                {
                    index = contentLbxAl.Items.Add(newTxtBx);
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                                (Action)(() => { Keyboard.Focus(newTxtBx); }));
                }
            }
            else
                contentLbxAl.Items.Add(newTxtBx);
        }

        private void addItemBoxAl_LostFocus(/*object sender, RoutedEventArgs e*/)
        {
            if (contentLbxAl.Items.Count != 0)
            {
                TextBox lastItem;
                lastItem = contentLbxAl.Items.GetItemAt(contentLbxAl.Items.Count - 1) as TextBox;

                if (lastItem != null && lastItem.Text != "")
                {
                    if (listAlt.Exists(x => x == lastItem.Text))
                    {
                        MessageBox.Show("duplicate alternative name!");
                        return;
                    }
                    listAlt.Add(lastItem.Text);
                    drawAhpHierarchy();


                    // delete the textbox
                    contentLbxAl.Items.RemoveAt(contentLbxAl.Items.Count - 1);

                    // add grid with delete button
                    ListBoxItem newItem = new ListBoxItem();

                    Grid newGrid = new Grid();
                    ColumnDefinition col1 = new ColumnDefinition();
                    ColumnDefinition col2 = new ColumnDefinition();
                    col1.Width = new GridLength(contentLbxAl.ActualWidth * 5 / 8);
                    col1.Width = new GridLength(contentLbxAl.ActualWidth * 3 / 8);
                    newGrid.ColumnDefinitions.Add(col1);
                    newGrid.ColumnDefinitions.Add(col2);

                    TextBlock newText = new TextBlock();
                    newText.Text = lastItem.Text;
                    //newText.AddHandler(MouseDoubleClickEvent, new RoutedEventHandler(ListItem_DoubleClick));
                    newText.MouseDown += new MouseButtonEventHandler(ListItemAl_DoubleClick);

                    Button newBtn = new Button();
                    newBtn.Name = "deleteBtnAl" + contentLbxAl.Items.Count.ToString();
                    newBtn.Width = 22;
                    newBtn.Height = 22;
                    newBtn.Cursor = Cursors.Hand;
                    newBtn.ToolTip = "delete alternative";
                    newBtn.Click += new RoutedEventHandler(deleteBtnAl_OnClick);

                    ImageBrush newB = new ImageBrush();
                    newB.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Resources/delete1.png"));
                    newBtn.Background = newB;

                    newGrid.Children.Add(newText);
                    newGrid.Children.Add(newBtn);

                    Grid.SetColumn(newBtn, 1);

                    newItem.Content = newGrid;

                    contentLbxAl.Items.Add(newItem);
                    RegisterName(newBtn.Name, newBtn);
                }
                else if (lastItem != null && lastItem.Text == "")
                    contentLbxAl.Items.RemoveAt(contentLbxAl.Items.Count - 1);
            }

            if (isEditingAl == true)
            {

                ListBoxItem lbi = contentLbxAl.Items.GetItemAt(editingItemIndexAl) as ListBoxItem;
                TextBox txt = (lbi.Content as Grid).Children[1] as TextBox;
                if (txt == null)
                    txt = (lbi.Content as Grid).Children[0] as TextBox;

                if (listAlt.Exists(x => x == txt.Text))
                {
                    MessageBox.Show("duplicate alternative name!");
                    return;
                }
                //listAlt.Remove(txt.Text);
                listAlt.Add(txt.Text);
                drawAhpHierarchy();

                TextBlock txb = new TextBlock();
                txb.Text = txt.Text;

                (lbi.Content as Grid).Children.Remove(txt);
                (lbi.Content as Grid).Children.Add(txb);

                isEditingAl = false;
            }
        }

        private void deleteBtnAl_OnClick(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null)
            {
                int i;
                int idx;
                Button btn1;

                idx = Convert.ToInt16(btn.Name.Substring(11));

                ListBoxItem lbi = contentLbxAl.Items.GetItemAt(idx) as ListBoxItem;
                TextBlock txt = (lbi.Content as Grid).Children[0] as TextBlock;
                if (txt == null)
                    txt = (lbi.Content as Grid).Children[1] as TextBlock;
                listAlt.Remove(txt.Text);
                drawAhpHierarchy();
                // delete this item according the index
                contentLbxAl.Items.RemoveAt(idx);
                if(null != FindName(btn.Name))
                    UnregisterName(btn.Name);

                if (idx != contentLbxAl.Items.Count)
                {
                    for (i = idx + 1; i < contentLbxAl.Items.Count + 1; i++)
                    {
                        btn1 = FindName("deleteBtnAl" + Convert.ToString(i)) as Button;
                        if(null != FindName(btn1.Name))
                            UnregisterName(btn1.Name);
                        btn1.Name = "deleteBtnAl" + (i - 1).ToString();
                        RegisterName(btn1.Name, btn1);
                    }
                }

            }
        }

        private void ListItemAl_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            // double click, trigger this editing event
            if (e.ClickCount != 2)
                return;

            TextBlock txb = sender as TextBlock;
            if (txb == null)
                return;

            TextBox txb1 = new TextBox();
            txb1.Text = txb.Text;

            listAlt.Remove(txb.Text);
            drawAhpHierarchy();

            Grid grd = txb.Parent as Grid;
            grd.Children.Remove(txb);
            grd.Children.Add(txb1);

            isEditingAl = true;
            ListBoxItem lbi = grd.Parent as ListBoxItem;
            editingItemIndexAl = contentLbxAl.Items.IndexOf(lbi);

            this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                                (Action)(() => { Keyboard.Focus(txb1); }));

        }

        private void addBtnCr_Click(object sender, RoutedEventArgs e)
        {
            TextBox newTxtBx = new TextBox();
            newTxtBx.Width = contentLbxCr.ActualWidth - 10;
            newTxtBx.Name = "addItemBoxCr";

            // if the last item is textbox, don't show the box            
            if (contentLbxCr.Items.Count != 0)
            {
                int index;
                object lastItem;
                lastItem = contentLbxCr.Items.GetItemAt(contentLbxCr.Items.Count - 1);// as ListBoxItem;

                if (lastItem.GetType().ToString() != "System.Windows.Controls.TextBox")
                {
                    index = contentLbxCr.Items.Add(newTxtBx);
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                                (Action)(() => { Keyboard.Focus(newTxtBx); }));
                }
            }
            else
                contentLbxCr.Items.Add(newTxtBx);
        }

        private void addItemBoxCr_LostFocus(/*object sender, RoutedEventArgs e*/)
        {
            if (contentLbxCr.Items.Count != 0)
            {
                TextBox lastItem;
                lastItem = contentLbxCr.Items.GetItemAt(contentLbxCr.Items.Count - 1) as TextBox;

                if (lastItem != null && lastItem.Text != "")
                {
                    if (listCr.Exists(x => x == lastItem.Text))
                    {
                        MessageBox.Show("duplicate criteria name!");
                        return;
                    }
                    listCr.Add(lastItem.Text);
                    drawAhpHierarchy();


                    // delete the textbox
                    contentLbxCr.Items.RemoveAt(contentLbxCr.Items.Count - 1);

                    // add grid with delete button
                    ListBoxItem newItem = new ListBoxItem();

                    Grid newGrid = new Grid();
                    ColumnDefinition col1 = new ColumnDefinition();
                    ColumnDefinition col2 = new ColumnDefinition();
                    col1.Width = new GridLength(contentLbxCr.ActualWidth * 5 / 8);
                    col1.Width = new GridLength(contentLbxCr.ActualWidth * 3 / 8);
                    newGrid.ColumnDefinitions.Add(col1);
                    newGrid.ColumnDefinitions.Add(col2);

                    TextBlock newText = new TextBlock();
                    newText.Text = lastItem.Text;
                    //newText.AddHandler(MouseDoubleClickEvent, new RoutedEventHandler(ListItem_DoubleClick));
                    newText.MouseDown += new MouseButtonEventHandler(ListItemCr_DoubleClick);

                    Button newBtn = new Button();
                    newBtn.Name = "deleteBtnCr" + contentLbxCr.Items.Count.ToString();
                    newBtn.Width = 22;
                    newBtn.Height = 22;
                    newBtn.Cursor = Cursors.Hand;
                    newBtn.ToolTip = "delete criteria";
                    newBtn.Click += new RoutedEventHandler(deleteBtnCr_OnClick);

                    ImageBrush newB = new ImageBrush();
                    newB.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Resources/delete1.png"));
                    newBtn.Background = newB;

                    newGrid.Children.Add(newText);
                    newGrid.Children.Add(newBtn);

                    Grid.SetColumn(newBtn, 1);

                    newItem.Content = newGrid;

                    contentLbxCr.Items.Add(newItem);
                    RegisterName(newBtn.Name, newBtn);
                }
                else if (lastItem != null && lastItem.Text == "")
                    contentLbxCr.Items.RemoveAt(contentLbxCr.Items.Count - 1);
            }

            if (isEditingCr == true)
            {

                ListBoxItem lbi = contentLbxCr.Items.GetItemAt(editingItemIndexCr) as ListBoxItem;
                TextBox txt = (lbi.Content as Grid).Children[1] as TextBox;
                if (txt == null)
                    txt = (lbi.Content as Grid).Children[0] as TextBox;

                if (listCr.Exists(x => x == txt.Text))
                {
                    MessageBox.Show("duplicate criteria name!");
                    return;
                }
                //listAlt.Remove(txt.Text);
                listCr.Add(txt.Text);
                drawAhpHierarchy();

                TextBlock txb = new TextBlock();
                txb.Text = txt.Text;

                (lbi.Content as Grid).Children.Remove(txt);
                (lbi.Content as Grid).Children.Add(txb);

                isEditingCr = false;
            }
        }

        private void deleteBtnCr_OnClick(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null)
            {
                int i;
                int idx;
                Button btn1;

                idx = Convert.ToInt16(btn.Name.Substring(11));

                ListBoxItem lbi = contentLbxCr.Items.GetItemAt(idx) as ListBoxItem;
                TextBlock txt = (lbi.Content as Grid).Children[0] as TextBlock;
                if (txt == null)
                    txt = (lbi.Content as Grid).Children[1] as TextBlock;
                listCr.Remove(txt.Text);
                drawAhpHierarchy();
                // delete this item according the index
                contentLbxCr.Items.RemoveAt(idx);
                if(null != FindName(btn.Name))
                    UnregisterName(btn.Name);

                if (idx != contentLbxCr.Items.Count)
                {
                    for (i = idx + 1; i < contentLbxCr.Items.Count + 1; i++)
                    {
                        btn1 = FindName("deleteBtnCr" + Convert.ToString(i)) as Button;
                        if(null != FindName(btn1.Name))
                            UnregisterName(btn1.Name);
                        btn1.Name = "deleteBtnCr" + (i - 1).ToString();
                        RegisterName(btn1.Name, btn1);
                    }
                }

            }
        }

        private void ListItemCr_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            // double click, trigger this editing event
            if (e.ClickCount != 2)
                return;

            TextBlock txb = sender as TextBlock;
            if (txb == null)
                return;

            TextBox txb1 = new TextBox();
            txb1.Text = txb.Text;

            listCr.Remove(txb.Text);
            drawAhpHierarchy();

            Grid grd = txb.Parent as Grid;
            grd.Children.Remove(txb);
            grd.Children.Add(txb1);

            isEditingCr = true;
            ListBoxItem lbi = grd.Parent as ListBoxItem;
            editingItemIndexCr = contentLbxCr.Items.IndexOf(lbi);

            this.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                                (Action)(() => { Keyboard.Focus(txb1); }));

        }

        private void cmdUp_Click(object sender, RoutedEventArgs e)
        {
            Button btnUp = sender as Button;

            if (btnUp == null)
                return;

            string strN = btnUp.Name;
            int intR, intC;
            intR = Convert.ToInt16(strN.Substring(5, 1));
            intC = Convert.ToInt16(strN.Substring(6, 1));

            double dbl = mtx[intR, intC];
            int idx = Array.FindIndex(dblValues, x => x == dbl);

            if (idx == -1)
            { MessageBox.Show("not found!"); return; }

            if (idx == dblValues.Length - 1)
                return;
            else
                idx += 1;

            mtx[intR, intC] = dblValues[idx];
            mtx[intC, intR] = dblValues[dblValues.Length - 1 - idx];

            (FindName("txtNum" + intR.ToString() + intC.ToString()) as TextBox).Text = strValues[idx];
            (FindName("txtBlk" + intC.ToString() + intR.ToString()) as TextBlock).Text = strValues[strValues.Length - 1 - idx];
        }

        private void cmdDn_Click(object sender, RoutedEventArgs e)
        {
            Button btnDn = sender as Button;

            if (btnDn == null)
                return;

            string strN = btnDn.Name;
            int intR, intC;
            intR = Convert.ToInt16(strN.Substring(5, 1));
            intC = Convert.ToInt16(strN.Substring(6, 1));

            double dbl = mtx[intR, intC];
            int idx = Array.FindIndex(dblValues, x => x == dbl);

            if (idx == -1)
            { MessageBox.Show("not found!"); return; }

            if (idx == 0)
                return;
            else
                idx -= 1;

            mtx[intR, intC] = dblValues[idx];
            mtx[intC, intR] = dblValues[dblValues.Length - 1 - idx];

            (FindName("txtNum" + intR.ToString() + intC.ToString()) as TextBox).Text = strValues[idx];
            (FindName("txtBlk" + intC.ToString() + intR.ToString()) as TextBlock).Text = strValues[strValues.Length - 1 - idx];

        }

        // calculate weights and CR
        private StructEvalResult ahp_eval(double[,] matrix)
        {
            int n = (int)Math.Sqrt(matrix.Length);
            int row = n;
            int column = n;
            double[] mul_column = new double[row];
            double[] rot_column = new double[row];
            double[] w = new double[row];
            double[] aw = new double[row];
            double[] aw_w = new double[row];
            double CI;
            double CR;
            double sum = 0;
            double average;
            double[] RI = { 0.0, 0.0, 0.0, 0.58, 0.9, 1.12, 1.24, 1.32, 1.41, 1.45, 1.49 };

            for (int i = 0; i < row; i++)
            {
                mul_column[i] = 1;
                for (int j = 0; j < column; j++)
                    mul_column[i] *= matrix[i, j];
                rot_column[i] = Math.Pow(mul_column[i], 1.0 / n);
                sum += rot_column[i];
            }

            // get normalized weight
            for (int i = 0; i < row; i++)
            {
                w[i] = rot_column[i] / sum;
            }

            sum = 0;
            // get aw and aww
            for (int i = 0; i < row; i++)
            {
                aw[i] = 0;
                for (int j = 0; j < column; j++)
                    aw[i] += matrix[i, j] * w[j];

                aw_w[i] = aw[i] / w[i];
                sum += aw_w[i];
            }
            average = sum / row;

            CI = (average - n) / (n - 1);
            CR = CI / RI[n];

            StructEvalResult result;
            result.w = w;
            result.CR = CR;
            //double[] result = new double[n + 1];
            //w.CopyTo(result, 0);
            //result[n] = CR;
            return result;
        }

        private void tabMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;
            TabItem selectedItem = e.AddedItems[0] as TabItem;
            if (selectedItem != null && selectedItem.Header.ToString() == "Result")
            {
                DrawResultMtx();
            }

        }

        private void tabPwC_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabItem selectedItem = e.AddedItems[0] as TabItem;
            if (selectedItem != null && selectedItem.Header.ToString() == "Criteria vs. Goal")
            {
                DrawCriteriaCompMtx();

            }
            else if(selectedItem.Header.ToString() == "Alternative vs. criteria")
            {
                DrawACCompMtx();

            }

        }

        // draw criteria vs goal comparison matrix
        private void DrawCriteriaCompMtx()
        {
            int i, j;
            mtxBdr.Visibility = Visibility.Visible;
            //this.mtxGrid.Visibility = Visibility.Visible;
            mtxGrid.Children.Clear();
            mtxGrid.RowDefinitions.Clear();
            mtxGrid.ColumnDefinitions.Clear();

            if (listCr.Count == 0)
                return;

            if (mtx != null && isOpeningPrj == false)
            {
                // unregister all the element names
                for (i = 0; i < Math.Sqrt(mtx.Length) - 1; i++)
                    for (j = i + 1; j < Math.Sqrt(mtx.Length); j++)
                    {
                        if(FindName("txtNum" + i.ToString() + j.ToString()) != null)
                            UnregisterName("txtNum" + i.ToString() + j.ToString());
                        if (FindName("txtBlk" + j.ToString() + i.ToString()) != null)
                            UnregisterName("txtBlk" + j.ToString() + i.ToString());
                    }
            }

            if (mtx == null && isOpeningPrj == false)
            {
                mtx = new double[listCr.Count, listCr.Count];
                for (i = 0; i < listCr.Count; i++)
                    for (j = 0; j < listCr.Count; j++)
                        mtx[i, j] = 1;
            }
            // delete one or more criteria
            else if (mtx.Length > listCr.Count * listCr.Count)
            {
                double[,] tmp = mtx;
                mtx = new double[listCr.Count, listCr.Count];

                for (i = 0; i < listCr.Count; i++)
                    for (j = 0; j < listCr.Count; j++)
                        mtx[i, j] = tmp[i, j];
            }
            // add one or more criteria
            else if (mtx.Length < listCr.Count * listCr.Count)
            {
                double[,] tmp = mtx;
                mtx = new double[listCr.Count, listCr.Count];
                for (i = 0; i < listCr.Count; i++)
                    for (j = 0; j < listCr.Count; j++)
                        mtx[i, j] = 1;

                for (i = 0; i < Math.Sqrt(tmp.Length); i++)
                    for (j = 0; j < Math.Sqrt(tmp.Length); j++)
                        mtx[i, j] = tmp[i, j];
            }
            //else // no change to criteria
             //   ;


            double gridWidth = 90;
            double gridHeight = 60;
            (mtxGrid.Parent as Border).HorizontalAlignment = HorizontalAlignment.Center;
            (mtxGrid.Parent as Border).VerticalAlignment = VerticalAlignment.Center;
            double x = tabMain.ActualWidth;
            double y = tabMain.ActualHeight;
            if (x >= (listCr.Count + 1) * gridWidth)
            {
                for (i = 0; i < listCr.Count + 1; i++)
                {
                    RowDefinition row = new RowDefinition();
                    row.Height = new GridLength(gridHeight);
                    ColumnDefinition col = new ColumnDefinition();
                    col.Width = new GridLength(gridWidth);

                    mtxGrid.RowDefinitions.Add(row);
                    mtxGrid.ColumnDefinitions.Add(col);
                }
                (mtxGrid.Parent as Border).Width = (listCr.Count + 1) * gridWidth;
                (mtxGrid.Parent as Border).Height = (listCr.Count + 1) * gridHeight;

            }

            else
            {
                gridWidth = (x - 10) / (listCr.Count + 1);
                gridHeight = gridWidth * 2 / 3;
                for (i = 0; i < listCr.Count + 1; i++)
                {
                    RowDefinition row = new RowDefinition();
                    row.Height = new GridLength(gridHeight);
                    ColumnDefinition col = new ColumnDefinition();
                    col.Width = new GridLength(gridWidth);

                    mtxGrid.RowDefinitions.Add(row);
                    mtxGrid.ColumnDefinitions.Add(col);
                }
                (mtxGrid.Parent as Border).Width = x - 10;
                (mtxGrid.Parent as Border).Height = (x - 10) * 2 / 3;

            }

            // redraw matrix
            for (i = 0; i < listCr.Count + 1; i++)
            {
                TextBlock txt;

                if (i != listCr.Count)
                {
                    txt = new TextBlock();
                    txt.Text = listCr.ElementAt(i);
                    mtxGrid.Children.Add(txt);
                    Grid.SetRow(txt, 0);
                    Grid.SetColumn(txt, i + 1);
                    txt.HorizontalAlignment = HorizontalAlignment.Center;
                    txt.VerticalAlignment = VerticalAlignment.Center;

                    txt = new TextBlock();
                    txt.Text = listCr.ElementAt(i);
                    mtxGrid.Children.Add(txt);
                    Grid.SetRow(txt, i + 1);
                    Grid.SetColumn(txt, 0);
                    txt.HorizontalAlignment = HorizontalAlignment.Center;
                    txt.VerticalAlignment = VerticalAlignment.Center;
                }


                if (i > 0)
                {
                    txt = new TextBlock();
                    txt.Text = "1";
                    mtxGrid.Children.Add(txt);
                    Grid.SetRow(txt, i);
                    Grid.SetColumn(txt, i);
                    txt.HorizontalAlignment = HorizontalAlignment.Center;
                    txt.VerticalAlignment = VerticalAlignment.Center;
                }


                // starting from first line;
                if (i > 0 && i < listCr.Count)
                {
                    for (j = i + 1; j < listCr.Count + 1; j++)
                    {
                        StackPanel sp = new StackPanel();
                        sp.Orientation = Orientation.Horizontal;

                        TextBox tb = new TextBox();
                        int idx = Array.FindIndex(dblValues, z => z == mtx[i - 1, j - 1]);
                        tb.Text = strValues[idx];
                        //tb.Margin = new Thickness(5,5,0,5);
                        tb.Width = 3 * gridWidth / 5;
                        tb.Name = "txtNum" + (i - 1).ToString() + (j - 1).ToString();
                        RegisterName(tb.Name, tb);
                        tb.VerticalContentAlignment = VerticalAlignment.Center;

                        Button btnUp = new Button();
                        btnUp.Content = "+";
                        //btnUp.Margin = new Thickness(0, 10, 0, 0);
                        //btnUp.Width = 20;
                        btnUp.Name = "cmdUp" + (i - 1).ToString() + (j - 1).ToString();
                        btnUp.Click += new RoutedEventHandler(cmdUp_Click);
                        //RegisterName(btnUp.Name, btnUp);

                        Button btnDn = new Button();
                        btnDn.Content = "-";
                        //btnDn.Margin = new Thickness(0, 0, 0, 10);
                        //btnDn.Width = 20;
                        btnDn.Name = "cmdDn" + (i - 1).ToString() + (j - 1).ToString();
                        btnDn.Click += new RoutedEventHandler(cmdDn_Click);
                        //RegisterName(btnDn.Name, btnUp);

                        StackPanel sp1 = new StackPanel();
                        sp1.Orientation = Orientation.Vertical;
                        sp1.Children.Add(btnUp);
                        sp1.Children.Add(btnDn);

                        sp.Children.Add(tb);
                        sp.Children.Add(sp1);

                        mtxGrid.Children.Add(sp);
                        Grid.SetRow(sp, i);
                        Grid.SetColumn(sp, j);
                        sp.HorizontalAlignment = HorizontalAlignment.Center;
                        sp.VerticalAlignment = VerticalAlignment.Center;

                        txt = new TextBlock();
                        idx = Array.FindIndex(dblValues, z => z == mtx[j - 1, i - 1]);
                        txt.Text = strValues[idx];
                        txt.Name = "txtBlk" + (j - 1).ToString() + (i - 1).ToString();
                        RegisterName(txt.Name, txt);
                        mtxGrid.Children.Add(txt);
                        Grid.SetRow(txt, j);
                        Grid.SetColumn(txt, i);
                        txt.HorizontalAlignment = HorizontalAlignment.Center;
                        txt.VerticalAlignment = VerticalAlignment.Center;
                    }
                }
            }

        }

        // draw Alternative vs. criteria matrix
        private void DrawACCompMtx()
        {
            int i, j;
            bdrAC.Visibility = Visibility.Visible;
            //this.mtxGrid.Visibility = Visibility.Visible;
            grdAC.Children.Clear();
            grdAC.RowDefinitions.Clear();
            grdAC.ColumnDefinitions.Clear();

            if (listCr.Count == 0 || listAlt.Count == 0)
                return;

            if (mtxAC != null && isOpeningPrj == false)
            {
                // unregister all the element names
                
                for (i = 0; i < mtxAC.GetLength(1) ; i++)
                {
                    for (j = 0; j < mtxAC.GetLength(0); j++)
                    {
                        if(FindName("txtScore" + j.ToString() + i.ToString()) != null)
                            UnregisterName("txtScore" + j.ToString() + i.ToString());
                    }
                }                    
                    
            }

            if (mtxAC == null || mtxAC.Length != listCr.Count * listAlt.Count)
            {
                mtxAC = new int[listCr.Count, listAlt.Count];
                for (i = 0; i < listCr.Count; i++)
                    for (j = 0; j < listAlt.Count; j++)
                        mtxAC[i, j] = 0;
            }


            double gridWidth = 90;
            double gridHeight = 60;
            bdrAC.HorizontalAlignment = HorizontalAlignment.Center;
            bdrAC.VerticalAlignment = VerticalAlignment.Center;
            double x = tabPwC.ActualWidth;
            double y = tabPwC.ActualHeight;
            if (x >= (listAlt.Count + 1) * gridWidth)
            {
                for (i = 0; i < listCr.Count + 1; i++)
                {
                    RowDefinition row = new RowDefinition();
                    row.Height = new GridLength(gridHeight);

                    grdAC.RowDefinitions.Add(row);
                }
                bdrAC.Height = (listCr.Count + 1) * gridHeight;

                for (i = 0; i < listAlt.Count + 1; i++)
                {
                    ColumnDefinition col = new ColumnDefinition();
                    col.Width = new GridLength(gridWidth);

                    grdAC.ColumnDefinitions.Add(col);
                }
                bdrAC.Width = (listAlt.Count + 1) * gridWidth;
            }
            else
            {
                gridWidth = (x - 10) / (listAlt.Count + 1);
                gridHeight = gridWidth * 2 / 3;
                for (i = 0; i < listCr.Count + 1; i++)
                {
                    RowDefinition row = new RowDefinition();
                    row.Height = new GridLength(gridHeight);

                    grdAC.RowDefinitions.Add(row);
                }
                bdrAC.Height = (x - 10) * 2 / 3;

                for (i = 0; i < listCr.Count + 1; i++)
                {
                    ColumnDefinition col = new ColumnDefinition();
                    col.Width = new GridLength(gridWidth);

                    grdAC.ColumnDefinitions.Add(col);
                }
                bdrAC.Width = x - 10;
            }


            // redraw matrix
            // fill criteria headers & related weights
            for (i = 0; i < listCr.Count; i++)
            {
                // criteria
                TextBlock txt;
                txt = new TextBlock();
                txt.Text = listCr.ElementAt(i);
                grdAC.Children.Add(txt);
                Grid.SetRow(txt, i + 1);
                Grid.SetColumn(txt, 0);
                txt.HorizontalAlignment = HorizontalAlignment.Center;
                txt.VerticalAlignment = VerticalAlignment.Center;
            }

            // fill all the alternative names
            for (i = 0; i < listAlt.Count; i++)
            {
                TextBlock txt = new TextBlock();
                txt.Text = listAlt.ElementAt(i);
                grdAC.Children.Add(txt);
                Grid.SetRow(txt, 0);
                Grid.SetColumn(txt, i + 1);
                txt.HorizontalAlignment = HorizontalAlignment.Center;
                txt.VerticalAlignment = VerticalAlignment.Center;

                // fill scores
                for(j = 0; j < listCr.Count; j++)
                {
                    // score
                    TextBox tb = new TextBox();
                    tb.Text = mtxAC[j, i].ToString();
                    tb.Name = "txtScore" + j.ToString() + i.ToString();
                    RegisterName(tb.Name, tb);
                    tb.VerticalContentAlignment = VerticalAlignment.Center;
                    tb.VerticalAlignment = VerticalAlignment.Center;
                    tb.HorizontalAlignment = HorizontalAlignment.Center;
                    tb.Width = 4*gridWidth/5;
                    tb.Height = 2 * tb.Width / 3;
                    tb.TextChanged += new TextChangedEventHandler(txtScore_TextChanged);
                    tb.GotFocus += new RoutedEventHandler(txtScore_GotFocus);
                    tb.MaxLength = 3;
                    grdAC.Children.Add(tb);
                    Grid.SetRow(tb, j + 1);
                    Grid.SetColumn(tb, i + 1);
                }
            }
        }

        // draw Result matrix
        private void DrawResultMtx()
        {
            int i, j;
            bdrRst.Visibility = Visibility.Visible;
            //this.mtxGrid.Visibility = Visibility.Visible;
            grdRst.Children.Clear();
            grdRst.RowDefinitions.Clear();
            grdRst.ColumnDefinitions.Clear();

            if (listCr.Count == 0 || listAlt.Count == 0)
                return;

            // if the criteria not evaluated, evaluate it.
            stctResult = ahp_eval(mtx);

            double gridWidth = 90;
            double gridHeight = 60;
            bdrRst.HorizontalAlignment = HorizontalAlignment.Center;
            bdrRst.VerticalAlignment = VerticalAlignment.Center;
            double x = tabMain.ActualWidth;
            double y = tabMain.ActualHeight;
            if (x >= 2 * (listAlt.Count + 1) * gridWidth)
            {
                for (i = 0; i < listCr.Count + 3; i++)
                {
                    RowDefinition row = new RowDefinition();
                    row.Height = new GridLength(gridHeight);

                    grdRst.RowDefinitions.Add(row);
                }
                bdrRst.Height = (listCr.Count + 3) * gridHeight;

                for (i = 0; i < 2 * listAlt.Count + 2; i++)
                {
                    ColumnDefinition col = new ColumnDefinition();
                    col.Width = new GridLength(gridWidth);

                    grdRst.ColumnDefinitions.Add(col);
                }
                bdrRst.Width = 2 * (listAlt.Count + 1) * gridWidth;
            }
            else
            {
                gridWidth = (x - 10) / (listAlt.Count + 1) / 2;
                gridHeight = gridWidth * 2 / 3;
                for (i = 0; i < listCr.Count + 3; i++)
                {
                    RowDefinition row = new RowDefinition();
                    row.Height = new GridLength(gridHeight);

                    grdRst.RowDefinitions.Add(row);
                }
                bdrRst.Height = (x - 10) * 2 / 3;

                for (i = 0; i < 2 * listCr.Count + 2; i++)
                {
                    ColumnDefinition col = new ColumnDefinition();
                    col.Width = new GridLength(gridWidth);

                    grdRst.ColumnDefinitions.Add(col);
                }
                bdrRst.Width = x - 10;
            }


            // redraw matrix
            // fill criteria headers & related weights
            for (i = 0; i < listCr.Count; i++)
            {
                // criteria
                TextBlock txt;
                txt = new TextBlock();
                txt.Text = listCr.ElementAt(i);
                grdRst.Children.Add(txt);
                Grid.SetRow(txt, i + 2);
                Grid.SetColumn(txt, 0);
                txt.HorizontalAlignment = HorizontalAlignment.Center;
                txt.VerticalAlignment = VerticalAlignment.Center;

                // weight
                txt = new TextBlock();
                txt.Text = String.Format("{0:#.000}", stctResult.w[i]);
                grdRst.Children.Add(txt);
                Grid.SetRow(txt, i + 2);
                Grid.SetColumn(txt, 1);
                txt.HorizontalAlignment = HorizontalAlignment.Center;
                txt.VerticalAlignment = VerticalAlignment.Center;
            }

            TextBlock txtW = new TextBlock();
            txtW.Text = "weitht";
            grdRst.Children.Add(txtW);
            Grid.SetRow(txtW, 1);
            Grid.SetColumn(txtW, 1);
            txtW.HorizontalAlignment = HorizontalAlignment.Center;
            txtW.VerticalAlignment = VerticalAlignment.Center;


            // fill all the alternative names
            for (i = 0; i < listAlt.Count; i++)
            {
                TextBlock txt = new TextBlock();
                txt.Text = listAlt.ElementAt(i);
                grdRst.Children.Add(txt);
                Grid.SetRow(txt, 0);
                Grid.SetColumn(txt, 2 * (i + 1));
                Grid.SetColumnSpan(txt, 2);
                txt.HorizontalAlignment = HorizontalAlignment.Center;
                txt.VerticalAlignment = VerticalAlignment.Center;

                txt = new TextBlock();
                txt.Text = "Score";
                grdRst.Children.Add(txt);
                Grid.SetRow(txt, 1);
                Grid.SetColumn(txt, 2 * (i + 1));
                txt.HorizontalAlignment = HorizontalAlignment.Center;
                txt.VerticalAlignment = VerticalAlignment.Center;

                txt = new TextBlock();
                txt.Text = "Function";
                grdRst.Children.Add(txt);
                Grid.SetRow(txt, 1);
                Grid.SetColumn(txt, 2 * (i + 1) + 1);
                txt.HorizontalAlignment = HorizontalAlignment.Center;
                txt.VerticalAlignment = VerticalAlignment.Center;

                double sumFunc = 0;

                // fill scores and functions
                for (j = 0; j < listCr.Count; j++)
                {
                    // score
                    TextBlock tb = new TextBlock();
                    tb.Text = mtxAC[j, i].ToString();
                    tb.Name = "txtScore" + j.ToString() + i.ToString();
                    tb.VerticalAlignment = VerticalAlignment.Center;
                    tb.HorizontalAlignment = HorizontalAlignment.Center;
                    grdRst.Children.Add(tb);
                    Grid.SetRow(tb, j + 2);
                    Grid.SetColumn(tb, 2 * i + 2);

                    // function
                    TextBlock tblk = new TextBlock();
                    tblk.Text = String.Format("{0:#.000}", (mtxAC[j, i] * stctResult.w[j]));
                    tblk.Name = "txtFunc" + j.ToString() + i.ToString();
                    tblk.VerticalAlignment = VerticalAlignment.Center;
                    tblk.HorizontalAlignment = HorizontalAlignment.Center;
                    grdRst.Children.Add(tblk);
                    Grid.SetRow(tblk, j + 2);
                    Grid.SetColumn(tblk, 2 * i + 3);

                    sumFunc += mtxAC[j, i] * stctResult.w[j];
                }

                // fill function sum
                TextBlock tblk1 = new TextBlock();
                tblk1.Text = String.Format("{0:#.000}", sumFunc); 
                tblk1.Name = "txtFuncSum" + i.ToString();
                tblk1.VerticalAlignment = VerticalAlignment.Center;
                tblk1.HorizontalAlignment = HorizontalAlignment.Center;
                grdRst.Children.Add(tblk1);
                Grid.SetRow(tblk1, listCr.Count + 2);
                Grid.SetColumn(tblk1, 2 * i + 3);
            }
        }

        private void evalButton_Click(object sender, RoutedEventArgs e)
        {
            if (mtx == null)
            {
                MessageBox.Show("criterias compare matrix has not been set!");
                return;
            }
            stctResult = ahp_eval(mtx);

            string weights = "\n";
            for (int i = 0; i < listCr.Count; i++)
                weights += "w(" + listCr[i] + ") = " + String.Format("{0:0.000000}", stctResult.w[i]) + "; ";

            string rslt = "CR = ";
            rslt += String.Format("{0:0.000000}", stctResult.CR);
            rslt += "\n";
            txtEvalRslt.Inlines.Clear();
            if (stctResult.CR < 0.1)
            {
                BitmapImage MyImageSource = new BitmapImage(new Uri("pack://application:,,,/Resources/glad.png"));

                Image image = new Image();
                image.Source = MyImageSource;
                image.Width = 20;
                image.Height = 20;
                image.Visibility = Visibility.Visible;
                InlineUIContainer container = new InlineUIContainer(image);

                rslt += "good!";
                txtEvalRslt.Inlines.Add(rslt);
                txtEvalRslt.Inlines.Add(container);
                txtEvalRslt.Inlines.Add(weights);
                txtEvalRslt.Foreground = Brushes.Green;
            }
            else
            {
                BitmapImage MyImageSource = new BitmapImage(new Uri("pack://application:,,,/Resources/sad.png"));

                Image image = new Image();
                image.Source = MyImageSource;
                image.Width = 20;
                image.Height = 20;
                image.Visibility = Visibility.Visible;
                InlineUIContainer container = new InlineUIContainer(image);

                txtEvalRslt.Inlines.Add(rslt);
                txtEvalRslt.Inlines.Add(container);
                txtEvalRslt.Inlines.Add(weights);
                txtEvalRslt.Foreground = Brushes.Red;
            }
        }

        //draw AHP Hierarchy
        private void drawAhpHierarchy()
        {
            double cvsWidth = this.cvsHrcGraph.ActualWidth;
            double cvsHeight = this.cvsHrcGraph.ActualHeight;

            //clear the canvas and redraw.
            this.cvsHrcGraph.Children.Clear();

            // 1/5 fill words
            TextBlock txt = new TextBlock();
            txt.Text = "Goal:";
            Canvas.SetLeft(txt, 5);
            Canvas.SetTop(txt, cvsHeight / 6);
            this.cvsHrcGraph.Children.Add(txt);



            txt = new TextBlock();
            txt.Text = "Criteria:";
            Canvas.SetLeft(txt, 5);
            Canvas.SetTop(txt, cvsHeight / 2);
            this.cvsHrcGraph.Children.Add(txt);

            txt = new TextBlock();
            txt.Text = "Alternatives:";
            Canvas.SetLeft(txt, 5);
            Canvas.SetTop(txt, cvsHeight * 5 / 6);
            this.cvsHrcGraph.Children.Add(txt);

            DropShadowEffect dse = new DropShadowEffect();
            dse.BlurRadius = 4;
            dse.ShadowDepth = 10;
            dse.Color = Colors.Silver;

            // 4/5 fill graphics
            // goal rectangle
            Rectangle rec;
            if (this.txbGoal.Text != "")
            {
                rec = new Rectangle();
                rec.Width = cvsWidth * 4 / 5 / 4;
                rec.Height = rec.Width * 2 / 3;
                rec.Fill = Brushes.YellowGreen;
                rec.Effect = dse;
                Canvas.SetLeft(rec, cvsWidth / 5 + (cvsWidth * 4 / 5 - rec.Width) / 2);
                Canvas.SetTop(rec, (cvsHeight / 3 - rec.Height) / 2);
                this.cvsHrcGraph.Children.Add(rec);

                txt = new TextBlock();
                txt.Text = this.txbGoal.Text;
                txt.VerticalAlignment = VerticalAlignment.Center;
                txt.HorizontalAlignment = HorizontalAlignment.Center;
                txt.TextWrapping = TextWrapping.Wrap;

                Grid grd = new Grid();
                grd.Width = rec.Width - 2;
                grd.Height = rec.Height - 2;
                grd.Children.Add(txt);                

                Canvas.SetTop(grd, Canvas.GetTop(rec) + 1);
                Canvas.SetLeft(grd, Canvas.GetLeft(rec) + 1);
                this.cvsHrcGraph.Children.Add(grd);
            }
            
            if (listCr.Count != 0)
            {
                double crWidth = cvsWidth * 4 / 5 / listCr.Count * 4 / 5;
                double crHeight = crWidth * 2 / 3;
                for (int i = 0; i < listCr.Count; i++)
                {
                    rec = new Rectangle();
                    rec.HorizontalAlignment = HorizontalAlignment.Center;
                    rec.VerticalAlignment = VerticalAlignment.Center;

                    rec.Width = crWidth;
                    rec.Height = crHeight;
                    if (crHeight > cvsHeight / 3 / 2)
                        rec.Height = cvsHeight / 3 / 2;
                    rec.Fill = Brushes.Gold;
                    rec.Effect = dse;
                    Canvas.SetLeft(rec, cvsWidth / 5 + cvsWidth * 4 / 5 / listCr.Count * i + (cvsWidth * 4 / 5 / listCr.Count - crWidth) / 2);
                    Canvas.SetTop(rec, (cvsHeight / 3 - rec.Height) / 2 + cvsHeight / 3);
                    this.cvsHrcGraph.Children.Add(rec);

                    txt = new TextBlock();
                    txt.Text = listCr[i];
                    txt.VerticalAlignment = VerticalAlignment.Center;
                    txt.HorizontalAlignment = HorizontalAlignment.Center;
                    txt.TextWrapping = TextWrapping.Wrap;

                    Grid grd = new Grid();
                    grd.Width = rec.Width - 2;
                    grd.Height = rec.Height - 2;
                    grd.Children.Add(txt);

                    Canvas.SetTop(grd, Canvas.GetTop(rec) + 1);
                    Canvas.SetLeft(grd, Canvas.GetLeft(rec) + 1);
                    this.cvsHrcGraph.Children.Add(grd);
                }
            }



            if (listAlt.Count != 0)
            {
                double alWidth = cvsWidth * 4 / 5 / listAlt.Count * 4 / 5;
                double alHeight = alWidth * 2 / 3;
                for (int i = 0; i < listAlt.Count; i++)
                {
                    rec = new Rectangle();
                    rec.HorizontalAlignment = HorizontalAlignment.Center;
                    rec.VerticalAlignment = VerticalAlignment.Center;
                    rec.Width = alWidth;
                    rec.Height = alHeight;
                    if (alHeight > cvsHeight / 3 / 2)
                        rec.Height = cvsHeight / 3 / 2;
                    rec.Fill = Brushes.Pink;
                    rec.Effect = dse;
                    Canvas.SetLeft(rec, cvsWidth / 5 + cvsWidth * 4 / 5 / listAlt.Count * i + (cvsWidth * 4 / 5 / listAlt.Count - alWidth) / 2);
                    Canvas.SetTop(rec, (cvsHeight / 3 - rec.Height) / 2 + cvsHeight * 2 / 3);
                    this.cvsHrcGraph.Children.Add(rec);

                    txt = new TextBlock();
                    txt.Text = listAlt[i];
                    txt.VerticalAlignment = VerticalAlignment.Center;
                    txt.HorizontalAlignment = HorizontalAlignment.Center;
                    txt.TextWrapping = TextWrapping.Wrap;

                    Grid grd = new Grid();
                    grd.Width = rec.Width - 2;
                    grd.Height = rec.Height - 2;
                    grd.Children.Add(txt);

                    Canvas.SetTop(grd, Canvas.GetTop(rec) + 1);
                    Canvas.SetLeft(grd, Canvas.GetLeft(rec) + 1);
                    this.cvsHrcGraph.Children.Add(grd);
                }
            }
        }

        private void cvsHrcGraph_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            drawAhpHierarchy();

        }

        private void txbGoal_TextChanged(object sender, TextChangedEventArgs e)
        {
            drawAhpHierarchy();
        }

        private void txtScore_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox txb = sender as TextBox;
            txb.SelectAll();
        }

        private void txtScore_TextChanged(object sender, TextChangedEventArgs e)
    {
            // get new score, calculate the function and sum function
            TextBox txb = sender as TextBox;
            if (txb == null) return;

            int rowN = Convert.ToInt32(txb.Name.Substring(8,1));
            int colN = Convert.ToInt32(txb.Name.Substring(9, 1));

            try
            {
                int value = Convert.ToInt32(txb.Text);
                if (value > 100)
                {
                    MessageBox.Show("Score should between 0 ~ 100.");
                    txb.Text = mtxAC[rowN, colN].ToString();
                    return;
                }                    
                mtxAC[rowN, colN] = value;
            }
            catch (FormatException)
            {
                //MessageBox.Show("The value " + txb.Text + " is not in a recognizable format.");
                txb.Text = mtxAC[rowN, colN].ToString();
            }
    }

        private void tabPwC_Loaded(object sender, RoutedEventArgs e)
        {
            if (tabPwC.SelectedIndex == 0)
                DrawCriteriaCompMtx();
            else if (tabPwC.SelectedIndex == 1)
                DrawACCompMtx();
        }

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            // code needed here, verify all the result to save

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Project1";
            dlg.DefaultExt = ".ahp";
            dlg.Filter = "AHP project (.ahp)|*.ahp";
            if (dlg.ShowDialog() == true)
            {
                string filename = dlg.FileName;

                //delete .tmp file if there is one
                

                //create .ahp file and save
                CreatXml(filename);
            }
        }

        private void newbtn_Click(object sender, RoutedEventArgs e)
        {
            int i, j;

            // save current unsaved project
            if (listCr.Count != 0 || listAlt.Count != 0)
            {
                MessageBoxResult rlt = MessageBox.Show("the current project is not saved, do you want to save it now?", "warning", MessageBoxButton.YesNoCancel);
                if (MessageBoxResult.Yes == rlt)
                {
                    saveBtn_Click(sender, e);
                }
                else if (MessageBoxResult.Cancel == rlt)
                {
                    return;
                }
                else
                {

                }
            }

            // goal
            txbGoal.Text = "";

            // clear alternatives list view, then refill it
            contentLbxAl.Items.Clear();
            for (i = 0; i < listAlt.Count; i++)
            {
                Button btn = FindName("deleteBtnAl" + Convert.ToString(i)) as Button;
                if (btn != null)
                    UnregisterName(btn.Name);
            }
            listAlt.Clear();

            // clear criteria list view, then refill it
            contentLbxCr.Items.Clear();
            for (i = 0; i < listCr.Count; i++)
            {
                Button btn = FindName("deleteBtnCr" + Convert.ToString(i)) as Button;
                if (btn != null)
                    UnregisterName(btn.Name);
            }
            listCr.Clear();

            // criteria VS goal comparison mtx
            if (mtx != null)
            {
                // unregister all the element names
                for (i = 0; i < Math.Sqrt(mtx.Length) - 1; i++)
                    for (j = i + 1; j < Math.Sqrt(mtx.Length); j++)
                    {
                        if (null != FindName("txtNum" + i.ToString() + j.ToString()))
                            UnregisterName("txtNum" + i.ToString() + j.ToString());
                        if (null != FindName("txtBlk" + j.ToString() + i.ToString()))
                            UnregisterName("txtBlk" + j.ToString() + i.ToString());
                    }
            }
            mtx = null;

            // alternative vs criteria comparison mtx
            if (mtxAC != null)
            {
                // unregister all the element names
                for (i = 0; i < mtxAC.GetLength(1); i++)
                {
                    for (j = 0; j < mtxAC.GetLength(0); j++)
                    {
                        if (null != FindName("txtScore" + j.ToString() + i.ToString()))
                            UnregisterName("txtScore" + j.ToString() + i.ToString());
                    }
                }
            }
            mtxAC = null;

            // draw AHP Hierarchy Graphics
            drawAhpHierarchy();

        }

        private void openBtn_Click(object sender, RoutedEventArgs e)
        {
            // save current unsaved project
            if(listCr.Count != 0 || listAlt.Count != 0)
            {
                MessageBoxResult rlt = MessageBox.Show("the current project is not saved, do you want to save it now?", "warning", MessageBoxButton.YesNoCancel);
                if (MessageBoxResult.Yes == rlt)
                {
                    saveBtn_Click(sender, e);
                }
                else if(MessageBoxResult.Cancel == rlt)
                {
                    return;
                }
                else
                {

                }
            }

            // code needed here, verify all the result to save

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "Project1";
            dlg.DefaultExt = ".ahp";
            dlg.Filter = "AHP project (.ahp)|*.ahp";
            if (dlg.ShowDialog() == true)
            {
                string filename = dlg.FileName;
                //delete .tmp file if there is one


                //load .ahp file and update the view.
                LoadXml(filename);
            }
        }

        private void buildAllBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private static string CreateTmpFile()
        {
            string fileName = string.Empty;

            try
            {
                // Get the full name of the newly created Temporary file. 
                // Note that the GetTempFileName() method actually creates
                // a 0-byte file and returns the name of the created file.
                fileName = System.IO.Path.GetTempFileName();

                // Craete a FileInfo object to set the file's attributes
                FileInfo fileInfo = new FileInfo(fileName);

                // Set the Attribute property of this file to Temporary. 
                // Although this is not completely necessary, the .NET Framework is able 
                // to optimize the use of Temporary files by keeping them cached in memory.
                fileInfo.Attributes = FileAttributes.Temporary;
                //CreatXmlTree(fileName);

                Console.WriteLine("TEMP file created at: " + fileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to create TEMP file or set its attributes: " + ex.Message);
            }

            return fileName;
        }

        private static void UpdateTmpFile(string tmpFile)
        {
            try
            {
                // Write to the temp file.
                StreamWriter streamWriter = File.AppendText(tmpFile);
                streamWriter.WriteLine("Hello from www.daveoncsharp.com!");
                streamWriter.Flush();
                streamWriter.Close();

                Console.WriteLine("TEMP file updated.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error writing to TEMP file: " + ex.Message);
            }
        }

        private static void ReadTmpFile(string tmpFile)
        {
            try
            {
                // Read from the temp file.
                StreamReader myReader = File.OpenText(tmpFile);
                Console.WriteLine("TEMP file contents: " + myReader.ReadToEnd());
                myReader.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reading TEMP file: " + ex.Message);
            }
        }

        private static void DeleteTmpFile(string tmpFile)
        {
            try
            {
                // Delete the temp file (if it exists)
                if (File.Exists(tmpFile))
                {
                    File.Delete(tmpFile);
                    Console.WriteLine("TEMP file deleted.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleteing TEMP file: " + ex.Message);
            }
        }

        public void CreatXml(string xmlPath)
        {
            XmlDocument doc = new XmlDocument();

            XmlElement root = doc.CreateElement("Project");
            doc.AppendChild(root);

            // goal
            XmlElement element = doc.CreateElement("Goal");
            element.InnerText = this.txbGoal.Text;
            root.AppendChild(element);

            // alternatives
            XmlElement alternatives = doc.CreateElement("Alternatives");
            root.AppendChild(alternatives);

            for(int i = 0; i < listAlt.Count; i++)
            {
                element = doc.CreateElement("Alternative");
                element.InnerText = listAlt.ElementAt(i);

                XmlAttribute attr = doc.CreateAttribute("cost1");
                attr.Value = "1";
                element.Attributes.Append(attr);

                attr = doc.CreateAttribute("cost2");
                attr.Value = "1";
                element.Attributes.Append(attr);

                // next step
                attr = doc.CreateAttribute("finalscore");
                attr.Value = "10";
                element.Attributes.Append(attr);

                alternatives.AppendChild(element);
            }

            // criterias
            XmlElement criterias = doc.CreateElement("Criterias");
            root.AppendChild(criterias);

            for (int i = 0; i < listCr.Count; i++)
            {
                element = doc.CreateElement("Criteria");
                element.InnerText = listCr.ElementAt(i);

                XmlAttribute attr = doc.CreateAttribute("weight");
                attr.Value = stctResult.w[i].ToString();
                element.Attributes.Append(attr);

                criterias.AppendChild(element);
            }


            // criteria vs goal pairwise comparison
            XmlElement cvgs = doc.CreateElement("CriteriaVsGoalMatrix");
            root.AppendChild(cvgs);

            for(int i = 0; i < listCr.Count; i++)
                for(int j = 0; j < listCr.Count; j++)
                {
                    element = doc.CreateElement("CVGItem");
                    element.InnerText = mtx[i, j].ToString();

                    XmlAttribute attr = doc.CreateAttribute("index");
                    attr.Value = i.ToString() + j.ToString();
                    element.Attributes.Append(attr);

                    cvgs.AppendChild(element);
                }

            // alternative vs criteria pairwise comparison
            XmlElement avcs = doc.CreateElement("AlternativeVsGoalMatrix");
            root.AppendChild(avcs);

            for (int i = 0; i < listCr.Count; i++)
                for (int j = 0; j < listAlt.Count; j++)
                {
                    element = doc.CreateElement("AVCItem");
                    element.InnerText = mtxAC[i, j].ToString();

                    XmlAttribute attr = doc.CreateAttribute("index");
                    attr.Value = i.ToString() + j.ToString();
                    element.Attributes.Append(attr);

                    avcs.AppendChild(element);
                }

            doc.Save(xmlPath);
        }

        public void LoadXml(string xmlPath)
        {
            int i, j;

            isOpeningPrj = true;

            // load xml file
            XmlDocument doc = new XmlDocument();
            doc.Load(xmlPath);

            // goal
            XmlNode goal = doc.SelectSingleNode("descendant::Goal");
            txbGoal.Text = goal.InnerText;

            // clear alternatives list view, then refill it
            contentLbxAl.Items.Clear();
            for(i = 0; i < listAlt.Count; i++)
            {
                Button btn = FindName("deleteBtnAl" + Convert.ToString(i)) as Button;
                if(FindName(btn.Name) != null)
                    UnregisterName(btn.Name);
            }
            listAlt.Clear();

            XmlNode alternatives = doc.SelectSingleNode("descendant::Alternatives");
            if(alternatives.HasChildNodes)
            {
                for(i = 0; i < alternatives.ChildNodes.Count; i++)
                {
                    listAlt.Add(alternatives.ChildNodes[i].InnerText);

                    // add grid with delete button
                    ListBoxItem newItem = new ListBoxItem();

                    Grid newGrid = new Grid();
                    ColumnDefinition col1 = new ColumnDefinition();
                    ColumnDefinition col2 = new ColumnDefinition();
                    col1.Width = new GridLength(contentLbxCr.ActualWidth * 5 / 8);
                    col1.Width = new GridLength(contentLbxCr.ActualWidth * 3 / 8);
                    newGrid.ColumnDefinitions.Add(col1);
                    newGrid.ColumnDefinitions.Add(col2);

                    TextBlock newText = new TextBlock();
                    newText.Text = alternatives.ChildNodes[i].InnerText;
                    //newText.AddHandler(MouseDoubleClickEvent, new RoutedEventHandler(ListItem_DoubleClick));
                    newText.MouseDown += new MouseButtonEventHandler(ListItemAl_DoubleClick);

                    Button newBtn = new Button();
                    newBtn.Name = "deleteBtnAl" + contentLbxAl.Items.Count.ToString();
                    newBtn.Width = 22;
                    newBtn.Height = 22;
                    newBtn.Cursor = Cursors.Hand;
                    newBtn.ToolTip = "delete alternative";
                    newBtn.Click += new RoutedEventHandler(deleteBtnAl_OnClick);

                    ImageBrush newB = new ImageBrush();
                    newB.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Resources/delete1.png"));
                    newBtn.Background = newB;

                    newGrid.Children.Add(newText);
                    newGrid.Children.Add(newBtn);
                    Grid.SetColumn(newBtn, 1);

                    newItem.Content = newGrid;
                    contentLbxAl.Items.Add(newItem);
                    RegisterName(newBtn.Name, newBtn);
                }
            }


            // clear criteria list view, then refill it
            contentLbxCr.Items.Clear();
            for (i = 0; i < listCr.Count; i++)
            {
                Button btn = FindName("deleteBtnCr" + Convert.ToString(i)) as Button;
                if(FindName(btn.Name) != null)
                    UnregisterName(btn.Name);
            }
            listCr.Clear();

            XmlNode criterias = doc.SelectSingleNode("descendant::Criterias");
            if (criterias.HasChildNodes)
            {
                for (i = 0; i < criterias.ChildNodes.Count; i++)
                {
                    listCr.Add(criterias.ChildNodes[i].InnerText);

                    // add grid with delete button
                    ListBoxItem newItem = new ListBoxItem();

                    Grid newGrid = new Grid();
                    ColumnDefinition col1 = new ColumnDefinition();
                    ColumnDefinition col2 = new ColumnDefinition();
                    col1.Width = new GridLength(contentLbxCr.ActualWidth * 5 / 8);
                    col1.Width = new GridLength(contentLbxCr.ActualWidth * 3 / 8);
                    newGrid.ColumnDefinitions.Add(col1);
                    newGrid.ColumnDefinitions.Add(col2);

                    TextBlock newText = new TextBlock();
                    newText.Text = criterias.ChildNodes[i].InnerText;
                    //newText.AddHandler(MouseDoubleClickEvent, new RoutedEventHandler(ListItem_DoubleClick));
                    newText.MouseDown += new MouseButtonEventHandler(ListItemCr_DoubleClick);

                    Button newBtn = new Button();
                    newBtn.Name = "deleteBtnCr" + contentLbxCr.Items.Count.ToString();
                    newBtn.Width = 22;
                    newBtn.Height = 22;
                    newBtn.Cursor = Cursors.Hand;
                    newBtn.ToolTip = "delete criteria";
                    newBtn.Click += new RoutedEventHandler(deleteBtnAl_OnClick);

                    ImageBrush newB = new ImageBrush();
                    newB.ImageSource = new BitmapImage(new Uri("pack://application:,,,/Resources/delete1.png"));
                    newBtn.Background = newB;

                    newGrid.Children.Add(newText);
                    newGrid.Children.Add(newBtn);
                    Grid.SetColumn(newBtn, 1);

                    newItem.Content = newGrid;
                    contentLbxCr.Items.Add(newItem);
                    RegisterName(newBtn.Name, newBtn);
                }
            }

            // criteria VS goal comparison mtx
            if (mtx != null)
            {
                // unregister all the element names
                for (i = 0; i < Math.Sqrt(mtx.Length) - 1; i++)
                    for (j = i + 1; j < Math.Sqrt(mtx.Length); j++)
                    {
                        if(null != FindName("txtNum" + i.ToString() + j.ToString()))
                            UnregisterName("txtNum" + i.ToString() + j.ToString());
                        if(null != FindName("txtBlk" + j.ToString() + i.ToString()))
                            UnregisterName("txtBlk" + j.ToString() + i.ToString());
                    }
            }

            XmlNode cvgs = doc.SelectSingleNode("descendant::CriteriaVsGoalMatrix");
            mtx = new double[criterias.ChildNodes.Count, criterias.ChildNodes.Count];
            for(i = 0; i < criterias.ChildNodes.Count; i++)
                for(j = 0; j < criterias.ChildNodes.Count; j++)
                {
                    mtx[i, j] = Convert.ToDouble(cvgs.ChildNodes[i*criterias.ChildNodes.Count + j].InnerText);
                }

            // alternative vs criteria comparison mtx
            if (mtxAC != null)
            {
                // unregister all the element names
                for (i = 0; i < mtxAC.GetLength(1); i++)
                {
                    for (j = 0; j < mtxAC.GetLength(0); j++)
                    {
                        if(null != FindName("txtScore" + j.ToString() + i.ToString()))
                            UnregisterName("txtScore" + j.ToString() + i.ToString());
                    }
                }

            }

            XmlNode avgs = doc.SelectSingleNode("descendant::AlternativeVsGoalMatrix");
            mtxAC = new int[criterias.ChildNodes.Count, alternatives.ChildNodes.Count];
            for (i = 0; i < criterias.ChildNodes.Count; i++)
                for (j = 0; j < alternatives.ChildNodes.Count; j++)
                {
                    mtxAC[i, j] = Convert.ToInt32(avgs.ChildNodes[i * alternatives.ChildNodes.Count + j].InnerText);
                }

            // draw AHP Hierarchy Graphics
            drawAhpHierarchy();

            isOpeningPrj = false ;
        }
    }

    class XmlOperation
    {
        public void Create(string xmlPath)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlPath);
            var root = xmlDoc.DocumentElement;//取到根结点

            XmlNode newNode = xmlDoc.CreateNode("element", "Name", "");
            newNode.InnerText = "Zery";

            //添加为根元素的第一层子结点
            root.AppendChild(newNode);
            xmlDoc.Save(xmlPath);
        }
        //属性
        public void CreateAttribute(string xmlPath)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlPath);
            XmlElement node = (XmlElement)xmlDoc.SelectSingleNode("Collection/Book");
            node.SetAttribute("Name", "C#");
            xmlDoc.Save(xmlPath);
        }

        public void Delete(string xmlPath)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlPath);
            var root = xmlDoc.DocumentElement;//取到根结点

            var element = xmlDoc.SelectSingleNode("Collection/Name");
            root.RemoveChild(element);
            xmlDoc.Save(xmlPath);
        }

        public void DeleteAttribute(string xmlPath)
        {

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlPath);
            XmlElement node = (XmlElement)xmlDoc.SelectSingleNode("Collection/Book");
            //移除指定属性
            node.RemoveAttribute("Name");
            //移除当前节点所有属性，不包括默认属性
            node.RemoveAllAttributes();

            xmlDoc.Save(xmlPath);

        }

        public void Modify(string xmlPath)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlPath);
            var root = xmlDoc.DocumentElement;//取到根结点
            XmlNodeList nodeList = xmlDoc.SelectNodes("/Collection/Book");
            //xml不能直接更改结点名称，只能复制然后替换，再删除原来的结点
            foreach (XmlNode node in nodeList)
            {
                var xmlNode = (XmlElement)node;
                xmlNode.SetAttribute("ISBN", "Zery");
            }
            xmlDoc.Save(xmlPath);

        }

        public void ModifyAttribute(string xmlPath)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlPath);
            XmlElement element = (XmlElement)xmlDoc.SelectSingleNode("Collection/Book");
            element.SetAttribute("Name", "Zhang");
            xmlDoc.Save(xmlPath);

        }

        public void Select(string xmlPath)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlPath);
            //取根结点
            var root = xmlDoc.DocumentElement;//取到根结点
                                              //取指定的单个结点
            XmlNode singleNode = xmlDoc.SelectSingleNode("Collection/Book");

            //取指定的结点的集合
            XmlNodeList nodes = xmlDoc.SelectNodes("Collection/Book");

            //取到所有的xml结点
            XmlNodeList nodelist = xmlDoc.GetElementsByTagName("*");
        }

        public void SelectAttribute(string xmlPath)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlPath);
            XmlElement element = (XmlElement)xmlDoc.SelectSingleNode("Collection/Book");
            string name = element.GetAttribute("Name");

        }
    }

    struct StructEvalResult
    {
        public double[] w;
        public double CR;
    }
}
