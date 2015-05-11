using System;
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
        private const int FPS = 25;
        private readonly TimeSpan _broadcastInterval = TimeSpan.FromMilliseconds(1000/FPS);
        private readonly IHubContext _hubContext;
        private Timer _broadcastLoop;

        public Broadcaster()
        {
            GameManager = new GameManager();

            // Save our hub context so we can easily use it 
            // to send to its connected clients
            _hubContext = GlobalHost.ConnectionManager.GetHubContext<MoveShapeHub>();

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
            //_hubContext.Clients
            //    //.All()
            //    .AllExcept(_model.LastUpdatedBy)
            //    .updateShape(_model);
            //_modelUpdated = false;
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
            var justAccessIt = TankModel;

            return base.OnReconnected();
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

}
