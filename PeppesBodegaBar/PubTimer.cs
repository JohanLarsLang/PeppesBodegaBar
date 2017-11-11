using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PeppesBodegaBar
{
    public class PubTimer
    {
        private int howLongOpen;

        public PubTimer(int howLongOpen)
        {
            this.howLongOpen = howLongOpen;
        }

        public event Action<int> Tick;  // en sekund har gått
        public event Action PubClosing;  // puben stänger, inga nya gäster

        public void Start()
        {
            while (howLongOpen > 0)
            {
                // vänta en sekund
                Thread.Sleep(1000);
                howLongOpen--;
                Tick?.Invoke(howLongOpen);
            }
            PubClosing?.Invoke();
            //vänta tills tiden har gått
        }
    }
}
