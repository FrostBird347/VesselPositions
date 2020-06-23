using System;
using System.IO;
using System.Text;
using DarkMultiPlayerServer;
using VesselPositions;

namespace VesselPositions
{
	public static class PointTime
	{
		public static Double GetTime(Double currentSubspaceTime, Double AmountAdded)
		{
			Double v = 250;
			Double i = currentSubspaceTime + AmountAdded;
			return Math.Ceiling(i / v) * v;
		}

		public static Double GetTimePercent(Double currentSubspaceTime)
		{
			Double LocalTime = currentSubspaceTime - GetTime(currentSubspaceTime - 250, 0);
			Double LocalTimePercent = ( LocalTime / 250 ) * 100;
			return LocalTimePercent;
		}
	}
}
