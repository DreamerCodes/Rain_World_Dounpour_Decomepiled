using System;
using System.Reflection;
using UnityEngine;

namespace Menu.Remix;

internal class StandardBoard : IBoard
{
	private static PropertyInfo m_systemCopyBufferProperty;

	private static PropertyInfo GetSystemCopyBufferProperty()
	{
		if (m_systemCopyBufferProperty == null)
		{
			Type typeFromHandle = typeof(GUIUtility);
			m_systemCopyBufferProperty = typeFromHandle.GetProperty("systemCopyBuffer", BindingFlags.Static | BindingFlags.Public);
			if (m_systemCopyBufferProperty == null)
			{
				m_systemCopyBufferProperty = typeFromHandle.GetProperty("systemCopyBuffer", BindingFlags.Static | BindingFlags.NonPublic);
			}
			if (m_systemCopyBufferProperty == null)
			{
				throw new Exception("Can't access internal member 'GUIUtility.systemCopyBuffer' it may have been removed / renamed");
			}
		}
		return m_systemCopyBufferProperty;
	}

	public void SetText(string str)
	{
		GetSystemCopyBufferProperty().SetValue(null, str, null);
	}

	public string GetText()
	{
		return (string)GetSystemCopyBufferProperty().GetValue(null, null);
	}
}
