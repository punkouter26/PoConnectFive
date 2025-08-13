using System;
using System.Collections.Generic;

namespace PoConnectFive.Shared.Models
{
    // Additional classes for enhanced features

    public class PreviewEffect
    {
        public EffectType Type { get; set; }
        public (int Row, int Column) Position { get; set; }
        public string Color { get; set; } = string.Empty;
        public double Opacity { get; set; } = 1.0;
    }

    public enum EffectType
    {
        Highlight,
        Shadow,
        Glow,
        Border
    }

    public class HapticPattern
    {
        public TimeSpan Duration { get; set; }
        public double Intensity { get; set; }
        public string Pattern { get; set; } = string.Empty;
    }
}
