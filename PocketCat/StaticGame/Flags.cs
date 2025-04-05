using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using PocketCat.StaticGame.Stats;

namespace PocketCat.StaticGame
{
    public class Flags : INotifyPropertyChanged
    {
        private static bool _isSleeping;
        public static bool IsSleeping
        {
            get => _isSleeping;
            set
            {
                if (_isSleeping != value)
                {
                    _isSleeping = value;
                    Console.WriteLine($"IsSleeping flag changed to: {_isSleeping}");
                }
            }
        }

        private static bool _wantsPets = false;
        public static bool WantsPets
        {
            get => _wantsPets;
            set
            {
                if (_wantsPets != value)
                {
                    _wantsPets = value;
                    Console.WriteLine($"WantsPets flag changed to: {_wantsPets}");
                }
            }
        }

        private static bool _uiHidden;
        public static bool UIHidden
        {
            get => _uiHidden;
            set
            {
                if (_uiHidden != value)
                {
                    _uiHidden = value;
                    Console.WriteLine($"UIHidden flag changed to: {_uiHidden}");
                }
            }
        }

        private bool _buttonLock = false;

        public bool ButtonLock
        {
            get => _buttonLock;
            set
            {
                if (_buttonLock != value)
                {
                    _buttonLock = value;
                    OnPropertyChanged(nameof(ButtonLock));
                    OnFlagsChanged(); // Notify when ButtonLock changes
                    Console.WriteLine($"ButtonLock flag changed to: {_buttonLock}");
                }
            }
        }

        private bool _blinkLock = false; //MainWindow.await RefreshBars() will unlock
        public bool BlinkLock
        {
            get => _blinkLock;
            set
            {
                if (_blinkLock != value)
                {
                    _blinkLock = value;
                    OnPropertyChanged(nameof(BlinkLock));
                    OnFlagsChanged(); // Notify when BlinkLock changes
                    Console.WriteLine($"BlinkLock flag changed to: {_blinkLock}");
                }
            }
        }

        private static bool _affectionReady = false; //MainWindow.await RefreshBars() will unlock
        public static bool AffectionReady
        {
            get => _affectionReady;
            set
            {
                if (_affectionReady != value)
                {
                    _affectionReady = value;
                    Console.WriteLine($"AffectionReady flag changed to: {_affectionReady}");
                }
                if(_affectionReady == false)
                {
                    Time.StopAffectionReadyTimer();
                    Time.StartAffectionReadyTimer();
                }
            }
        }
        private static bool _tipReady = true; //MainWindow.await RefreshBars() will unlock
        public static bool TipReady
        {
            get => _tipReady;
            set
            {
                if (_tipReady != value)
                {
                    _tipReady = value;
                    Console.WriteLine($"AffectionReady flag changed to: {_tipReady}");
                }
                if (_tipReady == false)
                {
                    Time.StopTipReadyTimer();
                    Time.StartTipReadyTimer();
                }
            }
        }
        private static bool _statDecreaseReady = false; //MainWindow.await RefreshBars() will unlock
        public static bool StatDecreaseReady
        {
            get => _statDecreaseReady;
            set
            {
                if (_statDecreaseReady != value)
                {
                    _statDecreaseReady = value;
                    Console.WriteLine($"StatDecreaseReady flag changed to: {_statDecreaseReady}");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event Action FlagsChanged;

        protected void OnFlagsChanged()
        {
            FlagsChanged?.Invoke();
        }
    }
}
