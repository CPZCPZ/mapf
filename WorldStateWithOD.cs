﻿using System.Collections.Generic;

namespace CPF_experiment
{
    /// <summary>
    /// This class represents a state in the A* search with operator decomposition, as proposed by Trevor Scott Standley's AAAI paper in 2010.
    /// More specifically, states can represent a partial move, in which only some of the agents have moved
    /// and the other have not yet moved in this turn. 
    /// </summary>
    public class WorldStateWithOD : WorldState
    {
        /// <summary>
        /// Marks the index of the agent that will move next. 
        /// All agents with index less than agentTurn are assumed to have already chosen their move for this time step,
        /// while agents with higher index have not chosen their move yet.
        /// </summary>
        public int agentTurn;
        public WorldStateWithOD mirrorState;

        public WorldStateWithOD(AgentState[] states) : base(states)
        {
            this.agentTurn = 0;
            this.potentialConflictsCount = 0;
        }
        
        public WorldStateWithOD(WorldStateWithOD cpy) : base(cpy)
        {
            this.g = cpy.g;
            this.agentTurn = cpy.agentTurn;
            this.potentialConflictsCount = cpy.potentialConflictsCount;
        }
        
        public WorldStateWithOD(AgentState[] states, List<uint> relevantAgents) : base(states, relevantAgents)
        {
            this.agentTurn = 0;
            this.potentialConflictsCount = 0;
        }

        /// <summary>
        /// Returns a hash value for the given state (used in Hash based data structures).
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = Constants.PRIMES_FOR_HASHING[0];
                hash = hash * Constants.PRIMES_FOR_HASHING[1] + base.GetHashCode();
                hash = hash * Constants.PRIMES_FOR_HASHING[2] + this.agentTurn;
                return hash;
            }
        }

        /// <summary>
        /// Returns false even if this is the same location and smaller g, as that's the behavior we need for the closed list.
        /// Also ignores the mirrorState, as that's also used by A*.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (((WorldStateWithOD)obj).agentTurn != this.agentTurn)
                return false;
            return base.Equals(obj);
        }

        /// <summary>
        /// Calculate and set the g of the state as the sum of the different agent g values.
        /// </summary>
        override public void CalculateG()
        {
            g = 0;
            for (int i = 0; i < allAgentsState.Length; i++)
            {
                AgentState singleAgentState = allAgentsState[i];
                if (singleAgentState.atGoal())
                    g += singleAgentState.arrivalTime;
                ///
                /// <remark> 
                /// The agents that have moved in this timestamp are all the agents until parent.agentTurn.
                /// Therefore, we add the makespan only to these agents, and the previous timestamp to the others.
                /// </remark>
                ///
                else if (i <= ((WorldStateWithOD)this.prevStep).agentTurn) // Agent already moved in this step.
                                                                           // (Using the parent's agentTurn is equivalent to using i <= (agentTurn-1)%num_agents)
                {
                    g += makespan;
                }   
                else 
                {
                    g += makespan - 1;
                }
            }
        }

        /// <summary>
        /// Returns count for last agent to move only.
        /// </summary>
        /// <param name="conflictAvoidance"></param>
        /// <returns></returns>
        override public int conflictsCount(HashSet<TimedMove> conflictAvoidance)
        {
            int ans = 0;
            int lastMove = agentTurn - 1;
            if (agentTurn == 0)
                lastMove = allAgentsState.Length - 1;
            if (allAgentsState[lastMove].last_move.isColliding(conflictAvoidance)) // Behavior change: this didn't check for head-on collisions
                ans++;
            return ans;
        }
    }
}
