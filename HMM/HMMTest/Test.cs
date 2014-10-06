using NUnit.Framework;
using System;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using HMM;
using System.Collections.Generic;
using System.Linq; 

namespace HMMTest
{
    using Dict = Dictionary<string, double>;

    [TestFixture()]
    public class UtilTests
    {
        [Test()]
        public void TestLargestBasic()
        {
            Assert.AreEqual("8", (new Tuple<string, int>[] { Tuple.Create("5", 5), Tuple.Create("8", 8), Tuple.Create("6", 6) }).Largest(i => i.Item2).Item1);
        }

        public void TestLargestEmptyIEnumerable()
        {
            try { (new Double[] { }).Largest(i => i); }
            catch (ArgumentException) { return; }
            Assert.True(false, "Should have returned in the catch");
        }
    }

	[TestFixture()]
	public class HMMTests
	{
        HMM.HMM hmm1 = null;
        HMM.HMM zakHmm1 = null;

        [SetUp()]
        public void Setup()
        {
            hmm1 = null;
            zakHmm1 = null;
        }

        public void InitHmm1()
        {
            hmm1 = new HMM.HMM(new string[3] { "Start", "Final1", "Final2" }, new string[2] { "A", "B" });
            hmm1.IntialStateProbabilities.SetValues(new double[] { 1, 0, 0 });
            hmm1.StateTransitionProbabilities.SetRow(0, new Double[] { 0, .5, .5 });

            hmm1.SymbolEmissionProbabilities[0].SetRow(1, new Double[] { 1, 0 });
            hmm1.SymbolEmissionProbabilities[0].SetRow(2, new Double[] { 0, 1 });
            hmm1.SymbolEmissionProbabilities[1].SetRow(1, new Double[] { 1, 0 });
            hmm1.SymbolEmissionProbabilities[2].SetRow(2, new Double[] { 0, 1 });

            hmm1.Validate();
        }

        public void InitZakHmm1()
        {
            zakHmm1 = new HMM.HMM(new string[] { "Sun", "Cloud", "Rain" }, new string[] { "Wet", "Damp", "Dry" });
            zakHmm1.IntialStateProbabilities.SetValues(new double[] { .25, .5, .25 });

            zakHmm1.StateTransitionProbabilities.SetRow(0, new Double[] { .9, .1, 0 });
            zakHmm1.StateTransitionProbabilities.SetRow(1, new Double[] { .5, 0, .5 });
            zakHmm1.StateTransitionProbabilities.SetRow(2, new Double[] { 0, .3, .7 });

            zakHmm1.SetSymbolEmissionProbabilities(0, 1, new Dict() { {"Damp",.5}, {"Dry",.5} });
            zakHmm1.SetSymbolEmissionProbabilities(0, 0, new Dict() { {"Dry",1} });
            zakHmm1.SetSymbolEmissionProbabilities(1, 0, new Dict() { {"Damp",.5}, {"Dry", .5} });
            zakHmm1.SetSymbolEmissionProbabilities(1, 2, new Dict() { {"Dry", .3}, {"Wet", .6}, {"Damp", .1} });
            zakHmm1.SetSymbolEmissionProbabilities(2, 2, new Dict() { {"Wet", .9}, {"Damp", .1} });
            zakHmm1.SetSymbolEmissionProbabilities(2, 1, new Dict() { {"Damp", .5}, {"Wet", .5} });

            zakHmm1.Validate();
        }

        [Test()]
        public void InitZakHmm1Test()
        { 
            InitZakHmm1();
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

        [Test()]
        public void ViterbiPathTest()
        {
            InitHmm1();
            var ret = hmm1.ViterbiPath("A", "A", "A");
            Assert.AreEqual(.5, ret.Last().Probability);
            Assert.AreEqual(new int[] {0, 1, 1, 1}, ret.Select(step => step.ToState).ToArray());
        }
	}
}

