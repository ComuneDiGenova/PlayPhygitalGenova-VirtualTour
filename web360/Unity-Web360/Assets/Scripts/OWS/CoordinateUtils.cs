using System;
using UnityEngine;


namespace GeoCoordinate
{

    [Serializable]
    public class Point
    {
        /// <summary>
        /// Easting Longitude
        /// </summary>
        /// <value></value>
        public double X { get; private set; }
        /// <summary>
        /// Northing Latitude
        /// </summary>
        /// <value></value>
        public double Y { get; private set; }

        public bool isRad { get; private set; }

        public Point() { }

        /// <summary>
        /// Create new Point
        /// </summary>
        /// <param name="easting">X, Easting, Longitude</param>
        /// <param name="northing">Y, Northing, Latitude</param>
        public Point(double easting, double northing)
        {
            X = easting;
            Y = northing;
            isRad = false;
        }

        public Coordinate ToCoordinate()
        {
            if (isRad)
            {
                return this.ToDecDegree().ToCoordinate();
            }
            else
            {
                return new Coordinate(Y, X);
            }
        }

        public Point ToRadiant() => new Point(Utils.Deg2Rad(X), Utils.Deg2Rad(Y)) { isRad = true };
        public Point ToDecDegree() => new Point(Utils.Rad2Deg(X), Utils.Rad2Deg(Y)) { isRad = false };

        public Vector2 ToVector2() => new Vector2((float)X, (float)Y);
        public Vector3 ToVector3() => new Vector3((float)X, 0, (float)Y);


        public override string ToString() => $"Point - rad: {isRad} | X Easting Long: {X}, Y Northing Lat: {Y}";

        public static Point operator +(Point a, Point b) => new Point(a.X + b.X, a.Y + b.Y);
        public static Point operator -(Point a, Point b) => new Point(a.X - b.X, a.Y - b.Y);
        public static Point operator *(Point a, double b) => new Point(a.X * b, a.Y * b);
        public static Point operator /(Point a, double b) => new Point(a.X / b, a.Y / b);

    }

    [Serializable]
    public class Coordinate
    {
        public double Latitude { get; private set; }  // degrees from -90 to 90
        public double Longitude { get; private set; } // degrees from -180 to 180

        /// <summary>
        /// Create new Coordinate with Value control
        /// </summary>
        /// <param name="lat">Latitude (-90 / 90)</param>
        /// <param name="lon">Longitude (-180 / 180)</param>
        public Coordinate(double lat, double lon)
        {
            Latitude = lat;
            Longitude = lon;
            this.Clamp();
        }
        private Coordinate(double lat, double lon, bool wrap)
        {
            Latitude = lat;
            Longitude = lon;
            this.Wrap();
        }

        public Point ToRad() => this.ToPoint().ToRadiant();

        public Vector2 ToVector2() => new Vector2((float)Longitude, (float)Latitude);

        public Point ToPoint() => new Point(Longitude, Latitude);

        public override string ToString() => $"Location - Latitude: {Latitude}, Longitude: {Longitude}";

        private Coordinate Clamp()
        {
            Latitude = Latitude > 90 ? 90 : Latitude;
            Latitude = Latitude < -90 ? -90 : Latitude;
            Longitude = Longitude > 180 ? 180 : Longitude;
            Longitude = Longitude < -180 ? -180 : Longitude;
            return this;
        }
        private Coordinate Wrap()
        {
            Latitude = Latitude > 90 ? Latitude - 180 : Latitude;
            Latitude = Latitude < -90 ? 180 + Latitude : Latitude;
            Longitude = Longitude > 180 ? Longitude - 360 : Longitude;
            Longitude = Longitude < -180 ? 360 + Longitude : Longitude;
            return this;
        }

        public static Coordinate operator +(Coordinate a, Coordinate b) => new Coordinate(a.Latitude + b.Latitude, a.Longitude + b.Longitude, true);
        public static Coordinate operator -(Coordinate a, Coordinate b) => new Coordinate(a.Latitude - b.Latitude, a.Longitude - b.Longitude, true);
        public static Coordinate operator *(Coordinate a, double b) => new Coordinate(a.Latitude * b, a.Longitude * b, true);
        public static Coordinate operator /(Coordinate a, double b) => new Coordinate(a.Latitude / b, a.Longitude / b, true);
    }

    public static class Utils
    {
        //Ellipsoid EPSG:4979 WGS84
        public const double Wgs84EquatorialRadius = 6378137.0d;
        public const double Wgs84MeridianRadius = Wgs84EquatorialRadius * 1d - (1d / 298.257223563d);
        public const double Wgs84MetersPerDegreeX = Wgs84EquatorialRadius * Math.PI / 180d;
        public const double Wgs84MetersPerDegreeY = Wgs84MeridianRadius * Math.PI / 180d;

