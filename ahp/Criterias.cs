using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using MyNumericUpDownControll;
using System.Windows.Media;

namespace ahp
{
    public class Criterias
    {
        public List<StrctCriteria> listCr = new List<StrctCriteria>();
        // pairwise comparison matrix
        public double[,] mtx;
        // controls in matrix views
        public Object[,] mtxCtrls;
        public object[] headerCtrls;

        private static string[] strValues = { "1/9", "1/8", "1/7", "1/6", "1/5", "1/4", "1/3", "1/2", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        private static double[] dblValues = { 1 / 9, 1 / 8, 1 / 7, 1 / 6, 1 / 5, 1 / 4, 1 / 3, 1 / 2, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

        // consistency ratio
        public double CR;

        // intermediate results for identification
        public List<StrctCalUnitInIdentify> listIdentifyData = new List<StrctCalUnitInIdentify>();
        public List<StrctCell> inconsistentCells = new List<StrctCell>();

        // constructor
        public Criterias()
        {
            inconsistentCells.Clear();

        }

        // constructor
        public Criterias(Criterias c)
        {
            inconsistentCells.Clear();

            this.listCr = c.listCr.ToList();
            this.mtx = new double[this.listCr.Count, this.listCr.Count];
            Array.Copy(c.mtx, this.mtx, c.mtx.Length);
        }

        // two criterias with the same name
        private bool IsDuplicated(StrctCriteria c)
        {
            if (listCr.Exists(x => x.name == c.name))
                return true;
            else
                return false;
        }

        // generate matrix with initial value all 1s
        private void NewMtx()
        {
            if (listCr.Count != 0)
            {
                mtx = new double[listCr.Count, listCr.Count];
                for (int i = 0; i < listCr.Count; i++)
                    for (int j = 0; j < listCr.Count; j++)
                        mtx[i, j] = 1;
            }
            else
                mtx = null;
        }

        // add item
        public bool Add(StrctCriteria c, bool openingProject = false)
        {
            if (IsDuplicated(c))
                return false;
            else
            {
                listCr.Add(c);
                if (openingProject == false)
                {
                    // add new cells in matrix
                    var mtxTemp = new double[listCr.Count, listCr.Count];
                    for (int i = 0; i < listCr.Count; i++)
                        for (int j = 0; j < listCr.Count; j++)
                            if (i == listCr.Count - 1 || j == listCr.Count - 1)
                                mtxTemp[i, j] = 1;
                            else
                                mtxTemp[i, j] = mtx[i, j];
                    mtx = mtxTemp;
                }
                return true;
            }
        }

        // delete item by index
        public void Delete(int index)
        {
            listCr.RemoveAt(index);

            if (listCr.Count() == 0)
                return;

            var mtxTemp = new double[listCr.Count, listCr.Count];
            for (int i = 0; i < listCr.Count; i++)
                for (int j = 0; j < listCr.Count; j++)
                    if (i >= index && j < index)
                        mtxTemp[i, j] = mtx[i + 1, j];
                    else if (i < index && j >= index)
                        mtxTemp[i, j] = mtx[i, j + 1];
                    else if (i > index && j > index)
                        mtxTemp[i, j] = mtx[i + 1, j + 1];
                    else
                        mtxTemp[i, j] = mtx[i, j];
            mtx = mtxTemp;
        }

        // update an item
        public void update(int index, StrctCriteria c)
        {
            listCr.RemoveAt(index);
            listCr.Insert(index, c);
        }

        // generate pairwise comparison matrix view
        public void GenerateMtxView(Grid mtxGrid)
        {
            int i, j;
            // initial cell width and height
            //double cellMaxLength = 200;
            double paddingValue = 5;
            // total width and height

            // golden ratio. 8 is full screen.
            mtxGrid.Width = ((MainWindow)Application.Current.MainWindow).grdMain.ActualWidth * 0.7 - 30;
            mtxGrid.Height = ((MainWindow)Application.Current.MainWindow).grdMain.ActualHeight - 30;

            if (mtxGrid.Width > 1.618 * mtxGrid.Height)
                mtxGrid.Width = mtxGrid.Height * 1.618;
            else
                mtxGrid.Height = mtxGrid.Width / 1.618;

            double r = ((MainWindow)Application.Current.MainWindow).sldGrdSz.Value;
            mtxGrid.Width = mtxGrid.Width * r / 8;
            mtxGrid.Height = mtxGrid.Height * r / 8;


            double cellWidth = mtxGrid.Width / (listCr.Count + 1);
            double cellHeight = mtxGrid.Height/(listCr.Count + 1);

            // clear grid and redraw on it.
            mtxGrid.Children.Clear();
            mtxGrid.RowDefinitions.Clear();
            mtxGrid.ColumnDefinitions.Clear();

            for (i = 0; i < listCr.Count + 1; i++)
            {
                RowDefinition row = new RowDefinition();
                //row.Height = new GridLength(cellHeight);
                row.Height = new GridLength(cellHeight);
                //row.MinHeight = cellHeight;
                ColumnDefinition col = new ColumnDefinition();
                //col.Width = new GridLength(cellWidth);
                col.Width = new GridLength(cellWidth);
                //col.MinWidth = cellWidth;

                mtxGrid.RowDefinitions.Add(row);
                mtxGrid.ColumnDefinitions.Add(col);
            }
            //(mtxGrid.Parent as Border).Width = (listCr.Count + 1) * cellWidth;
            //(mtxGrid.Parent as Border).Height = (listCr.Count + 1) * cellHeight;

            if (listCr.Count == 0)
                (mtxGrid.Parent as Border).Visibility = Visibility.Hidden;
            else
                (mtxGrid.Parent as Border).Visibility = Visibility.Visible;

            // create new matrix  of controls
            mtxCtrls = new Object[listCr.Count, listCr.Count];
            headerCtrls = new object[listCr.Count];

            // redraw matrix
            for (i = 1; i < listCr.Count + 1; i++)
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
                txt.Padding = new Thickness(paddingValue);


                mtxCtrls[i - 1, i - 1] = (Object)txt;

                txt = new TextBlock();
                txt.Text = listCr.ElementAt(i - 1).name;
                txt.Padding = new Thickness(paddingValue);
                //txt.MaxWidth = cellMaxLength;
                txt.TextWrapping = TextWrapping.Wrap;
                mtxGrid.Children.Add(txt);
                Grid.SetRow(txt, 0);
                Grid.SetColumn(txt, i);
                txt.HorizontalAlignment = HorizontalAlignment.Center;
                txt.VerticalAlignment = VerticalAlignment.Center;
                headerCtrls[i - 1] = (Object)txt;

                txt = new TextBlock();
                txt.Text = listCr.ElementAt(i - 1).name;
                txt.Padding = new Thickness(paddingValue);
                //txt.MaxWidth = cellMaxLength;
                txt.TextWrapping = TextWrapping.Wrap;
                mtxGrid.Children.Add(txt);
                Grid.SetRow(txt, i);
                Grid.SetColumn(txt, 0);
                txt.HorizontalAlignment = HorizontalAlignment.Center;
                txt.VerticalAlignment = VerticalAlignment.Center;


                // starting from first line;
                if (i < listCr.Count)
                {
                    for (j = i + 1; j < listCr.Count + 1; j++)
                    {
                        NumericUpDown nupCtrl = new NumericUpDown();
                        nupCtrl.RowID = i - 1;
                        nupCtrl.ColId = j - 1;
                        nupCtrl.dblValue = mtx[i - 1, j - 1];
                        nupCtrl.ValueChangedEvent += OnNumericUpDownValueChangedEvent;
                        mtxGrid.Children.Add(nupCtrl);
                        Grid.SetRow(nupCtrl, i);
                        Grid.SetColumn(nupCtrl, j);
                        nupCtrl.HorizontalAlignment = HorizontalAlignment.Stretch;
                        nupCtrl.VerticalAlignment = VerticalAlignment.Center;
                        mtxCtrls[i - 1, j - 1] = (Object)nupCtrl;

                        txt = new TextBlock();
                        txt.Text = nupCtrl.strValueR;
                        mtxGrid.Children.Add(txt);
                        Grid.SetRow(txt, j);
                        Grid.SetColumn(txt, i);
                        txt.HorizontalAlignment = HorizontalAlignment.Center;
                        txt.VerticalAlignment = VerticalAlignment.Center;
                        mtxCtrls[j - 1, i - 1] = (Object)txt;
                        txt.Padding = new Thickness(paddingValue);
                    }
                }
            }

        }

        public void updateHeaderTxtwithWeight()
        {
            if ((headerCtrls.Count() != 0))
            {
                for (int i = 0; i < headerCtrls.Count(); i++)
                {
                    (headerCtrls[i] as TextBlock).Text = listCr[i].name + "\r\n" + String.Format("{0:P1}", listCr.ElementAt(i).weight);
                }
            }
        }
        // calculate weights and CR
        public void AhpEval()
        {
            int n = (int)Math.Sqrt(mtx.Length);
            int row = n;
            int column = n;
            double[] mul_column = new double[row];
            double[] rot_column = new double[row];
            double[] w = new double[row];
            double[] aw = new double[row];
            double[] aw_w = new double[row];
            double CI;
            //double CR;
            double sum = 0;
            double average;
            double[] RI = { 0.0, 0.0, 0.0, 0.58, 0.9, 1.12, 1.24, 1.32, 1.41, 1.45, 1.51 };

            inconsistentCells.Clear();

            for (int i = 0; i < row; i++)
            {
                mul_column[i] = 1;
                for (int j = 0; j < column; j++)
                    mul_column[i] *= mtx[i, j];
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
                    aw[i] += mtx[i, j] * w[j];

                aw_w[i] = aw[i] / w[i];
                sum += aw_w[i];
            }
            average = sum / row;

            CI = (average - n) / (n - 1);
            CR = CI / RI[n];

            CR = Math.Round(CR, 3);
            for (int i = 0; i < listCr.Count; i++)
            {
                listCr[i] = new StrctCriteria(listCr[i].name, w[i]);
            }
        }

        public bool Equals(Criterias other)
        {
            if (other == null)
                return false;

            for (int i = 0; i < other.mtx.GetLength(0); i++)
                for (int j = 0; j < other.mtx.GetLength(1); j++)
                {
                    if (String.Format("{0:#.00}", mtx[i, j]) == String.Format("{0:#.00}", other.mtx[i, j]))
                        return false;
                }
            return true;

        }

        /*
                /// <summary>
                /// Serialize an object
                /// </summary>
                /// <param name="obj"></param>
                /// <returns></returns>
                /// http://www.dotnetfunda.com/articles/show/98/how-to-serialize-and-deserialize-an-object-into-xml
                public string SerializeAnObject()
                {
                    System.Xml.XmlDocument doc = new XmlDocument();
                    System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(this.GetType());
                    System.IO.MemoryStream stream = new System.IO.MemoryStream();
                    try
                    {
                        serializer.Serialize(stream, this);
                        stream.Position = 0;
                        doc.Load(stream);
                        return doc.InnerXml;
                    }
                    catch
                    {
                        throw;
                    }
                    finally
                    {
                        stream.Close();
                        stream.Dispose();
                    }
                }

                /// <summary>
                /// DeSerialize an object
                /// </summary>
                /// <param name="xmlOfAnObject"></param>
                /// <returns></returns>
                /// http://www.dotnetfunda.com/articles/show/98/how-to-serialize-and-deserialize-an-object-into-xml
                private object DeSerializeAnObject(string xmlOfAnObject)
                {
                    Criterias myObject = new Criterias();
                    System.IO.StringReader read = new StringReader(xmlOfAnObject);
                    System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(myObject.GetType());
                    System.Xml.XmlReader reader = new XmlTextReader(read);
                    try
                    {
                        myObject = (Criterias)serializer.Deserialize(reader);
                        return myObject;
                    }
                    catch
                    {
                        throw;
                    }
                    finally
                    {
                        reader.Close();
                        read.Close();
                        read.Dispose();
                    }
                }

                public bool Equals(Criterias other)
                {
                    if (other == null)
                        return false;

                    if (this.listCr.Equals(other.listCr) && this.mtx.Equals(other.mtx))
                        return true;
                    else
                        return false;
                }

                public override bool Equals(Object obj)
                {
                    if (obj == null)
                        return false;

                    Criterias cObj = obj as Criterias;
                    if (cObj == null)
                        return false;
                    else
                        return Equals(cObj);
                }

                public override int GetHashCode()
                {
                    return this.listCr.GetHashCode();
                }

                public static bool operator ==(Criterias c1, Criterias c2)
                {
                    if (c1 == null || c2 == null)
                        return Object.Equals(c1, c2);

                    return c1.Equals(c2);
                }

                public static bool operator !=(Criterias c1, Criterias c2)
                {
                    if (c1 == null || c2 == null)
                        return !Object.Equals(c1, c2);

                    return !(c1.Equals(c2));
                }
        */
        private void OnNumericUpDownValueChangedEvent(object sender, EventArgs e)
        {
            NumericUpDown nud = sender as NumericUpDown;
            if (nud == null)
                return;

            mtx[nud.RowID, nud.ColId] = nud.dblValue;
            mtx[nud.ColId, nud.RowID] = nud.dblValueR;

            (mtxCtrls[nud.ColId, nud.RowID] as TextBlock).Text = nud.strValueR;

        }

        public void Clear()
        {
            listCr.Clear();
            mtx = null;
            mtxCtrls = null;
            CR = 0;
        }

        // identify the inconsistency cells
        public void Identify()
        {
            double[] result = new double[listCr.Count];
            double[,] C = new double[listCr.Count, listCr.Count];
            double[,] sqrA = new double[listCr.Count, listCr.Count];
            int i, j, k;
            double max = 0;
            double[] b = new double[listCr.Count];

            listIdentifyData.Clear();

            //List<StrctCalUnitInIdentify> listIdentify = new List<StrctCalUnitInIdentify>();

            for (i = 0; i < listCr.Count; i++)
                for (j = 0; j < listCr.Count; j++)
                {
                    sqrA[i, j] = 0;
                    for (k = 0; k < listCr.Count; k++)
                        sqrA[i, j] += mtx[i, k] * mtx[k, j];
                    C[i, j] = sqrA[i, j] - 4 * mtx[i, j];
                }

            for (i = 0; i < listCr.Count; i++)
                for (j = 0; j < listCr.Count; j++)
                {
                    if (max < Math.Abs(C[i, j]))
                    {
                        max = Math.Abs(C[i, j]);
                    }
                }

            // There may be more than one value equal to maximum
            for (i = 0; i < listCr.Count; i++)
                for (j = 0; j < listCr.Count; j++)
                {
                    if (max == Math.Abs(C[i, j]))
                    {
                        listIdentifyData.Add(new StrctCalUnitInIdentify() { row = i, col = j });
                    }
                }

            // Every max value should do the following calculation
            for (k = 0; k < listIdentifyData.Count; k++)
            {
                for (i = 0; i < listCr.Count; i++)
                {
                    b[i] = mtx[listIdentifyData[k].row, i] * mtx[i, listIdentifyData[k].col];
                    result[i] = b[i] - mtx[listIdentifyData[k].row, listIdentifyData[k].col];
                }


                StrctCalUnitInIdentify tmp = listIdentifyData[k];
                tmp.idxF = new List<int>();
                WindowIdentify idW = new WindowIdentify(result);
                idW.FontSize = Application.Current.MainWindow.FontSize;
                idW.FontFamily = Application.Current.MainWindow.FontFamily;
                if (idW.ShowDialog() == true)
                {
                    tmp.idxF = idW.listSelectedIdxs;
                }
                listIdentifyData[k] = tmp;
                /*
                StrctCalUnitInIdentify tmp = listIdentify[k];
                tmp.idxF = new List<int>();
                for (j = 0; j < result.Count(); j++)
                {
                    if (result[j] != 0)
                        tmp.idxF.Add(j);
                }
                listIdentify[k] = tmp;
                */
            }


            PopulateIncosistentCells(listIdentifyData);
            //return result;
        }

        private void PopulateIncosistentCells(List<StrctCalUnitInIdentify> listIdentify)
        {
            inconsistentCells.Clear();
            for (int i = 0; i < listIdentify.Count(); i++)
            {
                for (int j = 0; j < listIdentify[i].idxF.Count(); j++)
                {
                    if (listIdentify[i].row != listIdentify[i].idxF[j])
                    {
                        // row number should be smaller
                        int rowNumber = listIdentify[i].row < listIdentify[i].idxF[j] ? listIdentify[i].row : listIdentify[i].idxF[j];
                        int colNumber = listIdentify[i].row > listIdentify[i].idxF[j] ? listIdentify[i].row : listIdentify[i].idxF[j];
                        inconsistentCells.Add(new StrctCell() { row = rowNumber, col = colNumber });
                    }

                    if (listIdentify[i].col != listIdentify[i].idxF[j])
                    {
                        int rowNumber = listIdentify[i].col < listIdentify[i].idxF[j] ? listIdentify[i].col : listIdentify[i].idxF[j];
                        int colNumber = listIdentify[i].col > listIdentify[i].idxF[j] ? listIdentify[i].col : listIdentify[i].idxF[j];
                        inconsistentCells.Add(new StrctCell() { row = rowNumber, col = colNumber });
                    }
                }
            }

            // Remove duplicate
            List<StrctCell> tmpCells = new List<StrctCell>();
            for (int i = 0; i < inconsistentCells.Count(); i++)
            {
                if (tmpCells.Count() == 0)
                    tmpCells.Add(inconsistentCells.ElementAt(i));
                else
                {
                    bool hasDuplicate = false;
                    for (int j = 0; j < tmpCells.Count(); j++)
                    {
                        if (inconsistentCells[i].row == tmpCells[j].row && inconsistentCells[i].col == tmpCells[j].col)
                        {
                            hasDuplicate = true;
                            break;
                        }
                    }
                    if (hasDuplicate == false)
                        tmpCells.Add(inconsistentCells.ElementAt(i));
                }
            }

            inconsistentCells.Clear();
            inconsistentCells = tmpCells;
        }

        public void Highlight()
        {
            ClearHighlight();

            for (int i = 0; i < inconsistentCells.Count(); i++)
            {
                (mtxCtrls[inconsistentCells[i].row, inconsistentCells[i].col] as NumericUpDown).Background = Brushes.Pink;
            }
        }

        public void ClearHighlight()
        {
            for (int i = 0; i < listCr.Count(); i++)
            {
                for (int j = i + 1; j < listCr.Count(); j++)
                    (mtxCtrls[i, j] as NumericUpDown).Background = (Brush)(new BrushConverter()).ConvertFromString("#FFE9F0F7");
            }
        }

    }

    public struct StrctCriteria
    {
        public string name;
        public double weight;

        public StrctCriteria(string s, double d)
        {
            name = s;
            weight = d;
        }
    }

    public struct StrctCalUnitInIdentify // During identify inconsistency cells, one max value in C is related to one row number and one column number, then two identified cells
    {
        //public double max;
        public double[] f;
        public int row;
        public int col;
        public List<int> idxF;
        // the inconsistency cells should be [row, idxF] and [idxF, col];
    }

    public struct StrctCell
    {
        public int row;
        public int col;
    }

}
