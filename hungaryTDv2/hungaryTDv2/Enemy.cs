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
using System.IO;

namespace hungaryTDv2
{
    public class Enemy
    {
        public ImageBrush enemyFill = new ImageBrush();
        public BitmapImage bi;
        public Type type;
        public enum Type { apple, pizza, donut, hamburger, fries }
        public Canvas cEnemies = new Canvas();
        public Canvas cBackground = new Canvas();
        public Rectangle sprite = new Rectangle();
        public int speed;
        public int health;
        public int damage;
        public Point[] track;
        public int[] positions;
        public int reward;
        public int position = 0;
        /// <summary>
        /// Description: Creates an instance of the enemy class with different characteristics based on the enemy type
        /// Author: Riley
        /// </summary>
        /// <param name="ty"></param>
        /// <param name="cE"></param>
        /// <param name="cB"></param>
        /// <param name="tr"></param>
        /// <param name="p"></param>
        public Enemy(int ty, Canvas cE, Canvas cB, Point[] tr, int[] p)
        {
            type = (Type)ty;
            cEnemies = cE;
            cBackground = cB;
            track = tr;
            positions = p;
            if (type == Type.apple)//set values for enemies based on input for enemy type
            {
                bi = new BitmapImage(new Uri("apple.png", UriKind.Relative));
                speed = 3;
                health = 150;
                damage = 50;
                reward = 25;
            }
            else if (type == Type.pizza)
            {
                bi = new BitmapImage(new Uri("pizza.png", UriKind.Relative));
                speed = 5;
                health = 200;
                damage = 50;
                reward = 25;
            }
            else if (type == Type.donut)
            {
                bi = new BitmapImage(new Uri("donut.png", UriKind.Relative));
                speed = 3;
                health = 250;
                damage = 100;
                reward = 50;
            }
            else if (type == Type.hamburger)
            {
                bi = new BitmapImage(new Uri("hamburger.png", UriKind.Relative));
                speed = 2;
                health = 5000;
                damage = 200;
                reward = 1000;
            }
            else if (type == Type.fries)
            {
                bi = new BitmapImage(new Uri("fries.png", UriKind.Relative));
                speed = 10;
                health = 350;
                damage = 50;
                reward = 50;
            }
            enemyFill = new ImageBrush(bi);//create the enemy sprite
            sprite.Fill = enemyFill;
            sprite.Height = 50;
            sprite.Width = 50;
            Canvas.SetLeft(sprite, track[position].X - 25);
            Canvas.SetTop(sprite, track[position].Y - 25);
            cEnemies.Children.Add(sprite);
            cBackground.Children.Remove(cEnemies);
            cBackground.Children.Add(cEnemies);
        }
        /// <summary>
        /// Description: Updates the enemies every game tick. Moves them along the track, making sure that they don't overlap and moving only by the speed amount. When the enemy is off the track it returns the damage and removes it. Otherwise it returns null
        /// Author: Riley
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public int update(int index)
        {
            positions[position] = -1;//sets current position to -1 or vacant
            for (int i = 1; i < 10; i++)
            {
                if (position + i < positions.Length)
                {
                    positions[position + i] = -1;
                }
                if (position - i > -1)
                {
                    positions[position - i] = -1;//sets surrounding positions to -1 or vacant
                }
            }


            if (position < 1450 - speed - 9)//Checks if the enemy is at the end of the track
            {
                for (int i = 0; i < speed + 1; i++)//Loops up to the speed of the enemy
                {
                    if (positions[position + i + 9] != -1) //checks if the end of the range + i is vacant
                    {
                        position = position + i - 1;//finds the first vacant position, then goes to that positions - 1
                        positions[position] = index;//set the new positions and the surrounding positions
                        for (int x = 1; x < 10; x++)
                        {
                            if (position + x < positions.Length)
                            {
                                positions[position + x] = index;
                            }
                            if (position - x > -1)
                            {
                                positions[position - x] = index;
                            }
                        }
                        break;
                    }
                    else if (i == speed && positions[position + i] == -1)//exception where the enemy gets to its speed and all those positions are empty
                    {
                        position = position + i - 1;
                        positions[position] = index;
                        for (int x = 1; x < 10; x++)
                        {
                            if (position + x < positions.Length)
                            {
                                positions[position + x] = index;
                            }
                            if (position - x > -1)
                            {
                                positions[position - x] = index;
                            }
                        }
                        break;
                    }
                }
                Canvas.SetLeft(sprite, track[position].X - 25);//move the enemy
                Canvas.SetTop(sprite, track[position].Y - 25);
                return 0;
            }
            else
            {
                cEnemies.Children.Remove(sprite);
                return damage;//when the enemy gets to the end of the track remove it and deal the damage to the health bar
            }
        }
    }
}
