
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


    //��ʱ��ϵͳ��������һ��֡��ִ�еĻص������������
    //������һ����ܻ����ʹ�ü�������(����)���ʱ�������ܣ���Ҳ���ܵ���һЩ�¼����ӳٵ���֡
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

        //���ַ�����Զ���ᴦ������MaxEventsPerFrame����
        //�Ա�����ֹ���ļ�ʱ�����������⡣����ܻᵼ��һЩ�¼����ӳټ�֡��
        eventBatch = 0;
		while ((Timer.activeEvent.Count > 0) && eventBatch < MaxEventsPerFrame) //�¼�һ������Ϳ�ʼִ��
		{
            if (eventIterator < 0)
			{
                eventIterator = Timer.activeEvent.Count - 1;
				break;
			}

			if (eventIterator > Timer.activeEvent.Count - 1)
                eventIterator = Timer.activeEvent.Count - 1;

			
			if (Time.time >= Timer.activeEvent[eventIterator].DueTime || Timer.activeEvent[eventIterator].Id == 0)
                Timer.activeEvent[eventIterator].Execute(); //��ʱ�����ˣ�ִ����Ӧ�Ļص�������

            else
			{				
				if (Timer.activeEvent[eventIterator].Paused)//��ͣ�Ļ��ӳ�����ʱ��
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
    ///��Start��������Ψһ��������һ����ʱ��
    ///Ŀ�Ĳ���ʱ��(��:�������)��
    ///��ֻ����ǿ���ԵĶ�ʱ��������Ϊ�������,
    ///����û�лص�����,������,ֱ����Զ��
    ///��ʱ���������������ͣ���ָ�
    ///�Ӽ�ʱ���¼�����ĸ�����Ϣ
    /// </summary>
    public static void Start(Handle timerHandle)
	{
		Schedule(315360000.0f, /* 10��, yo ;) */ delegate() { }, null, null, timerHandle, 1, -1.0f);
	}

    /// <summary>
    /// ������ʱ��
    /// </summary>
    /// <param ����ʱ��="time"></param>
    /// <param �޲����ص�����="func"> </param>
    /// <param �в����ص�����="argFunc"></param>
    /// <param �в����ص������Ĳ���="args"></param>
    /// <param Handle="timerHandle"></param>
    /// <param ��������="iterations"></param>
    /// <param ʱ����="interval"></param>
    private static void Schedule(float time, TimerCallback func, TimerCallbackArg argFunc, object args, Handle timerHandle, int iterations, float interval)
	{

		if (func == null && argFunc == null)
		{
			UnityEngine.Debug.LogError("Error: (Timer) ע����һ���ص�������û��,����ӻص�����.");
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
            poolEvent.Remove(newEvent);//����
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

		Timer.activeEvent.Add(newEvent);//���뼴��Ҫִ�е��¼�

		if (timerHandle != null)
		{
			if (timerHandle.Active)
				timerHandle.Cancel();
            //����Hander��Event
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
			Timer.activeEvent[t].Id = 0; // id == 0 ��ʾ�ǻ�Ծ���¼� id > 0 ��ʾ��Ծ���¼�
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
    ///������е�ǰ���Timer��Pool����:��ÿ����ʱ���¼����д������ڴ��ͷŵ������ռ�����
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
    *   ������װ������¼����������������¼���״̬
    */
    private class Event
	{

		public int Id;// id == 0 ��ʾ�ǻ�Ծ���¼� id > 0 ��ʾ��Ծ���¼�

        public TimerCallback Function = null;
		public TimerCallbackArg ArgFunction = null;
		public object Arguments = null; // �в����ص������Ĳ���

		public int Iterations = 1; //��������
		public float Interval = -1.0f; // ÿ��ִ�е�ʱ����
		public float DueTime = 0.0f; //��ֹʱ��
		public float StartTime = 0.0f;//��ʼʱ��
		public float LifeTime = 0.0f;//����ʱ��
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
				Error("��ֹ�¼���Ϊ�ص�����Ϊ��.");
				Recycle();
				return;
			}

            //��ʱ����
			if (Iterations > 0)
			{
				Iterations--;
				if (Iterations < 1)
				{
					Recycle();
					return;
				}
			}

            //���µ���ʱ��
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
        ��������ڸ��ٵ�ǰ�������е��¼�����ͨ������ȡ��һ���¼������߲鿴���Ƿ���Ȼ���
        ������Ҳ����������������¼���״̬���༭��ʹ��������ʾ������Ϣ.
        ���¼�״̬�ṩ��ѯ�ӿڣ��¼�ִ���˶೤ʱ�䣬����ʱ�䣬�Ƿ���Active��
        �Լ��ṩ Excute������ִ���¼�����Cancel(ȡ���¼�����Pause(��ͣ�¼����Ȳ���
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
        /// ���س�ʼ���ȵ�ʱ��
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
        /// ���ص�һ��ִ�е�ʱ��
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
        /// ����һ�ε����з���Ԥ�ڵĵ���ʱ��t
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
        /// ����һ���¼������һ�ε�����Ԥ��ʱ��
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

        //////// ʱ���� ////////

        /// <summary>
        /// �ڵ�һ��ִ��֮ǰ�����ӳ�
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
        /// ������ʱ����һ���¼�����Ԥ�ڵ�������
        /// </summary>
        public int IterationsTotal
		{
			get
			{
				return m_StartIterations;
			}
		}

		/// <summary>
		/// ����һ���¼�ʣ�µĵ�������
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
        ///��������¼������id����Ҳ�ǹ����¼���id(������ǣ��������Ϊ�ǲ����)��
        /// ������ֱ�����ô�����
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
