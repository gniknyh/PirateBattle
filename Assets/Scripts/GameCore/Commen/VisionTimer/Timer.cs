
//#define Debug	
using UnityEngine;
using System;
using System.Collections.Generic;

#if (UNITY_EDITOR && Debug)
using System.Diagnostics;
#endif

public class Timer : MonoBehaviour
{
	private static GameObject mainObject = null;

	private static List<Event> activeEvent = new List<Event>();
	private static List<Event> poolEvent = new List<Event>();

	private static Event newEvent = null;
	private static int eventCount = 0;

	private static int eventBatch = 0;//Update Method variable
	private static int eventIterator = 0;


    //计时器系统将允许在一个帧中执行的回调的最大数量。
    //减少这一点可能会提高使用极端数量(数百)活动计时器的性能，但也可能导致一些事件被延迟到几帧
    public static int MaxEventsPerFrame = 500;

	public bool WasAddedCorrectly
	{
		get
		{
			if (!Application.isPlaying)
				return false;
			if (gameObject != mainObject)
				return false;
			return true;
		}
        
	}

	private void Awake()
	{
		if (!WasAddedCorrectly)
		{
			Destroy(this);
			return;
		}
	}

	private void Update()
	{

        //这种方法永远不会处理超过“MaxEventsPerFrame”，
        //以避免出现过多的计时器的性能问题。这可能会导致一些事件被延迟几帧。
        eventBatch = 0;
		while ((Timer.activeEvent.Count > 0) && eventBatch < MaxEventsPerFrame) //事件一旦加入就开始执行
		{
            if (eventIterator < 0)
			{
                eventIterator = Timer.activeEvent.Count - 1;
				break;
			}

			if (eventIterator > Timer.activeEvent.Count - 1)
                eventIterator = Timer.activeEvent.Count - 1;

			
			if (Time.time >= Timer.activeEvent[eventIterator].DueTime || Timer.activeEvent[eventIterator].Id == 0)
                Timer.activeEvent[eventIterator].Execute(); //延时到点了，执行相应的回调函数的

            else
			{				
				if (Timer.activeEvent[eventIterator].Paused)//暂停的话延长持续时间
					Timer.activeEvent[eventIterator].DueTime += Time.deltaTime;
				else
					Timer.activeEvent[eventIterator].LifeTime += Time.deltaTime;
			}

            eventIterator--;
            eventBatch++;
		}
    }

    #region main methods

    public static void InvokeMe(float delay, TimerCallback callback, Handle timerHandle = null)
    {
        Schedule(delay, callback, null, null, timerHandle, 1, -1.0f);
    }

    public static void InvokeMe(float delay, TimerCallback callback, int iterations, Handle timerHandle = null)
    {
        Schedule(delay, callback, null, null, timerHandle, iterations, -1.0f);
    }

    public static void InvokeMe(float delay, TimerCallback callback, int iterations, float interval, Handle timerHandle = null)
    {
        Schedule(delay, callback, null, null, timerHandle, iterations, interval);
    }

    public static void InvokeMe(float delay, TimerCallbackArg callback, object arguments, Handle timerHandle = null)
    {
        Schedule(delay, null, callback, arguments, timerHandle, 1, -1.0f);
    }

    public static void InvokeMe(float delay, TimerCallbackArg callback, object arguments, int iterations, Handle timerHandle = null)
    {
        Schedule(delay, null, callback, arguments, timerHandle, iterations, -1.0f);
    }

    public static void InvokeMe(float delay, TimerCallbackArg callback, object arguments, int iterations, float interval,
        Handle timerHandle = null)
    {
        Schedule(delay, null, callback, arguments, timerHandle, iterations, interval);
    }


    #endregion


    /// <summary>
    ///“Start”方法是唯一用于运行一个计时器
    ///目的测量时间(如:秒表有用)。
    ///它只接受强制性的定时器处理作为输入参数,
    ///几乎没有回调方法,将运行,直到永远。
    ///定时器处理可以用于暂停、恢复
    ///从计时器事件调查的各种信息
    /// </summary>
    public static void Start(Handle timerHandle)
	{
		Schedule(315360000.0f, /* 10年, yo ;) */ delegate() { }, null, null, timerHandle, 1, -1.0f);
	}

