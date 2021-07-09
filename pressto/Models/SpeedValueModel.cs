namespace press_double.Models
{
    public class SpeedValueModel
    {
        private int _multiplier;

        private int _speedvalue;

        public SpeedValueModel(int val)
        {
            this.SpeedValue = val;
            this.Multiplier = 100;
        }

        public int Multiplier
        {
            get
            {
                return this._multiplier;
            }

            set
            {
                if (value >= 0)
                {
                    this._multiplier = value;
                }
            }
        }

        public int SpeedValue
        {
            get
            {
                return this._speedvalue;
            }

            set
            {
                if (value >= 0)
                {
                    this._speedvalue = value;
                }
            }
        }
    }
}