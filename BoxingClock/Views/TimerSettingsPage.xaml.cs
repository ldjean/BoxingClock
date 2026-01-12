using BoxingClock.Controls;
using BoxingClock.Models;
using BoxingClock.ViewModels;
using System;
using Microsoft.Maui.Controls;

namespace BoxingClock.Views;

public partial class TimerSettingsPage : ContentView
{
    private TimePickerDialog _timePickerDialog;

    public TimerSettingsPage()
    {
        InitializeComponent();
        InitializeTimePickerDialog();
    }

    private void InitializeTimePickerDialog()
    {
        // Create the TimePickerDialog
        _timePickerDialog = new TimePickerDialog();
        _timePickerDialog.TimeSelected += OnTimeSelected;
        _timePickerDialog.Cancelled += OnTimePickerCancelled;
    }

    private async void OnRoundTimeTapped(object sender, EventArgs e)
    {
        // Scale animation for visual feedback
        if (sender is View view)
        {
            await view.ScaleTo(0.95, 70);
            await view.ScaleTo(1, 30);
        }

        // Get current time from ViewModel
        var viewModel = BindingContext as BoxingTimerVM;
        if (viewModel != null && _timePickerDialog != null)
        {
            var currentTime = viewModel.CurrentUISettings?.RoundTime;
            if (!string.IsNullOrEmpty(currentTime))
            {
                _timePickerDialog.SetInitialTime(currentTime);
            }

            // Show the dialog
            ShowDialog();
        }
    }

    private void ShowDialog()
    {
        // Add to main page if not already added
        if (_timePickerDialog.Parent == null)
        {
            var mainPage = Application.Current?.MainPage;
            if (mainPage != null && mainPage is ContentPage contentPage)
            {
                if (contentPage.Content is Layout layout)
                {
                    layout.Add(_timePickerDialog);
                }
            }
        }

        _timePickerDialog.Show();
    }

    private void OnTimeSelected(object sender, string selectedTime)
    {
        var viewModel = BindingContext as BoxingTimerVM;
        if (viewModel?.CurrentUISettings != null)
        {
            // Update the setting
            viewModel.CurrentUISettings.RoundTime = selectedTime;

            viewModel.RefreshTimerDisplay();
        }
    }

    private void OnTimePickerCancelled(object sender, EventArgs e)
    {
        // Dialog was cancelled, nothing to do
    }
}