using System;
using System.Linq;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using System.Collections.Generic;

namespace HMM
{
    public class HMMParameterEstimator
    {
        public readonly HMM Parent;
        public readonly int[] OutputSequence;
        public readonly HMMTrellisFunc<int, double> ForwardFunc;
        public readonly HMMTrellisFunc<int, double> BackwardFunc;
        public readonly double ProbabilityOfOutput;

        public HMMParameterEstimator(HMM parentHMM, params int[] outputSequence)
        {
            Parent = parentHMM;
            OutputSequence = outputSequence;
            ForwardFunc = Parent.ForwardFunc(OutputSequence);
            BackwardFunc = Parent.BackwardFunc(OutputSequence);
            ProbabilityOfOutput = Util.Range(Parent.States.Count)
                .Select(state => ForwardFunc(OutputSequence.Length, state))
                    .LogSum();
        }
        //public double ProbabilityOfOutput(int time) { return Parent.States.Select((trash, state) => ForwardFunc(time, state) * BackwardFunc(time, state)).LogSum(); }


        public double ExpectedNumberOfTransitions(int time, int fromState, int toState)
        {
            return ForwardFunc(time, fromState) 
                + Parent.StateTransitionProbabilities[fromState, toState].Log()
                    + Parent.SymbolEmissionProbabilities[fromState][toState, OutputSequence[time]].Log()
                    + BackwardFunc(time, toState)
                    - ProbabilityOfOutput;
        }
        public double TotalExpectedNumberOfTransitions(int fromState, int toState)
        {
            return OutputSequence
                .Select((trash, time) => ExpectedNumberOfTransitions(time, fromState, toState))
                    .LogSum();
        }

        public double ExpectedNumberOfTransitions(int time, int toState)
        {
            return Parent.States
                .Select((trash, state) => ExpectedNumberOfTransitions(time, state, toState))
                    .LogSum();
        }
        public double TotalExpectedNumberOfTransitions(int toState)
        {
            return OutputSequence
                .Select((trash, time) => ExpectedNumberOfTransitions(time, toState))
                    .LogSum();
        }            
    }
}

