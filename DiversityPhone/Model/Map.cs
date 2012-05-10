﻿using System;
using System.Data.Linq.Mapping;
using DiversityPhone.Services;
using System.Linq;
using System.IO.IsolatedStorage;
using System.Xml.Linq;
using System.IO;
using System.Windows;

namespace DiversityPhone.Model
{
    [Table]
    public class Map:IModifyable
    {

        [Column(IsPrimaryKey = true)]
        public String ServerKey { get; set; }

        [Column]
        public String Uri { get; set; }

        [Column]
        public String Name { get; set; }

        [Column]
        public String Description { get; set; }

        [Column]
        public double NWLat { get; set; }

        [Column]
        public double NWLong { get; set; }

        [Column]
        public double SELat { get; set; }

        [Column]
        public double SELong { get; set; }

        [Column]
        public double SWLat { get; set; }

        [Column]
        public double SWLong { get; set; }
        [Column]
        public double NELat { get; set; }

        [Column]
        public double NELong { get; set; }
       
        [Column(CanBeNull = true)]
        public int? Transparency{get;set;}

        [Column(CanBeNull = true)]
        public int? ZoomLevel{ get; set; }

      

        /// <summary>
        /// Tracks modifications to this Object.
        /// is null for newly created Objects
        /// </summary>
        [Column(CanBeNull = true)]
        public bool? ModificationState { get; set; }

        [Column]
        public DateTime LogUpdatedWhen { get; set; }

        public static IQueryOperations<Map> Operations
        {
            get;
            private set;
        }
        
     

        public Map()
        {            
            this.ModificationState= null;
            this.LogUpdatedWhen = DateTime.Now;
            Operations = new QueryOperations<Map>(
                //Smallerthan
                        (q, map) => q.Where(row => row.NWLat < map.NWLat),
                //Equals
                        (q, map) => q.Where(row => row.ServerKey == map.ServerKey),
                //Orderby
                        (q) => from map in q
                               orderby map.NWLat, map.NWLong
                               select map,
                //FreeKey
                        (q, cep) =>
                        {
                            //Not Applicable
                        });

        }

        public static bool isParallelogramm(Map m)
        {
           
            Point AD = new Point(m.SWLong - m.NWLong, m.SWLat - m.NWLat);
            Point BC = new Point(m.SELong - m.NELong, m.SELat - m.NELat);
            Point AB = new Point(m.NELong - m.NWLong, m.NELat - m.NWLat);
            Point DC = new Point(m.SELong -m.SWLong, m.SELat - m.SWLat);

            double delta1 = AD.X - BC.X;
            double delta2 = AD.Y - BC.Y;
            double delta3 = AB.X - DC.X;
            double delta4 = AB.Y - DC.Y;

            if (AD.X != BC.X || AD.Y != BC.Y)
                return false;
            if (AB.X != DC.X || AB.Y != DC.Y)
                return false;
            return true;

        }

        public static bool isOnMap(Map map, double? latitude, double? longitude)
        {

            if (map == null || latitude == null || longitude == null)
                return false;
            Point? p = calculatePercentilePositionForMap((double) latitude,(double) longitude, map);
            if (p == null || p.Value.X < 0 || p.Value.X > 1 || p.Value.Y < 0 || p.Value.Y > 1)
                return false;
            return true;
        }



