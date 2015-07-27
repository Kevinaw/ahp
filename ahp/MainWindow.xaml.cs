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
using MyListControl;
using MyNumericUpDownControll;

namespace ahp
{
    /// <summary>
    /// Interaction logic for MainWindow_2.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        Criterias Criterias = new Criterias();
        Alternatives Alternatives = new Alternatives();
        private bool isOpeningPrj; //opening old project
        private string filePath;

        int inconsistencyRow;
        int inconsistencyCol;
        List<int> inconsistecyIndexes;

        public MainWindow()
        {
            InitializeComponent();
            isOpeningPrj = false;
            filePath = null;
            inconsistencyRow = -1;
            inconsistencyCol = -1;
            inconsistecyIndexes = new List<int>();
#if DEBUG
            Console.WriteLine("debug mode");
#endif

        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            switch (tabMain.SelectedIndex)
            {
                case 0:
                    grdCr.Focus();
                    break;
                case 1:
                    grdAl.Focus();
                    break;
                case 4:
                    //testGrid.Focus();
                    break;
                default:
                    break;
            }
            drawAhpHierarchy();
        }

        private void tabMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (tabMain.SelectedIndex)
            {
                case 0:
                    break;
                case 1:
                    //txbNameAl.Text = "";
                    //txbCostAl.Text = "";
                    //txbCostRAl.Text = "";
                    break;
                case 2:
                    BtnIdentify.IsEnabled = false;
                    BtnPropose.IsEnabled = false;
                    txtEvalRslt.Text = "";
                    inconsistencyRow = -1;
                    inconsistencyCol = -1;
                    inconsistecyIndexes.Clear();
                    double totalWidth = tabMain.ActualWidth - 200;
                    double totalHeight = tabMain.ActualHeight - 120;
                    Criterias.GenerateMtxView(mtxGrid, totalWidth, totalHeight);
                    break;
                case 3:
                    Alternatives.GenerateMtxView(grdAC, tabMain, Criterias);
                    break;
                case 4:
                    DrawResultMtx();
                    drawAhpRst();
                    break;
                default:
                    break;
            }

        }

        // draw Result matrix
        private void DrawResultMtx()
        {
            int i, j;
            grdRst.Children.Clear();
            grdRst.RowDefinitions.Clear();
            grdRst.ColumnDefinitions.Clear();

            if (Criterias.listCr.Count == 0 || Alternatives.listAl.Count == 0)
            {
                bdrRst.Visibility = Visibility.Hidden;
                return;
            }
            else
            {
                bdrRst.Visibility = Visibility.Visible;
            }

            if (Alternatives.mtxAC == null && Alternatives.listAl.Count != 0)
            {
                Alternatives.mtxAC = new int[Criterias.listCr.Count, Alternatives.listAl.Count];
            }

            if (Criterias.mtx == null && Criterias.listCr.Count != 0)
            {
                Criterias.mtx = new double[Criterias.listCr.Count, Criterias.listCr.Count];
            }


            double gridWidth = 100;
            double gridHeight = 50;
            double x = tabMain.ActualWidth - 80;
            double y = tabMain.ActualHeight - 100;
            if (x < 2 * (Alternatives.listAl.Count + 1) * gridWidth)
            {
                gridWidth = x / (Alternatives.listAl.Count + 1) / 2;
                gridHeight = gridWidth / 2;
            }
            if (y < (Criterias.listCr.Count + 3) * gridHeight)
            {
                gridHeight = y / (Criterias.listCr.Count + 2);
                gridWidth = 2 * gridHeight;
            }

            for (i = 0; i < Criterias.listCr.Count + 2; i++)
            {
                RowDefinition row = new RowDefinition();
                row.Height = new GridLength(gridHeight);

                grdRst.RowDefinitions.Add(row);
            }
            bdrRst.Height = (Criterias.listCr.Count + 2) * gridHeight;

            for (i = 0; i < 2 * Alternatives.listAl.Count + 2; i++)
            {
                ColumnDefinition col = new ColumnDefinition();
                col.Width = new GridLength(gridWidth);

                grdRst.ColumnDefinitions.Add(col);
            }
            bdrRst.Width = (2 * Alternatives.listAl.Count + 2) * gridWidth;


            // redraw matrix
            // fill criteria headers & related weights
            for (i = 0; i < Criterias.listCr.Count; i++)
            {
                // criteria
                TextBlock txt;
                txt = new TextBlock();
                txt.Text = Criterias.listCr.ElementAt(i).name;
                grdRst.Children.Add(txt);
                Grid.SetRow(txt, i + 1);
                Grid.SetColumn(txt, 0);
                txt.HorizontalAlignment = HorizontalAlignment.Center;
                txt.VerticalAlignment = VerticalAlignment.Center;

                // weight
                txt = new TextBlock();
                txt.Text = String.Format("{0:#.00}", Criterias.listCr.ElementAt(i).weight);
                grdRst.Children.Add(txt);
                Grid.SetRow(txt, i + 1);
                Grid.SetColumn(txt, 1);
                txt.HorizontalAlignment = HorizontalAlignment.Center;
                txt.VerticalAlignment = VerticalAlignment.Center;
            }

            TextBlock txtW = new TextBlock();
            txtW.Text = "weight";
            grdRst.Children.Add(txtW);
            Grid.SetRow(txtW, 0);
            Grid.SetColumn(txtW, 1);
            txtW.HorizontalAlignment = HorizontalAlignment.Center;
            txtW.VerticalAlignment = VerticalAlignment.Center;


            // fill all the alternative names
            for (i = 0; i < Alternatives.listAl.Count; i++)
            {
                TextBlock txt = new TextBlock();
                txt.Text = Alternatives.listAl.ElementAt(i).name + " " + "Score";
                grdRst.Children.Add(txt);
                Grid.SetRow(txt, 0);
                Grid.SetColumn(txt, 2 * (i + 1));
                txt.HorizontalAlignment = HorizontalAlignment.Center;
                txt.VerticalAlignment = VerticalAlignment.Center;

                txt = new TextBlock();
                txt.Text = Alternatives.listAl.ElementAt(i).name + " " + "Function";
                grdRst.Children.Add(txt);
                Grid.SetRow(txt, 0);
                Grid.SetColumn(txt, 2 * (i + 1) + 1);
                txt.HorizontalAlignment = HorizontalAlignment.Center;
                txt.VerticalAlignment = VerticalAlignment.Center;

                double sumFunc = 0;

                // fill scores and functions
                for (j = 0; j < Criterias.listCr.Count; j++)
                {
                    // score
                    TextBlock tb = new TextBlock();
                    tb.Text = Alternatives.mtxAC[j, i].ToString();
                    tb.Name = "txtScore" + j.ToString() + i.ToString();
                    tb.VerticalAlignment = VerticalAlignment.Center;
                    tb.HorizontalAlignment = HorizontalAlignment.Center;
                    grdRst.Children.Add(tb);
                    Grid.SetRow(tb, j + 1);
                    Grid.SetColumn(tb, 2 * i + 2);

                    // function
                    TextBlock tblk = new TextBlock();
                    tblk.Text = String.Format("{0:#.00}", (Alternatives.mtxAC[j, i] * Criterias.listCr.ElementAt(j).weight));
                    tblk.Name = "txtFunc" + j.ToString() + i.ToString();
                    tblk.VerticalAlignment = VerticalAlignment.Center;
                    tblk.HorizontalAlignment = HorizontalAlignment.Center;
                    grdRst.Children.Add(tblk);
                    Grid.SetRow(tblk, j + 1);
                    Grid.SetColumn(tblk, 2 * i + 3);

                    sumFunc += Alternatives.mtxAC[j, i] * Criterias.listCr.ElementAt(j).weight;
                }

                // fill function sum
                TextBlock tblk1 = new TextBlock();
                tblk1.Text = String.Format("{0:#.00}", sumFunc);
                tblk1.Name = "txtFuncSum" + i.ToString();
                tblk1.VerticalAlignment = VerticalAlignment.Center;
                tblk1.HorizontalAlignment = HorizontalAlignment.Center;
                grdRst.Children.Add(tblk1);
                Grid.SetRow(tblk1, Criterias.listCr.Count + 1);
                Grid.SetColumn(tblk1, 2 * i + 3);
            }
        }

        private void evalButton_Click(object sender, RoutedEventArgs e)
        {
            if (Criterias.mtx == null)
            {
                MessageBox.Show("criterias compare matrix has not been set!");
                return;
            }
            Criterias.AhpEval();

            string weights = "\n";
            for (int i = 0; i < Criterias.listCr.Count; i++)
                weights += "w(" + Criterias.listCr[i].name + ") = " + String.Format("{0:0.000000}", Criterias.listCr[i].weight) + "; ";

            string rslt = "CR = ";
            rslt += String.Format("{0:0.000000}", Criterias.CR);
            rslt += "\n";
            txtEvalRslt.Inlines.Clear();
            if (Criterias.CR < 0.1)
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

                BtnIdentify.IsEnabled = false;
                BtnPropose.IsEnabled = false;
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

                BtnIdentify.IsEnabled = true;
                BtnPropose.IsEnabled = true;
            }
        }

        private void BtnIdentify_Click(object sender, RoutedEventArgs e)
        {
            double[] res = Criterias.Identify();
            inconsistencyRow = Criterias.inconsistentRow;
            inconsistencyCol = Criterias.inconsistentCol;

            WindowSelectInconsistentElements popup = new WindowSelectInconsistentElements(res);
            popup.ShowDialog();

            // highlight the inconsistent cells
            for (int i = 0; i < popup.selectedIndexes.Count; i++)
            {
                inconsistecyIndexes.Add(popup.selectedIndexes[i]);
                Criterias.Highlight(popup.selectedIndexes[i]);
            }

        }

        private void BtnPropose_Click(object sender, RoutedEventArgs e)
        {
            WindowPropose wndP = new WindowPropose(Criterias, inconsistencyRow, inconsistencyCol, inconsistecyIndexes);
            if (true == wndP.ShowDialog())
            {
                int[,] freedomArry = wndP.freedomArry;
                var resultList = new List<Criterias>();
                var locationList = new List<StrctFreedomLocation>();
                int i, j;

                // store all the location for free changing
                for (i = 0; i < Criterias.listCr.Count; i++)
                    for (j = 0; j < Criterias.listCr.Count; j++)
                    {
                        if (freedomArry[i, j] != 0)
                        {
                            locationList.Add(new StrctFreedomLocation { row = i, col = j, freedom = freedomArry[i, j] });
                        }
                    }
                // There are some duplicate evaluations, we should remove them before continue;
                PopulateMtx(locationList, Criterias, ref resultList);

                // Remove duplicate
                //var resultList1 = new List<Criterias>();
                //for (i = 0; i < resultList.Count; i++)
                //{
                //    Criterias c = resultList.ElementAt(i);
                //    if (resultList1.Count != 0)
                //    {
                //        for(j = 0; j < resultList1.Count; j++)
                //        {
                //            if (c.Equals(resultList1.ElementAt(j)))
                //                break;
                //            else
                //                continue;
                //        }
                //        // not found
                //        if(j == resultList1.Count)
                //            resultList1.Add(c);

                //    }
                //    else
                //    {
                //        resultList1.Add(c);
                //    }
                //}


                //#if DEBUG
                //                Console.WriteLine("resultList.count = " + resultList.Count.ToString());
                //                Console.WriteLine("resultList1.count = " + resultList1.Count.ToString());
                //#endif
                // order the result to only extract the smallest 10 results.
                resultList.Sort((x, y) => x.CR.CompareTo(y.CR));

                if (resultList.Count == 0)
                {
                    MessageBox.Show("0 combinations found!");
                    return;
                }

                // Show window of CR results
                WindowCombinations wndC = new WindowCombinations(resultList, inconsistencyRow, inconsistencyCol, inconsistecyIndexes);

                if (true == wndC.ShowDialog())
                {
                    if (wndC.idx != -1)
                    {
                        //MessageBox.Show("combination selected:" + wndC.idx.ToString());
                        for (i = 0; i < Criterias.listCr.Count; i++)
                            for (j = i + 1; j < Criterias.listCr.Count; j++)
                            {
                                Criterias.mtx[i, j] = resultList[wndC.idx].mtx[i, j];
                                Criterias.mtx[j, i] = resultList[wndC.idx].mtx[j, i];
                                if (null != (Criterias.mtxCtrls[i, j] as NumericUpDown))
                                {
                                    NumericUpDown myN = Criterias.mtxCtrls[i, j] as NumericUpDown;
                                    myN.dblValue = resultList[wndC.idx].mtx[i, j];

                                    TextBlock myT = Criterias.mtxCtrls[j, i] as TextBlock;
                                    myT.Text = myN.strValueR;
                                }
                                else
                                    continue;
                            }

                        //Criterias.AhpEval();
                        evalButton_Click(null, null);

                    }
                }

            }
        }

        private void PopulateMtx(List<StrctFreedomLocation> locationList, Criterias Criterias, ref List<Criterias> resultList)
        {
            if (locationList.Count > 0)
            {
                var location = locationList.ElementAt(0);
                //locationList.RemoveAt(0);
                double[] range;
                // direction only
                if (location.freedom == 1)
                {
                    // 1, 2, 3, 4, 5, 6, 7, 8, 9
                    if (Criterias.mtx[location.row, location.col] >= 1)
                        range = new double[] { 2, 3, 4, 5, 6, 7, 8, 9 };
                    else
                        range = new double[] { 1 / 9, 1 / 8, 1 / 7, 1 / 6, 1 / 5, 1 / 4, 1 / 3, 1 / 2 };
                }
                else
                    range = new double[] { 1.0 / 9.0, 1.0 / 8.0, 1.0 / 7.0, 1.0 / 6.0, 1.0 / 5.0, 1.0 / 4.0, 1.0 / 3.0, 1.0 / 2.0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

                var ll = new List<StrctFreedomLocation>();
                ll = locationList.ToList();
                ll.RemoveAt(0);

                foreach (double val in range)
                {
                    var c = new Criterias(Criterias);
                    c.mtx[location.row, location.col] = val;
                    c.mtx[location.col, location.row] = 1 / val;
                    if (ll.Count == 0)
                    {
                        c.AhpEval();
                        if (c.CR < 0.1)
                            resultList.Add(c);

#if DEBUG
                        Console.WriteLine("CR = " + c.CR.ToString());
                        for (int i = 0; i < c.listCr.Count; i++)
                        {
                            for (int j = 0; j < c.listCr.Count; j++)
                                Console.Write(c.mtx[i, j].ToString() + "    ");
                            Console.Write("\n");
                        }
#endif

                    }


                    PopulateMtx(ll, c, ref resultList);

                }
            }
        }

        //draw AHP Hierarchy
        private void drawAhpHierarchy()
        {
            // 4/5 fill graphics
            // goal rectangle
            if (this.txbGoal.Text != "")
            {
                double cvsWidth = this.SrvHierary.ActualWidth;
                double cvsHeight = this.SrvHierary.ActualHeight;
                double cellWidth = 80;
                double cellHeight = 40;

                // Left side width.
                double leftHederWidth = 60;

                //clear the canvas and redraw.
                this.cvsHrcGraph1.Children.Clear();

                // 1/5 fill words
                TextBlock txt = new TextBlock();
                txt.Text = "Goal:";
                Canvas.SetLeft(txt, 20);
                Canvas.SetTop(txt, cvsHeight / 4);
                this.cvsHrcGraph1.Children.Add(txt);

                txt = new TextBlock();
                txt.Text = "Criteria:";
                Canvas.SetLeft(txt, 20);
                Canvas.SetTop(txt, cvsHeight / 2);
                this.cvsHrcGraph1.Children.Add(txt);


                DropShadowEffect dse = new DropShadowEffect();
                dse.BlurRadius = 4;
                dse.ShadowDepth = 10;
                dse.Color = Colors.Silver;

                Border bdr = new Border();
                bdr.Width = cellWidth;
                bdr.Height = cellHeight;
                bdr.CornerRadius = new CornerRadius(2);
                bdr.BorderThickness = new Thickness(1);
                bdr.BorderBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x8C, 0xA6, 0xC9));

                txt = new TextBlock();
                txt.Text = this.txbGoal.Text;
                txt.VerticalAlignment = VerticalAlignment.Center;
                txt.HorizontalAlignment = HorizontalAlignment.Center;
                txt.TextWrapping = TextWrapping.Wrap;

                bdr.Child = txt;
                Canvas.SetTop(bdr, cvsHeight/4);
                if(Criterias.listCr.Count > 0)
                    Canvas.SetLeft(bdr, ((cellWidth * Criterias.listCr.Count * 5/4 + cellWidth/4) - cellWidth)/2 + leftHederWidth);
                else
                    Canvas.SetLeft(bdr, (cvsWidth * 4 / 5 - cellWidth) / 2 + leftHederWidth);
                this.cvsHrcGraph1.Children.Add(bdr);

                Line myLine = new Line();
                myLine.Stroke = System.Windows.Media.Brushes.Black;
                myLine.X1 = cellWidth/ 2 + Canvas.GetLeft(bdr);
                myLine.Y1 = Canvas.GetTop(bdr) + cellHeight;
                myLine.X2 = cellWidth / 2 + Canvas.GetLeft(bdr);
                myLine.Y2 = cvsHeight / 2 - (cvsHeight/4 - cellHeight)/2;
                myLine.StrokeThickness = 1;
                this.cvsHrcGraph1.Children.Add(myLine);


                if (Criterias.listCr.Count != 0)
                {
                    this.cvsHrcGraph1.Width = cellWidth * Criterias.listCr.Count * 5 / 4 + cellWidth / 2 + leftHederWidth;
                    this.cvsHrcGraph1.MinWidth = cvsWidth;
                    for (int i = 0; i < Criterias.listCr.Count; i++)
                    {
                        bdr = new Border();
                        bdr.Width = cellWidth;
                        bdr.Height = cellHeight;
                        bdr.CornerRadius = new CornerRadius(2);
                        bdr.BorderThickness = new Thickness(1);
                        bdr.BorderBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x8C, 0xA6, 0xC9));

                        txt = new TextBlock();
                        txt.Text = Criterias.listCr[i].name + "\n" + Criterias.listCr[i].weight.ToString();
                        txt.VerticalAlignment = VerticalAlignment.Center;
                        txt.HorizontalAlignment = HorizontalAlignment.Center;
                        txt.TextWrapping = TextWrapping.Wrap;

                        bdr.Child = txt;
                        Canvas.SetTop(bdr, cvsHeight / 2);
                        Canvas.SetLeft(bdr, i * cellWidth + (i + 1) * cellWidth / 4 + leftHederWidth);
                        this.cvsHrcGraph1.Children.Add(bdr);

                        myLine = new Line();
                        myLine.Stroke = System.Windows.Media.Brushes.Black;
                        myLine.X1 = cellWidth / 2 + Canvas.GetLeft(bdr);
                        myLine.Y1 = cvsHeight / 2 - (cvsHeight / 4 - cellHeight) / 2;
                        myLine.X2 = cellWidth / 2 + Canvas.GetLeft(bdr);
                        myLine.Y2 = Canvas.GetTop(bdr);
                        myLine.StrokeThickness = 1;
                        this.cvsHrcGraph1.Children.Add(myLine);
                    }

                    Line myLine1 = new Line();
                    myLine1.Stroke = System.Windows.Media.Brushes.Black;
                    myLine1.X1 = leftHederWidth + cellWidth * 3 / 4;
                    myLine1.Y1 = cvsHeight / 2 - (cvsHeight / 4 - cellHeight) / 2;
                    myLine1.X2 = leftHederWidth + cellWidth * 5 / 4 * Criterias.listCr.Count - cellWidth / 2;
                    myLine1.Y2 = cvsHeight / 2 - (cvsHeight / 4 - cellHeight) / 2;
                    myLine1.StrokeThickness = 1;
                    this.cvsHrcGraph1.Children.Add(myLine1);
                }
            }

