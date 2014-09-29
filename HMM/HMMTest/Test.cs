using NUnit.Framework;
using System;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using HMM;

namespace HMMTest
{
	[TestFixture()]
	public class HMMTests
	{
		[Test()]
		public void ConstrTest()
		{
            (new HMM.HMM(new string[3] { "In1", "Out1", "Out2" }, new string[2] { "Out1", "Out2" })).Validate();
		}

        [Test()]
        public void ForwardTest()
        {
            var hmm = new HMM.HMM(new string[3] { "In1", "Out1", "Out2" }, new string[2] { "Out1", "Out2" });

            hmm.IntialStateProbabilities.Clear();
            hmm.IntialStateProbabilities[0] = 1;

            hmm.StateTransitionProbabilities.ClearRow(0);
            hmm.StateTransitionProbabilities[0, 1] = .5;
            hmm.StateTransitionProbabilities[0, 2] = .5;
       
            hmm.SymbolEmissionProbabilities[0].Clear();
            hmm.SymbolEmissionProbabilities[0][0, 1] = 1; //not used
            hmm.SymbolEmissionProbabilities[0][1, 0] = 1;
            hmm.SymbolEmissionProbabilities[0][2, 1] = 1;

            hmm.Validate();

            Assert.AreEqual(.5, hmm.Forward(0, "Out1", "Out1").Exp());
            Assert.AreEqual(.5, hmm.Forward(0, "Out1", "Out1", "Out1").Exp());
            Assert.AreEqual(.5, hmm.Forward(1, "Out1", "Out1", "Out1").Exp());
            Assert.AreEqual(0, hmm.Forward(0, "Out2", "Out1").Exp());
        }
	}
}