        //The corner coordinates of the map dont define an exact rectangle in all cases. Generally, a convex quadrangle is defined.
        //The represetation on the screen will be in an rectangle. Hence, the corresponding position is calculated. To perform this,
        //2 lines are definend: One Going from the upper-left corner to the upper right corner of the map. The other one from the lower left corner to the lower right corner of the map.
        //A cohort of lines is definend ny the connection between those lines parametrized by the percentual scale between the cornerpoint of the 2 lines definend in this way.
        //Analogously 2 lines are definend in the y-Direction which also define a cohort of lines between them. The GPS-Value is at a specific intersection of these cohorts.
        //These specific lines of the cohorts can be found by solving a quadratic equation. Theses lines define with their parameters the position of the GPS-Value on the map (On a percentual basis).
        public static Point? calculatePercentilePositionForMap(double GPSLatitude, double GPSLongitude, Map map)
        {
            double a, b,c, d, e, f, g, h; //Map-specific values derived form the position of the cornerpoints. a and e are dependent on addtional values and calculated in the position calculation method
            bool specialCase;//Criterion for the special case mu=-b/d (which leads to a division by zero in the lambda calculus)

            a = -GPSLongitude + map.NWLong;
            b = - map.NWLong + map.NELong;
            c = - map.NWLong + map.SWLong;
            d = map.NWLong - map.NELong + map.SELong - map.SWLong;

            e = -GPSLatitude + map.NWLat;
            f = - map.NWLat + map.NELat;
            g = - map.NWLat + map.SWLat;
            h = map.NWLat - map.NELat + map.SELat - map.SWLat;

            if(b==0 || g==0)
            {
                //Map coordinates are corrupted
                return null;
            }

            //Check d==0
            if (d != 0)
            {
                if (a - c * b / d == 0)
                    specialCase = true;
                else
                    specialCase = false;
            }
            else
                specialCase = false;
            if(specialCase==false) //=>mu!=-b/d
            {
                double alpha, beta, gamma;//Coeffizients for the Mitternachts-Equation
                double discrim;//Discriminant in the Mitternachts-Equation
                double mu1,mu2,lambda1,lambda2;//Solution-Parameters. As the equation is quadratic two possible solutions exist.
                alpha=g*d-h*c;
                beta=e*d-c*f+g*b-a*h;
                gamma=-a*f+e*b;
                if (alpha != null)
                {
                    discrim = beta * beta - 4 * alpha * gamma;
                    if (discrim < 0) //Equation unsovable
                        return null;
                    mu1 = (-beta + Math.Sqrt(discrim)) / (2 * alpha);
                    mu2 = (-beta - Math.Sqrt(discrim)) / (2 * alpha);
                    lambda1 = -(a + c * mu1) / (b + d * mu1);
                    lambda2 = -(a + c * mu2) / (b + d * mu2);

                    Point p1 = new Point(lambda1, mu1);
                    Point p2 = new Point(lambda2, mu2);
                    if (p1.X > 0 && p1.X < 1 && p1.Y > 0 && p1.Y < 1)
                        return p1;
                    if (p2.X > 0 && p2.X < 1 && p2.Y > 0 && p2.Y < 1)
                        return p2;
                    return null;
                }
                else
                {
                    if (beta != 0)
                    {
                        mu1 = -gamma / beta;
                        lambda1 = -(a + c * mu1) / (b + d * mu1);
                        Point p1 = new Point(lambda1, mu1);
                        if (p1.X > 0 && p1.X < 1 && p1.Y > 0 && p1.Y < 1)
                            return p1;
                        return null;
                    }
                    else
                    {
                        //No mu-Percentile can be calculated as the equation is either unsovable or there are no restriction to mu.
                        //In both cases no representation on the map can be given
                        return null;
                    }
                }
            }
            else //=> mu=-b/d
            {
                double lambda, mu;
                mu = -b / d;
                if (b * h - f * c != 0)
                {
                    lambda = -(e * d - b * g) / (b * h - f * c);//Div by Zero
                    Point p = new Point(lambda, mu);
                    return p;
                }
                else
                {
                    //No mu-Percentile can be calculated as the equation is either unsovable or there are no restriction to mu.
                    //In both cases no representation on the map can be given
                    return null;
                }
            }
        }

    
 

        public static Map loadMapParameterFromFile(string xmlFileName)
        {
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!isoStore.FileExists(xmlFileName))
                    throw new FileNotFoundException(xmlFileName + " not found");
                try
                {
                    using (IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream(xmlFileName, FileMode.Open, isoStore))
                    {
                        XDocument load = XDocument.Load(isoStream);
                        var data = from query in load.Descendants("ImageOptions")
                                   select new Map
                                   {
                                       Name = (string)query.Element("Name"),
                                       Description = (string)query.Element("Description"),
                                       NWLat = (double)query.Element("NWLat"),
                                       NWLong = (double)query.Element("NWLong"),
                                       SELat = (double)query.Element("SELat"),
                                       SELong = (double)query.Element("SELong"),
                                       SWLat = (double)query.Element("SWLat"),
                                       SWLong = (double)query.Element("SWLong"),
                                       NELat = (double)query.Element("NELat"),
                                       NELong = (double)query.Element("NELong"),
                                       ZoomLevel = (int?)query.Element("ZommLevel"),
                                       Transparency = (int?)query.Element("Transparency")
                                   };
                        if (data.Count<Map>() == 0)
                            throw new Exception("File not found or not in ImageOptions.xml-Standars");
                        else if (data.Count<Map>() > 1)
                            throw new Exception("Multiple Results in ImageOptionsFile");
                        return data.First<Map>();
                    }
                }

                catch (Exception e)
                {
                    Map i = new Map();
                    i.Name = e.Message;
                    i.Description = "XML-Parse-Error";
                    return i;
                }
            }
        }

    }
}
