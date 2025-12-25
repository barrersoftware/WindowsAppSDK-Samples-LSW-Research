#pragma once

#include "MainWindow.g.h"

#include <string>
#include <string_view>
#include <utility>
#include <vector>

namespace winrt::FilePickersAppSinglePackaged::implementation
{
    struct MainWindow : MainWindowT<MainWindow>
    {
        MainWindow();

        winrt::fire_and_forget NewPickSingleFile_Click(winrt::Windows::Foundation::IInspectable const& sender, Microsoft::UI::Xaml::RoutedEventArgs const& args);
        winrt::fire_and_forget NewPickMultipleFiles_Click(winrt::Windows::Foundation::IInspectable const& sender, Microsoft::UI::Xaml::RoutedEventArgs const& args);
        winrt::fire_and_forget NewPickSaveFile_Click(winrt::Windows::Foundation::IInspectable const& sender, Microsoft::UI::Xaml::RoutedEventArgs const& args);
        winrt::fire_and_forget NewPickFolder_Click(winrt::Windows::Foundation::IInspectable const& sender, Microsoft::UI::Xaml::RoutedEventArgs const& args);

    private:
        void LogResult(winrt::hstring const& message);
        Microsoft::Windows::Storage::Pickers::PickerLocationId GetSelectedNewLocationId();
        Microsoft::Windows::Storage::Pickers::PickerViewMode GetSelectedNewViewMode();
        std::vector<winrt::hstring> GetFileFilters();
        void AppendChoiceFromJsonPair(
            std::wstring_view pairText,
            std::vector<std::pair<std::wstring, std::vector<std::wstring>>>& orderedChoices);
        std::vector<std::pair<winrt::hstring, std::vector<winrt::hstring>>> DeserizeJsonInsertionOrder(std::wstring & jsonStr);
    };
}

namespace winrt::FilePickersAppSinglePackaged::factory_implementation
{
    struct MainWindow : MainWindowT<MainWindow, implementation::MainWindow>
    {
    };
}
