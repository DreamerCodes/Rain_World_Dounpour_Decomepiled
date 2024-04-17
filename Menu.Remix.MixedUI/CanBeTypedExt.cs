using UnityEngine;

namespace Menu.Remix.MixedUI;

public static class CanBeTypedExt
{
	private static TypingHandler _handler;

	public static void Assign(this ICanBeTyped typable)
	{
		if (_handler == null)
		{
			_InitializeHandler();
		}
		_handler.Assign(typable);
		static void _InitializeHandler()
		{
			_handler = new GameObject("RemixTyping").AddComponent<TypingHandler>();
		}
	}

	public static bool IsAssigned(this ICanBeTyped typable)
	{
		if (_handler == null)
		{
			return false;
		}
		return _handler.IsAssigned(typable);
	}

	public static void Unassign(this ICanBeTyped typable)
	{
		_handler?.Unassign(typable);
	}

	internal static void _ClearAssigned()
	{
		if (_handler != null)
		{
			Object.Destroy(_handler.gameObject);
		}
	}

	internal static void _HandlerOnDestroy()
	{
		_handler = null;
	}
}
