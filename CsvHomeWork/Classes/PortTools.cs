using System.Reflection.Metadata;

namespace CsvHomeWork.Classes;


/* PortTools is a class that contains information overall ports
 it is used to save and process data*/
public class PortTools 
{
    private string _filepath;

    private PortInfo[] _allPorts;
    
    // structure of file, contains colum names for future validation
    private static readonly List<String> ColumNames = ("id,Country,Port Name,UN Code,Vessels in Port," +
                                                        "Departures(Last 24 Hours),Arrivals(Last 24 Hours),Expected Arrivals,Type" +
                                                        ",Area Local,Area Global,Also known as")
        .Split(",").ToList()
        .Select(x => x.Trim()).ToList();
    public PortTools(string[] args) // working with input file
    {
        if (args.Length == 0) throw new Exception("File name or path cant be empty.");
        if (args.Length > 1) throw new Exception("Command load has only one argument.");

        DirectoryInfo di = new DirectoryInfo(Directory.GetCurrentDirectory());
        var fileNames = di.GetFiles().ToList().Select(v => v.Name); // all files in current directory

        if (fileNames.Contains(args[0])) // if current directory contains file with given name
            _filepath = Path.Combine(di.FullName, args[0]);
        else if (File.Exists(args[0])) // if path to file is valid
            _filepath = args[0];
        else // unable to load data
            throw new Exception("Unable to find file. Please check filename of path.");
        
        
        // validate the file, check that it is readable and the data is in necessary form
        using (StreamReader sr = new StreamReader(_filepath))
        {
            string s = sr.ReadLine();
            var colNames = s.Split(",").ToList()
                .Select(x => x.Trim()).ToList();
            if (colNames != ColumNames)
            {
                if (colNames.Count != ColumNames.Count) // the number of columns is different
                    throw new ArgumentException("File structure is not valid." +
                                                $"Expected {ColumNames.Count} columns, found {colNames.Count}.");
                
                for (int i = 0; i < colNames.Count; ++i) // checking that columns names are equal
                    if (colNames[i] != ColumNames[i] && i != 0) 
                        throw new ArgumentException("File structure is not valid." +
                                                    $"Expected {ColumNames[i]} column, found {colNames[i]}.");
            }
        }
        
        // everything is ok, file is correct and readable
        var file = new FileInfo(_filepath);
        LoadData();
        Console.WriteLine($"Successfully loaded file {file.Name}");
    }

    // loads Ports from file
    private void LoadData()
    {
        var pt = new List<PortInfo>();

        using (StreamReader sr = new StreamReader(_filepath))
        {
            for (int i = 0; !sr.EndOfStream; ++i)
            {
                string s = sr.ReadLine();
                if (i == 0) continue; // first line colum names, already validated
                PortInfo? port = Parser.PasePortInfo(s);
                if (port is not null) pt.Add(port);
            }
        }

        _allPorts = pt.ToArray();
    }

    // 3-d operation
    private void PrintCountries()
    {
        var allPortsSorted = new List<PortInfo>();
        for (int i = 0; i < _allPorts.Length; ++i)
            allPortsSorted.Add(_allPorts[i]);

        // sort Ports by country name
        allPortsSorted.Sort((x, y) => (String.Compare(x.country, y.country))); 
        var allPortsSortedAr = allPortsSorted.ToArray();
        
        int maxV = Int32.MinValue;
        var curP = new List<PortInfo>();

        for (int i = 0; i < allPortsSortedAr.Length; ++i)
        {
            curP.Add(allPortsSortedAr[i]);
            if (allPortsSortedAr[i].vesselsUnPort is not null)
                maxV = Math.Max(maxV, allPortsSortedAr[i].vesselsUnPort.Value);
            if ((i + 1 < allPortsSortedAr.Length && allPortsSortedAr[i + 1].country != allPortsSortedAr[i].country) ||
                i == allPortsSortedAr.Length - 1)
            {
                string maxVV = Convert.ToString(maxV);
                if (maxV == Int32.MinValue) maxVV = "null";
                
                Console.WriteLine($"{curP[0].country}(max vesselsUnPort {maxVV}).");
                BeautifulOutput.BeautifulPrint(BeautifulOutput.BeautifulPortInfo(curP.ToArray()));
                PrintInFile(BeautifulOutput.BeautifulPortInfo(curP.ToArray()), "Port-Country-" + curP[0].country + ".csv");
                
                maxV = Int32.MinValue;
                curP = new List<PortInfo>();
            }
        }
    }


