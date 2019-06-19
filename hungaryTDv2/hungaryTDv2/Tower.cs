/*
 * Name: Riley, Peter and Quinn
 * Date: June 18th, 2019
 * Description: A tower defense game where you try to eat angry food to protect a sacred fridge
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.Media;
using System.IO;

namespace hungaryTDv2
{
    public class Tower
    {
        public Point Location;
        public int towerType;
        public Rectangle towerRect;
        public Rectangle bullet = new Rectangle();
        public Polygon hitBox = new Polygon();
        public Canvas cBackground;
        public Canvas cObstacles;
        public Canvas cEnemies;
        public int[] positions;
        public Point[] track;
        public List<int> targets = new List<int>();
        public List<Shape> hits = new List<Shape>();
        public int range;
        public int bSpeed = 10;
        public int damage;
        public bool shooting = false;
        public double xMove;
        public double yMove;
        public BitmapImage bi;
        public double totalMove;
        public Rectangle[] famBullets = new Rectangle[8];
        public Polygon[] famHitboxes = new Polygon[8];
        public double[] famXMove = new double[8];
        public double[] famYMove = new double[8];
        /// <summary>
        /// Description: Creates an instance of the tower class. Chooses targets upon placement
        /// Author: Riley and Peter
        /// </summary>
        /// <param name="tT"></param>
        /// <param name="cBack"></param>
        /// <param name="cO"></param>
        /// <param name="p"></param>
        /// <param name="t"></param>
        /// <param name="l"></param>
        /// <param name="cE"></param>
        public Tower(int tT, Canvas cBack, Canvas cO, int[] p, Point[] t, Point l, Canvas cE)
        {
            towerType = tT;
            towerRect = new Rectangle();
            cBackground = cBack;
            cObstacles = cO;
            cEnemies = cE;
            positions = p;
            track = t;
            Location = l;
            if (towerType == 0)//norm
            {
                range = 100;
                damage = 25;

                bi = new BitmapImage(new Uri("normal.png", UriKind.Relative));
                towerRect.Fill = new ImageBrush(bi);
                towerRect.Height = 35;
                towerRect.Width = 35;

            }
            else if (towerType == 1)//popo
            {
                range = 300;
                damage = 50;

                bi = new BitmapImage(new Uri("police.png", UriKind.Relative));
                towerRect.Fill = new ImageBrush(bi);
                towerRect.Height = 35;
                towerRect.Width = 35;

            }
            else if (towerType == 2)//fam
            {
                range = 80;
                damage = 10;

                bi = new BitmapImage(new Uri("family.png", UriKind.Relative));
                towerRect.Fill = new ImageBrush(bi);
                towerRect.Height = 45;
                towerRect.Width = 70;
            }
            else//thicc
            {
                range = 100;
                damage = 500;
                bSpeed = 5;

                bi = new BitmapImage(new Uri("tank.png", UriKind.Relative));
                towerRect.Fill = new ImageBrush(bi);
                towerRect.Height = 70;
                towerRect.Width = 70;
            }

            Canvas.SetTop(towerRect, Location.Y - towerRect.Height / 2);
            Canvas.SetLeft(towerRect, Location.X - towerRect.Width / 2);
            cObstacles.Children.Add(towerRect);
            cBackground.Children.Remove(cObstacles);
            cBackground.Children.Add(cObstacles);

            StreamReader sr = new StreamReader("forkBox.txt");
            PointCollection myPointCollection = new PointCollection();
            while (!sr.EndOfStream)
            {
                string currentLine = sr.ReadLine();
                double xPosition, yPosition;
                double.TryParse(currentLine.Split(',')[0], out xPosition);
                double.TryParse(currentLine.Split(',')[1], out yPosition);
                Point point = new Point(xPosition, yPosition);
                myPointCollection.Add(point);
            }
            sr.Close();
            if (towerType != 2)
            {
                hitBox.Points = myPointCollection;

                bullet = new Rectangle();
                bullet.Height = 20;
                bullet.Width = 10;
                bi = new BitmapImage(new Uri("fork.png", UriKind.Relative));
                bullet.Fill = new ImageBrush(bi);
            }
            else
            {
                for (int i = 0; i < 8; i++)
                {
                    famBullets[i] = new Rectangle();
                    bi = new BitmapImage(new Uri("fork.png", UriKind.Relative));
                    famBullets[i].Fill = new ImageBrush(bi);
                    famBullets[i].Height = 20;
                    famBullets[i].Width = 10;
                    famHitboxes[i] = new Polygon();
                    famHitboxes[i].Points = myPointCollection;
                    double numOfTransforms = range / bSpeed;
                    famXMove[i] = range * Math.Cos(i * 45 * Math.PI / 180) / numOfTransforms;
                    famYMove[i] = range * Math.Sin(i * 45 * Math.PI / 180) / numOfTransforms;
                    RotateTransform rotate = new RotateTransform(-i * 45 + 90);
                    famBullets[i].RenderTransformOrigin = new Point(0.5, 0.5);
                    famBullets[i].RenderTransform = rotate;

                    famHitboxes[i].RenderTransformOrigin = new Point(0.5, 0.5);
                    famHitboxes[i].RenderTransform = rotate;
                }
            }
            for (int i = 0; i < positions.Length; i++)
            {
                double xDistance = xDistance = track[i].X - Location.X;
                double yDistance = yDistance = Location.Y - track[i].Y; ;
                double TotalDistance = Math.Sqrt(Math.Pow(xDistance, 2) + Math.Pow(yDistance, 2));
                if (TotalDistance < range)
                {
                    targets.Add(i);
                }
            }
        }
        /// <summary>
        /// Description: Runs every game tick, has two modes, shooting and drawing. When drawing it picks a target, then moves the bullet until it hits that target, taking input hit tests along the way. It only allows an enemy to be dealt damage once
        /// Author: Peter and Riley
        /// </summary>
        /// <returns></returns>
        public List<Shape> Shoot()
        {
            Point target;
            if (shooting)
            {
                if (towerType != 2)
                {
                    Canvas.SetLeft(bullet, Canvas.GetLeft(bullet) + xMove);
                    Canvas.SetTop(bullet, Canvas.GetTop(bullet) - yMove);
                    Canvas.SetLeft(hitBox, Canvas.GetLeft(hitBox) + xMove);
                    Canvas.SetTop(hitBox, Canvas.GetTop(hitBox) - yMove);
                    totalMove += Math.Sqrt(Math.Pow(xMove, 2) + Math.Pow(yMove, 2));
                    for (int i = 0; i < hitBox.Points.Count; i++)
                    {
                        Point screenPoint = hitBox.PointToScreen(hitBox.Points[i]);
                        Point canvasPoint = cBackground.PointFromScreen(screenPoint);
                        if (cEnemies.InputHitTest(canvasPoint) != null && !hits.Contains(cEnemies.InputHitTest(canvasPoint)))
                        {
                            Rectangle test = (Rectangle)cEnemies.InputHitTest(canvasPoint);
                            hits.Add(test);
                        }
                    }
                    if (totalMove >= range)
                    {
                        cBackground.Children.Remove(bullet);
                        cBackground.Children.Remove(hitBox);
                        shooting = false;
                        totalMove = 0;
                        xMove = 0;
                        yMove = 0;
                        return hits;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    for (int i = 0; i < famBullets.Length; i++)
                    {
                        Canvas.SetLeft(famBullets[i], Canvas.GetLeft(famBullets[i]) + famXMove[i]);
                        Canvas.SetTop(famBullets[i], Canvas.GetTop(famBullets[i]) - famYMove[i]);
                        Canvas.SetLeft(famHitboxes[i], Canvas.GetLeft(famHitboxes[i]) + famXMove[i]);
                        Canvas.SetTop(famHitboxes[i], Canvas.GetTop(famHitboxes[i]) - famYMove[i]);
                        totalMove += Math.Sqrt(Math.Pow(famXMove[i], 2) + Math.Pow(famYMove[i], 2));
                        for (int x = 0; x < famHitboxes[i].Points.Count; x++)
                        {
                            Point screenPoint = famHitboxes[i].PointToScreen(famHitboxes[i].Points[i]);
                            Point canvasPoint = cBackground.PointFromScreen(screenPoint);
                            if (cEnemies.InputHitTest(canvasPoint) != null && !hits.Contains(cEnemies.InputHitTest(canvasPoint)))
                            {
                                Rectangle test = (Rectangle)cEnemies.InputHitTest(canvasPoint);
                                hits.Add(test);
                            }
                        }
                    }
                    if (totalMove >= range * 8)
                    {
                        for (int i = 0; i < famBullets.Length; i++)
                        {
                            cBackground.Children.Remove(famBullets[i]);
                            cBackground.Children.Remove(famHitboxes[i]);
                        }
                        shooting = false;
                        totalMove = 0;
                        return hits;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            else
            {
                hits = new List<Shape>();
                if (towerType != 2)
                {
                    for (int i = 0; i < targets.Count; i++)
                    {
                        if (positions[targets[targets.Count - 1 - i]] != -1)
                        {
                            target = track[targets[targets.Count - 1 - i]];
                            double xDistance = target.X - Location.X;
                            double yDistance = Location.Y - target.Y;

                            double TotalDistance = Math.Sqrt(Math.Pow(xDistance, 2) + Math.Pow(yDistance, 2));
                            double NumbOfTransforms = Math.Ceiling(TotalDistance / bSpeed);
                            xMove = xDistance / NumbOfTransforms;
                            yMove = yDistance / NumbOfTransforms;

                            double temp = Math.Atan(xDistance / yDistance);
                            double angle = temp * 180 / Math.PI;

                            if (target.Y > Location.Y)
                            {
                                angle += 180;
                            }
                            RotateTransform rotate = new RotateTransform(angle);
                            bullet.RenderTransformOrigin = new Point(0.5, 0.5);
                            bullet.RenderTransform = rotate;
                            Canvas.SetLeft(bullet, Location.X);
                            Canvas.SetTop(bullet, Location.Y);
                            hitBox.RenderTransformOrigin = new Point(0.5, 0.5);
                            hitBox.RenderTransform = rotate;
                            Canvas.SetLeft(hitBox, Location.X);
                            Canvas.SetTop(hitBox, Location.Y);
                            cBackground.Children.Add(bullet);
                            cBackground.Children.Add(hitBox);
                            shooting = true;
                            break;
                        }
                    }
                }
                else
                {
                    for (int x = 0; x < targets.Count; x++)
                    {
                        if (positions[targets[targets.Count - 1 - x]] != -1)
                        {
                            for (int i = 0; i < famBullets.Length; i++)
                            {
                                Canvas.SetLeft(famHitboxes[i], Location.X);
                                Canvas.SetTop(famHitboxes[i], Location.Y);
                                Canvas.SetLeft(famBullets[i], Location.X);
                                Canvas.SetTop(famBullets[i], Location.Y);
                                cBackground.Children.Add(famBullets[i]);
                                cBackground.Children.Add(famHitboxes[i]);
                            }
                            shooting = true;
                            break;
                        }
                    }
                }
                return null;
            }
        }
    }
}