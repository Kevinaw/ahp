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

using System.Threading;
using System.ComponentModel;

namespace ahp
{
    /// <summary>
    /// Interaction logic for WindowProgressBar.xaml
    /// </summary>
    public partial class WindowProgressBar : Window
    {
        Int64 comN;
        Int64 iterN;
        List<StrctFreedomLocation> locationList;
        Criterias Criterias;
        List<Criterias> resultList;

        public WindowProgressBar()
        {
            InitializeComponent();
        }

        public WindowProgressBar(List<StrctFreedomLocation> locationList, Criterias Criterias, ref List<Criterias> resultList, Int64 comN)
        {
            InitializeComponent();

            this.locationList = locationList;
            this.Criterias = Criterias;
            this.resultList = resultList;
            this.comN = comN;
            iterN = 0;            
        }

        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            while(iterN < comN)
            {
                (sender as BackgroundWorker).ReportProgress(Convert.ToInt16(Convert.ToDouble(iterN) * 100 / Convert.ToDouble(comN)));
                Thread.Sleep(100);
            }
        }

        void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            PrgBar.Value = e.ProgressPercentage;
#if DEBUG
            Console.WriteLine(iterN.ToString() + " / " + comN.ToString() + "=" + PrgBar.Value.ToString());
#endif
            TxbValue.Text = iterN.ToString() + "/" + comN.ToString();
        }

        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Close();
        }

        void worker1_DoWork(object sender, DoWorkEventArgs e)
        {
            PopulateMtx(locationList, Criterias, ref resultList);
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
                        iterN++;
                        c.AhpEval();
                        if (c.CR < 0.1)
                            resultList.Add(c);

#if DEBUG
             /*           
                        Console.WriteLine("CR = " + c.CR.ToString());
                        for (int i = 0; i < c.listCr.Count; i++)
                        {
                            for (int j = 0; j < c.listCr.Count; j++)
                                Console.Write(c.mtx[i, j].ToString() + "    ");
                            Console.Write("\n");
                        }
                        */
#endif

                    }


                    PopulateMtx(ll, c, ref resultList);

                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += worker_DoWork;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            worker.RunWorkerAsync();

            BackgroundWorker worker1 = new BackgroundWorker();
            worker1.DoWork += worker1_DoWork;
            worker1.RunWorkerAsync();
        }
    }
}
