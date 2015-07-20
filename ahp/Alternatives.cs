using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Controls;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace ahp
{
    public class Alternatives : IEquatable<Alternatives>
    {
        public List<StrctAlternative> listAl = new List<StrctAlternative>();
        // pairwise comparison matrix
        public int[,] mtxAC;

        // constructor
        public Alternatives()
        {

        }

        // two criterias with the same name
        private bool IsDuplicated(StrctAlternative c)
        {
            if (listAl.Exists(x => x.name == c.name))
                return true;
            else
                return false;
        }

        // generate matrix with initial value all 1s
        private void NewMtx(Criterias c)
        {
            if (c.listCr.Count != 0 && this.listAl.Count != 0)
            {
                mtxAC = new int[c.listCr.Count, this.listAl.Count];
                for (int i = 0; i < c.listCr.Count; i++)
                    for (int j = 0; j < this.listAl.Count; j++)
                        mtxAC[i, j] = 0;
            }
            else
                mtxAC = null;
        }

        // add item
        public bool Add(StrctAlternative c)
        {
            if (IsDuplicated(c))
                return false;
            else
            {
                listAl.Add(c);
                return true;
            }
        }

        // delete item by index
        public void Delete(int index)
        {
            listAl.RemoveAt(index);
        }

        // update an item
        public void update(int index, StrctAlternative a)
        {
            listAl.RemoveAt(index);
            listAl.Insert(index, a);
        }

        // generate pairwise comparison matrix view
        public void GenerateMtxView(Grid grdAC, TabControl tabMain, Criterias c)
        {
            int i, j;
            double cellWidth = 100;
            double cellHeight = 50;
            double totalWidth = tabMain.ActualWidth -80;
            double totalHeight = tabMain.ActualHeight - 100;

            grdAC.Children.Clear();

            if (totalWidth < (listAl.Count + 1) * cellWidth)
            {
                cellWidth = (totalWidth - 10) / (listAl.Count + 1);
                cellHeight = cellWidth/2;
            }

            if (totalHeight < (listAl.Count + 1) * cellHeight)
            {
                cellHeight = (totalHeight - 10) / (listAl.Count + 1);
                cellWidth = cellHeight * 2;
            }

            for (i = 0; i < this.listAl.Count + 1; i++)
            {
                ColumnDefinition col = new ColumnDefinition();
                col.Width = new GridLength(cellWidth);

                grdAC.ColumnDefinitions.Add(col);
            }
            (grdAC.Parent as Border).Width = (this.listAl.Count + 1) * cellWidth;

            for (i = 0; i < c.listCr.Count + 1; i++)
            {
                RowDefinition row = new RowDefinition();
                row.Height = new GridLength(cellHeight);

                grdAC.RowDefinitions.Add(row);
            }
            (grdAC.Parent as Border).Height = (c.listCr.Count + 1) * cellHeight;

            if (c.listCr.Count == 0 && listAl.Count == 0)
                (grdAC.Parent as Border).Visibility = Visibility.Hidden;
            else
                (grdAC.Parent as Border).Visibility = Visibility.Visible;

            if (mtxAC == null || mtxAC.Length != c.listCr.Count * listAl.Count)
            {
                NewMtx(c);
            }

            // redraw matrix
            // fill criteria headers & related weights
            for (i = 0; i < c.listCr.Count; i++)
            {
                // criteria
                TextBlock txt;
                txt = new TextBlock();
                txt.Text = c.listCr.ElementAt(i).name;
                grdAC.Children.Add(txt);
                Grid.SetRow(txt, i + 1);
                Grid.SetColumn(txt, 0);
                txt.HorizontalAlignment = HorizontalAlignment.Center;
                txt.VerticalAlignment = VerticalAlignment.Center;
            }

            // fill all the alternative names
            for (i = 0; i < this.listAl.Count; i++)
            {
                TextBlock txt = new TextBlock();
                txt.Text = listAl.ElementAt(i).name;
                grdAC.Children.Add(txt);
                Grid.SetRow(txt, 0);
                Grid.SetColumn(txt, i + 1);
                txt.HorizontalAlignment = HorizontalAlignment.Center;
                txt.VerticalAlignment = VerticalAlignment.Center;

                // fill scores
                for (j = 0; j < c.listCr.Count; j++)
                {
                    // score
                    TextBox tb = new TextBox();
                    tb.Text = mtxAC[j, i].ToString();
                    tb.Name = "txtScore" + j.ToString() + i.ToString();
                    tb.VerticalContentAlignment = VerticalAlignment.Center;
                    tb.VerticalAlignment = VerticalAlignment.Center;
                    tb.HorizontalAlignment = HorizontalAlignment.Center;
                    tb.Width = 4 * cellWidth / 5;
                    tb.Height = tb.Width / 2;
                    tb.TextChanged += new TextChangedEventHandler(txtScore_TextChanged);
                    tb.GotFocus += new RoutedEventHandler(txtScore_GotFocus);
                    tb.MaxLength = 3;
                    grdAC.Children.Add(tb);
                    Grid.SetRow(tb, j + 1);
                    Grid.SetColumn(tb, i + 1);
                }
            }
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

            int rowN = Convert.ToInt32(txb.Name.Substring(8, 1));
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
            Alternatives myObject = new Alternatives();
            System.IO.StringReader read = new StringReader(xmlOfAnObject);
            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(myObject.GetType());
            System.Xml.XmlReader reader = new XmlTextReader(read);
            try
            {
                myObject = (Alternatives)serializer.Deserialize(reader);
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

        public bool Equals(Alternatives other)
        {
            if (other == null)
                return false;

            if (this.listAl.Equals(other.listAl) && this.mtxAC.Equals(other.mtxAC))
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
            return this.listAl.GetHashCode();
        }

        public static bool operator ==(Alternatives c1, Alternatives c2)
        {
            if (c1 == null || c2 == null)
                return Object.Equals(c1, c2);

            return c1.Equals(c2);
        }

        public static bool operator !=(Alternatives c1, Alternatives c2)
        {
            if (c1 == null || c2 == null)
                return !Object.Equals(c1, c2);

            return !(c1.Equals(c2));
        }

        public void Clear()
        {
            listAl.Clear();
            mtxAC = null;
        }
    }

    public struct StrctAlternative
    {
        public string name;
        public double cost1;
        public double cost2;
        //public List<Int32> scores;
        public double finalScore;

        public StrctAlternative(string s, double c1, double c2, double f)
        {
            name = s;
            cost1 = c1;
            cost2 = c2;
            finalScore = f;
        }
    }
}