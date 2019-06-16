using System;
using System.Collections.Generic;
using System.Threading;

namespace SignalingWaitPulse
{
    /// <summary>
    /// Producer/Consumer Queue with monitor wait/pulse
    /// </summary>
    public class PCQueue
    {
        readonly Thread[] _workers;
        readonly object _locker = new object();
        readonly Queue<Action> _itemQ = new Queue<Action>();

        public PCQueue (int workerCount)
        {
            _workers = new Thread [workerCount];
 
            // Create and start a separate thread for each worker
            for (var i = 0; i < workerCount; i++)
            {
                (_workers [i] = new Thread (Consume)).Start();
            }
        }

        public void Shutdown (bool waitForWorkers)
        {
            // Enqueue one null item per worker to make each exit.
            foreach (var worker in _workers)
            {
                EnqueueItem (null);
            }
            
            if (waitForWorkers)
            {// Wait for workers to finish
                foreach (var worker in _workers)
                {
                    worker.Join();
                }
            }
        }
 
        public void EnqueueItem (Action item)
        {
            lock (_locker)
            {
                _itemQ.Enqueue (item);           // We must pulse because we're
                Monitor.Pulse (_locker);         // changing a blocking condition.
            }
        }
 
        void Consume()
        {// Keep consuming until told otherwise.
            while (true)                        
            {
                Action item;
                lock (_locker)
                {
                    while (_itemQ.Count == 0) 
                        Monitor.Wait (_locker);

                    item = _itemQ.Dequeue();
                }
                if (item == null)
                    return;    // This signals our exit.

                item();  // Execute item.
            }
        }
    }
}
