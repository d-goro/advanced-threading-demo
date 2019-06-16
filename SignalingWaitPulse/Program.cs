using System;
using System.Threading;


namespace SignalingWaitPulse
{
    class Program
    {
        static readonly object _locker = new object();
        static bool _go;

        static void Main(string[] args)
        {
            new Thread (Work).Start();
            Console.WriteLine("Press Enter to pulse worker thread");
            Console.ReadLine();
 
            lock (_locker)
            {// setting _go=true and pulsing.
                _go = true;
                Monitor.Pulse (_locker);
            }

            Console.WriteLine("Demo of Producer/Consumer Queue with monitor wait/pulse");
            var q = new PCQueue (2);
            Console.WriteLine ("Enqueuing 10 items...");
 
            for (int i = 0; i < 10; i++)
            {
                int itemNumber = i;      // To avoid the captured variable trap
                q.EnqueueItem (() =>
                {
                    Thread.Sleep (1000);          // Simulate time-consuming work
                    Console.Write (" Task" + itemNumber);
                });
            }
 
            q.Shutdown (true);
            Console.WriteLine();
            Console.WriteLine ("Workers complete!");
        }

        static void Work()
        {
            Console.WriteLine("work thread started");
            lock (_locker)
            {
                while (!_go)
                {
                    Monitor.Wait (_locker);    // Lock is released while we’re waiting
                    // lock is regained
                }
            }
            Console.WriteLine ("Woken!!!");
        }
    }
}
