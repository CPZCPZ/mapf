using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;

namespace CPF_experiment
{
    /// <summary>
    /// Only keeps generated nodes that have the same f as the parent.
    /// The parent is then re-inserted with the lowest f from its discarded children
    /// (Yoshizumi, Miura, and Ishida 2000)
    /// </summary>
    class AStarWithPartialExpansionBasic : ClassicAStar
    {
        protected int generatedAndDiscarded;
        protected int expandedFullStates;

        bool hasMoreSuccessors;
        /// <summary>
        /// The lowest discarded F
        /// </summary>
        public int nextFvalue;
        public int currentFTarget;

        public AStarWithPartialExpansionBasic(HeuristicCalculator heuristic = null)
            : base(heuristic) { }

        override protected WorldState CreateSearchRoot()
        {
            return new WorldStateForPartialExpansion(this.instance.m_vAgents); // Consider using a WorldStateForBasicPartialExpansion that only has the isAlreadyExpanded stuff
        }

        protected override WorldState CreateSearchNode(WorldState from)
        {
            return new WorldStateForPartialExpansion((WorldStateForPartialExpansion)from);
        }

        public override void Setup(ProblemInstance problemInstance, Run runner) 
        { 
            base.Setup(problemInstance, runner);
            this.generatedAndDiscarded = 0;
            this.expandedFullStates = 0;
        }

        override public string GetName() { return "(B)PEA*"; }
        
        override public void Expand(WorldState simpleLookingNode)
        {
            var node = (WorldStateForPartialExpansion)simpleLookingNode;
            //Debug.Print("Expanding node " + node);
            if (!node.isAlreadyExpanded())
            {
                node.alreadyExpanded = true;
                this.expandedFullStates++;
            }

            hasMoreSuccessors = false;
            this.nextFvalue = int.MaxValue;
            this.currentFTarget = node.g + node.h;

            base.Expand(node);

            if (hasMoreSuccessors && this.nextFvalue <= this.maxCost)
            {
                node.h = this.nextFvalue - node.g; // Just to update this node's f value to the desired value.
                                                   // Although you could say that since we exhausted the current F value, if we get to this node again it means the heuristic was off by at least 1
                this.openList.Add(node); // Re-insert to open list with updated F
            }
        }

        /// <summary>
        /// Adds nodes of target F only
        /// </summary>
        /// <param name="currentNode"></param>
        /// <param name="parentNode"></param>
        /// <returns></returns>
        protected override bool ProcessGeneratedNode(WorldState currentNode)
        {
            if (currentNode.h + currentNode.g == this.currentFTarget)
                return base.ProcessGeneratedNode(currentNode);
            else generatedAndDiscarded++; // Notice we don't count the discarded nodes in the genereted count, only here

            if (currentNode.h + currentNode.g > this.currentFTarget)
            {
                this.hasMoreSuccessors = true;
                this.nextFvalue = (byte)Math.Min(this.nextFvalue, currentNode.h + currentNode.g);
            }
            return false;
        }

        public override void OutputStatisticsHeader(TextWriter output)
        {
            base.OutputStatisticsHeader(output);
            output.Write(this.ToString() + " Generated And Discarded (LL)");
            output.Write(Run.RESULTS_DELIMITER);
            output.Write(this.ToString() + " Expanded Full States (LL)");
            output.Write(Run.RESULTS_DELIMITER);
        }

        public override void OutputStatistics(TextWriter output)
        {
            base.OutputStatistics(output);

            Console.WriteLine("Generated And Discarded (Low-Level): {0}", this.generatedAndDiscarded);
            Console.WriteLine("Expanded Full States (Low-Level): {0}", this.expandedFullStates);

            output.Write(this.generatedAndDiscarded + Run.RESULTS_DELIMITER);
            output.Write(this.expandedFullStates + Run.RESULTS_DELIMITER);
            // Isn't there a CSV module in C# instead of fussing with the delimeter everywhere?
        }

        public int NumStatsColumns
        {
            get
            {
                return 2 + base.NumStatsColumns;
            }
        }
    }
}