using Kittehface.Framework20;
using RWCustom;
using UnityEngine;

public class ScreenSafeArea : MonoBehaviour
{
	[SerializeField]
	private Rect safeArea;

	private RectTransform rectTransform;

	private bool safeAreaDirty = true;

	private static Vector2 safeAreaMargins = new Vector3(0f, 0f);

	private void Start()
	{
		rectTransform = GetComponent<RectTransform>();
		Platform.OnSystemPause += Platform_OnSystemPause;
	}

	private void Update()
	{
		if (!safeAreaDirty)
		{
			return;
		}
		safeAreaDirty = false;
		Rect rect = Screen.safeArea;
		if (rect != safeArea)
		{
			safeArea = rect;
			float num = rect.xMin / (float)Screen.width;
			float num2 = rect.yMin / (float)Screen.height;
			Canvas componentInParent = GetComponentInParent<Canvas>();
			if (componentInParent == null)
			{
				Custom.LogWarning("ERROR: ScreenSafeArea on object", base.gameObject.name.ToString(), "could not find canvas!");
				return;
			}
			Rect rect2 = componentInParent.GetComponent<RectTransform>().rect;
			float width = rect2.width;
			float height = rect2.height;
			safeAreaMargins.x = num;
			safeAreaMargins.y = num2;
			float x = width * num;
			float y = height * num2;
			float x2 = width * (0f - num);
			float y2 = height * (0f - num2);
			rectTransform.offsetMax = new Vector2(x2, y2);
			rectTransform.offsetMin = new Vector2(x, y);
		}
	}

	private void Platform_OnSystemPause(bool paused)
	{
		safeAreaDirty = true;
	}

	public static Vector2 GetMargins()
	{
		return safeAreaMargins;
	}

	private Rect ModifyRectByPercentage(Rect area, float pct)
	{
		float num = area.width - area.width * pct;
		area.width *= pct;
		area.xMin += num / 2f;
		float num2 = area.height - area.height * pct;
		area.height *= pct;
		area.yMin += num2 / 2f;
		return area;
	}
}
