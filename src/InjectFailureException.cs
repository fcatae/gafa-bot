using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace TaskFlow
{
    class InjectFailureException : Exception
    {
        static Random _rnd = new Random();

        [DebuggerHidden]
        public static void ThrowRate(int rate)
        {
            int probability = _rnd.Next(0, 100);

            if (probability > rate)
            {
                throw new InjectFailureException();
            }
        }
    }
}
