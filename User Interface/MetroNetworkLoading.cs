using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

// additional imports
using System.ComponentModel;
using System.IO;

// custom libraries
using Diction;
using Routing;
using PreSorting;

namespace User_Interface
{
    /// <summary>
    /// contains loading data for metro network
    /// also draws map (heat and normal) on canvas given to it
    /// </summary>
    public class MetroNetworkLoading
    {
        // network loadings
        private static List<LineRecord> LineRecords { get; set; } // list of lines each containing a set of  records
        private List<RecordLocation> RecLocs { get; set; } // locations of records to increment
        private int NodeNumber { get; set; }

        // dictionaries
        private static StationDictionary StationDiction { get; set; }
        private static LineDictionary LineDiction { get; set; }
        private static Canvas C { get; set; }

        // display
        private StationDisplay StatDisplay { get; set; }
        public int MaxLoading { get; set; }

        // dropped 
        public int DroppedRecords { get; set; }

        // constants
        public const int Interval = 10; // group loadings into 10 minute time intervals

        //Constructor

        public MetroNetworkLoading(LineDictionary lineDiction, StationDictionary stationDiction, Canvas c)
        {
            // assigning static variables
            C = c;
            StationDiction = stationDiction;
            LineDiction = lineDiction;

            // instantiating variables
            LineRecords = new List<LineRecord>();
            RecLocs = new List<RecordLocation>();

            for (int i = 0; i < LineDiction.N; i++)
            {
                LineRecords.Add(new LineRecord(i)); // intialise record setup
            }

            NodeNumber = StationDiction.N;

            // displays stations
            StatDisplay = new StationDisplay(C);
        }

        // Reading Methods

        public void FillInNetwork(string outFile, Sorting S, BackgroundWorker bgw, List<string> cutEdges) // iterates through data and fills in edge loadings accordingly
        {
            List<FinalLine> data = S.GetRecordsList();
            FinalLine newRec = new FinalLine();
            FinalLine oldRec = new FinalLine();
            PathClass path; // just used as alias
            oldRec = data.First();

            RouteFinding rf = new RouteFinding(NodeNumber);
            rf.SetupFresh(oldRec.StartStn);
            path = rf.Retrace(oldRec.EndStn);
            SetupPath(path);

            //debugging variables
            List<string> droppedStations = new List<string>();
            string dropName;
            // end debugging variables     

            foreach (var record in data)
            {
                newRec.CopyFrom(record);

                if (newRec.StartStn != oldRec.StartStn) // need to repeat dijkstra forward and backwards
                {
                    OriginChange(newRec, bgw); // report progress
                    rf.SetupFresh(newRec.StartStn); // redefines origin and performs dijkstra forwards
                    path = rf.Retrace(newRec.EndStn); // gets path between origin and destination
                    SetupPath(path); // alters RecLocs for fast access to desired records
                }

                else if (newRec.EndStn != oldRec.EndStn) // need only repeat dijkstra backwards
                {
                    path = rf.Retrace(newRec.EndStn); // gets path
                    SetupPath(path); // alters RecLocs
                }

                if (RecLocs.Any())
                {
                    IncrementAccordingly(newRec.StartTime, newRec.EndTime); // increment desired records
                }

                else // debugging purposes only
                {
                    dropName = Backend.StationDiction.GetValue(newRec.EndStn);
                    if (!droppedStations.Contains(dropName))
                    {
                        droppedStations.Add(dropName);
                    }
                    DroppedRecords++;
                }

                oldRec.CopyFrom(newRec);
            }

            WriteFile(outFile, cutEdges);
            SetMaximumLoading();
        }

        // progress reports

        private void OriginChange(FinalLine f, BackgroundWorker bgw) // outputs current data record when origin station changes
        {
            int percentProgress = (f.StartStn * 100) / NodeNumber;
            bgw.ReportProgress(percentProgress);
        }

        // Writing Methods

        private void WriteFile(string outFile, List<string> cutEdges) // writes loading data to file in desired format
        {
            StreamWriter w = new StreamWriter(outFile);

            // cut edges
            w.Write("Cut Edges:,");

            if (cutEdges.Any())
            {
                foreach (var edge in cutEdges)
                {
                    w.Write(edge + ",");
                }
            }
            else
            {
                w.Write("No cut edges,");
            }

            w.WriteLine();

            //column headings
            int time = 0;
            w.Write("LineId,LineName,StationAId,StationAName,StationBId,StationBName,");

            for (int i = 0; i < 144; i++)
            {
                w.Write((time / 60) + ":" + (time % 60) + ",");
                time += Interval;
            }

            w.WriteLine();
            //end column headings

            //record printing
            foreach (var line in LineRecords)
            {
                foreach (var record in line.Records)
                {
                    w.Write(line.LineId + ",");

                    w.Write(line.LineName + ",");

                    w.Write(record.StationA.Id + ",");

                    w.Write(record.StationA.Name + ",");

                    w.Write(record.StationB.Id + ",");

                    w.Write(record.StationB.Name + ",");

                    foreach (var num in record.Loadings)
                    {
                        w.Write(num + ",");
                    }

                    w.WriteLine();
                }
            }

            w.Close();
        }

