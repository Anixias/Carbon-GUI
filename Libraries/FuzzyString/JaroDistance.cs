﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuzzyString
{
    public static partial class ComparisonMetrics
    {
        public static double JaroDistance(this string source, string target)
        {
            int m = source.Supersect(target).Count();

            if (m == 0) { return 0; }
            else
            {
                string sourceTargetIntersetAsString = "";
                string targetSourceIntersetAsString = "";
                IEnumerable<char> sourceIntersectTarget = source.Supersect(target);
                IEnumerable<char> targetIntersectSource = target.Supersect(source);
                foreach (char character in sourceIntersectTarget) { sourceTargetIntersetAsString += character; }
                foreach (char character in targetIntersectSource) { targetSourceIntersetAsString += character; }
                double t = sourceTargetIntersetAsString.LevenshteinDistance(targetSourceIntersetAsString) / 2;
                return ((m / source.Length) + (m / target.Length) + ((m - t) / m)) / 3;
            }
        }
    }
}
