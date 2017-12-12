//C# Lab 6 , Johan Lång     Göteborg 171115

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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Width = 1050;
            Height = 450;

            //Varaibles
            TextBoxNrGuest.Text = "1";
            TextBoxOpenBarInSek.Text = "120";
            TextBoxTimeGuest.Text = "";

            TextBoxAntalGlas.Text = "8";
            TextBoxAntalStolar.Text = "9";

            TextBoxPlockaGlasTid.Text = "10";
            TextBoxDiskaGlasTid.Text = "15";
        }

        private bool openBarStatus = false;

        public int nrGusetIntoBar = 1;
        public int patronCounter = 0;  //Antalet gäster i baren

        BlockingCollection<Patron> queBar = new BlockingCollection<Patron>();
        BlockingCollection<Patron> queChair = new BlockingCollection<Patron>();
        BlockingCollection<Patron> queBarLeave = new BlockingCollection<Patron>();

        private void ButtonOpenCloseBar_Click(object sender, RoutedEventArgs e)
        {
            if (openBarStatus == false)
            {
                int sekBarOpen = int.Parse(TextBoxOpenBarInSek.Text);

                Bouncer bouncher = new Bouncer();
                PubTimer pubtimer = new PubTimer(sekBarOpen);
                Bartender bartender = new Bartender();
                BarQue barque = new BarQue();

                bouncher.NrOfGuest = int.Parse(TextBoxNrGuest.Text);

                nrGusetIntoBar = bouncher.NrOfGuest;

                int antalGlas = int.Parse(TextBoxAntalGlas.Text);
                int antalStolar = int.Parse(TextBoxAntalStolar.Text);

                int plockaGlasSek = int.Parse(TextBoxPlockaGlasTid.Text);
                int diskaGlasSek = int.Parse(TextBoxDiskaGlasTid.Text);

                string sekNrGuset = TextBoxTimeGuest.Text;

                openBarStatus = true;

                Dispatcher.Invoke(() =>
                {
                    TextBoxOpenBarInSek.IsEnabled = false;
                    TextBoxTimeGuest.IsEnabled = false;
                    TextBoxNrGuest.IsEnabled = false;
                    TextBoxAntalGlas.IsEnabled = false;
                    TextBoxAntalStolar.IsEnabled = false;
                    TextBoxPlockaGlasTid.IsEnabled = false;
                    TextBoxDiskaGlasTid.IsEnabled = false;
                });

                pubtimer.PubClosing += ClosePub;
                pubtimer.PubClosing += bouncher.PubIsClosing;

                pubtimer.Tick += updateTimeLeftTextBox;

                Task.Run(() =>
               {

                   pubtimer.Start();

               });

                ButtonOpenCloseBar.Content = "Stäng baren...";
                int counterBartender = 1;
                int counterWaiter = 1;

                Task.Run(() =>
                        {

                            string bartenderTextWait = $"{counterBartender}_Bartender väntar på besökare";
                            counterBartender++;

                            string antalGlasText = $"Det finns {antalGlas} antal glas i hyllan";
                            SafeSetTextToLabel(LabelAntalGlas, antalGlasText);
                            SafeInsertTextToListBox(ListBoxBartender, bartenderTextWait);

                            string antalStolarText = $"Det finns {antalStolar} antal stolar i baren";
                            SafeSetTextToLabel(LabelAntalStolar, antalStolarText);

                            string waiterTextWait = $"{counterWaiter}_Servitrisen väntar på besökare";
                            SafeInsertTextToListBox(ListBoxWaitress, waiterTextWait);
                        });

                //Baren öppnar och bouncher släpper in gäster..
                //Bartender väntar på gäster, serverar öl när ett glas finns tillgängligt, gästen var för sig letar efter ledig stol, dricker öl och lämnar sedan baren. Waiterss plockar glas från bord, diskar det och stääler tillbaka det i baren.

                Task.Run(() =>
                 {
                     while (openBarStatus)
                     {
                         int sekIntoBar = 1;

                         Random rndSek = new Random();
                         if (sekNrGuset == "")
                         {
                             sekIntoBar = rndSek.Next(3, 11);
                         }
                         else
                         {
                             sekIntoBar = int.Parse(sekNrGuset);
                         }

                         Thread.Sleep(sekIntoBar * 1000);  //Släpper in ny gäst efter 3 - 10sek

                         for (int i = 0; i < nrGusetIntoBar; i++)
                         {
                             ++patronCounter;

                             Task.Run(() =>  //En tråd per Patron
                             {
                                 Patron patron = bouncher.getRandomPatron();
                                 queBar.Add(patron);

                                 string strListBox = $"{patronCounter}_{patron.Name} kommer in och går till baren";

                                 SafeInsertTextToListBox(ListBoxGuest, strListBox);

                                 string strLabel = "";
                                 if (patronCounter == 1)
                                 {
                                     strLabel = $"Det finns {patronCounter} gäst i baren";
                                 }
                                 else
                                 {
                                     strLabel = $"Det finns {patronCounter} gäster i baren";
                                 }

                                 SafeSetTextToLabel(LabelNrGuset, strLabel);

                                 Thread.Sleep(1000); //Går till baren

                                 if (queBar.Count > 0 && antalGlas > 0)
                                 {
                                     Patron firstPatron = queBar.Take(1).First();
                                     queBar.Take();

                                     string patronText = $"{counterBartender}_{firstPatron.Name}: En stor stark öl tack!";
                                     counterBartender++;

                                     SafeInsertTextToListBox(ListBoxBartender, patronText);
                                     Thread.Sleep(1000);

                                     string bartenderTextPick = $"{counterBartender}_Bartender hämtar glas";
                                     counterBartender++;
                                     SafeInsertTextToListBox(ListBoxBartender, bartenderTextPick);
                                     Thread.Sleep(3000);
                                     antalGlas--;
                                     SafeSetTextToTextBox(TextBoxAntalGlas, antalGlas.ToString());
                                     string antalGlasText = $"Det finns {antalGlas} antal glas i hyllan";
                                     SafeSetTextToLabel(LabelAntalGlas, antalGlasText);

                                     string bartenderTextBeer = $"{counterBartender}_Bartender häller upp öl till {firstPatron.Name}";
                                     counterBartender++;
                                     SafeInsertTextToListBox(ListBoxBartender, bartenderTextBeer);
                                     Thread.Sleep(3000);

                                     queChair.Add(firstPatron);

                                     counterWaiter++;
                                     if (antalStolar == 0)
                                     {
                                       string patronTextNoChair = $"{counterWaiter}_Inga lediga stolar";
                                         SafeInsertTextToListBox(ListBoxWaitress, patronTextNoChair);
                                         SafeSetTextToTextBox(TextBoxAntalStolar, antalStolar.ToString());
                                     }

                                     else if (antalStolar > 0)
                                     {

                                         Patron beerPatron = queChair.Take(1).First();
                                         queChair.Take();

                                         string patronTextFindChair = $"{counterWaiter}_{beerPatron.Name}: Letar efter ledig stol";
                                         --antalStolar;
                                         counterWaiter++; 

                                         SafeInsertTextToListBox(ListBoxWaitress, patronTextFindChair);
                                         SafeSetTextToTextBox(TextBoxAntalStolar, antalStolar.ToString());
                                         string antalStolarText = $"Det finns {antalStolar} antal stolar i baren";
                                         SafeSetTextToLabel(LabelAntalStolar, antalStolarText);
                                         Thread.Sleep(4000);

                                         string patronTextDrinkBeer = $"{counterWaiter}_{beerPatron.Name}: Dricker öl!";
                                         counterWaiter++;

                                         SafeInsertTextToListBox(ListBoxWaitress, patronTextDrinkBeer);
                                         Random rndDrinkBeer = new Random();
                                         int drinkBeerSek = rndDrinkBeer.Next(10, 21);
                                         Thread.Sleep(drinkBeerSek * 1000);

                                         queBarLeave.Add(beerPatron);

                                         string patronTextLeave = $"{counterWaiter}_{beerPatron.Name}: Lämnar baren!";
                                         counterWaiter++;
                                         patronCounter--;

                                         ++antalStolar;
                                         SafeSetTextToTextBox(TextBoxAntalStolar, antalStolar.ToString());
                                         antalStolarText = $"Det finns {antalStolar} antal stolar i baren";
                                         SafeSetTextToLabel(LabelAntalStolar, antalStolarText);


                                         SafeInsertTextToListBox(ListBoxWaitress, patronTextLeave);

                                         strLabel = "";
                                         if (patronCounter == 1)
                                         {
                                             strLabel = $"Det finns {patronCounter} gäst i baren";
                                         }
                                         else
                                         {
                                             strLabel = $"Det finns {patronCounter} gäster i baren";
                                         }

                                         SafeSetTextToLabel(LabelNrGuset, strLabel);

                                         string waiterTextPickGlas = $"{counterWaiter}_Plockar glas från bord";
                                         counterWaiter++;
                                         SafeInsertTextToListBox(ListBoxWaitress, waiterTextPickGlas);
                                         Thread.Sleep(plockaGlasSek * 1000);

                                         string waiterTextDishGlas = $"{counterWaiter}_Diskar glas och ställer i baren";
                                         counterWaiter++;
                                         SafeInsertTextToListBox(ListBoxWaitress, waiterTextDishGlas);
                                         Thread.Sleep(diskaGlasSek * 1000);
                                         antalGlas++;
                                         SafeSetTextToTextBox(TextBoxAntalGlas, antalGlas.ToString());
                                         antalGlasText = $"Det finns {antalGlas} antal glas i hyllan";
                                         SafeSetTextToLabel(LabelAntalGlas, antalGlasText);
                                     }

                                     else if (antalGlas == 0)
                                     {

                                         string bartenderTextWaitForGlas = $"{counterBartender}_Bartender väntar på rent glas";
                                         counterBartender++;
                                         SafeInsertTextToListBox(ListBoxBartender, bartenderTextWaitForGlas);
                                     }
                                 }

                             });
                         }
                     }
                 });
            }

            else
            {
                ClosePub();
            }
        }

        void ClosePub()
        {
            openBarStatus = false;


            Dispatcher.Invoke(() =>
            {
                ButtonOpenCloseBar.IsEnabled = false;
                TextBoxNrGuest.Text = "1";
                TextBoxTimeGuest.Text = "";
                TextBoxAntalGlas.Text = "8";
                TextBoxAntalStolar.Text = "9";
                TextBoxPlockaGlasTid.Text = "10";
                TextBoxDiskaGlasTid.Text = "15";

                TextBoxOpenBarInSek.Text = "";
                ListBoxGuest.Items.Clear();
                ListBoxBartender.Items.Clear();
                ListBoxWaitress.Items.Clear();

                ButtonOpenCloseBar.Content = "Baren är stängd!";

            });
        }


        private void SafeInsertTextToListBox(ListBox lb, string str)
        {
            Dispatcher.Invoke(() =>
            {
                lb.Items.Insert(0, str);

            });
        }


        private void SafeSetTextToTextBox(TextBox tb, string str)
        {
            Dispatcher.Invoke(() =>
            {
                tb.Text = str;

            });
        }

        private void SafeSetTextToLabel(Label lb, string str)
        {
            Dispatcher.Invoke(() =>
            {
                lb.Content = str;

            });
        }


        private void updateTimeLeftTextBox(int sekBarOpenLeft)
        {
            SafeSetTextToTextBox(TextBoxOpenBarInSek, sekBarOpenLeft.ToString());
        }
    }
}





