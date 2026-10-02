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

namespace Omsi_Ailist_Editor
{
    /// <summary>
    /// Interaction logic for Multiple_Bus_In_Depot_Select.xaml
    /// </summary>

    public partial class Multiple_Bus_In_Depot_Select : Window
    {
        public List<string> BusTypes { get; set; } = new List<string>();
        public string SelectedBus { get; private set; } = null;
        public bool IsSelectionMade { get; private set; } = false;

        public Multiple_Bus_In_Depot_Select(List<string> bustypes)
        {
            InitializeComponent();
            BusTypes = bustypes;
            Bus_Select_Box.ItemsSource = BusTypes;
            Bus_Select_Box.Visibility = Visibility.Visible;
        }

        private void Bus_Select_Box_Click(object sender, MouseButtonEventArgs e)
        {
            if (Bus_Select_Box.SelectedItem != null)
            {
                SelectedBus = Bus_Select_Box.SelectedItem.ToString();
                MessageBox.Show($"You selected: {SelectedBus}", "Selection", MessageBoxButton.OK, MessageBoxImage.Information);
                IsSelectionMade = true;
                this.Close();
            }
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // Ensure the application does not crash if the user presses 'X'
            if (!IsSelectionMade)
            {
                SelectedBus = null; // No selection was made
            }
            base.OnClosing(e);
        }
    }
}