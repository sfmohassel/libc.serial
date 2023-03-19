using System.Collections.Generic;

namespace libc.serial.Internal
{
  internal class QueueThreadSafe<T>
  {
    private readonly object _lck;
    private readonly Queue<T> _q;

    public QueueThreadSafe()
    {
      _q = new Queue<T>();
      _lck = new object();
    }

    public int Count
    {
      get
      {
        var cnt = -1;

        lock (_lck)
        {
          try
          {
            cnt = _q.Count;
          }
          catch
          {
            // ignored
          }
        }

        return cnt;
      }
    }

    public Queue<T> Duplicate()
    {
      var res = new Queue<T>();

      lock (_lck)
      {
        try
        {
          foreach (var item in _q) res.Enqueue(item);
        }
        catch
        {
          // ignored
        }
      }

      return res;
    }

    public void Clear()
    {
      lock (_lck)
      {
        try
        {
          _q.Clear();
        }
        catch
        {
          // ignored
        }
      }
    }

    public bool TryDequeue(out T item)
    {
      lock (_lck)
      {
        try
        {
          if (_q.Count > 0)
          {
            item = _q.Dequeue();

            return true;
          }
        }
        catch
        {
          // ignored
        }
      }

      item = default;

      return false;
    }

    public List<T> DequeueAll()
    {
      var res = new List<T>();

      lock (_lck)
      {
        try
        {
          while (_q.Count > 0) res.Add(_q.Dequeue());
        }
        catch
        {
          // ignored
        }
      }

      return res;
    }

    public List<T> DequeueMany(int N)
    {
      var res = new List<T>();

      lock (_lck)
      {
        try
        {
          for (var i = 0; i < N; i++)
          {
            T b;

            if (TryDequeue(out b))
              res.Add(b);
            else
              break;
          }
        }
        catch
        {
          // ignored
        }
      }

      return res;
    }

    public void Enqueue(T item)
    {
      lock (_lck)
      {
        try
        {
          _q.Enqueue(item);
        }
        catch
        {
          // ignored
        }
      }
    }

    public void EnqueueMany(IEnumerable<T> items)
    {
      lock (_lck)
      {
        try
        {
          foreach (var item in items) _q.Enqueue(item);
        }
        catch
        {
          // ignored
        }
      }
    }

    public bool Any()
    {
      return Count > 0;
    }

    ~QueueThreadSafe()
    {
      Clear();
    }
  }
}