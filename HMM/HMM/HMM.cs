using System;
using System.Linq;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using System.Collections.Generic;

namespace HMM
{
    public delegate TRETURN HMMTrellisFunc<TSTATE, TRETURN>(int time, TSTATE state);

	public class HMM
	{
        public Vector<double> IntialStateProbabilities;
        public Matrix<double> StateTransitionProbabilities;
        public Matrix<double>[] SymbolEmissionProbabilities;
        public Dictionary<string, int> States;
        public Dictionary<string, int> Alphabet;

		public HMM(IEnumerable<string> states, IEnumerable<string> alphabet)
		{
			States = states.Select((str, i) => new KeyValuePair<string,int>(str, i)).ToDictionary();
            Alphabet = alphabet.Select((str, i) => new KeyValuePair<string,int>(str, i)).ToDictionary();
            //init IntialStateProbabilities
			IntialStateProbabilities = Vector<double>.Build.Dense(States.Count);
			IntialStateProbabilities[0] = 1;
            //init StateTransitionProbabilities
			StateTransitionProbabilities = Matrix<double>.Build.DenseIdentity(States.Count);
            //init SymbolEmissionProbabilities
			SymbolEmissionProbabilities = new Matrix<double>[States.Count];
			for (int i = 0; i < States.Count; i++) {
				SymbolEmissionProbabilities[i] = Matrix<double>.Build.Dense(States.Count, Alphabet.Count);
                SymbolEmissionProbabilities[i].SetColumn(0, Vector<double>.Build.DenseOfConstant(States.Count, 1.0));
			}
		}

