﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace CPF_experiment
{
    public class DnCConstraint : IComparable
    {
        protected byte[] agents;
        protected TimedMove move;
      //public byte group;
      //bool onVartex;
        public static bool fullyEqual;

        public DnCConstraint(int agent, int posX, int posY, Move.Direction direction, int timeStep)
        {
            this.agents = new byte[1] { (byte)agent };
            this.move = new TimedMove(posX, posY, direction, timeStep);
          //this.onVartex = onVartex;
        }

        public DnCConstraint() {}

        public DnCConstraint(DnCConflict conflict, ProblemInstance instance, bool agentA)
        {
            Move move;
            int agentNum;
            if (agentA)
            {
                move = conflict.agentAmove;
                agentNum = instance.m_vAgents[conflict.agentA].agent.agentNum;
            }
            else
            {
                move = conflict.agentBmove;
                agentNum = instance.m_vAgents[conflict.agentB].agent.agentNum;
            }
            this.agents = new byte[1] { (byte)agentNum };

            this.move = new TimedMove(move, conflict.timeStep);
          //this.onVartex = conflict.vartex;
            if (conflict.vartex)
                this.move.direction = Move.Direction.NO_DIRECTION;
        }

        public void init(int agent, int posX, int posY, Move.Direction direction, int timeStep)
        {
            this.agents = new byte[1] { (byte)agent };
            this.move.setup(posX, posY, direction, timeStep);
        }

        public void init(int agent, TimedMove move)
        {
            this.agents = new byte[1] { (byte)agent };
            this.move.setup(move);
        }

        /// <summary>
        /// If fullyEqual, checks that the agent sets are equal, otherwise checks that this.agents is a subset of obj.agents.
        /// If not fullyEqual, this doesn't implement commutativity!
        /// Always compares the move.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            DnCConstraint other = (DnCConstraint)obj;
            if (fullyEqual && (sameAgents(other) == false))
                return false;

            // This only checks that this.agents is a subset of other.agents!
            foreach (byte agent in this.agents)
            {
                if (other.agents.Contains(agent) == false)
                    return false;
            }
            //if (obj.GetType().Equals(this.GetType()))
            //    if (this.group != ((DnCConstraint)obj).group)
            //         return false;
            return this.move.Equals(other.move);
        }

        public void addAgents(List<byte> addAgents)
        {
            // Because manually fiddling with arrays is the best!
            if (addAgents.Count == 0)
                return;
            byte[] newAgents = new byte[agents.Length + addAgents.Count];
            Array.Copy(agents, newAgents, agents.Length);
            Array.Copy(addAgents.ToArray<byte>(), 0, newAgents, agents.Length, addAgents.Count);
            agents = newAgents;
        }

        /// <summary>
        /// Only actually compares the number of agents :(
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool sameAgents(DnCConstraint other)
        {
            if (this.agents.Length != other.agents.Length)
                return false;
            //foreach (byte agents in this.agents)
            //{
            //    if (((DnCConstraint)obj).agents.Contains(agents) == false)
            //        return false;
            //}
            return true;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int ans = 0;
                ans += move.GetHashCode() * 3;
              //ans += agents * 79;
                return ans;
            }
        }

        public int getX() { return this.move.x; }
        public int getY() { return this.move.y; }
        //public int getAgentNum() { return this.agents; }
        public int getTimeStep() { return this.move.time; }

        public Move.Direction getDirection()
        {
            return this.move.direction;
        }
        
        public override string ToString()
        {
            return move.ToString() + " direction-{" + move.direction + "} time-{" + move.time + "}";
        }

        /// <summary>
        /// Kind of the opposite of Equals: checks that the moves are unequal or that not one of the other's agents appears in this.agents.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool allows(DnCConstraint other)
        {
            if (this.move.Equals(other.move) == false) // Minor behavior change: if exactly one move has a set direction, and they're otherwise equal the method used to return true.
                return true;
            foreach (byte agent in other.agents)
            {
                if (this.agents.Contains(agent))
                    return false;
            }
            return true;
        }

        public int CompareTo(object item)
        {
            DnCConstraint other = (DnCConstraint)item;

            return this.move.time.CompareTo(other.move.time);
        }

        /// <summary>
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="posX"></param>
        /// <param name="posY"></param>
        /// <param name="timeStep"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public bool violatesMustCond(byte agent, int posX, int posY, Move.Direction direction, int timeStep)
        {
            return this.violatesMustCond(agent, new TimedMove(posX, posY, direction, timeStep));
        }

        public bool violatesMustCond(byte agent, TimedMove move)
        {
            if (agents.Contains<byte>(agent) == false)
                return false;
            return this.move.Equals(move) == false;
        }
    }
}
