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

using System.Collections.ObjectModel;
using System.Globalization;

namespace ahp
{
    /// <summary>
    /// Interaction logic for WindowAlternativesScores.xaml
    /// </summary>
    public partial class WindowAlternativesScores : Window
    {
        public List<int> listValues = new List<int>();
        public int average;
        ObservableCollection<ScoreItem> scoreData = new ObservableCollection<ScoreItem>();

        public WindowAlternativesScores(List<int> scores)
        {
            int i = 1;

            InitializeComponent();
            
            foreach(int s in scores)
            {
                scoreData.Add(new ScoreItem() { Id = i++, Value = s});
            }
            this.DG.DataContext = scoreData;                

            listValues = scores;
            var listValue1 = listValues.Where(s => s != 0).ToList();
            average = 0;
            if (listValue1.Count != 0)
                average = listValue1.Sum() / listValue1.Count();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;

            for(int i = 0; i < scoreData.Count; i++)
            {
                listValues[i] = scoreData.ElementAt(i).Value;
            }

            var listValue1 = listValues.Where(s => s != 0).ToList();
            average = 0;
            if (listValue1.Count != 0)
                average = listValue1.Sum() / listValue1.Count();

            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }


    public class ScoreItem
    {
        public int Id { get; set; }
        public int Value { get; set; }
    }

    internal class ScoreValidationRule : ValidationRule
    {
        // Score should be integer and its value should be between 0 and 100
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if(value != null)
            {
                int proposedValue;
                if(!int.TryParse(value.ToString(), out proposedValue))
                {
                    MessageBox.Show("'" + value.ToString() + "' is not a integer value.");
                    return new ValidationResult(false, "'" + value.ToString() +"' is not a integer value.");
                }
                if(proposedValue < 0 || proposedValue > 100)
                {
                    MessageBox.Show("Score should be between 0 and 100.");
                    return new ValidationResult(false, "Score should be between 0 and 100.");
                }
            }

            return new ValidationResult(true, null);
        }
    }
}
