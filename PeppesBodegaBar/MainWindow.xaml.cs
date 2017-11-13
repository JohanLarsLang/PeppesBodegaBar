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
            TextBoxOpenBarInSek.Text = "45";
            TextBoxTimeGuest.Text = "";

            TextBoxAntalGlas.Text = "2";
            TextBoxAntalStolar.Text = "3";
        }

        private bool openBarStatus = false;
        private bool pauseTaskGuest = true;
        private bool pauseTaskBartender = true;
        private bool pauseTaskWaiter = true;
        private bool pauseAllTask = true;

        public int nrGusetIntoBar = 1;
        public int patronCounter = 0;  //Antalet gäster i baren

        //BlockingCollection<Patron> queBar = new BlockingCollection<Patron>();

        List<Patron> queBar = new List<Patron>();
        List<Patron> queChair = new List<Patron>();
        List<Patron> queBarLeave = new List<Patron>();

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

                string antalGlasStart = TextBoxAntalGlas.Text;
                string antalStolarStart = TextBoxAntalStolar.Text;

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
                                 //barque.AddPatronInBarQue(patron);

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
                 });

                Task.Run(() =>
                {
                    int counterBartender = 1;
                    string bartenderTextWait = $"{counterBartender}_Bartender väntar på besökare";
                    counterBartender++;

                    string antalGlasText = $"Det finns {antalGlas} antal glas i hyllan";
                    SafeSetTextToLabel(LabelAntalGlas, antalGlasText);
                    SafeInsertTextToListBox(ListBoxBartender, bartenderTextWait);

                    string antalStolarText = $"Det finns {antalStolar} antal stolar i baren";
                    SafeSetTextToLabel(LabelAntalStolar, antalStolarText);


                    while (pauseTaskBartender)
                    {
                        if (queBar.Count > 0 && antalGlas > 0)
                        {
                            Patron firstPatron = queBar[0];
                            queBar.RemoveAt(0);

                            string patronText = $"{counterBartender}_{firstPatron.Name}: En stor stark öl tack!";
                            counterBartender++;

                            SafeInsertTextToListBox(ListBoxBartender, patronText);
                            Thread.Sleep(2000);

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
                    //}

                });

                //Gäst letar stol, dricker öl och lämnar baren
                Task.Run(() =>
                {
                    int counterBeer = 1;
                    Thread.Sleep(8500);

                    while (pauseTaskGuest)
                    {
                        if (queChair.Count > 0 && antalStolar > 0)
                        {
                            Patron beerPatron = queChair[0];
                            queChair.RemoveAt(0);

                            string patronTextFindChair = $"{counterBeer}_{beerPatron.Name}: Letar efter ledig stol";
                            antalStolar--;
                            counterBeer++;

                            SafeInsertTextToListBox(ListBoxGuest, patronTextFindChair);
                            SafeSetTextToTextBox(TextBoxAntalStolar, antalStolar.ToString());
                            string antalStolarText = $"Det finns {antalStolar} antal stolar i baren";
                            SafeSetTextToLabel(LabelAntalStolar, antalStolarText);
                            Thread.Sleep(4000);

                            string patronTextDrinkBeer = $"{counterBeer}_{beerPatron.Name}: Dricker öl!";
                            counterBeer++;

                            SafeInsertTextToListBox(ListBoxGuest, patronTextDrinkBeer);
                            Random rndDrinkBeer = new Random();
                            int drinkBeerSek = rndDrinkBeer.Next(10, 21);
                            Thread.Sleep(drinkBeerSek * 1000);

                            string patronTextLeave = $"{counterBeer}_{beerPatron.Name}: Lämnar baren!";
                            counterBeer++;
                            patronCounter--;
                            queBarLeave.Add(beerPatron);


                            SafeInsertTextToListBox(ListBoxGuest, patronTextLeave);

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

                            if (antalStolar == 0)
                            {

                                string patronTextNoChair = $"{counterBeer}_Inga lediga stolar";
                                counterBeer++;
                                SafeInsertTextToListBox(ListBoxGuest, patronTextNoChair);
                                SafeSetTextToTextBox(TextBoxAntalStolar, antalStolar.ToString());
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

                    Thread.Sleep(16500);

                    while (pauseTaskWaiter)
                    {
                        if (queBarLeave.Count > 0)
                        {
                            Patron leavePatron = queBarLeave[0];
                            queBarLeave.RemoveAt(0);
                            string waiterTextPickGlas = $"{counterWaiter}_Plockar glas från bord";
                            counterWaiter++;
                            SafeInsertTextToListBox(ListBoxWaitress, waiterTextPickGlas);
                            Thread.Sleep(3000);

                            string waiterTextDishGlas = $"{counterWaiter}_Diskar glas och ställer i baren";
                            counterWaiter++;
                            SafeInsertTextToListBox(ListBoxWaitress, waiterTextDishGlas);
                            Thread.Sleep(3000);
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
            MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Are you sure?", "Confirm close the pub!", System.Windows.MessageBoxButton.YesNo);

            if (messageBoxResult == MessageBoxResult.Yes)
            {
                openBarStatus = false;
                pauseTaskGuest = false;
                pauseTaskBartender = false;
                pauseTaskWaiter = false;
                Thread.Sleep(10000);
                Dispatcher.Invoke(() =>
                {
                    ButtonPauseGuest.IsEnabled = false;
                    ButtonPauseBartender.IsEnabled = false;
                    ButtonPauseWaiter.IsEnabled = false;
                    ButtonOpenCloseBar.IsEnabled = false;
                    ButtoStopTasks.IsEnabled = false;
                    TextBoxNrGuest.Text = "1";
                    TextBoxTimeGuest.Text = "";
                    //TextBoxAntalGlas.Text = 
                    //TextBoxAntalStolar.Text =
                    TextBoxOpenBarInSek.Text = "";
                    ListBoxGuest.Items.Clear();
                    ListBoxBartender.Items.Clear();
                    ListBoxWaitress.Items.Clear();

                    ButtonOpenCloseBar.Content = "Baren stängd!";

                });
            }



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
            // Bartender bartender = new Bartender();
            //bartender.Work(sekBarOpenLeft);
        }

        private void ButtonStopTasks_Click(object sender, RoutedEventArgs e)
        {
            if (pauseAllTask)
            {
                ButtoStopTasks.Content = "Fortsätt...";
                openBarStatus = false;
                pauseTaskGuest = false;
                pauseTaskBartender = false;
                pauseTaskWaiter = false;
                pauseAllTask = false;
            }
            else
            {

                ButtoStopTasks.Content = "Stoppa alla trådar!";
                openBarStatus = false;
                pauseTaskGuest = true;
                pauseTaskBartender = true;
                pauseTaskWaiter = true;
                pauseAllTask = true;

            }

            openBarStatus = false;
            pauseTaskGuest = false;
            pauseTaskBartender = false;
            pauseTaskWaiter = false;


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

