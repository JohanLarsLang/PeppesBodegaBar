using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PeppesBodegaBar
{
    public class Patron
    {
        public string Name { get; set; }

        public bool gotBeer = false;

        public void ResiveBeer()
        {
            gotBeer = true;
        }

        public Patron(string name)
        {
            
            Name = name;

            /*
            bool alaive = true;

            Task.Run(() =>
            {
                while (alaive)
                {
                    if (gotBeer)
                    {
                        alaive = false;
                    }
                    Thread.Sleep(50);
                }
                
            });
            */

    

        }
    }
}
