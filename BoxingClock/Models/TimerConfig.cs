using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace BoxingClock.Models
{
    public class TimerConfig : INotifyPropertyChanged
    {
        private string _roundTime;
        public string RoundTime
        {
            get => _roundTime;
            set => SetProperty(ref _roundTime, value);
        }

        private string _breakTime;
        public string BreakTime
        {
            get => _breakTime;
            set => SetProperty(ref _breakTime, value);
        }

        private string _readyTime;
        public string ReadyTime
        {
            get => _readyTime;
            set => SetProperty(ref _readyTime, value);
        }

        private string _intervalNotification;
        public string IntervalNotification
        {
            get => _intervalNotification;
            set => SetProperty(ref _intervalNotification, value);
        }

        private string _presetName;
        public string PresetName
        {
            get => _presetName;
            set => SetProperty(ref _presetName, value);
        }

        private string _numberOfRounds;
        public string NumberOfRounds
        {
            get => _numberOfRounds;
            set => SetProperty(ref _numberOfRounds, value);
        }

        // Constructor with default values
        public TimerConfig()
        {
            RoundTime = "3:00";
            BreakTime = "1:00";
            ReadyTime = "5";
            IntervalNotification = "Off";
            PresetName = "Default";
            NumberOfRounds = "∞";
        }

        // Clone method for creating copies
        public TimerConfig Clone()
        {
            return new TimerConfig
            {
                RoundTime = this.RoundTime,
                BreakTime = this.BreakTime,
                ReadyTime = this.ReadyTime,
                IntervalNotification = this.IntervalNotification,
                PresetName = this.PresetName,
                NumberOfRounds = this.NumberOfRounds
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return $"RoundTime: {RoundTime}, BreakTime: {BreakTime}, ReadyTime: {ReadyTime}, " +
                   $"IntervalNotification: {IntervalNotification}, PresetName: {PresetName}, " +
                   $"NumberOfRounds: {NumberOfRounds}";
        }
    }
}