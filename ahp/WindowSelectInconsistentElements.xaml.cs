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

namespace ahp
{
    /// <summary>
    /// Interaction logic for WindowSelectInconsistentElements.xaml
    /// </summary>
    public partial class WindowSelectInconsistentElements : Window
    {
        double[] options;
        public List<int> selectedIndexes;
        public WindowSelectInconsistentElements()
        {
            InitializeComponent();
        }

        public WindowSelectInconsistentElements(double[] options)
        {
            InitializeComponent();
            selectedIndexes = new List<int>();

            int i;

            this.options = new double[options.Length];
            options.CopyTo(this.options, 0);

            for(i = 0; i < options.Length; i++)
            {
                RowDefinition rd = new RowDefinition();
                rd.Height = new GridLength(0, GridUnitType.Auto);
                GrdOptions.RowDefinitions.Add(rd);
            }

            RowDefinition rd0 = new RowDefinition();
            rd0.Height = new GridLength(0, GridUnitType.Star);
            GrdOptions.RowDefinitions.Add(rd0);


            for (i = 0; i < options.Length; i++)
            {
                CheckBox cb = new CheckBox();
                if(i == 0)
                    cb.Margin = new Thickness(20, 20, 20, 20);
                else
                    cb.Margin = new Thickness(20, 0, 20, 20);
                cb.Content = options[i].ToString();

                GrdOptions.Children.Add(cb);
                Grid.SetRow(cb, i);
            }

            
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            for(int i = 0; i < options.Length; i++)
            {
                CheckBox cb = GrdOptions.Children[i] as CheckBox;
                if(cb.IsChecked == true)
                {
                    selectedIndexes.Add(i);
                }
            }
            this.Close();
        }
    }
}
