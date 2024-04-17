using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using RWCustom;

internal static class RoomPreprocessor
{
	public static string[] PreprocessRoom(AbstractRoom abstractRoom, string[] levelText, World world, RainWorldGame.SetupValues setupValues, int preprocessingGeneration)
	{
		Room room = new Room(null, world, abstractRoom);
		RoomPreparer roomPreparer = new RoomPreparer(room, loadAiHeatMaps: false, !setupValues.bake, shortcutsOnly: false);
		while (!roomPreparer.done)
		{
			roomPreparer.Update();
			Thread.Sleep(1);
		}
		abstractRoom.InitNodes(roomPreparer.ReturnRoomConnectivity(), levelText[1]);
		levelText[9] = ConnMapToString(setupValues.bake ? preprocessingGeneration : 0, abstractRoom.nodes);
		levelText[10] = CompressAIMapsToString(room.aimap);
		return levelText;
	}

	public static string IntArrayToString(int[] ia)
	{
		byte[] array = new byte[ia.Length * 4];
		for (int i = 0; i < ia.Length; i++)
		{
			byte[] bytes = BitConverter.GetBytes(ia[i]);
			for (int j = 0; j < 4; j++)
			{
				array[i * 4 + j] = bytes[j];
			}
		}
		return Convert.ToBase64String(array);
	}

