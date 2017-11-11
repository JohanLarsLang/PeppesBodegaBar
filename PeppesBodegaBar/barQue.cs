using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeppesBodegaBar
{
    public class BarQue
    {
        public BlockingCollection<Patron> patronBarQue { get; set; }

        public Patron FirstPatronInBarQue()
        {
            //if (patronBarQue  != null)
            return patronBarQue.Last();
            //set { patronBarQue = value; }
        }
    }
}
