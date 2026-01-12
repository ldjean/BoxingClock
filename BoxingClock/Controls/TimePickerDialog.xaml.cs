using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
namespace BoxingClock.Controls;

public partial class TimePickerDialog : ContentView
{
    public event EventHandler<string> TimeSelected;
    public event EventHandler Cancelled;

    public TimePickerDialog()
    {
        InitializeComponent();
        InitializePickers();
        UpdateTimeDisplay();
    }

    private void InitializePickers()
    {
        // Initialize minutes (0-30)
        for (int i = 0; i <= 30; i++)
        {
            minutePicker.Items.Add(i.ToString());
        }

        // Initialize seconds (0-55 in 5-second increments)
        for (int i = 0; i < 60; i += 5)
        {
            secondPicker.Items.Add(i.ToString("00"));
        }

        // Set default selections
        minutePicker.SelectedIndex = 3; // 3 minutes
        secondPicker.SelectedIndex = 0; // 00 seconds

        // Handle selection changes
        minutePicker.SelectedIndexChanged += OnPickerSelectionChanged;
        secondPicker.SelectedIndexChanged += OnPickerSelectionChanged;
    }

    public void SetInitialTime(string time)
    {
        if (!string.IsNullOrEmpty(time) && time.Contains(":"))
        {
            var parts = time.Split(':');
            if (parts.Length == 2)
            {
                // Set minutes
                var minuteIndex = minutePicker.Items.IndexOf(parts[0]);
                if (minuteIndex >= 0)
                    minutePicker.SelectedIndex = minuteIndex;

                // Set seconds
                var secondIndex = secondPicker.Items.IndexOf(parts[1]);
                if (secondIndex >= 0)
                    secondPicker.SelectedIndex = secondIndex;
            }
        }
        else
        {
            // Default to 3:00 if format is invalid
            minutePicker.SelectedIndex = 3;
            secondPicker.SelectedIndex = 0;
        }
    }

    private void OnPickerSelectionChanged(object sender, EventArgs e)
    {
        // If 30 minutes is selected, force seconds to 00
        if (minutePicker.SelectedItem?.ToString() == "30")
        {
            secondPicker.SelectedIndex = 0; // 00 seconds
            secondPicker.IsEnabled = false;
        }
        else
        {
            secondPicker.IsEnabled = true;
        }

        UpdateTimeDisplay();
    }

    private void UpdateTimeDisplay()
    {
        var minutes = minutePicker.SelectedItem?.ToString() ?? "0";
        var seconds = secondPicker.SelectedItem?.ToString() ?? "00";
        timeDisplay.Text = $"{minutes}:{seconds}";
    }

    private void OnOkClicked(object sender, EventArgs e)
    {
        var minutes = minutePicker.SelectedItem?.ToString() ?? "0";
        var seconds = secondPicker.SelectedItem?.ToString() ?? "00";
        var selectedTime = $"{minutes}:{seconds}";

        TimeSelected?.Invoke(this, selectedTime);
        Hide();
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        Cancelled?.Invoke(this, EventArgs.Empty);
        Hide();
    }

    public void Show()
    {
        this.IsVisible = true;
        this.Opacity = 0;
        this.FadeTo(1, 200);
    }

    public void Hide()
    {
        this.FadeTo(0, 200, Easing.Linear).ContinueWith(t =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                this.IsVisible = false;
            });
        });
    }
}