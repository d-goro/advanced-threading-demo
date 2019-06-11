using System.Threading;


namespace ThreadingDemo
{
    class Program
    {
        private static /*volatile*/ bool _complete;

        static void Main(string[] args)
        {
            _complete = false; 
            var t = new Thread (() =>
            {
                bool toggle = false;

                //Thread.MemoryBarrier();
                while (!_complete)
                {
                    //Thread.MemoryBarrier();
                    toggle = !toggle;
                }
            });
            t.Start();
            Thread.Sleep (1000);
            
            //Thread.MemoryBarrier();
            _complete = true;
            //Thread.MemoryBarrier();
            
            t.Join();        // Blocks indefinitely
        }
        
        static void VolatileWrite (ref int address, int value)
        {
            Thread.MemoryBarrier(); address = value;
        }
 
        static int VolatileRead (ref int address)
        {
            int num = address; Thread.MemoryBarrier(); return num;
        }
    }
}