    /// <summary>
    /// 启动定时器
    /// </summary>
    /// <param 持续时间="time"></param>
    /// <param 无参数回调函数="func"> </param>
    /// <param 有参数回调函数="argFunc"></param>
    /// <param 有参数回调函数的参数="args"></param>
    /// <param Handle="timerHandle"></param>
    /// <param 迭代次数="iterations"></param>
    /// <param 时间间隔="interval"></param>
    private static void Schedule(float time, TimerCallback func, TimerCallbackArg argFunc, object args, Handle timerHandle, int iterations, float interval)
	{

		if (func == null && argFunc == null)
		{
			UnityEngine.Debug.LogError("Error: (Timer) 注意了一个回调函数都没有,请添加回调函数.");
			return;
		}

		// setup main gameobject
		if (mainObject == null)
		{
            mainObject = new GameObject("Timers");
            mainObject.AddComponent<Timer>();
			UnityEngine.Object.DontDestroyOnLoad(mainObject);

#if (UNITY_EDITOR && !Debug)
			mainObject.gameObject.hideFlags = HideFlags.HideInHierarchy;
#endif
        }


        time = Mathf.Max(0.0f, time);
		iterations = Mathf.Max(0, iterations);
		interval = (interval == -1.0f) ? time : Mathf.Max(0.0f, interval);

        newEvent = null;
		if (poolEvent.Count > 0)
		{
            newEvent = poolEvent[0];
            poolEvent.Remove(newEvent);//出池
		}
		else
            newEvent = new Event();

		Timer.eventCount++;
        newEvent.Id = Timer.eventCount;
		if (func != null)
            newEvent.Function = func;
		else if (argFunc != null)
		{
            newEvent.ArgFunction = argFunc;
            newEvent.Arguments = args;
		}
        newEvent.StartTime = Time.time;
        newEvent.DueTime = Time.time + time;
        newEvent.Iterations = iterations;
        newEvent.Interval = interval;
        newEvent.LifeTime = 0.0f;
        newEvent.Paused = false;

		Timer.activeEvent.Add(newEvent);//加入即将要执行的事件

		if (timerHandle != null)
		{
			if (timerHandle.Active)
				timerHandle.Cancel();
            //关联Hander和Event
			timerHandle.Id = newEvent.Id;
		}

#if (UNITY_EDITOR && Debug)
        newEvent.StoreCallingMethod();
		EditorRefresh();
#endif

	}
	

	private static void Cancel(Timer.Handle handle)
	{

		if (handle == null)
			return;

		if (handle.Active)
		{
			handle.Id = 0;
			return;
		}

	}
	

	public static void CancelAll()
	{

		for (int t = Timer.activeEvent.Count - 1; t > -1; t--)
		{
			Timer.activeEvent[t].Id = 0; // id == 0 表示非活跃的事件 id > 0 表示活跃的事件
		}

	}


	public static void CancelAll(string methodName)
	{

		for (int t = Timer.activeEvent.Count - 1; t > -1; t--)
		{
		    if (Timer.activeEvent[t].MethodName == methodName)
		    {
		        Timer.activeEvent[t].Id = 0;
                continue;
		    }
		}

	}


    /// <summary>
    ///清除所有当前活动的Timer和Pool，即:对每个计时器事件进行处理，将内存释放到垃圾收集器。
    /// </summary>
    public static void DestroyAll()
	{
		Timer.activeEvent.Clear();
		Timer.poolEvent.Clear();

#if (UNITY_EDITOR && Debug)
		EditorRefresh();
#endif

	}

    #region for Editor


    //for Editor class
    public struct Stats
    {
        public int Created;
        public int Inactive;
        public int Active;
    }

    public static Stats EditorGetStats()
    {

        Stats stats;
        stats.Created = activeEvent.Count + poolEvent.Count;
        stats.Inactive = poolEvent.Count;
        stats.Active = activeEvent.Count;
        return stats;

    }

    public static string EditorGetMethodInfo(int eventIndex)
    {

        if (eventIndex < 0 || eventIndex > activeEvent.Count - 1)
            return "Argument out of range.";

        return activeEvent[eventIndex].MethodInfo;

    }

    public static int EditorGetMethodId(int eventIndex)
    {
        if (eventIndex < 0 || eventIndex > activeEvent.Count - 1)
            return 0;

        return activeEvent[eventIndex].Id;

    }

#if (Debug && UNITY_EDITOR)

