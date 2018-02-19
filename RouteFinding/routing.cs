using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoutingLibrary
{
    public class MetroNetwork
    {
        public static List<LineClass> Net = new List<LineClass>(); //only want to use one

        //################# IMPORTANT ###############
        public static void GenerateNetwork() // TODO: complete
        {

        }
        //################# IMPORTANT ###############

        public static List<Node> GenChildren(Node parent) // continue later
        {
            List<Node> children = new List<Node>();
            List<int> neighbourIndices = new List<int>();

            int lineIndex = parent.LineIndex;
            int stationIndex = parent.StationIndex;

            neighbourIndices.AddRange(GenNeighbourIndices(lineIndex, stationIndex)); // potential aliasing problem
            AddOnChildren(ref children, lineIndex, neighbourIndices);
            neighbourIndices.Clear();

            foreach (int i in Net[lineIndex].GetOtherLineIDs(stationIndex))
            {
                lineIndex = i;
                stationIndex = Net[i].GetStationIndex(parent.StationID);
                neighbourIndices.AddRange(GenNeighbourIndices(lineIndex, stationIndex)); // potential aliasing problem
                AddOnChildren(ref children, lineIndex, neighbourIndices);
                neighbourIndices.Clear();
            }

            return children;
        }

        private static void AddOnChildren(ref List<Node> children, int lineIndex, List<int> neighbourIndices) // may not add properly
        {
            foreach (int i in neighbourIndices)
            {
                children.Add(new Node(lineIndex, i));
            }
        }

        private static List<int> GenNeighbourIndices(int lineIndex, int stationIndex)
        {
            return Net[lineIndex].GetNeighbourIndices(stationIndex);
        }

        public class LineClass // change modifier later
        {
            private List<Station> Line = new List<Station>();
            private bool Loop = false; // determines whether a line loops back, default = false

            public void InsertBreakAt(int idA, int idB)
            {
                for (int i = 0; i < Line.Count() - 1; i++)
                {
                    if (Line[i].StationID == idA)
                    {
                        if (Line[i+1].StationID == idB)
                        {
                            Station cut = new Station();
                            cut.SetToCut();
                            Line.Insert(i + 1, cut);
                        }
                    }
                }
                // linebreak has not worked
            }

            public int GetStationIndex(int targetID)
            {
                for (int i = 0; i < Line.Count(); i++)
                {
                    if (Line[i].StationID == targetID)
                    {
                        return i;
                    }
                }
                return -1; // null value
            } 

            public int GetStationID(int targetIndex)
            {
                return Line[targetIndex].StationID;
            }

            public List<int> GetOtherLineIDs(int targetIndex)
            {
                return Line[targetIndex].LineChanges;
            }

            //Finding Neighbours

            public List<int> GetNeighbourIndices(int index)
            {
                List<int> neighbours = new List<int>();
                int a, b;

                a = GenNeighbour(index + 1);
                b = GenNeighbour(index - 1);

                if (a != -1)
                {
                    neighbours.Add(a);
                }

                if (b != -1)
                {
                    neighbours.Add(b);
                }

                return neighbours;

            }

            private int GenNeighbour(int index)
            {
                if (Loop)
                {
                    index = index % Line.Count(); // check this works
                }
                return IsValid(index);
            }

            private int IsValid(int index)
            {
                if (index <= Line.Count() && index >= 0)
                {
                    if (Line[index].StationID != -1)
                    {
                        return index;
                    }
                }
                return -1; // null value
            }

            // end of neighbour finding

            private class Station
            {
                public int StationID { get; private set; }
                public List<int> LineChanges = new List<int>();

                public void SetToCut()
                {
                    LineChanges.Clear();
                    StationID = -1; // null station id - equivalent to linebreak
                }

                public void Set(int id, List<int> lines)
                {
                    StationID = id;
                    LineChanges.Clear();
                    for (int a = 0; a < lines.Count(); a++)
                    {
                        LineChanges.Add(lines[a]);
                    }
                }
            }
        }
    }    

    public class Node
    {
        public int StationID { get; set; }
        public int LineIndex { get; set; }
        public int StationIndex { get; set; }
        public int Score { get; set; }

        private const int k = 3; // penalty for changing lines

        // constructors

        public Node() { }

        public Node(int stationID)
        {
            StationID = stationID;
        }

        public Node(int lineIndex, int stationIndex)
        {
            LineIndex = lineIndex;
            StationIndex = stationIndex;
            AssignStationID();
        }

        // basic methods

        public void AssignStationID()
        {
            StationID = MetroNetwork.Net[LineIndex].GetStationID(StationIndex);
        }

        public void AssignLineAndIndex()
        {
            int tempIndex;
            for (int i = 0; i < MetroNetwork.Net.Count(); i++)
            {
                tempIndex = MetroNetwork.Net[i].GetStationIndex(this.StationID);
                if (tempIndex != -1)
                {
                    LineIndex = i;
                    StationIndex = tempIndex;
                    return;
                }
            }

            // ERROR: stationID does not exist in network 
        }

        //Dijkstra Going Forward

        public void ClonePositionAndID(Node n)
        {
            this.StationID = n.StationID;
            this.LineIndex = n.LineIndex;
            this.StationIndex = n.StationIndex;
        }

        public bool UpdateScore(Node parent, bool firstPass) // returns true if score has been changed
        {
            int tempScore;
            tempScore = parent.Score + 1;

            if (this.LineIndex != parent.LineIndex)
            {
                if (!firstPass)
                {
                    tempScore += k;
                }
            }

            if (this.Score > tempScore || this.Score == 0)
            {
                this.Score = tempScore;
                return true;
            }

            return false;
        }

        //Dijkstra Going Backwards

        public bool ValidRewind(Node parent, int originID)
        {
            if (parent.StationID == originID)
            {
                return true;
            }

            int tempScore;
            tempScore = parent.Score + 1;

            if (this.LineIndex != parent.LineIndex)
            {
                tempScore += k;
            }

            if (tempScore == this.Score)
            {
                return true;
            }

            return false;
        }
    }

    public class RouteFinding
    {
        private int OriginStationID { get; set; }
        private List<Node> Stations = new List<Node>(); // must be sorted by station ID
        private List<int> StationsToDo = new List<int>();

        //Constructor

        public RouteFinding(int originStationID, int numberOfStations)
        {
            OriginStationID = originStationID;
            for (int i = 0; i < numberOfStations; i++)
            {
                Stations.Add(new Node(i));
            }
            AddOnToList(OriginStationID); // sets up list of nodes still to do
        }

        //Dijkstra Forward

        public void DijsktraForward()
        {
            int nextStationID = GetNextStationID();
            Stations[nextStationID].AssignLineAndIndex(); // necessary
            NextGeneration(nextStationID, true);

            while (nextStationID != -1)
            {
                NextGeneration(nextStationID);
                nextStationID = GetNextStationID();
            }
        }

        private void NextGeneration(int parentID, bool firstPass = false)
        {
            Node parent = new Node();
            parent = Stations[parentID]; // aliasing problems maybe

            List<Node> children = new List<Node>();
            children = MetroNetwork.GenChildren(parent);
            int index;

            foreach (var child in children)
            {
                index = child.StationID;
                Stations[index].ClonePositionAndID(child);

                if (Stations[index].UpdateScore(parent, firstPass))
                {
                    AddOnToList(index);
                }
            }

            RemoveFromList(parentID);
        }

        private int GetNextStationID()
        {
            if (!Stations.Any())
            {
                return -1; // no stations left in list => terminate
            }

            int nextID = Stations[StationsToDo[0]].StationID; // default ID's and scores
            int minScore = Stations[StationsToDo[0]].Score;
            int currentScore, currentIndex;

            foreach (int index in StationsToDo)
            {
                currentScore = Stations[index].Score;
                currentIndex = Stations[index].StationID;

                if (currentScore < minScore)
                {
                    minScore = currentScore;
                    nextID = currentIndex;
                }
            }

            return nextID;
        }

        private void AddOnToList(int id)
        {
            if (!StationsToDo.Contains(id))
            {
                StationsToDo.Add(id);
            }
        }

        private void RemoveFromList(int id)
        {
            StationsToDo.Remove(id);
        }

        // Dijkstra backwards

        public PathClass Retrace(int destinationID)
        {
            PathClass path = new PathClass(OriginStationID, destinationID);
            path.RewindUntilOrigin(Stations);
            return path;
        }
        
    }

    public class PathClass
    {
        private List<Node> Path = new List<Node>();
        private int OriginID;
        private int DestinationID;

        public PathClass(int originID, int destinationID)
        {
            OriginID = originID;
            DestinationID = destinationID;
        }

        public void RewindUntilOrigin(List<Node> stations)
        {
            Path.Add(stations[DestinationID]);
            int index;
            bool ongoing = true;
            List<Node> neighbours = new List<Node>();

            do
            {
                neighbours = MetroNetwork.GenChildren(Path[0]); // may run into problems with aliasing
                foreach (Node n in neighbours) // may run into problems with foreach
                {
                    index = n.StationID;

                    if (Path[0].ValidRewind(stations[index], OriginID))
                    {
                        AddOn(stations[index]);
                        if (index == OriginID)
                        {
                            ongoing = false;
                        }
                    }
                }
                neighbours.Clear();
            } while (ongoing);
        }

        private void AddOn(Node n)
        {
            Path.Insert(0, n); // make sure not to edit
        }
    }

}
