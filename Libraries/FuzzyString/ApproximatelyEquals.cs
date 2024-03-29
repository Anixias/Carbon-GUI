﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuzzyString
{
	public static partial class ComparisonMetrics
	{ 
		public static bool ApproximatelyEquals(this string source, string target,  ComparisonTolerance tolerance, params ComparisonOptions[] options)
		{
			List<double> comparisonResults = new List<double>();
			
			if (!options.Contains(ComparisonOptions.CaseSensitive))
			{
				source = source.Capitalize();
				target = target.Capitalize();
			}
			
			// Min: 0    Max: source.Length = target.Length
			if (options.Contains(ComparisonOptions.UseHammingDistance))
			{
				if (source.Length == target.Length)
				{
					comparisonResults.Add(source.HammingDistance(target) / target.Length);
				}
			}
			
			// Min: 0    Max: 1
			if (options.Contains(ComparisonOptions.UseJaccardDistance))
			{
				comparisonResults.Add(source.JaccardDistance(target));
			}
			
			// Min: 0    Max: 1
			if (options.Contains(ComparisonOptions.UseJaroDistance))
			{
				comparisonResults.Add(source.JaroDistance(target));
			}
			
			// Min: 0    Max: 1
			if (options.Contains(ComparisonOptions.UseJaroWinklerDistance))
			{
				comparisonResults.Add(source.JaroWinklerDistance(target));
			}

			// Min: 0    Max: LevenshteinDistanceUpperBounds - LevenshteinDistanceLowerBounds
			// Min: LevenshteinDistanceLowerBounds    Max: LevenshteinDistanceUpperBounds
			if (options.Contains(ComparisonOptions.UseNormalizedLevenshteinDistance))
			{
				comparisonResults.Add(Convert.ToDouble(source.NormalizedLevenshteinDistance(target)) / Convert.ToDouble((Math.Max(source.Length, target.Length) - source.LevenshteinDistanceLowerBounds(target))));
			}
			else if (options.Contains(ComparisonOptions.UseLevenshteinDistance))
			{
				comparisonResults.Add(Convert.ToDouble(source.LevenshteinDistance(target)) / Convert.ToDouble(source.LevenshteinDistanceUpperBounds(target)));
			}

			if (options.Contains(ComparisonOptions.UseLongestCommonSubsequence))
			{
				comparisonResults.Add(1 - Convert.ToDouble((source.LongestCommonSubsequence(target).Length) / Convert.ToDouble(Math.Min(source.Length, target.Length))));
			}

			if (options.Contains(ComparisonOptions.UseLongestCommonSubstring))
			{
				comparisonResults.Add(1 - Convert.ToDouble((source.LongestCommonSubstring(target).Length) / Convert.ToDouble(Math.Min(source.Length, target.Length))));
			}

			// Min: 0    Max: 1
			if (options.Contains(ComparisonOptions.UseSorensenDiceDistance))
			{
				comparisonResults.Add(source.SorensenDiceDistance(target));
			}

			// Min: 0    Max: 1
			if (options.Contains(ComparisonOptions.UseOverlapCoefficient))
			{
				comparisonResults.Add(1 - source.OverlapCoefficient(target));
			}

			// Min: 0    Max: 1
			if (options.Contains(ComparisonOptions.UseRatcliffObershelpSimilarity))
			{
				comparisonResults.Add(1 - source.RatcliffObershelpSimilarity(target));
			}

			if (comparisonResults.Count == 0)
			{
				return false;
			}
			
			switch(tolerance)
			{
				default:
					return false;
				case ComparisonTolerance.Exact:
					return (comparisonResults.Average() == 0.0);
				case ComparisonTolerance.Strong:
					return (comparisonResults.Average() < 0.25);
				case ComparisonTolerance.Normal:
					return (comparisonResults.Average() < 0.5);
				case ComparisonTolerance.Weak:
					return (comparisonResults.Average() < 0.75);
				case ComparisonTolerance.Distinct:
					return (comparisonResults.Average() > 0.5);
				case ComparisonTolerance.Unique:
					return (comparisonResults.Average() > 0.7);
			}
		}
	}
}
