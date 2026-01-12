using BoxingClock.ViewModels;

namespace BoxingClock.Views;

public partial class TimerPresetsPage : ContentView
{
    private BoxingTimerVM ViewModel => (BoxingTimerVM)BindingContext;
    public TimerPresetsPage()
	{
		InitializeComponent();
	}

    private void PresetItem_Selected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Models.TimerConfig selectedPreset)
        {
            // Use the ViewModel command
            ViewModel.SelectPresetCommand.Execute(selectedPreset);

            // Clear selection
            presetCollectionView.SelectedItem = null;
        }
    }
}