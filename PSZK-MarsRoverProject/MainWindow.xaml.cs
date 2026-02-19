using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PSZK_MarsRoverProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string[,] terkep = new string[50, 50];
        public MainWindow()
        {
            InitializeComponent();
            CsvBeolvaso();
            for (int i = 0; i < terkep.GetLength(0); i++)
            {
                for (int j = 0; j < terkep.GetLength(1); j++)
                {
                    TextBlock textBlock = new TextBlock()
                    {
                        Height = 20,
                        Width = 20,
                        Text = terkep[i, j],
                        TextAlignment = TextAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Top,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Foreground = terkep[i, j] == "#" ? Brushes.Red : Brushes.White,
                        Margin = new Thickness(10 + j * 20,10 + i * 20, 0, 0)
                    };
                    jatekter.Children.Add(textBlock);
                }
            }
        }

        private void CsvBeolvaso()
        {
            if (!File.Exists("mars_map_50x50.csv"))
            {
                MessageBox.Show("A térkép fájl nem található!");
                Environment.Exit(1);
            }
            string[] sorok = File.ReadAllLines("mars_map_50x50.csv");
            for (int i = 0; i < sorok.Length; i++)
            {
                string[] elemek = sorok[i].Split(',');
                for (int j = 0; j < elemek.Length && j < 50; j++)
                {
                    terkep[i, j] = elemek[j];
                }
            }
        }
    }
}