using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using HUD;
using Menu;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace ArenaBehaviors;

public class SandboxEditor : ArenaGameBehavior
{
	public class EditCursor : UpdatableAndDeletable, IDrawable
	{
		public SandboxEditor editor;

		public SandboxEditorSelector.ButtonCursor menuCursor;

		public int playerNumber;

		public OverseerAbstractAI overseer;

		public Vector2 pos;

		public Vector2 lastPos;

		public Vector2 homePos;

		public Vector2 lastHomePos;

		public Vector2 vel;

		private Vector2 lastPushAroundPos;

		private Vector2 pushAroundPos;

		private float square;

		private float lastSquare;

		private float homeIn;

		private float lastHomeIn;

		private float lingerFac;

		private float lastMenuFac;

		private float menuFac;

		private float lastBump;

		private float bump;

		private float lastQuality;

		private float quality;

		public PlacedIcon homeInIcon;

		public PlacedIcon dragIcon;

		public PlacedIcon lastHomeInIcon;

		private bool lastDragIcon;

		private Vector2 dragOffset;

		public Player.InputPackage[] input;

		public float[] rotations;

		private int rotat;

		private int counter;

		private float mobile;

		private float lastMobile;

		public bool menuMode;

		public bool mouseMode;

		private bool red;

		public Vector2 ScreenPos => pos - room.game.cameras[0].pos;

		public bool MainCursor
		{
			get
			{
				if (editor.gameSession.arenaSitting.players.Count != 0)
				{
					return playerNumber == editor.gameSession.arenaSitting.players[0].playerNumber;
				}
				return true;
			}
		}

		public bool OverseerActive
		{
			get
			{
				if (overseer.parent.realizedCreature != null && overseer.parent.realizedCreature.room == room)
				{
					return (overseer.parent.realizedCreature as Overseer).mode != Overseer.Mode.Zipping;
				}
				return false;
			}
		}

		public Vector2 OverseerEyePos(float timeStacker)
		{
			if (overseer.parent.realizedCreature == null)
			{
				return Vector2.Lerp(lastPos, pos, timeStacker);
			}
			if (overseer.parent.realizedCreature.graphicsModule == null || overseer.parent.realizedCreature.room == null)
			{
				return Vector2.Lerp(overseer.parent.realizedCreature.mainBodyChunk.lastPos, overseer.parent.realizedCreature.mainBodyChunk.pos, timeStacker);
			}
			return (overseer.parent.realizedCreature.graphicsModule as OverseerGraphics).DrawPosOfSegment(0f, timeStacker);
		}

		public EditCursor(SandboxEditor editor, OverseerAbstractAI overseer, int playerNumber, Vector2 initPos)
		{
			this.editor = editor;
			this.playerNumber = playerNumber;
			this.overseer = overseer;
			pos = initPos;
			lastPos = initPos;
			lastHomePos = initPos;
			homePos = initPos;
			rotations = new float[5];
			rotations[0] = 1f;
			input = new Player.InputPackage[10];
		}

		public void NewRotation(float to, float goalSpeed)
		{
			if (!(rotations[0] < 1f) && !(rotations[1] < 1f) && to != rotations[3])
			{
				rotations[0] = 0f;
				rotations[1] = 0f;
				rotations[2] = rotations[3];
				rotations[3] = to;
				rotations[4] = Mathf.Lerp(1f / (Mathf.Abs(rotations[2] - rotations[3]) * 60f), goalSpeed, 0.5f);
			}
		}

		public float GetRotation(float timeStacker)
		{
			return Mathf.Lerp(rotations[2], rotations[3], Custom.SCurve(Mathf.Lerp(rotations[1], rotations[0], timeStacker), 0.65f));
		}

