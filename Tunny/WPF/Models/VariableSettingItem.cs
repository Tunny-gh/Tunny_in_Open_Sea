﻿using Tunny.WPF.Common;

namespace Tunny.WPF.Models
{
    internal class VariableSettingItem
    {
        public double Low { get; set; }
        public double High { get; set; }
        public double Step { get; set; }
        public string Name { get; set; }
        private bool _isLogScale;
        public bool IsLogScale
        {
            get => _isLogScale;
            set
            {
                if (value && (Low <= 0 || High <= 0))
                {
                    TunnyMessageBox.Warn_VariableMustLargerThanZeroInLogScale();
                    return;
                }
                _isLogScale = value;
            }
        }
    }
}