        // Path handling methods

        private void SetupPath(PathClass p) // edits RecLocs to point at records specified in path
        {
            RecLocs.Clear();
            int stationAId, stationBId, lineIndex, recordIndex;

            stationAId = -1; // prevents first iteration going through

            foreach (var node in p.Path)
            {
                stationBId = node.StationId;
                lineIndex = node.LineIndex;

                if (stationAId != -1) // avoids first iteration
                {
                    recordIndex = LineRecords[lineIndex].GetRecordIndex(stationAId, stationBId);
                    if (recordIndex != -1)
                    {
                        RecLocs.Add(new RecordLocation(lineIndex, recordIndex));
                    }
                }

                stationAId = stationBId;
            }
        }

        private void IncrementAccordingly(int startTime, int endTime) // increments specified records at appropriate times
        {
            int offset = (endTime - startTime) / RecLocs.Count();
            int time = startTime;

            foreach (var recLoc in RecLocs)
            {
                recLoc.IncrementSpecified(time);
                time += offset;
            }
        }

        // display and colour methods

        private void SetMaximumLoading()
        {
            int max = 0;
            foreach (var lineRec in LineRecords)
            {
                foreach (var rec in lineRec.Records)
                {
                    foreach (var load in rec.Loadings)
                    {
                        if (load > max)
                        {
                            max = load;
                        }
                    }
                }
            }
            MaxLoading = max;
        }

        public void ChangeTimeOfDay(int index)
        {
            foreach (var lineRec in LineRecords)
            {
                lineRec.UpdateColours(index, MaxLoading);
            }
        }

        // subclasses

        public class LineRecord // contains all records for a particular line
        {
            public int LineId { get; private set; }
            public string LineName { get; private set; }
            public List<Record> Records { get; private set; }

            private SolidColorBrush DefaultStroke { get; set; }

            // constructor

            public LineRecord(int lineId) // sets up record structure
            {
                this.LineId = lineId;
                this.LineName = LineDiction.GetValue(lineId);
                Records = new List<Record>();
                DefaultStroke = new SolidColorBrush(LineDiction.GetLineColor(this.LineId)); // TODO : change s.t. are actual line colours

                for (int i = 0; i < MetroNetwork.Lines[lineId].Branches.Count(); i++)
                {
                    for (int j = 0; j < MetroNetwork.Lines[lineId].Branches[i].StopSequence.Count() - 1; j++)
                    {
                        Records.Add(new Record(lineId, i, j));

                        if (InValid(Records.Last()))
                        {
                            Records.RemoveAt(Records.Count() - 1);
                        }

                        else
                        {
                            Records.Last().DrawEdge(DefaultStroke);
                        }
                    }
                }
            }

            // basic methods

