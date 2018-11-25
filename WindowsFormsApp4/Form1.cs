using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace WindowsFormsApp4
{
    public partial class Form1 : Form
    {
        public int numberOfCities = 100;
        Random rnd = new Random(10);
        public Tour tour;
        Stop[] Stoplist;
        public Stopwatch stopwatch = new Stopwatch();
        public Form1()
        {
            SetupCities();
            InitializeComponent();
        }

     //It goes over possible swaps and changes the bestTour if it has found a better one, does this until the tsp if fully optimized.
        Tour TourMutations(Tour tour)
        {
            Tour[] possibleMutations = new Tour[numberOfCities];
            bool improved = true;
            float bestDistance = TourDistance(tour);
            Tour bestTour = tour;
            while(improved)
            {
                improved = false;
                for (int x = 1; x < numberOfCities-1; x++)
                {
                    for (int y = x+1; y < numberOfCities; y++)
                    {                     
                        Tour newTour = TourClone(bestTour,x,y);
                        float newDistance = TourDistance(newTour);
                        if(newDistance<bestDistance)
                        {
                            improved = true;
                            bestTour = newTour;
                            bestDistance = newDistance;
                        }
                    }
                }
            }
            return bestTour;

        }

        //Individually copy the stops, not sure if necessary but probably cuz otherwise you would copy an array
        Tour TourClone(Tour tour,int x, int y)
        {
            //How 2-opt works (the swap)
            Stop[] stopListNew = new Stop[tour.AllStops.Length];
            for (int i = 0; i <= x-1; i++)
            {
                stopListNew[i] = new Stop(tour.AllStops[i].city);
            }
            for (int i = x; i <= y; i++)
            {
                stopListNew[y-(i-x)] = new Stop(tour.AllStops[i].city);
            }
            for (int i = y+1; i < tour.AllStops.Length; i++)
            {
                stopListNew[i] = new Stop(tour.AllStops[i].city);
            }

            //Connecting the Stops
            stopListNew[0].prevNext = new Stop[] { stopListNew[stopListNew.Length - 1], stopListNew[1] };
            for (int i = 1; i < stopListNew.Length; i++)
            {
                stopListNew[i].prevNext = new Stop[] { stopListNew[i - 1], stopListNew[(i + 1) % numberOfCities] };
            }

            return new Tour(stopListNew);
        }

        //Just calculates the total distance of a Tour
        static public float TourDistance(Tour tour)
        {
            float totalDistance = 0f;
            foreach(Stop stop in tour.AllStops)
            {
                totalDistance += Distance(stop.city.city, stop.prevNext[1].city.city);
            }

            return totalDistance;
        }


        //Calculates the distance between 2 points
        static float Distance(Point a, Point b)
        {
            return (float)(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }

        void SetupCities()
        {
            Stoplist = new Stop[numberOfCities];
            for (int i = 0; i < numberOfCities; i++)
            {
                City newcity = new City(rnd.Next(100, 800 - 100), rnd.Next(100, 800 - 100), i);
                Stoplist[i] = new Stop(newcity);
            }

            Stoplist[0].prevNext = new Stop[] { Stoplist[numberOfCities - 1], Stoplist[1] };
            for (int i = 1; i < numberOfCities; i++)
            {
                Stoplist[i].prevNext = new Stop[] { Stoplist[i - 1], Stoplist[(i + 1) % numberOfCities] };
            }

            tour = new Tour(Stoplist);
        }

        //Draws the points
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            SolidBrush brush = new SolidBrush(Color.Red);
            Pen pen = new Pen(Color.Blue, 3);

            foreach (Stop stop in tour.AllStops)
            {
                g.DrawLine(pen, stop.city.city, stop.prevNext[1].city.city);
                Rectangle rect = new Rectangle(stop.city.city.X - 4, stop.city.city.Y - 4, 8, 8);
                g.FillEllipse(brush, rect);
            }
        }

        //When the form is clicked the tsp is optimized
        private void Form1_Click(object sender, EventArgs e)
        {
            stopwatch.Start();
            tour = TourMutations(tour);            
            Invalidate();
            Console.WriteLine((float)(stopwatch.ElapsedMilliseconds)/1000f + "s");
            stopwatch.Stop();
        }
    }






    //Just some objects.. I thought it would be more clear this way.. Could be simplified
    public class City
    {
        public Point city;
        int cityName;

        public City(int cityX, int cityY, int cityName)
        {
            this.cityName = cityName;
            city = new Point(cityX, cityY);
        }
    }

    public class Stop
    {
        public Stop[] prevNext = new Stop[2];
        public City city;

        public Stop(City city)
        {
            this.city = city;
        }
    }

    public class Tour
    {
        public Stop[] AllStops;
        public float cost;
        public Tour(Stop[] AllStops)
        {
            this.AllStops = AllStops;
            cost = Form1.TourDistance(this);
        }
    }
}
