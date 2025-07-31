using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PDFRedaction
{
    /// <summary>
    /// Interaction logic for ConsolePopup.xaml
    /// </summary>
    public partial class ConsolePopup : Window
    {
        public ConsolePopup()
        {
            InitializeComponent();
        }
        public void WriteLine(string message)
        {
            ConsoleOutput.AppendText(message + Environment.NewLine);
            ConsoleOutput.ScrollToEnd();
        }
        public void Clear()
        { ConsoleOutput.Clear(); }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            ConsoleOutput.Clear();  // Clear the text
            this.Close();
        }
    }
}