    // 2-d operaion
    private void PortsAndAnchorages()
    {
        var ports = _allPorts.ToList().Where(x => x.type == PortTypes.Port).ToArray();
        var anchorages = _allPorts.ToList().Where(x => x.type == PortTypes.Anchorage).ToArray();
        
        Console.WriteLine("Ports:");
        BeautifulOutput.BeautifulPrint(BeautifulOutput.BeautifulPortInfo(ports));
        PrintInFile(BeautifulOutput.BeautifulPortInfo(ports), "Port-Port.csv");
        
        Console.WriteLine("Anchorages:");
        BeautifulOutput.BeautifulPrint(BeautifulOutput.BeautifulPortInfo(anchorages));
        PrintInFile(BeautifulOutput.BeautifulPortInfo(anchorages), "Port-Anchorage.csv");
    }
    
    
    private void PrintData(PortInfo[] arr) // for debug only
    {
        for (int i = 0; i < arr.Length; ++i) Console.WriteLine(arr[i]);
    }
    
    public void Print(string[] args) // function for invoking other functions depending on arguments
    {
        if (args.Length == 0) throw new Exception("Print must have one argument.");
        if (args.Length > 1) throw new Exception("Print has only one argument.");

        if (args[0] == "statistics") this.PrintStatistics();
        else if (args[0] == "departures") this.PrintDepartures();
        else if (args[0] == "countries") this.PrintCountries();
        else if (args[0] == "ports-and-anchorages") this.PortsAndAnchorages();
        else throw new Exception($"Cant print {args[0]}. Argument not found.");
    }


    private int GetMaxDepartures() // gets max departures through all ports
    {
        int ans = int.MinValue;
        foreach (var v in _allPorts)
            if (v.departures is not null)
                ans = Math.Max(ans, v.departures.Value);
        return ans;
    }
    private void PrintDepartures() // 4-th operaion
    {
        int maxDep = this.GetMaxDepartures();
        var listOfDep = new List<PortInfo>();
        
        if (maxDep != int.MinValue) {
            foreach (var v in _allPorts)
            {
                if (v.departures is null || v.type != PortTypes.Port) continue;
                if (maxDep - v.departures.Value <= 10) listOfDep.Add(v);
            }
        }

        var beutOut = BeautifulOutput.BeautifulPortInfo(listOfDep.ToArray());
        BeautifulOutput.BeautifulPrint(beutOut); // output
        this.PrintInFile(beutOut, "Departures-Port.csv");
    }
    
    private void PrintInFile(string[] arr, string filename) { // prints data in file
        var file = new FileInfo(_filepath); // current file
        var curFilePath = Path.Combine(file.Directory.FullName, filename);
        using (var sr = new StreamWriter(curFilePath))
        {
            foreach (var v in arr) sr.WriteLine(v);
        }
    }
    private void PrintStatistics() // 5-th operation
    { 
        // statistics overall countries 
        
        // step one: number of ports in each country
        var dictCountires = new Dictionary<string, int>();
        foreach (var v in _allPorts)
        {
            if (v.type != PortTypes.Port) continue; // not a port
            if (dictCountires.ContainsKey(v.country))
            {
                dictCountires[v.country] += 1;
            }
            else
            {
                dictCountires[v.country] = 1;
            }
        }

        var sortedCountries = new List<(int, string)>();
        foreach (var v in dictCountires)
        {
            sortedCountries.Add((v.Value, v.Key));
        }
        
        sortedCountries.Sort((x, y) => (x.Item1 == y.Item1 ? 0 : x.Item1 < y.Item1 ? 1 : -1));
        var printC = sortedCountries.Select(x => new string[] { x.Item2, Convert.ToString(x.Item1) }).ToArray();

        string[] beutifulPrint = BeautifulOutput.BeautifulStrings(printC);
        BeautifulOutput.BeautifulPrint(beutifulPrint);
        
        // step 2: min max vessels
        int maxV = int.MinValue, minV = int.MaxValue;
        foreach (var v in _allPorts)
        {
            if (v.type != PortTypes.Port || v.vesselsUnPort is null) continue;
            maxV = Math.Max(maxV, v.vesselsUnPort.Value);
            minV = Math.Min(minV, v.vesselsUnPort.Value);
        }
        
        if (minV == int.MaxValue)
            Console.WriteLine($"Min vessels: nan.");
        else
            Console.WriteLine($"Min vessels: {minV}.");
        if (maxV == int.MinValue)
            Console.WriteLine($"Max vessels: nan.");
        else
            Console.WriteLine($"Max vessels: {maxV}.");
        
        // step 3: yellow sea and Bohai sea

        int cntY = 0, cntB = 0;
        foreach (var v in _allPorts)
        {
            if (v.areaLocal == "Yellow Sea") ++cntY;
            if (v.areaLocal == "Bohai Sea") ++cntB;
        }
        
        Console.WriteLine($"Number of objects located in Yellow sea: {cntY}.");
        Console.WriteLine($"Number of objects located in Bohai sea: {cntB}.");
        
        // step 4: China, arrivals < 500.

        int cntChina = 0;
        foreach (var v in _allPorts)
            if (v.country == "China" && v.arrivals is not null && v.arrivals < 500)
                ++cntChina;
        
        Console.WriteLine($"Number of objects with arrival less than 500 in China: {cntChina}.");

    }
}