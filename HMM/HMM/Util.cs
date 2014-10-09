using System;
using System.Linq;
using System.Collections.Generic;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Random;

namespace HMM
{
	public static class Util
	{
        public static int Choose(this RandomSource rnd, IEnumerable<double> probabilities)
        {
            var r = rnd.NextDouble()*-1;
            var ret = probabilities
                .Select((prob, index) => new { Prob = prob, Index = index })
                .FirstOrDefault(
                    prob => 
                    {
                        r += prob.Prob;
                        return r > 0;
                    }
                );
            if (ret != null) return ret.Index;
            else return probabilities.Count() - 1; //last probability
        }
		public static IEnumerable<int> Range(int start, int end)
		{
			for (int i = start; i < end; i++) {
				yield return i;
			}
		}
		public static IEnumerable<int> Range(int end)
		{
			return Range(0, end);
		}
        public static void ForEach<T>(this IEnumerable<T> enumerable)
        {
            foreach (var item in enumerable) { }
        }
		public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> func)
		{
			foreach (var item in enumerable) {
				func(item);
			}
		}
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T, int> func)
        {
            int index = 0;
            foreach (var item in enumerable) {
                func(item, index);
                index++;
            }
        }
        public static T Largest<T, TC>(this IEnumerable<T> enumerable, Func<T, TC> func) where TC : IComparable
        {
            var e = enumerable.GetEnumerator();
            if (!e.MoveNext())
                throw new System.ArgumentException("No elements in IEnumerable");
            T ele = e.Current;
            TC max = func(e.Current);
            while (e.MoveNext())
            {
                TC currMax = func(e.Current);
                if (currMax.CompareTo(max) > 0)
                {
                    ele = e.Current;
                    max = currMax;
                }
            }
            return ele;
        }
        public static Dictionary<TK, TV> ToDictionary<TK, TV>(this IEnumerable<KeyValuePair<TK, TV>> enumerable)
        {
            return enumerable.ToDictionary(i => i.Key, i => i.Value);
        }
        public static IEnumerable<KeyValuePair<int, T>> ToKeyValues<T>(this IEnumerable<T> enumerable)
        {
            return enumerable.Select((v,i) => new KeyValuePair<int, T>(i, v));
        }
        public static Vector<T> DenseOfConstant<T>(this VectorBuilder<T> builder, int length, T constant)  where T : struct, IEquatable<T>, IFormattable
        {
            return builder.DenseOfEnumerable(Range(length).Select(i => constant));
        }
        public static double Log(this double i)
        {
            if (i == 0) return LOG_ZERO;
            return Math.Log(i);
        }
        public static double Exp(this double i)
        {
            if (i <= LOG_ZERO + 1000) return 0;
            return Math.Exp(i);
        }
        public static double LogAdd(this double i, double j)
        {
            if (i - j > 70) return i;
            else if (j - i > 70) return j;
            double min = Math.Min(i,j);
            return min + Math.Log(Math.Exp(i - min) + Math.Exp(j - min));
        }
        public const double LOG_ZERO = -999999999999;
        public static double LogSum(this IEnumerable<double> enumerable)
        {
            double ret = LOG_ZERO;
            foreach (var i in enumerable) ret = ret.LogAdd(i);
            return ret;
        }
        public static HMMTrellisFunc<TSTATE, TRETURN> Memorize<TSTATE, TRETURN>(this HMMTrellisFunc<TSTATE, TRETURN> func)
        {
            var cache = new Dictionary<Tuple<int,TSTATE>, TRETURN>();
            return (arg1, arg2) =>
            {
                var tuple = Tuple.Create(arg1, arg2);
                TRETURN ret;
                if(cache.TryGetValue(tuple, out ret)) return ret;
                ret = func(arg1, arg2);
                cache[tuple] = ret;
                return ret;
            };
        }
	}
}

