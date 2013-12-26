using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using ThreeByte.MoMath.Robots.Util;

namespace OrcaTest {
    public static class Util {
        public static string PropertyString<T>(Expression<Func<T>> exp) {
            if (exp.Body.NodeType == ExpressionType.MemberAccess) {
                return (exp.Body as MemberExpression).Member.Name;
            }
            throw new Exception();
        }

        public static double StandardDev(this List<double> range) {
            double ret = 0;
            if (range.Any()) {
                //Compute the Average      
                double avg = range.Average();
                //Perform the Sum of (value-avg)_2_2      
                double sum = range.Sum(d => Math.Pow(d - avg, 2));
                //Put it all together      
                ret = Math.Sqrt((sum) / (range.Count() - 1));
            }
            return ret;
        }

        public static double NormalizeAngle(this double angle, AngleType angleType = AngleType.Radians) {
            switch (angleType) {
                case AngleType.Degrees:
                    return NormalizeAngleDegrees(angle);
                case AngleType.Radians:
                default:
                    return NormalizeAngle(angle);
            }
        }

        /// <summary>
        /// Normalizes an angle to fall within the range -PI, PI degrees
        /// </summary>
        /// <param name="angle"></param>
        /// <returns></returns>
        private static double NormalizeAngle(this double angle) {
            while (angle > TWO_PI / 2) {
                angle -= TWO_PI;
            }

            while (angle < -TWO_PI / 2) {
                angle += TWO_PI;
            }
            return angle;
        }

        private static double NormalizeAngleDegrees(this double angle) {
            while (angle > 180) {
                angle -= 360;
            }

            while (angle < -180) {
                angle += 360;
            }

            return angle;
        }

        public static double Sqrd(this double val) {
            return val * val;
        }

        public const double TWO_PI = Math.PI * 2;
        public const double HALF_PI = Math.PI / 2;

        private static readonly ThreadLocal<Random> rand = new ThreadLocal<Random>(() => new Random(Guid.NewGuid().GetHashCode()));

        private static Brush[] brushes = { Brushes.Red, Brushes.Green, Brushes.Blue, Brushes.Black, Brushes.Orange, Brushes.Purple };
        private static int brushIdx;
        public static Brush GetRandomBrush() {
            return brushes[brushIdx++ % brushes.Count()];
        }


        public static double RandomGaussian(double mean, double stdDev) {
            double u1 = rand.Value.NextDouble(); //these are uniform(0,1) random doubles
            double u2 = rand.Value.NextDouble();
            double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                         Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
            double randNormal =
                         mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)
            return randNormal;
        }


        public static double DistanceToLine(Vec2 endPoint1, Vec2 endPoint2, Vec2 inspectionPt, bool absValue = true) {
            if (endPoint1.Dist(endPoint2).Equals(0)) {
                return inspectionPt.Dist(endPoint1);
            }
            double n = (endPoint2.X - endPoint1.X) * (endPoint1.Y - inspectionPt.Y) - (endPoint1.X - inspectionPt.X) * (endPoint2.Y - endPoint1.Y);
            double d = Math.Sqrt(Math.Pow(endPoint2.X - endPoint1.X, 2) + Math.Pow(endPoint2.Y - endPoint1.Y, 2));
            double distanceToLine = n / d;
            if (absValue) {
                distanceToLine = Math.Abs(distanceToLine);
            }
            double distanceToPt1 = inspectionPt.Dist(endPoint1);
            double distanceToPt2 = inspectionPt.Dist(endPoint2);
            double distanceBetweenEndPoints = endPoint1.Dist(endPoint2);
            if (distanceToPt1 > distanceBetweenEndPoints || distanceToPt2 > distanceBetweenEndPoints) {
                return (distanceToLine != 0 ? Math.Sign(distanceToLine) : 1) * Math.Min(distanceToPt1, distanceToPt2);
            }
            return distanceToLine;
        }

        //  from http://www.gamedev.net/topic/444154-closest-point-on-a-line/
        public static Vec2 ClosestPointOnLine(Vec2 endPoint1, Vec2 endPoint2, Vec2 inspectionPt) {
            Vec2 AP = inspectionPt - endPoint1;
            Vec2 AB = endPoint2 - endPoint1;
            double ab2 = Math.Pow(AB.X, 2) + Math.Pow(AB.Y, 2);
            double ap_ab = AP.X * AB.X + AP.Y * AB.Y;
            double t = ap_ab / ab2;
            if (t < 0) {
                t = 0;
            } else if (t > 1) {
                t = 1;
            }
            return endPoint1 + AB * t;

        }

        public static double Slope(Vec2 pt1, Vec2 pt2) {
            return NormalizeAngle((pt2.Y - pt1.Y) / (pt2.X - pt1.X));
        }

        public static double SlopePrime(Vec2 pt1, Vec2 pt2) {
            return NormalizeAngle(-((pt2.X - pt1.X) / (pt2.Y - pt1.Y)));
        }


        /// <summary>
        /// Finds the input value of a function such that the output is zero with the range of min and max
        /// </summary>
        /// <param name="function">The function to zero</param>
        /// <param name="min">The lower bound of the "zero search"</param>
        /// <param name="max">The upper bound of the "zero search"</param>
        /// <param name="counter">The amount of iterations to convergence</param>
        /// <param name="eps">Desired precision</param>
        /// <returns></returns>
        public static double Zero(this Func<double, double> function, double min, double max, out int counter, double eps = .001) {
            double lowerBound = min,
                upperBound = max;
            counter = 0;
            double range = long.MaxValue;
            double tryIndex = long.MinValue;
            double tryEval = double.MinValue;
            while (Math.Abs(tryEval) > eps /*&& range > 1.0e-9 */&& counter < 10000) {
                counter++;
                range = upperBound - lowerBound;
                double maxEval = function(upperBound);
                double minEval = function(lowerBound);
                bool signMax = (maxEval > 0);
                bool signMin = (minEval > 0);
                if (signMax == signMin) {
                    throw new Exception("Failed to converge");
                }
                tryIndex = lowerBound + (range / 2);
                tryEval = function(tryIndex);
                bool trySign = tryEval > 0;
                if (trySign == signMax) {
                    upperBound = tryIndex;
                } else lowerBound = tryIndex;
            }
            return tryIndex;
        }

        public static string Formatter(this string input, params object[] replace) {
            var split = input.Split(new string[] { "{}" }, StringSplitOptions.None);
            var outputString = "";
            for (int i = 0; i < split.Count() - 1; i++) {

                string replaceWith;
                if (i < replace.Count()) {
                    replaceWith = replace[i].ToString();
                } else {
                    replaceWith = replace.Last().ToString();
                }
                outputString += split[i] + replaceWith;
            }
            outputString += split.Last();
            for (int i = split.Count() - 1; i < replace.Count(); i++) {
                outputString += replace[i];
            }

            return outputString;
        }
    }

    public static class StringExtensions {
        public static bool EndsWithAny(this string value, List<string> acceptableValues) {
            bool endsWith = false;

            acceptableValues.ForEach(v => {
                if (value.EndsWith(v)) {
                    endsWith = true;
                }
            });

            return endsWith;
        }
    }


    public enum AngleType {
        Degrees,
        Radians
    }
}
