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
            hmm1 = new HMM.HMM(new string[3] { "In1", "Out1", "Out2" }, new string[2] { "Out1", "Out2" });

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
            Assert.AreEqual(.5, hmm1.ForwardFunc("Out1")(0, "Out1").Exp());
            Assert.AreEqual(.5, hmm1.ForwardFunc("Out1", "Out1")(0, "Out1").Exp());
            Assert.AreEqual(.5, hmm1.ForwardFunc("Out1", "Out1")(1, "Out1").Exp());
            Assert.AreEqual(0, hmm1.ForwardFunc("Out1")(0, "Out2").Exp());
        }

        [Test()]
        public void BackwardTest()
        {
            InitHmm1();
            Assert.AreEqual(1, hmm1.BackwardFunc("Out1", "Out1")(0, "Out1").Exp());
            Assert.AreEqual(0, hmm1.BackwardFunc("Out1", "Out2")(0, "Out1").Exp());
        }
	}
}

