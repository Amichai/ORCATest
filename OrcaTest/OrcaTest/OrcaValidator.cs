using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThreeByte.MoMath.Robots.Models;
using ThreeByte.MoMath.Robots.Util;

namespace OrcaTest {
    public class OrcaValidator {

        public OrcaValidator() {
            //this.TerrainMap = new TerrainMap(width, height);
            this.robotstateStore = new RobotStateStore();
            //this.robotActions = new Dictionary<int, RobotAction>();
        }

        public void Reset() {
            this.robotstateStore = new RobotStateStore();
        }

        public static double RobotRadius = 8;

        class velocityObstacle {
            private Bitmap bitmap;
            Vec2 offsetConeCenter;
            Vec2 originalConeCenter;
            double radius;
            public Vec2 finiteTimeCutoffVal;
            private bool ptIsInsideCircle(Vec2 pos, Vec2 center, double r) {
                return (pos.X - center.X).Sqrd() + (pos.Y - center.Y).Sqrd() < r.Sqrd();
            }
            static Dictionary<int, Dictionary<int, Bitmap>> lastCone = new Dictionary<int, Dictionary<int, Bitmap>>();

            public bool IsInsideCone(Vec2 pt, int inspectionRobotID, double drawingScale, int drawingOffset, bool printOnCollision, out bool directCollision) {
                double internalAngle = (this.offsetConeCenter - this.velocityOffset).AngleBetween(this.cornerA - this.velocityOffset);
                double inspectionAngle = (this.offsetConeCenter - this.velocityOffset).AngleBetween(pt - this.velocityOffset);

                var angleRange = Math.Abs(inspectionAngle) < Math.Abs(internalAngle);
                directCollision = false;
                return angleRange; 
            }


            private Bitmap drawCone(Vec2 pt, double drawingScale, int drawingOffset) {
                int side = 500;
                this.bitmap = new Bitmap(side, side);
                Graphics g = Graphics.FromImage(bitmap);
                Pen red = new Pen(Color.Red, 2f);
                Pen blue = new Pen(Color.Blue, 2f);
                Pen green = new Pen(Color.Green, 2f);

                ///Original circle
                Rectangle r = new Rectangle((int)Math.Round((this.offsetConeCenter.X - this.radius) * drawingScale) + drawingOffset,
                    (int)Math.Round((this.offsetConeCenter.Y - radius) * drawingScale) + drawingOffset,
                    (int)Math.Round(this.radius * 2 * drawingScale), (int)Math.Round(this.radius * 2 * drawingScale));
                g.DrawEllipse(red, r);
                if (!double.IsNaN(this.cornerA.X)) {
                    var pt1 = new Point((int)Math.Round(this.velocityOffset.X * drawingScale) + drawingOffset, (int)Math.Round(this.velocityOffset.Y * drawingScale) + drawingOffset);
                    var pt2 = new Point((int)Math.Round(this.cornerA.X * drawingScale) + drawingOffset, (int)Math.Round(this.cornerA.Y * drawingScale) + drawingOffset);
                    g.DrawLine(blue, pt1, pt2);

                    var pt3 = new Point((int)Math.Round(this.velocityOffset.X * drawingScale) + drawingOffset, (int)Math.Round(this.velocityOffset.Y * drawingScale) + drawingOffset);
                    var pt4 = new Point((int)Math.Round(this.cornerB.X * drawingScale) + drawingOffset, (int)Math.Round(this.cornerB.Y * drawingScale) + drawingOffset);
                    g.DrawLine(blue, pt3, pt4);
                }
                Rectangle target = new Rectangle((int)Math.Round(pt.X * drawingScale) + drawingOffset,
                    (int)Math.Round(pt.Y * drawingScale) + drawingOffset,
                    2, 2);
                g.DrawRectangle(blue, target);


                Rectangle target2 = new Rectangle((int)Math.Round(0 * drawingScale) + drawingOffset,
                    (int)Math.Round(0 * drawingScale) + drawingOffset,
                    2, 2);
                g.DrawRectangle(green, target2);

                return bitmap;
            }

            private static int counter = 0;

            Vec2 cornerA, cornerB;

            Vec2 velocityOffset;
            int robotId;

            double internalAngle;

