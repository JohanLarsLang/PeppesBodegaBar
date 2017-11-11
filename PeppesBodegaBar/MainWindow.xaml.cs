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
            Height = 400;

            //Varaibles
            TextBoxNrGuest.Text = "1";
            TextBoxOpenBarInSek.Text = "120";
            TextBoxTimeGuest.Text = "";
        }

        private bool openBarStatus = false;
        private bool pauseTaskGuest = true;
        private bool pauseTaskBartender = true;

        private int counter = 0;

       BlockingCollection<Patron> queBar = new BlockingCollection<Patron>();

        private void ButtonOpenCloseBar_Click(object sender, RoutedEventArgs e)
        {
            if (openBarStatus == false)
            {
                int sekBarOpen = int.Parse(TextBoxOpenBarInSek.Text);
                int nrGusetIntoBar = int.Parse(TextBoxNrGuest.Text);

                //int sekNrGuset = int.Parse(TextBoxTimeGuest.Text);
                //sekNrGuset *= 1000;

                openBarStatus = true;
                pauseTaskGuest = true;
                pauseTaskBartender = true;

                ButtonPauseGuest.IsEnabled = true;

                Bouncer bouncher = new Bouncer();
                BarQue barque = new BarQue();
                PubTimer pubtimer = new PubTimer(sekBarOpen);
                Bartender bartender = new Bartender();

                //bartender.GiveBeer = queBar[0].ResiveBeer;

                pubtimer.PubClosing += ClosePub;
                pubtimer.PubClosing += bouncher.PubIsClosing;

                pubtimer.Tick += updateTimeLeftTextBox;

                Task.Run(() =>
               {

                   pubtimer.Start();

               });

                ButtonOpenCloseBar.Content = "Stäng baren...";

                Task.Run(() =>
                 {
                     while (openBarStatus)
                     {
                         while (pauseTaskGuest)
                         {
                             BlockingCollection<Patron> queBar = bouncher.getRandomPatronName(nrGusetIntoBar);

                            barque.patronBarQue = queBar;

                             //List<Patron> patronsNameListInBar = bouncher.getRandomPatronName(nrGusetIntoBar);

                             Random rndSek = new Random();
                             int sekIntoBar = rndSek.Next(3, 11);
                             Thread.Sleep(sekIntoBar * 1000);  //Släpper in ny gäst efter 3 - 10sek

                             var nrOfPatron = queBar.Take(nrGusetIntoBar);

                             foreach (var patron in nrOfPatron)
                             {
                                 ++counter;
                                 string strListBox = $"{counter}_{patron.Name} kommer in och går till baren";

                                 SafeInsertTextToListBox(ListBoxGuest, strListBox);

                                 string strLabel = "";
                                 if (counter == 1)
                                 {
                                     strLabel = $"Det finns {counter} gäst i baren";
                                 }
                                 else
                                 {
                                     strLabel = $"Det finns {counter} gäster i baren";
                                 }

                                 SafeSetTextToLabel(LabelNrGuset, strLabel);

                             }
                                                          
                         }
                     }
                 });


               

                Task.Run(() =>
                {
                      while (pauseTaskBartender)
                        {
                        int counterBartender = 1;
                        string bartenderTextWait = $"{counterBartender}_{bartender.DoWaitInBar()}";

                            SafeInsertTextToListBox(ListBoxBartender, bartenderTextWait);
                        counterBartender++;

                            Thread.Sleep(10000);

                            Patron firstPatron = barque.FirstPatronInBarQue();

                            string patronText = $"{counterBartender}_{firstPatron.Name}: En stor stark tack!";

                            SafeInsertTextToListBox(ListBoxBartender, patronText);
                        counterBartender++;
                            Thread.Sleep(1000);

                            string bartenderTextPick = $"{counterBartender}_{bartender.DoPickGlas()}";
                            SafeInsertTextToListBox(ListBoxBartender, bartenderTextPick);
                        counterBartender++;
                            Thread.Sleep(3000);

                            string bartenderTextBeer = $"{counterBartender}_{bartender.DoPourBeer()} {firstPatron.Name}";
                            SafeInsertTextToListBox(ListBoxBartender, bartenderTextBeer);
                            Thread.Sleep(3000);
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
            Dispatcher.Invoke(() =>
            {
                ButtonOpenCloseBar.Content = "Öppna baren...";

                ListBoxGuest.Items.Clear();
                ListBoxBartender.Items.Clear();
                ListBoxWaitress.Items.Clear();

            });

            pauseTaskGuest = false;
            openBarStatus = false;
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
            if (pauseTaskGuest)
            {
                ButtonPauseGuest.Content = "Fortsätt";
                pauseTaskGuest = false;
                int nrGusetIntoBar = int.Parse(TextBoxNrGuest.Text);
            }
            else
            {

                ButtonPauseGuest.Content = "Pausa";
                pauseTaskGuest = true;
                int nrGusetIntoBar = int.Parse(TextBoxNrGuest.Text);
            }

        }

        private void updateTimeLeftTextBox(int sekBarOpenLeft)
        {
            SafeSetTextToTextBox(TextBoxOpenBarInSek, sekBarOpenLeft.ToString());
            Bartender bartender = new Bartender();
            bartender.Work(sekBarOpenLeft);
        }

        private void ButtonStopTasks_Click(object sender, RoutedEventArgs e)
        {

            openBarStatus = false;
            pauseTaskGuest = false;


        }
    }
}

