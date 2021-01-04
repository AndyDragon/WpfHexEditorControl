﻿//////////////////////////////////////////////
// Apache 2.0  - 2021
// Author : Derek Tremblay (derektremblay666@gmail.com)
//
//
// NOT A TRUE PROJECT! IT'S JUST FOR TESTING THE HEXEDITOR... DO NOT WATCH THE CODE LOL ;) 
//////////////////////////////////////////////

using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using WpfHexaEditor;
using WpfHexaEditor.Core;

namespace WpfHexEditor.Sample.BinaryFilesDifference
{
    public partial class MainWindow : Window
    {        
        /// <summary>
        /// Used to catch internal change for cath potential infinite loop
        /// </summary>
        private bool _internalChange = false;

        public MainWindow() => InitializeComponent();

        #region Various controls events
        private void FirstHexEditor_Click(object sender, RoutedEventArgs e) => OpenFile(FirstFile);

        private void SecondHexEditor_Click(object sender, RoutedEventArgs e) => OpenFile(SecondFile);

        private void FindDifferenceButton_Click(object sender, RoutedEventArgs e)
        {
            if (FirstFile.FileName == string.Empty || SecondFile.FileName == string.Empty)
            {
                MessageBox.Show("LOAD TWO FILE!!", "HexEditor sample", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            Application.Current.MainWindow.Cursor = Cursors.Wait;
            FindDifference();
            Application.Current.MainWindow.Cursor = null;
        }

        private void FileDifferenceList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (_internalChange) return;
            if (FileDifferenceList.SelectedItem == null) return;

            var position = ((string)FileDifferenceList.SelectedItem).Split("  ")[0];

            _internalChange = true;
            FirstFile.SetPosition(position, 1);
            SecondFile.SetPosition(position, 1);
            _internalChange = false;
        }
        #endregion

        #region Various methods
        private void OpenFile(HexEditor hexEditor)
        {
            ClearDifference();

            #region Create file dialog
            var fileDialog = new OpenFileDialog
            {
                Multiselect = true,
                CheckFileExists = true
            };

            if (fileDialog.ShowDialog() == null || !File.Exists(fileDialog.FileName)) return;
            #endregion

            hexEditor.FileName = fileDialog.FileName;
        }

        /// <summary>
        /// Clear the difference in various control
        /// </summary>
        private void ClearDifference()
        {
            FileDifferenceList.Items.Clear();
            FirstFile.CustomBackgroundBlockItems.Clear();
            SecondFile.CustomBackgroundBlockItems.Clear();
        }

        private void FindDifference()
        {
            ClearDifference();

            if (FirstFile.FileName == string.Empty || SecondFile.FileName == string.Empty) return;

            var firstFileLength = FirstFile.Length;
            var secondFileLength = SecondFile.Length;
            var maxLenght = firstFileLength > secondFileLength ? firstFileLength : secondFileLength;

            for(int i = 0; i < maxLenght; i++)
            {
                var firstFileByte = FirstFile.GetByte(i, true);
                var secondFileByte = SecondFile.GetByte(i, true);
                var equal = firstFileByte.singleByte == secondFileByte.singleByte;

                if (equal) continue;

                var cbb = new CustomBackgroundBlock(i, 1, Brushes.Cyan);

                FirstFile.CustomBackgroundBlockItems.Add(cbb);
                SecondFile.CustomBackgroundBlockItems.Add(cbb);




                string test = $"0x{i:X2}  Ori: 0x{firstFileByte.singleByte.Value:X2}  Mod: 0x{secondFileByte.singleByte.Value:X2}";

                FileDifferenceList.Items.Add(test);


            }

            FirstFile.RefreshView();
            SecondFile.RefreshView();
        }
        #endregion

        #region Synchronise the two hexeditor
        private void FirstFile_VerticalScrollBarChanged(object sender, WpfHexaEditor.Core.EventArguments.ByteEventArgs e)
        {
            if (_internalChange) return;

            _internalChange = true;
            SecondFile.SetPosition(e.BytePositionInStream);
            _internalChange = false;
        }

        private void SecondFile_VerticalScrollBarChanged(object sender, WpfHexaEditor.Core.EventArguments.ByteEventArgs e)
        {
            if (_internalChange) return;

            _internalChange = true;
            FirstFile.SetPosition(e.BytePositionInStream);
            _internalChange = false;
        }
        #endregion
    }
}