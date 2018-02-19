using System;
using System.Collections.Generic;
using System.Linq;

namespace Routing
{
    /// <summary>
    /// performs Dijkstra's algorithm
    /// to find shortest path between two nodes
    /// both forwards and backwards
    /// </summary>
    public class RouteFinding
    {
        private int OriginStationId { get; set; }
        private List<Node> Stations { get; set; } // must be sorted by station ID
        private LinkedList<int> StationsToDo = new LinkedList<int>(); // priority queue of station id's to visit next

        //Constructor

        public RouteFinding(int numberOfStations)
        {
            Stations = new List<Node>();

            for (int i = 0; i < numberOfStations; i++)
            {
                Stations.Add(new Node(i)); // fills in Stations list
            }

        }

        public void SetupFresh(int originStationId) // prepares for new round of dijkstra forwards
        {
            if (originStationId >= Stations.Count() || originStationId < 0) // validation checks
            {
                throw new ArgumentOutOfRangeException("Given origin station id (" + originStationId + ") is invalid");
            }

            this.OriginStationId = originStationId;

            for (int i = 0; i < Stations.Count(); i++)
            {
                Stations[i].Reset();
            }

            Enqueue(this.OriginStationId); // sets up list of nodes still to do
            DijkstraForward();
        }

        //Dijkstra Forward

        private void DijkstraForward()
        {
            int nextStationID = Dequeue();
            Stations[nextStationID].AssignLineBranchIndex(); // necessary to define entry point =to network

            while (nextStationID != -1)
            {
                NextGeneration(nextStationID); // visits adjacent stations
                nextStationID = Dequeue();
            }
        }

        private void NextGeneration(int parentID)
        {
            Node parent = new Node();
            parent = Stations[parentID];         

            List<Node> children = new List<Node>();
            children = MetroNetwork.GenChildren(parent); // generates list of neighbouring nodes
            int index;

            // nondeterministic element:
            Shuffle(children); // mixes up children so that different ones are selected first each time

            foreach (var child in children)
            {
                index = child.StationId;
                child.Score = Stations[index].Score;

                if (child.UpdateScore(parent, OriginStationId)) // returns true only if it is a valid forwards step
                {
                    Stations[index].CopyLineBranchIndexFrom(child); // must only edit location when sure new route is faster
                    Stations[index].Score = child.Score; // assign new score
                    Enqueue(index); // adds this stationId onto list of stations to visit
                }
            }
        }

        private void Shuffle<T>(List<T> list)
        {
            if (!list.Any())
            {
                return;
            }

            // Fisher-Yates Shuffle
            Random rand = new Random();
            int index = list.Count();
            int swapIndex;
            T temp;

            do
            {
                index--;
                swapIndex = rand.Next(index + 1);

                temp = list[index];
                list[index] = list[swapIndex];
                list[swapIndex] = temp;

            } while (index > 0);
        }

        // priority queue handling methods

        private int Dequeue() // dequeues first item
        {
            if (StationsToDo.Any())
            {
                int nextId = StationsToDo.First();
                StationsToDo.RemoveFirst();
                return nextId;
            }

            else
            {
                return -1; // queue is empty >> terminate
            }
        }

        private void Enqueue(int id) // inserts item at position corresponding to its score s.t. score increases from left to right
        {
            StationsToDo.Remove(id); // removes any possible duplicate

            if (!StationsToDo.Any())
            {
                StationsToDo.AddFirst(id);
                return;
            }

            int score = Stations[id].Score;
            LinkedListNode<int> lln = StationsToDo.First;

            while (true)
            {
                if (Stations[lln.Value].Score > score)
                {
                    StationsToDo.AddBefore(lln, id);
                    return;
                }

                if (lln != StationsToDo.Last)
                {
                    lln = lln.Next;
                }

                else
                {
                    StationsToDo.AddLast(id);
                    return;
                }
            }
        }

        // Dijkstra backwards

        public PathClass Retrace(int destinationID) // returns path from origin to given destination
        {
            PathClass path = new PathClass(OriginStationId, destinationID);
            path.RewindUntilOrigin(Stations);
            return path;
        }
    }
}
