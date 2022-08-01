﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuzzyString
{
    public static partial class ComparisonMetrics
    {
        public static double OverlapCoefficient(this string source, string target)
        {
			return (Convert.ToDouble(source.Supersect(target).Count())) / Convert.ToDouble(Math.Min(source.Length, target.Length));
        }
    }
}
