using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Caravel.Core.Entity;
using Caravel.Core.Scripting;
using Caravel.Debugging;
using static Caravel.Core.Events.Cv_Event;

namespace Caravel.Core.Events
{
    public class Cv_EventManager
    {
        public delegate void NewEventDelegate(Cv_Event eventData);
        public readonly int NUM_QUEUES = 2;

        public long MaxProcessTimeMillis
        {
            get; set;
        }

        public static Cv_EventManager Instance
        {
            get; private set;
        }

        private struct Cv_ScriptListener
        {
            public readonly Cv_Entity Entity;
            public readonly string Delegate;

            public Cv_ScriptListener(Cv_Entity entity, string script)
            {
                Entity = entity;
                Delegate = script;
            }
        }

        private Dictionary<Cv_EventType, List<NewEventDelegate>> m_EventListeners;
        private Dictionary<Cv_EventType, List<Cv_ScriptListener>> m_ScriptEventListeners;
        private LinkedList<Cv_Event>[] m_EventQueues;
        private ConcurrentQueue<Cv_Event> m_RealTimeEventQueue;
        private int m_iActiveQueue = 0;

        public Cv_EventManager(bool global)
        {
            MaxProcessTimeMillis = long.MaxValue;

            if (global) {

                if (Instance != null)
                {
                    Cv_Debug.Error("Attempting to create two EventManagers as global. The old one will be overwritten.");
                }

                Instance = this;
            }

			m_RealTimeEventQueue = new ConcurrentQueue<Cv_Event>();
            m_EventListeners = new Dictionary<Cv_EventType, List<NewEventDelegate>>();
            m_ScriptEventListeners = new Dictionary<Cv_EventType, List<Cv_ScriptListener>>();
            
            m_EventQueues = new LinkedList<Cv_Event>[NUM_QUEUES];
            for (int i = 0; i < NUM_QUEUES; i++)
            {
                m_EventQueues[i] = new LinkedList<Cv_Event>();
            }
        }

        public bool AddListener<EventType>(NewEventDelegate callback) where EventType : Cv_Event
        {
            Cv_Debug.Log("Events", "Attempting to add listener for event type " + typeof(EventType).Name);

            Cv_EventType eType = Cv_Event.GetType<EventType>();

			lock (m_EventListeners)
			{
				if (!m_EventListeners.ContainsKey(eType))
				{
					m_EventListeners[eType] = new List<NewEventDelegate>();
				}

				var listeners = m_EventListeners[eType];

				foreach (var l in listeners)
				{
					if (l == callback)
					{
						Cv_Debug.Warning("Attempting to double register a listener.");
						return false;
					}
				}

				listeners.Add(callback);
			}

            Cv_Debug.Log("Events", "Successfully added listener for event type " + typeof(EventType).Name);

            return true;
        }

        public bool AddListener(string eventName, string onEvent, Cv_Entity entity)
        {
            Cv_Debug.Log("Events", "Attempting to add listener for event type " + eventName);

            Cv_EventType eType = Cv_Event.GetType(eventName);

			lock (m_ScriptEventListeners)
			{
				if (!m_ScriptEventListeners.ContainsKey(eType))
				{
					m_ScriptEventListeners[eType] = new List<Cv_ScriptListener>();
				}

				var listeners = m_ScriptEventListeners[eType];

				foreach (var l in listeners)
				{
					if (l.Delegate == onEvent && l.Entity == entity)
					{
						Cv_Debug.Warning("Attempting to double register a listener.");
						return false;
					}
				}

				listeners.Add(new Cv_ScriptListener(entity, onEvent));
			}

            Cv_Debug.Log("Events", "Successfully added listener for event type " + eventName);

            return true;
        }

        public bool RemoveListener<EventType>(NewEventDelegate callback) where EventType : Cv_Event
        {
            Cv_Debug.Log("Events", "Attempting to remove listener from event type " + typeof(EventType).Name);
            var success = false;
            Cv_EventType eType = Cv_Event.GetType<EventType>();

			lock (m_EventListeners)
			{
				if (m_EventListeners.ContainsKey(eType))
				{
					var listeners = m_EventListeners[eType];

					success = listeners.Remove(callback);
				}
			}

            if (success)
            {
                Cv_Debug.Log("Events", "Successfully removed listener from event type " + typeof(EventType).Name);
            }

            return success;
        }

