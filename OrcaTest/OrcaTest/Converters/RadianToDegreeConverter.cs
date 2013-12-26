using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace OrcaTest.Converters {
    public class RadianToDegreeConverter : IValueConverter {
        private bool _normalize;

        public bool Normalize {
            get { return _normalize; }
            set { _normalize = value; }
        }

        Random rand = new Random();

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            double offset = 0;
            if (parameter != null) {
                offset = double.Parse(parameter.ToString());
            }

            double radians = double.Parse(value.ToString());
            double degrees = radians * (180.0 / Math.PI);

            return Normalize ? (degrees + offset).NormalizeAngle(AngleType.Degrees) : degrees + offset;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            double degrees = double.Parse(value.ToString());

            return degrees * (Math.PI / 180.0);
        }
    }
}
