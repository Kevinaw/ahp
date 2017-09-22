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

using System.Data;
using Microsoft.Reporting.WinForms;

namespace ahp.Report
{
    /// <summary>
    /// Interaction logic for ReportViewer.xaml
    /// </summary>
    public partial class ReportViewer : UserControl
    {
        public ReportViewer()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // 1. Hierarchy of the task
            List<entityHierarcy> lstHierarchy = new List<entityHierarcy>();

            string strGoal = ((MainWindow)Application.Current.MainWindow).txbGoal.Text;
            lstHierarchy.Add(new entityHierarcy() { groupName = "Goal", itemName = strGoal });
            foreach (var cr in ((MainWindow)Application.Current.MainWindow).Criterias.listCr)
                lstHierarchy.Add(new entityHierarcy() { groupName = "Criteria", itemName = cr.name });
            foreach (var al in ((MainWindow)Application.Current.MainWindow).Alternatives.listAl)
                lstHierarchy.Add(new entityHierarcy() { groupName = "Alternatives", itemName = al.name });

            ReportDataSource reportDataSource1 = new ReportDataSource();
            reportDataSource1.Name = "DataSetHierarchy"; // Name of the DataSet we set in .rdlc
            reportDataSource1.Value = lstHierarchy;

            // 2. criteria pairwise comparison
            List<entityCriteriaComparison> lstComp = new List<entityCriteriaComparison>();
            List<StrctCriteria> lstC = ((MainWindow)Application.Current.MainWindow).Criterias.listCr;
            double[,] mtx = ((MainWindow)Application.Current.MainWindow).Criterias.mtx;
            for (int i = 0; i < lstC.Count; i++)
                for(int j = 0; j < lstC.Count; j++)
                    lstComp.Add(new entityCriteriaComparison() { criteria1 = lstC.ElementAt(i).name, criteria2 = lstC.ElementAt(j).name, value = Math.Round(mtx[i, j], 2) });

            ReportDataSource reportDataSource2 = new ReportDataSource();
            reportDataSource2.Name = "DatasetCriteriaPairwiseComparison"; // Name of the DataSet we set in .rdlc
            reportDataSource2.Value = lstComp;

            // 3. Alternative scores
            List<entityAlternativeScores> lstAlt = new List<entityAlternativeScores>();
            List<StrctAlternative> lstA = ((MainWindow)Application.Current.MainWindow).Alternatives.listAl;
            int[,] mtxAC = ((MainWindow)Application.Current.MainWindow).Alternatives.mtxAC;
            for (int i = 0; i < lstC.Count; i++)
                for (int j = 0; j < lstA.Count; j++)
                    lstAlt.Add(new entityAlternativeScores() { criteria = lstC.ElementAt(i).name, alternative = lstA.ElementAt(j).name, value = mtxAC[i, j] });

            ReportDataSource reportDataSource3 = new ReportDataSource();
            reportDataSource3.Name = "DataSetAlternativeScore"; // Name of the DataSet we set in .rdlc
            reportDataSource3.Value = lstAlt;

            // 4. Results.
            // criteria weights
            List<entityCriteria> listCriteria = new List<entityCriteria>();
            for (int i = 0; i < lstC.Count; i++)
                listCriteria.Add(new entityCriteria() { name = lstC.ElementAt(i).name, weight = Math.Round(lstC.ElementAt(i).weight, 2) });
            ReportDataSource reportDataSource4 = new ReportDataSource();
            reportDataSource4.Name = "DataSetCriteria"; // Name of the DataSet we set in .rdlc
            reportDataSource4.Value = listCriteria;

            // final function and value
            List<entityAlternative> listAls = new List<entityAlternative>();
            for(int i = 0; i < lstA.Count; i++)
            {
                double total = 0.0;
                for (int j = 0; j < lstC.Count; j++)
                    total = total + lstC.ElementAt(j).weight * mtxAC[j, i];
                listAls.Add(new entityAlternative() { Name = lstA.ElementAt(i).name, TotalFunction = Math.Round(total, 2), Cost = Math.Round(lstA.ElementAt(i).cost1, 2), CostRisk = Math.Round(lstA.ElementAt(i).cost2, 2), Value =Math.Round(total / (lstA.ElementAt(i).cost1 + lstA.ElementAt(i).cost2), 2) });
            }
            ReportDataSource reportDataSource5 = new ReportDataSource();
            reportDataSource5.Name = "DatasetAlternatives"; // Name of the DataSet we set in .rdlc
            reportDataSource5.Value = listAls;


            // 5. Selection
            double mxValue = 0.0;
            string strS = "";
            foreach (var a in listAls)
                if (mxValue < a.Value)
                {
                    mxValue = a.Value;
                    strS = a.Name;
                }
            ReportParameter p = new ReportParameter();
            p.Name = "selection";
            p.Values.Add(strS);

            //
            string exeFolder = System.AppDomain.CurrentDomain.BaseDirectory;
            string reportPath = exeFolder + @"\Report\Report.rdlc";
            reportViewer.LocalReport.ReportPath = reportPath; // Path of the rdlc file
            reportViewer.LocalReport.DataSources.Add(reportDataSource1);
            reportViewer.LocalReport.DataSources.Add(reportDataSource2);
            reportViewer.LocalReport.DataSources.Add(reportDataSource3);
            reportViewer.LocalReport.DataSources.Add(reportDataSource4);
            reportViewer.LocalReport.DataSources.Add(reportDataSource5);
            reportViewer.LocalReport.SetParameters(p);
            reportViewer.SetDisplayMode(DisplayMode.PrintLayout);
            reportViewer.RefreshReport();
        }

        private void reportViewer_RenderingComplete(object sender, Microsoft.Reporting.WinForms.RenderingCompleteEventArgs e)
        {

        }

    }
}