            private void getTangentPoints(out Vec2 p1, out Vec2 p2) {
                double a = this.radius;
                double c = this.originalConeCenter.Mag();
                double b = Math.Sqrt(c.Sqrd() - a.Sqrd());
                double a1 = Math.Asin(a / c);
                var angle1 = this.originalConeCenter.AngleFromXAxis() - a1;
                var angle2 = this.originalConeCenter.AngleFromXAxis() + a1;


                p1 = Vec2.FromAngleAndMagnitude(angle1, b);
                p2 = Vec2.FromAngleAndMagnitude(angle2, b);
            }

            public velocityObstacle(Vec2 posDiff, double radius, Vec2 velocityOffset, int robotId) {
                this.velocityOffset = velocityOffset;
                this.offsetConeCenter = posDiff + this.velocityOffset;
                this.originalConeCenter = posDiff;
                this.radius = radius;
                this.robotId = robotId;
                Vec2 p1, p2;
                getTangentPoints(out p1, out p2);

                this.cornerA = p1 + this.velocityOffset;
                this.cornerB = p2 + this.velocityOffset;

                this.internalAngle = p1.AngleBetween(p2);
            }

            internal void drawObstacle(Bitmap b, double drawingScale, int drawingOffset) {
                if (double.IsNaN(this.cornerA.X) || double.IsNaN(this.cornerA.Y) || double.IsNaN(this.cornerB.X) || double.IsNaN(this.cornerB.Y)) {
                    return;
                }
                Graphics g = Graphics.FromImage(b);
                Pen red = new Pen(Color.Red, 2f);
                Pen blue = new Pen(Color.Blue, 2f);

                ///Original circle
                Rectangle r = new Rectangle((int)Math.Round((this.offsetConeCenter.X - this.radius) * drawingScale) + drawingOffset,
                    (int)Math.Round((this.offsetConeCenter.Y - this.radius) * drawingScale) + drawingOffset,
                    (int)Math.Round(this.radius * 2 * drawingScale), (int)Math.Round(this.radius * 2 * drawingScale));
                g.DrawEllipse(red, r);
                var pt1 = new Point((int)Math.Round(this.velocityOffset.X * drawingScale) + drawingOffset, (int)Math.Round(this.velocityOffset.Y * drawingScale) + drawingOffset);
                var pt2 = new Point((int)Math.Round(this.cornerA.X * drawingScale) + drawingOffset, (int)Math.Round(this.cornerA.Y * drawingScale) + drawingOffset);
                g.DrawLine(blue, pt1, pt2);

                var pt3 = new Point((int)Math.Round(this.velocityOffset.X * drawingScale) + drawingOffset, (int)Math.Round(this.velocityOffset.Y * drawingScale) + drawingOffset);
                var pt4 = new Point((int)Math.Round(this.cornerB.X * drawingScale) + drawingOffset, (int)Math.Round(this.cornerB.Y * drawingScale) + drawingOffset);
                g.DrawLine(blue, pt3, pt4);

            }
        }

        private bool collision(Vec2 inputVelocity, List<velocityObstacle> obstacles, int robotID, bool printOnCollision, out bool directCollision) {
            directCollision = false;
            foreach (var o in obstacles) {
                if (o.IsInsideCone(inputVelocity, robotID, drawingScale, drawingOffset, printOnCollision, out directCollision)) {
                    return true;

                    //return inputVelocity / 2;
                    //return o.circleBasePoint;
                }
            }
            return false;
        }

        private Dictionary<int, Vec2> lastVelocityVector = new Dictionary<int, Vec2>();