        public bool RemoveListener(string eventName, string onEvent, Cv_Entity entity)
        {
            Cv_Debug.Log("Events", "Attempting to remove listener from event type " + eventName);
            var success = false;
            Cv_EventType eType = Cv_Event.GetType(eventName);

			lock (m_ScriptEventListeners)
			{
				if (m_ScriptEventListeners.ContainsKey(eType))
				{
					var listeners = m_ScriptEventListeners[eType];

					success = listeners.RemoveAll(l => l.Delegate == onEvent && l.Entity == entity) > 0;
				}
			}

            if (success)
            {
                Cv_Debug.Log("Events", "Successfully removed listener from event type " + eventName);
            }

            return success;
        }

        public bool TriggerEvent(Cv_Event newEvent)
        {
            if (newEvent.WriteToLog)
            {
                Cv_Debug.Log("Events", "Attempting to trigger event " + newEvent.VGetName() + " for entity " + newEvent.EntityID);
            }

            var processed = false;

            List<NewEventDelegate> listenersCopy = null;
            lock (m_EventListeners)
            {
                List<NewEventDelegate> listeners;
                if (m_EventListeners.TryGetValue(newEvent.Type, out listeners))
                {
                    listenersCopy = new List<NewEventDelegate>(listeners);
                }
            }

            if (listenersCopy != null) {
				foreach (var l in listenersCopy)
				{
					if (newEvent.WriteToLog)
					{
						Cv_Debug.Log("Events", "Sending event " + newEvent.VGetName() + " to listener.");
					}

					l(newEvent);
					processed = true;
				}
			}

            List<Cv_ScriptListener> scriptListenersCopy = null;
            lock (m_ScriptEventListeners)
            {
                List<Cv_ScriptListener> scriptListeners;
                if (m_ScriptEventListeners.TryGetValue(newEvent.Type, out scriptListeners))
                {
                    scriptListenersCopy = new List<Cv_ScriptListener>(scriptListeners);
                }
            }

            if (scriptListenersCopy != null) {
				foreach (var l in scriptListenersCopy)
				{
					if (newEvent.WriteToLog)
					{
						Cv_Debug.Log("Events", "Sending event " + newEvent.VGetName() + " to listener.");
					}

					Cv_ScriptManager.Instance.VExecuteString("Cv_EventManager", l.Delegate, newEvent, l.Entity);
					processed = true;
				}
			}
        
            return processed;
        }

        public bool QueueEvent(Cv_Event newEvent, bool threadSafe = false)
        {
            if (!threadSafe)
            {
                Cv_Debug.Assert( (m_iActiveQueue >= 0 && m_iActiveQueue < NUM_QUEUES), "EventManager must have an active event queue.");

                if (newEvent == null)
                {
                    Cv_Debug.Error("Invalid event in QueueEvent.");
                    return false;
                }

                if (newEvent.WriteToLog)
                {
                    Cv_Debug.Log("Events", "Attempting to queue event " + newEvent.VGetName() + " for entity " + newEvent.EntityID);
                }

				lock (m_EventListeners)
				{
					if (m_EventListeners.ContainsKey(newEvent.Type))
					{
						m_EventQueues[m_iActiveQueue].AddLast(newEvent);
						if (newEvent.WriteToLog)
						{
							Cv_Debug.Log("Events", "Successfully queued event " + newEvent.VGetName());
						}
						return true;
					}
				}

                lock (m_ScriptEventListeners)
				{
					if (m_ScriptEventListeners.ContainsKey(newEvent.Type))
					{
						m_EventQueues[m_iActiveQueue].AddLast(newEvent);
						if (newEvent.WriteToLog)
						{
							Cv_Debug.Log("Events", "Successfully queued event " + newEvent.VGetName());
						}
						return true;
					}
				}

                Cv_Debug.Log("Events", "Skipping event " + newEvent.VGetName() + " since there are no listeners for it.");
				return false;
            }
            else
            {
                m_RealTimeEventQueue.Enqueue(newEvent);
                return true;
            }
        }

