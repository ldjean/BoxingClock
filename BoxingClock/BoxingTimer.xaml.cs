using BoxingClock.ViewModels;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Layouts;

namespace BoxingClock
{
    public partial class BoxingTimer : ContentPage
    {
        private BoxingTimerVM ViewModel => (BoxingTimerVM)BindingContext;

        public BoxingTimer()
        {
            InitializeComponent();
            BindingContext = App.MainViewModel;

            // Set up data bindings for timer display
            timerDisplay.SetBinding(Label.TextProperty, nameof(ViewModel.CountDownDisplay));
            roundCount.SetBinding(Label.TextProperty, nameof(ViewModel.RoundStatusText));
            tick.SetBinding(Label.TextProperty, nameof(ViewModel.ReadyCount));

            // Subscribe to timer events for animations
            App.TimerService.ReadyCountdownChanged += OnReadyCountdownChanged;
            App.TimerService.ReadyCountdownFinished += OnReadyCountdownFinished;
            App.TimerService.PropertyChanged += TimerService_PropertyChanged;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Initial fade-in animation
            foreach (View view in pagelayout.Children)
            {
                if (view != settingsCarousel) // Don't fade in the overlay initially
                {
                    view.Opacity = 0;
                }
            }

            foreach (View view in pagelayout.Children)
            {
                if (view != settingsCarousel)
                {
                    await Task.WhenAny(view.FadeTo(1, 1000, Easing.CubicInOut), Task.Delay(100));
                }
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            App.TimerService.Stop();
            App.TimerService.Reset();
            App.TimerService.PropertyChanged -= TimerService_PropertyChanged;
            App.TimerService.ReadyCountdownChanged -= OnReadyCountdownChanged;
            App.TimerService.ReadyCountdownFinished -= OnReadyCountdownFinished;
        }

        private void TimerService_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                // Handle gear animation based on running state
                if (e.PropertyName == nameof(App.TimerService.IsRunning))
                {
                    if (App.TimerService.IsRunning)
                    {
                        ShrinkGear();
                    }
                    else
                    {
                        ResetGear();
                    }
                }
                // Don't update UI during ready countdown - we're handling that separately
                if (App.TimerService.IsInReadyCountdown) return;

                // Update the display when countdown changes
                if (e.PropertyName == nameof(App.TimerService.CountDown))
                {
                    // The binding should handle this, but we can add custom logic here if needed
                }
            });
        }

        private void Handle_SizeChanged(object sender, EventArgs e)
        {
            UpdateUIForOrientation();
        }

        private void UpdateUIForOrientation()
        {
            // Only update if overlay is NOT visible
            if (ViewModel.IsOverlayVisible) return;

            // Store current timer state for reference
            bool isRunning = App.TimerService.IsRunning;
            bool isInReadyCountdown = App.TimerService.IsInReadyCountdown;

            // portrait
            if (Width < Height)
            {
                timerDisplay.ScaleTo(1, 1000);

                // Add timerDisplay directly to timerlayout at center
                AbsoluteLayout.SetLayoutBounds(countLayout,
                    new Rect(0.5, 0.5, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
                AbsoluteLayout.SetLayoutFlags(countLayout,
                    AbsoluteLayoutFlags.PositionProportional);

                // In portrait, show everything and restore enabled states
                foreach (View view in timerlayout.Children)
                {
                    // Skip overlay
                    if (view == settingsCarousel) continue;

                    // Show roundCount in portrait
                    if (view == countLayout)
                    {
                        view.Opacity = 1;
                        view.IsEnabled = true;
                        roundCount.FadeTo(1, 500);
                        continue;
                    }

                    // For timerButtons: Let ViewModel control visibility, ensure it's enabled
                    if (view == timerButtons)
                    {
                        view.Opacity = 1;
                        view.IsEnabled = true; // Always enabled in portrait
                        continue;
                    }

                    // For gear: Restore ViewModel's enabled state
                    if (view == gear)
                    {
                        view.Opacity = 0.5;
                        continue;
                    }

                    // For bsblogo: Always visible and enabled in portrait
                    if (view == bsblogo)
                    {
                        view.Opacity = 1;
                        view.IsEnabled = true;
                        continue;
                    }

                    // For logos: Show them and enable
                    if (view == topLeftLogo || view == topRightLogo ||
                        view == bottomLeftLogo || view == bottomRightLogo)
                    {
                        view.Opacity = 1;
                        view.IsEnabled = true;
                        continue;
                    }

                    // For all other elements
                    view.Opacity = 1;
                    view.IsEnabled = true;
                }
            }
            // landscape
            else
            {
                timerDisplay.ScaleTo(3, 1000);

                // Position timerDisplay at top center
                AbsoluteLayout.SetLayoutBounds(countLayout,
                    new Rect(0.5, 0, AbsoluteLayout.AutoSize, AbsoluteLayout.AutoSize));
                AbsoluteLayout.SetLayoutFlags(countLayout,
                    AbsoluteLayoutFlags.PositionProportional);

                // In landscape, show only timer display and logos, disable everything else
                foreach (View view in timerlayout.Children)
                {
                    // Skip overlay
                    if (view == settingsCarousel) continue;

                    // Keep timer display visible and enabled
                    if (view == countLayout || view == tick)
                    {
                        view.Opacity = 1;
                        view.IsEnabled = true;
                        if (view == countLayout)
                        {
                            roundCount.Opacity = 0; // Hide round count in landscape
                        }
                        continue;
                    }

                    // Keep logos visible and enabled in landscape
                    if (view == topLeftLogo || view == topRightLogo ||
                        view == bottomLeftLogo || view == bottomRightLogo)
                    {
                        view.Opacity = 1;
                        view.IsEnabled = true;
                        continue;
                    }

                    // For timerButtons: Hide AND disable in landscape
                    if (view == timerButtons)
                    {
                        view.Opacity = 0;
                        view.IsEnabled = false; // Important: Disable to prevent taps
                        continue;
                    }

                    // For gear: Hide AND disable in landscape
                    if (view == gear)
                    {
                        view.Opacity = 0;
                        //view.IsEnabled = false;
                        continue;
                    }

                    // For bsblogo: Hide AND disable in landscape
                    if (view == bsblogo)
                    {
                        view.Opacity = 0;
                        view.IsEnabled = false;
                        continue;
                    }

                    // For all other elements: hide AND disable
                    view.Opacity = 0;
                    view.IsEnabled = false;
                }
            }
        }

        // Animation methods for ready countdown
        private async void OnReadyCountdownChanged(int count)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                // Show the tick label and hide the main timer during ready countdown
                if (!tick.IsVisible)
                {
                    tick.IsVisible = true;
                    countLayout.IsVisible = false;
                    timerButtons.IsVisible = false;

                    ShrinkGear();
                }

                // The tick label text is already bound to ReadyCount
                //SoundManager.PlayTick();

                tick.Opacity = 1;
                tick.FadeTo(0.10, 1000);
                await tick.ScaleTo(2, 200, Easing.SinOut);
                await tick.ScaleTo(1, 750, Easing.SinOut);
            });
        }

        private void OnReadyCountdownFinished()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                // Show main timer UI
                countLayout.IsVisible = true;
                tick.IsVisible = false;

                // Update UI state for portrait mode
                if (Width < Height)
                {
                    timerButtons.IsVisible = true;
                }
            });
        }

        // Social media and UI interaction methods
        async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            await gear.ScaleTo(.8, 100);
            await gear.ScaleTo(1, 30);

            // Toggle overlay visibility through ViewModel
            ViewModel.IsOverlayVisible = !ViewModel.IsOverlayVisible;

            //if (ViewModel.IsOverlayVisible)
            //{
            //    //countLayout.IsVisible = false;
            //    // The overlay will show settings by default (as per ViewModel)
            //}
            //else
            //{
            //    countLayout.IsVisible = true;
            //}
        }

        private async void BSBtapped(object sender, EventArgs e)
        {
            await bsblogo.ScaleTo(.95, 70);
            bsblogo.ScaleTo(1, 30);

            if (icons.IsEnabled == true)
            {
                icons.IsEnabled = false;
                icons.TranslateTo(0, -10);
                icons.FadeTo(0);
            }
            else
            {
                icons.IsEnabled = true;
                await Task.WhenAny<bool>(icons.TranslateTo(0, 10, 500, Easing.BounceOut), icons.FadeTo(1));
            }
        }

        private async void igTapped(object sender, EventArgs e)
        {
            try
            {
                await Launcher.Default.OpenAsync("https://www.instagram.com/blackstallionboxing_plus/");
            }
            catch (Exception ex)
            {
                // Handle exception
            }

            icons.IsEnabled = false;
            await icons.FadeTo(0);
            await igLogo.ScaleTo(.95, 70);
            igLogo.ScaleTo(1, 30);
        }

        private async void fbTapped(object sender, EventArgs e)
        {
            try
            {
                await Launcher.Default.OpenAsync("https://www.facebook.com/Blackstallionboxingplus/");
            }
            catch (Exception ex)
            {
                // Handle exception
            }

            icons.IsEnabled = false;
            icons.FadeTo(0);
            fbLogo.ScaleTo(.95, 70);
            fbLogo.ScaleTo(1, 30);
        }

        private async void webTapped(object sender, EventArgs e)
        {
            try
            {
                await Launcher.Default.OpenAsync("http://blackstallionboxingplus.com/take-a-tour/");
            }
            catch (Exception ex)
            {
                // Handle exception
            }

            icons.IsEnabled = false;
            await webLogo.ScaleTo(.95, 70);
            await webLogo.ScaleTo(1, 30);
        }

        private async void ShrinkGear()
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                gear.ScaleTo(0.6, 1000, Easing.SpringOut);
                gear.Opacity = 0.5; // Make it look disabled
            });
        }

        private async void ResetGear()
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                gear.Opacity = 1; // Restore full opacity
                await gear.ScaleTo(1, 1000, Easing.SpringOut);
            });
        }



    }
}