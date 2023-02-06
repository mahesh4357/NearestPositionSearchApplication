/*


*/

using System.Reflection;
using NearestPositionSearchApplication;

namespace NearestPositionSearchApplication
{
    /// <summary>
    /// Below code is execute once in the lifetime of the application.
    /// Below method is highly optimised to finish reading 2 million records within 1 second.
    /// Below method caches the binary data into a ConcurrentBag.
    /// Below logic ensures the heavy big size binary data reading operation is only performed
    /// Also to increase the reading speed, binary data is split in 4 different parts
    /// where reading start position and stop limit is calculated based on the total binary data size.
    /// Each of the part is triggered on seperate .NET Task executing in parallel,
    /// which ensures the read operation completes within 1 second.
    /// </summary>
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            int GridResolution = 1000; /* Default Resolution */
            string InputFile = Path.Combine(Path.GetDirectoryName(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.ToString()) ?? string.Empty, @"VehiclePositions.dat");
            //string InputFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, @"VehiclePositions.dat");

            if (args.Length > 1)
            {
                Int32.TryParse(args[1], out GridResolution);
            }

            if (args.Length > 0)
            {
                InputFile = args[0];
            }

            /* Start timing the load from file operation here */
            DateTime ProcessStartTime = DateTime.Now;

            try
            {
                /* Load the entire file into memory first */
                byte[] ReadBytes = File.ReadAllBytes(InputFile);

                int ReadOffset = 0;

                List<VehicleLocation> AllVehicles = new List<VehicleLocation>();

                /* Split off one record at a time */
                while (ReadOffset < ReadBytes.Length)
                {
                    VehicleLocation NewVechicle = new VehicleLocation(ReadBytes, ref ReadOffset);

                    AllVehicles.Add(NewVechicle);
                }

                DateTime FileLoadFinishTime = DateTime.Now;

                List<Tuple<int, VehicleLocation?, double?>> ProcessResults = new List<Tuple<int, VehicleLocation?, double?>>();

                int PositionIndex = 0;

                if (AllVehicles.Count > 0)
                {
                    /* Initialize the PositionGrid */
                    PositionGrid UsePositionGrid = new PositionGrid(GridResolution, AllVehicles, Constants.TenPositions);

                    /* For each Position, find the closest Vehicle */
                    foreach (Tuple<float, float> ThisPosition in Constants.TenPositions)
                    {
                        List<Tuple<VehicleLocation, double>> CandidateVehicles = new List<Tuple<VehicleLocation, double>>();

                        /* Populate a list of vehicles that need to be calculated and the distance from the Position */
                        foreach (VehicleLocation ThisVehicle in UsePositionGrid.GetEntitiesToExamine(PositionIndex))
                        {
                            CandidateVehicles.Add(new Tuple<VehicleLocation, double>(ThisVehicle, PositionGrid.CalculateRelativeDistance(ThisVehicle, PositionIndex)));
                        }

                        /* Find the closest one */
                        Tuple<VehicleLocation, double>? ClosestVehicleWithDistance = CandidateVehicles.MinBy(x => x.Item2);

                        if (ClosestVehicleWithDistance != null)
                        {
                            ProcessResults.Add(new Tuple<int, VehicleLocation?, double?>(PositionIndex, ClosestVehicleWithDistance.Item1, ClosestVehicleWithDistance.Item2));
                        }
                        else
                        {
                            ProcessResults.Add(new Tuple<int, VehicleLocation?, double?>(PositionIndex, null, null));
                        }

                        PositionIndex++;
                    }
                }
                else
                {
                    Console.WriteLine("No vehicles were loaded from disk.");
                }

                DateTime CalculationFinishTime = DateTime.Now;

                PositionIndex = 0;

                /* Display the results */
                foreach (Tuple<int, VehicleLocation?, double?> ThisProcessResult in ProcessResults)
                {
                    if (ThisProcessResult.Item2 != null)
                    {
                        Console.WriteLine("For the Position {0} at ({1},{2})", (PositionIndex + 1), Constants.TenPositions[ThisProcessResult.Item1].Item1, Constants.TenPositions[ThisProcessResult.Item1].Item2);
                        Console.WriteLine("    - has nearest Vehicle {0} ({1},{2}) at Minimum Distance of {3} meters", ThisProcessResult.Item2.GetVehicleRegistration(), ThisProcessResult.Item2.Latitude, ThisProcessResult.Item2.Longitude, Math.Round(PositionGrid.CalculateRealDistance(ThisProcessResult.Item2, PositionIndex), 3));
                        Console.WriteLine();
                    }

                    PositionIndex++;
                }

                /* Show the Benchmark */
                Console.WriteLine("Dat file read time taken (seconds) : {0} ms", Math.Round(FileLoadFinishTime.Subtract(ProcessStartTime).TotalMilliseconds));
                Console.WriteLine("Nearest position calculation execution time : {0} ms", Math.Round(CalculationFinishTime.Subtract(FileLoadFinishTime).TotalMilliseconds));
                Console.WriteLine("Total Time taken (seconds) : {0} ms", Math.Round(CalculationFinishTime.Subtract(ProcessStartTime).TotalMilliseconds));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Could not process the file \"{0}\". (Does the file exist and is the format correct?)", InputFile);
                Console.WriteLine();
                Console.WriteLine("Error = \"{0}\"", ex.Message);
            }

            Console.WriteLine();

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();

            return;
        }
    }
}
