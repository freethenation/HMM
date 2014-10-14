using System;
using System.Linq;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using System.Collections.Generic;
using MathNet.Numerics.Random;

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

        #region Constructor
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
        public void Normalize()
        {
            this.IntialStateProbabilities.SetValues(this.IntialStateProbabilities.Normalize(1).ToArray());
            this.StateTransitionProbabilities.SetSubMatrix(0, 0, this.StateTransitionProbabilities.NormalizeRows(1));
            foreach (var fromState in States.Values)
                this.SymbolEmissionProbabilities[fromState]
                    .SetSubMatrix(0, 0, this.SymbolEmissionProbabilities[fromState].NormalizeRows(1));
        }
		public void Validate()
		{
			//validate IntialStateProbabilities
			if (!IntialStateProbabilities.Sum().AlmostEqual(1.0))
				throw new InvalidProgramException("IntialStateProbabilities must sum to 1");
			//validate StateTransitionProbabilities
			var rowSums = StateTransitionProbabilities.RowSums();
			for (int i = 0; i < rowSums.Count; i++) {
				if (!rowSums[i].AlmostEqual(1.0, 5))
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
        #endregion

        #region ForwardFunc
        /// <summary>
        /// The joint probabilty of being in a specific state at time t AND having seen observations O..t-1
        /// </summary>
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
            forward = forward.Memorize();
            //Precompute every 1000th time to avoid stack overflows... hack but fuck it
            for (int time = 1000; time < outputSequence.Length; time+=1000)
                States.Values.ForEach(state => forward(time, state));
            return forward;
		}
        #endregion

        #region BackwardFunc
        /// <summary>
        /// The probabilty of seeing the observations t+1...T given we are in a specific state at time t
        /// </summary>
        public HMMTrellisFunc<string, double> BackwardFunc(params string[] outputSequence)
        {
            return (time, state) => 
                BackwardFunc(outputSequence.Select(i => this.Alphabet[i]).ToArray())(time, States[state]);
        }
        /// <summary>
        /// The probabilty of seeing the observations t+1...T given we are in a specific state at time t
        /// </summary>
        public HMMTrellisFunc<int, double> BackwardFunc(params int[] outputSequence)
        {
            HMMTrellisFunc<int, double> backward = null;
            backward = (time, state)=>
            {
                if(time == outputSequence.Length) return 0; //aka (1.0).Log();
                return Util.Range(States.Count).Select(
                    (s) => backward(time + 1, s)
                    + StateTransitionProbabilities[state, s].Log()
                    + SymbolEmissionProbabilities[state][s, outputSequence[time]].Log())
                .LogSum();
            };
            backward = backward.Memorize();
            //Precompute every 1000th time to avoid stack overflows... hack but fuck it
            for (int time = outputSequence.Length-1000; time > 0; time-=1000)
                States.Values.ForEach(state => backward(time, state));
            return backward;
        }
        #endregion

        #region ViterbiPath
        private HMMTrellisFunc<int, ViterbiStep> ViterbiFunc(params int[] outputSequence)
        {
            return ViterbiFunc((fromState, toState, time) => SymbolEmissionProbabilities[fromState][toState, outputSequence[time-1]], outputSequence);
        }
        private HMMTrellisFunc<int, ViterbiStep> ViterbiFunc(Func<int, int, int, double> symbolEmissionProbFunc, params int[] outputSequence)
        {
            HMMTrellisFunc<int, ViterbiStep> viterbi = null;
            viterbi = (time, state) =>
            {
                if(time == 0) return new ViterbiStep(-1, state, this.IntialStateProbabilities[state].Log());
                return States.Values
                    .Select(fromState => 
                        new ViterbiStep(fromState, state,
                            viterbi(time-1, fromState).LogProbability 
                                + StateTransitionProbabilities[fromState, state].Log()
                                + symbolEmissionProbFunc(fromState, state, time).Log()
                        ))
                    .Largest(viterbiStep => viterbiStep.LogProbability);
            };
            viterbi = viterbi.Memorize();
            return viterbi;
        }
        public IEnumerable<ViterbiStep> ViterbiPath(params string[] outputSequence)
        {
            return ViterbiPath(outputSequence.Select(i => this.Alphabet[i]).ToArray());
        }
        public IEnumerable<ViterbiStep> ViterbiPath(params int[] outputSequence)
        {
            var viterbiFunc = this.ViterbiFunc(outputSequence);
            List<ViterbiStep> ret = new List<ViterbiStep>();
            //Add the finial state with the largest probability
            ret.Add(States.Values.Select(state => viterbiFunc(outputSequence.Length, state)).Largest(i => i.LogProbability));
            foreach (var time in Util.Range(outputSequence.Length).Reverse())
            {
                //Look at the last state added and return its FromState
                ret.Insert(0, viterbiFunc(time, ret[0].FromState));
            }
            return ret;
        }
        public IEnumerable<HMMSample> SampleHmm(int? seed = null)
        {
            if (seed == null) seed = (new Random()).Next();
            var rnd = new MersenneTwister(seed.Value);
            HMMSample state;
            {
                int initalState = rnd.Choose(this.IntialStateProbabilities);
                state = new HMMSample(this, -1, initalState, this.IntialStateProbabilities[initalState].Log(), -1);
            }
            while (true)
            {
                int fromState = state.ToState;
                int toState = rnd.Choose(StateTransitionProbabilities.Row(state.ToState));
                state = new HMMSample(this, fromState, toState, 
                                      state.LogProbability + StateTransitionProbabilities[fromState, toState].Log(),
                                      rnd.Choose(this.SymbolEmissionProbabilities[fromState].Row(toState)));
                yield return state;
            }
        }
        #endregion

        #region Misc Helper Functions
        public string GetStateName(int state)
        {
            return States.FirstOrDefault(i => i.Value == state).Key;
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
        #endregion
	}
}

