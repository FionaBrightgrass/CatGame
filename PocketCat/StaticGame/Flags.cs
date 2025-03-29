using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PocketCat.StaticGame.Stats;

namespace PocketCat.StaticGame
{
    public static class Flags
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
                    Console.WriteLine($"IsSleeping state changed to: {_isSleeping}");
                }
            }
        }

        private static bool _wantsPets;
        public static bool WantsPets
        {
            get => _wantsPets;
            set
            {
                if (_wantsPets != value)
                {
                    _wantsPets = value;
                    Console.WriteLine($"WantsPets state changed to: {_wantsPets}");
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
                    Console.WriteLine($"WantsPets state changed to: {_uiHidden}");
                }
            }
        }
    }
}
