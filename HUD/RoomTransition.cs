using System;
using System.IO;
using RWCustom;
using UnityEngine;

namespace HUD;

public class RoomTransition : HudPart
{
	public FSprite sprite;

	public FSprite lightSprite;

	public Vector2 pos;

	public Vector2 lastPos;

	public float fade;

	public float lastFade;

	public float fadeInMode;

	public float lastFadeInMode;

	public float fadeSpeed;

	public RoomCamera cam;

	private int lastRoom = -1;

	public int fadeOutWaitFrames;

	private FloatRect spriteRect;

	private int waitToStartFadeIn;

	public Vector3 fromColor = new Vector3(-1f, -1f, -1f);

	public Vector3 toColor;

	public float colorFade;

	public float lastColorFade;

	public float colorFadeSpeed;

	private int colorFadeWait;

	public float lightFade;

	public float lastLightFade;

	public float lightFadeSpeed;

	private float maxOpacity;

	public RoomTransition(HUD hud, FContainer fContainer, RoomCamera cam)
		: base(hud)
	{
		this.cam = cam;
		sprite = new FSprite("Futile_White");
		sprite.shader = hud.rainWorld.Shaders["RoomTransition"];
		sprite.alpha = 0.85f;
		fContainer.AddChild(sprite);
		sprite.scaleX = 87.5f;
		sprite.scaleY = 87.5f;
		sprite.x = hud.rainWorld.screenSize.x / 2f;
		sprite.y = hud.rainWorld.screenSize.y / 2f;
		spriteRect = new FloatRect(hud.rainWorld.screenSize.x / 2f - 800f, hud.rainWorld.screenSize.y / 2f - 800f, hud.rainWorld.screenSize.x / 2f + 800f, hud.rainWorld.screenSize.y / 2f + 800f);
		lightSprite = new FSprite("Futile_White");
		lightSprite.shader = hud.rainWorld.Shaders["FlatLight"];
		lightSprite.color = new Color(1f, 1f, 1f);
		fContainer.AddChild(lightSprite);
	}

	public override void Update()
	{
		lastPos = pos;
		lastFade = fade;
		lastFadeInMode = fadeInMode;
		lastLightFade = lightFade;
		lightFade = Mathf.Min(1f, lightFade + lightFadeSpeed);
		fadeInMode = Mathf.Lerp(fadeInMode, (fadeSpeed < 0f || waitToStartFadeIn > 0) ? 1f : fade, 0.1f);
		lastColorFade = colorFade;
		if (colorFadeWait > 0)
		{
			colorFadeWait--;
		}
		else
		{
			colorFade = Mathf.Min(1f, colorFade + colorFadeSpeed);
		}
		if (cam.followAbstractCreature.realizedCreature != null && cam.followAbstractCreature.realizedCreature.room == cam.room)
		{
			pos = cam.followAbstractCreature.realizedCreature.mainBodyChunk.pos - cam.pos;
			fadeSpeed -= 0.0125f;
		}
		else
		{
			Vector2? vector = cam.game.shortcuts.OnScreenPositionOfInShortCutCreature(cam.room, cam.followAbstractCreature.realizedCreature);
			if (vector.HasValue)
			{
				pos = vector.Value - cam.pos;
			}
		}
		if (waitToStartFadeIn > 0)
		{
			waitToStartFadeIn--;
			if (waitToStartFadeIn == 0)
			{
				fadeSpeed = -0.0125f;
				lightFadeSpeed = 1f / 60f;
			}
		}
		if (lastRoom != cam.followAbstractCreature.pos.room)
		{
			waitToStartFadeIn = 2;
			lastRoom = cam.followAbstractCreature.pos.room;
			fadeOutWaitFrames = 0;
			lastLightFade = 0f;
			lightFade = 0f;
			lightFadeSpeed = 0f;
		}
		if (fadeOutWaitFrames > 0)
		{
			fadeOutWaitFrames--;
		}
		else
		{
			fade = Mathf.Clamp(fade + fadeSpeed, 0f, 1f);
		}
	}

