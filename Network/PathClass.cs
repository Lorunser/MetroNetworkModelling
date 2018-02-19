using System;
using System.Collections.Generic;
using System.Linq;

namespace Routing
{
    /// <summary>
    /// specifies path between two nodes
    /// provides helper methods
    /// </summary>
    public class PathClass
    {
        public LinkedList<Node> Path { get; private set; } // List of Nodes in route from origin to destination
        public int OriginId { get; private set; }
        public int DestinationId { get; private set; }

        // constructor

        public PathClass(int originId, int destinationId)
        {
            Path = new LinkedList<Node>();
            this.OriginId = originId;
            this.DestinationId = destinationId;
        }

        // basic methods

        internal void RewindUntilOrigin(List<Node> stations) // retraces steps from destination to origin
        {
            Path.AddLast(stations[DestinationId]);

            int index;
            bool ongoing = true;
            int parentCount;

            Random rand = new Random();
            List<Node> neighbours = new List<Node>();
            List<int> validParentIds = new List<int>();

            do
            {
                neighbours = MetroNetwork.GenChildren(Path.First());

                foreach (Node n in neighbours)
                {
                    index = n.StationId;

                    if (Path.First().ValidRewind(stations[index], OriginId))
                    {
                        validParentIds.Add(index);
                    }
                }

                parentCount = validParentIds.Count();

                if (parentCount > 0)
                {
                    index = validParentIds[rand.Next(parentCount)]; // goes down a random path
                    AddOn(stations[index]);

                    if (index == OriginId) // terminates when reached origin
                    {
                        ongoing = false;
                    }
                }

                else // there are no valid ancestors >> no route exists (disconnected graph)
                {
                    ongoing = false;
                }

                validParentIds.Clear();
                neighbours.Clear();

            } while (ongoing);
        }

        private void AddOn(Node n) // insert given node at front of path
        {
            Path.AddFirst(n);
        }
    }
}
