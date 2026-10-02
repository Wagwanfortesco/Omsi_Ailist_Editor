using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace Omsi_Ailist_Editor
{
    /// <summary>
    /// Interaction logic for RepaintSelect.xaml
    /// </summary>
    public partial class RepaintSelect : Window
    {
        public List<string> Repaints { get; set; } = new List<string>();
        public string SelectedRepaint { get; set; } = string.Empty;
        public RepaintSelect(List<string> repaints)
        {
            InitializeComponent();
            RepaintListBox.Visibility = Visibility.Hidden;
            Repaints = repaints;
            RepaintListBox.ItemsSource = Repaints;
            RepaintListBox.Visibility = Visibility.Visible;

        }
        private void Repaint_list_click(object sender, MouseButtonEventArgs e)
        {
            if (RepaintListBox.SelectedItem != null)
            {
                SelectedRepaint = RepaintListBox.SelectedItem.ToString();
                MessageBoxResult result = MessageBox.Show($"You selected: {SelectedRepaint}", "Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true; 
                this.Close();
            }
        }
    }
}
