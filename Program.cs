using Newtonsoft.Json;

namespace MinecraftPlaytimeSummarizer
{
    internal class Program
    {
        const string savesPath = @"C:\Users\User\AppData\Roaming\.minecraft\saves";
        const string mcUUID = "your_uuid_here";

        //stats -> minecraft:custom -> minecraft:playtime (rep in ticks)
        //all located in world/stats/uuid.json
        //OR newer: world/players/stats/uuid.json

        static void Main(string[] args)
        {
            //real stats
            long totalPlaytimeTicks = 0;
            long totalDeaths = 0;
            long totalMobKills = 0;
            long totalBlocksBroken = 0;

            //other stats
            int worldCount = 0;

            //oldest world
            string oldestWorld = "";
            DateTime oldestWorldCT = DateTime.Now;

            foreach (string dir in Directory.GetDirectories(savesPath))
            {
                string json = "";
                if (Directory.Exists(Path.Combine(dir, "stats")))
                {
                    //older format
                    json = File.ReadAllText(Path.Combine(dir, "stats", mcUUID + ".json"));
                }
                else if (Directory.Exists(Path.Combine(dir, "players", "stats")))
                {
                    //newer format
                    json = File.ReadAllText(Path.Combine(dir, "players", "stats", mcUUID + ".json"));
                }
                else
                {
                    Console.WriteLine("Invalid world! path is " + dir + " (created on " + Directory.GetCreationTime(dir).ToString("MMM dd, yyyy") + ")");
                    continue;
                }

                StatsStructure? str = JsonConvert.DeserializeObject<StatsStructure>(json);

                if (str == null)
                {
                    Console.WriteLine("Couldn't parse stats structure!");
                    continue;
                }

                worldCount++;

                DateTime worldCT = Directory.GetCreationTime(dir).Date;
                if (worldCT < oldestWorldCT)
                {
                    oldestWorldCT = worldCT;
                    oldestWorld = dir;
                }

                if (str.stats.TryGetValue("minecraft:custom", out var value))
                {
                    if (value.TryGetValue("minecraft:play_time", out int ticks))
                        totalPlaytimeTicks += ticks;

                    if (value.TryGetValue("minecraft:deaths", out int deaths))
                        totalDeaths += deaths;

                    if (value.TryGetValue("minecraft:mob_kills", out int kills))
                        totalMobKills += kills;
                }

                if (str.stats.TryGetValue("minecraft:mined", out var mined))
                {
                    foreach (KeyValuePair<string, int> kvp in mined)
                    {
                        totalBlocksBroken += kvp.Value;
                    }
                }
            }

            float mins = totalPlaytimeTicks / 20 / 60;
            float hours = mins / 60;

            Console.WriteLine("");
            Console.WriteLine("Complete! Worlds checked: " + worldCount + " worlds");
            Console.WriteLine($"Oldest world is {oldestWorld} (created on {oldestWorldCT.ToString("MMM dd, yyyy")})");
            Console.WriteLine("------------------------------");

            Console.WriteLine($"MINECRAFT: JAVA EDITION STATISTICS:\n" +
                $"Time Played (hours): {hours} hours\n" +
                $"Mobs Killed: {totalMobKills}\n" +
                $"Player Deaths: {totalDeaths}\n" +
                $"Blocks Broken: {totalBlocksBroken}");            
        }
    }

    class StatsStructure
    {
        public Dictionary<string, Dictionary<string, int>> stats = new();
    }
}
