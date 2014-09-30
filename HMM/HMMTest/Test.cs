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

            hmm1.SymbolEmissionProbabilities[0].SetRow(1, new Double[] { 1, 0});
            hmm1.SymbolEmissionProbabilities[0].SetRow(2, new Double[] { 0, 1});
            hmm1.SymbolEmissionProbabilities[1].SetRow(1, new Double[] { 1, 0 });
            hmm1.SymbolEmissionProbabilities[2].SetRow(2, new Double[] { 0, 1 });

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
            Assert.AreEqual(1, hmm1.ForwardFunc("A", "A", "A")(0, "Start").Exp());
            Assert.AreEqual(.5, hmm1.ForwardFunc("A", "A", "A")(1, "Final1").Exp());
            Assert.AreEqual(0, hmm1.ForwardFunc("B", "B", "B")(1, "Final1").Exp());
            Assert.AreEqual(.5, hmm1.ForwardFunc("B", "B", "B")(2, "Final2").Exp());
        }

        [Test()]
        public void BackwardTest()
        {
            InitHmm1();
            Assert.AreEqual(.5, hmm1.BackwardFunc("A", "A", "A")(0, "Start").Exp());
            Assert.AreEqual(1, hmm1.BackwardFunc("A", "A", "A")(1, "Final1").Exp());
            Assert.AreEqual(0, hmm1.BackwardFunc("B", "B", "B")(1, "Final1").Exp());
            Assert.AreEqual(1, hmm1.BackwardFunc("B", "B", "B")(2, "Final2").Exp());
        }
	}
}

