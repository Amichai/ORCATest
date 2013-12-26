using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using ThreeByte.MoMath.Robots.Models;
using ThreeByte.MoMath.Robots.Util;

namespace OrcaTest {
    public class RobotModel : INotifyPropertyChanged {
        public RobotModel(double x, double y) {
            this.X = x;
            this.Y = y;
            this.ID = counter++;
            this.state = new RobotState(this.ID);
            this.CurrentState = State.normal;
        }

        private double _Heading;
        public double Heading {
            get { return _Heading; }
            set {
                _Heading = value;
                NotifyPropertyChanged();
            }
        }

        private static int counter = 0;

        private double _Y;
        public double Y {
            get { return _Y; }
            set {
                _Y = value;
                NotifyPropertyChanged();
            }
        }

        private double _X;
        public double X {
            get { return _X; }
            set {
                _X = value;
                NotifyPropertyChanged();
            }
        }

        private RobotState state;

        private static Random rand = new Random();

        public int ID { get; set; }

        private DateTime lastMove = DateTime.Now;
        private Vec2 targetVelocity;

        public void CalculateNextMove() {
            var pos = new Vec2(this.X, this.Y);
            var dir = pos - this.Target;
            if (dir.Mag() < 2) {
                this.targetVelocity = new Vec2(0, 0);
            } else {
                var vel = Vec2.FromAngleAndMagnitude(dir.AngleFromXAxis(), -20);
                if (this.state.LatestReport != null) {
                    bool directCollision;
                    this.targetVelocity = validator.Validate(vel, this.state.LatestReport, out directCollision);
                    if (directCollision) {
                        this.CurrentState = State.collided;
                    } else {
                        this.CurrentState = State.normal;
                    }
                } else {
                    this.targetVelocity = vel;
                }
            }
        }

        public static OrcaValidator validator = new OrcaValidator();

        public void Move() {
            double dt = (DateTime.Now - lastMove).TotalSeconds;
            Vec2 dr = new Vec2(targetVelocity.X * dt, targetVelocity.Y * dt);
            this.Heading = dr.AngleFromXAxis();
            this.X += dr.X;
            this.Y += dr.Y;

            var report = new RobotReport() {
                Yaw = Heading,
                RobotID = this.ID,
                Position = new ThreeByte.MoMath.Robots.Util.Vec2(this.X, this.Y),
            };
            state.PushReport(report);
            this.lastMove = DateTime.Now;
        }

        public static IEnumerable<RobotModel> Generate(int count, double radius, double xOffest, double yOffset) {
            var dtheta = Math.PI * 2 / count;
            for (int i = 0; i < count; i++) {
                double x, y;
                getPos(radius, xOffest, yOffset, dtheta, i, out x, out y);
                var model = new RobotModel(x, y);
                double targetX, targetY;
                getPos(radius, xOffest, yOffset, dtheta, 
                    (i + count / 2) % count, out targetX, out targetY);
                model.Target = new Vec2(targetX, targetY);
                yield return model;
            }
        }

        public enum State { collided, converged, normal };


        private State _CurrentState;
        public State CurrentState {
            get { return _CurrentState; }
            set {
                if (_CurrentState != value) {
                    _CurrentState = value;
                    NotifyPropertyChanged("CurrentState");
                    NotifyPropertyChanged("Brush");

                }
            }
        }

        public Brush Brush {
            get {
                switch (this.CurrentState) {
                    case RobotModel.State.collided:
                        return Brushes.Red;
                    case RobotModel.State.converged:
                        return Brushes.Green;
                    case RobotModel.State.normal:
                        return Brushes.Blue;
                    default:
                        throw new Exception();
                }
            }
        }

        private static void getPos(double radius, double xOffest, double yOffset, double dtheta, int i, out double x, out double y) {
            x = radius * Math.Cos(i * dtheta) + xOffest;
            y = radius * Math.Sin(i * dtheta) + yOffset;
        }

        public Vec2 Target { get; set; }

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "") {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion INotifyPropertyChanged Implementation
    }
}
