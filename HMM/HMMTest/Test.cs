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
        HMM.HMM hmm1 = null;

        [SetUp()]
        public void Setup()
        {
            hmm1 = null;
        }

        public void InitHmm1()
        {
            hmm1 = new HMM.HMM(new string[3] { "Start", "Final1", "Final2" }, new string[2] { "A", "B" });

            hmm1.IntialStateProbabilities.Clear();
            hmm1.IntialStateProbabilities[0] = 1;

            hmm1.StateTransitionProbabilities.ClearRow(0);
            hmm1.StateTransitionProbabilities[0, 1] = .5;
            hmm1.StateTransitionProbabilities[0, 2] = .5;

            hmm1.SymbolEmissionProbabilities[0].Clear();
            hmm1.SymbolEmissionProbabilities[0][0, 1] = 1; //not used but must be set so HMM is valid
            hmm1.SymbolEmissionProbabilities[0][1, 0] = 1;
            hmm1.SymbolEmissionProbabilities[0][2, 1] = 1;

            hmm1.Validate();
        }

		[Test()]
		public void ConstrTest()
		{
            (new HMM.HMM(new string[3] { "In1", "Out1", "Out2" }, new string[2] { "Out1", "Out2" })).Validate();
		}

        [Test()]
        public void ForwardTest()
        {
            InitHmm1();
            Assert.AreEqual(.5, hmm1.ForwardFunc("A")(0, "Final1").Exp());
            Assert.AreEqual(.5, hmm1.ForwardFunc("A", "A")(0, "Final1").Exp());
            Assert.AreEqual(.5, hmm1.ForwardFunc("A", "A")(1, "Final1").Exp());
            Assert.AreEqual(0, hmm1.ForwardFunc("A")(0, "Final2").Exp());
        }

        [Test()]
        public void BackwardTest()
        {
            InitHmm1();
            Assert.AreEqual(1, hmm1.BackwardFunc("A", "A", "A")(0, "Final1").Exp());
            Assert.AreEqual(0, hmm1.BackwardFunc("B", "B", "B")(0, "Final1").Exp());
        }
	}
}

