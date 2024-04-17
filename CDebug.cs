using System.Diagnostics;
using RWCustom;
using UnityEngine;

public static class CDebug
{
	private static readonly AGLog<FromStaticClass> _log = new AGLog<FromStaticClass>();

	[Conditional("UNITY_EDITOR")]
	[Conditional("DEVELOPMENT_BUILD")]
	public static void Log(IntVector2 iv)
	{
	}

	[Conditional("UNITY_EDITOR")]
	[Conditional("DEVELOPMENT_BUILD")]
	public static void Log(string str, IntVector2 iv)
	{
	}

	[Conditional("UNITY_EDITOR")]
	[Conditional("DEVELOPMENT_BUILD")]
	public static void Log(Vector2 iv)
	{
	}

	[Conditional("UNITY_EDITOR")]
	[Conditional("DEVELOPMENT_BUILD")]
	public static void Log(string str, Vector2 iv)
	{
	}

	[Conditional("UNITY_EDITOR")]
	[Conditional("DEVELOPMENT_BUILD")]
	public static void Log(WorldCoordinate wc)
	{
	}

	[Conditional("UNITY_EDITOR")]
	[Conditional("DEVELOPMENT_BUILD")]
	public static void Log(string str, WorldCoordinate wc)
	{
	}

	[Conditional("UNITY_EDITOR")]
	[Conditional("DEVELOPMENT_BUILD")]
	public static void Log(MovementConnection mc)
	{
	}

	[Conditional("UNITY_EDITOR")]
	[Conditional("DEVELOPMENT_BUILD")]
	public static void Log(string str, MovementConnection mc)
	{
	}
}
