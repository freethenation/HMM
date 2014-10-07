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

        public void UpdateHMM()
        {
            //Calculate new model variables
            double[] newIntialStateProbabilities = Parent.IntialStateProbabilities.Select(
                (trash, state) => ExpectedNumberOfTransitions(0, state).Exp()
            ).ToArray();
            double[][] newTransitionProbabilities = Parent.StateTransitionProbabilities
                .EnumerateRows()
                .Select(
                    (row, fromState) =>
                    {
                        return row.Select(
                            (oldProb, toState) => (TotalExpectedNumberOfTransitions(fromState, toState) - TotalExpectedNumberOfTransitions(fromState)).Exp()
                        ).ToArray();
                    }
                ).ToArray();
            double[][][] newSymbolEmissionProbabilities = Parent.SymbolEmissionProbabilities
                .Select((matrix, fromState) =>
                    matrix.EnumerateRows().Select(
                        (row, toState) =>
                        {
                            return row.Select(
                                (oldProb, symbol) => (TotalExpectedNumberOfTransitions(fromState, toState, symbol) - TotalExpectedNumberOfTransitions(fromState, toState)).Exp()
                            ).ToArray();
                        }
                   ).ToArray()
                ).ToArray();

            //Update the model!
            Parent.IntialStateProbabilities.SetValues(newIntialStateProbabilities.ToArray());
            newTransitionProbabilities.ForEach(
                (row, fromState) => Parent.StateTransitionProbabilities.SetRow(fromState, row)
            );
            newSymbolEmissionProbabilities.ForEach(
                (matrix, fromState) => matrix.ForEach(
                    (row, toState) => Parent.SymbolEmissionProbabilities[fromState].SetRow(toState, row)
                )
           );     
        }
        public double ExpectedNumberOfTransitions(int time, int fromState, int toState)
        {
            return ForwardFunc(time, fromState) 
                + Parent.StateTransitionProbabilities[fromState, toState].Log()
                + Parent.SymbolEmissionProbabilities[fromState][toState, OutputSequence[time]].Log()
                + BackwardFunc(time, toState)
                - ProbabilityOfOutput;
        }
        public double TotalExpectedNumberOfTransitions(int fromState, int toState, int outputedSymbol)
        {
            return Util.Range(OutputSequence.Length)
                .Where(time => OutputSequence[time] == outputedSymbol)
                .Select((time) => ExpectedNumberOfTransitions(time, fromState, toState))
                .LogSum();
        }
        public double TotalExpectedNumberOfTransitions(int fromState, int toState)
        {
            return Util.Range(OutputSequence.Length)
                .Select((time) => ExpectedNumberOfTransitions(time, fromState, toState))
                .LogSum();
        }
        public double ExpectedNumberOfTransitions(int time, int fromState)
        {
            return Parent.States.Values
                .Select(state => ExpectedNumberOfTransitions(time, fromState, state))
                .LogSum();
        }
        public double TotalExpectedNumberOfTransitions(int fromState)
        {
            return OutputSequence
                .Select((trash, time) => ExpectedNumberOfTransitions(time, fromState))
                .LogSum();
        }            
    }
}

