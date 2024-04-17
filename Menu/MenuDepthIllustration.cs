using UnityEngine;

namespace Menu;

public class MenuDepthIllustration : MenuIllustration
{
	public class MenuShader : ExtEnum<MenuShader>
	{
		public static readonly MenuShader Normal = new MenuShader("Normal", register: true);

		public static readonly MenuShader Lighten = new MenuShader("Lighten", register: true);

		public static readonly MenuShader LightEdges = new MenuShader("LightEdges", register: true);

		public static readonly MenuShader Rain = new MenuShader("Rain", register: true);

		public static readonly MenuShader Overlay = new MenuShader("Overlay", register: true);

		public static readonly MenuShader Basic = new MenuShader("Basic", register: true);

		public static readonly MenuShader SoftLight = new MenuShader("SoftLight", register: true);

		public static readonly MenuShader Multiply = new MenuShader("Multiply", register: true);

		public MenuShader(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	public float depth;

	public MenuShader shader;

	public Vector2 offset = Vector2.zero;

	public MenuDepthIllustration(Menu menu, MenuObject owner, string folderName, string fileName, Vector2 pos, float depth, MenuShader shader)
		: base(menu, owner, folderName, fileName, pos, crispPixels: false, anchorCenter: false)
	{
		this.shader = shader;
		this.depth = depth;
		size = new Vector2(texture.width, (float)texture.height / 2f);
		sprite.scaleY = 0.5f;
		if (shader == MenuShader.Normal)
		{
			sprite.shader = menu.manager.rainWorld.Shaders["SceneBlur"];
		}
		else if (shader == MenuShader.Lighten)
		{
			sprite.shader = menu.manager.rainWorld.Shaders["SceneLighten"];
		}
		else if (shader == MenuShader.LightEdges)
		{
			sprite.shader = menu.manager.rainWorld.Shaders["SceneBlurLightEdges"];
		}
		else if (shader == MenuShader.Overlay)
		{
			sprite.shader = menu.manager.rainWorld.Shaders["SceneOverlay"];
		}
		else if (shader == MenuShader.SoftLight)
		{
			sprite.shader = menu.manager.rainWorld.Shaders["SceneSoftLight"];
		}
		else if (shader == MenuShader.Multiply)
		{
			sprite.shader = menu.manager.rainWorld.Shaders["SceneMultiply"];
		}
		else if (shader == MenuShader.Rain)
		{
			sprite.shader = menu.manager.rainWorld.Shaders["SceneRain"];
			size.y *= 2f;
			sprite.scaleY = 1f;
		}
		else if (shader == MenuShader.Basic)
		{
			sprite.shader = menu.manager.rainWorld.Shaders["Basic"];
			size.y *= 2f;
			sprite.scaleY = 1f;
		}
	}

	public float DepthAtPosition(Vector2 ps, bool devtool)
	{
		if (texture == null)
		{
			return -1f;
		}
		Vector2 vector = default(Vector2);
		vector.x = pos.x - (owner as MenuScene).CamPos(1f).x * 80f / depth;
		vector.y = pos.y - (owner as MenuScene).CamPos(1f).y * 80f / depth;
		ps -= vector;
		if (ps.x < 0f || ps.y < 0f || ps.x > size.x || ps.y > size.y)
		{
			return -1f;
		}
		if (shader == MenuShader.Basic || shader == MenuShader.Rain)
		{
			if (texture.GetPixel((int)ps.x, (int)ps.y).a < 0.25f)
			{
				return -1f;
			}
			return 1f + depth;
		}
		if (texture.GetPixel((int)ps.x, (int)(size.y + ps.y)).a < 0.25f)
		{
			return -1f;
		}
		if (devtool)
		{
			return depth;
		}
		return (1f - texture.GetPixel((int)ps.x, (int)ps.y).r) * 2f + depth;
	}

	public override void GrafUpdate(float timeStacker)
	{
		base.GrafUpdate(timeStacker);
		sprite.x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) - (owner as MenuScene).CamPos(timeStacker).x * 80f / depth;
		sprite.y = Mathf.Lerp(lastPos.y, pos.y, timeStacker) - (owner as MenuScene).CamPos(timeStacker).y * 80f / depth;
		if (shader != MenuShader.Basic)
		{
			sprite.color = new Color(Mathf.InverseLerp(1f, 1400f, size.x), Mathf.InverseLerp(1f, 800f, size.y), Mathf.InverseLerp(0f, 10f, depth));
		}
	}
}