    private static void EditorRefresh()
    {
        mainObject.name = "Timers (" + activeEvent.Count + " / " + (poolEvent.Count + activeEvent.Count).ToString() + ")";
    }
#endif

    #endregion






    /**
    *@desc:
    *   用来封装传入的事件（函数），保持事件的状态
    */
    private class Event
	{

		public int Id;// id == 0 表示非活跃的事件 id > 0 表示活跃的事件

        public TimerCallback Function = null;
		public TimerCallbackArg ArgFunction = null;
		public object Arguments = null; // 有参数回调函数的参数

		public int Iterations = 1; //迭代次数
		public float Interval = -1.0f; // 每次执行的时间间隔
		public float DueTime = 0.0f; //终止时间
		public float StartTime = 0.0f;//开始时间
		public float LifeTime = 0.0f;//持续时间
		public bool Paused = false;

#if (Debug && UNITY_EDITOR)
		private string m_CallingMethod = "";
#endif
		

		public void Execute()
		{	         
			if (Id == 0 || DueTime == 0.0f)
			{
				Recycle();
				return;
			}
			
			if (Function != null)
				Function();
			else if (ArgFunction != null)
				ArgFunction(Arguments);
			else
			{				
				Error("终止事件因为回调函数为空.");
				Recycle();
				return;
			}

            //延时回收
			if (Iterations > 0)
			{
				Iterations--;
				if (Iterations < 1)
				{
					Recycle();
					return;
				}
			}

            //更新到期时间
			DueTime = Time.time + Interval;

		}
		
		private void Recycle()
		{
			Id = 0;
			DueTime = 0.0f;
			StartTime = 0.0f;

			Function = null;
			ArgFunction = null;
			Arguments = null;

			if (Timer.activeEvent.Remove(this))
                poolEvent.Add(this);

#if (UNITY_EDITOR && Debug)
			EditorRefresh();
#endif

		}

		private void Destroy()
		{
			Timer.activeEvent.Remove(this);
			Timer.poolEvent.Remove(this);

		}


#if (UNITY_EDITOR && Debug)

		public void StoreCallingMethod()
		{
			StackTrace stackTrace = new StackTrace();

			string result = "";
			string declaringType = "";
			for (int v = 3; v < stackTrace.FrameCount; v++)
			{
				StackFrame stackFrame = stackTrace.GetFrame(v);
				declaringType = stackFrame.GetMethod().DeclaringType.ToString();
				result += " <- " + declaringType + ":" + stackFrame.GetMethod().Name.ToString();
			}

			m_CallingMethod = result;

		}
#endif

		private void Error(string message)
		{

			string msg = "Error: (Timer.Event) " + message;
#if (UNITY_EDITOR && Debug)
			msg += MethodInfo;
#endif
			UnityEngine.Debug.LogError(msg);

		}


		public string MethodName
		{
			get
			{
				if (Function != null)
				{
					if (Function.Method != null)
					{
						if (Function.Method.Name[0] == '<')
							return "delegate";
						else
                            return Function.Method.Name;
					}
				}
				else if (ArgFunction != null)
				{
					if (ArgFunction.Method != null)
					{
						if (ArgFunction.Method.Name[0] == '<')
							return "delegate";
						else
                            return ArgFunction.Method.Name;
					}
				}
				return null;
			}

		}


		public string MethodInfo
		{
			get
			{
				string s = MethodName;
				if (!string.IsNullOrEmpty(s))
				{
					s += "(";
					if (Arguments != null)
					{
						if (Arguments.GetType().IsArray)
						{
							object[] array = (object[])Arguments;
							foreach (object o in array)
							{
								s += o.ToString();
								if (Array.IndexOf(array, o) < array.Length - 1)
									s += ", ";
							}
						}
						else
							s += Arguments;
					}
					s += ")";
				}
				else
					s = "(function = null)";

#if (Debug && UNITY_EDITOR)
				s += m_CallingMethod;
#endif
				return s;
			}
		}

	}



    /********
    @desc:
        这个类用于跟踪当前正在运行的事件。它通常用于取消一个事件，或者查看它是否仍然活动，
        但是它也有许多属性来分析事件的状态。编辑器使用它来显示调试信息.
        对事件状态提供查询接口（事件执行了多长时间，结束时间，是否还是Active）
        以及提供 Excute（立即执行事件），Cancel(取消事件），Pause(暂停事件）等操作
    */
    public class Handle
	{

