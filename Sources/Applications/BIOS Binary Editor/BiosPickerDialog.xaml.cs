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

namespace BIOS_Binary_Editor
{
    /// <summary>
    /// Interaction logic for BiosPickerDialog.xaml
    /// </summary>
    public partial class BiosPickerDialog : Window
    {
        public BiosPickerDialog()
        {
            InitializeComponent();
        }

        public int RomSize
        {
            get
            {
                return int.Parse(((ComboBoxItem)RomComboBox.SelectedItem).Tag as string);
            }
        }

        private void OK_Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Cancel_Button_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
