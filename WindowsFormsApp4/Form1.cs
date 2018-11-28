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
        public int numberOfCities = 50;
        public Stopwatch stopwatch = new Stopwatch();

        //For the same citymap
        Random rnd2 = new Random(1);
        //For random annealing
        Random rnd = new Random();

        public Tour tour;
        Stop[] Stoplist;

        public Form1()
        {
            SetupCities();
            InitializeComponent();
        }

        //It goes over possible swaps and changes the bestTour if it has found a better one, does this until the tsp if fully optimized.
        Tour TourMutations(Tour tour)
        {
            bool improved = true;
            float bestDistance = tour.cost;
            int improvX = 0; int improvY = 0;
            while (improved)
            {
                improved = false;
                for (int x = 1; x < numberOfCities - 1; x++)
                {
                    for (int y = x + 1; y < numberOfCities - 1; y++)
                    {
                        float distanceBefore = Distance(tour.AllStops[x - 1].city.city, tour.AllStops[x].city.city) + Distance(tour.AllStops[y].city.city, tour.AllStops[y + 1].city.city);

                        float deltaChange = Distance(tour.AllStops[x - 1].city.city, tour.AllStops[y].city.city) + Distance(tour.AllStops[x].city.city, tour.AllStops[y + 1].city.city);
                        if (deltaChange < distanceBefore)
                        {
                            improved = true;
                            bestDistance = bestDistance - distanceBefore + deltaChange;
                            improvX = x;
                            improvY = y;
                        }
                    }
                }

                if (improved)
                    tour = TourClone(tour, improvX, improvY);

            }

            return TourClone(tour, improvX, improvY);
        }

        float acceptance(float oldC, float newC, float T)
        {
            float between = (newC - oldC) / T;
            return (float)Math.Exp(between);
        }

        //Mutation with SimulAnneal
        Tour SimulAnnealTourMutations(Tour tour)
        {
            //Parameters
            float alpha = 0.99f;
            float T_min = 0.01f;
            float T = 16.0f;
            int iterations =100;

            float oldDistance = tour.cost;
            int x; int y;

            while (T > T_min)
            {
                int i = 0;
                while (i < iterations)
                {
                    i++;
                    while (true)
                    {
                        x = rnd.Next(1, numberOfCities);
                        y = rnd.Next(0, numberOfCities - 1);
                        if (x != y)
                            break;
                    }

                    float distanceBefore = Distance(tour.AllStops[x - 1].city.city, tour.AllStops[x].city.city) + Distance(tour.AllStops[y].city.city, tour.AllStops[y + 1].city.city);
                    float deltaChange = Distance(tour.AllStops[x - 1].city.city, tour.AllStops[y].city.city) + Distance(tour.AllStops[x].city.city, tour.AllStops[y + 1].city.city);
                    float newDistance = oldDistance - distanceBefore + deltaChange;

                    if(deltaChange < distanceBefore)
                    {
                        tour = TourClone(tour, x, y);
                        oldDistance = newDistance;
                    }
                    else if (acceptance(distanceBefore, deltaChange, T) < (float)rnd.NextDouble())
                    {
                        tour = TourClone(tour, x, y);
                        oldDistance = newDistance;
                    }
                }
                T = T * alpha;
            }
            return tour;
        }



        //Individually copy the stops, not sure if necessary but probably cuz otherwise you would copy an array
        Tour TourClone(Tour tour, int x, int y)
        {
            //How 2-opt works (the swap)
            Stop[] stopListNew = new Stop[tour.AllStops.Length];
            for (int i = 0; i <= x - 1; i++)
            {
                stopListNew[i] = new Stop(tour.AllStops[i].city);
            }
            for (int i = x; i <= y; i++)
            {
                stopListNew[y - (i - x)] = new Stop(tour.AllStops[i].city);
            }
            for (int i = y + 1; i < tour.AllStops.Length; i++)
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
            foreach (Stop stop in tour.AllStops)
            {
                totalDistance += Distance(stop.city.city, stop.prevNext[1].city.city);
            }

            return totalDistance;
        }


        //Calculates the distance between 2 points.. Dont need this in our assignment
        static float Distance(Point a, Point b)
        {
            return (float)(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
        }

        void SetupCities()
        {
            Stoplist = new Stop[numberOfCities];
            for (int i = 0; i < numberOfCities; i++)
            {
                City newcity = new City(rnd2.Next(100, 800 - 100), rnd2.Next(100, 800 - 100), i);
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
            tour = SimulAnnealTourMutations(tour);
            Console.WriteLine("Cost is " + TourDistance(tour));
            Invalidate();
            Console.WriteLine((float)(stopwatch.ElapsedMilliseconds) / 1000f + "s");
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
