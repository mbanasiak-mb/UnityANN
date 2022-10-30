using System;

namespace ANN
{
    public static class Functions
    {
        // {0, 1}
        public static double BinaryStep(double x)
        {
            return x < 0 ? 0 : 1;
        }

        // (0, 1)
        public static double Logistic(double x)
        {
            return 1 / (1 + Math.Exp(-x));
        }

        // (0, inf)
        public static double ReLU(double x)
        {
            return x < 0 ? 0 : x;
        }

        // (0, inf)
        public static double Softplus(double x)
        {
            return Math.Log(1 + Math.Exp(x));
        }

        // (0, inf)
        public static double SiLU(double x)
        {
            return x / (1 + Math.Exp(-x));
        }

        // (0, inf)
        public static double Mish(double x)
        {
            return x * Math.Tanh(Math.Log(1 + Math.Exp(x)));
        }

        // (-inf, inf)
        public static double Linear(double x)
        {
            return x;
        }

        // (-inf, inf)
        public static double LeakyReLU(double x)
        {
            return x < 0 ? 0.01 * x : x;
        }
    }
}