		private Timer.Event m_Event = null;	
		private int m_Id = 0;					
		private int m_StartIterations = 1;		
		private float m_FirstDueTime = 0.0f;	


        //pause timer event
		public bool Paused
		{
			get
			{
				return Active && m_Event.Paused;
			}
			set
			{
				if (Active)
					m_Event.Paused = value;
			}
		}


        /// <summary>
        /// 返回初始调度的时间
        /// </summary>
        public float TimeOfInitiation
		{
			get
			{
				if (Active)
					return m_Event.StartTime;
				else
                    return 0.0f;
			}
		}

        /// <summary>
        /// 返回第一次执行的时间
        /// </summary>
        public float TimeOfFirstIteration
		{
			get
			{
				if (Active)
					return m_FirstDueTime;
				return 0.0f;
			}
		}

        /// <summary>
        /// 在下一次迭代中返回预期的到期时间t
        /// </summary>
        public float TimeOfNextIteration
		{
			get
			{
				if (Active)
					return m_Event.DueTime;
                return 0.0f;

            }
		}

        /// <summary>
        /// 返回一个事件的最后一次迭代的预期时间
        /// </summary>
        public float TimeOfLastIteration
		{
			get
			{
				if (Active)
					return Time.time + DurationLeft;
				return 0.0f;
			}
		}

        //////// 时间间隔 ////////

        /// <summary>
        /// 在第一次执行之前返回延迟
        /// </summary>
        public float Delay
		{
			get
			{
				return (Mathf.Round((m_FirstDueTime - TimeOfInitiation) * 1000.0f) / 1000.0f);
			}
		}

		public float Interval
		{
			get
			{
				if (Active)
					return m_Event.Interval;
				return 0.0f;
			}
		}

		public float TimeUntilNextIteration
		{
			get
			{
				if (Active)
					return m_Event.DueTime - Time.time;
				return 0.0f;
			}
		}


		public float DurationLeft
		{
			get
			{
				if (Active)
					return TimeUntilNextIteration + ((m_Event.Iterations - 1) * m_Event.Interval);
				return 0.0f;
			}
		}


		public float DurationTotal
		{
			get
			{
				if (Active)
				{
					return Delay +
						((m_StartIterations) * ((m_StartIterations > 1) ? Interval : 0.0f));
				}
				return 0.0f;
			}
		}

		public float Duration
		{
			get
			{
				if (Active)
					return m_Event.LifeTime;
				return 0.0f;
			}
		}

        //////// iterations ////////

        /// <summary>
        /// 在启动时返回一个事件的总预期迭代次数
        /// </summary>
        public int IterationsTotal
		{
			get
			{
				return m_StartIterations;
			}
		}

		/// <summary>
		/// 返回一个事件剩下的迭代次数
		/// </summary>
		public int IterationsLeft
		{
			get
			{
				if (Active)
					return m_Event.Iterations;
				return 0;
			}
		}

        /// <summary>
        ///返回这个事件句柄的id，它也是关联事件的id(如果不是，句柄被认为是不活动的)。
        /// 不建议直接设置此属性
        /// </summary>
        public int Id
		{
			get
			{
				return m_Id;
			}
			set
			{
				m_Id = value;

				if (m_Id == 0)
				{
					m_Event.DueTime = 0.0f;
					return;
				}


				m_Event = null;
				for (int t = Timer.activeEvent.Count - 1; t > -1; t--)
				{
					if (Timer.activeEvent[t].Id == m_Id)
					{
						m_Event = Timer.activeEvent[t];
						break;
					}
				}
				if (m_Event == null)
					UnityEngine.Debug.LogError("Error: (Timer.Handle) Failed to assign event with Id '" + m_Id + "'.");

				m_StartIterations = m_Event.Iterations;
				m_FirstDueTime = m_Event.DueTime;

			}

		}

		public bool Active
		{
			get
			{
				if (m_Event == null || Id == 0 || m_Event.Id == 0)
					return false;
				return m_Event.Id == Id;
			}
		}

		public string MethodName { get { return m_Event.MethodName; } }

		public string MethodInfo { get { return m_Event.MethodInfo; } }
		

		public void Cancel()
		{
			Timer.Cancel(this);
		}

		public void Execute()
		{
			m_Event.DueTime = Time.time;
		}

	}
	

}