		public void Validate()
		{
			//validate IntialStateProbabilities
			if (!IntialStateProbabilities.Sum().AlmostEqual(1.0))
				throw new InvalidProgramException("IntialStateProbabilities must sum to 1");
			//validate StateTransitionProbabilities
			var rowSums = StateTransitionProbabilities.RowSums();
			for (int i = 0; i < rowSums.Count; i++) {
				if (!rowSums[i].AlmostEqual(1.0))
					throw new InvalidProgramException(string.Format("StateTransitionProbabilities must sum to 1 for state '{0}'", States.First(j => j.Value == i).Key));
			}
			//validate SymbolEmissionProbabilities
            var badEmissionProb = SymbolEmissionProbabilities
                .SelectMany((matrix, i) => matrix.RowSums().Select((sum, j) => Tuple.Create(i, j, sum)))
                .FirstOrDefault(i => !i.Item3.AlmostEqual(1.0));
            if (badEmissionProb != null)
                throw new InvalidProgramException(String.Format("SymbolEmissionProbabilities must sum to 1 from state '{0}' to state '{1}'", 
                                                                States.First(i => badEmissionProb.Item1 == i.Value).Key,
                                                                States.First(i => badEmissionProb.Item2 == i.Value).Key));
		}
        public HMMTrellisFunc<string, double> ForwardFunc(params string[] outputSequence)
		{
            return (time, state) => 
                ForwardFunc(outputSequence.Select(i => this.Alphabet[i]).ToArray())(time, States[state]);
		}
        /// <summary>
        /// The joint probabilty of being in a specific state at time t AND having seen observations O..t-1
        /// </summary>
        public HMMTrellisFunc<int, double> ForwardFunc(params int[] outputSequence)
		{
            HMMTrellisFunc<int, double> forward = null;
            forward = (time, state)=>
            {
                if (time == 0) return IntialStateProbabilities[state].Log();
                return Util.Range(States.Count).Select(
                    (s) => forward(time - 1, s)
                    + StateTransitionProbabilities[s, state].Log()
                    + SymbolEmissionProbabilities[s][state, outputSequence[time-1]].Log())
                .LogSum();
            };
            return forward.Memorize();
		}
        public HMMTrellisFunc<string, double> BackwardFunc(params string[] outputSequence)
        {
            return (time, state) => 
                BackwardFunc(outputSequence.Select(i => this.Alphabet[i]).ToArray())(time, States[state]);
        }
        public HMMTrellisFunc<int, double> BackwardFunc(params int[] outputSequence)
        {
            HMMTrellisFunc<int, double> backward = null;
            backward = (time, state)=>
            {
                if(time == outputSequence.Length) return 0; //aka (1.0).Log();
                return States.Select(
                    (trash, s) => backward(time + 1, s)
                    + StateTransitionProbabilities[state, s].Log()
                    + SymbolEmissionProbabilities[state][s, outputSequence[time]].Log())
                .LogSum();
            };
            return backward.Memorize();
        }
        private HMMTrellisFunc<int, ViterbiStep> ViterbiFunc(params int[] outputSequence)
        {
            HMMTrellisFunc<int, ViterbiStep> viterbi = null;
            viterbi = (time, state) =>
            {
                if(time == 0) return new ViterbiStep(-1, state, this.IntialStateProbabilities[state].Log());
                return States
                    .Select((trash, s) => 
                        new ViterbiStep(s, state,
                            viterbi(time -1, s).LogProbability + StateTransitionProbabilities[s, state].Log()
                                + SymbolEmissionProbabilities[s][state, outputSequence[time-1]].Log()
                        ))
                    .Largest(i=> i.LogProbability);
            };
            return viterbi.Memorize();
        }
        public IEnumerable<ViterbiStep> ViterbiPath(params string[] outputSequence)
        {
            return ViterbiPath(outputSequence.Select(i => this.Alphabet[i]).ToArray());
        }
        public IEnumerable<ViterbiStep> ViterbiPath(params int[] outputSequence)
        {
            var viterbiFunc = this.ViterbiFunc(outputSequence);
            List<ViterbiStep> ret = new List<ViterbiStep>();
            ret.Add(States.Select((trash, s) => viterbiFunc(outputSequence.Length, s)).Largest(i => i.LogProbability));
            foreach (var time in Util.Range(outputSequence.Length).Reverse())
            {
                ret.Insert(0, viterbiFunc(time, ret[0].FromState));
            }
            return ret;
        }
        public HMMParameterEstimator CreateParameterEstimator(params string[] outputSequence)
        {
            return CreateParameterEstimator(outputSequence.Select(i => this.Alphabet[i]).ToArray());
        }
        public HMMParameterEstimator CreateParameterEstimator(params int[] outputSequence)
        {
            return new HMMParameterEstimator(this, outputSequence);
        }
        public void SetSymbolEmissionProbabilities(int fromState, int toState, IDictionary<string, double> alphabetProbabilities)
        {
            SymbolEmissionProbabilities[fromState].ClearRow(toState);
            foreach (var alphabetProbability in alphabetProbabilities)
            {
                SymbolEmissionProbabilities[fromState][toState, Alphabet[alphabetProbability.Key]] = alphabetProbability.Value;
            }
        }
        public class HMMParameterEstimator
        {
            public readonly HMM Parent;
            public readonly int[] OutputSequence;
            public readonly HMMTrellisFunc<int, double> ForwardFunc;
            public readonly HMMTrellisFunc<int, double> BackwardFunc;

            public HMMParameterEstimator(HMM parentHMM, params int[] outputSequence)
            {
                Parent = parentHMM;
                OutputSequence = outputSequence;
                ForwardFunc = Parent.ForwardFunc(OutputSequence);
                BackwardFunc = Parent.BackwardFunc(OutputSequence);
            }

            public double ProbabilityOfOutput(int time)
            {
               return Parent.States
                    .Select((trash, state) => ForwardFunc(time, state) * BackwardFunc(time, state))
                    .LogSum();
            }

            public double ExpectedNumberOfTransitions(int time, int fromState, int toState)
            {
                return ForwardFunc(time, fromState) 
                    + Parent.StateTransitionProbabilities[fromState, toState].Log()
                    + Parent.SymbolEmissionProbabilities[fromState][toState, OutputSequence[time]].Log()
                    + BackwardFunc(time, toState)
                    - ProbabilityOfOutput(time);
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
}

