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

namespace MyNumericUpDownControll
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class NumericUpDown : UserControl
    {
        private int idx;
        public int Idx
        {
            get
            {
                return idx;
            }
            set
            {
                idx = value;
                dblvalue = dblValues[idx];
                dblValueR = dblValuesR[idx];
                strValue = strValues[idx];
                strValueR = strValuesR[idx];
                TxbValue.Text = strValue;
            }
        }

        // position in matrix
        public int RowID;
        public int ColId;
        
        /// <summary> 
        /// Gets or sets the double value
        /// </summary> 
        private double dblvalue;
        public double dblValue
        {
            get { return dblvalue; }
            set {
                int i = 0;
                double diff = 0.001;
                for(i = 0; i < dblValues.Length; i++)
                {
                    if (Math.Abs(value - dblValues[i]) < diff)
                        break;
                    else
                        continue;
                }
                Idx = i;
            }
        }

        public double dblValueR;
        /// <summary> 
        /// Gets or sets the value assigned to the control. 
        /// </summary> 
        public string strValue;
        public string strValueR;

        private static string[] strValues = { "1/9", "1/8", "1/7", "1/6", "1/5", "1/4", "1/3", "1/2", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        private static string[] strValuesR = { "9", "8", "7", "6", "5", "4", "3", "2", "1", "1/2", "1/3", "1/4", "1/5", "1/6", "1/7", "1/8", "1/9" };
        private static double[] dblValues = { 0.1111111, 0.125, 0.1428571, 0.1666666, 0.2, 0.25, 0.3333333, 0.5, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        private static double[] dblValuesR = { 9, 8, 7, 6, 5, 4, 3, 2, 1, 0.5, 0.3333333, 0.25, 0.2, 0.1666666, 0.1428571, 0.125, 0.1111111 };

        public NumericUpDown()
        {
            InitializeComponent();
            Idx = 8;

        }

        public event EventHandler ValueChangedEvent;
        /// <summary> 
        /// Raises the ValueChanged event. 
        /// </summary> 
        /// <param name="args">Arguments associated with the ValueChanged event.</param>
        protected virtual void OnValueChangedEvent(EventArgs e)
        {
            if (ValueChangedEvent != null)
                ValueChangedEvent(this, e);
        }

        private void BtnUp_Click(object sender, EventArgs e)
        {
            if(Idx < 16)
            {
                Idx++;
                OnValueChangedEvent(EventArgs.Empty);
            }   
        }

        private void BtnDown_Click(object sender, EventArgs e)
        { 
            if(Idx > 0)
            {
                Idx--;
                OnValueChangedEvent(EventArgs.Empty);
            }
        }
    }
}
