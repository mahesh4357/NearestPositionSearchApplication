using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NearestPositionSearchApplication
{
    public class PositionGrid
    {
        private double GridCellWidth;
        private double GridCellHeight;

        private float MinLatitude;
        private float MaxLatitude;

        private float MinLongitude;
        private float MaxLongitude;

        private int GridResolution;

        /* Map a vehicle to its X,Y coordinate on the bitmap */
        private Dictionary<VehicleLocation, Tuple<int, int>> PositionMapping;

        public Tuple<int, int> GetGridPos(float LatitudeParam, float LongitudeParam)
        {
            /* Ensure our grid is neat and all indexes are zero-based positive integers */
            return new Tuple<int, int>((int)Math.Round((LatitudeParam - ((MinLatitude > 0) ? -MinLatitude : MinLatitude)) / GridCellWidth),
                                       (int)Math.Round((LongitudeParam - ((MinLongitude > 0) ? -MinLongitude : MinLongitude)) / GridCellHeight));
        }

        public PositionGrid(int GridResolutionParam, List<VehicleLocation> VehicleLocationParam, Tuple<float, float>[] PositionListParam)
        {
            /* Get the bounds for the values we are working with */
            MinLatitude = Math.Min(VehicleLocationParam.Min(x => x.Latitude), PositionListParam.Min(x => x.Item1));
            MaxLatitude = Math.Max(VehicleLocationParam.Max(x => x.Latitude), PositionListParam.Max(x => x.Item1));
            MinLongitude = Math.Min(VehicleLocationParam.Min(x => x.Longitude), PositionListParam.Min(x => x.Item2));
            MaxLongitude = Math.Max(VehicleLocationParam.Max(x => x.Longitude), PositionListParam.Max(x => x.Item2));

            GridResolution = GridResolutionParam;

            /* Determine the size of each cell by dividing the span by the given resolution */
            GridCellWidth = (Math.Abs(MaxLatitude - ((MinLatitude > 0) ? -MinLatitude : MinLatitude)) / GridResolution);
            GridCellHeight = (Math.Abs(MaxLongitude - ((MinLongitude > 0) ? -MinLongitude : MinLongitude)) / GridResolution);

            PositionMapping = new Dictionary<VehicleLocation, Tuple<int, int>>();

            /* For each vehicle, determine its location on our bitmap */
            foreach (VehicleLocation ThisVehicleLocation in VehicleLocationParam)
            {
                PositionMapping.Add(ThisVehicleLocation, GetGridPos(ThisVehicleLocation.Latitude, ThisVehicleLocation.Longitude));
            }
        }

        /* Return a list of vehicles that are in the same cell on our bitmap for a given Lat,Long as well as additional vehicles in surrounding cells */
        public List<VehicleLocation> GetEntitiesToExamine(int PositionParam)
        {
            List<VehicleLocation> TempResult = new List<VehicleLocation>();

            Tuple<int, int> PositionPosition = GetGridPos(Constants.TenPositions[PositionParam].Item1, Constants.TenPositions[PositionParam].Item2);

            /* Always add items in its own block and two extra layer, if they exist */
            int Counter = 2;

            do
            {
                /* Select Vehicles from adjacent cells */
                TempResult.AddRange(PositionMapping.Where(x => (x.Value.Item1 > PositionPosition.Item1 - Counter) &&
                                                               (x.Value.Item1 < PositionPosition.Item1 + Counter) &&

                                                               (x.Value.Item2 > PositionPosition.Item2 - Counter) &&
                                                               (x.Value.Item2 < PositionPosition.Item2 + Counter)).Select(y => y.Key));

                /* Prevent an infinite loop if we fall out of the grid */
                if (((PositionPosition.Item1 - Counter) < 0) &&
                    ((PositionPosition.Item1 + Counter) > GridResolution) &&
                    ((PositionPosition.Item2 - Counter) < 0) &&
                    ((PositionPosition.Item2 + Counter) > GridResolution)) break;

                Counter *= 2; /* Double the size of our search each time we don't find anything */
            } while (TempResult.Count == 0); /* Continue looking until we have found some items, or exceed the bounds of the grid */

            return TempResult;
        }

        /* Distance calculation functions here... */
        public static double CalculateRealDistance(VehicleLocation VehicleParam, int PositionIndexParam)
        {
            return CalculateRealDistance(VehicleParam.Latitude, VehicleParam.Longitude, Constants.TenPositions[PositionIndexParam].Item1, Constants.TenPositions[PositionIndexParam].Item2);
        }

        static double CalculateRealDistance(float Lat1Param, float Long1Param, float Lat2Param, float Long2Param)
        {
            /* https://www.geeksforgeeks.org/program-distance-two-points-earth/ */

            double Lat1Radians = Lat1Param * Math.PI / 180;
            double Lat2Radians = Lat2Param * Math.PI / 180;

            double DistanceCalc = (Math.Acos(Math.Sin(Lat1Radians) * Math.Sin(Lat2Radians) + Math.Cos(Lat1Radians) * Math.Cos(Lat2Radians) * Math.Cos(Math.PI * (Long1Param - Long2Param) / 180))) * 180 / Math.PI * 60 * 1.1515 * 1609.344 /* Meters */;

            return DistanceCalc; /* Distance in meters */
        }

        public static double CalculateRelativeDistance(VehicleLocation VehicleParam, int PositionIndexParam)
        {
            return CalculateRelativeDistance(VehicleParam.Latitude, VehicleParam.Longitude, Constants.TenPositions[PositionIndexParam].Item1, Constants.TenPositions[PositionIndexParam].Item2);
        }

        static double CalculateRelativeDistance(float Lat1Param, float Long1Param, float Lat2Param, float Long2Param)
        {
            /* Using Pythagoras to get a relative distance. Faster than calculating real units here. Useful only for comparison. */
            return Math.Sqrt(Math.Pow((double)(Lat1Param) - (double)(Lat2Param), 2) + Math.Pow((double)(Long1Param) - (double)(Long2Param), 2));
        }
    }


    /// <summary>
    /// Provided assessment input data here prepares by main function.
    /// 10 given positions is added and passed to the method that uses FindNearestVehicle.
    /// </summary>
    /// <param name="args">args</param>
    public static class Constants
    {
        /* The Positions (Positions) Hard Coded as provided. Since it is hard-coded, I could cheat and sort the list manually,
           saving some CPU, but I did not do this. */

        public static Tuple<float, float>[] TenPositions = new Tuple<float, float>[]
        {
            new Tuple<float, float>((float)34.544909, (float)-102.100843),
            new Tuple<float, float>((float)32.345544, (float)-99.123124),
            new Tuple<float, float>((float)33.234235, (float)-100.214124),
            new Tuple<float, float>((float)35.195739, (float)-95.348899),
            new Tuple<float, float>((float)31.895839, (float)-97.789573),
            new Tuple<float, float>((float)32.895839, (float)-101.789573),
            new Tuple<float, float>((float)34.115839, (float)-100.225732),
            new Tuple<float, float>((float)32.335839, (float)-99.992232),
            new Tuple<float, float>((float)33.535339, (float)-94.792232),
            new Tuple<float, float>((float)32.234235, (float)-100.222222)
        };
    }
}

