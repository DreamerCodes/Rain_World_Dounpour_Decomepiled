using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Menu.Remix;
using RWCustom;
using UnityEngine;

namespace JollyCoop.JollyMenu;

public class JollyPlayerOptions
{
	public int playerNumber;

	public bool joined;

	private Color bodyColor;

	private Color faceColor;

	private Color uniqueColor;

	public bool colorsEverModified;

	public bool backSpear;

	public bool isPup;

	public SlugcatStats.Name playerClass;

	public string customPlayerName;

	public JollyPlayerOptions(int playerNumber)
	{
		this.playerNumber = playerNumber;
		joined = true;
		colorsEverModified = false;
		playerClass = null;
		bodyColor = GetBodyColor();
		faceColor = GetFaceColor();
		uniqueColor = GetUniqueColor();
		backSpear = false;
		isPup = false;
		customPlayerName = null;
	}

	public void SetColorsToDefault(SlugcatStats.Name slugcat)
	{
		List<string> list = PlayerGraphics.DefaultBodyPartColorHex(slugcat);
		if (list.Count >= 1)
		{
			bodyColor = Custom.hexToColor(list[0]);
		}
		if (list.Count >= 2)
		{
			faceColor = Custom.hexToColor(list[1]);
		}
		if (list.Count >= 3)
		{
			uniqueColor = Custom.hexToColor(list[2]);
		}
	}

	public void SetBodyColor(Color col)
	{
		bodyColor = col;
		colorsEverModified = true;
	}

	public void SetFaceColor(Color col)
	{
		faceColor = col;
		colorsEverModified = true;
	}

	public void SetUniqueColor(Color col)
	{
		uniqueColor = col;
		colorsEverModified = true;
	}

	public Color GetBodyColor()
	{
		if (colorsEverModified)
		{
			return bodyColor;
		}
		List<string> list = null;
		if (playerClass != null)
		{
			list = PlayerGraphics.DefaultBodyPartColorHex(playerClass);
		}
		if (list != null && list.Count >= 1)
		{
			return Custom.hexToColor(list[0]);
		}
		return Color.red;
	}

	public Color GetFaceColor()
	{
		if (colorsEverModified)
		{
			return faceColor;
		}
		List<string> list = null;
		if (playerClass != null)
		{
			list = PlayerGraphics.DefaultBodyPartColorHex(playerClass);
		}
		if (list != null && list.Count >= 2)
		{
			return Custom.hexToColor(list[1]);
		}
		return Color.blue;
	}

	public Color GetUniqueColor()
	{
		if (colorsEverModified)
		{
			return uniqueColor;
		}
		List<string> list = null;
		if (playerClass != null)
		{
			list = PlayerGraphics.DefaultBodyPartColorHex(playerClass);
		}
		if (list != null && list.Count >= 3)
		{
			return Custom.hexToColor(list[2]);
		}
		return Color.green;
	}

	public void FromString(string origin)
	{
		string[] array = origin.Split('#');
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = array[i].Split(new char[1] { '=' }, 2);
			if (array2.Length != 2)
			{
				continue;
			}
			string name = array2[0].Trim();
			string[] array3 = Regex.Split(array2[1], "<>");
			if (array3.Length != 2)
			{
				continue;
			}
			string text = array3[0].Trim();
			string text2 = array3[1].Trim();
			FieldInfo field = GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field != null && text != string.Empty)
			{
				Type type = Type.GetType(text2);
				JollyCustom.Log("Setting value of " + text + ", type " + type?.ToString() + " (" + text2 + ")");
				if (type == null && text2.Equals("UnityEngine.Color"))
				{
					type = typeof(Color);
				}
				field.SetValue(this, ValueConverter.ConvertToValue(text, type));
			}
		}
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		FieldInfo[] fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		if (playerNumber == 0)
		{
			joined = true;
		}
		FieldInfo[] array = fields;
		foreach (FieldInfo fieldInfo in array)
		{
			string text = string.Empty;
			object value = fieldInfo.GetValue(this);
			if (value != null)
			{
				text = ValueConverter.ConvertToString(value, fieldInfo.FieldType);
			}
			stringBuilder.Append(string.Concat(fieldInfo.Name, "=", text, "<>", fieldInfo.FieldType, "#"));
		}
		return stringBuilder.ToString();
	}
}
