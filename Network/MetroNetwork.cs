using System.Collections.Generic;
using System.Linq;

// custom libraries
using API;
using Diction;

namespace Routing
{
    /// <summary>
    /// contains graph representation
    /// provides graph traversal/editing methods
    /// </summary>
    public static class MetroNetwork // contains adjacency data
    {
        public static List<Line> Lines { get; private set; }

        // Initialisation methods

        public static void Initialise(StationDictionary stationDiction, LineDictionary lineDiction)
        {
            Lines = new List<Line>(); // intialise variable
            List<JsonRoute> jRoutes = APIHandler.GetRoutes(lineDiction); // makes api request for link data

            foreach (var jRoute in jRoutes)
            {
                Lines.Add(new Line(jRoute, stationDiction, lineDiction));
            }

            FixCircleLine(stationDiction, lineDiction); // necessary fix to inconsistency in circle line branch structure
            FixPaddingtonEdgwareRdHammersmith(stationDiction, lineDiction); // ensures these stations have consistent id's
        }

        private static void FixCircleLine(StationDictionary stationDiction, LineDictionary lineDiction) // fix to error in circle line, where paddington and edgware road are not end of branch
        {
            int circleId = lineDiction.GetKey("circle");
            int paddingtonId = stationDiction.GetKey("paddington");
            int edgwareRoadId = stationDiction.GetKey("edgware road");

            Line.Branch.Stop edgwareRoad = null;
            Line.Branch.Stop paddington = null;

            Line circleLine = Lines[circleId]; // alias
            Line.Branch branch;
            int branchLength;

            for (int i = 0; i < circleLine.Branches.Count(); i++)
            {
                branch = circleLine.Branches[i];
                branchLength = branch.StopSequence.Count();

                if (branch.StopSequence[0].StationId == edgwareRoadId)
                {
                    edgwareRoad = branch.StopSequence[0];
                    if (branch.StopSequence[1].StationId == paddingtonId)
                    {
                        paddington = branch.StopSequence[1];
                        branch.StopSequence.RemoveAt(0);
                    }
                }

                else if (branch.StopSequence[branchLength - 1].StationId == edgwareRoadId)
                {
                    edgwareRoad = branch.StopSequence[branchLength - 1];
                    if (branch.StopSequence[branchLength - 2].StationId == paddingtonId)
                    {
                        paddington = branch.StopSequence[branchLength - 2];
                        branch.StopSequence.RemoveAt(branchLength - 1);
                    }
                }
            }

            List<Line.Branch.Stop> stops = new List<Line.Branch.Stop> { paddington, edgwareRoad };
            circleLine.Branches.Add(new Line.Branch(stops));
        }

        private static void FixPaddingtonBakerloo(StationDictionary stationDiction, LineDictionary lineDiction) // adds bakerloo line to paddington
        {
            int paddingtonId = stationDiction.GetKey("paddington");
            int bakerlooId = lineDiction.GetKey("bakerloo");

            Line line;
            Line.Branch branch;
            Line.Branch.Stop stop;

            for (int lineIndex = 0; lineIndex < Lines.Count(); lineIndex++)
            {
                line = Lines[lineIndex]; // alias
                for (int branchIndex = 0; branchIndex < line.Branches.Count(); branchIndex++)
                {
                    branch = line.Branches[branchIndex]; // alias
                    for (int stationindex = 0; stationindex < branch.StopSequence.Count(); stationindex++)
                    {
                        stop = branch.StopSequence[stationindex]; // alias
                        if (stop.StationId == paddingtonId)
                        {
                            if (!stop.LineIds.Contains(bakerlooId))
                            {
                                stop.LineIds.Add(bakerlooId);
                            }
                        }
                    }
                }
            }
        }

