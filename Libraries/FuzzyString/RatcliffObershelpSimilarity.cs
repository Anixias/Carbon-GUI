﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuzzyString
{
    public static partial class ComparisonMetrics
    {
        public static double RatcliffObershelpSimilarity(this string source, string target)
        {
            return (2 * Convert.ToDouble(source.Supersect(target).Count())) / (Convert.ToDouble(source.Length + target.Length));
        }
    }
}
