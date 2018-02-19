using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

// custom libraries
using Diction;

namespace PreSorting
{
    /// <summary>
    /// format for a translated record
    /// provides methods for translating
    /// from a line to a record 
    /// 
    /// called FinalLine to distinguish from Tube Line
    /// also gives dramatic effect
    /// </summary>
    public class FinalLine
    {
        public int StartStn { get; set; } // column index 2 in source file
        public int EndStn { get; set; } // index 3
        public int StartTime { get; set; } // index 4 (in minutes past midnight)
        public int EndTime { get; set; } // index 5

        const int OriginalColumnNumber = 14; // number of columns in source file
        const int FinalColumnNumber = 4; // number of columns in output file

        public static List<string> RejectedNames { get; private set; } // debugging purposes

        // constructors

        public FinalLine() { }

        public FinalLine(FinalLine f) // creates new instance of class with same attributes as f
        {
            CopyFrom(f);
        }

        // basic methods

        public bool TryInitialise(string line, StationDictionary stations) // reads and converts data form sourceFile
        {
            var cells = line.Split(',');

            for (int i = 0; i < cells.Count(); i++)
            {
                cells[i] = cells[i].Trim('"');
            }

            if(cells.Count() < 8)
            {
                return false; // avoids index out of array
            }

            string start, end;
            start = cells[3];
            end = cells[4];

            if (cells[2].Length >= 3)
            {
                if (cells[2].Substring(0, 3) == "LUL") // logic checks, can also be LUL/NR or LUL/DLR
                {
                    if (start != "Unstarted") // more
                    {
                        if (end != "Unfinished") // more
                        {
                            StartStn = stations.GetKey(start);
                            EndStn = stations.GetKey(end);
                            StartTime = Convert.ToInt16(cells[5]);
                            EndTime = Convert.ToInt16(cells[7]);

                            if (StartStn == -1)
                            {
                                return false;
                            }

                            if (EndStn == -1)
                            {
                                return false;
                            }

                            if (StartStn == EndStn)
                            {
                                return false;
                            }

                            return true; // operation succeeded
                        }
                    }
                }
            }

            return false; // operation failed, must repeat
        }

        public void WriteLine(StreamWriter w) // writes contents to specified file
        {
            string Line;
            Line = StartStn + "," + EndStn + "," + StartTime + "," + EndTime;
            w.WriteLine(Line);
        }

        public bool ComesBefore(FinalLine f) // returns true if this FinalLine comes before f according to ordering
        {
            // if result is false => swap

            if (this.StartStn < f.StartStn) // primary ordering
            {
                return true;
            }

            else if (this.StartStn > f.StartStn)
            {
                return false;
            }

            else // start stations are the same
            {
                if (this.EndStn < f.EndStn) // secondary ordering
                {
                    return true;
                }

                else if (this.EndStn > f.EndStn)
                {
                    return false;
                }

                else // end stations also the same
                {
                    if (this.StartTime <= f.StartTime) // tertiary ordering
                    {
                        return true;
                    }

                    else
                    {
                        return false;
                    }
                }
            }
        }

        public void CopyFrom(FinalLine f) // avoids aliasing problems => copies values form f to this
        {
            this.StartStn = f.StartStn;
            this.EndStn = f.EndStn;
            this.StartTime = f.StartTime;
            this.EndTime = f.EndTime;
        }

        // debugging methods

        private void AddOn(string s)
        {
            if (!RejectedNames.Contains(s))
            {
                RejectedNames.Add(s);
            }
        }

        private void Output(string[] cells)
        {
            foreach (var cell in cells)
            {
                Console.Write(cell + " | ");
            }
            Console.WriteLine();
        }

        public void OutputReadData()
        {
            Console.WriteLine(StartStn + " | " + EndStn + " | " + StartTime + " | " + EndTime);
        }

        public void ReadLine(string line) // reads data form sorted file
        {
            string[] cells = line.Split(',');

            if (cells.Length == 4)
            {
                StartStn = Convert.ToInt16(cells[0]);
                EndStn = Convert.ToInt16(cells[1]);
                StartTime = Convert.ToInt16(cells[2]);
                EndTime = Convert.ToInt16(cells[3]);
            }
        }
    }
}
