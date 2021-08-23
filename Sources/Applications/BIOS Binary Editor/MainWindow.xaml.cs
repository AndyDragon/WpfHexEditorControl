using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using WpfHexaEditor.Core;

namespace BIOS_Binary_Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string tempFileName;
        int romSize;

        public MainWindow()
        {
            InitializeComponent();
            HexEdit.TypeOfCharacterTable = CharacterTableType.Ascii;
        }

        public bool HaveFileOpen
        {
            get => (bool)GetValue(HaveFileOpenProperty);
            set => SetValue(HaveFileOpenProperty, value);
        }

        public static readonly DependencyProperty HaveFileOpenProperty =
            DependencyProperty.Register(nameof(HaveFileOpen), typeof(bool), typeof(MainWindow),
                new FrameworkPropertyMetadata(false, (DependencyObject d, DependencyPropertyChangedEventArgs e) => 
                {
                    if (d is not MainWindow wnd || e.NewValue == e.OldValue) return;

                    int bankCount = wnd.romSize / 128;
                    wnd.ReplaceBank0.IsEnabled = wnd.ClearBank0.IsEnabled = wnd.ShowBank0.IsEnabled = (bool)e.NewValue && bankCount > 1;
                    wnd.ReplaceBank1.IsEnabled = wnd.ClearBank1.IsEnabled = wnd.ShowBank1.IsEnabled = (bool)e.NewValue && bankCount > 1;
                    wnd.ReplaceBank2.IsEnabled = wnd.ClearBank2.IsEnabled = wnd.ShowBank2.IsEnabled = (bool)e.NewValue && bankCount > 2;
                    wnd.ReplaceBank3.IsEnabled = wnd.ClearBank3.IsEnabled = wnd.ShowBank3.IsEnabled = (bool)e.NewValue && bankCount > 2;
                    wnd.ReplaceBank4.IsEnabled = wnd.ClearBank4.IsEnabled = wnd.ShowBank4.IsEnabled = (bool)e.NewValue && bankCount > 4;
                    wnd.ReplaceBank5.IsEnabled = wnd.ClearBank5.IsEnabled = wnd.ShowBank5.IsEnabled = (bool)e.NewValue && bankCount > 4;
                    wnd.ReplaceBank6.IsEnabled = wnd.ClearBank6.IsEnabled = wnd.ShowBank6.IsEnabled = (bool)e.NewValue && bankCount > 4;
                    wnd.ReplaceBank7.IsEnabled = wnd.ClearBank7.IsEnabled = wnd.ShowBank7.IsEnabled = (bool)e.NewValue && bankCount > 4;
                    wnd.ReplaceBank8.IsEnabled = wnd.ClearBank8.IsEnabled = wnd.ShowBank8.IsEnabled = (bool)e.NewValue && bankCount > 8;
                    wnd.ReplaceBank9.IsEnabled = wnd.ClearBank9.IsEnabled = wnd.ShowBank9.IsEnabled = (bool)e.NewValue && bankCount > 8;
                    wnd.ReplaceBankA.IsEnabled = wnd.ClearBankA.IsEnabled = wnd.ShowBankA.IsEnabled = (bool)e.NewValue && bankCount > 8;
                    wnd.ReplaceBankB.IsEnabled = wnd.ClearBankB.IsEnabled = wnd.ShowBankB.IsEnabled = (bool)e.NewValue && bankCount > 8;
                    wnd.ReplaceBankC.IsEnabled = wnd.ClearBankC.IsEnabled = wnd.ShowBankC.IsEnabled = (bool)e.NewValue && bankCount > 8;
                    wnd.ReplaceBankD.IsEnabled = wnd.ClearBankD.IsEnabled = wnd.ShowBankD.IsEnabled = (bool)e.NewValue && bankCount > 8;
                    wnd.ReplaceBankE.IsEnabled = wnd.ClearBankE.IsEnabled = wnd.ShowBankE.IsEnabled = (bool)e.NewValue && bankCount > 8;
                    wnd.ReplaceBankF.IsEnabled = wnd.ClearBankF.IsEnabled = wnd.ShowBankF.IsEnabled = (bool)e.NewValue && bankCount > 8;
                }));

        private void CleanUpAnyTempFile(bool clearEditor = true)
        {
            if (!string.IsNullOrEmpty(tempFileName) && File.Exists(tempFileName))
            {
                if (clearEditor)
                {
                    HexEdit.CloseProvider();
                }
                File.Delete(tempFileName);
                tempFileName = null;
                HaveFileOpen = false;
            }
        }

        private void NewMenu_Click(object sender, RoutedEventArgs e)
        {
            var romPicker = new BiosPickerDialog();
            if (!romPicker.ShowDialog() ?? false)
            {
                return;
            }
            romSize = romPicker.RomSize;

            CleanUpAnyTempFile();
            tempFileName = Path.GetTempFileName();
            using (FileStream stream = File.Create(tempFileName))
            {
                byte[] section = new byte[romSize * 128];
                Array.Fill(section, (byte)0xff);
                stream.Write(section, 0, section.Length);
                stream.Close();
            }
            HexEdit.FileName = tempFileName;
            HaveFileOpen = true;
        }

        private void OpenMenu_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new OpenFileDialog
            {
                Multiselect = false,
                CheckFileExists = true
            };

            if (fileDialog.ShowDialog() == null || !File.Exists(fileDialog.FileName))
            {
                return;
            }

            long fileLength = new FileInfo(fileDialog.FileName).Length;
            // 32K, 64K and 256K
            if (fileLength != 0x8000 && fileLength != 0x10000 && fileLength != 0x40000)
            {
                MessageBox.Show("The file is not the right size, the file must be 32K (32768 bytes), 64K (65536 bytes) or 256K (262144 bytes)", "File wrong size", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            romSize = (int)fileLength / 128;

            CleanUpAnyTempFile();

            Application.Current.MainWindow.Cursor = Cursors.Wait;

            HexEdit.FileName = fileDialog.FileName;
            HaveFileOpen = true;

            Application.Current.MainWindow.Cursor = null;
        }

        private void ExitMenu_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SaveMenu_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(tempFileName))
            {
                SaveAsMenu_Click(sender, e);
                return;
            }

            Application.Current.MainWindow.Cursor = Cursors.Wait;

            HexEdit.SubmitChanges();

            Application.Current.MainWindow.Cursor = null;
        }

        private void SaveAsMenu_Click(object sender, RoutedEventArgs e)
        {
            var fileDialog = new SaveFileDialog();

            if (fileDialog.ShowDialog() is not null)
            {
                Application.Current.MainWindow.Cursor = Cursors.Wait;

                HexEdit.SubmitChanges(fileDialog.FileName, true);
                CleanUpAnyTempFile(false);

                Application.Current.MainWindow.Cursor = null;
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            CleanUpAnyTempFile();
            e.Cancel = false;
        }

        private void ReplaceBank(int bank)
        {
            var fileDialog = new OpenFileDialog
            {
                Multiselect = false,
                CheckFileExists = true
            };

            if (fileDialog.ShowDialog() == null || !File.Exists(fileDialog.FileName))
            {
                return;
            }

            long fileLength = new FileInfo(fileDialog.FileName).Length;
            if (fileLength > 0x4000)
            {
                MessageBox.Show("The file is too large for the bank, the file must be 16K or smaller", "File too large", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Application.Current.MainWindow.Cursor = Cursors.Wait;

            using (FileStream stream = File.OpenRead(fileDialog.FileName))
            {
                byte[] buffer = new byte[0x4000];
                if (stream.Read(buffer, 0, (int)fileLength) != fileLength)
                {
                    MessageBox.Show("Failed to read the contents of the file for the bank", "Read error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                HexEdit.ReplaceBytes(buffer, bank * 0x4000);
                HexEdit.SetPosition(bank * 0x4000, 1);
            }

            Application.Current.MainWindow.Cursor = null;
        }

        private void ClearBank(int bank)
        {
            Application.Current.MainWindow.Cursor = Cursors.Wait;

            byte[] buffer = new byte[0x4000];
            Array.Fill(buffer, (byte)0xff);
            HexEdit.ReplaceBytes(buffer, bank * 0x4000);
            HexEdit.SetPosition(bank * 0x4000, 1);

            Application.Current.MainWindow.Cursor = null;
        }

        private void ShowBank(int bank)
        {
            Application.Current.MainWindow.Cursor = Cursors.Wait;

            HexEdit.SetPosition(bank * 0x4000, 1);

            Application.Current.MainWindow.Cursor = null;
        }

        private void ReplaceBank0_Click(object sender, RoutedEventArgs e)
        {
            ReplaceBank(0);
        }

        private void ReplaceBank1_Click(object sender, RoutedEventArgs e)
        {
            ReplaceBank(1);
        }

        private void ReplaceBank2_Click(object sender, RoutedEventArgs e)
        {
            ReplaceBank(2);
        }

        private void ReplaceBank3_Click(object sender, RoutedEventArgs e)
        {
            ReplaceBank(3);
        }

        private void ReplaceBank4_Click(object sender, RoutedEventArgs e)
        {
            ReplaceBank(4);
        }

        private void ReplaceBank5_Click(object sender, RoutedEventArgs e)
        {
            ReplaceBank(5);
        }

        private void ReplaceBank6_Click(object sender, RoutedEventArgs e)
        {
            ReplaceBank(6);
        }

        private void ReplaceBank7_Click(object sender, RoutedEventArgs e)
        {
            ReplaceBank(7);
        }

        private void ReplaceBank8_Click(object sender, RoutedEventArgs e)
        {
            ReplaceBank(8);
        }

        private void ReplaceBank9_Click(object sender, RoutedEventArgs e)
        {
            ReplaceBank(9);
        }

        private void ReplaceBankA_Click(object sender, RoutedEventArgs e)
        {
            ReplaceBank(10);
        }

        private void ReplaceBankB_Click(object sender, RoutedEventArgs e)
        {
            ReplaceBank(11);
        }

        private void ReplaceBankC_Click(object sender, RoutedEventArgs e)
        {
            ReplaceBank(12);
        }

        private void ReplaceBankD_Click(object sender, RoutedEventArgs e)
        {
            ReplaceBank(13);
        }

        private void ReplaceBankE_Click(object sender, RoutedEventArgs e)
        {
            ReplaceBank(14);
        }

        private void ReplaceBankF_Click(object sender, RoutedEventArgs e)
        {
            ReplaceBank(15);
        }

        private void ClearBank0_Click(object sender, RoutedEventArgs e)
        {
            ClearBank(0);
        }

        private void ClearBank1_Click(object sender, RoutedEventArgs e)
        {
            ClearBank(1);
        }

        private void ClearBank2_Click(object sender, RoutedEventArgs e)
        {
            ClearBank(2);
        }

        private void ClearBank3_Click(object sender, RoutedEventArgs e)
        {
            ClearBank(3);
        }

        private void ClearBank4_Click(object sender, RoutedEventArgs e)
        {
            ClearBank(4);
        }

        private void ClearBank5_Click(object sender, RoutedEventArgs e)
        {
            ClearBank(5);
        }

        private void ClearBank6_Click(object sender, RoutedEventArgs e)
        {
            ClearBank(6);
        }

        private void ClearBank7_Click(object sender, RoutedEventArgs e)
        {
            ClearBank(7);
        }

        private void ClearBank8_Click(object sender, RoutedEventArgs e)
        {
            ClearBank(8);
        }

        private void ClearBank9_Click(object sender, RoutedEventArgs e)
        {
            ClearBank(9);
        }

        private void ClearBankA_Click(object sender, RoutedEventArgs e)
        {
            ClearBank(10);
        }

        private void ClearBankB_Click(object sender, RoutedEventArgs e)
        {
            ClearBank(11);
        }

        private void ClearBankC_Click(object sender, RoutedEventArgs e)
        {
            ClearBank(12);
        }

        private void ClearBankD_Click(object sender, RoutedEventArgs e)
        {
            ClearBank(13);
        }

        private void ClearBankE_Click(object sender, RoutedEventArgs e)
        {
            ClearBank(14);
        }

        private void ClearBankF_Click(object sender, RoutedEventArgs e)
        {
            ClearBank(15);
        }

        private void ShowBank0_Click(object sender, RoutedEventArgs e)
        {
            ShowBank(0);
        }

        private void ShowBank1_Click(object sender, RoutedEventArgs e)
        {
            ShowBank(1);
        }

        private void ShowBank2_Click(object sender, RoutedEventArgs e)
        {
            ShowBank(2);
        }

        private void ShowBank3_Click(object sender, RoutedEventArgs e)
        {
            ShowBank(3);
        }

        private void ShowBank4_Click(object sender, RoutedEventArgs e)
        {
            ShowBank(4);
        }

        private void ShowBank5_Click(object sender, RoutedEventArgs e)
        {
            ShowBank(5);
        }

        private void ShowBank6_Click(object sender, RoutedEventArgs e)
        {
            ShowBank(6);
        }

        private void ShowBank7_Click(object sender, RoutedEventArgs e)
        {
            ShowBank(7);
        }

        private void ShowBank8_Click(object sender, RoutedEventArgs e)
        {
            ShowBank(8);
        }

        private void ShowBank9_Click(object sender, RoutedEventArgs e)
        {
            ShowBank(9);
        }

        private void ShowBankA_Click(object sender, RoutedEventArgs e)
        {
            ShowBank(10);
        }

        private void ShowBankB_Click(object sender, RoutedEventArgs e)
        {
            ShowBank(11);
        }

        private void ShowBankC_Click(object sender, RoutedEventArgs e)
        {
            ShowBank(12);
        }

        private void ShowBankD_Click(object sender, RoutedEventArgs e)
        {
            ShowBank(13);
        }

        private void ShowBankE_Click(object sender, RoutedEventArgs e)
        {
            ShowBank(14);
        }

        private void ShowBankF_Click(object sender, RoutedEventArgs e)
        {
            ShowBank(15);
        }
    }
}
