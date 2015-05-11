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
    }
}