using System;
using System.Linq;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using System.Collections.Generic;

namespace HMM
{
	public class HMM
	{
        public Vector<double> IntialStateProbabilities;
        public Matrix<double> StateTransitionProbabilities;
        public Matrix<double>[] SymbolEmissionProbabilities;
        public Dictionary<string, int> States;
        public Dictionary<string, int> Alphabet;

		public HMM (IEnumerable<string> states, IEnumerable<string> alphabet)
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
        /// <returns>(time, state)=></returns>
        public Func<int, string, double> ForwardFunc(params string[] outputSequence)
		{
            return (time, state) => 
                ForwardFunc(outputSequence.Select(i => this.Alphabet[i]).ToArray())(time, States[state]);
		}
        /// <returns>(time, state)=></returns>
        public Func<int, int, double> ForwardFunc(params int[] outputSequence)
		{
            Func<int, int, double> forward = null;
            forward = (time, state)=>
            {
                if (time == 0) return IntialStateProbabilities[state].Log();
                return States.Select(
                    (trash, s) => forward(time - 1, s)
                    + StateTransitionProbabilities[s, state].Log()
                    + SymbolEmissionProbabilities[s][state, outputSequence[time-1]].Log())
                .LogSum();
            };
            return forward.Memorize();
		}
        /// <returns>(time, state)=></returns>
        public Func<int, string, double> BackwardFunc(params string[] outputSequence)
        {
            return (time, state) => 
                BackwardFunc(outputSequence.Select(i => this.Alphabet[i]).ToArray())(time, States[state]);
        }
        /// <returns>(time, state)=></returns>
        public Func<int, int, double> BackwardFunc(params int[] outputSequence)
        {
            Func<int, int, double> backward = null;
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
	}
}

