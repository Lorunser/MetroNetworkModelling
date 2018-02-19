using System;
using System.Linq;

namespace Routing
{
    /// <summary>
    /// Points to a particular node on the graph
    /// Keeps track of the score assigned to that node
    /// Provides various helper methods regarding score
    /// </summary>
    public class Node
    {
        public int LineIndex { get; set; }
        public int BranchIndex { get; set; }
        public int StationIndex { get; set; }
        public int StationId { get; set; }
        public int Score { get; set; }

        public const int k = 3; // penalty for changing lines

        // constructors

        public Node() { }

        public Node(int stationId)
        {
            this.StationId = stationId;
        }

        public Node(int lineIndex, int branchIndex, int stationIndex)
        {
            this.LineIndex = lineIndex;
            this.BranchIndex = branchIndex;
            this.StationIndex = stationIndex;
            AssignStationId();
        }

        // basic methods

        internal bool AssignLineBranchIndex() // assigns line branch and stationIndex from given stationId
        {
            int tempId;
            for (int i = 0; i < MetroNetwork.Lines.Count(); i++)
            {
                for (int j = 0; j < MetroNetwork.Lines[i].Branches.Count(); j++)
                {
                    for (int k = 0; k < MetroNetwork.Lines[i].Branches[j].StopSequence.Count(); k++)
                    {
                        tempId = MetroNetwork.Lines[i].Branches[j].StopSequence[k].StationId;
                        if (this.StationId == tempId)
                        {
                            this.LineIndex = i;
                            this.BranchIndex = j;
                            this.StationIndex = k;
                            return true;
                        }
                    }
                }
            }
            return false;
            // error has occured, station not found
        }

        internal bool AssignBranchAndIndex() // assign branch and stationIndex from given line and stationId
        {
            int tempId;

            for (int i = 0; i < MetroNetwork.Lines[this.LineIndex].Branches.Count(); i++)
            {
                for (int j = 0; j < MetroNetwork.Lines[this.LineIndex].Branches[i].StopSequence.Count(); j++)
                {
                    tempId = MetroNetwork.Lines[this.LineIndex].Branches[i].StopSequence[j].StationId;
                    if (this.StationId == tempId)
                    {
                        this.BranchIndex = i;
                        this.StationIndex = j;
                        return true;
                    }
                }
            }

            return false;
        }

        internal bool AssignStationId() // assigns stationId from given line, branch and stationIndex
        {
            if (this.StationIndex >= 0) // logic checks to ensure pointers point to a valid location
            {
                if (this.LineIndex < MetroNetwork.Lines.Count())
                {
                    MetroNetwork.Line line = MetroNetwork.Lines[this.LineIndex];
                    if (this.BranchIndex < line.Branches.Count())
                    {
                        MetroNetwork.Line.Branch branch = line.Branches[this.BranchIndex];
                        if (this.StationIndex < branch.StopSequence.Count())
                        {
                            this.StationId = branch.StopSequence[this.StationIndex].StationId;
                            return true;
                        }
                    }
                }
            }

            return false; // pointers do not correspond to a station
        }

        internal int GetBranchLength() // gte length of branch this node is on
        {
            return MetroNetwork.Lines[this.LineIndex].Branches[this.BranchIndex].StopSequence.Count();
        }

        internal void Reset() // reset all instance attributes
        {
            LineIndex = 0;
            BranchIndex = 0;
            StationIndex = 0;
            Score = 0;
        }

        // Dijkstra going forward

        internal void CopyLineBranchIndexFrom(Node n) // copies attributes form node n
        {
            this.LineIndex = n.LineIndex;
            this.BranchIndex = n.BranchIndex;
            this.StationIndex = n.StationIndex;
        }

        internal bool UpdateScore(Node parent, int originId) // returns true if score has been updated OR 50:50 if scores are equal >> non-deterministic
        {
            int tempScore;
            tempScore = parent.Score + 1;
            bool firstPass = false;

            // dealing with origin
            if (parent.StationId == originId)
            {
                firstPass = true;
            }

            if (this.StationId == originId)
            {
                return false; // cannot visit origin once left
            }

            // dealing with line changes
            if (this.LineIndex != parent.LineIndex)
            {
                if (!firstPass)
                {
                    tempScore += k; // k is penalty for a lineChange
                }
            }

            // making sure tempScore is greater than current score
            if (this.Score > tempScore || this.Score == 0)
            {
                this.Score = tempScore;
                return true;
            }

            // random element
            else if (this.Score == tempScore)
            {
                Random rand = new Random();
                if (rand.Next(2) == 1)
                {
                    return true;
                }
            }

            return false;
        }

        // Dijkstra going backwards

        internal bool ValidRewind(Node parent, int originId) // returns true if given step backwards is valid according to scoring system
        {
            if (parent.StationId == originId) // always takes direct path to origin if possible
            {
                return true;
            }

            int tempScore;
            tempScore = parent.Score + 1;

            if (this.LineIndex != parent.LineIndex) // deals with line change
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
}