        public bool AbortEvent<EventType>(bool allOfType = false) where EventType : Cv_Event
        {
            Cv_Debug.Assert( (m_iActiveQueue >= 0 && m_iActiveQueue < NUM_QUEUES), "EventManager must have an active event queue.");
            Cv_EventType eType = Cv_Event.GetType<EventType>();
            
            var success = false;

            Cv_Debug.Log("Events", "Attempting to abort event type " + typeof(EventType).Name);

            var queue = m_EventQueues[m_iActiveQueue];

            if (allOfType)
            {
                if ( queue.Remove(queue.First( e => e.Type == eType )) )
                {
                    success = true;
                }
            }
            else
            {
                while ( queue.Remove(queue.First( e => e.Type == eType )) )
                {
                    success = true;
                }
            }

            if (success)
            {
                Cv_Debug.Log("Events", "Successfully aborted event type " + typeof(EventType).Name);
            }

            return success;
        }

        internal bool Initialize()
        {
            return true;
        }

        internal bool OnUpdate(float time, float elapsedTime)
        {
            long currentMs = 0;

            Cv_Event e;

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            while (m_RealTimeEventQueue.TryDequeue(out e))
            {
                QueueEvent(e);

                currentMs = stopwatch.ElapsedMilliseconds;

                if (MaxProcessTimeMillis != long.MaxValue)
                {
                    if (currentMs >= MaxProcessTimeMillis)
                    {
                        Cv_Debug.Error("A concurrent process is spamming the EventManager.");
                    }
                }
            }

            var queueToProcess = m_iActiveQueue;
            m_iActiveQueue = (m_iActiveQueue + 1) % NUM_QUEUES;
            m_EventQueues[m_iActiveQueue].Clear();

            Cv_Debug.Log("Events", "Processing event queue " + queueToProcess + ". " + m_EventQueues[queueToProcess].Count + " events to process.");

            while (m_EventQueues[queueToProcess].Count > 0)
            {
                e = m_EventQueues[queueToProcess].First.Value;
                m_EventQueues[queueToProcess].RemoveFirst();

                List<NewEventDelegate> listeners;

                List<NewEventDelegate> listenersCopy = null;
				lock (m_EventListeners)
				{
					if (m_EventListeners.TryGetValue(e.Type, out listeners))
					{
                        listenersCopy = new List<NewEventDelegate>(listeners);
					}
				}

                if (m_EventListeners != null)
                {
                    foreach (var l in listenersCopy)
                    {
                        if (e.WriteToLog)
                        {
                            Cv_Debug.Log("Events", "Sending event " + e.VGetName() + " to listener.");
                        }

                        l(e);
                    }
                }

                List<Cv_ScriptListener> scriptListeners;

                lock (m_ScriptEventListeners)
				{
					if (m_ScriptEventListeners.TryGetValue(e.Type, out scriptListeners))
					{
						foreach (var l in scriptListeners)
						{
							if (e.WriteToLog)
							{
								Cv_Debug.Log("Events", "Sending event " + e.VGetName() + " to listener.");
							}
							
							Cv_ScriptManager.Instance.VExecuteString("Cv_EventManager", l.Delegate, e, l.Entity);
						}
					}
				}

                currentMs = stopwatch.ElapsedMilliseconds;

                if (MaxProcessTimeMillis != long.MaxValue)
                {
                    if (currentMs >= MaxProcessTimeMillis)
                    {
                        Cv_Debug.Error("EventManager processing time exceeded. Aborting.");
                        stopwatch.Stop();
                        break;
                    }
                }
            }

            bool queueFlushed = true;
            while (m_EventQueues[queueToProcess].Count > 0)
            {
                queueFlushed = false;
                m_EventQueues[m_iActiveQueue].AddBefore(m_EventQueues[m_iActiveQueue].First, m_EventQueues[queueToProcess].Last);
                m_EventQueues[queueToProcess].RemoveLast();
            }

            stopwatch.Stop();
            return queueFlushed;
        }
    }
}