/*

            if (Alternatives.listAl.Count != 0)
            {
                double alWidth = cvsWidth * 4 / 5 / Alternatives.listAl.Count * 4 / 5;
                double alHeight = recHeight;
                for (int i = 0; i < Alternatives.listAl.Count; i++)
                {
                    rec = new Rectangle();
                    rec.HorizontalAlignment = HorizontalAlignment.Center;
                    rec.VerticalAlignment = VerticalAlignment.Center;
                    rec.Width = alWidth;
                    rec.Height = alHeight;
                    rec.Fill = Brushes.Pink;
                    rec.Effect = dse;
                    Canvas.SetLeft(rec, cvsWidth / 5 + cvsWidth * 4 / 5 / Alternatives.listAl.Count * i + (cvsWidth * 4 / 5 / Alternatives.listAl.Count - alWidth) / 2);
                    Canvas.SetTop(rec, (cvsHeight / 3 - rec.Height) / 2 + cvsHeight * 2 / 3);
                    this.cvsHrcGraph1.Children.Add(rec);

                    txt = new TextBlock();
                    txt.Text = Alternatives.listAl[i].name + "\n" + Alternatives.listAl[i].finalScore;
                    txt.VerticalAlignment = VerticalAlignment.Center;
                    txt.HorizontalAlignment = HorizontalAlignment.Center;
                    txt.TextWrapping = TextWrapping.Wrap;

                    Grid grd = new Grid();
                    grd.Width = rec.Width - 2;
                    grd.Height = rec.Height - 2;
                    grd.Children.Add(txt);

                    Canvas.SetTop(grd, Canvas.GetTop(rec) + 1);
                    Canvas.SetLeft(grd, Canvas.GetLeft(rec) + 1);
                    this.cvsHrcGraph1.Children.Add(grd);
                }
                
            }*/
        }

        private void drawAhpRst()
        {

            if (this.txbGoal.Text != "")
            {
                double cvsWidth = this.CvsRstGraph.ActualWidth;
                double cvsHeight = this.CvsRstGraph.ActualHeight;

                //clear the canvas and redraw.
                this.CvsRstGraph.Children.Clear();

                // 1/5 fill words
                TextBlock txt = new TextBlock();
                txt.Text = "Goal:";
                Canvas.SetLeft(txt, 5);
                Canvas.SetTop(txt, cvsHeight / 6);
                this.CvsRstGraph.Children.Add(txt);



                txt = new TextBlock();
                txt.Text = "Criteria:";
                Canvas.SetLeft(txt, 5);
                Canvas.SetTop(txt, cvsHeight / 2);
                this.CvsRstGraph.Children.Add(txt);

                txt = new TextBlock();
                txt.Text = "Alternatives:";
                Canvas.SetLeft(txt, 5);
                Canvas.SetTop(txt, cvsHeight * 5 / 6);
                this.CvsRstGraph.Children.Add(txt);

                DropShadowEffect dse = new DropShadowEffect();
                dse.BlurRadius = 4;
                dse.ShadowDepth = 10;
                dse.Color = Colors.Silver;

                // 4/5 fill graphics
                // goal rectangle
                Rectangle rec;
                double recHeight = cvsHeight / 6;

                rec = new Rectangle();
                rec.Width = cvsWidth * 4 / 5 / 4;
                rec.Height = recHeight;
                rec.Fill = Brushes.YellowGreen;
                rec.Effect = dse;
                Canvas.SetLeft(rec, cvsWidth / 5 + (cvsWidth * 4 / 5 - rec.Width) / 2);
                Canvas.SetTop(rec, (cvsHeight / 3 - rec.Height) / 2);
                this.CvsRstGraph.Children.Add(rec);

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
                this.CvsRstGraph.Children.Add(grd);

                if (Criterias.listCr.Count != 0)
                {
                    double crWidth = cvsWidth * 4 / 5 / Criterias.listCr.Count * 4 / 5;
                    double crHeight = recHeight;
                    for (int i = 0; i < Criterias.listCr.Count; i++)
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
                        Canvas.SetLeft(rec, cvsWidth / 5 + cvsWidth * 4 / 5 / Criterias.listCr.Count * i + (cvsWidth * 4 / 5 / Criterias.listCr.Count - crWidth) / 2);
                        Canvas.SetTop(rec, (cvsHeight / 3 - rec.Height) / 2 + cvsHeight / 3);
                        this.CvsRstGraph.Children.Add(rec);

                        txt = new TextBlock();
                        txt.Text = Criterias.listCr[i].name + "\n" + Criterias.listCr[i].weight.ToString();
                        txt.VerticalAlignment = VerticalAlignment.Center;
                        txt.HorizontalAlignment = HorizontalAlignment.Center;
                        txt.TextWrapping = TextWrapping.Wrap;

                        grd = new Grid();
                        grd.Width = rec.Width - 2;
                        grd.Height = rec.Height - 2;
                        grd.Children.Add(txt);

                        Canvas.SetTop(grd, Canvas.GetTop(rec) + 1);
                        Canvas.SetLeft(grd, Canvas.GetLeft(rec) + 1);
                        this.CvsRstGraph.Children.Add(grd);
                    }
                }

                if (Alternatives.listAl.Count != 0)
                {
                    double alWidth = cvsWidth * 4 / 5 / Alternatives.listAl.Count * 4 / 5;
                    double alHeight = recHeight;
                    for (int i = 0; i < Alternatives.listAl.Count; i++)
                    {
                        rec = new Rectangle();
                        rec.HorizontalAlignment = HorizontalAlignment.Center;
                        rec.VerticalAlignment = VerticalAlignment.Center;
                        rec.Width = alWidth;
                        rec.Height = alHeight;
                        rec.Fill = Brushes.Pink;
                        rec.Effect = dse;
                        Canvas.SetLeft(rec, cvsWidth / 5 + cvsWidth * 4 / 5 / Alternatives.listAl.Count * i + (cvsWidth * 4 / 5 / Alternatives.listAl.Count - alWidth) / 2);
                        Canvas.SetTop(rec, (cvsHeight / 3 - rec.Height) / 2 + cvsHeight * 2 / 3);
                        this.CvsRstGraph.Children.Add(rec);

                        txt = new TextBlock();
                        txt.Text = Alternatives.listAl[i].name + "\n" + Alternatives.listAl[i].finalScore;
                        txt.VerticalAlignment = VerticalAlignment.Center;
                        txt.HorizontalAlignment = HorizontalAlignment.Center;
                        txt.TextWrapping = TextWrapping.Wrap;

                        grd = new Grid();
                        grd.Width = rec.Width - 2;
                        grd.Height = rec.Height - 2;
                        grd.Children.Add(txt);

                        Canvas.SetTop(grd, Canvas.GetTop(rec) + 1);
                        Canvas.SetLeft(grd, Canvas.GetLeft(rec) + 1);
                        this.CvsRstGraph.Children.Add(grd);
                    }
                }
            }
        }

        private void cvsHrcGraph_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //drawAhpHierarchy();

        }

        private void cvsHrcGraph1_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //drawAhpHierarchy();

        }

        private void txbGoal_TextChanged(object sender, TextChangedEventArgs e)
        {
            drawAhpHierarchy();
        }

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            // code needed here, verify all the result to save

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Project1";
            dlg.DefaultExt = ".ahp";
            dlg.Filter = "AHP project (.ahp)|*.ahp";
            if (filePath == null)
            {
                if (dlg.ShowDialog() == true)
                    filePath = dlg.FileName;
                else
                    return;
            }
            //create .ahp file and save
            CreatXml(filePath);
            MessageBox.Show("saved!");
        }

        private void newbtn_Click(object sender, RoutedEventArgs e)
        {
            // save current unsaved project
            if ((filePath == null && (Criterias.listCr.Count != 0 || Alternatives.listAl.Count != 0)) ||
                (filePath != null && true == IsPrjChanged(filePath)))
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

            filePath = null;

            tabMain.SelectedIndex = 0;
            // goal
            txbGoal.Text = "";

            // clear alternatives list view, then refill it
            // contentLbxAl.Items.Clear();
            Alternatives.Clear();
            usrCtrlAlt.Clear();

            // clear criteria list view, then refill it
            //contentLbxCr.Items.Clear();
            Criterias.Clear();
            usrCtrlCr.Clear();

            // criteria VS goal comparison mtx


            // alternative vs criteria comparison mtx


            // draw AHP Hierarchy Graphics
            drawAhpHierarchy();

        }

        private void openBtn_Click(object sender, RoutedEventArgs e)
        {
            // save current unsaved project
            if ((filePath == null && (Criterias.listCr.Count != 0 || Alternatives.listAl.Count != 0)) ||
                (filePath != null && true == IsPrjChanged(filePath)))
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

            // code needed here, verify all the result to save

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "Project1";
            dlg.DefaultExt = ".ahp";
            dlg.Filter = "AHP project (.ahp)|*.ahp";
            if (dlg.ShowDialog() == true)
            {
                filePath = dlg.FileName;
                //delete .tmp file if there is one


                //load .ahp file and update the view.
                tabMain.SelectedIndex = 0;
                LoadXml(filePath);
            }
        }

        private void buildAllBtn_Click(object sender, RoutedEventArgs e)
        {

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

            for (int i = 0; i < Alternatives.listAl.Count; i++)
            {
                element = doc.CreateElement("Alternative");
                element.InnerText = Alternatives.listAl.ElementAt(i).name;

                XmlAttribute attr = doc.CreateAttribute("cost1");
                attr.Value = Alternatives.listAl.ElementAt(i).cost1.ToString();
                element.Attributes.Append(attr);

                attr = doc.CreateAttribute("cost2");
                attr.Value = Alternatives.listAl.ElementAt(i).cost2.ToString();
                element.Attributes.Append(attr);

                // next step
                attr = doc.CreateAttribute("finalscore");
                attr.Value = Alternatives.listAl.ElementAt(i).finalScore.ToString();
                element.Attributes.Append(attr);

                alternatives.AppendChild(element);
            }

            // criterias
            XmlElement criterias = doc.CreateElement("Criterias");
            root.AppendChild(criterias);

            for (int i = 0; i < Criterias.listCr.Count; i++)
            {
                element = doc.CreateElement("Criteria");
                element.InnerText = Criterias.listCr.ElementAt(i).name;

                XmlAttribute attr = doc.CreateAttribute("weight");
                attr.Value = Criterias.listCr[i].weight.ToString();
                element.Attributes.Append(attr);

                criterias.AppendChild(element);
            }


            // criteria vs goal pairwise comparison
            XmlElement cvgs = doc.CreateElement("CriteriaVsGoalMatrix");
            root.AppendChild(cvgs);

            for (int i = 0; i < Criterias.listCr.Count; i++)
                for (int j = 0; j < Criterias.listCr.Count; j++)
                {
                    element = doc.CreateElement("CVGItem");
                    element.InnerText = Criterias.mtx[i, j].ToString();

                    XmlAttribute attr = doc.CreateAttribute("index");
                    attr.Value = i.ToString() + j.ToString();
                    element.Attributes.Append(attr);

                    cvgs.AppendChild(element);
                }

            // alternative vs criteria pairwise comparison
            XmlElement avcs = doc.CreateElement("AlternativeVsGoalMatrix");
            root.AppendChild(avcs);

            for (int i = 0; i < Criterias.listCr.Count; i++)
                for (int j = 0; j < Alternatives.listAl.Count; j++)
                {
                    element = doc.CreateElement("AVCItem");
                    element.InnerText = Alternatives.mtxAC[i, j].ToString();

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
            //contentLbxAl.Items.Clear();
            Alternatives.Clear();
            usrCtrlAlt.Clear();

            XmlNode alternatives = doc.SelectSingleNode("descendant::Alternatives");
            if (alternatives.HasChildNodes)
            {
                for (i = 0; i < alternatives.ChildNodes.Count; i++)
                {
                    StrctAlternative a = new StrctAlternative();
                    a.name = alternatives.ChildNodes[i].InnerText;
                    a.cost1 = Convert.ToDouble(alternatives.ChildNodes[i].Attributes["cost1"].Value);
                    a.cost2 = Convert.ToDouble(alternatives.ChildNodes[i].Attributes["cost2"].Value); ;
                    a.finalScore = Convert.ToDouble(alternatives.ChildNodes[i].Attributes["finalscore"].Value); ;
                    Alternatives.Add(a);

                    usrCtrlAlt.AddItem(a.name);
                }
            }


            // clear criteria list view, then refill it
            // contentLbxCr.Items.Clear();
            Criterias.Clear();
            usrCtrlCr.Clear();

            XmlNode criterias = doc.SelectSingleNode("descendant::Criterias");
            if (criterias.HasChildNodes)
            {
                for (i = 0; i < criterias.ChildNodes.Count; i++)
                {
                    StrctCriteria c = new StrctCriteria();
                    c.name = criterias.ChildNodes[i].InnerText;
                    c.weight = Convert.ToDouble(criterias.ChildNodes[i].Attributes["weight"].Value);
                    Criterias.Add(c);

                    usrCtrlCr.AddItem(c.name);
                }
            }

            // criteria VS goal comparison mtx
            XmlNode cvgs = doc.SelectSingleNode("descendant::CriteriaVsGoalMatrix");
            Criterias.mtx = new double[criterias.ChildNodes.Count, criterias.ChildNodes.Count];
            for (i = 0; i < criterias.ChildNodes.Count; i++)
                for (j = 0; j < criterias.ChildNodes.Count; j++)
                {
                    Criterias.mtx[i, j] = Convert.ToDouble(cvgs.ChildNodes[i * criterias.ChildNodes.Count + j].InnerText);
                }

            // alternative vs criteria comparison mtx
            XmlNode avgs = doc.SelectSingleNode("descendant::AlternativeVsGoalMatrix");
            Alternatives.mtxAC = new int[criterias.ChildNodes.Count, alternatives.ChildNodes.Count];
            for (i = 0; i < criterias.ChildNodes.Count; i++)
                for (j = 0; j < alternatives.ChildNodes.Count; j++)
                {
                    Alternatives.mtxAC[i, j] = Convert.ToInt32(avgs.ChildNodes[i * alternatives.ChildNodes.Count + j].InnerText);
                }

            // draw AHP Hierarchy Graphics
            drawAhpHierarchy();

            isOpeningPrj = false;
        }

        private bool IsPrjChanged(string xmlPath)
        {
            int i, j;

            // load xml file
            XmlDocument doc = new XmlDocument();
            doc.Load(xmlPath);

            // goal
            XmlNode goal = doc.SelectSingleNode("descendant::Goal");
            if (txbGoal.Text != goal.InnerText)
                return true;

            XmlNode alternatives = doc.SelectSingleNode("descendant::Alternatives");
            if (alternatives.HasChildNodes)
            {
                for (i = 0; i < alternatives.ChildNodes.Count; i++)
                {
                    StrctAlternative a = Alternatives.listAl.ElementAt(i);
                    if (a.name != alternatives.ChildNodes[i].InnerText)
                        return true;
                    if (a.cost1 != Convert.ToDouble(alternatives.ChildNodes[i].Attributes["cost1"].Value))
                        return true;
                    if (a.cost2 != Convert.ToDouble(alternatives.ChildNodes[i].Attributes["cost2"].Value))
                        return true;
                    if (String.Format("{0:#.00}", a.finalScore) != String.Format("{0:#.00}", Convert.ToDouble(alternatives.ChildNodes[i].Attributes["finalscore"].Value)))
                        return true;
                }
            }

            XmlNode criterias = doc.SelectSingleNode("descendant::Criterias");
            if (criterias.HasChildNodes)
            {
                for (i = 0; i < criterias.ChildNodes.Count; i++)
                {
                    StrctCriteria c = Criterias.listCr.ElementAt(i);
                    if (c.name != criterias.ChildNodes[i].InnerText)
                        return true;
                    if (String.Format("{0:#.00}", c.weight) != String.Format("{0:#.00}", Convert.ToDouble(criterias.ChildNodes[i].Attributes["weight"].Value)))
                        return true;
                }
            }

            // criteria VS goal comparison mtx
            XmlNode cvgs = doc.SelectSingleNode("descendant::CriteriaVsGoalMatrix");
            if (Criterias.mtx != null && Criterias.mtx.Length != criterias.ChildNodes.Count * criterias.ChildNodes.Count)
                return true;
            for (i = 0; i < criterias.ChildNodes.Count; i++)
                for (j = 0; j < criterias.ChildNodes.Count; j++)
                {
                    if (Criterias.mtx[i, j] != Convert.ToDouble(cvgs.ChildNodes[i * criterias.ChildNodes.Count + j].InnerText))
                        return true;
                }

            // alternative vs criteria comparison mtx
            XmlNode avgs = doc.SelectSingleNode("descendant::AlternativeVsGoalMatrix");
            if (Alternatives.mtxAC != null && Alternatives.mtxAC.Length != criterias.ChildNodes.Count * alternatives.ChildNodes.Count)
                return true;
            for (i = 0; i < criterias.ChildNodes.Count; i++)
                for (j = 0; j < alternatives.ChildNodes.Count; j++)
                {
                    if (Alternatives.mtxAC[i, j] != Convert.ToInt32(avgs.ChildNodes[i * alternatives.ChildNodes.Count + j].InnerText))
                        return true;
                }

            return false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            usrCtrlCr.ListItemAddedEvent += onListItemAddedEventCr;
            usrCtrlCr.ListItemDeletedEvent += onListItemDeletedEventCr;
            usrCtrlCr.ListItemUpdatedEvent += onListItemUpdatedEventCr;

            usrCtrlAlt.ListItemAddedEvent += onListItemAddedEventAl;
            usrCtrlAlt.ListItemDeletedEvent += onListItemDeletedEventAl;
            usrCtrlAlt.ListItemUpdatedEvent += onListItemUpdatedEventAl;

            usrCtrlAlt.SelectionChangedEvent += onSelectionChangedEvent;

        }

        private void onListItemAddedEventCr(object sender, ListItemChangedEventArgs e)
        {
            StrctCriteria c = new StrctCriteria();
            c.name = e.NewName;
            c.weight = 1.0;
            Criterias.Add(c);

            drawAhpHierarchy();
        }

        private void onListItemDeletedEventCr(object sender, ListItemChangedEventArgs e)
        {
            Criterias.Delete(e.Idx);
            drawAhpHierarchy();
        }

        private void onListItemUpdatedEventCr(object sender, ListItemChangedEventArgs e)
        {
            StrctCriteria c = new StrctCriteria();
            c.name = e.NewName;
            c.weight = Criterias.listCr[e.Idx].weight;

            Criterias.update(e.Idx, c);
            drawAhpHierarchy();
        }

        private void onListItemAddedEventAl(object sender, ListItemChangedEventArgs e)
        {
            StrctAlternative a = new StrctAlternative();
            a.name = e.NewName;
            a.cost1 = 1;
            a.cost2 = 1;
            Alternatives.Add(a);
            //drawAhpHierarchy();
        }

        private void onListItemDeletedEventAl(object sender, ListItemChangedEventArgs e)
        {
            Alternatives.Delete(e.Idx);
            //drawAhpHierarchy();
        }

        private void onListItemUpdatedEventAl(object sender, ListItemChangedEventArgs e)
        {
            if (e.Idx < Alternatives.listAl.Count)
            {
                StrctAlternative a = new StrctAlternative();
                a.name = e.NewName;
                a.cost1 = Alternatives.listAl[e.Idx].cost1;
                a.cost2 = Alternatives.listAl[e.Idx].cost2;

                Alternatives.update(e.Idx, a);
            }

            //drawAhpHierarchy();
        }

        private void onSelectionChangedEvent(object sender, EventArgs e)
        {
            if (isOpeningPrj == false)
            {
                int idx = usrCtrlAlt.selectedIdx;
                if (idx >= 0)
                {

                    txbNameAl.Text = Alternatives.listAl[idx].name;
                    txbCostAl.Text = Alternatives.listAl[idx].cost1.ToString();
                    txbCostRAl.Text = Alternatives.listAl[idx].cost2.ToString();
                }


            }

        }

        private void txbNameAl_TextChanged(object sender, TextChangedEventArgs e)
        {
            int idx = usrCtrlAlt.selectedIdx;
            if (-1 != idx)
            {
                StrctAlternative a = new StrctAlternative();
                a = Alternatives.listAl[idx];
                a.name = txbNameAl.Text;
                Alternatives.update(idx, a);

                usrCtrlAlt.Update(idx, a.name);
                //drawAhpHierarchy();
            }


        }

        private void txbCostAl_TextChanged(object sender, TextChangedEventArgs e)
        {
            int idx = usrCtrlAlt.selectedIdx;
            if (-1 != idx)
            {
                StrctAlternative a = new StrctAlternative();
                a = Alternatives.listAl[idx];

                try
                {
                    double value = Convert.ToDouble(txbCostAl.Text);
                    a.cost1 = value;
                }
                catch (FormatException)
                {
                    //MessageBox.Show("The value " + txb.Text + " is not in a recognizable format.");
                    txbCostAl.Text = a.cost1.ToString();
                }

                Alternatives.update(idx, a);
            }

        }

        private void txbCostRAl_TextChanged(object sender, TextChangedEventArgs e)
        {
            int idx = usrCtrlAlt.selectedIdx;
            if (-1 != idx)
            {
                StrctAlternative a = new StrctAlternative();
                a = Alternatives.listAl[idx];

                try
                {
                    double value = Convert.ToDouble(txbCostRAl.Text);
                    a.cost2 = value;
                }
                catch (FormatException)
                {
                    //MessageBox.Show("The value " + txb.Text + " is not in a recognizable format.");
                    txbCostRAl.Text = a.cost2.ToString();
                }

                Alternatives.update(idx, a);
            }

        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();

        }

        private void Saveas_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Project1";
            dlg.DefaultExt = ".ahp";
            dlg.Filter = "AHP project (.ahp)|*.ahp";
            if (dlg.ShowDialog() == true)
                filePath = dlg.FileName;

            //create .ahp file and save
            CreatXml(filePath);
            MessageBox.Show("saved!");

        }
    }

    struct StrctFreedomLocation
    {
        public int row;
        public int col;
        public int freedom;
    }
}
