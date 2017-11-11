using System;
using System.Collections.Generic;
using System.Collections;
using System.Collections.Concurrent;
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
    public class Bouncer
    {
        public BlockingCollection<Patron> patronBarQue { get; set; }

        List<string> patronNameList = new List<string> {
            "Anders", "Johan L", "Tommy", "Jari",
            "Pontus", "David", "Johanna", "Erik",
            "Linus", "Molyn", "Joakim", "Niklas",
            "Camilla", "Julia", "Maria", "Marcus",
            "Otto", "Simone", "Johan S", "Mitchell"};

        Random rnd = new Random();

        public BlockingCollection<Patron> getRandomPatronName(int nrPatron)
        {
            BlockingCollection<Patron> patronsList = new BlockingCollection<Patron>();
            //List<Patron> patronsList = new List<Patron>();

            for (int i = 0; i < nrPatron; i++)
            { 

                Task.Run(() =>
                {
                    nrPatron = rnd.Next(0, 19); //Slumpat namn av gäster
                    Patron patron = new Patron(patronNameList[nrPatron]);
                    patronsList.Add(patron);
                });
                                  
            }

           return patronsList;
        }

        public void PubIsClosing()
        {
            MessageBox.Show("Baren stänger nu!");
        }

    }
}
