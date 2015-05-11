using System;

namespace MoveShapeDemo
{
    public class TankModel
    {
        public string Id { get; private set; }
        private float _angle;
        private float _turretAngle;
        private const float MOVING_SPEED = 1;
        private const float ROTATION_SPEED = .3f;
        private const float TURRET_SPEED = .5f;

        public TankModel(string id)
        {
            Id = id;
        }

        public float Top { get; set; }
        public float Left { get; set; }

        public void MoveForward()
        {
            MovingIncrement = 1;
        }
        public void MoveBackward()
        {
            MovingIncrement = -1;
        }

        public void Stop()
        {
            MovingIncrement = 0;
        }

        public int MovingIncrement { get; set; }

        /// <summary>
        /// relative to Ox axis
        /// </summary>
        public float Angle
        {
            get { return _angle; }
            set { _angle = FixAngle(value); }
        }

        private static float FixAngle(float angle)
        {
            return angle%360;
        }

        /// <summary>
        /// relative to tank
        /// </summary>
        public float TurretAngle
        {
            get { return _turretAngle; }
            set {  _turretAngle = FixAngle(value); }
        }

        public float DesiredAngle { get; set; }
        public float TurretDesiredAngle { get; set; }

        public void Tick()
        {
            Rotate();

            RotateTurret();

            Move();
        }

        private void Move()
        {
            if (MovingIncrement == 0)
            {
                return;
            }
            var angle = Angle;
            if (MovingIncrement < 0)
            {
                angle += 180;
            }

            var radAngle = 2*Math.PI*angle/360;

            Top -= MOVING_SPEED*(float)Math.Sin(radAngle); // minus because Oy points down on the screen
            Left += MOVING_SPEED*(float)Math.Cos(radAngle);

        }

        private void Rotate()
        {
            if (Math.Abs(Angle - DesiredAngle) < ROTATION_SPEED) //don't overshoot
            {
                var offset = DesiredAngle - Angle;
                Angle += offset;
                return;
            }
            var rotationIncrement = Angle > DesiredAngle ? -1 : 1;
            Angle += rotationIncrement * ROTATION_SPEED;
        }


        private void RotateTurret()
        {
            if (Math.Abs(TurretAngle - TurretDesiredAngle) < TURRET_SPEED)
            {
                TurretAngle = TurretDesiredAngle;
                return;
            }
            var turretRotationIncrement = TurretAngle > TurretDesiredAngle ? -1 : 1;
            TurretAngle += turretRotationIncrement * TURRET_SPEED;
        }
    }
}