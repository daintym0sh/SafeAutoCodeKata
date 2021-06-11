using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace DriverUtility
{
    public class Reporting
    {
        public static bool createReport(String inputPath, String outputPath)
        {
            try
            {
                Dictionary<string, List<TripData>> trips = new Dictionary<string, List<TripData>>();

                foreach (string line in File.ReadAllLines(inputPath))
                {
                    if (String.IsNullOrWhiteSpace(line))
                        continue;

                    string[] entry = Regex.Split(line, "\\s+");

                    if (entry.Length < 2)
                        continue;

                    string command = entry[0];

                    addDriver(trips, entry);

                    if(command == "Trip" && trips.ContainsKey(entry[1]))
                    {
                        addTrip(trips, entry);
                    }
                }

                if (trips.Count == 0) return false;

                List<DriverData> driverDatas = createDriverData(trips);

                if (driverDatas.Count == 0) return false;

                String reportData = createReportData(driverDatas);
                String fileName = $"SafeAutoReport-{String.Format("{0:s}", DateTime.Now).Replace(":", "-")}.txt";
                File.WriteAllText(Path.Combine(outputPath, fileName), reportData);

                return true;
            }
            catch(Exception e)
            {
                Console.WriteLine("Unable to create report\n" + e.Message);
                return false;
            }
        }

        private static string createReportData(List<DriverData> driverDatas)
        {
            StringBuilder stringBuilder = new StringBuilder();

            foreach(DriverData driverData in driverDatas)
            {
                if(driverData.Distance > 0)
                {
                    stringBuilder.AppendLine($"{driverData.Name}: {driverData.Distance} miles @ {driverData.Speed} mph");
                }
                else
                {
                    stringBuilder.AppendLine($"{driverData.Name}: {driverData.Distance} miles");
                }
            }

            return stringBuilder.ToString();
        }

        private static List<DriverData> createDriverData(Dictionary<string, List<TripData>> trips)
        {
            List<DriverData> driverDatas = new List<DriverData>();

            foreach(KeyValuePair<string, List<TripData>> trip in trips)
            {
                string name = trip.Key;
                List<TripData> tripDatas = trip.Value;

                if (tripDatas.Count == 0)
                {
                    driverDatas.Add(
                        new DriverData(
                            name,
                            0,
                            0)
                        );
                }
                else
                {
                    double totalDistance = 0;
                    double totalTimeMinutes = 0;

                    foreach (TripData tripData in tripDatas)
                    {
                        totalDistance += tripData.Distance;
                        totalTimeMinutes += tripData.TripLength.TotalMinutes;
                    }

                    int speed = Convert.ToInt32(totalDistance / (totalTimeMinutes / 60));
                    int distance = Convert.ToInt32(totalDistance);

                    driverDatas.Add(
                        new DriverData(
                            name,
                            distance,
                            speed)
                        );
                }
            }

            driverDatas.Sort();

            return driverDatas;
        } 

        private static void addDriver(Dictionary<string, List<TripData>> trips, string[] entry)
        {
            string driver = entry[1];

            if (!trips.ContainsKey(driver))
                trips.Add(driver, new List<TripData>());
        }

        private static void addTrip(Dictionary<string, List<TripData>> trips, string[] data)
        {
            if (data.Length < 5)
                return;

            bool parseSuccess = false;

            double distance = 0;
            parseSuccess = Double.TryParse(data[4], out distance);

            DateTime start, end;
            parseSuccess = DateTime.TryParse(data[2], out start);
            parseSuccess = DateTime.TryParse(data[3], out end);

            if (!parseSuccess && DateTime.Compare(start, end) >= 0)
                return;

            TripData tripData = new TripData(end - start, distance);

            trips[data[1]].Add(tripData);
        }
    }

    class TripData
    {

        public TimeSpan TripLength;
        public double Distance;

        public TripData(TimeSpan tripLength, double distance)
        {
            TripLength = tripLength;
            Distance = distance;
        }
    }

    class DriverData : IComparable<DriverData>
    {
        public string Name;
        public int Distance;
        public int Speed;

        public DriverData(string name, int distance, int speed)
        {
            Name = name;
            Distance = distance;
            Speed = speed;
        }

        public int CompareTo(DriverData other)
        {
            if (Distance > other.Distance)
                return -1;
            if (Distance == other.Distance)
                return 0;

            return 1;
        }
    }
}
