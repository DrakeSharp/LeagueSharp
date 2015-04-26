using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;

namespace Drake.DLib
{
    class Benchmark
    {
        private string description;
        private int pass;
        private bool printwhenfull;
        private long[] samples;
        private int samplenum;
        private int rsamples;
        private Stopwatch stopwatch;
        //private ushort
        public Benchmark(string description ="", bool printwhenfull=true, int maxsamples=100)
        {
            this.description = description;
            samplenum = 0;
            rsamples = maxsamples;
            this.printwhenfull = printwhenfull;
            samples = new long[maxsamples];
            stopwatch=new Stopwatch();
            
        }
        public void Start()
        {

            stopwatch.Restart();

        }
        public void Reset(bool printwhenfull=true, int maxsamples = 100)
        {
            pass = 0;
            this.printwhenfull = printwhenfull;
            samplenum = 0;
            rsamples = maxsamples;
            samples = new long[rsamples];
            stopwatch = new Stopwatch();

        }
        public void Stop()
        {

            stopwatch.Stop();
            
            samples[samplenum] = stopwatch.ElapsedTicks;
            samplenum++;
            if (samplenum >= rsamples)
            {
                pass++;
                if (printwhenfull)
                    PrintResult();
                samplenum = 0;
            }
        }
        public int Result()
        {
            int i = 0;
            long sum=0;
            while (true)
            {
                if (i>=rsamples||samples[i] == 0) break;
                sum += samples[i];
                i++;

            }
            if (i == 0) return -1;
            return (int)(sum/i);
        }
        public void PrintResult()
        {
            long res = Result();
            Game.PrintChat("Pass " + pass.ToString("00000") + "|Samples:" + rsamples + "|Avg ticks:" + res.ToString("00000") + "|Avg ms:" + (1000 * res / (double)Stopwatch.Frequency).ToString("##0.000000")+"|desc:"+description);

        }
    }
}
