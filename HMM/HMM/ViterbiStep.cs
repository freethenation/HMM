using System;

namespace HMM
{
    public class ViterbiStep
    {
        public readonly int FromState;
        public readonly int ToState;
        public readonly double LogProbability;

        public ViterbiStep(int fromState, int toState, double logProbability)
        {
            FromState = fromState;
            ToState = toState;
            LogProbability = logProbability;
        }

        public double Probability
        {
            get { return LogProbability.Exp(); }
        }
    }
}

