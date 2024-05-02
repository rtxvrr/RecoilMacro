using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecoilMacro.Model
{
    public class Preset
    {
        public string Name { get; set; }
        public double Intensity { get; set; }
        public bool BothButtonsRequired { get; set; }
        public bool WinApiMethod { get; set; }
    }
}
