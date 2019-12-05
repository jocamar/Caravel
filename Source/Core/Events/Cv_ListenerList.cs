using System;
using System.Collections.Generic;

namespace Caravel.Core.Events
{
    public class Cv_ListenerList : IDisposable
    {
        private List<Cv_EventManager.Cv_EventListenerHandle> handles = new List<Cv_EventManager.Cv_EventListenerHandle>();

        public static Cv_ListenerList operator+ (Cv_ListenerList list, Cv_EventManager.Cv_EventListenerHandle handle)
        {
            list.handles.Add(handle);
            return list;
        }

        public Cv_ListenerList()
        {
        }

        public void Dispose()
        {
            foreach (var handle in handles)
            {
                if (handle)
                {
                    handle.Manager.RemoveListener(handle);
                }
            }
        }
    }
}