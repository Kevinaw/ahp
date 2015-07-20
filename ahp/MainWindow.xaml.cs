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

        public MainWindow()
        {
            InitializeComponent();
            isOpeningPrj = false;
            filePath = null;

#if DEBUG
            Console.WriteLine("debug mode");
#endif

        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            switch(tabMain.SelectedIndex)
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
            //drawAhpHierarchy();
        }
        
        private void tabMain_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch(tabMain.SelectedIndex)
            {
                case 0:
                    break;
                case 1:
                    break;
                case 2:
                    Criterias.GenerateMtxView(mtxGrid, tabMain);
                    break;
                case 3:
                    Alternatives.GenerateMtxView(grdAC, tabMain, Criterias);
                    break;
                case 4:
                    DrawResultMtx();
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

            double gridWidth = 100;
            double gridHeight = 50;
            double x = tabMain.ActualWidth -80;
            double y = tabMain.ActualHeight -100;
            if (x < 2 * (Alternatives.listAl.Count + 1) * gridWidth)
            {
                gridWidth = x / (Alternatives.listAl.Count + 1) / 2;
                gridHeight = gridWidth / 2;
            }
            if(y < (Criterias.listCr.Count + 3) * gridHeight)
            {
                gridHeight = y/ (Criterias.listCr.Count + 2);
                gridWidth = 2 * gridHeight;
            }

            for (i = 0; i < Criterias.listCr.Count + 2; i++)
            {
                RowDefinition row = new RowDefinition();
                row.Height = new GridLength(gridHeight);

                grdRst.RowDefinitions.Add(row);
            }
            bdrRst.Height = (Criterias.listCr.Count + 2)*gridHeight;

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
            double recHeight = cvsHeight / 6;
            if (this.txbGoal.Text != "")
            {
                rec = new Rectangle();
                rec.Width = cvsWidth * 4 / 5 / 4;
                rec.Height = recHeight;
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
                    this.cvsHrcGraph.Children.Add(rec);

                    txt = new TextBlock();
                    txt.Text = Criterias.listCr[i].name + "\n" + Criterias.listCr[i].weight.ToString();
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
                    this.cvsHrcGraph.Children.Add(rec);

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
                    this.cvsHrcGraph.Children.Add(grd);
                }
            }
        }

        private void cvsHrcGraph_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //drawAhpHierarchy();

        }

        private void txbGoal_TextChanged(object sender, TextChangedEventArgs e)
        {
            //drawAhpHierarchy();
        }

        private void saveBtn_Click(object sender, RoutedEventArgs e)
        {
            // code needed here, verify all the result to save

            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "Project1";
            dlg.DefaultExt = ".ahp";
            dlg.Filter = "AHP project (.ahp)|*.ahp";
            if (filePath == null && dlg.ShowDialog() == true)
                filePath = dlg.FileName;
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
            //drawAhpHierarchy();

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

            for(int i = 0; i < Criterias.listCr.Count; i++)
                for(int j = 0; j < Criterias.listCr.Count; j++)
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
                    element.InnerText =Alternatives.mtxAC[i, j].ToString();

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
            if(alternatives.HasChildNodes)
            {
                for(i = 0; i < alternatives.ChildNodes.Count; i++)
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
                    c.weight =Convert.ToDouble(criterias.ChildNodes[i].Attributes["weight"].Value);
                    Criterias.Add(c);

                    usrCtrlCr.AddItem(c.name);
                }
            }

            // criteria VS goal comparison mtx
            XmlNode cvgs = doc.SelectSingleNode("descendant::CriteriaVsGoalMatrix");
            Criterias.mtx = new double[criterias.ChildNodes.Count, criterias.ChildNodes.Count];
            for(i = 0; i < criterias.ChildNodes.Count; i++)
                for(j = 0; j < criterias.ChildNodes.Count; j++)
                {
                    Criterias.mtx[i, j] = Convert.ToDouble(cvgs.ChildNodes[i*criterias.ChildNodes.Count + j].InnerText);
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
            //drawAhpHierarchy();

            isOpeningPrj = false ;
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
                    if(c.name != criterias.ChildNodes[i].InnerText)
                        return true;
                    if(String.Format("{0:#.00}", c.weight) != String.Format("{0:#.00}", Convert.ToDouble(criterias.ChildNodes[i].Attributes["weight"].Value)))
                        return true;
                }
            }

            // criteria VS goal comparison mtx
            XmlNode cvgs = doc.SelectSingleNode("descendant::CriteriaVsGoalMatrix");
            if (Criterias.mtx.Length != criterias.ChildNodes.Count * criterias.ChildNodes.Count)
                return true;
            for (i = 0; i < criterias.ChildNodes.Count; i++)
                for (j = 0; j < criterias.ChildNodes.Count; j++)
                {
                    if(Criterias.mtx[i, j] != Convert.ToDouble(cvgs.ChildNodes[i * criterias.ChildNodes.Count + j].InnerText))
                        return true;
                }

            // alternative vs criteria comparison mtx
            XmlNode avgs = doc.SelectSingleNode("descendant::AlternativeVsGoalMatrix");
            if (Alternatives.mtxAC.Length != criterias.ChildNodes.Count * alternatives.ChildNodes.Count)
                return true;
            for (i = 0; i < criterias.ChildNodes.Count; i++)
                for (j = 0; j < alternatives.ChildNodes.Count; j++)
                {
                    if(Alternatives.mtxAC[i, j] != Convert.ToInt32(avgs.ChildNodes[i * alternatives.ChildNodes.Count + j].InnerText))
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

            //drawAhpHierarchy();
        }

        private void onListItemDeletedEventCr(object sender, ListItemChangedEventArgs e)
        {
            Criterias.Delete(e.Idx);
            //drawAhpHierarchy();
        }

        private void onListItemUpdatedEventCr(object sender, ListItemChangedEventArgs e)
        {
            StrctCriteria c = new StrctCriteria();
            c.name = e.NewName;
            c.weight = Criterias.listCr[e.Idx].weight;

            Criterias.update(e.Idx, c);
            //drawAhpHierarchy();
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
            StrctAlternative a = new StrctAlternative();
            a.name = e.NewName;
            a.cost1 = Alternatives.listAl[e.Idx].cost1;
            a.cost2 = Alternatives.listAl[e.Idx].cost2;

            Alternatives.update(e.Idx, a);
            //drawAhpHierarchy();
        }

        private void onSelectionChangedEvent(object sender, EventArgs e)
        {
            if(isOpeningPrj ==  false)
            {
                int idx = usrCtrlAlt.selectedIdx;
                if(idx >= 0)
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
            if(-1 != idx)
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
            if(-1 != idx)
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
    }
}
