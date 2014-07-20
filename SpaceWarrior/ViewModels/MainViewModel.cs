﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SpaceWarrior.Entities;

namespace SpaceWarrior.ViewModels
{
    public class MainViewModel : PropertyChangedImplementation
    {
        private Player Player { get; set; }
        private readonly List<Bullet> _bullets;

        public double BulletWidth { get; private set; }
        public double BulletHeight { get; private set; }

        public MainViewModel(
            double playerPosX,
            double playerPosY,
            double playerWidth,
            double playerHeight,
            double worldWidth,
            double worldHeight,
            double playerSpeedX,
            double playerSpeedY,
            Action<double> movePlayerX,
            Action<double> movePlayerY,
            double bulletWidth,
            double bulletHeigth)
        {
            Player = new Player(playerPosX, playerPosY, playerWidth, playerHeight, playerSpeedX, playerSpeedY, movePlayerX, movePlayerY);

            WorldWidth = worldWidth;
            WorldHeight = worldHeight;
            _bullets = new List<Bullet>();
            BulletWidth = bulletWidth;
            BulletHeight = bulletHeigth;
        }

        private static double GetCurrentMilli()
        {
            var jan1970 = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var span = DateTime.UtcNow - jan1970;
            return span.TotalMilliseconds;
        }

        public bool CanAddBullet
        {
            get { return _timeSinceLastShot >= ShotFrequency; }
        }

        public void AddBulletIfPossible(Action<double> moveBulletX, Action<double> moveBulletY, Action removeBullet)
        {
            if (!CanAddBullet) return;

            var bulletPosX = Player.PosX + Player.Width - BulletWidth;
            var bulletPosY = Player.PosY + (Player.Height / 2) - (BulletHeight / 2);

            var bullet = new Bullet(bulletPosX, bulletPosY, Player.SpeedX + 300.0, 0, BulletWidth, BulletHeight, moveBulletX, moveBulletY, removeBullet);
            _bullets.Add(bullet);
            bullet.BulletOutOfScope += RemoveBullet;
            _timeSinceLastShot = 0;
        }

        private const double ShotFrequency = 0.1; //Frquenz ist eigentlich das falsche Wort hier
        private double _timeSinceLastShot = 0;


        public void RunWorker()
        {
            Task.Factory.StartNew(() =>
                                  {
                                      //Berechnung anhand der Zeit die der Rechner für das Berechnenn und Zeichnen braucht.
                                      //Dadurch bewegt sich der player auf jeder Hardware gleich schnell (wenn auch vll. ruckeliger)
                                      var lastFrame = GetCurrentMilli();

                                      while (true)
                                      {
                                          var thisFrame = GetCurrentMilli();
                                          var timeSinceLastFrame = (thisFrame - lastFrame) / 1000.0;
                                          _timeSinceLastShot += timeSinceLastFrame;
                                          lastFrame = thisFrame;

                                          Player.Update(timeSinceLastFrame, Up, Down, Left, Right, WorldWidth, WorldHeight);
                                          UpdateBullets(timeSinceLastFrame);

                                          Thread.Sleep(15);
                                      }
                                  });
        }

        private void RemoveBullet(object sender, EventArgs e)
        {
            _bullets.Remove(sender as Bullet);
        }

        public bool Up { get; set; }
        public bool Down { get; set; }
        public bool Left { get; set; }
        public bool Right { get; set; }
        public bool Space { get; set; }

        public void UpdateBullets(double timeSinceLastFram)
        {
            for (int i = 0; i < _bullets.Count; i++)
            {
                _bullets[i].Update(timeSinceLastFram, WorldWidth);
            }
        }

        //public void UpdateBullets(double timeSinceLastFrame)
        //{
        //    var bulletsToRemove = new List<Bullet>();
        //    for (int i = 0; i < _bullets.Count; i++)
        //    {
        //        _bullets[i].UpdateBullet(timeSinceLastFrame);
        //        if (_bullets[i].PosX > this.WorldWidth) bulletsToRemove.Add(_bullets[i]);
        //    }


        //    foreach (var bullet in bulletsToRemove)
        //    {
        //        _bullets.Remove(bullet);
        //    }
        //}

        private double _worldWidth;
        public double WorldWidth
        {
            get { return _worldWidth; }
            set
            {
                _worldWidth = value;
                OnPropertyChanged();
            }
        }

        private double _worlHeight;
        public double WorldHeight
        {
            get { return _worlHeight; }
            set
            {
                _worlHeight = value;
                OnPropertyChanged();
            }
        }


    }
}