            private bool InValid(Record r) // return true if: id is -1 or that edge has already appeared in reverse
            {
                if (r.StationA.Id == -1 || r.StationB.Id == -1)
                {
                    return true;
                }

                foreach (var rec in Records)
                {
                    if (r.StationB.Id == rec.StationA.Id)
                    {
                        if (r.StationA.Id == rec.StationB.Id)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            public int GetRecordIndex(int stationAId, int stationBId) // returns index corresponding to record on line with these two stations
            {
                for (int i = 0; i < Records.Count(); i++)
                {
                    if (Records[i].StationA.Id == stationAId)
                    {
                        if (Records[i].StationB.Id == stationBId)
                        {
                            return i;
                        }
                    }

                    else if (Records[i].StationB.Id == stationAId)
                    {
                        if (Records[i].StationA.Id == stationBId)
                        {
                            return i;
                        }
                    }
                }

                return -1; // error has occured
            }

            // colour scheme

            public void UpdateColours(int index, int max)
            {
                foreach (var rec in Records)
                {
                    rec.ChangeColour(index, max);
                }
            }

            // subclass

            public class Record
            {
                public Station StationA { get; private set; }
                public Station StationB { get; private set; }
                public int[] Loadings { get; private set; } // void value is 0

                public Line L { get; set; } // canvas drawing

                public const int MinutesPerDay = 1440;
                private const int EmptyLineThickness = 1;
                private const int OriginalLineThickness = 3;
                private const int LineExpansion = 12;

                // constructor

                public Record(int lineIndex, int branchIndex, int stationAIndex)
                {
                    int stationIdA, stationIdB;
                    stationIdA = MetroNetwork.Lines[lineIndex].Branches[branchIndex].StopSequence[stationAIndex].StationId;
                    stationIdB = MetroNetwork.Lines[lineIndex].Branches[branchIndex].StopSequence[stationAIndex + 1].StationId;

                    this.StationA = new Station(stationIdA);
                    this.StationB = new Station(stationIdB);

                    Loadings = new int[MinutesPerDay / Interval];
                }

                // editing method

                public void IncrementAt(int minsAfterMidnight)
                {
                    minsAfterMidnight = minsAfterMidnight % MinutesPerDay; // TFL forgets that there are only 1440 minutes in 24 hours
                    int index = minsAfterMidnight / Interval; // finds index that time corresponds to
                    Loadings[index]++; // increments desired item
                }

                // drawing method

                public void DrawEdge(Brush defaultStroke)
                {
                    L = new Line();
                    L.StrokeThickness = OriginalLineThickness;
                    L.Stroke = defaultStroke;

                    C.Children.Add(L);

                    Coordinate Coord1 = StationDiction.GetPosition(StationA.Id);
                    Coordinate Coord2 = StationDiction.GetPosition(StationB.Id);

                    L.X1 = Coord1.X * C.ActualWidth;
                    L.Y1 = Coord1.Y * C.ActualHeight;

                    L.X2 = Coord2.X * C.ActualWidth;
                    L.Y2 = Coord2.Y * C.ActualHeight;
                }

                public void ChangeColour(int index, int max)
                {
                    int x = Loadings[index];

                    if (x == 0)
                    {
                        L.Stroke = Brushes.Black;
                        L.StrokeThickness = EmptyLineThickness;
                    }

                    else
                    {
                        double ratio = Math.Log(x) / Math.Log(max);
                        Color col = HSVtoRGB.Convert((float)ratio, 1.0f, 1.0f, 1.0f);

                        L.Stroke = new SolidColorBrush(col);
                        L.StrokeThickness = EmptyLineThickness + (LineExpansion * ratio * ratio); // squared to get more obvious expansion
                    }
                }

                // subclass

                public class Station
                {
                    public int Id { get; set; }
                    public string Name { get; set; }

                    public Station(int id)
                    {
                        this.Id = id;
                        this.Name = StationDiction.GetValue(id);
                    }
                }
            }
        }

        public class RecordLocation // contains pointers to a specific record's location
        {
            public int LineIndex { get; set; }
            public int RecordIndex { get; set; }

            // constructor

            public RecordLocation(int lineIndex, int recordIndex)
            {
                this.LineIndex = lineIndex;
                this.RecordIndex = recordIndex;
            }

            // basic method

            public void IncrementSpecified(int time)
            {
                LineRecords[LineIndex].Records[RecordIndex].IncrementAt(time);
            }
        }

        public class StationDisplay // displays stations on canvas
        {
            private static Label DisplayLabel { get; set; }
            private List<Station> Stations { get; set; }

            public StationDisplay(Canvas C)
            {
                DisplayLabel = (Label)C.Children[0];
                Coordinate coord;
                Stations = new List<Station>();

                for (int i = 0; i < StationDiction.N; i++)
                {
                    coord = StationDiction.GetPosition(i);
                    Stations.Add(new Station(i, coord, C));
                }
            }

            public class Station
            {
                private int StationId { get; set; }
                private Ellipse E { get; set; }

                private const int Radius = 3;

                public Station(int id, Coordinate coord, Canvas C)
                {
                    this.StationId = id;

                    if (coord != null)
                    {
                        E = new Ellipse();
                        E.Stroke = Brushes.Black;
                        E.StrokeThickness = 1;
                        E.Width = Radius * 2;
                        E.Height = Radius * 2;
                        E.Fill = Brushes.White;

                        C.Children.Add(E);
                        Canvas.SetLeft(E, C.ActualWidth * coord.X - Radius);
                        Canvas.SetTop(E, C.ActualHeight * coord.Y - Radius);

                        E.MouseEnter += E_MouseEnter;
                    }
                }

                private void E_MouseEnter(object sender, MouseEventArgs e)
                {
                    DisplayLabel.Content = StationDiction.GetValue(this.StationId);
                }
            }
        }
    }
}
