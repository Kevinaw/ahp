using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ahp.Report
{
    class entity
    {
        public string goal { get; set; }

        public List<StrctCriteria> listCr { get; set; }
        double[,] mtx { get; set; }

        public List<StrctAlternative> listAl { get; set; }
        int[,] mtxAC { get; set; }
    }
}
