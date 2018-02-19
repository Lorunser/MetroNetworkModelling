using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.ComponentModel;

// custom libraries
using Diction;

namespace PreSorting
{
    /// <summary>
    /// loads, translates and sorts source data file
    /// source data is in csv format
    /// translates station names to ids using StationDictionary
    /// sorts using a merge sort
    /// </summary>
    public class Sorting
    {
        private string OutFile { get; set; } // path for output file
        private string SourceFile { get; set; } // path for source file
        private LinkedList<List<FinalLine>> Records { get; set; } // LinkedList for faster editing

        const int split = 70; // time split between reading / binary searching and sorting
        private BackgroundWorker bgw { get; set; }

        // constructor

        public Sorting(string source, string output = "data.txt")
        {
            SourceFile = source;
            OutFile = output;
            Records = new LinkedList<List<FinalLine>>();
        }

        // MergeSort methods

        public int Sort(StationDictionary stations, BackgroundWorker backgroundWorker) // begins sorting data on request
        {
            this.bgw = backgroundWorker;
            int N = 0;

            if (SourceFile.Substring(SourceFile.LastIndexOf('.')) == ".csv")
            {
                N = FirstPass(stations);
                ListMergeSort();
                WriteFile();
            }

            else // pre sorted txt file
            {
                N = LoadPreSortedData();
            }

            return N;
        }

        private int FirstPass(StationDictionary stations) // reads in data and converts to numerical form
        {
            StreamReader r = new StreamReader(SourceFile);
            FinalLine f = new FinalLine();

            FileInfo fi = new FileInfo(SourceFile);
            long size = fi.Length; //bytes in file          

            long bytesRead = 0;
            int percent = 0;

            string line;

            const int bytesPerChar = 1; // UTF-8

            while (!r.EndOfStream)
            {
                line = r.ReadLine();
                if (f.TryInitialise(line, stations))
                {
                    Records.AddFirst(new List<FinalLine> { new FinalLine(f) });
                }

                bytesRead += (line.Length + 2) * bytesPerChar; // +2 to account for return and new line

                if (percent < (bytesRead * split) / size)
                {
                    percent++;
                    bgw.ReportProgress(percent);
                }
            }

            r.Close();
            return Records.Count();
        }

        private void ListMergeSort() // merges until only 1 list left
        {
            int totalRounds = Records.Count();
            int round = totalRounds;
            int percent = split;
            int oldPercent = split;

            double logRounds = Math.Log(totalRounds);

            while (Records.Count > 1)
            {
                MergeFirstTwo();
                round--;

                percent = (int) ((split * Math.Log(totalRounds / round)) / logRounds) + split;
                if (oldPercent < percent)
                {
                    oldPercent = percent;
                    bgw.ReportProgress(percent);
                }
            }
        }

        private void MergeFirstTwo() // merges first two lists and puts result at back
        {
            List<FinalLine> aList = Records.First();
            List<FinalLine> bList = Records.ElementAt(1);
            List<FinalLine> merged = new List<FinalLine>();

            FinalLine aLine = new FinalLine();
            FinalLine bLine = new FinalLine();
            int a = 0;
            int b = 0;
            bool ongoing = true;

            aLine = aList[a];
            a++;
            bLine = bList[b];
            b++;

            while (ongoing)
            {
                if (aLine.ComesBefore(bLine))
                {
                    merged.Add(new FinalLine(aLine));

                    if (a < aList.Count())
                    {
                        aLine = aList[a];
                        a++;
                    }

                    else
                    {
                        ongoing = false;
                        merged.Add(new FinalLine(bLine));
                        for (int i = b; i < bList.Count(); i++)
                        {
                            merged.Add(new FinalLine(bList[i]));
                        }
                    }
                }

                else
                {
                    merged.Add(new FinalLine(bLine));

                    if (b < bList.Count())
                    {
                        bLine = bList[b];
                        b++;
                    }

                    else
                    {
                        ongoing = false;
                        merged.Add(new FinalLine(aLine));
                        for (int i = a; i < aList.Count(); i++)
                        {
                            merged.Add(new FinalLine(aList[i]));
                        }
                    }
                }
            }

            Records.RemoveFirst(); // remove first two lists
            Records.RemoveFirst();

            Records.AddLast(merged); // add merged result to back
        }

        private void WriteFile() // writes sorted data to text file
        {
            StreamWriter w = new StreamWriter(OutFile);
            foreach (var line in Records.First())
            {
                line.WriteLine(w);
            }
            w.Close();
        }

        // data returning methods

        public List<FinalLine> GetRecordsList() // returns sorted list of records for processing
        {
            return Records.First();
        }

        // Mocking methods

        private int LoadPreSortedData() // speeds up debugging by not having to re sort file
        {
            FinalLine f = new FinalLine();
            StreamReader r = new StreamReader(SourceFile);
            List<FinalLine> sorted = new List<FinalLine>();

            FileInfo fi = new FileInfo(SourceFile);
            long size = fi.Length; // size in bits
            long bytesRead = 0;
            int percent = 0;
            const int bytesPerChar = 1; //ASCII
            string line;

            while (!r.EndOfStream)
            {
                line = r.ReadLine();
                f.ReadLine(line);
                sorted.Add(new FinalLine(f));
                bytesRead += (line.Length + 2) * bytesPerChar; // +2 to account for return char and new line

                if (percent < bytesRead * 100 / size)
                {
                    percent++;
                    bgw.ReportProgress(percent);
                }
            }

            r.Close();
            Records.AddFirst(sorted);
            return Records.First().Count();
        }
    }
}
