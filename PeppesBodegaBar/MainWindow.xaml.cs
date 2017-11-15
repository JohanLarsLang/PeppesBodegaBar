//Lab 6, Johan Lång     Göteborg 171115

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
        private bool pauseTaskGuest = true;
        private bool pauseTaskBartender = true;
        private bool pauseTaskWaiter = true;
        private bool pauseAllTask = true;
        private bool bouncherWork = true;


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

                //StartSetting startsetting = new StartSetting();

                //startsetting.AntalGlasStart = TextBoxAntalGlas.Text;

                //string antalStolarStart = TextBoxAntalStolar.Text;

                string sekNrGuset = TextBoxTimeGuest.Text;

                openBarStatus = true;
                pauseTaskGuest = true;
                pauseTaskBartender = true;
                pauseTaskWaiter = true;

                Dispatcher.Invoke(() =>
                {
                    ButtonPauseGuest.IsEnabled = true;
                    ButtonPauseBartender.IsEnabled = true;
                    ButtonPauseWaiter.IsEnabled = true;
                    ButtoStopTasks.IsEnabled = true;
                    TextBoxOpenBarInSek.IsEnabled = false;
                    TextBoxTimeGuest.IsEnabled = false;
                    TextBoxNrGuest.IsEnabled = false;
                    TextBoxAntalGlas.IsEnabled = false;
                    TextBoxAntalStolar.IsEnabled = false;
                    TextBoxPlockaGlasTid.IsEnabled = false;
                    TextBoxDiskaGlasTid.IsEnabled = false;
                });

                //bartender.GiveBeer = queBar[0].ResiveBeer;

                pubtimer.PubClosing += ClosePub;
                pubtimer.PubClosing += bouncher.PubIsClosing;

                pubtimer.Tick += updateTimeLeftTextBox;

                Task.Run(() =>
               {

                   pubtimer.Start();

               });

                ButtonOpenCloseBar.Content = "Stäng baren...";


                //Öppnar baren och släpper in gäster..
                Task.Run(() =>
                 {
                     while (openBarStatus)
                     {
                         while (pauseTaskGuest)
                         {
                             if (bouncherWork != false)
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

                                 //nrGusetIntoBar = bouncher.GetNrOfGuest();

                                 for (int i = 0; i < nrGusetIntoBar; i++)
                                 {
                                     ++patronCounter;
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
                                 }
                             }
                         }
                     }
                 });

                Task.Run(() =>
                {
                    int counterBartender = 1;
                    string bartenderTextWait = $"{counterBartender}_Bartender väntar på besökare";
                    counterBartender++;

                    string antalGlasText = $"Det finns {antalGlas} antal glas i hyllan";
                    SafeSetTextToLabel(LabelAntalGlas, antalGlasText);
                    SafeInsertTextToListBox(ListBoxBartender, bartenderTextWait);

                    while (pauseTaskBartender)
                    {
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
                            antalGlasText = $"Det finns {antalGlas} antal glas i hyllan";
                            SafeSetTextToLabel(LabelAntalGlas, antalGlasText);

                            string bartenderTextBeer = $"{counterBartender}_Bartender häller upp öl till {firstPatron.Name}";
                            counterBartender++;
                            SafeInsertTextToListBox(ListBoxBartender, bartenderTextBeer);
                            Thread.Sleep(3000);

                            queChair.Add(firstPatron);

                            if (antalGlas == 0)
                            {

                                string bartenderTextWaitForGlas = $"{counterBartender}_Bartender väntar på rent glas";
                                counterBartender++;
                                SafeInsertTextToListBox(ListBoxBartender, bartenderTextWaitForGlas);
                            }
                        }
                    }
                });

                //Gäst letar stol, dricker öl och lämnar baren
                Task.Run(() =>
                {
                    string antalStolarText = $"Det finns {antalStolar} antal stolar i baren";
                    SafeSetTextToLabel(LabelAntalStolar, antalStolarText);

                    int counterBeer = 1;

                    while (pauseTaskWaiter)
                    {
                        if (queChair.Count > 0 )
                        {
                            if (antalStolar == 0)
                            {
                                counterBeer++;
                                string patronTextNoChair = $"{counterBeer}_Inga lediga stolar";
                                SafeInsertTextToListBox(ListBoxWaitress, patronTextNoChair);
                                SafeSetTextToTextBox(TextBoxAntalStolar, antalStolar.ToString());
                            }

                            else if (antalStolar > 0)
                            {
                            
                                Patron beerPatron = queChair.Take(1).First();
                                queChair.Take();

                                string patronTextFindChair = $"{counterBeer}_{beerPatron.Name}: Letar efter ledig stol";
                                --antalStolar;
                                counterBeer++;

                                SafeInsertTextToListBox(ListBoxWaitress, patronTextFindChair);
                                SafeSetTextToTextBox(TextBoxAntalStolar, antalStolar.ToString());
                                antalStolarText = $"Det finns {antalStolar} antal stolar i baren";
                                SafeSetTextToLabel(LabelAntalStolar, antalStolarText);
                                Thread.Sleep(4000);

                                string patronTextDrinkBeer = $"{counterBeer}_{beerPatron.Name}: Dricker öl!";
                                counterBeer++;

                                SafeInsertTextToListBox(ListBoxWaitress, patronTextDrinkBeer);
                                Random rndDrinkBeer = new Random();
                                int drinkBeerSek = rndDrinkBeer.Next(10, 21);
                                Thread.Sleep(drinkBeerSek * 1000);

                                queBarLeave.Add(beerPatron);
                            }
                        }
                    }
                });

                //Servitör väntar på gäster, plockar glas från bord, diskar och ställer dem i baren

                Task.Run(() =>
                {
                    int counterWaiter = 1;
                    string waiterTextWait = $"{counterWaiter}_Servitrisen väntar på besökare";
                    counterWaiter++;

                    SafeInsertTextToListBox(ListBoxWaitress, waiterTextWait);

                    while (pauseTaskWaiter)
                    {
                        if (queBarLeave.Count > 0)
                        {
                            Patron leavePatron = queBarLeave.Take(1).First();
                            queBarLeave.Take();

                            string patronTextLeave = $"{counterWaiter}_{leavePatron.Name}: Lämnar baren!";
                            counterWaiter++;
                            patronCounter--;

                            ++antalStolar;
                            SafeSetTextToTextBox(TextBoxAntalStolar, antalStolar.ToString());
                            string antalStolarText = $"Det finns {antalStolar} antal stolar i baren";
                            SafeSetTextToLabel(LabelAntalStolar, antalStolarText);


                            SafeInsertTextToListBox(ListBoxWaitress, patronTextLeave);

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
                            string antalGlasText = $"Det finns {antalGlas} antal glas i hyllan";
                            SafeSetTextToLabel(LabelAntalGlas, antalGlasText);
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
            /*
            MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Are you sure?", "Confirm close the pub!", System.Windows.MessageBoxButton.YesNo);

            if (messageBoxResult == MessageBoxResult.Yes)
            {
            */
            StartSetting startsetting = new StartSetting();
            bouncherWork = false;
            openBarStatus = false;

            //if (queBarLeave.Count == 0)
            //{
            Dispatcher.Invoke(() =>
            {
                ButtonPauseGuest.IsEnabled = false;
                ButtonPauseBartender.IsEnabled = false;
                ButtonPauseWaiter.IsEnabled = false;
                ButtonOpenCloseBar.IsEnabled = false;
                ButtoStopTasks.IsEnabled = false;
                TextBoxNrGuest.Text = "1";
                TextBoxTimeGuest.Text = "";
                TextBoxAntalGlas.Text = "8";
                TextBoxAntalStolar.Text = "9";
                TextBoxPlockaGlasTid.Text = "10";
                TextBoxDiskaGlasTid.Text = "15";

                //TextBoxAntalGlas.Text = startsetting.AntalGlasStart;
                //TextBoxAntalGlas.Text = 
                //TextBoxAntalStolar.Text =
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

        private void ButtonPauseGuest_Click(object sender, RoutedEventArgs e)
        {
            Bouncer bouncher = new Bouncer();

            if (pauseTaskGuest)
            {
                ButtonPauseGuest.Content = "Fortsätt";
                pauseTaskGuest = false;
                bouncher.NrOfGuest = int.Parse(TextBoxNrGuest.Text);
            }
            else
            {

                ButtonPauseGuest.Content = "Pausa";
                pauseTaskGuest = true;
                bouncher.NrOfGuest = int.Parse(TextBoxNrGuest.Text);

            }

        }

        private void updateTimeLeftTextBox(int sekBarOpenLeft)
        {
            SafeSetTextToTextBox(TextBoxOpenBarInSek, sekBarOpenLeft.ToString());
        }

        private void ButtonStopTasks_Click(object sender, RoutedEventArgs e)
        {
            if (pauseAllTask)
            {
                ButtoStopTasks.Content = "Fortsätt...";
                bouncherWork = false;
                openBarStatus = false;
                pauseTaskGuest = false;
                pauseTaskBartender = false;
                pauseTaskWaiter = false;

            }
            else
            {

                ButtoStopTasks.Content = "Stoppa alla trådar!";
                bouncherWork = true;
                openBarStatus = true;
                pauseTaskGuest = true;
                pauseTaskBartender = true;
                pauseTaskWaiter = true;
            }
            pauseAllTask = !pauseAllTask;
        }

        private void ButtonPauseBartender_Click(object sender, RoutedEventArgs e)
        {
            if (pauseTaskBartender)
            {
                ButtonPauseBartender.Content = "Fortsätt";
                pauseTaskBartender = false;
            }
            else
            {
                ButtonPauseBartender.Content = "Pausa";
                pauseTaskBartender = true;
            }
        }

        private void ButtonPauseWaiter_Click(object sender, RoutedEventArgs e)
        {
            if (pauseTaskWaiter)
            {
                ButtonPauseWaiter.Content = "Fortsätt";
                pauseTaskWaiter = false;
            }
            else
            {
                ButtonPauseWaiter.Content = "Pausa";
                pauseTaskWaiter = true;
            }

        }
    }
}

