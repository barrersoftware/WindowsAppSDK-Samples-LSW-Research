using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FilePickersAppSinglePackaged
{

    [JsonSerializable(typeof(Dictionary<string, List<string>>))]
    internal partial class SourceGenerationContext : JsonSerializerContext
    {
    }

    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            Title = "File Pickers Sample App";
            
            // Set window size to make it taller
            this.AppWindow.Resize(new Windows.Graphics.SizeInt32(1800, 1100));
        }

        #region Helper Methods

        private void LogResult(string message)
        {
            ResultsTextBlock.Text = $"[{DateTime.Now:HH:mm:ss}] {message}\n{ResultsTextBlock.Text}";
        }

        private Microsoft.Windows.Storage.Pickers.PickerLocationId GetSelectedNewLocationId()
        {
            switch (StartLocationComboBox.SelectedIndex)
            {
                case 0: return Microsoft.Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
                case 1: return Microsoft.Windows.Storage.Pickers.PickerLocationId.ComputerFolder;
                case 2: return Microsoft.Windows.Storage.Pickers.PickerLocationId.Desktop;
                case 3: return Microsoft.Windows.Storage.Pickers.PickerLocationId.Downloads;

                // HomeGroup is excluded from the new PickerLocationId enum. This example demonstrates how the error would look like.
                case 4: return (Microsoft.Windows.Storage.Pickers.PickerLocationId)4;

                case 5: return Microsoft.Windows.Storage.Pickers.PickerLocationId.MusicLibrary;
                case 6: return Microsoft.Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
                case 7: return Microsoft.Windows.Storage.Pickers.PickerLocationId.VideosLibrary;
                case 8: return Microsoft.Windows.Storage.Pickers.PickerLocationId.Objects3D;
                case 9: return Microsoft.Windows.Storage.Pickers.PickerLocationId.Unspecified;
                case 10: return (Microsoft.Windows.Storage.Pickers.PickerLocationId)10;
                default: throw new InvalidOperationException("Invalid location selected");
            }
        }

        private Microsoft.Windows.Storage.Pickers.PickerViewMode GetSelectedNewViewMode()
        {
            switch (ViewModeComboBox.SelectedIndex)
            {
                case 0: return Microsoft.Windows.Storage.Pickers.PickerViewMode.List;
                case 1: return Microsoft.Windows.Storage.Pickers.PickerViewMode.Thumbnail;
                case 2: return (Microsoft.Windows.Storage.Pickers.PickerViewMode)2;
                default: throw new InvalidOperationException("Invalid view mode selected");
            }
        }

        private string[] GetFileFilters()
        {
            string input = FileTypeFilterInput.Text?.Trim() ?? "";
            if (string.IsNullOrEmpty(input))
                return ["*"];

            return input.Split([','], StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim())
                        .ToArray();
        }

        private IEnumerable<KeyValuePair<string, List<string>>> DeserizlizeJsonWithInsertionOrder(string choicesJson)
        {
            // This method parses the text input to preserve its insertion order.
            // When developers coding with FileTypeChoices, the order can be directly reflected from code and this parsing is not required.
            if (string.IsNullOrWhiteSpace(choicesJson))
            {
                yield break;
            }

            using var document = JsonDocument.Parse(choicesJson);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw new ArgumentException("Expected a JSON object of file type choices.", nameof(choicesJson));
            }

            foreach (var property in document.RootElement.EnumerateObject())
            {
                if (property.Value.ValueKind != JsonValueKind.Array)
                {
                    throw new ArgumentException($"Value for '{property.Name}' must be an array of strings.", nameof(choicesJson));
                }

                var extensions = new List<string>();
                foreach (var item in property.Value.EnumerateArray())
                {
                    if (item.ValueKind != JsonValueKind.String)
                    {
                        throw new ArgumentException($"All extensions for '{property.Name}' must be strings.", nameof(choicesJson));
                    }

                    var extension = item.GetString();
                    if (!string.IsNullOrWhiteSpace(extension))
                    {
                        extensions.Add(extension);
                    }
                }

                LogResult($"Deserialized choice: {property.Name} with extensions: {string.Join(", ", extensions)}");
                yield return new KeyValuePair<string, List<string>>(property.Name, extensions);
            }

        }

        #endregion

        #region FileOpenPicker Tests

        private async void NewPickSingleFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Initialize new picker with AppWindow.Id
                //     for console apps, use the `default` as app window id, for instance:
                //     var picker = new Microsoft.Windows.Storage.Pickers.FileOpenPicker(default);
                var picker = new Microsoft.Windows.Storage.Pickers.FileOpenPicker(this.AppWindow.Id);

                if (CommitButtonCheckBox.IsChecked == true)
                {
                    picker.CommitButtonText = CommitButtonTextInput.Text;
                }

                if (ViewModeCheckBox.IsChecked == true)
                {
                    picker.ViewMode = GetSelectedNewViewMode();
                }

                if (SuggestedStartLocationCheckBox.IsChecked == true)
                {
                    picker.SuggestedStartLocation = GetSelectedNewLocationId();
                }

                if (SuggestedStartFolderCheckBox.IsChecked == true)
                {
                    picker.SuggestedStartFolder = SuggestedStartFolderInput.Text;
                }

                if (SuggestedFolderCheckBox.IsChecked == true)
                {
                    picker.SuggestedFolder = SuggestedFolderInput.Text;
                }

                if (FileTypeFilterCheckBox.IsChecked == true)
                {
                    foreach (var filter in GetFileFilters())
                    {
                        picker.FileTypeFilter.Add(filter);
                    }
                }

                if (OpenPickerFileTypeChoicesCheckBox.IsChecked == true)
                {
                    var choicesJson = (string)OpenPickerFileTypeChoicesInput.Text;
                    if (!string.IsNullOrEmpty(choicesJson))
                    {
                        foreach(var choice in DeserizlizeJsonWithInsertionOrder(choicesJson))
                        {
                            picker.FileTypeChoices.Add(choice.Key, choice.Value);
                        }
                    }
                }

                var result = await picker.PickSingleFileAsync();
                if (result != null)
                {
                    LogResult($"New FileOpenPicker - PickSingleFileAsync:\nFile: {System.IO.Path.GetFileName(result.Path)}\nPath: {result.Path}");
                }
                else
                {
                    LogResult("New FileOpenPicker - PickSingleFileAsync: Operation cancelled");
                }
            }
            catch (Exception ex)
            {
                LogResult($"Error in New FileOpenPicker: {ex.Message}");
            }
        }

        private async void NewPickMultipleFiles_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Initialize new picker with AppWindow.Id
                //     for console apps, use the `default` as app window id, for instance:
                //     var picker = new Microsoft.Windows.Storage.Pickers.FileOpenPicker(default);
                var picker = new Microsoft.Windows.Storage.Pickers.FileOpenPicker(this.AppWindow.Id);

                if (CommitButtonCheckBox.IsChecked == true)
                {
                    picker.CommitButtonText = CommitButtonTextInput.Text;
                }

                if (ViewModeCheckBox.IsChecked == true)
                {
                    picker.ViewMode = GetSelectedNewViewMode();
                }

                if (SuggestedStartLocationCheckBox.IsChecked == true)
                {
                    picker.SuggestedStartLocation = GetSelectedNewLocationId();
                }

                if (SuggestedStartFolderCheckBox.IsChecked == true)
                {
                    picker.SuggestedStartFolder = SuggestedStartFolderInput.Text;
                }

                if (SuggestedFolderCheckBox.IsChecked == true)
                {
                    picker.SuggestedFolder = SuggestedFolderInput.Text;
                }

                if (FileTypeFilterCheckBox.IsChecked == true)
                {
                    foreach (var filter in GetFileFilters())
                    {
                        picker.FileTypeFilter.Add(filter);
                    }
                }

                if (OpenPickerFileTypeChoicesCheckBox.IsChecked == true)
                {
                    var choicesJson = (string)OpenPickerFileTypeChoicesInput.Text;
                    if (!string.IsNullOrEmpty(choicesJson))
                    {
                        foreach(var choice in DeserizlizeJsonWithInsertionOrder(choicesJson))
                        {
                            picker.FileTypeChoices.Add(choice.Key, choice.Value);
                        }
                    }
                }

                var results = await picker.PickMultipleFilesAsync();
                if (results != null && results.Count > 0)
                {
                    var sb = new StringBuilder($"New FileOpenPicker - PickMultipleFilesAsync: {results.Count} files\n");
                    foreach (var result in results)
                    {
                        sb.AppendLine($"- {System.IO.Path.GetFileName(result.Path)}: {result.Path}");
                    }
                    LogResult(sb.ToString());
                }
                else
                {
                    LogResult("New FileOpenPicker - PickMultipleFilesAsync: Operation cancelled or no files selected");
                }
            }
            catch (Exception ex)
            {
                LogResult($"Error in New PickMultipleFilesAsync: {ex.Message}");
            }
        }

        #endregion

        #region FileSavePicker Tests

        private async void NewPickSaveFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Initialize new picker with AppWindow.Id
                //     for console apps, use the `default` as app window id, for instance:
                //     var picker = new Microsoft.Windows.Storage.Pickers.FileSavePicker(default);
                var picker = new Microsoft.Windows.Storage.Pickers.FileSavePicker(this.AppWindow.Id);

                if (SuggestedFileNameCheckBox.IsChecked == true)
                {
                    picker.SuggestedFileName = SuggestedFileNameInput.Text;
                }

                if (DefaultFileExtensionCheckBox.IsChecked == true)
                {
                    picker.DefaultFileExtension = DefaultFileExtensionInput.Text;
                }

                if (SuggestedFolderCheckBox.IsChecked == true)
                {
                    picker.SuggestedFolder = SuggestedFolderInput.Text;
                }

                if (SavePickerFileTypeChoicesCheckBox.IsChecked == true)
                {
                    var choicesJson = (string)SavePickerFileTypeChoicesInput.Text;
                    if (!string.IsNullOrEmpty(choicesJson))
                    {
                        foreach(var choice in DeserizlizeJsonWithInsertionOrder(choicesJson))
                        {
                            picker.FileTypeChoices.Add(choice.Key, choice.Value);
                        }
                    }
                }

                if (CommitButtonCheckBox.IsChecked == true)
                {
                    picker.CommitButtonText = CommitButtonTextInput.Text;
                }

                if (SuggestedStartLocationCheckBox.IsChecked == true)
                {
                    picker.SuggestedStartLocation = GetSelectedNewLocationId();
                }

                if (SuggestedStartFolderCheckBox.IsChecked == true)
                {
                    picker.SuggestedStartFolder = SuggestedStartFolderInput.Text;
                }

                if (SuggestedFolderCheckBox.IsChecked == true)
                {
                    picker.SuggestedFolder = SuggestedFolderInput.Text;
                }

                var result = await picker.PickSaveFileAsync();
                if (result != null)
                {
                    LogResult($"New FileSavePicker picked file: \n{System.IO.Path.GetFileName(result.Path)}\nPath: {result.Path}");
                }
                else
                {
                    LogResult("New FileSavePicker with FileTypeChoices\nOperation cancelled");
                }
            }
            catch (Exception ex)
            {
                LogResult($"Error in New FileTypeChoices: {ex.Message}");
            }
        }

        #endregion

        #region FolderPicker Tests

        private async void NewPickFolder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Initialize new picker with AppWindow.Id
                //     for console apps, use the `default` as app window id, for instance:
                //     var picker = new Microsoft.Windows.Storage.Pickers.FolderPicker(default);
                var picker = new Microsoft.Windows.Storage.Pickers.FolderPicker(this.AppWindow.Id);

                if (CommitButtonCheckBox.IsChecked == true)
                {
                    picker.CommitButtonText = CommitButtonTextInput.Text;
                }

                if (SuggestedStartLocationCheckBox.IsChecked == true)
                {
                    picker.SuggestedStartLocation = GetSelectedNewLocationId();
                }

                if (SuggestedStartFolderCheckBox.IsChecked == true)
                {
                    picker.SuggestedStartFolder = SuggestedStartFolderInput.Text;
                }

                if (SuggestedFolderCheckBox.IsChecked == true)
                {
                    picker.SuggestedFolder = SuggestedFolderInput.Text;
                }

                var result = await picker.PickSingleFolderAsync();
                if (result != null)
                {
                    LogResult($"New FolderPicker - PickSingleFolderAsync:\nFolder: {System.IO.Path.GetFileName(result.Path)}\nPath: {result.Path}");
                }
                else
                {
                    LogResult("New FolderPicker - PickSingleFolderAsync: Operation cancelled");
                }
            }
            catch (Exception ex)
            {
                LogResult($"Error in New FolderPicker: {ex.Message}");
            }
        }

        #endregion
    }

}
