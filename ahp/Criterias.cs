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
    public class Criterias : IEquatable<Criterias>
    {
        public List<StrctCriteria> listCr = new List<StrctCriteria>();
        // pairwise comparison matrix
        public double[,] mtx;
        // controls in matrix views
        public Object[,] mtxCtrls;

        private static string[] strValues = { "1/9", "1/8", "1/7", "1/6", "1/5", "1/4", "1/3", "1/2", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        private static double[] dblValues = { 0.1111111, 0.125, 0.1428571, 0.1666666, 0.2, 0.25, 0.3333333, 0.5, 1, 2, 3, 4, 5, 6, 7, 8, 9 };

        // consistency ratio
        public double CR;

        // identify inconsistent row and column
        public int inconsistentRow;
        public int inconsistentCol;

        // constructor
        public Criterias()
        {
            inconsistentRow = -1;
            inconsistentCol = -1;

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
        public bool Add(StrctCriteria c)
        {
            if (IsDuplicated(c))
                return false;
            else
            {
                listCr.Add(c);
                NewMtx();
                return true;
            } 
        }

        // delete item by index
        public void Delete(int index)
        {
            listCr.RemoveAt(index);
            NewMtx();
        }

        // update an item
        public void update(int index, StrctCriteria c)
        {
            listCr.RemoveAt(index);
            listCr.Insert(index, c);
        }

        // generate pairwise comparison matrix view
        public void GenerateMtxView(Grid mtxGrid, double totalWidth, double totalHeight)
        {
            int i, j;
            // initial cell width and height
            double cellWidth = 120;
            double cellHeight = 60;
            // total width and height
            //double totalWidth = tabMain.ActualWidth - 200;
            //double totalHeight = tabMain.ActualHeight - 120;

            // clear grid and redraw on it.
            mtxGrid.Children.Clear();
            mtxGrid.RowDefinitions.Clear();
            mtxGrid.ColumnDefinitions.Clear();
            
            if (totalWidth < (listCr.Count + 1) * cellWidth)
            {
                cellWidth = totalWidth / (listCr.Count + 1);
                cellHeight = cellWidth/2;
            }

            if (totalHeight < (listCr.Count + 1) * cellHeight)
            {
                cellHeight = totalHeight/(listCr.Count + 1);
                cellWidth = cellHeight*2;
            }

            for (i = 0; i < listCr.Count + 1; i++)
            {
                RowDefinition row = new RowDefinition();
                row.Height = new GridLength(cellHeight);
                ColumnDefinition col = new ColumnDefinition();
                col.Width = new GridLength(cellWidth);

                mtxGrid.RowDefinitions.Add(row);
                mtxGrid.ColumnDefinitions.Add(col);
            }
            (mtxGrid.Parent as Border).Width = (listCr.Count + 1) * cellWidth;
            (mtxGrid.Parent as Border).Height = (listCr.Count + 1) * cellHeight;

            if (listCr.Count == 0)
                (mtxGrid.Parent as Border).Visibility = Visibility.Hidden;
            else
                (mtxGrid.Parent as Border).Visibility = Visibility.Visible;

            // create new matrix  of controls
            mtxCtrls = new Object[listCr.Count, listCr.Count];

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

                mtxCtrls[i - 1, i - 1] = (Object)txt;

                    txt = new TextBlock();
                    txt.Text = listCr.ElementAt(i - 1).name;
                    mtxGrid.Children.Add(txt);
                    Grid.SetRow(txt, 0);
                    Grid.SetColumn(txt, i);
                    txt.HorizontalAlignment = HorizontalAlignment.Center;
                    txt.VerticalAlignment = VerticalAlignment.Center;

                    txt = new TextBlock();
                    txt.Text = listCr.ElementAt(i - 1).name;
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
                        nupCtrl.Width = cellWidth;
                        nupCtrl.RowID = i - 1;
                        nupCtrl.ColId = j - 1;
                        nupCtrl.dblValue = mtx[i - 1, j - 1];
                        nupCtrl.ValueChangedEvent += OnNumericUpDownValueChangedEvent;
                        mtxGrid.Children.Add(nupCtrl);
                        Grid.SetRow(nupCtrl, i);
                        Grid.SetColumn(nupCtrl, j);
                        nupCtrl.HorizontalAlignment = HorizontalAlignment.Center;
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
                    }
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
            double[] RI = { 0.0, 0.0, 0.0, 0.58, 0.9, 1.12, 1.24, 1.32, 1.41, 1.45, 1.49 };

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

            for(int i = 0; i < listCr.Count; i++)
            {
                listCr[i] = new StrctCriteria(listCr[i].name, w[i]);
            }
        }

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
        public double[] Identify()
        {
            double[] result = new double[listCr.Count];
            double[,] C = new double[listCr.Count, listCr.Count];
            double[,] sqrA = new double[listCr.Count, listCr.Count];
            int i, j, k;
            double max = 0;
            int row = 0, col = 0;
            double[] b = new double[listCr.Count];

            for (i = 0; i < listCr.Count; i++)
                for(j = 0; j < listCr.Count; j++)
                {
                    sqrA[i, j] = 0;
                    for (k = 0; k < listCr.Count; k++)
                        sqrA[i, j] += mtx[i, k] * mtx[k, j];
                    C[i, j] = sqrA[i, j] - 4 * mtx[i, j];

                }

            for (i = 0; i < listCr.Count; i++)
                for (j = 0; j < listCr.Count; j++)
                {
                    if(max < Math.Abs(C[i, j]))
                    {
                        max = Math.Abs(C[i, j]);
                        row = i;
                        col = j;
                    }
                }

            for(i = 0; i < listCr.Count; i++)
            {
                b[i] = mtx[row, i] * mtx[i, col];

                result[i] = b[i] - mtx[row, col];
            }

            inconsistentRow = row;
            inconsistentCol = col;
            return result;
        }

        public void Highlight(int idx)
        {
            if ((mtxCtrls[inconsistentRow, idx] as TextBlock) == null)
                (mtxCtrls[inconsistentRow, idx] as NumericUpDown).Background = Brushes.Pink;
            else
                (mtxCtrls[inconsistentRow, idx] as TextBlock).Background = Brushes.Pink;

            if ((mtxCtrls[idx, inconsistentCol] as TextBlock) == null)
                (mtxCtrls[idx, inconsistentCol] as NumericUpDown).Background = Brushes.Pink;
            else
                (mtxCtrls[idx, inconsistentCol] as TextBlock).Background = Brushes.Pink;

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
}