        private static void FixPaddingtonEdgwareRdHammersmith(StationDictionary stationDiction, LineDictionary lineDiction)
        {
            // paddington, edgware road and hammermsith are only stations to have duplicate id's in TFL's database
            // this procedure fixes the resulting problems of inconsistent links

            int paddingtonId = stationDiction.GetKey("paddington");
            int edgwareRdId = stationDiction.GetKey("edgware road");
            int hammermsithId = stationDiction.GetKey("hammersmith");

            int bakerlooId = lineDiction.GetKey("bakerloo");
            int circleId = lineDiction.GetKey("circle");
            int districtId = lineDiction.GetKey("district");
            int hamCityId = lineDiction.GetKey("hammersmith-city");
            int piccadillyId = lineDiction.GetKey("piccadilly");

            List<int> PadEdgLineIds = new List<int> { bakerlooId, circleId, districtId, hamCityId };
            List<int> HamLineIds = new List<int> { circleId, districtId, hamCityId, piccadillyId };

            Line line;
            Line.Branch branch;
            Line.Branch.Stop stop;

            for (int lineIndex = 0; lineIndex < Lines.Count(); lineIndex++)
            {
                line = Lines[lineIndex];
                for (int branchIndex = 0; branchIndex < line.Branches.Count(); branchIndex++)
                {
                    branch = line.Branches[branchIndex];
                    for (int stationIndex = 0; stationIndex < branch.StopSequence.Count(); stationIndex++)
                    {
                        stop = branch.StopSequence[stationIndex];

                        if (stop.StationId == paddingtonId || stop.StationId == edgwareRdId)
                        {
                            CopyList<int>(stop.LineIds, PadEdgLineIds);
                        }
                        else if (stop.StationId == hammermsithId)
                        {
                            CopyList<int>(stop.LineIds, HamLineIds);
                        }
                    }
                }
            }
        }

        private static void CopyList<T>(List<T> targetList, List<T> itemsList)
        {
            targetList.Clear();

            foreach (var item in itemsList)
            {
                targetList.Add(item);
            }
        }

        // Network editing methods

        public static List<Node> GenChildrenFromStationId(int stationId)
        {
            Node parent = new Node(stationId);
            parent.AssignLineBranchIndex();
            return GenChildren(parent);
        }

        public static void InsertBreakBetween(int stationIdA, Node B) // inserts cut between two stations
        {
            Node A = new Node();
            A.CopyLineBranchIndexFrom(B);
            int stationIndex = B.StationIndex;

            // test going right
            A.StationIndex = stationIndex + 1;

            if (A.StationIndex < A.GetBranchLength()) // ensure hasn't fallen off
            {
                A.AssignStationId(); // assigns A with stationIndex corresponding to position
                if (A.StationId == stationIdA)
                {
                    RightInsertBreak(B);
                    return;
                }
            }

            A.StationIndex = stationIndex - 1;
            if (A.StationIndex >= 0)
            {
                A.AssignStationId();
                if (A.StationId == stationIdA)
                {
                    LeftInsertBreak(B);
                    return;
                }
            }

            // TODO : throw custom exception
        }

        private static void LeftInsertBreak(Node n)
        {
            Line.Branch.Stop cut = new Line.Branch.Stop(-1); // -1 is break value
            Lines[n.LineIndex].Branches[n.BranchIndex].StopSequence.Insert(n.StationIndex, cut); // left insert
        }

        private static void RightInsertBreak(Node n)
        {
            Line.Branch.Stop cut = new Line.Branch.Stop(-1); // -1 is break value
            Lines[n.LineIndex].Branches[n.BranchIndex].StopSequence.Insert(n.StationIndex + 1, cut); // right insert
        }

        // Children Generation Methods

        public static List<Node> GenChildren(Node realParent) // generates children nodes from parent
        {
            List<Node> children = new List<Node>();
            Node tempParent = new Node();
            tempParent.CopyLineBranchIndexFrom(realParent);
            tempParent.StationId = realParent.StationId; // ensures a copy of parent is used => prevent aliasing issues

            int originalLineIndex = tempParent.LineIndex;
            children.AddRange(GenInLineChildren(tempParent));

            foreach (var lineIndex in GetLineIndices(tempParent)) // spawns children from all lines
            {
                if (lineIndex != originalLineIndex) // avoids uneccesary first iteration
                {
                    tempParent.LineIndex = lineIndex;
                    if (tempParent.AssignBranchAndIndex()) // return true if stationId has been found on that line
                    {
                        children.AddRange(GenInLineChildren(tempParent));
                    }
                }
            }

            return children;
        }

