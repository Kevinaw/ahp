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

using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Windows.Controls.Ribbon;
using System.Windows.Threading;

namespace ahp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        //string tmpFile;
        private Boolean isEditingAl = false;
        private int editingItemIndexAl;
        private Boolean isEditingCr = false;
        private int editingItemIndexCr;

        List<string> listAlt = new List<string>();
        List<string> listCr = new List<string>();

        private double[,] mtx;
        private static string[] strValues = {"1/9", "1/8", "1/7", "1/6", "1/5", "1/4", "1/3", "1/2", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        private static double[] dblValues = { 0.1111111, 0.125, 0.1428571, 0.1666666, 0.2, 0.25, 0.3333333, 0.5, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

        public MainWindow()
        {
            InitializeComponent();
            showChart();

            // when software starts up, new project file is created (temporary file)
            // also, xml structure is set up.
            //tmpFile = CreateTmpFile();
            //UpdateTmpFile(tmpFile);
            //ReadTmpFile(tmpFile);
            //DeleteTmpFile(tmpFile);
        }

        private void comMatrixButton_Click(object sender, RoutedEventArgs e)
        {
            if (comMatrixButton.Background != comMatrixButton.MouseOverBackground)
            {
                int i, j;
                comMatrixButton.Background = comMatrixButton.MouseOverBackground;
                this.criGpBx.Visibility = Visibility.Hidden;
                this.mtxBdr.Visibility = Visibility.Visible;
                //this.mtxGrid.Visibility = Visibility.Visible;
                this.mtxGrid.Children.Clear();
                this.mtxGrid.RowDefinitions.Clear();
                this.mtxGrid.ColumnDefinitions.Clear();

                if (listCr.Count == 0)
                    return;

                if(mtx != null)
                {
                    // unregister all the element names
                    for (i = 0; i < Math.Sqrt(mtx.Length) - 1; i++)
                        for (j = i + 1; j < Math.Sqrt(mtx.Length); j++)
                        {
                            UnregisterName("txtNum" + i.ToString() + j.ToString());
                            UnregisterName("txtBlk" + j.ToString() + i.ToString());
                        }
                }

                if (mtx == null)
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
                else // no change to criteria
                    ;


                double gridWidth = 90;
                double gridHeight = 60;
                (mtxGrid.Parent as Border).HorizontalAlignment = HorizontalAlignment.Center;
                (mtxGrid.Parent as Border).VerticalAlignment = VerticalAlignment.Center;
                double x = MainGrid.ActualWidth;
                double y = MainGrid.ActualHeight;
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
                    gridWidth = (x - 10)/ (listCr.Count + 1);
                    gridHeight = gridWidth*2/3;
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
                    (mtxGrid.Parent as Border).Height = (x - 10)*2/3;

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
                    

                    if(i > 0)
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
                        for(j = i + 1; j < listCr.Count + 1; j++)
                        {
                            StackPanel sp = new StackPanel();
                            sp.Orientation = Orientation.Horizontal;

                            TextBox tb = new TextBox();
                            int idx = Array.FindIndex(dblValues, z => z == mtx[i - 1, j - 1]);
                            tb.Text = strValues[idx];
                            //tb.Margin = new Thickness(5,5,0,5);
                            tb.Width = 3*gridWidth/5;
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
            else
            {
                comMatrixButton.Background = Brushes.Transparent;
                this.criGpBx.Visibility = Visibility.Visible;
                this.mtxBdr.Visibility = Visibility.Hidden;
            }
        }

        private void evalButton_Click(object sender, RoutedEventArgs e)
        {
            if (mtx == null)
            {
                MessageBox.Show("criterias compare matrix has not been set!");
                return;
            }                
            StructEvalResult res = ahp_eval(mtx);
            txbCR.Text = String.Format("{0:0.000000}", res.CR);
            if (res.CR < 0.1)
            {
                imgCR.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/glad.png"));
                txbCR.Foreground = Brushes.Green;
            }
            else
            {
                imgCR.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/sad.png"));
                txbCR.Foreground = Brushes.Red;
            }
        }

        private void showChart()
        {
            List<KeyValuePair<string, int>> MyValue = new List<KeyValuePair<string, int>>();
            MyValue.Add(new KeyValuePair<string, int>("Alternative1", 10));
            MyValue.Add(new KeyValuePair<string, int>("Alternative3", 30));
            MyValue.Add(new KeyValuePair<string, int>("Alternative2", 60));

            BarChart1.DataContext = MyValue;
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

        public void CreatXmlTree(string xmlPath)
        {
            XElement xElement = new XElement(
                new XElement("Project",
                    new XElement("Name"),
                    new XElement("Goal"),
                    new XElement("Description"),
                    new XElement("Alternatives",
                         new XElement("AlternativesItem",
                            new XElement("Name", "Alternative1"),
                            new XElement("Description")
                        ),
                        new XElement("AlternativesItem",
                            new XElement("Name", "Alternative2"),
                            new XElement("Description")
                        ),
                        new XElement("AlternativesItem", 
                            new XElement("Name", "Alternative3"),
                            new XElement("Description")
                        )
                    ),
                    new XElement("Criteria",
                        new XElement("CriteriaItem",
                            new XElement("Name", "Criteria1"),
                            new XElement("Description")
                        ),
                        new XElement("CriteriaItem",
                            new XElement("Name", "Criteria2"),
                            new XElement("Description")
                        ),
                        new XElement("CriteriaItem",
                            new XElement("Name", "Criteria3"),
                            new XElement("Description")
                        )
                    ),
                    new XElement("Results")));
 
             //需要指定编码格式，否则在读取时会抛：根级别上的数据无效。 第 1 行 位置 1异常
             XmlWriterSettings settings = new XmlWriterSettings();
             settings.Encoding = new UTF8Encoding(false);
             settings.Indent = true;
             XmlWriter xw = XmlWriter.Create(xmlPath, settings);
             xElement.Save(xw);
             //写入文件
             xw.Flush();
             xw.Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
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
                    if (listAlt.Exists(x=>x==lastItem.Text))
                    {
                        MessageBox.Show("duplicate alternative name!");
                        return;
                    }
                    listAlt.Add(lastItem.Text);
                        

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

                if (listAlt.Exists(x => x == txt.Text))
                {
                    MessageBox.Show("duplicate alternative name!");
                    return;
                }
                //listAlt.Remove(txt.Text);
                listAlt.Add(txt.Text);

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
                listAlt.Remove(txt.Text);
                // delete this item according the index
                contentLbxAl.Items.RemoveAt(idx);
                UnregisterName(btn.Name);

                if (idx != contentLbxAl.Items.Count)
                {
                    for (i = idx + 1; i < contentLbxAl.Items.Count + 1; i++)
                    {
                        btn1 = FindName("deleteBtnAl" + Convert.ToString(i)) as Button;
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

                if (listCr.Exists(x => x == txt.Text))
                {
                    MessageBox.Show("duplicate criteria name!");
                    return;
                }
                //listAlt.Remove(txt.Text);
                listCr.Add(txt.Text);

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
                listCr.Remove(txt.Text);
                // delete this item according the index
                contentLbxCr.Items.RemoveAt(idx);
                UnregisterName(btn.Name);

                if (idx != contentLbxCr.Items.Count)
                {
                    for (i = idx + 1; i < contentLbxCr.Items.Count + 1; i++)
                    {
                        btn1 = FindName("deleteBtnCr" + Convert.ToString(i)) as Button;
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

            if(idx == -1)
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
            int n = (int) Math.Sqrt(matrix.Length);
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

        //计算节点的特征向量
        private double[] normalize(double[][] matrix)
        {
            int row = matrix.Length;
            int column = matrix[0].Length;
            double[] Sum_column = new double[column];
            double[] w = new double[row];
            string normalizeType = "和法";

            if (normalizeType == "和法")
            {
                for (int i = 0; i < column; i++)
                {
                    Sum_column[i] = 0;
                    for (int j = 0; j < row; j++)
                    {
                        Sum_column[i] += matrix[j][i];
                    }
                }

                //进行归一化,计算特征向量W

                for (int i = 0; i < row; i++)
                {
                    w[i] = 0;
                    for (int j = 0; j < column; j++)
                    {
                        w[i] += matrix[i][j] / Sum_column[j];
                    }
                    w[i] /= row;
                }
            }

            if (normalizeType == "根法")
            {
                for (int i = 0; i < column; i++)
                {
                    Sum_column[i] = 0;
                    for (int j = 0; j < row; j++)
                    {
                        Sum_column[i] += matrix[j][i];
                    }
                }

                //进行归一化,计算特征向量W
                double sum = 0;
                for (int i = 0; i < row; i++)
                {
                    w[i] = 1;
                    for (int j = 0; j < column; j++)
                    {
                        w[i] *= matrix[i][j] / Sum_column[j];
                    }

                    w[i] = Math.Pow(w[i], 1.0 / row);
                    sum += w[i];
                }

                for (int i = 0; i < row; i++)
                {
                    w[i] /= sum;
                }
            }

            if (normalizeType == "幂法")
            {
                double[] w0 = new double[row];
                for (int i = 0; i < row; i++)
                {
                    w0[i] = 1.0 / row;
                }

                //一般向量W（k+1）
                double[] w1 = new double[row];
                //W（k+1）的归一化向量                
                double sum = 1.0;
                double d = 1.0;
                double delt = 0.00001;
                while (d > delt)
                {
                    d = 0.0;
                    sum = 0;

                    //获取向量
                    for (int j = 0; j < row; j++)
                    {
                        w1[j] = 0;
                        for (int k = 0; k < row; k++)
                        {
                            w1[j] += matrix[j][k] * w0[k];
                        }
                        sum += w1[j];
                    }

                    //向量归一化 
                    for (int k = 0; k < row; k++)
                    {
                        w[k] = w1[k] / sum;
                        d = Math.Max(Math.Abs(w[k] - w0[k]), d);//最大差值
                        w0[k] = w[k];//用于下次迭代使用 
                    }
                }
            }
            return w;
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
