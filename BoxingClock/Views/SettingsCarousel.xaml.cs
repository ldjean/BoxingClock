using BoxingClock.Models;
using BoxingClock.ViewModels;
using System.Collections.ObjectModel;

namespace BoxingClock.Views
{
    public partial class SettingsCarousel : ContentView
    {
        public SettingsCarousel()
        {
            InitializeComponent();
            BindingContext = App.MainViewModel;
            InitializePages();
        }

        private void InitializePages()
        {
            if (BindingContext is BoxingTimerVM viewModel)
            {
                var pages = new ObservableCollection<CarouselPage>
                {
                    new CarouselPage { Title = "Timer Options", PageType = typeof(TimerSettingsPage) },
                    new CarouselPage { Title = "Presets", PageType = typeof(TimerPresetsPage) }
                };

                viewModel.Pages = pages;
                viewModel.CurrentPage = pages.FirstOrDefault();
            }
        }
    }
}