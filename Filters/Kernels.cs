namespace ConvolutionalMasks.Filters
{
    public static class Kernels
    {
        public static readonly double[,] Blur = {
            { 0.0625, 0.125, 0.0625 },
            { 0.125,  0.25,  0.125  },
            { 0.0625, 0.125, 0.0625 }
        };

        public static readonly double[,] Sharpen = {
            { 0, -1, 0 },
            { -1, 5, -1 },
            { 0, -1, 0 }
        };

        public static readonly double[,] Emboss = {
            { -2, -1, 0 },
            { -1,  1, 1 },
            {  0,  1, 2 }
        };    

        public static double GetFactor(double[,] kernel)
        {
            double sum = 0.0;
            int h = kernel.GetLength(0);
            int w = kernel.GetLength(1);
            for (int i = 0; i < h; i++)
                for (int j = 0; j < w; j++)
                    sum += kernel[i, j];
            return Math.Abs(sum) > double.Epsilon ? 1.0 / sum : 1.0;
        }
    }
}