        private static List<Node> GenInLineChildren(Node parent) // returns children on the same line as parent
        {
            Node forward = new Node();
            Node backwards = new Node();
            List<Node> inLineChildren = new List<Node>();
            int lineIndex = parent.LineIndex;

            forward.CopyLineBranchIndexFrom(parent);
            backwards.CopyLineBranchIndexFrom(parent);

            forward.StationIndex++; // forwards node is in branch one ahead
            backwards.StationIndex--; // backwards node is in branch one behind

            inLineChildren.AddRange(Lines[lineIndex].GenLineNeighbours(forward, parent));
            inLineChildren.AddRange(Lines[lineIndex].GenLineNeighbours(backwards, parent));

            for (int i = 0; i < inLineChildren.Count(); i++)
            {
                if (inLineChildren[i].StationId == -1) // break identifier
                {
                    inLineChildren.RemoveAt(i); // this child does not count
                    i--;
                }
            }

            return inLineChildren;
        }

        private static List<int> GetLineIndices(Node parent) // returns all lines passing through this node
        {
            Line line = Lines[parent.LineIndex];
            Line.Branch branch = line.Branches[parent.BranchIndex];
            Line.Branch.Stop stop = branch.StopSequence[parent.StationIndex];
            return stop.LineIds;
            //return Lines[parent.LineIndex].Branches[parent.BranchIndex].StopSequence[parent.StationIndex].LineIds;
        }

        // Subclass

        public class Line
        {
            public List<Branch> Branches = new List<Branch>(); // list of branches wihtin line structure

            // Children validation

            public List<Node> GenLineNeighbours(Node child, Node parent)
            {
                List<Node> inLineChildren = new List<Node>();
                int stationIndex;

                if (child.AssignStationId()) // returns true if child index lies within branch
                {
                    inLineChildren.Add(child);
                }

                else // child fell off the branch >> must check start and end of otehr branches
                {
                    for (int i = 0; i < Branches.Count(); i++)
                    {
                        if (i != child.BranchIndex) // cannot check current branch
                        {
                            if (Branches[i].StopSequence.Last().StationId == parent.StationId)
                            {
                                stationIndex = Branches[i].StopSequence.Count() - 2; // 2nd to last item in branch
                                inLineChildren.Add(new Node(child.LineIndex, i, stationIndex));
                            }

                            else if (Branches[i].StopSequence.First().StationId == parent.StationId)
                            {
                                stationIndex = 1; // 2nd item from start in branch
                                inLineChildren.Add(new Node(child.LineIndex, i, stationIndex));
                            }
                        }
                    }
                }


                return inLineChildren;
            }

            // Constructor

            public Line(JsonRoute jRoute, StationDictionary stationDiction, LineDictionary lineDiction) // sets up line structure
            {
                foreach (var sps in jRoute.stopPointSequences)
                {
                    Branches.Add(new Branch(sps, stationDiction, lineDiction));
                }
            }

            // Subclass

            public class Branch
            {
                public List<Stop> StopSequence { get; private set; }

                // constructors

                public Branch(List<Stop> stops) // creates branch form list of stops
                {
                    StopSequence = new List<Stop>();
                    foreach (var stop in stops)
                    {
                        StopSequence.Add(stop);
                    }
                }

                public Branch(JsonRoute.StopPointSequence sps, StationDictionary stationDiction, LineDictionary lineDiction) // creates branch from JSON stop point sequence
                {
                    StopSequence = new List<Stop>();
                    int stationId;
                    int lineId;
                    List<int> lineIds = new List<int>();

                    foreach (var station in sps.stopPoint)
                    {
                        stationId = stationDiction.GetKey(station.name, station.lat, station.lon);
                        // gets key and updates coordinates within station Dictionary

                        foreach (var line in station.lines)
                        {
                            lineId = lineDiction.GetKey(line.id);
                            if (lineId != -1)
                            {
                                lineIds.Add(lineId);
                            }
                        }

                        StopSequence.Add(new Stop(stationId, lineIds));

                        if (lineIds.Any()) // avoids error if list is empty
                        {
                            lineIds.Clear();
                        }
                    }
                }

                // subclass

                public class Stop
                {
                    public int StationId { get; set; }
                    public List<int> LineIds { get; set; } // list of liens that pass through station

                    public Stop(int stationId, List<int> lineIds = null)
                    {
                        this.StationId = stationId;
                        this.LineIds = new List<int>();

                        if (lineIds != null)
                        {
                            foreach (var lineId in lineIds)
                            {
                                this.LineIds.Add(lineId);
                            }
                        }
                    }
                }
            }
        }
    }
}
