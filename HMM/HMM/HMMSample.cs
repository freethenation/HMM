using System;
using System.Linq;

namespace HMM
{
    public class HMMSample
    {
        public readonly int FromState;
        public readonly int ToState;
        public readonly double LogProbability;
        public readonly int Symbol;
        public readonly HMM Parent;
        public HMMSample(HMM parent, int fromState, int toState, double logProbability, int symbol)
        {
            Parent = parent;
            FromState = fromState;
            ToState = toState;
            LogProbability = logProbability;
            Symbol = symbol;
        }
        public string SymbolName 
        { 
            get { return Parent.Alphabet.First(i => i.Value == this.Symbol).Key; } 
        }
        public string FromStateName
        {
            get { return Parent.States.First(i => i.Value == this.FromState).Key; } 
        }
        public string ToStateName
        {
            get { return Parent.States.First(i => i.Value == this.ToState).Key; } 
        }
        public double Probability
        {
            get { return LogProbability.Exp(); }
        }
    }
}

