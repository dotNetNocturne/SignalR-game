using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;

namespace MoveShapeDemo
{
    public class Broadcaster
    {
        private readonly static Lazy<Broadcaster> _instance = new Lazy<Broadcaster>();
        // We're going to broadcast to all clients a maximum of 25 times per second
        private readonly TimeSpan _broadcastInterval = TimeSpan.FromMilliseconds(1000/25);
        private readonly IHubContext _hubContext;
        private Timer _broadcastLoop;
        private ShapeModel _model;
        private bool _modelUpdated;

        public Broadcaster()
        {
            GameManager = new GameManager();

            // Save our hub context so we can easily use it 
            // to send to its connected clients
            _hubContext = GlobalHost.ConnectionManager.GetHubContext<MoveShapeHub>();

            _model = new ShapeModel();
            _modelUpdated = false;
            // Start the broadcast loop
            _broadcastLoop = new Timer(
                GameLoop,
                null,
                _broadcastInterval,
                _broadcastInterval);
        }

        private void GameLoop(object state)
        {

            // No need to send anything if our model hasn't changed
            //if (!_modelUpdated) 
            //    return;

            GameManager.Tick();

            _hubContext.Clients
                .All
                .updateScene(GameManager);

             //This is how we can access the Clients property 
             //in a static hub method or outside of the hub entirely
            _hubContext.Clients
                //.All()
                .AllExcept(_model.LastUpdatedBy)
                .updateShape(_model);
            //_modelUpdated = false;
        }
        public void UpdateShape(ShapeModel clientModel)
        {
            _model = clientModel;
            _modelUpdated = true;
        }
        public GameManager GameManager { get; private set; }
        public static Broadcaster Instance
        {
            get
            {
                return _instance.Value;
            }
        }
    }

    public class GameManager
    {
        private readonly Random _random = new Random((int)DateTime.Now.Ticks);
        private readonly KeyedCollection<string, TankModel> _tanks = new KeyedCollectionEx<string, TankModel>(t => t.Id); 
        public TankModel GetTank(string userId)
        {
            if (!_tanks.Contains(userId))
            {
                var newTank = new TankModel(userId)
                {
                    Top=_random.Next(600),
                    Left = _random.Next(1000),
                    Angle = _random.Next(360),
                    DesiredAngle = _random.Next(360),
                    TurretAngle = _random.Next(360),
                    TurretDesiredAngle = _random.Next(360),
                    
                    MovingIncrement = _random.Next(2)
                };
                _tanks.Add(newTank);
            }
            return _tanks[userId];
        }

        [JsonProperty("tanks")]
        public IEnumerable<TankModel> Tanks { get { return _tanks; } } 
        
        public void Tick()
        {
            foreach (var tank in _tanks)
            {
                tank.Tick();
            }
        }
    }

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
            return angle%180;
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

            var radAngle = 2*Math.PI*360/angle;

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

    public class MoveShapeHub : Hub
    {
        // Is set via the constructor on each creation
        private readonly Broadcaster _broadcaster;

        private readonly Lazy<TankModel> _tankModel; 
        private TankModel TankModel {get { return _tankModel.Value; }}

        
        public MoveShapeHub()
            : this(Broadcaster.Instance)
        {
        }

        public MoveShapeHub(Broadcaster broadcaster)
        {
            _broadcaster = broadcaster;
            _tankModel = new Lazy<TankModel>(() => _broadcaster.GameManager.GetTank(Context.ConnectionId));
        }

        public override Task OnConnected()
        {
            var id = Context.ConnectionId;
            var justAccessIt = TankModel;

            return base.OnConnected();
        }

        public override Task OnReconnected()
        {
            var id = Context.ConnectionId;
            return base.OnReconnected();
        }

        public void UpdateModel(ShapeModel clientModel)
        {
            clientModel.LastUpdatedBy = Context.ConnectionId;

            // Update the shape model within our broadcaster
            _broadcaster.UpdateShape(clientModel);
        }

        public void Move()
        {
            TankModel.MoveForward();
        }
        public void Stop()
        {
            TankModel.Stop();
        }

        public void Rotate(float to)
        {
            TankModel.DesiredAngle = to;
        }
    }
    public class ShapeModel
    {
        // We declare Left and Top as lowercase with 
        // JsonProperty to sync the client and server models
        [JsonProperty("left")]
        public double Left { get; set; }
        [JsonProperty("top")]
        public double Top { get; set; }
        // We don't want the client to get the "LastUpdatedBy" property
        [JsonIgnore]
        public string LastUpdatedBy { get; set; }
    }

}
