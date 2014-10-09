using NUnit.Framework;
using System;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using HMM;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.Random;

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
        HMM.HMM zakHmm = null;

        [SetUp()]
        public void Setup()
        {
            hmm1 = null;
            zakHmm = null;
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

        [Test()]
        public void LogAddTest()
        {
            Assert.True(Util.LogAdd(Util.Log(20), Util.Log(60)).Exp().AlmostEqual(80, .1));
            Assert.True((new Double[] { Util.Log(20), Util.Log(60), Util.Log(20) }).LogSum().Exp().AlmostEqual(100, .1));
        }

        public void InitZakHmm()
        {
            zakHmm = new HMM.HMM(new string[] { "Sun", "Cloud", "Rain" }, new string[] { "Wet", "Damp", "Dry" });
            zakHmm.IntialStateProbabilities.SetValues(new double[] { .25, .5, .25 });

            zakHmm.StateTransitionProbabilities.SetRow(0, new Double[] { .9, .1, 0 });
            zakHmm.StateTransitionProbabilities.SetRow(1, new Double[] { .5, 0, .5 });
            zakHmm.StateTransitionProbabilities.SetRow(2, new Double[] { 0, .3, .7 });

            zakHmm.SetSymbolEmissionProbabilities(0, 1, new Dict() { {"Damp", .1}, {"Dry", .9} });
            zakHmm.SetSymbolEmissionProbabilities(0, 0, new Dict() { {"Dry",1} });
            zakHmm.SetSymbolEmissionProbabilities(1, 0, new Dict() { {"Damp",.5}, {"Dry", .5} });
            zakHmm.SetSymbolEmissionProbabilities(1, 2, new Dict() { {"Dry", .3}, {"Wet", .6}, {"Damp", .1} });
            zakHmm.SetSymbolEmissionProbabilities(2, 2, new Dict() { {"Wet", .9}, {"Damp", .1} });
            zakHmm.SetSymbolEmissionProbabilities(2, 1, new Dict() { {"Damp", .5}, {"Wet", .5} });

            zakHmm.Validate();
        }

		[Test()]
		public void ConstrTest()
		{
            (new HMM.HMM(new string[3] { "In1", "Out1", "Out2" }, new string[2] { "Out1", "Out2" })).Validate();
		}

        public void AssertAlmostEqual(double expected, double actual)
        {
            if (expected == actual)
                return;
            if (!expected.AlmostEqual(actual, Math.Min(.001, Math.Max(Math.Abs(expected), Math.Abs(actual)) * 0.01)))
                Assert.Fail(String.Format("expected {0} does not almost equal {1}", expected, actual));
        }

        [Test()]
        public void ForwardBackwardTest()
        {
            InitZakHmm();
            var symbols = (new string[] { "Wet", "Dry", "Damp", "Dry", "Damp", "Wet" })
                .Select(i => zakHmm.Alphabet[i])
                    .ToArray();
            var forwardFunc = zakHmm.ForwardFunc(symbols);
            var backwardFunc = zakHmm.BackwardFunc(symbols);

            double combined = Util.Range(zakHmm.States.Count)
                .Select(state => forwardFunc(3, state) + backwardFunc(3, state))
                .LogSum();
            double backward = Util.Range(zakHmm.States.Count)
                .Select(state => forwardFunc(0, state) + backwardFunc(0, state))
                .LogSum();
            double forward = Util.Range(zakHmm.States.Count)
                .Select(state => forwardFunc(6, state))
                .LogSum();

            AssertAlmostEqual(backward, forward);
            AssertAlmostEqual(combined, backward);
            AssertAlmostEqual(combined, forward);
        }

        [Test()]
        public void ZakForwardTest()
        {
            InitZakHmm();

            var forwardFunc = zakHmm.ForwardFunc("Wet", "Dry", "Damp", "Dry", "Damp", "Wet");
            Assert.AreEqual(.25, forwardFunc(0, "Rain").Exp()); //Just intial prob of rain

            AssertAlmostEqual(0.000000, forwardFunc(1, "Sun").Exp()); //It was wet at time 0 therefore it can not be sunny yet
            AssertAlmostEqual(0.307500, forwardFunc(1, "Rain").Exp());
            AssertAlmostEqual(0.037500, forwardFunc(1, "Cloud").Exp());

            AssertAlmostEqual(0.005625, forwardFunc(2, "Rain").Exp());
            AssertAlmostEqual(0.009375, forwardFunc(2, "Sun").Exp());
            AssertAlmostEqual(0.000000, forwardFunc(2, "Cloud").Exp());


            AssertAlmostEqual(0.234375e-3, forwardFunc(4, "Sun").Exp());
            AssertAlmostEqual(0.000000 , forwardFunc(4, "Cloud").Exp());
            AssertAlmostEqual(.140625e-3 , forwardFunc(4, "Rain").Exp());

            AssertAlmostEqual(0.000000, forwardFunc(5, "Sun").Exp());
            AssertAlmostEqual(.234375e-4 , forwardFunc(5, "Cloud").Exp());
            AssertAlmostEqual(.984e-5 , forwardFunc(5, "Rain").Exp());
        }

        [Test()]
        public void ZakBackwardTest()
        {
            InitZakHmm();

            var backwardFunc = zakHmm.BackwardFunc("Wet", "Dry", "Damp", "Dry", "Damp", "Wet");

            AssertAlmostEqual(0.588375e-4, backwardFunc(0, "Rain").Exp());
            AssertAlmostEqual(0, backwardFunc(0, "Sun").Exp());
            AssertAlmostEqual(0, backwardFunc(0, "Cloud").Exp());

            AssertAlmostEqual(0.000280, backwardFunc(1, "Sun").Exp());
            AssertAlmostEqual(0.000000, backwardFunc(1, "Rain").Exp());
            AssertAlmostEqual(0.000392, backwardFunc(1, "Cloud").Exp());

            AssertAlmostEqual(0.002353, backwardFunc(2, "Rain").Exp());
            AssertAlmostEqual(0.000156, backwardFunc(2, "Sun").Exp());
            AssertAlmostEqual(0.001552, backwardFunc(2, "Cloud").Exp());

            AssertAlmostEqual(0.099600, backwardFunc(4, "Rain").Exp());
            AssertAlmostEqual(0.003000, backwardFunc(4, "Sun").Exp());
            AssertAlmostEqual(0.039000, backwardFunc(4, "Cloud").Exp());

            AssertAlmostEqual(0.780000, backwardFunc(5, "Rain").Exp());
            AssertAlmostEqual(0.000000, backwardFunc(5, "Sun").Exp());
            AssertAlmostEqual(0.300000, backwardFunc(5, "Cloud").Exp());
        }

        [Test()]
        public void ZakViterbiPathTest()
        {
            InitZakHmm();
            var viterbiPath = zakHmm.ViterbiPath("Wet", "Dry", "Damp", "Dry", "Damp", "Wet");
            Assert.AreEqual(new String[] { "Rain", "Cloud", "Rain", "Cloud", "Rain", "Cloud", "Rain" }, 
                viterbiPath.Select(i => i.ToState).Select(i => zakHmm.GetStateName(i)).ToArray());
            AssertAlmostEqual(5.69531E-6, viterbiPath.Last().Probability);
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

        [Test()]
        public void ChooseTest()
        {
            var rnd = new MersenneTwister(3);
            Assert.AreEqual(2, rnd.Choose(new double[] { .1, .2, .3, .3, .1 }));
            Assert.AreEqual(0, rnd.Choose(new double[] { .1, .2, .3, .3, .1 }));
            Assert.AreEqual(3, rnd.Choose(new double[] { .1, .2, .3, .3, .1 }));
            rnd = new MersenneTwister(3);
            for (int i = 0; i < 30; i++)
                Assert.AreEqual(1, rnd.Choose(new double[] { 0, 1, 0, 0, 0 }));
        }

        [Test()]
        public void SampleTest()
        {
            InitZakHmm();
            //var t = MathNet.Numerics.LinearAlgebra.MatrixModule.eigen<Matrix<double> , double>(zakHmm.StateTransitionProbabilities);
            int sampleSize = 100000;
            var sample = zakHmm.SampleHmm(14).Take(sampleSize).ToArray();
            //var sunProb = sample.Where(i => i.Symbol == i.Parent.States["Sun"]).Count() / (double)sampleSize;
            //var cloudProb = sample.Where(i => i.Symbol == i.Parent.States["Cloud"]).Count() / (double)sampleSize;
            //var rainProb = sample.Where(i => i.Symbol == i.Parent.States["Rain"]).Count() / (double)sampleSize;
            var wetProb = sample.Where(i => i.SymbolName == "Wet").Count() / (double)sampleSize;
            var dryProb = sample.Where(i => i.SymbolName == "Dry").Count() / (double)sampleSize;
            var dampProb = sample.Where(i => i.SymbolName == "Damp").Count() / (double)sampleSize;
            AssertAlmostEqual(.209, wetProb);
            AssertAlmostEqual(.698, dryProb);
            AssertAlmostEqual(.094, dampProb);
        }

        /*
        [Test()]
        public void ParameterEstimator()
        {
        }
        */
	}
}

