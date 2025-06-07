using BFS_Intersection;
using System.Diagnostics;

string dbPathTemplate = "C:\\Users\\User\\source\\repos\\BITIRP\\Datasets\\{0}\\{0}.csv";
string runResultTemplate = "BITIRP,{0},{1},{2},{3},{4}";

string OUTPUT_PATH = "C:\\Users\\User\\source\\repos\\BITIRP\\Results\\bitirp_results.csv";

Console.WriteLine("Starting BITIRP runs!");

var dbMaxGap = new Dictionary<string, int[]>()
{
    {"diabetes", new int[]{30} },
    {"smarthome", new int[]{30} },
    {"aslgt", new int[]{Int32.MaxValue} },
    {"context", new int[]{30} },
    {"pioneer", new int[]{30} },
    {"hepatitis", new int[]{Int32.MaxValue} },
};

var dbVerticalSupport = new Dictionary<string, double[]>
{
    {"diabetes", new double[]{60, 55, 50, 45, 40, 35} },
    {"smarthome", new double[]{60, 55, 50, 45, 40, 35} },
    {"aslgt", new double[]{30, 25, 20, 15, 10} },
    {"context", new double[]{60, 55, 50, 45, 40, 35} },
    {"pioneer", new double[]{30, 25, 20, 15, 10} },
    {"hepatitis", new double[]{60, 55, 50, 45, 40, 35} },
};

string[] dbNames = new string[]{
    "pioneer",
    "aslgt",
    "context",
    "hepatitis",
    "diabetes",
    "smarthome",
};

foreach (string dbName in dbNames)
{
    foreach (int maxGap in dbMaxGap[dbName])
    {
        Console.WriteLine("Entering runDataset {0}, {1}", dbName, maxGap);
        runDataset(dbName, maxGap);
    }
}

void runDataset(string dbName, int maxGap)
{
    int maxDepth = 100;
    string dbPath = string.Format(dbPathTemplate, dbName);

    double[] verticalSupportValues = dbVerticalSupport[dbName];

    foreach (double verticalSupport in verticalSupportValues)
    {
        EntitiesDb db = EntitiesDbLoader.Load(dbPath);
        int numOfEntities = db.Entities.Count;

        int minSupport = (int)Math.Ceiling(0.01 * verticalSupport * numOfEntities);
        Console.WriteLine("Vertical Support {0}%, Number of Entities {1} => Min Support {2} | MaxGap {3}", verticalSupport, numOfEntities, minSupport, maxGap);

        var timer = new Stopwatch();
        long startAllocated = GC.GetAllocatedBytesForCurrentThread();

        timer.Start();
        Miner miner = new Miner(db, db.GetSymbols(), maxDepth, minSupport, maxGap);
        miner.Run();
        timer.Stop();

        // Give the monitor a little time to catch the final high point
        Thread.Sleep(1000);

        long endAllocated = GC.GetAllocatedBytesForCurrentThread();
        long totalAllocatedIncludingGC = endAllocated - startAllocated;


        Console.WriteLine(timer.ElapsedMilliseconds);
        using (StreamWriter writer = new StreamWriter(OUTPUT_PATH, true))
        {
            writer.WriteLine(string.Format(runResultTemplate, dbName, maxGap, verticalSupport, timer.ElapsedMilliseconds, $"{totalAllocatedIncludingGC / (1024 * 1024.0):F2} MB"));
        }
    }
}