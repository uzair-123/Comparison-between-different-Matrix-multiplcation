using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Threading;

namespace Test
{
    class Matrix
    {
        decimal[,] matA;
        decimal[,] matB;
        decimal[,] matC;
        long n, m, o;

        class Element
        {
            public long x, y;
        }
        public void RandomizeMatAB(int n, int m, int o)
        {
       
            Random random = new Random();
            double min = 0.24;
            double max = 1;
            this.m = m;
            this.n = n;
            this.o = o;
            matA = new decimal[n, m];
            matB = new decimal[m, o];
            matC = new decimal[n, o];
            var rnd = new Random(int.Parse(DateTime.Now.ToString("HHmmssfff")) + (int)(n * o));
            for (long i = 0; i < n; ++i)
            {
                for (long j = 0; j < m; ++j)
                {
                    matA[i, j] = Math.Round(Convert.ToDecimal(random.NextDouble() * (max - min) + min),2);
                    
                }
            }
            for (long i = 0; i < m; ++i)
            {
                for (long j = 0; j < o; ++j)
                {
                    matB[i, j] = Math.Round(Convert.ToDecimal(random.NextDouble() * (max - min) + min), 2);
                }
            }
        }
        public void SeqMultiply()
        {
            for (long i = 0; i < n; ++i)
            {
                for (long j = 0; j < o; ++j)
                {
                    for (long k = 0; k < m; ++k)
                    {
                        matC[i, j] += matA[i, k] * matB[k, j];
                    }
                }
            }
        }
        private void ComputeElement(Object _elem)
        {
            Element elem = (Element)_elem;
            matC[elem.x, elem.y] = 0;
            for (int i = 0; i < m; ++i)
            {
                matC[elem.x, elem.y] += matA[elem.x, i] * matB[i, elem.y];
            }
        }
        private void ComputeElements(Object _range)
        {
            Element range = (Element)_range;
            for (long i = range.x; i < range.y; ++i)
            {
                for (long j = range.x; j < range.y; ++j)
                {
                    Element elem = new Element();
                    elem.x = i;
                    elem.y = j;
                    for (int k = 0; k < m; ++k)
                    {
                        ComputeElement(elem);
                    }
                }
            }
        }
        public void ParallelThreadSplitMultiply(int split)
        {
            long subLength = this.m / split;
            Thread[] th = new Thread[split];
            Element elem;
            for (var i = 0; i < split; ++i)
            {
                elem = new Element();
                th[i] = new Thread(new ParameterizedThreadStart(ComputeElements));
                elem.x = i * subLength;
                elem.y = (i + 1) * subLength;
                th[i].Start(elem);
            }
            for (var i = 0; i < split; ++i)
            {
                th[i].Join();
            }
        }
        public void ParallelThreadMultiply()
        {
            Thread[,] th = new Thread[n, o];
            for (long i = 0; i < n; ++i)
            {
                for (long j = 0; j < o; ++j)
                {
                    Element elem = new Element();
                    elem.x = i;
                    elem.y = j;
                    th[i, j] = new Thread(new ParameterizedThreadStart(ComputeElement));
                    th[i, j].Start(elem);
                }
            }
            for (long i = 0; i < n; ++i)
            {
                for (long j = 0; j < o; ++j)
                {
                    th[i, j].Join();
                }
            }
        }
        public void ParallelForMultiply()
        {
            
            Parallel.For(0, n, i => {
                Parallel.For(0, o, j => {
                    Element elem = new Element();
                    elem.x = i;
                    elem.y = j;
                    ComputeElement(elem);
                });
            });
        }
        public override string ToString()
        {
            string res = "";
            for (long i = 0; i < n; ++i)
            {
                for (long j = 0; j < m; ++j)
                {
                    res += (matA[i, j] + " ");
                }
                res += "\n";
            }
            res += "\n\n\n";
            for (long i = 0; i < m; ++i)
            {
                for (long j = 0; j < o; ++j)
                {
                    res += (matB[i, j] + " ");
                }
                res += "\n";
            }
            res += "\n\n\n";
            for (long i = 0; i < n; ++i)
            {
                for (long j = 0; j < o; ++j)
                {
                    res += ( Math.Round(matC[i, j],2) + " ");
                }
                res += "\n";
            }
            Console.WriteLine(res);
            return res;

       
        }


    }

    class MainClass
    {
        public static void PerformMultiplication(int a, int b, int c)
        {
            Stopwatch sw = new Stopwatch();

            Matrix mat = new Matrix();
            mat.RandomizeMatAB(a, b, c);
            sw = new Stopwatch();

            sw.Start();
            mat.SeqMultiply();
            sw.Stop();
            Console.WriteLine("Time Sequential MatMul elapsed: " + sw.Elapsed);

            sw.Restart();
            mat.ParallelThreadMultiply();
            sw.Stop();
            Console.WriteLine("Time for Thread Parallel MatMul elapsed: " + sw.Elapsed);

            sw.Restart();
            mat.ParallelForMultiply();
            sw.Stop();
            Console.WriteLine("Time Parallel For MatMul elapsed: " + sw.Elapsed);

            sw.Restart();
            mat.ParallelThreadSplitMultiply(2);
            sw.Stop();
            Console.WriteLine("Time Parallel Thread with split MatMul elapsed: " + sw.Elapsed);
            mat.ToString();

        }
        public static void Main(string[] args)
        {
            // ----- Performing matrix multiplication on two 10 X 10 matrices
            Console.WriteLine("Matrix multiplication of two 10 X 10 matrices");
            PerformMultiplication(10, 10, 10);
            Console.WriteLine();
            // ----- End of stress test on 50 X 50 matrices

           
        }
    }
}