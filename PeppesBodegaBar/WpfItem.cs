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
    public class WpfItem
    {
        private void SafeInsertTextToListBox(ListBox lb, string str)
        {
           // Task.Run(() => {
               // Dispatcher.Invoke(() =>
                //{
                    lb.Items.Insert(0, str);

               // };

           // });


        }
    }
}
