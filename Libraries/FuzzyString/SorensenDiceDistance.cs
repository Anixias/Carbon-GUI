﻿using System;
using System.Linq;

namespace FuzzyString
{
	public static partial class ComparisonMetrics
	{
		public static double SorensenDiceDistance(this string source, string target)
		{
			return 1 - source.SorensenDiceIndex(target);
		}

		public static double SorensenDiceIndex(this string source, string target)
		{
			return (2 * Convert.ToDouble(source.Supersect(target).Count())) / (Convert.ToDouble(source.Length + target.Length));
		}
	}
}
