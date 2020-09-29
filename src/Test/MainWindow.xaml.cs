using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
        }

        private FCS.IFCS fcsset;

        public FCS.IFCS FCSSet
        {
            get { return fcsset; }
            set { fcsset = value; OnPropertyChanged("FCSSet"); }
        }

        private string message;

        public string Message
        {
            get { return message; }
            set { message = value; OnPropertyChanged("Message"); }
        }


        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            open.Filter = "FCS文件|*.fcs";
            if (open.ShowDialog() == true)
            {
                var fcss = FCS.Factory.ReadFCSFile(open.FileName).ToList();
                if (fcss.Count == 1)
                {
                    this.FCSSet = fcss[0];
                    Message = this.FCSSet.TOT.ToString();
                }
            }
        }
    }
}
