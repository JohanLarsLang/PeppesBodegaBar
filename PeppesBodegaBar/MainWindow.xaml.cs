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

            TextBoxAntalGlas.Text = "3";
            TextBoxAntalStolar.Text = "9";
        }

        private bool openBarStatus = false;
        private bool pauseTaskGuest = true;
        private bool pauseTaskBartender = true;
        private bool pauseTaskWaiter = true;

        private int counter = 0;

        //BlockingCollection<Patron> queBar = new BlockingCollection<Patron>();
        List<Patron> queBar = new List<Patron>();

        private void ButtonOpenCloseBar_Click(object sender, RoutedEventArgs e)
        {
            if (openBarStatus == false)
            {
                int sekBarOpen = int.Parse(TextBoxOpenBarInSek.Text);
                int nrGusetIntoBar = int.Parse(TextBoxNrGuest.Text);

                int antalGlas = int.Parse(TextBoxAntalGlas.Text);
                int antalStolar = int.Parse(TextBoxAntalStolar.Text);

                string sekNrGuset = TextBoxTimeGuest.Text;


                //sekNrGuset *= 1000;

                openBarStatus = true;
                pauseTaskGuest = true;
                pauseTaskBartender = true;
                pauseTaskWaiter = true;

                Dispatcher.Invoke(() =>
                {
                    ButtonPauseGuest.IsEnabled = true;
                    ButtonPauseBartender.IsEnabled = true;
                    ButtonPauseWaiter.IsEnabled = true;
                });

                Bouncer bouncher = new Bouncer();
                PubTimer pubtimer = new PubTimer(sekBarOpen);
                Bartender bartender = new Bartender();
                BarQue barque = new BarQue();

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
                                 ++counter;
                                 Patron patron = bouncher.getRandomPatron();
                                 queBar.Add(patron);
                                 //barque.AddPatronInBarQue(patron);

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
                        if (queBar.Count > 0 && antalGlas > 0)
                        {
                            int counterBartender = 1;
                            string bartenderTextWait = $"{counterBartender}_Bartender väntar på besökare";

                            string antalGlasText = $"Det finns {antalGlas} antal glas i hyllan";
                            SafeSetTextToLabel(LabelAntalGlas, antalGlasText);

                            SafeInsertTextToListBox(ListBoxBartender, bartenderTextWait);
                            counterBartender++;

                            Thread.Sleep(2000);


                            // BlockingCollection<Patron> BarQue = barque.GetpatronBarQue;
                            //if (BarQue.Count > 0)
                            //{

                            //Patron firstPatron = queBar.Take(1).First();
                            //queBar.Take(1);

                            //bartender.GiveBeer = queBar[0].ResiveBeer;

                            Patron firstPatron = queBar[0];
                            queBar.RemoveAt(0);
                            string patronText = $"{counterBartender}_{firstPatron.Name}: En stor stark öl tack!";

                            SafeInsertTextToListBox(ListBoxBartender, patronText);
                            counterBartender++;
                            Thread.Sleep(2000);

                            string bartenderTextPick = $"{counterBartender}_Bartender hämtar glas";
                            SafeInsertTextToListBox(ListBoxBartender, bartenderTextPick);
                            counterBartender++;
                            antalGlas--;
                            SafeSetTextToTextBox(TextBoxAntalGlas, antalGlas.ToString());
                            antalGlasText = $"Det finns {antalGlas} antal glas i hyllan";
                            SafeSetTextToLabel(LabelAntalGlas, antalGlasText);
                            Thread.Sleep(3000);

                            string bartenderTextBeer = $"{counterBartender}_Bartender häller upp öl till {firstPatron.Name}";
                            SafeInsertTextToListBox(ListBoxBartender, bartenderTextBeer);
                            Thread.Sleep(3000);
                        }
                    }
                
                });

            Task.Run(() =>
            {
                while (pauseTaskWaiter)
                {
                    int counterWaiter = 1;
                    string waiterTextWait = $"{counterWaiter}_Väntar på besökare";

                    string antalStolarText = $"Det finns {antalStolar} lediga stolar i baren";
                    SafeSetTextToLabel(LabelAntalStolar, antalStolarText);

                    SafeInsertTextToListBox(ListBoxWaitress, waiterTextWait);
                    counterWaiter++;

                    Thread.Sleep(10000);

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

    openBarStatus = true;
    pauseTaskGuest = true;
    pauseTaskBartender = true;
    pauseTaskWaiter = true;
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
    Bouncer bouncer = new Bouncer();

    if (pauseTaskGuest)
    {
        ButtonPauseGuest.Content = "Fortsätt";
        pauseTaskGuest = false;
        int nrGusetIntoBar = int.Parse(TextBoxNrGuest.Text);
        bouncer.nrOfGuest(nrGusetIntoBar);
    }
    else
    {

        ButtonPauseGuest.Content = "Pausa";
        pauseTaskGuest = true;
        int nrGusetIntoBar = int.Parse(TextBoxNrGuest.Text);
        bouncer.nrOfGuest(nrGusetIntoBar);

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

    openBarStatus = false;
    pauseTaskGuest = false;
    pauseTaskBartender = false;


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

}
    }
}