		public void Bump(bool redBump)
		{
			bump = 1f;
			red = redBump;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (overseer.parent.realizedCreature != null)
			{
				(overseer.parent.realizedCreature as Overseer).editCursor = this;
			}
			bool flag = false;
			mouseMode = editor.game.manager.menuesMouseMode && MainCursor;
			counter++;
			lastHomeIn = homeIn;
			lastHomePos = homePos;
			lastPos = pos;
			lastSquare = square;
			lastDragIcon = dragIcon != null;
			lastMobile = mobile;
			lastMenuFac = menuFac;
			lastBump = bump;
			lastQuality = quality;
			lastPushAroundPos = pushAroundPos;
			pushAroundPos *= 0.8f;
			if (OverseerActive && (overseer.parent.realizedCreature as Overseer).extended > 0f)
			{
				pushAroundPos += (overseer.parent.realizedCreature.firstChunk.pos - overseer.parent.realizedCreature.firstChunk.lastPos) * (overseer.parent.realizedCreature as Overseer).extended;
			}
			if (OverseerActive)
			{
				quality = Mathf.Min(1f, quality + 0.05f);
			}
			else
			{
				quality = Mathf.Max(0f, quality - 1f / Mathf.Lerp(30f, 80f, quality));
			}
			if (UnityEngine.Random.value < 0.1f)
			{
				quality = Mathf.Min(quality, Mathf.InverseLerp(600f, 400f, Vector2.Distance(OverseerEyePos(1f), pos)));
			}
			rotations[1] = rotations[0];
			rotations[0] = Mathf.Min(1f, rotations[0] + rotations[4]);
			menuFac = Custom.LerpAndTick(menuFac, menuMode ? 1f : 0f, 0.07f, 0.1f);
			bump = Mathf.Max(0f, bump - 1f / 30f);
			if (bump == 0f && lastBump == 0f)
			{
				red = false;
			}
			if (mouseMode)
			{
				vel *= 0f;
				pos = (Vector2)Futile.mousePosition + editor.game.cameras[0].pos;
			}
			else
			{
				pos += vel;
				vel *= 0.6f * (1f - homeIn);
			}
			for (int num = input.Length - 1; num > 0; num--)
			{
				input[num] = input[num - 1];
			}
			if (mouseMode)
			{
				input[0] = new Player.InputPackage(gamePad: false, Options.ControlSetup.Preset.KeyboardSinglePlayer, 0, 0, jmp: false, Input.GetKey(KeyCode.Mouse0), Input.GetKey(KeyCode.Mouse1), mp: false, crouchToggle: false);
			}
			else
			{
				input[0] = RWInput.PlayerInput(playerNumber);
			}
			float num2 = 0f;
			if (input[0].pckp && !input[1].pckp)
			{
				menuMode = !menuMode;
				flag = true;
				room.PlaySound(menuMode ? SoundID.SANDBOX_Overseer_Switch_To_Menu_Mode : SoundID.SANDBOX_Overseer_Switch_Out_Of_Menu_Mode, 0f, 1f, 1f);
				if (menuMode && mouseMode)
				{
					editor.overlay.sandboxEditorSelector.MouseCursorEnterMenuMode(this);
				}
			}
			if (menuMode)
			{
				if (input[0].x != 0 && input[0].x != input[1].x)
				{
					menuCursor.Move(input[0].x, 0);
				}
				if (input[0].y != 0 && input[0].y != input[1].y)
				{
					menuCursor.Move(0, input[0].y);
				}
				if (!mouseMode && input[0].thrw != input[1].thrw)
				{
					if (input[0].thrw)
					{
						menuCursor.clickOnRelease = true;
					}
					else if (menuCursor.clickOnRelease)
					{
						menuCursor.Click();
					}
				}
				mobile = Custom.LerpAndTick(mobile, 0f, 0.02f, 1f / 30f);
			}
			else
			{
				Vector2 vector = input[0].analogueDir;
				if (vector.x == 0f && vector.y == 0f && (input[0].x != 0 || input[0].y != 0))
				{
					vector = Custom.DirVec(new Vector2(0f, 0f), new Vector2(input[0].x, input[0].y));
				}
				vel += vector * 2f;
				pos += vector * 5f;
				mobile = Custom.LerpAndTick(mobile, vector.magnitude, 0.02f, 1f / 30f);
				if (!input[0].jmp && input[1].jmp)
				{
					if (dragIcon != null)
					{
						pos = dragIcon.pos;
						homeIn = 0f;
						flag = true;
						room.PlaySound(SoundID.SANDBOX_Remove_Item, pos, 1f, 1f);
						editor.RemoveIcon(dragIcon, updatePerfEstimate: true);
						dragIcon = null;
						Bump(redBump: true);
						square *= 0.5f;
					}
					else if (homeInIcon != null && homeIn > 0.65f)
					{
						pos = homeInIcon.pos;
						homeIn = 0f;
						flag = true;
						room.PlaySound(SoundID.SANDBOX_Remove_Item, pos, 1f, 1f);
						editor.RemoveIcon(homeInIcon, updatePerfEstimate: true);
						homeInIcon = null;
						Bump(redBump: true);
						square *= 0.5f;
					}
				}
			}
			pos.x = Mathf.Clamp(pos.x, room.game.cameras[0].pos.x, room.game.cameras[0].pos.x + room.game.cameras[0].sSize.x);
			pos.y = Mathf.Clamp(pos.y, room.game.cameras[0].pos.y, room.game.cameras[0].pos.y + room.game.cameras[0].sSize.y);
			square = Custom.LerpAndTick(square, (homeInIcon != null || dragIcon != null) ? 1f : 0f, 0.03f, 0.025f);
			if (dragIcon != null)
			{
				dragIcon.pos = Vector2.Lerp(dragIcon.pos, pos + dragOffset + pushAroundPos * 0.5f, 0.8f);
				rotations[0] = 1f;
				if (!mouseMode)
				{
					homeIn = Mathf.Max(0f, homeIn - 1f / 30f);
				}
				homeInIcon = dragIcon;
				homePos = dragIcon.pos;
				if (!menuMode && input[0].thrw && !input[1].thrw)
				{
					flag = true;
					room.PlaySound(SoundID.SANDBOX_Release_Item, pos, 1f, 1f);
					dragIcon = null;
				}
				if (dragIcon != null && mouseMode && !input[0].thrw)
				{
					room.PlaySound(SoundID.SANDBOX_Release_Item, pos, 1f, 1f);
					editor.overlay.trashBin.IconReleased(dragIcon);
					dragIcon = null;
				}
			}
			else
			{
				if (UnityEngine.Random.value < 1f / 45f)
				{
					rotat += UnityEngine.Random.Range(1, UnityEngine.Random.Range(1, 4)) * ((!(UnityEngine.Random.value < 0.5f)) ? 1 : (-1));
					NewRotation((float)rotat / 4f, 1f / Mathf.Lerp(30f, 80f, UnityEngine.Random.value));
				}
				if (menuMode)
				{
					homeInIcon = null;
				}
				else
				{
					PlacedIcon placedIcon = null;
					float dst = 50f;
					for (int i = 0; i < editor.icons.Count; i++)
					{
						if (Custom.DistLess(pos, editor.icons[i].pos, dst) && editor.icons[i].DraggedBy == null)
						{
							dst = Vector2.Distance(pos, editor.icons[i].pos);
							placedIcon = editor.icons[i];
						}
					}
					if (placedIcon != homeInIcon)
					{
						if (homeInIcon != null)
						{
							lingerFac = 1f;
						}
						placedIcon?.Flash();
						homeInIcon = placedIcon;
					}
				}
				if (homeInIcon != null)
				{
					homeIn = Custom.LerpAndTick(homeIn, 1f, 0.03f, 1f / (30f + num2 * 120f));
				}
				else
				{
					homeIn = Custom.LerpAndTick(homeIn, 0f, 0.03f, 1f / 30f);
				}
				if (homeInIcon != null)
				{
					homeInIcon.SetFlashValue((0.5f + 0.5f * Mathf.Sin((float)counter / 8f)) * homeIn);
					homeInIcon.setDisplace += Vector2.ClampMagnitude(pos - homeInIcon.pos, 30f) / 15f * homeIn;
					homePos = Vector2.Lerp(homePos, homeInIcon.DrawPos(1f), homeIn * (1f - lingerFac));
					if (!menuMode && input[0].thrw && !input[1].thrw && homeInIcon.DraggedBy == null)
					{
						dragIcon = homeInIcon;
						flag = true;
						room.PlaySound(SoundID.SANDBOX_Grab_Item, pos, 1f, 1f);
						if (mouseMode)
						{
							dragOffset = dragIcon.pos - pos;
							homeIn = 1f;
						}
						else
						{
							dragOffset *= 0f;
							pos = DrawPos(1f);
							homeIn = 0f;
							homePos = pos;
						}
					}
				}
				else
				{
					homePos = Vector2.Lerp(pos, homePos, Mathf.Max(homeIn, lingerFac));
				}
			}
			lingerFac = Mathf.Max(0f, lingerFac - 0.05f);
			if (!menuMode && !flag && ((input[0].thrw && !input[1].thrw) || (input[0].jmp && !input[1].jmp && !menuMode)))
			{
				room.PlaySound(SoundID.SANDBOX_Nothing_Click, pos, 1f, 1f);
			}
		}

		public void SpawnObject(IconSymbol.IconSymbolData iconData, EntityID ID)
		{
			Bump(redBump: false);
			Vector2 vector = pos;
			for (int i = 0; i < 10; i++)
			{
				bool flag = true;
				for (int j = 0; j < editor.icons.Count; j++)
				{
					if (Custom.DistLess(vector, editor.icons[j].pos, 5f))
					{
						flag = false;
						vector = editor.icons[j].pos + Custom.DirVec(editor.icons[j].pos, vector + new Vector2(1f, 1f)) * 5f;
					}
				}
				if (flag)
				{
					break;
				}
			}
			pos = vector;
			PlacedIcon placedIcon = editor.AddIcon(iconData, vector, ID, fadeCircle: true, updatePerfEstimate: true);
			room.PlaySound(SoundID.SANDBOX_Add_Item, pos, 1f, 1f);
			if (dragIcon != null)
			{
				dragIcon = placedIcon;
			}
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[10];
			sLeaser.sprites[0] = new FSprite("Futile_White");
			sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["FlatLight"];
			sLeaser.sprites[0].color = Color.black;
			sLeaser.sprites[1] = new FSprite("Futile_White");
			sLeaser.sprites[1].shader = rCam.game.rainWorld.Shaders["LightSource"];
			for (int i = 0; i < 8; i++)
			{
				sLeaser.sprites[2 + i] = new FSprite("pixel");
				sLeaser.sprites[2 + i].scaleX = 2f;
				sLeaser.sprites[2 + i].scaleY = 15f;
				sLeaser.sprites[2 + i].anchorY = 0f;
				sLeaser.sprites[2 + i].shader = rCam.game.rainWorld.Shaders["Hologram"];
			}
			AddToContainer(sLeaser, rCam, null);
		}

