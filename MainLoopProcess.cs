public abstract class MainLoopProcess
{
	public ProcessManager manager;

	public ProcessManager.ProcessID ID;

	public int framesPerSecond = 40;

	private float myTimeStacker;

	public bool processActive = true;

	public virtual float TimeSpeedFac => (float)framesPerSecond / 40f;

	public virtual float FadeInTime => 0.45f;

	public virtual float InitialBlackSeconds => 0f;

	public virtual bool AllowDialogs => false;

	public MainLoopProcess(ProcessManager manager, ProcessManager.ProcessID ID)
	{
		this.manager = manager;
		this.ID = ID;
	}

	public virtual void RawUpdate(float dt)
	{
		myTimeStacker += dt * (float)framesPerSecond;
		int num = 0;
		while (myTimeStacker > 1f)
		{
			Update();
			myTimeStacker -= 1f;
			num++;
			if (num > 2)
			{
				myTimeStacker = 0f;
			}
			if (myTimeStacker > 1f)
			{
				manager.rainWorld.RunRewiredUpdate();
			}
		}
		GrafUpdate(myTimeStacker);
	}

	public virtual void Update()
	{
	}

	public virtual void GrafUpdate(float timeStacker)
	{
	}

	public virtual void ShutDownProcess()
	{
	}

	public virtual void CommunicateWithUpcomingProcess(MainLoopProcess nextProcess)
	{
	}
}
