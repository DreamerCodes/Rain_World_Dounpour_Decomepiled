using System.Globalization;
using System.Text.RegularExpressions;
using RWCustom;

namespace MoreSlugcats;

public class PersistentObjectTracker
{
	public AbstractPhysicalObject obj;

	public string lastSeenRoom;

	public bool realizedThisCycle;

	public string objRepresentation;

	public AbstractPhysicalObject.AbstractObjectType repType;

	public string lastSeenRegion;

	public WorldCoordinate desiredSpawnLocation;

	public string repData;

	public string[] unrecognizedAttributes;

	public PersistentObjectTracker(AbstractPhysicalObject obj)
	{
		this.obj = obj;
		objRepresentation = "";
		realizedThisCycle = false;
		if (obj != null)
		{
			lastSeenRegion = obj.world.name;
			lastSeenRoom = obj.Room.name;
			desiredSpawnLocation = obj.pos;
			obj.tracker = this;
			repType = obj.type;
			repData = getRepData(obj);
		}
		else
		{
			repData = "";
			lastSeenRoom = "";
			lastSeenRegion = "";
			desiredSpawnLocation = new WorldCoordinate(-1, -1, -1, -1);
		}
	}

	public override string ToString()
	{
		string repString = ((obj != null) ? obj.ToString() : objRepresentation);
		repString = InjectDesiredSpawnToObjRepresentation(repString);
		return SaveUtils.AppendUnrecognizedStringAttrs(string.Format(CultureInfo.InvariantCulture, "{0}<pOT>{1}<pOT>{2}<pOT>{3}<pOT>{4}", lastSeenRegion.ToString(), lastSeenRoom.ToString(), desiredSpawnLocation.SaveToString(), repString, repData), "<pOT>", unrecognizedAttributes);
	}

	public void FromString(string str, SlugcatStats.Name saveSlot)
	{
		string[] array = Regex.Split(str, "<pOT>");
		lastSeenRegion = array[0];
		lastSeenRoom = array[1];
		desiredSpawnLocation = WorldCoordinate.FromString(array[2]);
		if (ModManager.MSC && repType == MoreSlugcatsEnums.AbstractObjectType.HalcyonPearl && (lastSeenRoom == "RM_AI" || lastSeenRoom == "CL_AI"))
		{
			Custom.Log("Halcyon pearl in pebbles room. Force location to safe location");
			if (lastSeenRoom == "RM_AI")
			{
				desiredSpawnLocation.Tile = new IntVector2(76, 40);
			}
			else
			{
				desiredSpawnLocation.Tile = new IntVector2(126, 6);
			}
		}
		if (ModManager.MSC && lastSeenRoom == "SS_AI" && saveSlot != MoreSlugcatsEnums.SlugcatStatsName.Artificer)
		{
			Custom.Log("Persistent object in pebbles room. Force location to safe location");
			lastSeenRoom = "SS_D07";
			desiredSpawnLocation.Tile = new IntVector2(178, 8);
		}
		array[3] = InjectDesiredSpawnToObjRepresentation(array[3]);
		setRepresentation(array[3]);
		if (array.Length > 4)
		{
			repData = array[4];
		}
		unrecognizedAttributes = SaveUtils.PopulateUnrecognizedStringAttrs(array, 5);
	}

	public void AbstractizeRepresentation(World w)
	{
		if (objRepresentation != "")
		{
			if (lastSeenRoom != "")
			{
				objRepresentation = InjectDesiredSpawnToObjRepresentation(objRepresentation);
			}
			Custom.Log("Abstractizing Persistent object from rep: " + objRepresentation);
			obj = SaveState.AbstractPhysicalObjectFromString(w, objRepresentation);
			if (obj == null)
			{
				Custom.LogWarning("Malformed object rep, unable to load persistent object.");
				return;
			}
			repType = obj.type;
			repData = getRepData(obj);
			obj.tracker = this;
			objRepresentation = "";
		}
	}

	public void UninitializeTracker()
	{
		if (obj != null && objRepresentation == "")
		{
			objRepresentation = obj.ToString();
			obj = null;
		}
	}

	public void setRepresentation(string rep)
	{
		objRepresentation = rep;
		if (rep != "")
		{
			string[] array = Regex.Split(rep, "<oA>");
			if (ExtEnum<AbstractPhysicalObject.AbstractObjectType>.values.entries.Contains(array[1]))
			{
				repType = new AbstractPhysicalObject.AbstractObjectType(array[1]);
			}
		}
	}

	public bool LinkObjectToTracker(AbstractPhysicalObject abstractObj)
	{
		if (objRepresentation != "" && CompatibleWithTracker(abstractObj))
		{
			obj = abstractObj;
			objRepresentation = "";
			abstractObj.tracker = this;
			return true;
		}
		return false;
	}

	public void ChangeDesiredSpawnLocation(WorldCoordinate newCoord)
	{
		desiredSpawnLocation = newCoord;
		if (objRepresentation != "")
		{
			objRepresentation = InjectDesiredSpawnToObjRepresentation(objRepresentation);
		}
	}

	public bool CompatibleWithTracker(AbstractPhysicalObject abstractObj)
	{
		if (abstractObj.type == repType)
		{
			return getRepData(abstractObj) == repData;
		}
		return false;
	}

	public string getRepData(AbstractPhysicalObject abstractObj)
	{
		if (abstractObj == null)
		{
			return "";
		}
		if (abstractObj is DataPearl.AbstractDataPearl)
		{
			return (abstractObj as DataPearl.AbstractDataPearl).dataPearlType.ToString();
		}
		if (abstractObj is VultureMask.AbstractVultureMask)
		{
			return "MSK_Vk" + ((!(abstractObj as VultureMask.AbstractVultureMask).king) ? "0" : "1") + "_Sk" + ((!(abstractObj as VultureMask.AbstractVultureMask).scavKing) ? "0" : "1");
		}
		return "";
	}

	public string InjectDesiredSpawnToObjRepresentation(string repString)
	{
		string[] array = Regex.Split(repString, "<oA>");
		if (array.Length >= 2)
		{
			array[2] = desiredSpawnLocation.SaveToString();
		}
		return string.Join("<oA>", array);
	}
}