		public Vector2 DrawPos(float timeStacker)
		{
			return Vector2.Lerp(Vector2.Lerp(lastPos, pos, timeStacker), Vector2.Lerp(lastHomePos, homePos, timeStacker), Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(lastHomeIn, homeIn, timeStacker)), 2f) * 0.95f);
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = OverseerEyePos(timeStacker);
			Vector2 vector2 = Vector2.Lerp(lastPushAroundPos, pushAroundPos, timeStacker);
			float num = Mathf.Pow(1f - Mathf.Lerp(lastQuality, quality, timeStacker), 1.5f);
			Vector2 vector3 = DrawPos(timeStacker) + vector2 * Mathf.Lerp(0.5f, 1f, num);
			float a = 0.5f + 0.5f * Mathf.Sin(((float)counter + timeStacker) / 8f);
			a = Mathf.Lerp(a, UnityEngine.Random.value, num * 0.2f);
			float a2 = Mathf.InverseLerp(0.25f, 0.75f, Mathf.Lerp(lastSquare, square, timeStacker));
			float num2 = Mathf.Lerp(lastMobile, mobile, timeStacker);
			float num3 = Mathf.Lerp(lastMenuFac, menuFac, timeStacker);
			a2 = Mathf.Max(a2, num3);
			float num4 = Mathf.Lerp(lastBump, bump, timeStacker);
			float num5 = Mathf.Lerp(lastDragIcon ? 1f : 0f, (dragIcon != null) ? 1f : 0f, timeStacker);
			float num6 = 20f + num5 * (Mathf.Lerp(-10f, -5f, a * (1f - num2)) + 10f * num3);
			num6 += num4 * 10f;
			if (input[0].thrw || (input[0].jmp && !menuMode))
			{
				num6 -= 4f;
			}
			float num7 = GetRotation(timeStacker) * 360f + 45f * Mathf.Lerp(lastSquare, square, timeStacker) + 45f * num5;
			float num8 = Mathf.Lerp(Mathf.Lerp(45f, 75f, num5), 90f, num3);
			num6 -= 10f * num3;
			Color color = PlayerGraphics.DefaultSlugcatColor(SlugcatStats.Name.ArenaColor(playerNumber));
			if (playerNumber == 3)
			{
				color = Color.Lerp(Custom.Saturate(PlayerGraphics.DefaultSlugcatColor(SlugcatStats.Name.ArenaColor(playerNumber)), 0.5f), Color.white, 0.2f);
			}
			if (input[0].jmp && !menuMode)
			{
				color = Color.Lerp(color, Color.red, UnityEngine.Random.value);
			}
			else if (red)
			{
				color = Color.Lerp(color, Color.red, num4);
			}
			sLeaser.sprites[0].x = vector3.x - camPos.x;
			sLeaser.sprites[0].y = vector3.y - camPos.y;
			sLeaser.sprites[0].scale = (150f + num6 * (1f - UnityEngine.Random.value * num)) / 8f;
			sLeaser.sprites[0].alpha = 0.3f * (1f - UnityEngine.Random.value * num);
			sLeaser.sprites[1].x = vector3.x - camPos.x;
			sLeaser.sprites[1].y = vector3.y - camPos.y;
			sLeaser.sprites[1].scale = (150f + num6 * (1f - UnityEngine.Random.value * num) * 2f) / 8f;
			sLeaser.sprites[1].alpha = 0.3f * (1f - UnityEngine.Random.value * num);
			sLeaser.sprites[1].color = color;
			float num9 = Vector2.Distance(vector, vector3);
			for (int i = 0; i < 8; i++)
			{
				float num10 = Mathf.InverseLerp(0f, 4f, i / 2) * 360f + num7;
				Vector2 vector4 = vector3 + Custom.DegToVec(num10) * (num6 + 15f);
				Vector2 vector5 = vector3 + Vector2.Lerp(Custom.DegToVec(num10) * num6, Custom.DegToVec(num10) * (num6 + 15f) - Custom.DegToVec(num10 + ((i % 2 == 0) ? (-1f) : 1f) * num8) * Mathf.Lerp(10f, 8f, num5), a2);
				vector4 += pushAroundPos * (0.5f * Mathf.Pow(Mathf.InverseLerp(num9 + 40f, num9 - 40f, Vector2.Distance(vector, vector4)), 2f) + 0.5f * UnityEngine.Random.value * num);
				vector5 += pushAroundPos * (0.5f * Mathf.Pow(Mathf.InverseLerp(num9 + 40f, num9 - 40f, Vector2.Distance(vector, vector5)), 2f) + 0.5f * UnityEngine.Random.value * num);
				if (UnityEngine.Random.value < num)
				{
					if (UnityEngine.Random.value < 0.5f)
					{
						vector4 += Custom.RNV() * UnityEngine.Random.value * 20f * num;
					}
					else
					{
						vector5 += Custom.RNV() * UnityEngine.Random.value * 20f * num;
					}
				}
				if (UnityEngine.Random.value < 1f / Mathf.Lerp(40f, 10f, num))
				{
					sLeaser.sprites[2 + i].scaleX = 1f;
					sLeaser.sprites[2 + i].alpha = Mathf.Pow(UnityEngine.Random.value, Mathf.Lerp(2f, 0.2f, num));
					if (UnityEngine.Random.value < 0.5f)
					{
						vector4 = Vector2.Lerp(vector5, vector, UnityEngine.Random.value * UnityEngine.Random.value);
					}
					else
					{
						vector5 = Vector2.Lerp(vector4, vector, UnityEngine.Random.value * UnityEngine.Random.value);
					}
				}
				else
				{
					sLeaser.sprites[2 + i].scaleX = 2f + num4;
					sLeaser.sprites[2 + i].alpha = ((UnityEngine.Random.value < num) ? (1f - UnityEngine.Random.value * num) : 1f);
				}
				sLeaser.sprites[2 + i].x = vector4.x - camPos.x;
				sLeaser.sprites[2 + i].y = vector4.y - camPos.y;
				sLeaser.sprites[2 + i].rotation = Custom.AimFromOneVectorToAnother(vector4, vector5);
				sLeaser.sprites[2 + i].scaleY = Vector2.Distance(vector4, vector5);
				sLeaser.sprites[2 + i].color = color;
			}
			if (base.slatedForDeletetion || room != rCam.room)
			{
				sLeaser.CleanSpritesAndRemove();
			}
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
		}

		public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			if (newContatiner == null)
			{
				newContatiner = rCam.ReturnFContainer("HUD2");
			}
			for (int i = 0; i < sLeaser.sprites.Length; i++)
			{
				sLeaser.sprites[i].RemoveFromContainer();
				if (i < 2)
				{
					rCam.ReturnFContainer("Bloom").AddChild(sLeaser.sprites[i]);
				}
				else
				{
					newContatiner.AddChild(sLeaser.sprites[i]);
				}
			}
		}
	}

	public abstract class PlacedIcon : UpdatableAndDeletable, IDrawable
	{
		public SandboxEditor editor;

		public Vector2 pos;

		public Vector2 lastPos;

		private Vector2 lastDisplace;

		private Vector2 displace;

		public Vector2 setDisplace;

		protected float flash;

		protected float lastFlash;

		protected float setFlash;

		protected bool fadeOut;

		public float fade;

		public float lastFade;

		public EditCursor DraggedBy
		{
			get
			{
				for (int i = 0; i < editor.cursors.Count; i++)
				{
					if (editor.cursors[i].dragIcon == this)
					{
						return editor.cursors[i];
					}
				}
				return null;
			}
		}

		public virtual float GlowRad(float timeStacker)
		{
			return 16f;
		}

		public virtual float GlowAlpha(float timeStacker)
		{
			return 1f;
		}

		public virtual Color GlowColor(float timeStacker)
		{
			return Color.white;
		}

		public PlacedIcon(SandboxEditor editor, Vector2 initPos)
		{
			this.editor = editor;
			pos = initPos;
			lastPos = initPos;
		}

		public virtual void Fade()
		{
			fadeOut = true;
		}

		public virtual void Flash()
		{
		}

		public void SetFlashValue(float f)
		{
			setFlash = Mathf.Max(setFlash, f);
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			lastPos = pos;
			lastDisplace = displace;
			lastFlash = flash;
			lastFade = fade;
			displace *= 0.8f;
			if (setDisplace.x != 0f || setDisplace.y != 0f)
			{
				displace = setDisplace;
			}
			setDisplace *= 0f;
			if (setFlash > 0f)
			{
				flash = setFlash;
				setFlash = 0f;
			}
			else
			{
				flash = Custom.LerpAndTick(flash, 0f, 0.08f, 0.1f);
			}
			if (fadeOut)
			{
				fade = Custom.LerpAndTick(fade, 0f, 0.02f, 0.025f);
				if (fade == 0f && lastFade == 0f)
				{
					Destroy();
				}
			}
			else
			{
				fade = Custom.LerpAndTick(fade, 1f, 0.02f, 0.05f);
			}
			if (room.cameraPositions.Length == 1)
			{
				pos.x = Mathf.Clamp(pos.x, room.game.cameras[0].pos.x, room.game.cameras[0].pos.x + room.game.cameras[0].sSize.x);
				pos.y = Mathf.Clamp(pos.y, room.game.cameras[0].pos.y, room.game.cameras[0].pos.y + room.game.cameras[0].sSize.y);
			}
		}

		public void InitiateLightSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites[0] = new FSprite("Futile_White");
			sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["FlatLight"];
			sLeaser.sprites[0].color = Color.black;
			sLeaser.sprites[1] = new FSprite("Futile_White");
			sLeaser.sprites[1].shader = rCam.game.rainWorld.Shaders["FlatLight"];
		}

		public virtual void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
		}

		public Vector2 DrawPos(float timeStacker)
		{
			return Vector2.Lerp(lastPos, pos, timeStacker) + Vector2.Lerp(lastDisplace, displace, timeStacker);
		}

		public virtual void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = DrawPos(timeStacker);
			float num = Mathf.Lerp(lastFade, fade, timeStacker);
			for (int i = 0; i < 2; i++)
			{
				sLeaser.sprites[i].x = vector.x - camPos.x;
				sLeaser.sprites[i].y = vector.y - camPos.y;
			}
			sLeaser.sprites[0].scale = Mathf.Lerp(5f, 10f, num);
			sLeaser.sprites[0].alpha = 0.8f * num;
			sLeaser.sprites[1].scale = GlowRad(1f) / 8f;
			sLeaser.sprites[1].alpha = GlowAlpha(1f) * num;
			sLeaser.sprites[1].color = GlowColor(1f);
			if (base.slatedForDeletetion || room != rCam.room)
			{
				sLeaser.CleanSpritesAndRemove();
			}
		}

		public virtual void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
		}

		public virtual void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			if (newContatiner == null)
			{
				newContatiner = rCam.ReturnFContainer("Bloom");
			}
			FSprite[] sprites = sLeaser.sprites;
			foreach (FSprite fSprite in sprites)
			{
				fSprite.RemoveFromContainer();
				newContatiner.AddChild(fSprite);
			}
		}
	}

	public abstract class CreatureOrItemIcon : PlacedIcon
	{
		private IconSymbol symbol;

		public IconSymbol.IconSymbolData iconData;

		public EntityID ID;

		public float move;

		public Vector2 moveToPoint;

		public virtual bool AllowedToMove
		{
			get
			{
				if (base.DraggedBy == null)
				{
					return !fadeOut;
				}
				return false;
			}
		}

		public override float GlowAlpha(float timeStacker)
		{
			return 0.5f;
		}

		public override float GlowRad(float timeStacker)
		{
			if (symbol == null)
			{
				return 8f;
			}
			return Mathf.Max(8f, symbol.graphWidth);
		}

		public override Color GlowColor(float timeStacker)
		{
			if (symbol == null)
			{
				return base.GlowColor(timeStacker);
			}
			return Color.Lerp(symbol.myColor, new Color(1f, 1f, 1f), Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(symbol.lastShowFlash, symbol.showFlash, timeStacker)), 3f));
		}

		public CreatureOrItemIcon(SandboxEditor editor, Vector2 initPos, IconSymbol.IconSymbolData iconData, EntityID ID)
			: base(editor, initPos)
		{
			this.iconData = iconData;
			this.ID = ID;
		}

		public override void Flash()
		{
			if (symbol != null)
			{
				symbol.showFlash = 1f;
			}
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			if (symbol != null)
			{
				symbol.Update();
				symbol.showFlash = Mathf.Max(symbol.showFlash, flash);
			}
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[3];
			sLeaser.sprites[2] = new FSprite("Futile_White");
			sLeaser.sprites[2].alpha = 0.5f;
			InitiateLightSprites(sLeaser, rCam);
			AddToContainer(sLeaser, rCam, null);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			Vector2 vector = DrawPos(timeStacker);
			symbol.Draw(timeStacker, vector - camPos);
			float alpha = Mathf.Lerp(lastFade, fade, timeStacker);
			symbol.symbolSprite.alpha = alpha;
			symbol.shadowSprite1.alpha = alpha;
			symbol.shadowSprite2.alpha = alpha;
			sLeaser.sprites[2].color = GlowColor(timeStacker);
			sLeaser.sprites[2].x = vector.x - camPos.x;
			sLeaser.sprites[2].y = vector.y - camPos.y;
			sLeaser.sprites[2].alpha = alpha;
			if (base.slatedForDeletetion || room != rCam.room)
			{
				if (symbol != null)
				{
					symbol.RemoveSprites();
				}
				symbol = null;
			}
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
		}

		public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
			base.ApplyPalette(sLeaser, rCam, palette);
		}

		public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			base.AddToContainer(sLeaser, rCam, newContatiner);
			if (symbol != null)
			{
				symbol.RemoveSprites();
			}
			symbol = IconSymbol.CreateIconSymbol(iconData, rCam.ReturnFContainer("HUD"));
			if (iconData.critType == CreatureTemplate.Type.Slugcat)
			{
				symbol.myColor = global::Menu.Menu.MenuRGB(global::Menu.Menu.MenuColors.MediumGrey);
			}
			sLeaser.sprites[2].element = Futile.atlasManager.GetElementWithName(symbol.spriteName);
			symbol.Show(showShadowSprites: true);
			symbol.symbolSprite.shader = rCam.game.rainWorld.Shaders["Hologram"];
			symbol.shadowSprite1.shader = rCam.game.rainWorld.Shaders["Hologram"];
			symbol.shadowSprite2.shader = rCam.game.rainWorld.Shaders["Hologram"];
		}
	}

	public class StayOutOfTerrainIcon : CreatureOrItemIcon
	{
		private float checkDistance;

		private bool foundPoint;

		private int counter;

		public override Color GlowColor(float timeStacker)
		{
			if (!AllowedTile(pos) && base.DraggedBy != null)
			{
				float num = 0.5f + 0.5f * Mathf.Sin(((float)counter + timeStacker) / 30f * (float)Math.PI * 2f);
				return Color.Lerp(Color.Lerp(base.GlowColor(timeStacker), global::Menu.Menu.MenuRGB(global::Menu.Menu.MenuColors.MediumGrey), Mathf.Pow(num, 0.5f)), Color.red, num * 0.5f);
			}
			return base.GlowColor(timeStacker);
		}

		public bool AllowedTile(Vector2 tst)
		{
			if (room == null || !room.readyForAI)
			{
				return true;
			}
			if (room.GetTile(tst).Terrain != 0 && room.GetTile(tst).Terrain != Room.Tile.TerrainType.Floor)
			{
				return false;
			}
			if (iconData.critType == CreatureTemplate.Type.Vulture || iconData.critType == CreatureTemplate.Type.KingVulture || iconData.critType == CreatureTemplate.Type.BrotherLongLegs || iconData.critType == CreatureTemplate.Type.DaddyLongLegs || iconData.critType == CreatureTemplate.Type.MirosBird)
			{
				return room.aimap.getTerrainProximity(tst) > 1;
			}
			if (iconData.critType == CreatureTemplate.Type.Deer)
			{
				return room.aimap.getTerrainProximity(tst) > 3;
			}
			if (iconData.critType == CreatureTemplate.Type.BigEel)
			{
				return room.aimap.getTerrainProximity(tst) > 4;
			}
			return true;
		}

		public StayOutOfTerrainIcon(SandboxEditor editor, Vector2 initPos, IconSymbol.IconSymbolData iconData, EntityID ID)
			: base(editor, initPos, iconData, ID)
		{
			moveToPoint = initPos;
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			counter++;
			if (!AllowedToMove)
			{
				foundPoint = false;
				checkDistance = 20f;
				moveToPoint = pos;
			}
			else if (!foundPoint && !AllowedTile(pos))
			{
				checkDistance += 4f;
				float dst = float.MaxValue;
				for (int i = 0; i < 20; i++)
				{
					Vector2 vector = pos + Custom.DegToVec((float)i * 18f) * checkDistance;
					if (vector.x > 0f && vector.x < room.PixelWidth && vector.y > 0f && vector.y < room.PixelHeight && AllowedTile(vector) && Custom.DistLess(pos, vector, dst))
					{
						dst = Vector2.Distance(pos, vector);
						moveToPoint = Vector2.Lerp(vector, room.MiddleOfTile(vector), 0.6f);
						foundPoint = true;
					}
				}
			}
			if (AllowedToMove && foundPoint && !Custom.DistLess(pos, moveToPoint, 1f))
			{
				move = Custom.LerpAndTick(move, 1f, 0.03f, 1f / 120f);
				pos = Custom.MoveTowards(pos, moveToPoint, 8f * move * Mathf.InverseLerp(0f, 30f, Vector2.Distance(pos, moveToPoint)));
			}
			else
			{
				move = Custom.LerpAndTick(move, 0f, 0.03f, 1f / 30f);
			}
		}
	}

	public class InDenCreatureIcon : CreatureOrItemIcon
	{
		public int den = -1;

		private AbstractRoomNode.Type searchType;

		public override bool AllowedToMove
		{
			get
			{
				if (den >= 0)
				{
					return base.AllowedToMove;
				}
				return false;
			}
		}

		public InDenCreatureIcon(SandboxEditor editor, Vector2 initPos, IconSymbol.IconSymbolData iconData, EntityID ID)
			: base(editor, initPos, iconData, ID)
		{
			searchType = AbstractRoomNode.Type.Den;
			if (iconData.critType == CreatureTemplate.Type.Slugcat)
			{
				searchType = AbstractRoomNode.Type.Exit;
			}
			FindDen();
		}

		public void FindDen()
		{
			float dst = float.MaxValue;
			den = -1;
			for (int i = 0; i < editor.room.abstractRoom.nodes.Length; i++)
			{
				if (editor.room.abstractRoom.nodes[i].type == searchType)
				{
					Vector2 vector = editor.room.MiddleOfTile(editor.room.ShortcutLeadingToNode(i).StartTile);
					if (Custom.DistLess(pos, vector, dst))
					{
						dst = Vector2.Distance(pos, vector);
						den = i;
						iconData.intData = den;
					}
				}
			}
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			FindDen();
			if (den > -1)
			{
				moveToPoint = editor.room.MiddleOfTile(editor.room.ShortcutLeadingToNode(den).StartTile);
				if (AllowedToMove)
				{
					move = Custom.LerpAndTick(move, 1f, 0.03f, 1f / 120f);
					pos = Custom.MoveTowards(pos, moveToPoint, 8f * move * Mathf.InverseLerp(30f, 60f, Vector2.Distance(pos, moveToPoint)));
				}
				else
				{
					move = Custom.LerpAndTick(move, 0f, 0.03f, 1f / 30f);
				}
			}
			else
			{
				moveToPoint = pos;
			}
		}

		public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[4];
			sLeaser.sprites[2] = new FSprite("Futile_White");
			sLeaser.sprites[2].alpha = 0.5f;
			sLeaser.sprites[3] = new FSprite("pixel");
			sLeaser.sprites[3].anchorY = 0f;
			sLeaser.sprites[3].shader = rCam.game.rainWorld.Shaders["Hologram"];
			InitiateLightSprites(sLeaser, rCam);
			AddToContainer(sLeaser, rCam, null);
		}

		public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
			Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);
			if (den < 0)
			{
				sLeaser.sprites[3].isVisible = false;
				return;
			}
			Vector2 vector2 = editor.room.MiddleOfTile(editor.room.ShortcutLeadingToNode(den).StartTile);
			if (Custom.DistLess(vector, vector2, 30f))
			{
				sLeaser.sprites[3].isVisible = false;
				return;
			}
			sLeaser.sprites[3].isVisible = true;
			sLeaser.sprites[3].x = vector.x + Custom.DirVec(vector, vector2).x * 20f - camPos.x;
			sLeaser.sprites[3].y = vector.y + Custom.DirVec(vector, vector2).y * 20f - camPos.y;
			sLeaser.sprites[3].rotation = Custom.AimFromOneVectorToAnother(vector, vector2);
			sLeaser.sprites[3].scaleY = Vector2.Distance(vector, vector2) - 20f;
			sLeaser.sprites[3].color = GlowColor(timeStacker);
			sLeaser.sprites[3].alpha = Mathf.Lerp(0.4f, 0.9f, UnityEngine.Random.value) * Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(lastFade, fade, timeStacker)), 2f);
		}
	}

	public class PlacedIconData
	{
		public Vector2 pos;

		public IconSymbol.IconSymbolData data;

		public EntityID ID;

		public PlacedIconData(Vector2 pos, IconSymbol.IconSymbolData data, EntityID ID)
		{
			this.pos = pos;
			this.data = data;
			this.ID = ID;
		}
	}

	private static readonly AGLog<SandboxEditor> Log = new AGLog<SandboxEditor>();

	public const string SANDBOX_KEY_FORMAT = "{0}_Sandbox";

	private SandboxGameSession sandboxSession;

	public List<EditCursor> cursors;

	public List<PlacedIcon> icons;

	public int currentConfig = -1;

	public int performanceWarning;

	public SandboxOverlay overlay => sandboxSession.overlay;

	public SandboxEditor(SandboxGameSession sandboxSession)
		: base(sandboxSession)
	{
		this.sandboxSession = sandboxSession;
		cursors = new List<EditCursor>();
		icons = new List<PlacedIcon>();
		bool flag = false;
		if (sandboxSession.arenaSitting.players.Count < 1)
		{
			flag = true;
			sandboxSession.arenaSitting.players.Add(new ArenaSitting.ArenaPlayer(0));
		}
		for (int i = 0; i < sandboxSession.arenaSitting.players.Count; i++)
		{
			Vector2 initPos = new Vector2(base.room.PixelWidth / 2f, base.room.PixelHeight / 2f);
			if (sandboxSession.arenaSitting.players.Count > 1)
			{
				initPos += Custom.DegToVec((float)i / (float)sandboxSession.arenaSitting.players.Count * 360f + ((sandboxSession.arenaSitting.players.Count == 2) ? 90f : 0f)) * 50f;
			}
			OverseerAbstractAI overseerAbstractAI = null;
			for (int j = 0; j < base.room.abstractRoom.creatures.Count; j++)
			{
				if (base.room.abstractRoom.creatures[j].creatureTemplate.type == CreatureTemplate.Type.Overseer && (base.room.abstractRoom.creatures[j].abstractAI as OverseerAbstractAI).ownerIterator == sandboxSession.arenaSitting.players[i].playerNumber + 10)
				{
					overseerAbstractAI = base.room.abstractRoom.creatures[j].abstractAI as OverseerAbstractAI;
					break;
				}
			}
			if (overseerAbstractAI != null)
			{
				EditCursor editCursor = new EditCursor(this, overseerAbstractAI, sandboxSession.arenaSitting.players[i].playerNumber, initPos);
				cursors.Add(editCursor);
				base.room.AddObject(editCursor);
			}
		}
		if (flag)
		{
			sandboxSession.arenaSitting.players.Clear();
		}
		LoadConfig();
	}

	public override void Update()
	{
		base.Update();
		for (int num = icons.Count - 1; num >= 0; num--)
		{
			if (icons[num].slatedForDeletetion)
			{
				icons.RemoveAt(num);
			}
		}
	}

	public PlacedIcon AddIcon(IconSymbol.IconSymbolData iconData, Vector2 pos, EntityID ID, bool fadeCircle, bool updatePerfEstimate)
	{
		CreatureOrItemIcon icon = ((!(iconData.itemType == AbstractPhysicalObject.AbstractObjectType.Creature) || (!(iconData.critType == CreatureTemplate.Type.Slugcat) && !(iconData.critType == CreatureTemplate.Type.TentaclePlant) && !(iconData.critType == CreatureTemplate.Type.PoleMimic) && (!ModManager.MSC || !(iconData.critType == MoreSlugcatsEnums.CreatureTemplateType.StowawayBug)))) ? ((CreatureOrItemIcon)new StayOutOfTerrainIcon(this, pos, iconData, ID)) : ((CreatureOrItemIcon)new InDenCreatureIcon(this, pos, iconData, ID)));
		return AddIcon(icon, fadeCircle, updatePerfEstimate);
	}

	public PlacedIcon AddIcon(PlacedIcon icon, bool fadeCircle, bool updatePerfEstimate)
	{
		icons.Add(icon);
		base.room.AddObject(icon);
		if (fadeCircle)
		{
			base.room.game.cameras[0].hud.fadeCircles.Add(new FadeCircle(base.room.game.cameras[0].hud, 10f, 10f, 0.82f, 30f, 4f, icon.pos - base.room.game.cameras[0].pos, base.room.game.cameras[0].hud.fContainers[1]));
		}
		if (updatePerfEstimate)
		{
			UpdatePerformanceEstimate();
		}
		return icon;
	}

	public void RemoveIcon(PlacedIcon icon, bool updatePerfEstimate)
	{
		base.room.game.cameras[0].hud.fadeCircles.Add(new FadeCircle(base.room.game.cameras[0].hud, 10f, 5f, 0.82f, 15f, 4f, icon.pos - base.room.game.cameras[0].pos, base.room.game.cameras[0].hud.fContainers[1]));
		icon.Flash();
		icon.Fade();
		icons.Remove(icon);
		if (updatePerfEstimate)
		{
			UpdatePerformanceEstimate();
		}
	}

	public void ClearAll()
	{
		for (int i = 0; i < icons.Count; i++)
		{
			icons[i].Flash();
			icons[i].Fade();
		}
		icons.Clear();
	}

	public void SwitchConfig(int newConfig)
	{
		if (currentConfig != newConfig)
		{
			SaveConfig(currentConfig, newConfig);
			currentConfig = newConfig;
			LoadConfig();
		}
	}

	public void UpdatePerformanceEstimate()
	{
		float exponentialPart = 0f;
		float linearPart = 0f;
		for (int i = 0; i < icons.Count; i++)
		{
			GetPerformanceEstimate(icons[i], ref exponentialPart, ref linearPart);
		}
		float num = exponentialPart * (exponentialPart + linearPart * 0.2f) + Mathf.Pow(linearPart, 1.12f);
		if (num > 220f)
		{
			performanceWarning = 2;
		}
		else if (num > 80f)
		{
			performanceWarning = 1;
		}
		else if (num < 75f)
		{
			performanceWarning = 0;
		}
	}

	private static void GetPerformanceEstimate(PlacedIcon placedIcon, ref float exponentialPart, ref float linearPart)
	{
		if (!(placedIcon is CreatureOrItemIcon))
		{
			linearPart += 0.2f;
			return;
		}
		IconSymbol.IconSymbolData iconData = (placedIcon as CreatureOrItemIcon).iconData;
		if (iconData.itemType == AbstractPhysicalObject.AbstractObjectType.Creature)
		{
			CreaturePerfEstimate(iconData.critType, ref linearPart, ref exponentialPart);
		}
		else if (iconData.itemType == AbstractPhysicalObject.AbstractObjectType.Spear)
		{
			if (iconData.intData == 1)
			{
				linearPart += 0.4f;
				exponentialPart += 0.1f;
			}
			else
			{
				linearPart += 0.25f;
			}
		}
		else if (iconData.itemType == AbstractPhysicalObject.AbstractObjectType.ScavengerBomb)
		{
			linearPart += 1.2f;
		}
		else if (iconData.itemType == AbstractPhysicalObject.AbstractObjectType.SporePlant)
		{
			linearPart += 0.4f;
			exponentialPart += 0.3f;
		}
		else if (iconData.itemType == AbstractPhysicalObject.AbstractObjectType.JellyFish)
		{
			linearPart += 1f;
			exponentialPart += 0.1f;
		}
		else
		{
			linearPart += 0.2f;
		}
	}

	public void Play()
	{
		if (!(base.game.manager.upcomingProcess != null))
		{
			SaveConfig(currentConfig, currentConfig);
			gameSession.arenaSitting.sandboxPlayMode = true;
			base.game.manager.RequestMainProcessSwitch(ProcessManager.ProcessID.Game);
		}
	}

	private static string FilePath(string room)
	{
		return Custom.RootFolderDirectory() + (Path.DirectorySeparatorChar + "UserData" + Path.DirectorySeparatorChar + "Sandbox" + Path.DirectorySeparatorChar + MultiplayerUnlocks.LevelDisplayName(room) + "_Sandbox.txt").ToLowerInvariant();
	}

	public void SaveConfig()
	{
		if (currentConfig >= 0)
		{
			SaveConfig(currentConfig, currentConfig);
		}
	}

	private string GetSandboxString(int configNumber, int nextSelected)
	{
		if (configNumber < 0 || nextSelected < 0)
		{
			return null;
		}
		string text = string.Format(CultureInfo.InvariantCulture, "SELECTED<sbB>{0}<sbA>", nextSelected);
		if (icons.Count > 0)
		{
			text += string.Format(CultureInfo.InvariantCulture, "CONFIG<sbB>{0}<sbB>", configNumber);
			for (int i = 0; i < icons.Count; i++)
			{
				if (icons[i] is CreatureOrItemIcon)
				{
					text += string.Format(CultureInfo.InvariantCulture, "{0}<sbC>{1}<sbC>{2}<sbC>{3}<sbB>", icons[i].pos.x, icons[i].pos.y, (icons[i] as CreatureOrItemIcon).iconData, (icons[i] as CreatureOrItemIcon).ID);
				}
			}
			text += "<sbA>";
		}
		string[] array = new string[0];
		string text2 = sandboxSession.game.manager.rainWorld.options.LoadSandbox(base.room.abstractRoom.name, FilePath(base.room.abstractRoom.name));
		if (text2 != null)
		{
			array = Regex.Split(text2, "<sbA>");
		}
		for (int j = 0; j < array.Length; j++)
		{
			if (array[j].Length > 0)
			{
				string[] array2 = Regex.Split(array[j], "<sbB>");
				if (array2.Length >= 2 && array2[0] == "CONFIG" && int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture) != configNumber)
				{
					text = text + array[j] + "<sbA>";
				}
			}
		}
		return text;
	}

	private void SaveConfig(int configNumber, int nextSelected)
	{
		string sandboxString = GetSandboxString(configNumber, nextSelected);
		if (sandboxString != null)
		{
			sandboxSession.game.manager.rainWorld.options.SaveSandbox(base.room.abstractRoom.name, sandboxString);
		}
	}

	public void DevToolsExportConfig()
	{
		string sandboxString = GetSandboxString(currentConfig, currentConfig);
		if (sandboxString != null)
		{
			string text = Custom.LegacyRootFolderDirectory() + Path.DirectorySeparatorChar + "Sandbox";
			if (!Directory.Exists(text))
			{
				Directory.CreateDirectory(text);
			}
			File.WriteAllText(text + Path.DirectorySeparatorChar + base.room.abstractRoom.name + "_Sandbox.txt", sandboxString);
		}
	}

	private void LoadConfig()
	{
		ClearAll();
		List<PlacedIconData> list = LoadConfiguration(ref currentConfig, base.room.abstractRoom.name, sandboxSession.game.manager.rainWorld);
		for (int i = 0; i < list.Count; i++)
		{
			AddIcon(list[i].data, list[i].pos, list[i].ID, fadeCircle: false, updatePerfEstimate: false);
		}
		UpdatePerformanceEstimate();
	}

	public static List<PlacedIconData> LoadConfiguration(ref int currConfNumber, string room, RainWorld rainWorld)
	{
		List<PlacedIconData> list = new List<PlacedIconData>();
		string text = rainWorld.options.LoadSandbox(room, FilePath(room));
		if (text == null)
		{
			currConfNumber = 0;
			return list;
		}
		string[] array = Regex.Split(text, "<sbA>");
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i].Length <= 0)
			{
				continue;
			}
			string[] array2 = Regex.Split(array[i], "<sbB>");
			if (array2[0] == "SELECTED")
			{
				if (currConfNumber == -1)
				{
					Custom.Log("setting undefined config to :", currConfNumber.ToString());
					currConfNumber = int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture);
					return LoadConfiguration(ref currConfNumber, room, rainWorld);
				}
			}
			else
			{
				if (!(array2[0] == "CONFIG") || int.Parse(array2[1], NumberStyles.Any, CultureInfo.InvariantCulture) != currConfNumber)
				{
					continue;
				}
				for (int j = 2; j < array2.Length; j++)
				{
					string[] array3 = Regex.Split(array2[j], "<sbC>");
					if (array3.Length >= 3)
					{
						Vector2 pos = new Vector2(float.Parse(array3[0], NumberStyles.Any, CultureInfo.InvariantCulture), float.Parse(array3[1], NumberStyles.Any, CultureInfo.InvariantCulture));
						IconSymbol.IconSymbolData data = IconSymbol.IconSymbolData.IconSymbolDataFromString(array3[2]);
						if (!(data.critType == null) && data.critType.Index != -1 && !(data.itemType == null) && data.itemType.Index != -1)
						{
							EntityID iD = EntityID.FromString(array3[3]);
							list.Add(new PlacedIconData(pos, data, iD));
						}
					}
				}
			}
		}
		return list;
	}

	public static void CreaturePerfEstimate(CreatureTemplate.Type critType, ref float linear, ref float exponential)
	{
		if (critType == CreatureTemplate.Type.PinkLizard || critType == CreatureTemplate.Type.GreenLizard || critType == CreatureTemplate.Type.BlueLizard || critType == CreatureTemplate.Type.WhiteLizard || critType == CreatureTemplate.Type.BlackLizard || critType == CreatureTemplate.Type.Salamander)
		{
			linear += 0.5f;
			exponential += 0.5f;
		}
		else if (critType == CreatureTemplate.Type.YellowLizard)
		{
			linear += 0.5f;
			exponential += 0.6f;
		}
		else if (critType == CreatureTemplate.Type.RedLizard)
		{
			linear += 0.75f;
			exponential += 1.15f;
		}
		else if (critType == CreatureTemplate.Type.CyanLizard)
		{
			linear += 0.6f;
			exponential += 0.5f;
		}
		else if (critType == CreatureTemplate.Type.Fly)
		{
			linear += 0.5f;
			exponential += 0.1f;
		}
		else if (critType == CreatureTemplate.Type.Leech || critType == CreatureTemplate.Type.SeaLeech)
		{
			linear += 0.3f;
			exponential += 0.1f;
		}
		else if (critType == CreatureTemplate.Type.Snail)
		{
			linear += 0.2f;
			exponential += 0.3f;
		}
		else if (critType == CreatureTemplate.Type.Vulture)
		{
			linear += 1.1f;
			exponential += 0.65f;
		}
		else if (critType == CreatureTemplate.Type.GarbageWorm)
		{
			linear += 0.4f;
			exponential += 0.1f;
		}
		else if (critType == CreatureTemplate.Type.LanternMouse)
		{
			linear += 0.4f;
			exponential += 0.3f;
		}
		else if (critType == CreatureTemplate.Type.CicadaA || critType == CreatureTemplate.Type.CicadaB)
		{
			linear += 0.4f;
			exponential += 0.5f;
		}
		else if (critType == CreatureTemplate.Type.Spider)
		{
			linear += 0.1f;
			exponential += 0.5f;
		}
		else if (critType == CreatureTemplate.Type.JetFish)
		{
			linear += 0.4f;
			exponential += 0.5f;
		}
		else if (critType == CreatureTemplate.Type.BigEel)
		{
			linear += 4f;
			exponential += 1.2f;
		}
		else if (critType == CreatureTemplate.Type.Deer)
		{
			linear += 2.4f;
			exponential += 1.2f;
		}
		else if (critType == CreatureTemplate.Type.TubeWorm)
		{
			linear += 0.3f;
			exponential += 0.3f;
		}
		else if (critType == CreatureTemplate.Type.DaddyLongLegs)
		{
			linear += 3f;
			exponential += 1.5f;
		}
		else if (critType == CreatureTemplate.Type.BrotherLongLegs)
		{
			linear += 2.1f;
			exponential += 1.25f;
		}
		else if (critType == CreatureTemplate.Type.TentaclePlant)
		{
			linear += 1.1f;
			exponential += 0.6f;
		}
		else if (critType == CreatureTemplate.Type.PoleMimic)
		{
			linear += 0.4f;
			exponential += 0.4f;
		}
		else if (critType == CreatureTemplate.Type.MirosBird)
		{
			linear += 1.5f;
			exponential += 0.5f;
		}
		else if (critType == CreatureTemplate.Type.Centipede)
		{
			linear += 0.8f;
			exponential += 0.5f;
		}
		else if (critType == CreatureTemplate.Type.RedCentipede)
		{
			linear += 1f;
			exponential += 0.7f;
		}
		else if (critType == CreatureTemplate.Type.Scavenger)
		{
			linear += 0.5f;
			exponential += 0.925f;
		}
		else if (critType == CreatureTemplate.Type.Centiwing)
		{
			linear += 0.8f;
			exponential += 0.5f;
		}
		else if (critType == CreatureTemplate.Type.SmallCentipede)
		{
			linear += 0.3f;
			exponential += 0.3f;
		}
		else if (critType == CreatureTemplate.Type.VultureGrub)
		{
			linear += 0.35f;
			exponential += 0.1f;
		}
		else if (critType == CreatureTemplate.Type.BigSpider)
		{
			linear += 0.6f;
			exponential += 0.55f;
		}
		else if (critType == CreatureTemplate.Type.SpitterSpider)
		{
			linear += 0.55f;
			exponential += 0.65f;
		}
		else if (critType == CreatureTemplate.Type.EggBug)
		{
			linear += 0.3f;
			exponential += 0.4f;
		}
		else if (critType == CreatureTemplate.Type.SmallNeedleWorm)
		{
			linear += 0.4f;
			exponential += 0.6f;
		}
		else if (critType == CreatureTemplate.Type.BigNeedleWorm)
		{
			linear += 0.5f;
			exponential += 0.55f;
		}
		else if (critType == CreatureTemplate.Type.DropBug)
		{
			linear += 0.5f;
			exponential += 0.5f;
		}
		else if (critType == CreatureTemplate.Type.KingVulture)
		{
			linear += 2f;
			exponential += 1.25f;
		}
		else if (critType == CreatureTemplate.Type.Hazer)
		{
			linear += 0.4f;
			exponential += 0.4f;
		}
		else if (ModManager.MSC)
		{
			if (critType == MoreSlugcatsEnums.CreatureTemplateType.SpitLizard || critType == MoreSlugcatsEnums.CreatureTemplateType.EelLizard || critType == MoreSlugcatsEnums.CreatureTemplateType.ZoopLizard)
			{
				linear += 0.5f;
				exponential += 0.5f;
			}
			else if (critType == MoreSlugcatsEnums.CreatureTemplateType.AquaCenti)
			{
				linear += 0.8f;
				exponential += 0.5f;
			}
			else if (critType == MoreSlugcatsEnums.CreatureTemplateType.ScavengerElite)
			{
				linear += 0.5f;
				exponential += 0.925f;
			}
			else if (critType == MoreSlugcatsEnums.CreatureTemplateType.FireBug)
			{
				linear += 0.3f;
				exponential += 0.4f;
			}
			else if (critType == MoreSlugcatsEnums.CreatureTemplateType.TerrorLongLegs)
			{
				linear += 4f;
				exponential += 1.75f;
			}
			else if (critType == MoreSlugcatsEnums.CreatureTemplateType.MotherSpider)
			{
				linear += 3f;
				exponential += 1.5f;
			}
			else if (critType == MoreSlugcatsEnums.CreatureTemplateType.StowawayBug)
			{
				linear += 3f;
				exponential += 1.5f;
			}
			else if (critType == MoreSlugcatsEnums.CreatureTemplateType.Inspector)
			{
				linear += 2.4f;
				exponential += 1.2f;
			}
			else if (critType == MoreSlugcatsEnums.CreatureTemplateType.Yeek)
			{
				linear += 0.3f;
				exponential += 0.4f;
			}
			else if (critType == MoreSlugcatsEnums.CreatureTemplateType.BigJelly)
			{
				linear += 3f;
				exponential += 1.9f;
			}
			else if (critType == MoreSlugcatsEnums.CreatureTemplateType.SlugNPC)
			{
				linear += 0.5f;
				exponential += 0.925f;
			}
			else
			{
				linear += 0.2f;
			}
		}
		else
		{
			linear += 0.2f;
		}
	}
}