	public void PlayerEnterShortcut(ShortcutData shortcut)
	{
		int num = Math.Max(0, shortcut.length - 1);
		int num2 = Math.Min(8, num);
		fadeOutWaitFrames = (num - num2) * 3;
		fadeSpeed = Mathf.Min(0.25f, 0.33f / (float)num2);
		lastRoom = cam.followAbstractCreature.pos.room;
		fadeInMode = 0f;
		int destNode = shortcut.destNode;
		if (destNode <= -1)
		{
			return;
		}
		int room = cam.room.abstractRoom.connections[destNode];
		fromColor = Custom.ColorToVec3(FadeColor(1f));
		colorFade = 0f;
		if (cam.game.world.GetAbstractRoom(room) != null && cam.game.world.GetAbstractRoom(room).realizedRoom != null && cam.game.world.GetAbstractRoom(room).realizedRoom.shortCutsReady)
		{
			int palette = cam.game.world.GetAbstractRoom(room).realizedRoom.roomSettings.Palette;
			Texture2D texture2D = new Texture2D(1, 1);
			string text = AssetManager.ResolveFilePath("Palettes" + Path.DirectorySeparatorChar + "palette" + palette + ".png");
			AssetManager.SafeWWWLoadTexture(ref texture2D, "file:///" + text, clampWrapMode: false, crispPixels: false);
			Color color = texture2D.GetPixel(14, 15);
			if (cam.game.world.GetAbstractRoom(room).realizedRoom.roomSettings.fadePalette != null)
			{
				int viewedByCamera = cam.game.world.GetAbstractRoom(room).nodes[cam.game.world.GetAbstractRoom(room).ExitIndex(cam.room.abstractRoom.index)].viewedByCamera;
				if (cam.game.world.GetAbstractRoom(room).realizedRoom.roomSettings.fadePalette.fades[viewedByCamera] > 0f)
				{
					text = AssetManager.ResolveFilePath("Palettes" + Path.DirectorySeparatorChar + "palette" + cam.game.world.GetAbstractRoom(room).realizedRoom.roomSettings.fadePalette.palette + ".png");
					AssetManager.SafeWWWLoadTexture(ref texture2D, "file:///" + text, clampWrapMode: false, crispPixels: false);
					color = Color.Lerp(color, texture2D.GetPixel(14, 15), cam.game.world.GetAbstractRoom(room).realizedRoom.roomSettings.fadePalette.fades[viewedByCamera]);
				}
			}
			toColor = Custom.ColorToVec3(color);
			int num3 = fadeOutWaitFrames + num * 3;
			colorFadeSpeed = 1f / ((float)(num3 / 2) + (float)cam.game.world.GetAbstractRoom(room).realizedRoom.ShortcutLeadingToNode(cam.game.world.GetAbstractRoom(room).ExitIndex(cam.room.abstractRoom.index)).length * 2f);
			colorFadeWait = fadeOutWaitFrames + 4;
			maxOpacity = Custom.LerpMap(shortcut.length + cam.game.world.GetAbstractRoom(room).realizedRoom.ShortcutLeadingToNode(cam.game.world.GetAbstractRoom(room).ExitIndex(cam.room.abstractRoom.index)).length, 8f, 30f, 0.25f, 0.5f);
			UnityEngine.Object.Destroy(texture2D);
		}
		else
		{
			colorFadeSpeed = 1f / 30f;
			colorFadeWait = 10;
		}
	}

	public override void Draw(float timeStacker)
	{
		float t = Mathf.Lerp(lastFadeInMode, fadeInMode, timeStacker);
		float num = Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(lastFade, fade, timeStacker)), Mathf.Lerp(3f, 0.5f, t));
		float num2 = Mathf.Sin(Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(lastLightFade, lightFade, timeStacker)), 0.2f) * (float)Math.PI) * Mathf.Lerp(lastFade, fade, timeStacker);
		if (num2 > 0f)
		{
			lightSprite.x = Mathf.Lerp(lastPos.x, pos.x, timeStacker);
			lightSprite.y = Mathf.Lerp(lastPos.y, pos.y, timeStacker);
			lightSprite.scale = Mathf.Lerp(150f, 800f, Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(lastLightFade, lightFade, timeStacker)), 2f)) / 8f;
			lightSprite.alpha = num2 * 0.05f;
			lightSprite.isVisible = true;
		}
		else
		{
			lightSprite.isVisible = false;
		}
		if (num > 0f)
		{
			sprite.alpha = num;
			sprite.color = new Color(Mathf.InverseLerp(spriteRect.left, spriteRect.right, Mathf.Lerp(lastPos.x, pos.x, timeStacker)), Mathf.InverseLerp(spriteRect.bottom, spriteRect.top, Mathf.Lerp(lastPos.y, pos.y, timeStacker)), maxOpacity);
			sprite.isVisible = true;
			Shader.SetGlobalVector(RainWorld.ShadPropTransitionColor, FadeColor(timeStacker));
		}
		else
		{
			sprite.isVisible = false;
		}
	}

	private Color FadeColor(float timeStacker)
	{
		return Custom.Vec3ToColor(Vector3.Lerp(Vector3.Slerp(fromColor, toColor, Mathf.Lerp(lastColorFade, colorFade, timeStacker)), Vector3.Lerp(fromColor, toColor, Mathf.Lerp(lastColorFade, colorFade, timeStacker)), 0.75f));
	}

	public override void ClearSprites()
	{
		base.ClearSprites();
		sprite.RemoveFromContainer();
		lightSprite.RemoveFromContainer();
	}
}
