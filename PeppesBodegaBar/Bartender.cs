using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PeppesBodegaBar
{
    public class Bartender
    {
        public Action GiveBeer = null;

        public Bartender()
        {
            bool alaive = true;

            Task.Run(() =>
            {
                while (alaive)
                {
                    if (GiveBeer != null)
                    {
                        GiveBeer.Invoke();
                        alaive = false;
                    }

                    Thread.Sleep(3000); //Häller upp öl, 3 sek
                }

            });

        }
        public void Work(int sekLeftBarOpen)
        {
            while (sekLeftBarOpen > 0)
            {
                DoWaitInBar();
                DoPickGlas();
                Thread.Sleep(3000);
                DoPourBeer();
                Thread.Sleep(3000);
            }
        }

        public string DoWaitInBar()
        {
            //WaitInBar();
            return $"Bartendern väntar på besökare";
        }

        public string DoPickGlas()
        {
            return $"Bartendern plockar glas från hyllan";
        }

        public string DoPourBeer()
        {
            return $"Häller upp öl till ";
        }


    }
}
