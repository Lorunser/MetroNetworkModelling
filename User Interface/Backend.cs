using System.Collections.Generic;
using System.Windows.Controls;

// additional imports
using System.ComponentModel;

// custom libraries
using API;
using Diction;
using Routing;
using PreSorting;

namespace User_Interface
{
    /// <summary>
    /// manages entirety of backend procedures e.g.:
    /// 
    /// loading file
    /// running analysis
    /// outputting file
    /// cutting edges
    /// 
    /// </summary>
    public class Backend
    {
        public static LineDictionary LineDiction { get; private set; }
        public static StationDictionary StationDiction { get; private set; }
        private MetroNetworkLoading MNL { get; set; }
        private Sorting S { get; set; }

        private List<Node> Children { get; set; }
        private int ParentId { get; set; }

        public int NumberOfRecords { get; private set; }

        // constructor

        public Backend()
        {
            // instantiating variables
            List<string> tempList = new List<string>();
            Children = new List<Node>();

            // api requests
            tempList = APIHandler.GetLineList(); // makes request for list of lines
            LineDiction = new LineDictionary(tempList, APIHandler.GetLineColors()); // sets up line dictionary

            tempList = APIHandler.GetStationList(); // makes request for station list
            StationDiction = new StationDictionary(tempList); // sets up station dictionary

            MetroNetwork.Initialise(StationDiction, LineDiction); // sets up network structure
        }

        public void DisplayMap(Canvas C)
        {
            // sets up laodings class and display
            MNL = new MetroNetworkLoading(LineDiction, StationDiction, C);
        }

        // working methods

        public void LoadFile(BackgroundWorker bgw, string sourcePath, bool debug = false) // Loads and sorts file
        {
            S = new Sorting(sourcePath);
            NumberOfRecords = S.Sort(StationDiction, bgw); // loads, translates and sorts source data
        }

        public void RunAnalysis(string outFile, BackgroundWorker bgw, List<string> cutEdges) // runs route-finding analysis on data
        {
            MNL.FillInNetwork(outFile, S, bgw, cutEdges);
        }

        // basic methods

        public List<string> GetStationList()
        {
            return StationDiction.Values;
        }

        public int GetMaxLoading()
        {
            return MNL.MaxLoading;
        }

        public int GetDroppedRecords()
        {
            return MNL.DroppedRecords;
        }

        // editing methods

        public List<string> GetNeighbourNames(int parentId)
        {
            Children.Clear();
            this.ParentId = parentId;
            List<string> neighbours = new List<string>();
            Children = MetroNetwork.GenChildrenFromStationId(this.ParentId);
            string val;

            foreach (var child in Children)
            {
                val = LineDiction.GetValue(child.LineIndex) + ": " + StationDiction.GetValue(child.StationId);
                neighbours.Add(val);
            }

            return neighbours;
        }

        public string CutEdge(int index)
        {
            Node child = Children[index];
            MetroNetwork.InsertBreakBetween(ParentId, child);

            string rec = LineDiction.GetValue(child.LineIndex) + ": " + StationDiction.GetValue(ParentId) + " - " + StationDiction.GetValue(child.StationId);
            return rec;
        }

        // drawing methods

        public void TimeChanged(int index)
        {
            MNL.ChangeTimeOfDay(index);
        }

        // subclasses
    }
}
