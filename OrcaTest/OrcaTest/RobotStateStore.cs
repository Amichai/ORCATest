using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeByte.MoMath.Robots.Models;
using ThreeByte.MoMath.Robots.Util;

namespace OrcaTest {
    public class RobotStateStore {
        public RobotStateStore() {
            RobotStates = new Dictionary<int, RobotState>();
        }

        private Dictionary<int, RobotState> _robotStates;
        private readonly object _syncRoot = new object();

        //TODO: make this accessor clear that it returns a shallow clone of the dictionary
        public Dictionary<int, RobotState> RobotStates {
            get {
                lock (_syncRoot) {
                    return new Dictionary<int, RobotState>(_robotStates);
                }
            }
            private set {
                lock (_syncRoot) {
                    _robotStates = value;
                }
            }
        }

        public double Left {
            get {
                lock (_syncRoot) {
                    return this.RobotStates.Min(i => i.Value.Position.X);
                }
            }
        }

        public double Right {
            get {
                lock (_syncRoot) {
                    return this.RobotStates.Max(i => i.Value.Position.X);
                }
            }
        }

        public double Bottom {
            get {
                lock (_syncRoot) {
                    return this.RobotStates.Max(i => i.Value.Position.Y);
                }
            }
        }

        public double Top {
            get {
                lock (_syncRoot) {
                    return this.RobotStates.Min(i => i.Value.Position.Y);
                }
            }
        }


        public List<RobotState> GetList() {
            lock (_syncRoot) {
                return RobotStates.Values.ToList();
            }
        }

        public RobotState this[int idx] {
            get {
                return RobotStates[idx];
            }
        }

        /// <summary>
        /// Pushes a robot report to a Dictionary of robotStates.
        /// </summary>
        /// <param name="report"></param>
        /// <returns>boolean representing if robot is already represented in this RobotStateStore.</returns>
        public bool Update(RobotReport report) {
            lock (_syncRoot) {
                if (_robotStates.ContainsKey(report.RobotID)) {
                    _robotStates[report.RobotID].PushReport(report);
                    return true;
                } else {
                    _robotStates[report.RobotID] = new RobotState(report.RobotID) { LatestReport = report, Acceleration = new Vec2(), Velocity = new Vec2() };
                    return false;
                }

            }
        }

        public RobotState Retrieve(int id) {
            lock (_syncRoot) {
                if (_robotStates.ContainsKey(id)) {
                    return _robotStates[id];
                } else {
                    return null;
                }

            }
        }
    }
}
