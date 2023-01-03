using System;
using CsvHomeWork.Classes;

/*
 * All arguments must be given using "-"
 * for instance, here is command to load a file:
 * load -Port_Data.csv
 * or
 * load -C:\Users\andrey\RiderProjects\CvsHomeWork\CsvHomeWork\bin\Debug\net6.0\Port_Data.csv
 *                                              ↑
 *                                         path to file
 * 
 * all output files will be created in data file directory
 */

namespace CsvHomeWork
{
    class Program
    {
        public static void Main(string[] args)
        {
            // program interface for operations
            Console.WriteLine("Enter the number of operation with arguments.\n" +
                              "1. load -[file name of path] : Loads data from given file.\n" +
                              "2. print -ports-and-anchorages : Prints the information about ports and anchorages.\n" +
                              "3. print -countries : Prints the information about ports in all countries.\n" +
                              "4. print -departures : Prints the information about ports with departure not less than MaxDeparture-10.\n" +
                              "5. print -statistics : Prints the statistics overall data.\n" +
                              "6. exit : Stops the program.");

            PortTools? pt = null; // the whole data is here
            
            while (true)
            {
                string s = Console.ReadLine();
                
                try
                {
                    var (command, arguments) = Parser.ParseCommand(s); // parsing command for arguments
                    switch (command)
                    {
                        case "exit":
                            return;
                        case "load":
                            pt = new PortTools(arguments);
                            break;
                        case "print" when pt is null:
                            throw new Exception("File hasnt been loaded yet, cant perform commands on it.");
                        case "print":
                            pt.Print(arguments);
                            break;
                        default:
                            throw new Exception($"Unable to find command {command}.");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }
}