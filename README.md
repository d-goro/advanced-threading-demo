# Advanced Threading
*Demo of nonblocking synchronization*

Following short program showing importance of shared variable protection on any processor. You’ll need to run it with optimizations enabled and without a debugger (in Visual Studio, select Release Mode in the solution’s configuration manager, and then start without debugging). The program never terminates because the complete variable is cached in a CPU register.It also shown how to do non-blocking synchronization with memory barier or volatile keyword.


Full fences
--
The simplest kind of memory barrier is a **full memory barrier** (full fence) which prevents any kind of instruction reordering or caching around that fence. Calling Thread.MemoryBarrier generates a full fence; we can fix our example by applying four full fences (before and after every instruction that reads or writes a shared field *_complete*).
Monitor.Enter and Monitor.Exit both generate full fences. 
So if we ignore a lock’s mutual exclusion guarantee, we could say that this:

*lock (someField) { ... }*

is equivalent to this:

*Thread.MemoryBarrier(); { ... } Thread.MemoryBarrier();*


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


