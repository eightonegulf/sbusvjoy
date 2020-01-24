using System;
using System.Collections.Generic;
using System.Text;

namespace SBUSReader
{
    public class Channels
    {
        float[] _c;
        public Channels(int nrOfChannels)
        {
            _c = new float[nrOfChannels];
        }

        public void SetChannel(int i, float v, float min = 200, float max = 1800)
        {
            _c[i] = (v - min) / (max-min);
        }

        public float GetChannel(int i)
        {
            return _c[i];
        }

        public void Print()
        {
            foreach(float c in _c)
            {
                Console.Write(c);
                Console.Write(' ');
            }
            Console.WriteLine("");
        }
    }
}