	public static int[] StringToIntArray(string s)
	{
		byte[] array = Convert.FromBase64String(s);
		int[] array2 = new int[array.Length / 4];
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i] = BitConverter.ToInt32(array, i * 4);
		}
		return array2;
	}

	public static string FloatArrayToString(float[] fa)
	{
		byte[] array = new byte[fa.Length * 4];
		for (int i = 0; i < fa.Length; i++)
		{
			byte[] bytes = BitConverter.GetBytes(fa[i]);
			for (int j = 0; j < 4; j++)
			{
				array[i * 4 + j] = bytes[j];
			}
		}
		return Convert.ToBase64String(array);
	}

	public static float[] StringToFloatArray(string s)
	{
		byte[] array = Convert.FromBase64String(s);
		float[] array2 = new float[array.Length / 4];
		for (int i = 0; i < array2.Length; i++)
		{
			array2[i] = BitConverter.ToSingle(array, i * 4);
		}
		return array2;
	}

	public static string CompressAIMapsToString(AImap aimap)
	{
		string text = "";
		text += IntArrayToString(aimap.GetCompressedVisibilityMap());
		for (int i = 0; i < StaticWorld.preBakedPathingCreatures.Length; i++)
		{
			text += "<<DIV - A>>";
			text = text + IntArrayToString(aimap.creatureSpecificAImaps[i].ReturnCompressedIntGrid()) + "<<DIV - B>>" + FloatArrayToString(aimap.creatureSpecificAImaps[i].ReturnCompressedFloatGrid());
		}
		return text;
	}

	public static CreatureSpecificAImap[] DecompressStringToAImaps(string s, AImap aimap)
	{
		CreatureSpecificAImap[] array = new CreatureSpecificAImap[StaticWorld.preBakedPathingCreatures.Length];
		if (s == null)
		{
			Custom.LogWarning("AI MAP STRING WAS NULL!");
			for (int i = 0; i < StaticWorld.preBakedPathingCreatures.Length; i++)
			{
				array[i] = new CreatureSpecificAImap(aimap, StaticWorld.preBakedPathingCreatures[i]);
			}
			return array;
		}
		string[] array2 = Regex.Split(s, "<<DIV - A>>");
		aimap.SetVisibilityMapFromCompressedArray(StringToIntArray(array2[0]));
		for (int j = 0; j < StaticWorld.preBakedPathingCreatures.Length; j++)
		{
			array[j] = new CreatureSpecificAImap(aimap, StaticWorld.preBakedPathingCreatures[j]);
			try
			{
				int[] intArray = StringToIntArray(Regex.Split(array2[j + 1], "<<DIV - B>>")[0]);
				float[] floatArray = StringToFloatArray(Regex.Split(array2[j + 1], "<<DIV - B>>")[1]);
				array[j].LoadFromCompressedIntGrid(intArray);
				array[j].LoadFromCompressedFloatGrid(floatArray);
			}
			catch (FormatException)
			{
				Custom.LogWarning("AI MAP STRING WAS IN THE WRONG FORMAT:", array2[j + 1]);
			}
		}
		return array;
	}

	public static string ConnMapToString(int connMapGeneration, AbstractRoomNode[] connMap)
	{
		string text = connMapGeneration + "|";
		text = text + connMap.Length + "|";
		text = text + connMap[0].connectivity.GetLength(0) + "|";
		for (int i = 0; i < connMap.Length; i++)
		{
			if (connMap[i].type.Index == -1)
			{
				continue;
			}
			text = text + (int)connMap[i].type + ",";
			text = text + connMap[i].shortCutLength + ",";
			text = text + (connMap[i].submerged ? 1 : 0) + ",";
			text = text + connMap[i].viewedByCamera + ",";
			text = text + connMap[i].entranceWidth + ",";
			for (int j = 0; j < connMap[0].connectivity.GetLength(0); j++)
			{
				for (int k = 0; k < connMap.Length; k++)
				{
					text = text + connMap[i].connectivity[j, k, 0] + " " + connMap[i].connectivity[j, k, 1] + ",";
				}
			}
			text += "|";
		}
		return text;
	}

	public static AbstractRoomNode[] StringToConnMap(string str)
	{
		string[] array = str.Split('|');
		AbstractRoomNode[] array2 = new AbstractRoomNode[Convert.ToInt32(array[1], CultureInfo.InvariantCulture)];
		int num = Convert.ToInt32(array[2], CultureInfo.InvariantCulture);
		int num2 = 3;
		for (int i = 0; i < array2.Length; i++)
		{
			string[] array3 = array[num2].Split(',');
			array2[i] = new AbstractRoomNode(new AbstractRoomNode.Type(ExtEnum<AbstractRoomNode.Type>.values.GetEntry(Convert.ToInt32(array3[0], CultureInfo.InvariantCulture))), Convert.ToInt32(array3[1], CultureInfo.InvariantCulture), array2.Length, Convert.ToInt32(array3[2], CultureInfo.InvariantCulture) == 1, Convert.ToInt32(array3[3], CultureInfo.InvariantCulture), Convert.ToInt32(array3[4], CultureInfo.InvariantCulture));
			for (int j = 0; j < num; j++)
			{
				for (int k = 0; k < array2.Length; k++)
				{
					array2[i].connectivity[j, k, 0] = Convert.ToInt32(array3[j * array2.Length + k + 5].Split(' ')[0], CultureInfo.InvariantCulture);
					array2[i].connectivity[j, k, 1] = Convert.ToInt32(array3[j * array2.Length + k + 5].Split(' ')[1], CultureInfo.InvariantCulture);
				}
			}
			num2++;
		}
		return array2;
	}

	public static bool VersionFix(ref string[] roomText)
	{
		bool result = false;
		if (roomText.Length == 8)
		{
			string[] array = new string[12];
			for (int i = 0; i < 5; i++)
			{
				array[i] = roomText[i];
			}
			array[5] = "";
			array[6] = "";
			array[7] = "";
			array[8] = "";
			for (int j = 5; j < 8; j++)
			{
				array[j + 4] = roomText[j];
			}
			roomText = array;
			result = true;
		}
		if (roomText[1].Split('|').Length < 3)
		{
			roomText[1] = roomText[1] + "|-1|0";
			result = true;
		}
		if (roomText[2].Split('|').Length < 3)
		{
			roomText[2] = roomText[2] + "|0|0";
			result = true;
		}
		return result;
	}
}
