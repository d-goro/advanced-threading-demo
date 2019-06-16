# Advanced Threading


## NonblockingSync

Following short program showing importance of shared variable protection on any processor. You’ll need to run it with optimizations enabled and without a debugger (in Visual Studio, select Release Mode in the solution’s configuration manager, and then start without debugging). The program never terminates because the complete variable is cached in a CPU register.It also shown how to do non-blocking synchronization with memory barier or volatile keyword.


Full fences
--
The simplest kind of memory barrier is a **full memory barrier** (full fence) which prevents any kind of instruction reordering or caching around that fence. Calling Thread.MemoryBarrier generates a full fence; we can fix our example by applying four full fences (before and after every instruction that reads or writes a shared field *_complete*).
Monitor.Enter and Monitor.Exit both generate full fences. 
So if we ignore a lock’s mutual exclusion guarantee, we could say that this:
```csharp
lock (someField)
{
  ...
}
```
is equivalent to this:

```csharp
Thread.MemoryBarrier();
{ ... }
Thread.MemoryBarrier();
```


The volatile keyword
--
Another (more advanced) way to solve this problem is to apply the volatile keyword to the *_complete* field.
The **volatile** keyword instructs the compiler to generate an acquire-fence on every read from that field, and a release-fence on every write to that field. An acquire-fence prevents other reads/writes from being moved before the fence; a release-fence prevents other reads/writes from being moved after the fence. These “half-fences” are faster than full fences because they give the runtime and hardware more scope for optimization.

The effect of applying volatile to fields can be summarized as follows:

|First instruction	| Second instruction	| Can they be swapped? |
|:-----------------:|:-------------------:|:--------------------:|
|Read	              |         Read	      |         No           |
|Read	              |         Write	      |         No           |
|Write	            |         Write	      |         No (The CLR ensures that write-write operations are never swapped, even without the volatile keyword)|
|Write              |        	Read        |       	**Yes!**     |


VolatileRead and VolatileWrite
--
The **volatile** keyword is not supported with pass-by-reference arguments or captured local variables: in these cases you must use the VolatileRead and VolatileWrite methods. The static VolatileRead and VolatileWrite methods in the Main class read/write a variable while enforcing the guarantees made by the volatile keyword. 


## SignalingWaitPulse

The principle is that you write the signaling logic yourself using custom flags and fields (enclosed in lock statements), and then introduce Wait and Pulse commands to prevent spinning. With just these methods and the lock statement, you can achieve the functionality of AutoResetEvent, ManualResetEvent, and Semaphore. 

In terms of performance, calling Pulse takes around about a third of the time it takes to call Set on a wait handle. In practice, this is very simple and amounts purely to the cost of taking a lock.


Simple demo of signaling with Wait and Pulse
--
1. Define a single field for use as the synchronization object, such as:
*readonly object _locker = new object();*
2. Define field for use in custom blocking condition
*bool _go;*
3. Whenever you want to block, include the following code:
```csharp
lock (_locker)
  while ( <blocking-condition> )
    Monitor.Wait (_locker);
```
4. Whenever you change (or potentially change) a blocking condition, include this code:
```csharp
lock (_locker)
{
  // Alter field(s) or data that might impact blocking condition(s)
  // ...
  Monitor.Pulse(_locker);  // or: Monitor.PulseAll (_locker);
}
```


The Work method is where we block, waiting for the _go flag to become true. The Monitor.Wait method does the following, in order:

- Releases the lock on _locker.
- Blocks until _locker is “pulsed.”
- Reacquires the lock on _locker. If the lock is contended, then it blocks until the lock is available.
**This means that despite appearances, no lock is held on the synchronization object while Monitor.Wait awaits a pulse**


Producer/Consumer Queue
--

Demo of producer/consumer queue with wait/pulse. Arbitrary number of worker threads is used.
Main method starts a producer/consumer queue, specifying two concurrent consumer threads, and then enqueues 10 delegates to be shared among the two consumers.
