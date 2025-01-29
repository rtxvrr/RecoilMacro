namespace RecoilMacro.Model
{
    public class Preset
    {
        #region Private Fields

        private string _name;
        private double _intensity;

        #endregion

        #region Properties

        public string Name
        {
            get => _name;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new System.ArgumentException("Name cannot be null or whitespace.");
                }
                _name = value;
            }
        }

        public double Intensity
        {
            get => _intensity;
            set
            {
                if (value < 0)
                {
                    throw new System.ArgumentOutOfRangeException(nameof(value));
                }
                _intensity = value;
            }
        }

        public bool BothButtonsRequired { get; set; }
        public bool VirtualDriverMethod { get; set; }
        public double IncrementalStep { get; set; }

        #endregion
    }
}
