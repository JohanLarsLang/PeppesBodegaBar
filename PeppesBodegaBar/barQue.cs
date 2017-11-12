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
        public BlockingCollection<Patron> PatronBarQue { get; set; }

        public void AddPatronInBarQue(Patron patron)
        {
            PatronBarQue.Add(patron);
        }

        public BlockingCollection<Patron> GetpatronBarQue
        {
            get { return PatronBarQue; }
            set { PatronBarQue = value; }
        }
    }
}
