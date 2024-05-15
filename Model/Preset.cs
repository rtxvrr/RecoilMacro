using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecoilMacro.Model
{
    public class Preset
    {
        private string name;
        private double intensity;

        public string Name
        {
            get => name;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Name cannot be null or whitespace.");
                name = value;
            }
        }

        public double Intensity
        {
            get => intensity;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException(nameof(value), "Intensity must be > 0");
                intensity = value;
            }
        }

        public bool BothButtonsRequired { get; set; }
        public bool VirtualDriverMethod { get; set; }
        public double IncrementalStep { get; set; }
    }

}