        /// <summary>
        /// Convert Decimal Degrees to Radiants
        /// </summary>
        /// <param name="deg"></param>
        /// <returns></returns>
        public static double Deg2Rad(double deg) => (deg * Math.PI / 180d);

        /// <summary>
        /// Convert Radiants to Decimal Degrees
        /// </summary>
        /// <param name="rad"></param>
        /// <returns></returns>
        public static double Rad2Deg(double rad) => (rad * 180d / Math.PI);

        /*
            https://www.movable-type.co.uk/scripts/latlong.html

            Haversine formula:
            λ -> long
            φ -> lat

            a = sin²(Δφ/2) + cos φ1 ⋅ cos φ2 ⋅ sin²(Δλ/2)
            c = 2 ⋅ atan2( √a, √(1−a) )
            d = R ⋅ c
        */
        /// <summary>
        /// Geographic Distance between two coordiantes (WGS84)
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public static double HaversineDistance(Coordinate from, Coordinate to)
        {
            var lon1 = Deg2Rad(from.Longitude);
            var lon2 = Deg2Rad(to.Longitude);
            var deltaLon = lon2 - lon1;
            var lat1 = Deg2Rad(from.Latitude);
            var lat2 = Deg2Rad(to.Latitude);
            var deltaLat = lat2 - lat1;
            var a = Math.Pow(Math.Sin(deltaLat / 2d), 2d) + Math.Cos(lat1) * Math.Cos(lat2) * Math.Pow(Math.Sin(deltaLon / 2d), 2d);
            var c = 2d * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1d - a));
            return Wgs84EquatorialRadius * c;
        }

        /// <summary>
        /// Interpolate points along a segment with a fixed distance
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="distanceBetweenPoints"></param>
        /// <returns></returns>
        public static Coordinate[] InterpolatePoints(Coordinate p1, Coordinate p2, double distanceBetweenPoints = 10d, double totalDistance = 0)
        {
            if (totalDistance == 0)
                totalDistance = HaversineDistance(p1, p2);
            int numPoints = (int)Math.Ceiling(totalDistance / distanceBetweenPoints) + 1;

            // Linear interpolation between the two points
            Coordinate[] points = new Coordinate[numPoints];
            for (int i = 0; i < numPoints; i++)
            {
                double fraction = (double)i / (numPoints - 1);
                //double lat = p1.Latitude + (p2.Latitude - p1.Latitude) * fraction;
                //double lon = p1.Longitude + (p2.Longitude - p1.Longitude) * fraction;
                //points[i] = new Coordinate(lat, lon);
                points[i] = p1 + (p2 - p1) * fraction;
            }
            return points;
        }

        /*
        β = atan2(X,Y),

        For  variable X = sin(toRadians(lo2-lo1)) *  cos(toRadians(la2))

        and variable Y = cos(toRadians(la1))*sin(toRadians(la2)) – sin(toRadians(la1))*cos(toRadians(la2))*cos(toRadians(lo2-lo1))

        β = atan2(X,Y),

        where, X and Y are two quantities and can be calculated as:

        X = cos θb * sin ∆L

        Y = cos θa * sin θb – sin θa * cos θb * cos ∆L

        */
        /// <summary>
        /// Calcualte the north bearing from two coordinates
        /// </summary>
        /// <param name="from">base coordinate</param>
        /// <param name="to">target coordinate</param>
        /// <param name="decDegree">return decimal degrees (true) or Radians (false)</param>
        /// <returns></returns>
        public static double Bearing(Coordinate from, Coordinate to, bool decDegree = true)
        {
            var fromRad = from.ToRad();
            var toRad = to.ToRad();
            Debug.Log(from.ToString() + " | " + fromRad.ToString());
            Debug.Log(to.ToString() + " | " + toRad.ToString());
            double Y = (Math.Cos(fromRad.Y) * Math.Sin(toRad.Y)) - (Math.Sin(fromRad.Y) * Math.Cos(toRad.Y) * Math.Cos(toRad.X - fromRad.X));
            double X = Math.Sin(toRad.X - fromRad.X) * Math.Cos(toRad.Y);
            Debug.Log($"{X} | {Y}");
            double bearing = Math.Atan2(X, Y);
            Debug.Log(bearing);
            if (decDegree)
                return Utils.Rad2Deg(bearing);
            else
                return bearing;
        }

        /************* MERCATORE *******************/

        //https://stackoverflow.com/a/14457180
        //https://mathworld.wolfram.com/MercatorProjection.html
        /*
            λ -> long
            φ -> lat

            x	=	λ-λ
            y	=	ln[tan(1/4𝜋+1/φ)]

            λ	=	x+λ,
            phi	=	2tan^(-1)(e^y)-1/2𝜋
        */

        /// <summary>
        /// Convert Geographic Coordiante (lat/long) to Universal Transverse Mercator (Northing / Easting) (WGS84)
        /// </summary>
        /// <param name="location">Coordiante</param>
        /// <returns>Point (X,Y)</returns>
        public static Point GeoToUtm(Coordinate location)
        {
            return new Point(
                Wgs84EquatorialRadius * Deg2Rad(location.Longitude),
                Wgs84MeridianRadius * LatitudeToY(Deg2Rad(location.Latitude)));
        }
        /// <summary>
        /// Convert Geographic Coordiante (X/Y) to Universal Transverse Mercator (Northing / Easting) (WGS84)
        /// </summary>
        /// <param name="location">Coordiante</param>
        /// <returns>Point (X,Y)</returns>
        public static Point GeoToUtm(Point point)
        {
            return new Point(
                Wgs84EquatorialRadius * Deg2Rad(point.X),
                Wgs84MeridianRadius * LatitudeToY(Deg2Rad(point.Y)));
        }

        /// <summary>
        /// Convert Universal Transverse Mercator (Northing / Easting) to Geographic Coordiante (lat/long) (WGS84)
        /// </summary>
        /// <param name="location">Coordiante</param>
        /// <returns>Coordinates (Latitude,Longitude)</returns>
        public static Coordinate UtmToGeo(Point point)
        {
            return new Coordinate(
                Rad2Deg(YToLatitude(point.Y / Wgs84MeridianRadius)),
                Rad2Deg(point.X) / Wgs84EquatorialRadius);
        }

        /// <summary>
        /// Convert Universal Transverse Mercator (Northing / Easting) to Geographic Coordiante (lat/long) (WGS84)
        /// </summary>
        /// <param name="location">Coordiante</param>
        /// <returns>Point (X,Y)</returns>
        public static Point UtmToGeoPoint(Point point)
        {
            return new Point(
                Rad2Deg(point.X) / Wgs84EquatorialRadius,
                Rad2Deg(YToLatitude(point.Y / Wgs84MeridianRadius))
            );
        }

        static double LatitudeToY(double latitudeRad)
        {
            if (latitudeRad <= -90d) return double.NegativeInfinity;
            if (latitudeRad >= 90d) return double.PositiveInfinity;
            return Math.Log(Math.Tan((Math.PI / 4d) + latitudeRad / 2d));
        }

        static double YToLatitude(double yRad)
        {
            return 2d * Math.Atan(Math.Exp(yRad)) - Math.PI / 2d;
        }


        /*********** AlbersProjection ***********/
        public static Point AlbersProjection(double centralMeridian, double standardParallel1, double standardParallel2, Coordinate loc, double scale = 7500000d)
        {

            // Coordinate di riferimento per Genova
            //double centralMeridian = 8.9463f; // Longitudine di riferimento
            //double standardParallel1 = 44f; // Primo parallelo standard
            //double standardParallel2 = 45f; // Secondo parallelo standard

            // Conversione da gradi a radianti
            double latRad = Deg2Rad(loc.Latitude);
            double longRad = Deg2Rad(loc.Longitude);
            double centralMeridianRad = Deg2Rad(centralMeridian);
            double stdParallel1Rad = Deg2Rad(standardParallel1);
            double stdParallel2Rad = Deg2Rad(standardParallel2);

            // Parametri per la proiezione di Albers
            double n = (Math.Sin(stdParallel1Rad) + Math.Sin(stdParallel2Rad)) / 2f;
            double c = Math.Pow(Math.Cos(stdParallel1Rad), 2f) + 2f * n * Math.Sin(stdParallel1Rad);
            double rho0 = Math.Sqrt(c - 2f * n * Math.Sin(stdParallel1Rad)) / n;

            // Calcolo della proiezione di Albers
            double rho = Math.Sqrt(c - 2f * n * Math.Sin(latRad)) / n;
            double theta = n * (longRad - centralMeridianRad);

            double x = scale * (rho * Math.Sin(theta));
            double z = scale * (rho0 - rho * Math.Cos(theta));

            return new Point(x, z);
        }
    }
}

/*

 Ellipsoid model constants (actual values here are for WGS84) 
sm_a = 6378137.0 
sm_b = 6356752.314

def projLatLonToWorldMercator(lat,lon,isDeg=False):
    """
    LatLonToWorldMercator

     Converts a latitude/longitude pair to x and y coordinates in the
     World Mercator projection.

     Inputs:
       lat   - Latitude of the point.
       lon   - Longitude of the point.
       isDeg - Whether the given latitude and longitude are in degrees. If False 
               (default) it is assumed they are in radians.

     Returns:
       x,y - A 2-element tuple with the World Mercator x and y values.

    """     
    lon0 = 0
    if isDeg:
        lat = projDegToRad(lat)
        lon = projDegToRad(lon)

    x = sm_a*(lon-lon0)
    y = sm_a*math.log((math.sin(lat)+1)/math.cos(lat))

    return  x,y 

    def projDegToRad(deg):
    return (deg / 180.0 * pi)

def projRadToDeg (rad):
    return (rad / pi * 180.0)
*/