using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace MoveShapeDemo
{
    public class GameManager
    {
        private readonly Random _random = new Random((int)DateTime.Now.Ticks);
        private readonly KeyedCollection<string, TankModel> _tanks = new KeyedCollectionEx<string, TankModel>(t => t.Id); 
        public TankModel GetTank(string userId)
        {
            if (!_tanks.Contains(userId))
            {
                //_tanks.Clear();
                var newTank = new TankModel(userId)
                {
                    Top = 52,
                    Left = 303,
                    Angle = 260.300323f,
                    DesiredAngle = 260.300323f,
                    TurretAngle = 21,
                    TurretDesiredAngle = 21,
                    
                    //Top = _random.Next(400),
                    //Left = _random.Next(400),
                    //Angle = _random.Next(360),
                    //DesiredAngle = _random.Next(360),
                    //TurretAngle = _random.Next(360),
                    //TurretDesiredAngle = _random.Next(360),

                    MovingIncrement = 0//_random.Next(2)
                };
                _tanks.Add(newTank);
            }
            return _tanks[userId];
        }

        [JsonProperty("tanks")]
        public IEnumerable<TankModel> Tanks { get { return _tanks; } }

        bool ticking;


        public void Tick()
        {

            if(ticking)
                return;

            ticking = true;
            foreach (var tank in _tanks)
            {
                tank.Tick();
            }
            ticking = false;

        }
        private readonly ICollection<string> _removedIds = new Collection<string>();

        public IEnumerable<string> RemovedTankIds { get { return _removedIds; } }

        public void RemoveTank(string id)
        {
            if ( _tanks.Contains(id) )
            {
                _tanks.Remove(id);
                _removedIds.Add(id);
            }
        }
    }
}