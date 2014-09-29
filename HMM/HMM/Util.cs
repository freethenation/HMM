using System;
using System.Linq;
using System.Collections.Generic;
using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;

namespace HMM
{
	public static class Util
	{
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
		public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> func)
		{
			foreach (var item in enumerable) {
				func(item);
			}
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
        public static Func<T1, T2, TResult> Memorize<T1, T2, TResult>(this Func<T1, T2, TResult> func)
        {
            var cache = new Dictionary<Tuple<T1,T2>, TResult>();
            return (arg1, arg2) =>
            {
                var tuple = Tuple.Create(arg1, arg2);
                TResult ret;
                if(cache.TryGetValue(tuple, out ret)) return ret;
                ret = func(arg1, arg2);
                cache[tuple] = ret;
                return ret;
            };
        }
	}
}

