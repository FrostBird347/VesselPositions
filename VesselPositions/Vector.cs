using System;
using System.Text;
namespace VesselPositions
{
    public static class Vector
    {
        public static double[] Multiply(double[] lhs, double[] rhs)
        {
            double[] retVal = new double[lhs.Length];
            for (int i = 0; i < lhs.Length; i++)
            {
                retVal[i] = lhs[i] * rhs[i];
            }
            return retVal;
        }

        public static double[] Multiply(double[] lhs, double rhs)
        {
            double[] retVal = new double[lhs.Length];
            for (int i = 0; i < lhs.Length; i++)
            {
                retVal[i] = lhs[i] * rhs;
            }
            return retVal;
        }

        public static double[] Divide(double[] lhs, double[] rhs)
        {
            double[] retVal = new double[lhs.Length];
            for (int i = 0; i < lhs.Length; i++)
            {
                retVal[i] = lhs[i] / rhs[i];
            }
            return retVal;
        }

        public static double[] Divide(double lhs, double[] rhs)
        {
            double[] retVal = new double[rhs.Length];
            for (int i = 0; i < rhs.Length; i++)
            {
                retVal[i] = lhs / rhs[i];
            }
            return retVal;
        }

        public static double[] Divide(double[] lhs, double rhs)
        {
            double[] retVal = new double[lhs.Length];
            for (int i = 0; i < lhs.Length; i++)
            {
                retVal[i] = lhs[i] / rhs;
            }
            return retVal;
        }

        public static double Length(double[] input)
        {
            double total = 0;
            for (int i = 0; i < input.Length; i++)
            {
                total += input[i] * input[i];
            }
            return Math.Sqrt(total);
        }

        public static string GetString(double[] input, int round)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("[");
            for (int i = 0; i < input.Length; i++)
            {
                sb.Append(input[i].ToString("F" + round));
                if (i != input.Length - 1)
                {
                    sb.Append(", ");
                }
            }
            sb.Append("]");
            return sb.ToString();
        }
    }
}