        public Vec2 Validate(Vec2 vel, RobotReport latestReport, out bool directCollision) {
            ///check if the desired action is in the velocity obstacle
            List<velocityObstacle> obstacles = new List<velocityObstacle>();
            
            foreach (var state in robotstateStore.RobotStates) {
                if (state.Key == latestReport.RobotID) {
                    continue;
                }
                var posDiff1 = state.Value.Position - latestReport.Position;
                var radiusSum = RobotRadius + RobotRadius;

                Vec2 lastActionVec = this.lastVelocityVector[state.Key];
                obstacles.Add(new velocityObstacle(posDiff1, radiusSum,
                    lastActionVec, state.Key));


                var distance = latestReport.Position.Dist(state.Value.Position);
                //log.InfoFormat("Distance: {0} id1: {1}, id2: {2}", distance, latestReport.RobotID, state.Key);

                ///First we should check if angle of the velocity vector is within the range of the
                ///velocity obstacle. 

            }
            //foreach (var o in obstructionsMap.Attractors) {
            //    var closestPt = o.ClosestPoint(latestReport.Position);
            //    var posDiff1 = closestPt - latestReport.Position;
            //    double radius = RobotRadius;
            //    obstacles.Add(new velocityObstacle(posDiff1, radius * 4,
            //        Vec2.ZeroVector, -1));
            //}
            var adjusted = UpdateVelocityVectorFromObstacles(obstacles, vel, latestReport.RobotID, out directCollision);
            ///If we have a new velocity:
            if (adjusted == null) {
                adjusted =  vel / 100;
            }

            ///if so, calculate the closest velocity outside the obstacle
            //log.InfoFormat("Latest report: {0}", latestReport.ToString());
            //log.InfoFormat("Adjusted action: {0}", adjustedAction);
            UpdateRobots(latestReport, adjusted);

            //log.InfoFormat("Adjusted action: {0}", adjustedAction);
            return adjusted;
        }

        double drawingScale = 6.0;
        int drawingOffset = 250;
        private void drawObstructions(List<velocityObstacle> obstacles, Vec2 old, Vec2 newPt) {
            Bitmap b = new Bitmap(drawingOffset * 2, drawingOffset * 2);
            foreach (var o in obstacles) {
                o.drawObstacle(b, drawingScale, drawingOffset);
            }

            Pen red = new Pen(Color.Red, 2f);
            Pen blue = new Pen(Color.Blue, 2f);
            Graphics g = Graphics.FromImage(b);

            Rectangle target = new Rectangle((int)Math.Round(old.X * drawingScale) + drawingOffset,
                (int)Math.Round(old.Y * drawingScale) + drawingOffset,
                2, 2);
            g.DrawRectangle(blue, target);


            if (newPt != null) {
                Rectangle target2 = new Rectangle((int)Math.Round(newPt.X * drawingScale) + drawingOffset,
                    (int)Math.Round(newPt.Y * drawingScale) + drawingOffset,
                    2, 2);
                g.DrawRectangle(red, target2);
            }
            b.Save(string.Format("cones\\testing_{0}.bmp", counter++));
        }

        private static int counter = 0;
        private static Random rand = new Random();

        private Vec2 UpdateVelocityVectorFromObstacles(List<velocityObstacle> obstacles, Vec2 velocity, int id, out bool directCollision) {
            directCollision = false;
            int counter = 0;
            if (!collision(velocity, obstacles, id, true, out directCollision)) {
                return velocity;
            }
            Vec2 newVelocity = null;
            for (double dttheta = 0.01; dttheta < Math.PI * 2; dttheta += .05) {
                Vec2 inspectionPt;
                double magOffset = rand.NextDouble() * (velocity.Mag() * 2 + .1);

                if (counter % 2 == 0) {
                    inspectionPt = Vec2.FromAngleAndMagnitude(velocity.AngleFromXAxis() + dttheta, magOffset);
                } else {
                    inspectionPt = Vec2.FromAngleAndMagnitude(velocity.AngleFromXAxis() - dttheta, magOffset);
                }

                if (!collision(inspectionPt, obstacles, id, false, out directCollision)) {
                    newVelocity = inspectionPt;
                    //if (counter > 100) {
                    //    drawObstructions(obstacles, velocity, newVelocity);
                    //}
                    break;
                } else {
                    //if (counter > 100) {
                    //    drawObstructions(obstacles, velocity, newVelocity);
                    //}
                }
                counter++;
            }

            //if (newVelocity == null) {
            //    drawObstructions(obstacles, velocity, newVelocity);
            //}

            return newVelocity;
        }

        private double _ORCAScale = 4;
        public double ORCAScale {
            get { return _ORCAScale; }
            set {
                _ORCAScale = value;
            }
        }


        private RobotStateStore robotstateStore;

        private void UpdateRobots(RobotReport latestReport, Vec2 vel) {
            robotstateStore.Update(latestReport);
            this.lastVelocityVector[latestReport.RobotID] = vel;
        }

        public TerrainMap TerrainMap { get; set; }
    }
}
