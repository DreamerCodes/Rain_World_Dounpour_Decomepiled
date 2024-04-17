using System.Globalization;
using System.IO;
using System.Threading;

public class RoomPreparer
{
	private int status;

	public bool finished;

	private ShortcutMapper scMapper;

	private AImapper aiMapper;

	private AIdataPreprocessor aiDataPreprocessor;

	private string aiHeatMapsData;

	private bool loadAiHeatMaps;

	private bool shortcutsOnly;

	private Thread thread;

	private bool threadFinished;

	private bool requestShortcutsReady;

	private bool requestReadyForAI;

	public Room room { get; private set; }

	public bool done { get; private set; }

	public bool falseBake { get; private set; }

	public bool failed { get; private set; }

	public RoomPreparer(Room room, bool loadAiHeatMaps, bool falseBake, bool shortcutsOnly)
	{
		this.room = room;
		this.loadAiHeatMaps = loadAiHeatMaps;
		this.falseBake = falseBake;
		this.shortcutsOnly = shortcutsOnly;
		string path = WorldLoader.FindRoomFile(room.abstractRoom.name, includeRootDirectory: false, ".txt");
		if (!File.Exists(path))
		{
			done = true;
			failed = true;
			return;
		}
		string[] array = File.ReadAllLines(path);
		room.LoadFromDataString(array);
		if (ModManager.MSC && room.defaultWaterLevel == -1)
		{
			room.defaultWaterLevel = -5000;
		}
		if (loadAiHeatMaps)
		{
			aiHeatMapsData = array[10];
		}
		scMapper = new ShortcutMapper(room);
	}

	public void Update()
	{
		bool flag = false;
		bool flag2 = false;
		lock (this)
		{
			if (!threadFinished && thread == null)
			{
				thread = new Thread(UpdateThread);
				thread.Start();
			}
			if (requestShortcutsReady)
			{
				flag = true;
				requestShortcutsReady = false;
				status++;
			}
			if (requestReadyForAI)
			{
				flag2 = true;
				requestReadyForAI = false;
				status++;
			}
			if (threadFinished)
			{
				if (!done)
				{
					done = true;
				}
				if (thread != null)
				{
					thread = null;
				}
			}
		}
		if (flag)
		{
			room.ShortCutsReady();
		}
		if (flag2)
		{
			room.ReadyForAI();
		}
	}

	private void UpdateThread()
	{
		CultureInfo.CurrentCulture = CultureInfo.GetCultureInfoByIetfLanguageTag("en-US");
		while (true)
		{
			int num;
			bool flag;
			bool flag2;
			lock (this)
			{
				num = status;
				flag = requestShortcutsReady;
				flag2 = requestReadyForAI;
			}
			switch (num)
			{
			case 0:
				if (flag)
				{
					break;
				}
				scMapper.Update();
				if (!scMapper.done)
				{
					break;
				}
				scMapper = null;
				if (shortcutsOnly)
				{
					lock (this)
					{
						requestShortcutsReady = true;
						threadFinished = true;
						return;
					}
				}
				aiMapper = new AImapper(room);
				lock (this)
				{
					requestShortcutsReady = true;
				}
				break;
			case 1:
				aiMapper.Update();
				if (aiMapper.done)
				{
					lock (this)
					{
						status++;
					}
					room.aimap = aiMapper.ReturnAIMap();
					aiMapper = null;
				}
				break;
			case 2:
				if (loadAiHeatMaps)
				{
					room.aimap.creatureSpecificAImaps = RoomPreprocessor.DecompressStringToAImaps(aiHeatMapsData, room.aimap);
					lock (this)
					{
						status = 4;
					}
				}
				else
				{
					aiDataPreprocessor = new AIdataPreprocessor(room.aimap, falseBake);
					lock (this)
					{
						status++;
					}
				}
				break;
			case 3:
				aiDataPreprocessor.Update();
				if (aiDataPreprocessor.done)
				{
					lock (this)
					{
						status++;
					}
				}
				break;
			case 4:
				lock (this)
				{
					if (!flag2)
					{
						requestReadyForAI = true;
					}
				}
				break;
			case 5:
				lock (this)
				{
					threadFinished = true;
					return;
				}
			}
		}
	}

	public AbstractRoomNode[] ReturnRoomConnectivity()
	{
		if (aiDataPreprocessor != null)
		{
			return aiDataPreprocessor.Connectivity();
		}
		return null;
	}
}
