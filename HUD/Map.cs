using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using Menu;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace HUD;

public class Map : HudPart
{
	private class OnMapConnection
	{
		public int roomA;

		public int roomB;

		public IntVector2 posInRoomA;

		public IntVector2 posInRoomB;

		public Map map;

		public FSprite lineSprite;

		public FSprite dotA;

		public FSprite dotB;

		public int segments;

		public int dirA;

		public int dirB;

		public float lastRevealA;

		public float revealA;

		public float lastRevealB;

		public float revealB;

		public int startRevealA = -1;

		public int startRevealB = -1;

		private float lgnthFac;

		public float direction;

		public OnMapConnection(Map map, int roomA, int roomB, IntVector2 posInRoomA, IntVector2 posInRoomB, int dirA, int dirB)
		{
			this.map = map;
			this.roomA = roomA;
			this.roomB = roomB;
			this.posInRoomA = posInRoomA;
			this.posInRoomB = posInRoomB;
			this.dirA = dirA;
			this.dirB = dirB;
		}

		public void Update()
		{
			lastRevealA = revealA;
			if (startRevealA > 0)
			{
				startRevealA--;
			}
			else if (startRevealA == 0 && revealA < 1f)
			{
				revealA = Mathf.Min(revealA + 1f / Mathf.Lerp(30f, map.revealPixelsList.Count, 0.5f), 1f);
			}
			lastRevealB = revealB;
			if (startRevealB > 0)
			{
				startRevealB--;
			}
			else if (startRevealB == 0 && revealB < 1f)
			{
				revealB = Mathf.Min(revealB + 1f / Mathf.Lerp(30f, map.revealPixelsList.Count, 0.5f), 1f);
			}
		}

		public void InitiateSprites()
		{
			Vector2 a = map.OnTexturePos(posInRoomA.ToVector2() * 20f, roomA, accountForLayer: false);
			Vector2 b = map.OnTexturePos(posInRoomB.ToVector2() * 20f, roomB, accountForLayer: false);
			segments = Custom.IntClamp((int)(Vector2.Distance(a, b) / 2f), 2, 100);
			if (map.mapData.LayerOfRoom(roomA) != map.mapData.LayerOfRoom(roomB))
			{
				segments += 4;
			}
			lgnthFac = 2f / (float)segments;
			lineSprite = TriangleMesh.MakeLongMesh(segments, pointyTip: false, customColor: true);
			lineSprite.shader = map.hud.rainWorld.Shaders["MapShortcut"];
			map.container.AddChild(lineSprite);
			dotA = new FSprite("deerEyeB");
			dotB = new FSprite("deerEyeB");
			map.container.AddChild(dotA);
			map.container.AddChild(dotB);
		}

		public void DrawSprites(float timeStacker)
		{
			float num = Mathf.Lerp(lastRevealA, revealA, timeStacker);
			float num2 = Mathf.Lerp(lastRevealB, revealB, timeStacker);
			dotA.isVisible = num > 0f;
			dotB.isVisible = num2 > 0f;
			if (num == 0f && num2 == 0f)
			{
				lineSprite.isVisible = false;
				return;
			}
			lineSprite.isVisible = true;
			Vector2 pos = posInRoomA.ToVector2() * 20f;
			pos = map.RoomToMapPos(pos, roomA, timeStacker);
			Vector2 pos2 = posInRoomB.ToVector2() * 20f;
			pos2 = map.RoomToMapPos(pos2, roomB, timeStacker);
			dotA.x = pos.x;
			dotA.y = pos.y;
			dotB.x = pos2.x;
			dotB.y = pos2.y;
			float num3 = Vector2.Distance(pos, pos2);
			if (num3 > 300f)
			{
				num3 = Mathf.Lerp(num3, 300f, 0.5f);
			}
			Vector2 vector = pos;
			float num4 = map.Alpha(map.mapData.LayerOfRoom(roomA), timeStacker, compensateForLayersInFront: true) * num;
			float num5 = map.Alpha(map.mapData.LayerOfRoom(roomB), timeStacker, compensateForLayersInFront: true) * num2;
			float num6 = Mathf.InverseLerp(num, num + 0.75f, num2);
			float num7 = Mathf.InverseLerp(num2, num2 + 0.75f, num);
			dotA.color = Custom.RGB2RGBA(Color.Lerp(new Color(1f, 1f, 1f), RainWorld.MapColor, num6), num4);
			dotB.color = Custom.RGB2RGBA(Color.Lerp(new Color(1f, 1f, 1f), RainWorld.MapColor, num7), num5);
			IntVector2 intVector = Custom.fourDirections[dirA];
			IntVector2 intVector2 = Custom.fourDirections[dirB];
			num3 = ((intVector.x != -intVector2.x && intVector.y != -intVector2.y) ? (num3 / 1.5f) : (num3 / 3f));
			(lineSprite as TriangleMesh).verticeColors[0] = new Color(num6, lgnthFac, direction, num4);
			(lineSprite as TriangleMesh).verticeColors[1] = new Color(num6, lgnthFac, direction, num4);
			for (int i = 0; i < segments; i++)
			{
				float num8 = (float)(i + 1) / (float)segments;
				float t = ((float)i - 0.5f) / (float)segments;
				Vector2 vector2 = Custom.Bezier(pos, pos - intVector.ToVector2() * num3, pos2, pos2 - intVector2.ToVector2() * num3, num8);
				Vector2 vector3 = Custom.PerpendicularVector((vector - vector2).normalized);
				(lineSprite as TriangleMesh).verticeColors[i * 4] = new Color(Mathf.Lerp(num6, num7, t), lgnthFac, direction, Mathf.Lerp(num4, num5, t));
				(lineSprite as TriangleMesh).verticeColors[i * 4 + 1] = new Color(Mathf.Lerp(num6, num7, t), lgnthFac, direction, Mathf.Lerp(num4, num5, t));
				(lineSprite as TriangleMesh).verticeColors[i * 4 + 2] = new Color(Mathf.Lerp(num6, num7, num8), lgnthFac, direction, Mathf.Lerp(num4, num5, num8));
				(lineSprite as TriangleMesh).verticeColors[i * 4 + 3] = new Color(Mathf.Lerp(num6, num7, num8), lgnthFac, direction, Mathf.Lerp(num4, num5, num8));
				(lineSprite as TriangleMesh).MoveVertice(i * 4, Vector2.Lerp(vector, vector2, 0.5f) - vector3 * 1f);
				(lineSprite as TriangleMesh).MoveVertice(i * 4 + 1, Vector2.Lerp(vector, vector2, 0.5f) + vector3 * 1f);
				(lineSprite as TriangleMesh).MoveVertice(i * 4 + 2, vector2 - vector3 * 1f);
				(lineSprite as TriangleMesh).MoveVertice(i * 4 + 3, vector2 + vector3 * 1f);
				vector = vector2;
			}
		}
	}

	public class SwarmCircle
	{
		public Map map;

		public HUDCircle circle;

		public Vector2 pos;

		public int room;

		public float rad;

		public float radSpeed;

		public float life;

		public SwarmCircle(Map map, Vector2 pos, int room)
		{
			this.map = map;
			this.pos = pos;
			this.room = room;
			rad = 2.5f;
			radSpeed = 2f;
			life = 1f;
			circle = new HUDCircle(map.hud, HUDCircle.SnapToGraphic.None, map.container, 3);
			circle.pos = map.RoomToMapPos(pos, room, 1f);
			circle.lastPos = circle.pos;
		}

		public void Update()
		{
			rad += radSpeed;
			radSpeed *= 0.95f;
			life -= 1f / 35f;
			circle.Update();
			circle.rad = rad;
			circle.fade = Mathf.Pow(life, 0.5f) * map.fade * Mathf.Lerp(map.Alpha(map.mapData.LayerOfRoom(room), 1f, compensateForLayersInFront: true), 1f, 0.25f);
			circle.thickness = life * 4f;
			circle.pos = map.RoomToMapPos(pos, room, 1f);
		}

		public void Destroy()
		{
			circle.ClearSprite();
		}
	}

	public abstract class MapObject
	{
		public Map map;

		public bool slatedForDeletion;

		public MapObject(Map map)
		{
			this.map = map;
		}

		public virtual void Update()
		{
		}

		public virtual void Draw(float timeStacker)
		{
		}

		public virtual void Destroy()
		{
			slatedForDeletion = true;
		}
	}

	public class FastTravelCursor : MapObject
	{
		private HUDCircle circle;

		private FSprite bkgFade;

		private FSprite symbolSprite;

		private FSprite[] lines;

		private FastTravelScreen fastTravelScreen;

		public ShelterMarker selectedShelter;

		private float snapToMarker;

		private float pulse;

		private float lastPulse;

		public int changeLayerCounter;

		public bool lastPlaceButton;

		public float fadeOpacity;

		public float lastFadeOpacity;

		public float buttonPrompt;

		public float lastButtonPrompt;

		public FastTravelCursor(Map map, FastTravelScreen fastTravelScreen)
			: base(map)
		{
			this.fastTravelScreen = fastTravelScreen;
			bkgFade = new FSprite("Futile_White");
			bkgFade.shader = map.hud.rainWorld.Shaders["FlatLight"];
			bkgFade.color = new Color(0f, 0f, 0f);
			map.inFrontContainer.AddChild(bkgFade);
			bkgFade.isVisible = false;
			symbolSprite = new FSprite(map.hud.rainWorld.options.controls[0].gamePad ? "buttonSquareB" : "keyShiftB");
			map.inFrontContainer.AddChild(symbolSprite);
			symbolSprite.isVisible = false;
			lines = new FSprite[4];
			for (int i = 0; i < lines.Length; i++)
			{
				lines[i] = new FSprite("pixel");
				lines[i].anchorY = 0f;
				lines[i].scaleX = 2f;
				map.inFrontContainer.AddChild(lines[i]);
				lines[i].isVisible = false;
			}
			circle = new HUDCircle(map.hud, HUDCircle.SnapToGraphic.None, map.inFrontContainer, 0);
			circle.thickness = 2f;
			circle.pos = map.hud.rainWorld.screenSize / 2f;
			circle.lastPos = circle.pos;
			circle.fade = 0f;
			circle.lastFade = 0f;
			circle.rad = 0f;
		}

		public override void Update()
		{
			if (!map.visible)
			{
				snapToMarker = 0f;
				selectedShelter = null;
				circle.pos = map.hud.rainWorld.screenSize / 2f;
				circle.lastPos = circle.pos;
				circle.fade = 0f;
				circle.lastFade = 0f;
				circle.rad = 0f;
				return;
			}
			base.Update();
			lastPulse = pulse;
			pulse += 0.02f;
			lastFadeOpacity = fadeOpacity;
			ShelterMarker shelterMarker = null;
			float num = float.MaxValue;
			for (int i = 0; i < map.mapObjects.Count; i++)
			{
				if (map.mapObjects[i] is ShelterMarker)
				{
					Vector2 b = map.RoomToMapPos((map.mapObjects[i] as ShelterMarker).inRoomPos, (map.mapObjects[i] as ShelterMarker).room, 1f);
					float num2 = Vector2.Distance(map.hud.rainWorld.screenSize / 2f, b);
					if (num2 < num && num2 < 200f)
					{
						shelterMarker = map.mapObjects[i] as ShelterMarker;
						num = num2;
					}
				}
			}
			if (selectedShelter != shelterMarker)
			{
				snapToMarker = Custom.LerpAndTick(snapToMarker, 0f, 0.01f, 0.05f);
				if (snapToMarker == 0f)
				{
					selectedShelter = shelterMarker;
				}
			}
			else if (selectedShelter != null)
			{
				snapToMarker = Custom.LerpAndTick(snapToMarker, 1f, 0.01f, 0.05f);
				if (map.mapData.LayerOfRoom(selectedShelter.room) != map.layer && map.panVel.magnitude < 0.05f && Mathf.Abs(map.depth - map.lastDepth) < 0.1f)
				{
					changeLayerCounter++;
					if (changeLayerCounter > 60)
					{
						map.layer = map.mapData.LayerOfRoom(selectedShelter.room);
						changeLayerCounter = 0;
					}
				}
				else
				{
					changeLayerCounter = 0;
				}
			}
			if (selectedShelter != null && selectedShelter.room == fastTravelScreen.selectedShelter)
			{
				fadeOpacity = Custom.LerpAndTick(fadeOpacity, 1f - snapToMarker, 0.05f, 0.05f);
			}
			else
			{
				fadeOpacity = Custom.LerpAndTick(fadeOpacity, 1f, 0.05f, 0.05f);
			}
			lastButtonPrompt = buttonPrompt;
			if (selectedShelter != null && selectedShelter.room != fastTravelScreen.selectedShelter)
			{
				buttonPrompt = Custom.LerpAndTick(buttonPrompt, 1f, 0.05f, 0.04f);
			}
			else
			{
				buttonPrompt = Custom.LerpAndTick(buttonPrompt, 0f, 0.05f, 1f / 15f);
			}
			circle.Update();
			if (selectedShelter != null && snapToMarker > 0f)
			{
				circle.pos = Vector2.Lerp(map.hud.rainWorld.screenSize / 2f, map.RoomToMapPos(selectedShelter.inRoomPos, selectedShelter.room, 1f), Custom.SCurve(snapToMarker, 0.3f));
			}
			else
			{
				circle.pos = map.hud.rainWorld.screenSize / 2f;
			}
			circle.rad = Mathf.Lerp(40f, 30f + 3f * Mathf.Sin(pulse * (float)Math.PI * 2f), Mathf.Pow(snapToMarker, 2.5f));
			circle.fade = map.fade;
			bool pckp = map.hud.owner.MapInput.pckp;
			if (pckp && !lastPlaceButton)
			{
				if (selectedShelter != null)
				{
					map.hud.PlaySound(SoundID.MENU_Fast_Travel_Shelter_Select);
					fastTravelScreen.SelectNewShelter(selectedShelter.room);
					map.layer = map.mapData.LayerOfRoom(selectedShelter.room);
					if (map.playerMarker != null)
					{
						map.playerMarker.rad = 80f;
						map.playerMarker.thickness = 0f;
					}
					circle.rad = 45f;
					buttonPrompt = 0f;
				}
				else
				{
					circle.rad = 35f;
				}
			}
			lastPlaceButton = pckp;
		}

		public override void Draw(float timeStacker)
		{
			base.Draw(timeStacker);
			float num = Mathf.Lerp(circle.lastRad, circle.rad, timeStacker);
			float num2 = Mathf.Lerp(circle.lastFade, circle.fade, timeStacker);
			Vector2 vector = Vector2.Lerp(circle.lastPos, circle.pos, timeStacker);
			bkgFade.x = vector.x;
			bkgFade.y = vector.y;
			bkgFade.alpha = Mathf.Lerp(lastFadeOpacity, fadeOpacity, timeStacker) * 0.4f * num2;
			symbolSprite.x = vector.x;
			symbolSprite.y = vector.y + Mathf.Lerp(num, 80f, 0.4f);
			symbolSprite.alpha = Custom.SCurve(Mathf.Lerp(lastButtonPrompt, buttonPrompt, timeStacker), 0.4f) * num2;
			float num3 = Mathf.Pow(0.5f - 0.5f * Mathf.Sin(Mathf.Lerp(lastPulse, pulse, timeStacker) * (float)Math.PI * 2f), 0.5f);
			symbolSprite.color = new Color(num3, num3, num3);
			for (int i = 0; i < lines.Length; i++)
			{
				float num4 = -45f + 90f * (float)i;
				Vector2 vector2 = vector + Custom.DegToVec(num4) * num;
				lines[i].x = vector2.x;
				lines[i].y = vector2.y;
				lines[i].rotation = num4;
				lines[i].scaleY = Mathf.Lerp(45f - num, 10f, 0.5f);
				lines[i].alpha = num2;
				lines[i].isVisible = map.visible;
			}
			bkgFade.isVisible = map.visible;
			symbolSprite.isVisible = map.visible;
			symbolSprite.isVisible = map.visible;
			circle.Draw(timeStacker);
			circle.sprite.isVisible = map.visible;
			bkgFade.scale = 12.5f;
		}

		public override void Destroy()
		{
			base.Destroy();
			circle.ClearSprite();
			bkgFade.RemoveFromContainer();
			for (int i = 0; i < lines.Length; i++)
			{
				lines[i].RemoveFromContainer();
			}
			symbolSprite.RemoveFromContainer();
		}
	}

	public abstract class FadeInMarker : MapObject
	{
		public int room;

		public Vector2 inRoomPos;

		public float fade;

		public float lastFade;

		public float fadeInSpeed;

		public float fadeInRad;

		public FSprite bkgFade;

		public FSprite symbolSprite;

		public FadeInMarker(Map map, int room, Vector2 inRoomPos, float fadeInRad)
			: base(map)
		{
			base.map = map;
			this.room = room;
			this.inRoomPos = inRoomPos;
			this.fadeInRad = fadeInRad;
			bkgFade = new FSprite("Futile_White");
			bkgFade.shader = map.hud.rainWorld.Shaders["FlatLight"];
			bkgFade.color = new Color(0f, 0f, 0f);
			map.inFrontContainer.AddChild(bkgFade);
			bkgFade.isVisible = false;
		}

		public override void Update()
		{
			base.Update();
			lastFade = fade;
			fade = Mathf.Min(1f, fade + fadeInSpeed);
		}

		public void FadeIn(float fdSpd)
		{
			if (!(fadeInSpeed > 0f))
			{
				fadeInSpeed = 1f / fdSpd;
			}
		}

		public void SetInvisible()
		{
			fadeInSpeed = 0f;
			fade = 0f;
		}

		public override void Destroy()
		{
			base.Destroy();
			bkgFade.RemoveFromContainer();
			symbolSprite.RemoveFromContainer();
		}
	}

	public class GateMarker : FadeInMarker
	{
		public bool showAsOpen;

		public GateMarker(Map map, int room, RegionGate.GateRequirement karma, bool showAsOpen)
			: base(map, room, new Vector2(480f, 240f), 3f)
		{
			this.showAsOpen = showAsOpen;
			int result = 0;
			if (karma == null)
			{
				symbolSprite = new FSprite("smallKarmaNoRing-1");
			}
			else if (int.TryParse(karma.value, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
			{
				symbolSprite = new FSprite("smallKarmaNoRing" + Mathf.Clamp(result - 1, -1, 4));
			}
			else
			{
				symbolSprite = new FSprite("smallKarmaNoRing" + karma.value);
			}
			map.inFrontContainer.AddChild(symbolSprite);
			symbolSprite.isVisible = false;
		}

		public override void Draw(float timeStacker)
		{
			base.Draw(timeStacker);
			bkgFade.isVisible = map.visible;
			symbolSprite.isVisible = map.visible;
			if (map.visible)
			{
				float num = Mathf.Lerp(map.lastFade, map.fade, timeStacker) * Mathf.Lerp(lastFade, fade, timeStacker);
				Vector2 vector = map.RoomToMapPos(inRoomPos, room, timeStacker);
				bkgFade.x = vector.x;
				bkgFade.y = vector.y;
				bkgFade.alpha = num * 0.5f;
				symbolSprite.x = vector.x;
				symbolSprite.y = vector.y;
				symbolSprite.alpha = num;
				symbolSprite.color = Color.Lerp(showAsOpen ? global::Menu.Menu.MenuRGB(global::Menu.Menu.MenuColors.DarkGrey) : new Color(1f, 0f, 0f), global::Menu.Menu.MenuRGB(global::Menu.Menu.MenuColors.White), 0.5f + 0.5f * Mathf.Sin(((float)map.counter + timeStacker) / 14f));
				bkgFade.scale = 12.5f;
			}
		}
	}

	public class FlowerMarker : FadeInMarker
	{
		public FlowerMarker(Map map, int room, Vector2 inRoomPosition)
			: base(map, room, inRoomPosition, 5f)
		{
			symbolSprite = new FSprite("FlowerMarker");
			map.inFrontContainer.AddChild(symbolSprite);
			symbolSprite.isVisible = false;
		}

		public override void Draw(float timeStacker)
		{
			base.Draw(timeStacker);
			bkgFade.isVisible = map.visible;
			symbolSprite.isVisible = map.visible;
			if (map.visible)
			{
				float num = Mathf.Lerp(map.lastFade, map.fade, timeStacker) * Mathf.Lerp(lastFade, fade, timeStacker);
				Vector2 vector = map.RoomToMapPos(inRoomPos, room, timeStacker);
				bkgFade.x = vector.x;
				bkgFade.y = vector.y;
				bkgFade.alpha = num * 0.5f;
				symbolSprite.x = vector.x;
				symbolSprite.y = vector.y;
				symbolSprite.alpha = num;
				symbolSprite.color = Color.Lerp(new Color(1f, 1f, 1f), RainWorld.GoldRGB, 0.5f + 0.5f * Mathf.Sin(((float)map.counter + timeStacker) / 7f));
				bkgFade.scale = 10f;
			}
		}
	}

	public class ShelterMarker : FadeInMarker
	{
		public class ItemInShelterMarker
		{
			public struct ItemInShelterData
			{
				public IconSymbol.IconSymbolData symbolData;

				public EntityID ID;

				public int status;

				public ItemInShelterData(IconSymbol.IconSymbolData symbolData, EntityID ID, int status)
				{
					this.symbolData = symbolData;
					this.ID = ID;
					this.status = status;
				}

				public static ItemInShelterData? DataFromAbstractPhysical(AbstractPhysicalObject obj)
				{
					if (obj.destroyOnAbstraction)
					{
						return null;
					}
					if (obj is AbstractCreature)
					{
						if ((obj as AbstractCreature).creatureTemplate.type == CreatureTemplate.Type.Slugcat || (obj as AbstractCreature).creatureTemplate.quantified)
						{
							return null;
						}
						return new ItemInShelterData(CreatureSymbol.SymbolDataFromCreature(obj as AbstractCreature), obj.ID, (!(obj as AbstractCreature).state.alive) ? 1 : 0);
					}
					IconSymbol.IconSymbolData? iconSymbolData = ItemSymbol.SymbolDataFromItem(obj);
					int num = 0;
					if (obj is BubbleGrass.AbstractBubbleGrass && (obj as BubbleGrass.AbstractBubbleGrass).oxygenLeft < 0.1f)
					{
						num = 1;
					}
					if (iconSymbolData.HasValue)
					{
						return new ItemInShelterData(iconSymbolData.Value, obj.ID, num);
					}
					return null;
				}
			}

			private ShelterMarker owner;

			public IconSymbol symbol;

			public Vector2 pos;

			public ItemInShelterData data;

			public ItemInShelterMarker(ShelterMarker owner, FContainer container, ItemInShelterData data)
			{
				this.owner = owner;
				this.data = data;
				symbol = IconSymbol.CreateIconSymbol(data.symbolData, container);
				symbol.Show(showShadowSprites: true);
			}

			public void Update()
			{
				symbol.Update();
			}

			public void Draw(float shelterFade, Vector2 shelterScreenPos, float timeStacker, float sinAdd)
			{
				symbol.Draw(timeStacker, shelterScreenPos + pos);
				if (data.status == 1)
				{
					symbol.symbolSprite.color = Color.Lerp(global::Menu.Menu.MenuRGB(global::Menu.Menu.MenuColors.VeryDarkGrey), symbol.symbolSprite.color, Mathf.Pow(Mathf.Clamp01(0.5f + 0.5f * Mathf.Sin(((float)owner.map.counter - sinAdd + timeStacker) / 14f)), 3f));
				}
				symbol.symbolSprite.alpha = shelterFade;
				symbol.shadowSprite1.alpha = shelterFade;
				symbol.shadowSprite2.alpha = shelterFade;
			}

			public void RemoveSprites()
			{
				symbol.RemoveSprites();
			}
		}

		public List<ItemInShelterMarker> items;

		private List<float> rowsWidth;

		private bool lastVisible;

		private bool symbolsActive;

		public ShelterMarker(Map map, int room, Vector2 inRoomPosition)
			: base(map, room, inRoomPosition, 3f)
		{
			symbolSprite = new FSprite("ShelterMarker");
			map.inFrontContainer.AddChild(symbolSprite);
			symbolSprite.isVisible = false;
			rowsWidth = new List<float>();
			items = new List<ItemInShelterMarker>();
		}

		public override void Update()
		{
			base.Update();
			if (map.visible && symbolsActive && fade > 0f)
			{
				for (int i = 0; i < items.Count; i++)
				{
					items[i].Update();
				}
			}
		}

		public override void Draw(float timeStacker)
		{
			base.Draw(timeStacker);
			bkgFade.isVisible = map.visible;
			symbolSprite.isVisible = map.visible;
			bool flag = fade > 0f && lastFade > 0f && map.visible;
			if (flag && !lastVisible)
			{
				RevealSymbols();
			}
			else if (!flag && lastVisible)
			{
				HideSymbols();
			}
			lastVisible = flag;
			if (map.visible)
			{
				float num = Mathf.Lerp(map.lastFade, map.fade, timeStacker) * Mathf.Lerp(lastFade, fade, timeStacker);
				Vector2 shelterScreenPos = map.RoomToMapPos(inRoomPos, room, timeStacker);
				bkgFade.x = shelterScreenPos.x;
				bkgFade.y = shelterScreenPos.y;
				bkgFade.alpha = num * 0.5f;
				symbolSprite.x = shelterScreenPos.x;
				symbolSprite.y = shelterScreenPos.y;
				symbolSprite.alpha = num;
				symbolSprite.color = Color.Lerp(global::Menu.Menu.MenuRGB(global::Menu.Menu.MenuColors.DarkGrey), global::Menu.Menu.MenuRGB(global::Menu.Menu.MenuColors.White), 0.5f + 0.5f * Mathf.Sin(((float)map.counter + timeStacker) / 14f));
				for (int i = 0; i < items.Count; i++)
				{
					items[i].Draw(Mathf.InverseLerp(0.5f, 1f, num), shelterScreenPos, timeStacker, (float)i * 5f);
				}
				bkgFade.scale = 10f;
			}
		}

		public void RevealSymbols()
		{
			symbolsActive = true;
			items.Clear();
			rowsWidth.Clear();
			for (int i = 0; i < 50; i++)
			{
				ItemInShelterMarker.ItemInShelterData? itemInShelter = map.GetItemInShelter(room, i);
				if (itemInShelter.HasValue)
				{
					if (itemInShelter.Value.status < 0)
					{
						break;
					}
					items.Add(new ItemInShelterMarker(this, map.inFrontContainer, itemInShelter.Value));
				}
			}
			if (items.Count <= 0)
			{
				return;
			}
			for (int j = 0; j < 2; j++)
			{
				float num = 0f;
				int num2 = 0;
				for (int k = 0; k < items.Count; k++)
				{
					if (rowsWidth.Count <= num2)
					{
						rowsWidth.Add(0f);
					}
					items[k].pos = new Vector2(num + (items[k].symbol.graphWidth - rowsWidth[num2]) / 2f, (float)(num2 + 1) * -30f);
					num += items[k].symbol.graphWidth;
					if (num > 200f || k == items.Count - 1)
					{
						rowsWidth[num2] = num;
					}
					if (num > 200f)
					{
						num = 0f;
						num2++;
					}
					else
					{
						num += 5f;
					}
				}
			}
		}

		public void HideSymbols()
		{
			symbolsActive = false;
			for (int i = 0; i < items.Count; i++)
			{
				items[i].RemoveSprites();
			}
			items.Clear();
			rowsWidth.Clear();
		}

		public override void Destroy()
		{
			for (int i = 0; i < items.Count; i++)
			{
				items[i].RemoveSprites();
			}
			base.Destroy();
		}
	}

	public class SlugcatMarker : FadeInMarker
	{
		public SlugcatMarker(Map map, int room, Vector2 inRoomPosition, Color slugcatColor)
			: base(map, room, inRoomPosition, 3f)
		{
			symbolSprite = new FSprite("Kill_Slugcat");
			symbolSprite.color = slugcatColor;
			map.inFrontContainer.AddChild(symbolSprite);
			symbolSprite.isVisible = false;
		}

		public override void Draw(float timeStacker)
		{
			base.Draw(timeStacker);
			bkgFade.isVisible = map.visible;
			symbolSprite.isVisible = map.visible;
			if (map.visible)
			{
				float num = Mathf.Lerp(map.lastFade, map.fade, timeStacker) * Mathf.Lerp(lastFade, fade, timeStacker);
				Vector2 vector = map.RoomToMapPos(inRoomPos, room, timeStacker);
				bkgFade.x = vector.x;
				bkgFade.y = vector.y;
				bkgFade.alpha = num * 0.5f;
				symbolSprite.x = vector.x;
				symbolSprite.y = vector.y;
				symbolSprite.alpha = num;
				bkgFade.scale = 6.25f;
			}
		}
	}

	public class DeathMarker : FadeInMarker
	{
		private int age;

		private float ageFade;

		private FSprite shadeSprite;

		public DeathMarker(Map map, int room, Vector2 inRoomPosition, int age)
			: base(map, room, inRoomPosition, 3f)
		{
			this.age = age;
			ageFade = Mathf.InverseLerp(6f, 2f, age);
			shadeSprite = new FSprite("Multiplayer_Bones");
			shadeSprite.color = new Color(0f, 0f, 0f);
			map.inFrontContainer.AddChild(shadeSprite);
			shadeSprite.isVisible = false;
			symbolSprite = new FSprite("Multiplayer_Bones");
			map.inFrontContainer.AddChild(symbolSprite);
			symbolSprite.isVisible = false;
		}

		public override void Draw(float timeStacker)
		{
			base.Draw(timeStacker);
			bkgFade.isVisible = map.visible;
			symbolSprite.isVisible = map.visible;
			shadeSprite.isVisible = map.visible;
			if (!map.visible)
			{
				return;
			}
			float num = Mathf.Lerp(map.lastFade, map.fade, timeStacker) * Mathf.Lerp(lastFade, fade, timeStacker) * (0.5f + 0.5f * ageFade);
			Vector2 vector = map.RoomToMapPos(inRoomPos, room, timeStacker);
			bkgFade.x = vector.x;
			bkgFade.y = vector.y;
			bkgFade.alpha = num * 0.5f;
			symbolSprite.x = vector.x;
			symbolSprite.y = vector.y;
			symbolSprite.alpha = num;
			shadeSprite.x = vector.x - 1f;
			shadeSprite.y = vector.y - 2f;
			shadeSprite.alpha = num;
			if (age == 0)
			{
				if (map.hud.owner.GetOwnerType() == HUD.OwnerType.DeathScreen)
				{
					symbolSprite.color = new Color(1f, 0f, 0f);
				}
				else
				{
					symbolSprite.color = Color.Lerp(global::Menu.Menu.MenuRGB(global::Menu.Menu.MenuColors.White), new Color(1f, 0f, 0f), 0.25f + 0.25f * Mathf.Sin(((float)map.counter + timeStacker) / 7f));
				}
			}
			else
			{
				symbolSprite.color = Color.Lerp(Color.Lerp(global::Menu.Menu.MenuRGB(global::Menu.Menu.MenuColors.MediumGrey), new Color(1f, 0f, 0f), 0.25f + 0.25f * Mathf.Sin(((float)map.counter + timeStacker) / 14f)), global::Menu.Menu.MenuRGB(global::Menu.Menu.MenuColors.DarkGrey), 1f - ageFade);
			}
			bkgFade.scale = (40f + 10f * num) / 8f;
		}

		public override void Destroy()
		{
			shadeSprite.RemoveFromContainer();
			base.Destroy();
		}
	}

	public class CycleLabel
	{
		private Map owner;

		private FLabel label;

		private float fade;

		private float lastFade;

		public int revealTimer;

		private int red = -1;

		public CycleLabel(Map owner)
		{
			this.owner = owner;
			label = new FLabel(Custom.GetDisplayFont(), "");
			UpdateCycleText();
			owner.container.AddChild(label);
			label.x = 20.01f;
			label.y = 754.1f;
			label.alignment = FLabelAlignment.Left;
			label.color = global::Menu.Menu.MenuRGB(global::Menu.Menu.MenuColors.MediumGrey);
			label.alpha = 0f;
		}

		public void UpdateCycleText()
		{
			Player player = owner.hud.owner as Player;
			int num = player.abstractCreature.world.game.GetStorySession.saveState.cycleNumber;
			if ((player.abstractCreature.world.game.session as StoryGameSession).saveState.saveStateNumber == SlugcatStats.Name.Red)
			{
				num = RedsIllness.RedsCycles(player.abstractCreature.world.game.GetStorySession.saveState.redExtraCycles) - num;
			}
			if (num <= 0)
			{
				red = 1;
			}
			else
			{
				red = -1;
			}
			label.text = owner.hud.rainWorld.inGameTranslator.Translate("Cycle") + " " + num;
		}

		public void Update()
		{
			lastFade = fade;
			revealTimer = Custom.IntClamp(revealTimer + ((owner.fade > 0.9f) ? 1 : ((owner.fade < 0.05f) ? (-20) : (-1))), 0, 120);
			fade = Custom.LerpAndTick(fade, (revealTimer > 30) ? 1f : 0f, 0.04f, 1f / 60f);
			if (red > 0)
			{
				red++;
			}
		}

		public void Draw(float timeStacker, float useAlpha)
		{
			label.alpha = Custom.SCurve(useAlpha * Mathf.Lerp(lastFade, fade, timeStacker), 0.7f);
			if (red > 0)
			{
				label.color = Color.Lerp(global::Menu.Menu.MenuRGB(global::Menu.Menu.MenuColors.MediumGrey), Color.red, 0.5f + 0.5f * Mathf.Sin(((float)red + timeStacker) / 7f));
			}
		}

		public void ClearSprites()
		{
			label.RemoveFromContainer();
		}
	}

	public class MapData
	{
		public class SwarmRoomData
		{
			public int roomIndex;

			public IntVector2[] hivePositions;

			public bool active;

			public SwarmRoomData(int roomIndex, List<IntVector2> hivePositions)
			{
				this.roomIndex = roomIndex;
				this.hivePositions = hivePositions.ToArray();
			}
		}

		public class ShelterData
		{
			public int roomIndex;

			public Vector2 markerPos;

			public List<ShelterMarker.ItemInShelterMarker.ItemInShelterData> itemsData;

			public ShelterData(int roomIndex, Vector2 markerPos)
			{
				this.roomIndex = roomIndex;
				this.markerPos = markerPos;
			}
		}

		public class GateData
		{
			public int roomIndex;

			public RegionGate.GateRequirement karma;

			public GateData(int roomIndex, RegionGate.GateRequirement karma)
			{
				this.roomIndex = roomIndex;
				this.karma = karma;
			}
		}

		public int firstRoomIndex;

		public string regionName;

		public Vector2[] roomPositions;

		public IntVector2[] roomSizes;

		public int[] roomLayers;

		public string[] roomNames;

		public int[] roomIndices;

		public SwarmRoomData[] activeSwarmRooms;

		public ShelterData[] shelterData;

		public GateData[] gateData;

		public WorldCoordinate? karmaFlowerPos;

		public int currentKarma;

		public List<string> roomConnections;

		public List<PersistentObjectTracker> objectTrackers;

		public MapData(World initWorld, RainWorld rainWorld)
		{
			regionName = ((initWorld.region == null) ? null : initWorld.region.name);
			roomPositions = new Vector2[initWorld.NumberOfRooms];
			roomSizes = new IntVector2[initWorld.NumberOfRooms];
			roomLayers = new int[initWorld.NumberOfRooms];
			roomNames = new string[initWorld.NumberOfRooms];
			roomIndices = new int[initWorld.NumberOfRooms];
			firstRoomIndex = initWorld.firstRoomIndex;
			roomConnections = new List<string>();
			List<SwarmRoomData> list = new List<SwarmRoomData>();
			List<ShelterData> list2 = new List<ShelterData>();
			List<GateData> list3 = new List<GateData>();
			for (int i = 0; i < initWorld.NumberOfRooms; i++)
			{
				roomPositions[i] = initWorld.GetAbstractRoom(i + initWorld.firstRoomIndex).mapPos;
				roomSizes[i] = initWorld.GetAbstractRoom(i + initWorld.firstRoomIndex).size;
				roomLayers[i] = initWorld.GetAbstractRoom(i + initWorld.firstRoomIndex).layer;
				roomNames[i] = initWorld.GetAbstractRoom(i + initWorld.firstRoomIndex).name;
				roomIndices[i] = i + initWorld.firstRoomIndex;
				bool flag = true;
				foreach (string disabledMapRoom in initWorld.DisabledMapRooms)
				{
					if (disabledMapRoom == roomNames[i])
					{
						flag = false;
						break;
					}
				}
				AbstractRoom abstractRoom = null;
				if (flag)
				{
					abstractRoom = initWorld.GetAbstractRoom(i + initWorld.firstRoomIndex);
					for (int j = 0; j < abstractRoom.connections.Length; j++)
					{
						if (abstractRoom.connections[j] <= -1)
						{
							continue;
						}
						AbstractRoom abstractRoom2 = initWorld.GetAbstractRoom(abstractRoom.connections[j]);
						if (abstractRoom2 != null)
						{
							string item = abstractRoom.name + "," + abstractRoom2.name;
							string item2 = abstractRoom2.name + "," + abstractRoom.name;
							if (!roomConnections.Contains(item) && !roomConnections.Contains(item2))
							{
								roomConnections.Add(item);
							}
						}
					}
				}
				if (!flag)
				{
					continue;
				}
				if (abstractRoom.swarmRoom)
				{
					List<IntVector2> list4 = new List<IntVector2>();
					Room room = new Room(null, initWorld, abstractRoom);
					new RoomPreparer(room, loadAiHeatMaps: true, falseBake: false, shortcutsOnly: false);
					for (int k = 0; k < room.hives.Length; k++)
					{
						for (int l = 0; l < room.hives[k].Length; l++)
						{
							list4.Add(room.hives[k][l]);
						}
					}
					list.Add(new SwarmRoomData(abstractRoom.index, list4));
				}
				else if (abstractRoom.shelter && (!initWorld.brokenShelters[abstractRoom.shelterIndex] || initWorld.brokenShelterIndexDueToPrecycle == abstractRoom.shelterIndex))
				{
					Room room2 = new Room(null, initWorld, abstractRoom);
					if (abstractRoom.isAncientShelter)
					{
						IntVector2 size = abstractRoom.size;
						size.x /= 2;
						size.y /= 2;
						size.x -= 2;
						size.y += 2;
						list2.Add(new ShelterData(abstractRoom.index, room2.MiddleOfTile(size)));
						continue;
					}
					RoomPreparer roomPreparer = new RoomPreparer(room2, loadAiHeatMaps: true, falseBake: false, shortcutsOnly: true);
					while (!roomPreparer.done)
					{
						roomPreparer.Update();
						Thread.Sleep(1);
					}
					IntVector2 startTile = room2.ShortcutLeadingToNode(0).StartTile;
					startTile += room2.ShorcutEntranceHoleDirection(startTile) * 6;
					list2.Add(new ShelterData(abstractRoom.index, room2.MiddleOfTile(startTile)));
				}
				else if (abstractRoom.gate)
				{
					list3.Add(new GateData(i + initWorld.firstRoomIndex, KarmaOfGate(rainWorld.progression, initWorld, abstractRoom.name)));
				}
			}
			activeSwarmRooms = list.ToArray();
			shelterData = list2.ToArray();
			gateData = list3.ToArray();
			if (initWorld.game != null && initWorld.game.session != null && initWorld.game.session is StoryGameSession)
			{
				UpdateData(initWorld, 0, (initWorld.game.session as StoryGameSession).saveState.deathPersistentSaveData.karma, (initWorld.game.session as StoryGameSession).karmaFlowerMapPos, putItemsInShelters: false);
				if (ModManager.MMF)
				{
					if (MMF.cfgKeyItemTracking.Value)
					{
						objectTrackers = new List<PersistentObjectTracker>((initWorld.game.session as StoryGameSession).saveState.objectTrackers);
					}
					else
					{
						objectTrackers = new List<PersistentObjectTracker>();
					}
				}
			}
			else
			{
				UpdateData(initWorld, 0, 1000, null, putItemsInShelters: false);
				if (ModManager.MMF)
				{
					objectTrackers = new List<PersistentObjectTracker>();
				}
			}
		}

		public string NameOfRoom(int room)
		{
			room -= firstRoomIndex;
			if (room < 0 || room >= roomNames.Length)
			{
				return "";
			}
			return roomNames[room];
		}

		public Vector2 PositionOfRoom(int room)
		{
			room -= firstRoomIndex;
			if (room < 0 || room >= roomPositions.Length)
			{
				return new Vector2(0f, 0f);
			}
			return roomPositions[room];
		}

		public IntVector2 SizeOfRoom(int room)
		{
			room -= firstRoomIndex;
			if (room < 0 || room >= roomSizes.Length)
			{
				return new IntVector2(0, 0);
			}
			return roomSizes[room];
		}

		public int LayerOfRoom(int room)
		{
			room -= firstRoomIndex;
			if (room < 0 || room >= roomLayers.Length)
			{
				return 1;
			}
			return roomLayers[room];
		}

		public RegionGate.GateRequirement KarmaOfGate(PlayerProgression progression, World initWorld, string roomName)
		{
			if (initWorld.game != null && initWorld.game.IsStorySession && initWorld.game.GetStorySession.saveState.deathPersistentSaveData.unlockedGates != null && initWorld.game.GetStorySession.saveState.deathPersistentSaveData.CanUseUnlockedGates(initWorld.game.GetStorySession.saveStateNumber))
			{
				for (int i = 0; i < initWorld.game.GetStorySession.saveState.deathPersistentSaveData.unlockedGates.Count; i++)
				{
					if (initWorld.game.GetStorySession.saveState.deathPersistentSaveData.unlockedGates[i] == roomName)
					{
						return null;
					}
				}
			}
			for (int j = 0; j < progression.karmaLocks.Length; j++)
			{
				string[] array = Regex.Split(progression.karmaLocks[j], " : ");
				if (array[0] == roomName)
				{
					string value = array[1];
					string value2 = array[2];
					bool flag = array.Length >= 4 && array[3] == "SWAPMAPSYMBOL";
					RegionGate.GateRequirement gateRequirement = ((Region.EquivalentRegion(Regex.Split(roomName, "_")[1], initWorld.region.name) == flag) ? new RegionGate.GateRequirement(value2) : new RegionGate.GateRequirement(value));
					if (ModManager.MMF && MMF.cfgDisableGateKarma.Value && int.TryParse(gateRequirement.value, out var _))
					{
						gateRequirement = RegionGate.GateRequirement.OneKarma;
					}
					return gateRequirement;
				}
			}
			return RegionGate.GateRequirement.OneKarma;
		}

		public Vector2 ShelterMarkerPosOfRoom(int room)
		{
			for (int i = 0; i < shelterData.Length; i++)
			{
				if (shelterData[i].roomIndex == room)
				{
					return shelterData[i].markerPos;
				}
			}
			return SizeOfRoom(room).ToVector2() * 10f;
		}

		public void UpdateData(World world, int foodTicks, int currKarm, WorldCoordinate? kfPos, bool putItemsInShelters)
		{
			currentKarma = currKarm;
			if (kfPos.HasValue && kfPos.Value.Valid && kfPos.Value.room >= world.firstRoomIndex && kfPos.Value.room < world.firstRoomIndex + world.NumberOfRooms)
			{
				karmaFlowerPos = kfPos;
			}
			else
			{
				karmaFlowerPos = null;
			}
			if (world.singleRoomWorld || world.game == null)
			{
				return;
			}
			for (int i = 0; i < world.swarmRooms.Length; i++)
			{
				string name = world.GetAbstractRoom(world.swarmRooms[i]).name;
				int num = 0;
				if (world.regionState.swarmRoomCounters.ContainsKey(name))
				{
					num = world.regionState.swarmRoomCounters[name];
				}
				activeSwarmRooms[i].active = world.regionState.SwarmRoomActive(i) || num - foodTicks <= 0;
			}
			if (!putItemsInShelters)
			{
				return;
			}
			for (int j = 0; j < shelterData.Length; j++)
			{
				shelterData[j].itemsData = new List<ShelterMarker.ItemInShelterMarker.ItemInShelterData>();
			}
			for (int k = world.firstRoomIndex; k < world.firstRoomIndex + world.NumberOfRooms; k++)
			{
				if (!world.GetAbstractRoom(k).shelter)
				{
					continue;
				}
				int num2 = -1;
				for (int l = 0; l < shelterData.Length; l++)
				{
					if (shelterData[l].roomIndex == k)
					{
						num2 = l;
						break;
					}
				}
				if (num2 <= -1)
				{
					continue;
				}
				for (int m = 0; m < 50; m++)
				{
					ShelterMarker.ItemInShelterMarker.ItemInShelterData? itemInShelterFromWorld = GetItemInShelterFromWorld(world, k, m);
					if (itemInShelterFromWorld.HasValue)
					{
						if (itemInShelterFromWorld.Value.status < 0)
						{
							break;
						}
						shelterData[num2].itemsData.Add(itemInShelterFromWorld.Value);
					}
				}
			}
		}
	}

	public class ItemMarker : FadeInMarker
	{
		public struct ItemMakerData
		{
			public IconSymbol.IconSymbolData symbolData;

			public EntityID ID;

			public int status;

			public bool positionSetFromRealized;

			public ItemMakerData(IconSymbol.IconSymbolData symbolData, EntityID ID, int status)
			{
				this.symbolData = symbolData;
				this.ID = ID;
				this.status = status;
				positionSetFromRealized = false;
			}

			public static ItemMakerData? DataFromAbstractPhysical(AbstractPhysicalObject obj)
			{
				if (obj.destroyOnAbstraction)
				{
					return null;
				}
				if (obj is AbstractCreature)
				{
					if ((obj as AbstractCreature).creatureTemplate.type == CreatureTemplate.Type.Slugcat || (obj as AbstractCreature).creatureTemplate.quantified)
					{
						return null;
					}
					return new ItemMakerData(CreatureSymbol.SymbolDataFromCreature(obj as AbstractCreature), obj.ID, (!(obj as AbstractCreature).state.alive) ? 1 : 0);
				}
				IconSymbol.IconSymbolData? iconSymbolData = ItemSymbol.SymbolDataFromItem(obj);
				int num = 0;
				if (obj is BubbleGrass.AbstractBubbleGrass && (obj as BubbleGrass.AbstractBubbleGrass).oxygenLeft < 0.1f)
				{
					num = 1;
				}
				if (iconSymbolData.HasValue)
				{
					return new ItemMakerData(iconSymbolData.Value, obj.ID, num);
				}
				return null;
			}
		}

		public IconSymbol symbol;

		public ItemMakerData data;

		public AbstractPhysicalObject obj;

		public bool positionSetFromRealized;

		public ItemMarker(Map map, int room, Vector2 inRoomPosition, AbstractPhysicalObject obj)
			: base(map, room, inRoomPosition, 3f)
		{
			symbolSprite = new FSprite("Futile_White");
			map.inFrontContainer.AddChild(symbolSprite);
			symbolSprite.isVisible = false;
			bkgFade.isVisible = false;
			this.obj = obj;
		}

		public override void Draw(float timeStacker)
		{
			base.Draw(timeStacker);
			if (!MMF.cfgCreatureSense.Value || !map.visible || obj == null || obj.Room.shelter)
			{
				if (symbol != null)
				{
					symbol.RemoveSprites();
					symbol = null;
				}
				return;
			}
			ItemMakerData? itemMakerData = ItemMakerData.DataFromAbstractPhysical(obj);
			if (symbol == null && itemMakerData.HasValue)
			{
				data = itemMakerData.Value;
				symbol = IconSymbol.CreateIconSymbol(data.symbolData, map.inFrontContainer);
				symbol.Show(showShadowSprites: true);
			}
			symbol.Draw(timeStacker, map.RoomToMapPos(inRoomPos, room, timeStacker));
			symbol.symbolSprite.color = symbol.myColor;
			if (data.status == 1)
			{
				symbol.symbolSprite.color = Color.Lerp(global::Menu.Menu.MenuRGB(global::Menu.Menu.MenuColors.VeryDarkGrey), symbol.symbolSprite.color, Mathf.Pow(Mathf.Clamp01(0.5f + 0.5f * Mathf.Sin(((float)map.counter + timeStacker) / 14f)), 3f));
			}
			float alpha = Mathf.InverseLerp(0.5f, 1f, Mathf.Lerp(map.lastFade, map.fade, timeStacker) * Mathf.Lerp(lastFade, fade, timeStacker));
			symbol.symbolSprite.alpha = alpha;
			symbol.shadowSprite1.alpha = alpha;
			symbol.shadowSprite2.alpha = alpha;
		}

		public void RemoveSprites()
		{
			symbol.RemoveSprites();
		}
	}

	public Texture2D mapTexture;

	public Texture2D revealTexture;

	public MapData mapData;

	public List<IntVector2> revealPixelsList;

	public List<IntVector2> revealFadePixels;

	public Vector2 mapSize;

	public FSprite backgroundBlack;

	public FSprite playerMarkerFade;

	public HUDCircle playerMarker;

	public FSprite[] mapSprites;

	public List<MapObject> mapObjects;

	public List<FadeInMarker> notRevealedFadeMarkers;

	public Vector2 lastPanPos;

	public Vector2 panPos;

	public Vector2 panVel;

	public float fade;

	public float lastFade;

	public int fadeCounter;

	public float MapScale = 4f;

	public float DiscoverResolution = 7f;

	public bool mapLoaded;

	public bool discLoaded;

	public bool revealAllDiscovered;

	public int layer = 1;

	public float depth;

	public float lastDepth;

	private bool layerButtonA;

	private bool layerButtonB;

	private bool visible;

	public int resetRevealCounter = -1;

	public int counter;

	public Vector2 playerStandPos;

	private List<OnMapConnection> mapConnections;

	public IntVector2 lastOnTexturePos = new IntVector2(-1, -1);

	public bool[] STANDARDELEMENTLIST;

	public FContainer inFrontContainer;

	public List<SwarmCircle> swarmCircles;

	public float speedUp;

	public CycleLabel cycleLabel;

	public List<ItemMarker> itemMarkers;

	private List<CreatureSymbol> creatureSymbols;

	public Texture2D discoverTexture
	{
		get
		{
			if (mapData.regionName == null || !hud.rainWorld.progression.mapDiscoveryTextures.ContainsKey(mapData.regionName))
			{
				return null;
			}
			return hud.rainWorld.progression.mapDiscoveryTextures[mapData.regionName];
		}
		set
		{
			hud.rainWorld.progression.mapDiscoveryTextures[mapData.regionName] = value;
		}
	}

	public string RegionName
	{
		get
		{
			if (mapData.regionName != null)
			{
				return mapData.regionName;
			}
			return "";
		}
	}

	public FContainer container => hud.fContainers[0];

	public void ResetNotRevealedMarkers()
	{
		notRevealedFadeMarkers.Clear();
		for (int i = 0; i < mapObjects.Count; i++)
		{
			if (mapObjects[i] is FadeInMarker)
			{
				(mapObjects[i] as FadeInMarker).SetInvisible();
				notRevealedFadeMarkers.Add(mapObjects[i] as FadeInMarker);
			}
		}
	}

	public Map(HUD hud, MapData mapData)
		: base(hud)
	{
		this.mapData = mapData;
		LoadConnectionPositions();
		revealPixelsList = new List<IntVector2>();
		revealFadePixels = new List<IntVector2>();
		STANDARDELEMENTLIST = new bool[3];
		if (RegionName == "CC" || RegionName == "SI")
		{
			STANDARDELEMENTLIST[1] = true;
			STANDARDELEMENTLIST[2] = true;
		}
		Shader.SetGlobalVector(RainWorld.ShadPropScreenSize, hud.rainWorld.screenSize);
		Shader.SetGlobalColor(RainWorld.ShadPropMapCol, RainWorld.MapColor);
		if (!ModManager.MMF || mapData.regionName == null)
		{
			RainWorld.MapWaterColor(RainWorld.DefaultWaterColor);
		}
		else if (base.hud.rainWorld.processManager.currentMainLoop is RainWorldGame)
		{
			Region[] regions = (base.hud.rainWorld.processManager.currentMainLoop as RainWorldGame).overWorld.regions;
			bool flag = false;
			for (int i = 0; i < regions.Length; i++)
			{
				if (regions[i].name == mapData.regionName)
				{
					RainWorld.MapWaterColor(regions[i].propertiesWaterColor);
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				RainWorld.MapWaterColor(RainWorld.DefaultWaterColor);
			}
		}
		else if (base.hud.rainWorld.processManager.currentMainLoop is FastTravelScreen)
		{
			Region[] allRegions = (base.hud.rainWorld.processManager.currentMainLoop as FastTravelScreen).allRegions;
			bool flag2 = false;
			for (int j = 0; j < allRegions.Length; j++)
			{
				if (allRegions[j].name == mapData.regionName)
				{
					RainWorld.MapWaterColor(allRegions[j].propertiesWaterColor);
					flag2 = true;
					break;
				}
			}
			if (!flag2)
			{
				RainWorld.MapWaterColor(RainWorld.DefaultWaterColor);
			}
		}
		Shader.SetGlobalColor(RainWorld.ShadPropMapWaterCol, RainWorld.MapWaterColor(null));
		mapObjects = new List<MapObject>();
		swarmCircles = new List<SwarmCircle>();
		inFrontContainer = new FContainer();
		Futile.stage.AddChild(inFrontContainer);
		SaveState saveState = null;
		if (hud.owner.GetOwnerType() == HUD.OwnerType.Player || hud.owner.GetOwnerType() == HUD.OwnerType.FastTravelScreen || hud.owner.GetOwnerType() == HUD.OwnerType.RegionOverview || (ModManager.MSC && hud.owner.GetOwnerType() == MoreSlugcatsEnums.OwnerType.SafariOverseer))
		{
			saveState = hud.rainWorld.progression.currentSaveState;
		}
		else if (hud.owner.GetOwnerType() != HUD.OwnerType.RegionOverview)
		{
			saveState = (hud.owner as SleepAndDeathScreen).saveState;
		}
		for (int k = 0; k < mapData.gateData.Length; k++)
		{
			bool flag3 = false;
			int result;
			if (mapData.gateData[k].karma == null)
			{
				flag3 = true;
			}
			else if (int.TryParse(mapData.gateData[k].karma.value, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
			{
				flag3 = result - 1 <= mapData.currentKarma;
			}
			RegionGate.GateRequirement karma = mapData.gateData[k].karma;
			if (saveState == null || (hud.owner.GetOwnerType() != HUD.OwnerType.Player && hud.owner.GetOwnerType() != HUD.OwnerType.SleepScreen && hud.owner.GetOwnerType() != HUD.OwnerType.DeathScreen))
			{
				flag3 = true;
			}
			else if (ModManager.MSC)
			{
				Custom.Log($"gate condition on map, karma {mapData.gateData[k].karma}");
				if (mapData.gateData[k].karma == MoreSlugcatsEnums.GateRequirement.RoboLock)
				{
					flag3 = false;
					if (saveState.hasRobo && saveState.deathPersistentSaveData.theMark)
					{
						flag3 = true;
					}
					Custom.Log("LC/MS gate condition on map", flag3.ToString());
				}
				if (this.mapData.NameOfRoom(mapData.gateData[k].roomIndex) == "GATE_SB_OE")
				{
					flag3 = false;
					if (saveState.saveStateNumber == MoreSlugcatsEnums.SlugcatStatsName.Gourmand && saveState.deathPersistentSaveData.theMark)
					{
						flag3 = true;
					}
					if (saveState.saveStateNumber != MoreSlugcatsEnums.SlugcatStatsName.Gourmand && (saveState.saveStateNumber == SlugcatStats.Name.White || saveState.saveStateNumber == SlugcatStats.Name.Yellow) && base.hud.rainWorld.progression.miscProgressionData.beaten_Gourmand)
					{
						flag3 = true;
					}
					if (global::MoreSlugcats.MoreSlugcats.chtUnlockOuterExpanse.Value)
					{
						flag3 = true;
					}
					karma = ((!flag3) ? MoreSlugcatsEnums.GateRequirement.OELock : ((!(RegionName == "SB")) ? RegionGate.GateRequirement.FiveKarma : RegionGate.GateRequirement.OneKarma));
					Custom.Log("OE gate condition on map", flag3.ToString());
				}
			}
			mapObjects.Add(new GateMarker(this, mapData.gateData[k].roomIndex, karma, flag3));
		}
		if (ModManager.MMF && MMF.cfgKeyItemTracking.Value)
		{
			itemMarkers = new List<ItemMarker>();
			for (int l = 0; l < mapData.objectTrackers.Count; l++)
			{
				AbstractPhysicalObject obj = mapData.objectTrackers[l].obj;
				if (obj != null)
				{
					ItemMarker item = new ItemMarker(this, obj.Room.index, obj.pos.Tile.ToVector2(), obj);
					mapObjects.Add(item);
					itemMarkers.Add(item);
				}
				else
				{
					itemMarkers.Add(null);
				}
			}
		}
		for (int m = 0; m < mapData.shelterData.Length; m++)
		{
			bool flag4 = true;
			if (hud.owner.GetOwnerType() == HUD.OwnerType.FastTravelScreen)
			{
				flag4 = false;
				for (int n = 0; n < (hud.owner as FastTravelScreen).discoveredSheltersInRegion.Count; n++)
				{
					if ((hud.owner as FastTravelScreen).discoveredSheltersInRegion[n] == mapData.shelterData[m].roomIndex)
					{
						flag4 = true;
						break;
					}
				}
			}
			if (flag4)
			{
				mapObjects.Add(new ShelterMarker(this, mapData.shelterData[m].roomIndex, mapData.shelterData[m].markerPos));
			}
		}
		if (hud.owner.GetOwnerType() == HUD.OwnerType.RegionOverview)
		{
			List<SlugcatMarker> list = new List<SlugcatMarker>();
			for (int num = 0; num < (hud.owner as FastTravelScreen).playerShelters.Length; num++)
			{
				if ((hud.owner as FastTravelScreen).playerShelters[num] == null)
				{
					continue;
				}
				for (int num2 = 0; num2 < mapData.roomNames.Length; num2++)
				{
					if (mapData.roomNames[num2] == (hud.owner as FastTravelScreen).playerShelters[num])
					{
						list.Add(new SlugcatMarker(this, mapData.firstRoomIndex + num2, mapData.roomSizes[num2].ToVector2() * 10f, PlayerGraphics.SlugcatColor(new SlugcatStats.Name(ExtEnum<SlugcatStats.Name>.values.GetEntry(num)))));
						mapObjects.Add(list[list.Count - 1]);
						break;
					}
				}
			}
			for (int num3 = list.Count - 1; num3 >= 0; num3--)
			{
				List<SlugcatMarker> list2 = new List<SlugcatMarker> { list[num3] };
				for (int num4 = num3 + 1; num4 < list.Count; num4++)
				{
					if (list[num3].room == list[num4].room)
					{
						list2.Add(list[num4]);
					}
				}
				if (list2.Count > 1)
				{
					for (int num5 = 0; num5 < list2.Count; num5++)
					{
						float t = Mathf.InverseLerp(0f, list2.Count - 1, num5);
						list2[num5].inRoomPos = mapData.roomSizes[list2[num5].room - mapData.firstRoomIndex].ToVector2() * 10f + Vector2.Lerp(new Vector2(-30f, 30f), new Vector2(30f, -30f), t);
					}
				}
			}
		}
		else if (hud.owner.GetOwnerType() == HUD.OwnerType.Player || hud.owner.GetOwnerType() == HUD.OwnerType.DeathScreen || hud.owner.GetOwnerType() == HUD.OwnerType.SleepScreen)
		{
			SaveState saveState2 = ((!(hud.owner.GetOwnerType() == HUD.OwnerType.Player)) ? (hud.owner as SleepAndDeathScreen).saveState : hud.rainWorld.progression.currentSaveState);
			if (saveState2 != null)
			{
				for (int num6 = 0; num6 < saveState2.deathPersistentSaveData.deathPositions.Count; num6++)
				{
					if (saveState2.deathPersistentSaveData.deathPositions[num6].Valid && saveState2.deathPersistentSaveData.deathPositions[num6].room >= mapData.firstRoomIndex && saveState2.deathPersistentSaveData.deathPositions[num6].room < mapData.firstRoomIndex + mapData.roomPositions.Length)
					{
						mapObjects.Add(new DeathMarker(this, saveState2.deathPersistentSaveData.deathPositions[num6].room, saveState2.deathPersistentSaveData.deathPositions[num6].Tile.ToVector2() * 20f, saveState2.deathPersistentSaveData.deathPositions[num6].abstractNode));
					}
				}
			}
		}
		else if (hud.owner.GetOwnerType() == HUD.OwnerType.FastTravelScreen)
		{
			mapObjects.Add(new FastTravelCursor(this, hud.owner as FastTravelScreen));
		}
		if (mapData.karmaFlowerPos.HasValue)
		{
			mapObjects.Add(new FlowerMarker(this, mapData.karmaFlowerPos.Value.room, mapData.karmaFlowerPos.Value.Tile.ToVector2() * 20f));
		}
		notRevealedFadeMarkers = new List<FadeInMarker>();
		ResetNotRevealedMarkers();
	}

	public override void Update()
	{
		counter++;
		if (!mapLoaded)
		{
			if (RegionName.Length > 0)
			{
				string text = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + RegionName + Path.DirectorySeparatorChar + "map_" + RegionName + "-" + hud.rainWorld.progression.PlayingAsSlugcat?.ToString() + ".png");
				if (File.Exists(text))
				{
					mapTexture = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false);
					AssetManager.SafeWWWLoadTexture(ref mapTexture, "file:///" + text, clampWrapMode: true, crispPixels: false);
				}
				else
				{
					text = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + RegionName + Path.DirectorySeparatorChar + "map_" + RegionName + ".png");
					if (File.Exists(text))
					{
						mapTexture = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false);
						AssetManager.SafeWWWLoadTexture(ref mapTexture, "file:///" + text, clampWrapMode: true, crispPixels: false);
					}
				}
			}
			if (mapTexture != null)
			{
				HeavyTexturesCache.LoadAndCacheAtlasFromTexture("map_" + RegionName, mapTexture, textureFromAsset: false);
				backgroundBlack = new FSprite("Futile_White");
				backgroundBlack.color = new Color(0f, 0f, 0f);
				container.AddChild(backgroundBlack);
				backgroundBlack.scaleX = hud.rainWorld.options.ScreenSize.x / 16f;
				backgroundBlack.scaleY = 48f;
				backgroundBlack.anchorX = 0f;
				backgroundBlack.anchorY = 0f;
				backgroundBlack.x = 0f;
				backgroundBlack.y = 0f;
				backgroundBlack.isVisible = false;
				mapSprites = new FSprite[3];
				for (int i = 0; i < mapSprites.Length; i++)
				{
					mapSprites[i] = new FSprite("map_" + RegionName);
					mapSprites[i].shader = hud.rainWorld.Shaders[(!STANDARDELEMENTLIST[i]) ? "Map" : "MapAerial"];
					container.AddChild(mapSprites[i]);
					mapSprites[i].isVisible = false;
				}
				if (hud.owner is Player && (hud.owner as Player).slugcatStats.name == SlugcatStats.Name.Red && !hud.rainWorld.ExpeditionMode)
				{
					cycleLabel = new CycleLabel(this);
				}
				for (int j = 0; j < mapConnections.Count; j++)
				{
					mapConnections[j].InitiateSprites();
				}
				mapSize = new Vector2(mapTexture.width, mapTexture.height / 3);
				panPos = mapSize / 2f;
				Shader.SetGlobalVector(RainWorld.ShadPropMapSize, mapSize);
				if (hud.owner.GetOwnerType() != HUD.OwnerType.RegionOverview && hud.owner.GetOwnerType() != HUD.OwnerType.DeathScreen)
				{
					playerMarkerFade = new FSprite("Futile_White");
					playerMarkerFade.color = new Color(0f, 0f, 0f);
					playerMarkerFade.shader = hud.rainWorld.Shaders["FlatLight"];
					container.AddChild(playerMarkerFade);
					playerMarkerFade.isVisible = false;
					playerMarker = new HUDCircle(hud, HUDCircle.SnapToGraphic.FoodCircleA, container, 1);
				}
				mapLoaded = true;
			}
		}
		if (mapLoaded && !discLoaded)
		{
			if (!hud.rainWorld.setup.cleanMaps && discoverTexture == null && !hud.rainWorld.setup.revealMap)
			{
				hud.rainWorld.progression.LoadMapTexture(mapData.regionName);
			}
			bool flag = false;
			if (discoverTexture != null && (discoverTexture.width != (int)((float)mapTexture.width / DiscoverResolution) || discoverTexture.height != (int)((float)mapTexture.height / DiscoverResolution)))
			{
				Custom.LogWarning($"DISCOVER TEXTURE WAS INVALID; WAS {discoverTexture.width}x{discoverTexture.height}, EXPECTED TO BE {(int)((float)mapTexture.width / DiscoverResolution)}x{(int)((float)mapTexture.height / DiscoverResolution)}! REGENERATING FRESH FROM VISITED ROOMS!");
				flag = true;
			}
			if (discoverTexture == null || flag)
			{
				Custom.LogImportant("CREATING DISCOVERY TEXTURE FRESH FOR", (mapData.regionName == null) ? "NULL" : mapData.regionName);
				if (hud.rainWorld.setup.revealMap)
				{
					discoverTexture = new Texture2D((int)((float)mapTexture.width / DiscoverResolution), (int)((float)mapTexture.height / DiscoverResolution));
					for (int k = 0; k < discoverTexture.width; k++)
					{
						for (int l = 0; l < discoverTexture.height; l++)
						{
							Color pixel = mapTexture.GetPixel(k * (int)DiscoverResolution, l * (int)DiscoverResolution);
							Color pixel2 = mapTexture.GetPixel((k + 1) * (int)DiscoverResolution, (l + 1) * (int)DiscoverResolution);
							if (pixel.g == 1f && pixel.r == 0f && pixel.b == 0f && pixel2.g == 1f && pixel2.r == 0f && pixel2.b == 0f)
							{
								discoverTexture.SetPixel(k, l, new Color(0f, 0f, 0f));
							}
							else
							{
								discoverTexture.SetPixel(k, l, new Color(1f, 0f, 0f));
							}
						}
					}
				}
				else
				{
					CreateDiscoveryTextureFromVisitedRooms();
				}
			}
			if (mapData.regionName != null)
			{
				hud.rainWorld.progression.mapLastUpdatedTime[mapData.regionName] = DateTime.Now.Ticks;
			}
			discLoaded = true;
			revealTexture = new Texture2D(discoverTexture.width, discoverTexture.height);
		}
		lastPanPos = panPos;
		lastFade = fade;
		lastDepth = depth;
		depth = Mathf.Lerp(depth, layer, 0.1f);
		if (depth < (float)layer)
		{
			depth = Mathf.Min(depth + 0.02f, layer);
		}
		else
		{
			depth = Mathf.Max(depth - 0.02f, layer);
		}
		panVel *= Custom.LerpMap(panVel.magnitude, 0f, 2.5f + 4f * Custom.SCurve(Mathf.InverseLerp(0.5f, 1f, speedUp), 0.65f), 1f, 0.85f);
		for (int num = swarmCircles.Count - 1; num >= 0; num--)
		{
			if (swarmCircles[num].life < 0f)
			{
				swarmCircles[num].Destroy();
				swarmCircles.RemoveAt(num);
			}
			else
			{
				swarmCircles[num].Update();
			}
		}
		for (int num2 = mapObjects.Count - 1; num2 >= 0; num2--)
		{
			if (mapObjects[num2].slatedForDeletion)
			{
				mapObjects.RemoveAt(num2);
			}
			else
			{
				mapObjects[num2].Update();
			}
		}
		if (mapLoaded && discLoaded)
		{
			if (ModManager.MMF && MMF.cfgKeyItemTracking.Value)
			{
				for (int m = 0; m < mapData.objectTrackers.Count; m++)
				{
					if (itemMarkers[m] != null)
					{
						AbstractPhysicalObject obj = mapData.objectTrackers[m].obj;
						if (obj != null && obj.Room != null)
						{
							itemMarkers[m].room = obj.Room.index;
						}
						if (!itemMarkers[m].positionSetFromRealized && (obj == null || obj.realizedObject == null))
						{
							IntVector2 tile = mapData.objectTrackers[m].desiredSpawnLocation.Tile;
							itemMarkers[m].inRoomPos = new Vector2(10f + (float)tile.x * 20f, 10f + (float)tile.y * 20f);
						}
						if (obj != null && obj.realizedObject != null)
						{
							itemMarkers[m].inRoomPos = obj.realizedObject.firstChunk.pos;
							itemMarkers[m].positionSetFromRealized = true;
						}
					}
				}
			}
			if (hud.owner.MapDiscoveryActive && discoverTexture != null)
			{
				IntVector2 intVector = IntVector2.FromVector2(OnTexturePos(hud.owner.MapOwnerInRoomPosition, hud.owner.MapOwnerRoom, accountForLayer: true) / DiscoverResolution);
				if (lastOnTexturePos != intVector)
				{
					DiscoverMap(intVector);
				}
				lastOnTexturePos = intVector;
			}
			if (revealPixelsList.Count > 0 && revealTexture != null)
			{
				RevealRoutine();
			}
			if (revealFadePixels.Count > 0)
			{
				FadeRoutine();
			}
			for (int n = 0; n < mapConnections.Count; n++)
			{
				mapConnections[n].Update();
			}
			Vector2 vector = hud.owner.MapInput.analogueDir;
			if (vector.x == 0f && vector.y == 0f && (hud.owner.MapInput.x != 0 || hud.owner.MapInput.y != 0))
			{
				vector = Custom.DirVec(new Vector2(0f, 0f), new Vector2(hud.owner.MapInput.x, hud.owner.MapInput.y));
			}
			if (vector.magnitude > 0.1f)
			{
				speedUp = Mathf.Clamp01(speedUp + Custom.LerpMap(Vector2.Dot(panVel.normalized, vector.normalized) * vector.magnitude, 0.6f, 0.8f, -1f / Custom.LerpMap(speedUp, 0.5f, 1f, 7f, 30f), 1f / 140f));
				panVel += vector * (0.45f + 0.1f * Mathf.InverseLerp(0.5f, 1f, speedUp));
			}
			else
			{
				speedUp = Mathf.Max(0f, speedUp - 1f / 30f);
				panVel *= 0.8f;
			}
			panPos += panVel;
			if (panPos.x < 0f)
			{
				panPos.x = 0f;
			}
			else if (panPos.x > mapSize.x)
			{
				panPos.x = mapSize.x;
			}
			if (panPos.y < 0f)
			{
				panPos.y = 0f;
			}
			else if (panPos.y > mapSize.y)
			{
				panPos.y = mapSize.y;
			}
			if (hud.owner.RevealMap && revealTexture != null && discoverTexture != null)
			{
				fadeCounter += ((!ModManager.MMF || !MMF.cfgFastMapReveal.Value) ? 1 : 2);
				bool jmp = hud.owner.MapInput.jmp;
				bool thrw = hud.owner.MapInput.thrw;
				if (jmp && !layerButtonA)
				{
					layer--;
					if (layer < 0)
					{
						layer = 0;
					}
				}
				if (thrw && !layerButtonB)
				{
					layer++;
					if (layer > 2)
					{
						layer = 2;
					}
				}
				layerButtonA = jmp;
				layerButtonB = thrw;
				if (fadeCounter > 30 || hud.owner.GetOwnerType() != HUD.OwnerType.Player)
				{
					fade = Mathf.Min(fade + 1f / 30f, 1f);
					fade = Mathf.Lerp(fade, 1f, 0.1f);
					if (lastFade == 0f)
					{
						InitiateMapView();
						if (revealAllDiscovered)
						{
							RevealAllDiscovered();
						}
					}
				}
			}
			else
			{
				fadeCounter -= 5;
				if (fadeCounter < 0)
				{
					fadeCounter = 0;
				}
				fade = Mathf.Max(fade - 0.05f, 0f);
				fade = Mathf.Lerp(fade, 0f, 0.1f);
			}
			if (hud.HideGeneralHud)
			{
				fade = 0f;
			}
			visible = fade > 0f && lastFade > 0f;
			if (hud.owner.GetOwnerType() == HUD.OwnerType.Player && revealTexture != null)
			{
				if (fade == 0f)
				{
					if (resetRevealCounter > 0)
					{
						resetRevealCounter -= Custom.IntClamp((int)Vector2.Distance(playerStandPos, OnTexturePos(hud.owner.MapOwnerInRoomPosition, hud.owner.MapOwnerRoom, accountForLayer: true)) / 2, 1, 100);
						if (resetRevealCounter < 1)
						{
							ResetReveal();
							resetRevealCounter = 0;
						}
					}
				}
				else if (fade > 0.5f)
				{
					resetRevealCounter = 100;
				}
			}
			if (playerMarker != null)
			{
				Vector2 pos = RoomToMapPos(hud.owner.MapOwnerInRoomPosition, hud.owner.MapOwnerRoom, 1f);
				if (ModManager.MSC && hud.owner is Creature && (hud.owner as Creature).room != null && (hud.owner as Creature).room.game.rainWorld.safariMode && ((hud.owner as Creature).abstractCreature.abstractAI as OverseerAbstractAI).targetCreature != null && ((hud.owner as Creature).abstractCreature.abstractAI as OverseerAbstractAI).targetCreature.realizedCreature != null)
				{
					pos = RoomToMapPos(((hud.owner as Creature).abstractCreature.abstractAI as OverseerAbstractAI).targetCreature.realizedCreature.firstChunk.pos, ((hud.owner as Creature).abstractCreature.abstractAI as OverseerAbstractAI).targetCreature.Room.index, 1f);
				}
				playerMarker.Update();
				playerMarker.pos = pos;
				playerMarker.fade = fade;
				if (playerMarker.rad > playerMarker.snapRad)
				{
					playerMarker.rad = Mathf.Max(playerMarker.snapRad, playerMarker.rad - 4f);
					playerMarker.rad = Mathf.Lerp(playerMarker.rad, playerMarker.snapRad, 0.005f);
				}
				if (playerMarker.thickness < playerMarker.snapThickness)
				{
					playerMarker.thickness = Mathf.Min(playerMarker.snapThickness, playerMarker.thickness + 0.125f);
				}
			}
			if (UnityEngine.Random.value < (float)mapData.activeSwarmRooms.Length / 20f && fade > 0.95f)
			{
				MapData.SwarmRoomData swarmRoomData = mapData.activeSwarmRooms[UnityEngine.Random.Range(0, mapData.activeSwarmRooms.Length)];
				if (swarmRoomData.active && swarmRoomData.hivePositions.Length != 0 && revealTexture != null)
				{
					Vector2 pos2 = swarmRoomData.hivePositions[UnityEngine.Random.Range(0, swarmRoomData.hivePositions.Length)].ToVector2() * 20f - new Vector2(10f, 10f);
					Vector2 vector2 = RoomToMapPos(pos2, swarmRoomData.roomIndex, 1f);
					if (vector2.x > 0f && vector2.x < hud.rainWorld.screenSize.x && vector2.y > 0f && vector2.y < hud.rainWorld.screenSize.y)
					{
						Vector2 vector3 = OnTexturePos(pos2, swarmRoomData.roomIndex, accountForLayer: true) / DiscoverResolution;
						if (revealTexture.GetPixel((int)vector3.x, (int)vector3.y).r == 1f)
						{
							swarmCircles.Add(new SwarmCircle(this, pos2, swarmRoomData.roomIndex));
						}
					}
				}
			}
		}
		if (fade == 0f && lastFade == 0f && swarmCircles.Count > 0)
		{
			for (int num3 = 0; num3 < swarmCircles.Count; num3++)
			{
				swarmCircles[num3].Destroy();
			}
			swarmCircles.Clear();
		}
		if (cycleLabel != null)
		{
			cycleLabel.Update();
		}
	}

	private void CreateDiscoveryTextureFromVisitedRooms()
	{
		SaveState saveState = null;
		if (hud.owner.GetOwnerType() == HUD.OwnerType.Player || hud.owner.GetOwnerType() == HUD.OwnerType.FastTravelScreen || hud.owner.GetOwnerType() == HUD.OwnerType.RegionOverview || (ModManager.MSC && hud.owner.GetOwnerType() == MoreSlugcatsEnums.OwnerType.SafariOverseer))
		{
			saveState = hud.rainWorld.progression.currentSaveState;
		}
		else if (hud.owner.GetOwnerType() != HUD.OwnerType.RegionOverview)
		{
			saveState = (hud.owner as SleepAndDeathScreen).saveState;
		}
		int num = -1;
		Region[] array = null;
		if (hud.rainWorld.processManager.currentMainLoop is RainWorldGame)
		{
			array = (hud.rainWorld.processManager.currentMainLoop as RainWorldGame).overWorld.regions;
		}
		else if (hud.rainWorld.processManager.currentMainLoop is FastTravelScreen)
		{
			array = (hud.rainWorld.processManager.currentMainLoop as FastTravelScreen).allRegions;
		}
		if (array != null)
		{
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].name == mapData.regionName)
				{
					num = i;
					break;
				}
			}
		}
		discoverTexture = new Texture2D((int)((float)mapTexture.width / DiscoverResolution), (int)((float)mapTexture.height / DiscoverResolution));
		for (int j = 0; j < discoverTexture.width; j++)
		{
			for (int k = 0; k < discoverTexture.height; k++)
			{
				discoverTexture.SetPixel(j, k, new Color(0f, 0f, 0f));
			}
		}
		if (saveState == null || num < 0 || saveState.regionStates[num] == null)
		{
			return;
		}
		for (int l = 0; l < saveState.regionStates[num].roomsVisited.Count; l++)
		{
			for (int m = 0; m < mapData.roomNames.Length; m++)
			{
				if (!(mapData.roomNames[m] == saveState.regionStates[num].roomsVisited[l]))
				{
					continue;
				}
				IntVector2 intVector = IntVector2.FromVector2(OnTexturePos(Vector2.zero, mapData.roomIndices[m], accountForLayer: true) / DiscoverResolution);
				for (int n = 0; (float)n <= (float)mapData.roomSizes[m].x / DiscoverResolution; n++)
				{
					for (int num2 = 0; (float)num2 <= (float)mapData.roomSizes[m].y / DiscoverResolution; num2++)
					{
						discoverTexture.SetPixel(intVector.x + n, intVector.y + num2, new Color(1f, 0f, 0f));
					}
				}
			}
		}
	}

	private void RevealRoutine()
	{
		int num = (int)Custom.LerpMap(revealPixelsList.Count, 1f, 100f, 6f, 1f);
		for (int i = 0; i < num; i++)
		{
			if (revealPixelsList.Count <= 0)
			{
				break;
			}
			int index = UnityEngine.Random.Range(0, revealPixelsList.Count);
			IntVector2 p = revealPixelsList[index];
			revealPixelsList.RemoveAt(index);
			RevealPixel(p);
		}
		if (revealPixelsList.Count > 0)
		{
			int num2 = NextRevealPixelClosestToPan();
			if (num2 > -1)
			{
				IntVector2 p2 = revealPixelsList[num2];
				revealPixelsList.RemoveAt(num2);
				RevealPixel(p2);
			}
		}
	}

	private void RevealPixel(IntVector2 p)
	{
		for (int i = 0; i < 4; i++)
		{
			if (ShouldPixelBeRevealed(p + Custom.fourDirections[i]))
			{
				AddPixelToRevealList(p + Custom.fourDirections[i]);
			}
		}
		for (int j = 0; j < mapConnections.Count; j++)
		{
			IntVector2 intVector = IntVector2.FromVector2(OnTexturePos(mapConnections[j].posInRoomA.ToVector2() * 20f, mapConnections[j].roomA, accountForLayer: true) / DiscoverResolution);
			IntVector2 intVector2 = IntVector2.FromVector2(OnTexturePos(mapConnections[j].posInRoomB.ToVector2() * 20f, mapConnections[j].roomB, accountForLayer: true) / DiscoverResolution);
			if (intVector == p)
			{
				if (ShouldPixelBeRevealed(intVector2))
				{
					AddPixelToRevealList(intVector2);
				}
				if (mapConnections[j].startRevealA < 0)
				{
					mapConnections[j].startRevealA = 1 + revealPixelsList.Count;
					if (mapConnections[j].startRevealB < 0)
					{
						mapConnections[j].direction = 0f;
					}
				}
			}
			else if (intVector2 == p)
			{
				if (ShouldPixelBeRevealed(intVector))
				{
					AddPixelToRevealList(intVector);
				}
				if (mapConnections[j].startRevealB < 0)
				{
					mapConnections[j].startRevealB = 1 + revealPixelsList.Count;
				}
				if (mapConnections[j].startRevealA < 0)
				{
					mapConnections[j].direction = 1f;
				}
			}
		}
		for (int num = notRevealedFadeMarkers.Count - 1; num >= 0; num--)
		{
			if (IntVector2.FromVector2(OnTexturePos(notRevealedFadeMarkers[num].inRoomPos, notRevealedFadeMarkers[num].room, accountForLayer: true) / DiscoverResolution).FloatDist(p) < notRevealedFadeMarkers[num].fadeInRad)
			{
				notRevealedFadeMarkers[num].FadeIn(30 + revealPixelsList.Count);
				notRevealedFadeMarkers.RemoveAt(num);
			}
		}
	}

	private void AddPixelToRevealList(IntVector2 p)
	{
		revealTexture.SetPixel(p.x, p.y, new Color(0.05f, 0f, 0f));
		revealPixelsList.Add(p);
		revealFadePixels.Add(p);
	}

	private bool ShouldPixelBeRevealed(IntVector2 pxl)
	{
		if (discoverTexture == null)
		{
			return false;
		}
		if (discoverTexture.GetPixel(pxl.x, pxl.y).r > 0f)
		{
			return revealTexture.GetPixel(pxl.x, pxl.y).r == 0f;
		}
		return false;
	}

	private int NextRevealPixelClosestToPan()
	{
		float num = float.MaxValue;
		int result = -1;
		for (int i = 0; i < revealPixelsList.Count; i++)
		{
			Vector2 a = new Vector2((float)revealPixelsList[i].x * DiscoverResolution, (float)revealPixelsList[i].y * DiscoverResolution);
			int num2 = Mathf.FloorToInt(a.y / mapSize.y);
			if (num2 == layer)
			{
				a.y -= mapSize.y * (float)num2;
				float num3 = Vector2.Distance(a, panPos);
				if (num3 < num)
				{
					result = i;
					num = num3;
				}
			}
		}
		return result;
	}

	private void FadeRoutine()
	{
		if (discoverTexture == null || revealTexture == null)
		{
			return;
		}
		for (int num = revealFadePixels.Count - 1; num >= 0; num--)
		{
			float r = discoverTexture.GetPixel(revealFadePixels[num].x, revealFadePixels[num].y).r;
			float r2 = revealTexture.GetPixel(revealFadePixels[num].x, revealFadePixels[num].y).r;
			r2 += UnityEngine.Random.value / (float)(1 + ((revealPixelsList != null) ? revealPixelsList.Count : 0));
			revealTexture.SetPixel(revealFadePixels[num].x, revealFadePixels[num].y, new Color(Mathf.Min(r2, r), 0f, 0f));
			if (r2 >= r)
			{
				revealFadePixels.RemoveAt(num);
			}
		}
		revealTexture.Apply();
	}

	private void InitiateMapView()
	{
		if (resetRevealCounter == -1)
		{
			ResetReveal();
			resetRevealCounter = 0;
		}
		if (resetRevealCounter < 1)
		{
			Vector2 mapOwnerInRoomPosition = hud.owner.MapOwnerInRoomPosition;
			int mapOwnerRoom = hud.owner.MapOwnerRoom;
			Vector2 vector = mapData.PositionOfRoom(mapOwnerRoom) / 3f;
			mapOwnerInRoomPosition -= new Vector2((float)mapData.SizeOfRoom(mapOwnerRoom).x * 20f, (float)mapData.SizeOfRoom(mapOwnerRoom).y * 20f) / 2f;
			panPos = vector + mapOwnerInRoomPosition / 20f + new Vector2(10f, 10f);
			lastPanPos = panPos;
			layer = mapData.LayerOfRoom(mapOwnerRoom);
			depth = layer;
			lastDepth = depth;
			if (playerMarker != null)
			{
				playerMarker.rad = 80f;
				playerMarker.thickness = 0f;
			}
		}
		IntVector2 intVector = IntVector2.FromVector2(OnTexturePos(hud.owner.MapOwnerInRoomPosition, hud.owner.MapOwnerRoom, accountForLayer: true) / DiscoverResolution);
		for (int i = 0; i < 9; i++)
		{
			AddPixelToRevealList(intVector + Custom.eightDirectionsAndZero[i]);
		}
		revealTexture.SetPixel(intVector.x, intVector.y, new Color(1f, 0f, 0f));
		for (int j = 0; j < 4; j++)
		{
			if (revealTexture.GetPixel(intVector.x + Custom.fourDirections[j].x, intVector.y + Custom.fourDirections[j].y).r < 0.5f)
			{
				revealTexture.SetPixel(intVector.x + Custom.fourDirections[j].x, intVector.y + Custom.fourDirections[j].y, new Color(0.5f, 0f, 0f));
			}
		}
		for (int k = 0; k < 4; k++)
		{
			if (revealTexture.GetPixel(intVector.x + Custom.fourDirections[k].x, intVector.y + Custom.fourDirections[k].y).r < 0.3f)
			{
				revealTexture.SetPixel(intVector.x + Custom.diagonals[k].x, intVector.y + Custom.diagonals[k].y, new Color(0.3f, 0f, 0f));
			}
		}
		revealTexture.Apply();
		playerStandPos = OnTexturePos(hud.owner.MapOwnerInRoomPosition, hud.owner.MapOwnerRoom, accountForLayer: true);
	}

	private void ResetReveal()
	{
		for (int i = 0; i < revealTexture.width; i++)
		{
			for (int j = 0; j < revealTexture.height; j++)
			{
				revealTexture.SetPixel(i, j, new Color(0f, 0f, 0f));
			}
		}
		revealPixelsList.Clear();
		revealFadePixels.Clear();
		Shader.SetGlobalTexture(RainWorld.ShadPropMapFogTexture, revealTexture);
		for (int k = 0; k < mapConnections.Count; k++)
		{
			mapConnections[k].revealA = 0f;
			mapConnections[k].revealB = 0f;
			mapConnections[k].lastRevealA = 0f;
			mapConnections[k].lastRevealB = 0f;
			mapConnections[k].startRevealA = -1;
			mapConnections[k].startRevealB = -1;
		}
		ResetNotRevealedMarkers();
	}

	public void RemoveKarmaFlower()
	{
		mapData.karmaFlowerPos = null;
		for (int i = 0; i < mapObjects.Count; i++)
		{
			if (mapObjects[i] is FlowerMarker)
			{
				mapObjects[i].Destroy();
			}
		}
	}

	public void RevealAllDiscovered()
	{
		Custom.Log("reveal all!");
		for (int i = 0; i < revealTexture.width; i++)
		{
			for (int j = 0; j < revealTexture.height; j++)
			{
				revealTexture.SetPixel(i, j, new Color(discoverTexture.GetPixel(i, j).r, 0f, 0f));
			}
		}
		revealTexture.Apply();
		revealPixelsList.Clear();
		revealFadePixels.Clear();
		for (int k = 0; k < mapConnections.Count; k++)
		{
			IntVector2 intVector = IntVector2.FromVector2(OnTexturePos(mapConnections[k].posInRoomA.ToVector2() * 20f, mapConnections[k].roomA, accountForLayer: true) / DiscoverResolution);
			IntVector2 intVector2 = IntVector2.FromVector2(OnTexturePos(mapConnections[k].posInRoomB.ToVector2() * 20f, mapConnections[k].roomB, accountForLayer: true) / DiscoverResolution);
			if (discoverTexture.GetPixel(intVector.x, intVector.y).r > 0f && mapConnections[k].startRevealA < 0)
			{
				mapConnections[k].startRevealA = 1;
				if (mapConnections[k].startRevealB < 0)
				{
					mapConnections[k].direction = 0f;
				}
			}
			if (discoverTexture.GetPixel(intVector2.x, intVector2.y).r > 0f)
			{
				if (mapConnections[k].startRevealB < 0)
				{
					mapConnections[k].startRevealB = 1;
				}
				if (mapConnections[k].startRevealA < 0)
				{
					mapConnections[k].direction = 1f;
				}
			}
		}
		for (int l = 0; l < notRevealedFadeMarkers.Count; l++)
		{
			IntVector2 intVector3 = IntVector2.FromVector2(OnTexturePos(notRevealedFadeMarkers[l].inRoomPos, notRevealedFadeMarkers[l].room, accountForLayer: true) / DiscoverResolution);
			if (discoverTexture.GetPixel(intVector3.x, intVector3.y).r > 0f)
			{
				notRevealedFadeMarkers[l].FadeIn(0.1f);
				continue;
			}
			bool flag = false;
			for (int m = -(int)notRevealedFadeMarkers[l].fadeInRad; m < (int)notRevealedFadeMarkers[l].fadeInRad; m++)
			{
				for (int n = -(int)notRevealedFadeMarkers[l].fadeInRad; n < (int)notRevealedFadeMarkers[l].fadeInRad; n++)
				{
					if (Custom.DistLess(new Vector2(0f, 0f), new Vector2(m, n), notRevealedFadeMarkers[l].fadeInRad + 0.1f) && discoverTexture.GetPixel(intVector3.x + m, intVector3.y + n).r > 0f)
					{
						notRevealedFadeMarkers[l].FadeIn(0.1f);
						flag = true;
						break;
					}
				}
				if (flag)
				{
					break;
				}
			}
		}
		Shader.SetGlobalTexture(RainWorld.ShadPropMapFogTexture, revealTexture);
	}

	public void ExternalDiscover(Vector2 pos, int room)
	{
		if (discLoaded)
		{
			DiscoverMap(IntVector2.FromVector2(OnTexturePos(pos, room, accountForLayer: true) / DiscoverResolution));
		}
	}

	public void ExternalSmallDiscover(Vector2 pos, int room)
	{
		if (discLoaded)
		{
			SmallDiscoverMap(IntVector2.FromVector2(OnTexturePos(pos, room, accountForLayer: true) / DiscoverResolution));
		}
	}

	public void ExternalExitDiscover(Vector2 pos, int room)
	{
		if (discLoaded)
		{
			SmallDiscoverMap(IntVector2.FromVector2(OnTexturePos(pos, room, accountForLayer: true) / DiscoverResolution));
			SmallDiscoverMap(IntVector2.FromVector2(OnTexturePos(pos, room, accountForLayer: true) / DiscoverResolution - new Vector2(0.5f, 0.5f)));
		}
	}

	public void ExternalOnePixelDiscover(Vector2 pos, int room)
	{
		if (discLoaded)
		{
			OnePixelDiscoverMap(IntVector2.FromVector2(OnTexturePos(pos, room, accountForLayer: true) / DiscoverResolution));
		}
	}

	private void OnePixelDiscoverMap(IntVector2 texturePos)
	{
		discoverTexture.SetPixel(texturePos.x, texturePos.y, new Color(1f, 1f, 1f));
	}

	private void SmallDiscoverMap(IntVector2 texturePos)
	{
		discoverTexture.SetPixel(texturePos.x, texturePos.y, new Color(1f, 1f, 1f));
		for (int i = 0; i < 4; i++)
		{
			if (UnityEngine.Random.value < 0.5f && discoverTexture.GetPixel(texturePos.x + Custom.fourDirections[i].x, texturePos.y + Custom.fourDirections[i].y).r < 0.25f)
			{
				discoverTexture.SetPixel(texturePos.x + Custom.fourDirections[i].x, texturePos.y + Custom.fourDirections[i].y, new Color(0.25f, 0.25f, 0.25f));
			}
		}
	}

	private void DiscoverMap(IntVector2 texturePos)
	{
		if (hud.owner is Player && (hud.owner as Player).abstractCreature.Room.realizedRoom != null && (hud.owner as Player).abstractCreature.Room.realizedRoom.CompleteDarkness((hud.owner as Player).firstChunk.pos, 0f, 0.95f, checkForPlayers: false))
		{
			SmallDiscoverMap(texturePos);
			return;
		}
		for (int i = 0; i < 5; i++)
		{
			discoverTexture.SetPixel(texturePos.x + Custom.fourDirectionsAndZero[i].x, texturePos.y + Custom.fourDirectionsAndZero[i].y, new Color(1f, 1f, 1f));
		}
		for (int j = 0; j < 4; j++)
		{
			if (discoverTexture.GetPixel(texturePos.x + Custom.diagonals[j].x, texturePos.y + Custom.diagonals[j].y).r < 0.5f)
			{
				discoverTexture.SetPixel(texturePos.x + Custom.diagonals[j].x, texturePos.y + Custom.diagonals[j].y, new Color(0.5f, 0.5f, 0.5f));
			}
		}
	}

	protected ShelterMarker.ItemInShelterMarker.ItemInShelterData? GetItemInShelter(int room, int index)
	{
		if (hud.owner is Player && (hud.owner as Player).room != null)
		{
			return GetItemInShelterFromWorld((hud.owner as Player).room.world, room, index);
		}
		for (int i = 0; i < mapData.shelterData.Length; i++)
		{
			if (mapData.shelterData[i].roomIndex == room)
			{
				if (mapData.shelterData[i].itemsData != null && index < mapData.shelterData[i].itemsData.Count)
				{
					return mapData.shelterData[i].itemsData[index];
				}
				return new ShelterMarker.ItemInShelterMarker.ItemInShelterData(default(IconSymbol.IconSymbolData), new EntityID(-1, -1), -1);
			}
		}
		return null;
	}

	protected static ShelterMarker.ItemInShelterMarker.ItemInShelterData? GetItemInShelterFromWorld(World world, int room, int index)
	{
		AbstractRoom abstractRoom = world.GetAbstractRoom(room);
		if (abstractRoom == null)
		{
			return new ShelterMarker.ItemInShelterMarker.ItemInShelterData(default(IconSymbol.IconSymbolData), new EntityID(-1, -1), -1);
		}
		if (index < abstractRoom.entities.Count && abstractRoom.entities[index] is AbstractPhysicalObject)
		{
			return ShelterMarker.ItemInShelterMarker.ItemInShelterData.DataFromAbstractPhysical(abstractRoom.entities[index] as AbstractPhysicalObject);
		}
		return new ShelterMarker.ItemInShelterMarker.ItemInShelterData(default(IconSymbol.IconSymbolData), new EntityID(-1, -1), -1);
	}

	public override void Draw(float timeStacker)
	{
		if (Futile.atlasManager.GetAtlasWithName("map_" + RegionName) == null || !mapLoaded || !discLoaded)
		{
			return;
		}
		for (int i = 0; i < 3; i++)
		{
			mapSprites[i].isVisible = visible;
		}
		for (int j = 0; j < mapConnections.Count; j++)
		{
			mapConnections[j].lineSprite.isVisible = visible;
			mapConnections[j].dotA.isVisible = visible;
			mapConnections[j].dotB.isVisible = visible;
		}
		if (playerMarker != null)
		{
			playerMarker.visible = visible;
			playerMarker.Draw(timeStacker);
			playerMarkerFade.isVisible = visible;
		}
		backgroundBlack.isVisible = visible;
		for (int k = 0; k < mapObjects.Count; k++)
		{
			mapObjects[k].Draw(timeStacker);
		}
		if (ModManager.MMF && ((hud.owner.GetOwnerType() == HUD.OwnerType.Player && hud.owner is Player) || (ModManager.MSC && hud.rainWorld.safariMode)))
		{
			if (creatureSymbols == null)
			{
				creatureSymbols = new List<CreatureSymbol>();
			}
			for (int l = 0; l < creatureSymbols.Count; l++)
			{
				if (creatureSymbols[l] != null)
				{
					creatureSymbols[l].RemoveSprites();
				}
			}
			if ((MMF.cfgCreatureSense.Value || (ModManager.MSC && hud.rainWorld.safariMode)) && visible && (hud.owner as Creature).room != null)
			{
				for (int m = 0; m < (hud.owner as Creature).room.abstractRoom.creatures.Count; m++)
				{
					AbstractCreature abstractCreature = (hud.owner as Creature).room.abstractRoom.creatures[m];
					if (abstractCreature == null || abstractCreature.realizedCreature == null || abstractCreature.realizedCreature.inShortcut || abstractCreature.realizedCreature == hud.owner)
					{
						continue;
					}
					creatureSymbols.Add(new CreatureSymbol(CreatureSymbol.SymbolDataFromCreature(abstractCreature), inFrontContainer));
					creatureSymbols[creatureSymbols.Count - 1].myColor = CreatureSymbol.ColorOfCreature(CreatureSymbol.SymbolDataFromCreature(abstractCreature));
					if (abstractCreature.IsVoided())
					{
						creatureSymbols[creatureSymbols.Count - 1].myColor = RainWorld.SaturatedGold;
					}
					creatureSymbols[creatureSymbols.Count - 1].Show(showShadowSprites: true);
					creatureSymbols[creatureSymbols.Count - 1].lastShowFlash = 0f;
					creatureSymbols[creatureSymbols.Count - 1].showFlash = 0f;
					float num = 360f;
					num -= num * 0.8f * abstractCreature.Room.realizedRoom.Darkness(abstractCreature.realizedCreature.mainBodyChunk.pos);
					num -= num * 0.4f * abstractCreature.realizedCreature.Submersion;
					Vector2 a = RoomToMapPos(hud.owner.MapOwnerInRoomPosition, hud.owner.MapOwnerRoom, timeStacker);
					creatureSymbols[creatureSymbols.Count - 1].symbolSprite.alpha = Mathf.InverseLerp(1.8f, 0f, (hud.owner as Creature).Submersion) * Mathf.InverseLerp(num, 50f, Vector2.Distance(a, RoomToMapPos(abstractCreature.pos.Tile.ToVector2() * 20f, abstractCreature.Room.index, timeStacker)));
					if (abstractCreature.creatureTemplate.type == CreatureTemplate.Type.Slugcat)
					{
						creatureSymbols[creatureSymbols.Count - 1].myColor = RainWorld.PlayerObjectBodyColors[(abstractCreature.realizedCreature as Player).playerState.playerNumber];
					}
					else if (abstractCreature.creatureTemplate.type == CreatureTemplate.Type.WhiteLizard)
					{
						if (abstractCreature.realizedCreature.graphicsModule != null)
						{
							creatureSymbols[creatureSymbols.Count - 1].symbolSprite.alpha = Mathf.Lerp(creatureSymbols[creatureSymbols.Count - 1].symbolSprite.alpha, 0f, (abstractCreature.realizedCreature.graphicsModule as LizardGraphics).Camouflaged);
						}
						else
						{
							creatureSymbols[creatureSymbols.Count - 1].symbolSprite.alpha = 0f;
						}
					}
					else if (ModManager.MSC && abstractCreature.creatureTemplate.type == MoreSlugcatsEnums.CreatureTemplateType.StowawayBug)
					{
						creatureSymbols[creatureSymbols.Count - 1].symbolSprite.alpha = Mathf.InverseLerp(-1f, 0f, abstractCreature.realizedCreature.VisibilityBonus);
					}
					else if (abstractCreature.creatureTemplate.type == CreatureTemplate.Type.PoleMimic)
					{
						creatureSymbols[creatureSymbols.Count - 1].symbolSprite.alpha = Mathf.Lerp(creatureSymbols[creatureSymbols.Count - 1].symbolSprite.alpha, 0f, (abstractCreature.realizedCreature as PoleMimic).mimic);
					}
					else if (abstractCreature.creatureTemplate.type == CreatureTemplate.Type.DropBug)
					{
						creatureSymbols[creatureSymbols.Count - 1].symbolSprite.alpha = Mathf.Lerp(creatureSymbols[creatureSymbols.Count - 1].symbolSprite.alpha, 0f, (abstractCreature.realizedCreature as DropBug).inCeilingMode);
					}
					else if (abstractCreature.creatureTemplate.type == CreatureTemplate.Type.Spider || abstractCreature.creatureTemplate.type == CreatureTemplate.Type.TempleGuard)
					{
						creatureSymbols[creatureSymbols.Count - 1].symbolSprite.alpha = 0f;
					}
					else if (abstractCreature.creatureTemplate.TopAncestor().type == CreatureTemplate.Type.Centipede && abstractCreature.superSizeMe)
					{
						creatureSymbols[creatureSymbols.Count - 1].myColor = Custom.HSL2RGB(Mathf.Lerp(0.28f, 0.38f, 0.5f), 0.5f, 0.5f);
					}
					else if (abstractCreature.creatureTemplate.type == CreatureTemplate.Type.BrotherLongLegs && (abstractCreature.realizedCreature as DaddyLongLegs).colorClass)
					{
						creatureSymbols[creatureSymbols.Count - 1].myColor = new Color(0f, 0f, 1f);
					}
					if (abstractCreature.creatureTemplate.type == CreatureTemplate.Type.Overseer)
					{
						if (abstractCreature.realizedCreature.graphicsModule != null)
						{
							creatureSymbols[creatureSymbols.Count - 1].myColor = (abstractCreature.realizedCreature.graphicsModule as OverseerGraphics).MainColor;
						}
						creatureSymbols[creatureSymbols.Count - 1].symbolSprite.scale = 0.5f;
					}
					else if (abstractCreature.creatureTemplate.smallCreature)
					{
						creatureSymbols[creatureSymbols.Count - 1].symbolSprite.scale = 0.6f;
					}
					else
					{
						creatureSymbols[creatureSymbols.Count - 1].symbolSprite.scale = 0.9f;
					}
					if (ModManager.MSC && hud.rainWorld.safariMode)
					{
						creatureSymbols[creatureSymbols.Count - 1].symbolSprite.alpha = 1f;
					}
					if (abstractCreature.realizedCreature.dead)
					{
						creatureSymbols[creatureSymbols.Count - 1].symbolSprite.scale *= 0.8f;
						creatureSymbols[creatureSymbols.Count - 1].symbolSprite.rotation = abstractCreature.realizedCreature.mainBodyChunk.pos.GetRadians() * 57.29578f;
						creatureSymbols[creatureSymbols.Count - 1].symbolSprite.alpha *= 0.8f;
					}
					creatureSymbols[creatureSymbols.Count - 1].symbolSprite.alpha *= 1f - abstractCreature.realizedCreature.Submersion * 0.8f;
					creatureSymbols[creatureSymbols.Count - 1].symbolSprite.alpha = Mathf.Min(creatureSymbols[creatureSymbols.Count - 1].symbolSprite.alpha, fade);
					creatureSymbols[creatureSymbols.Count - 1].shadowSprite1.alpha = creatureSymbols[creatureSymbols.Count - 1].symbolSprite.alpha;
					creatureSymbols[creatureSymbols.Count - 1].shadowSprite1.scale = creatureSymbols[creatureSymbols.Count - 1].symbolSprite.scale;
					creatureSymbols[creatureSymbols.Count - 1].shadowSprite2.alpha = creatureSymbols[creatureSymbols.Count - 1].symbolSprite.alpha;
					creatureSymbols[creatureSymbols.Count - 1].shadowSprite2.scale = creatureSymbols[creatureSymbols.Count - 1].symbolSprite.scale;
					creatureSymbols[creatureSymbols.Count - 1].Draw(timeStacker, RoomToMapPos(abstractCreature.realizedCreature.mainBodyChunk.pos, abstractCreature.Room.index, timeStacker));
				}
			}
		}
		if (!visible)
		{
			return;
		}
		for (int n = 0; n < swarmCircles.Count; n++)
		{
			swarmCircles[n].circle.Draw(timeStacker);
		}
		float num2 = Mathf.Lerp(lastFade, fade, timeStacker);
		float num3 = Mathf.Lerp(lastDepth, depth, timeStacker);
		float num4 = 0f;
		float num5 = 1f;
		for (int num6 = 0; num6 < 3; num6++)
		{
			num5 *= 1f - AlphaOfDefaultMaterial(2 - num6, timeStacker);
		}
		num5 = 1f - num5;
		num5 = Mathf.InverseLerp(1.2f, 0.7f, num5);
		for (int num7 = 0; num7 < 3; num7++)
		{
			mapSprites[num7].alpha = Alpha(2 - num7, timeStacker, compensateForLayersInFront: false) * num5;
			if (STANDARDELEMENTLIST[num7])
			{
				num4 = Mathf.Max(num4, 1f - OutOfFocus(2 - num7, timeStacker));
			}
		}
		backgroundBlack.alpha = num4 * 0.5f * Mathf.Lerp(lastFade, fade, timeStacker);
		float num8 = num3 / 2f / 3f;
		mapSprites[0].color = new Color(OutOfFocus(2, timeStacker), num8, 1f);
		mapSprites[1].color = new Color(OutOfFocus(1, timeStacker), 1f / 3f + num8, 0.5f);
		mapSprites[2].color = new Color(OutOfFocus(0, timeStacker), 2f / 3f + num8, 0f);
		for (int num9 = 0; num9 < mapSprites.Length; num9++)
		{
			mapSprites[num9].scaleX = hud.rainWorld.screenSize.x / (float)mapTexture.width;
			mapSprites[num9].scaleY = hud.rainWorld.screenSize.y / (float)mapTexture.height;
			mapSprites[num9].x = hud.rainWorld.screenSize.x / 2f;
			mapSprites[num9].y = hud.rainWorld.screenSize.y / 2f;
			Vector2 vector = Vector2.Lerp(lastPanPos, panPos, timeStacker);
			Shader.SetGlobalVector(RainWorld.ShadPropMapPan, new Vector2(Mathf.InverseLerp(0f, mapSize.x, vector.x), Mathf.InverseLerp(0f, mapSize.y, vector.y)));
		}
		Vector2 vector2 = RoomToMapPos(hud.owner.MapOwnerInRoomPosition, hud.owner.MapOwnerRoom, timeStacker);
		if (playerMarker != null)
		{
			playerMarkerFade.x = (ModManager.MMF ? playerMarker.pos.x : vector2.x);
			playerMarkerFade.y = (ModManager.MMF ? playerMarker.pos.y : vector2.y);
			playerMarkerFade.scale = 20f;
			playerMarkerFade.alpha = 0.5f * Mathf.Pow(num2, 2f);
		}
		for (int num10 = 0; num10 < mapConnections.Count; num10++)
		{
			mapConnections[num10].DrawSprites(timeStacker);
		}
		if (cycleLabel != null)
		{
			cycleLabel.Draw(timeStacker, num2);
		}
	}

	public float Alpha(int layer, float timeStacker, bool compensateForLayersInFront)
	{
		float num = Mathf.Lerp(lastFade, fade, timeStacker) * Custom.LerpMap(Mathf.Abs((float)layer - Mathf.Lerp(lastDepth, depth, timeStacker)), 0.45f, 0.9f, 1f, (layer == 2) ? 0.5f : 0.35f);
		if (compensateForLayersInFront)
		{
			switch (layer)
			{
			case 1:
				num *= Mathf.Lerp(0.3f, 1f, OutOfFocus(0, timeStacker));
				break;
			case 2:
				num *= Mathf.Lerp(0.3f, 1f, Mathf.Min(OutOfFocus(0, timeStacker), OutOfFocus(1, timeStacker)));
				break;
			}
		}
		return num * Mathf.Lerp(lastFade, fade, timeStacker);
	}

	public float OutOfFocus(int layer, float timeStacker)
	{
		return Mathf.InverseLerp(0.45f, 0.9f, Mathf.Abs((float)layer - Mathf.Lerp(lastDepth, depth, timeStacker)));
	}

	public float AlphaOfDefaultMaterial(int layer, float timeStacker)
	{
		return 0.7f * Alpha(layer, timeStacker, compensateForLayersInFront: false) * (1f - OutOfFocus(layer, timeStacker));
	}

	public Vector2 RoomToMapPos(Vector2 pos, int room, float timeStacker)
	{
		Vector2 vector = mapData.PositionOfRoom(room) / 3f + new Vector2(10f, 10f);
		Vector2 vector2 = pos - new Vector2((float)mapData.SizeOfRoom(room).x * 20f, (float)mapData.SizeOfRoom(room).y * 20f) / 2f;
		Vector2 result = vector + vector2 / 20f;
		result -= Vector2.Lerp(lastPanPos, panPos, timeStacker);
		result *= MapScale;
		result.x += hud.rainWorld.screenSize.x / 2f;
		result.y += hud.rainWorld.screenSize.y / 2f;
		Vector2 vector3 = new Vector2(Mathf.InverseLerp(0f, hud.rainWorld.screenSize.x, result.x), Mathf.InverseLerp(0f, hud.rainWorld.screenSize.y, result.y));
		float t = (float)(2 - mapData.LayerOfRoom(room)) / 3f + Mathf.Lerp(lastDepth, depth, timeStacker) / 2f / 3f;
		t = Mathf.Lerp(3.25f, 4.75f, t) / 4f;
		Vector2 vector4 = vector3;
		vector3 -= new Vector2(0.5f, 0.5f);
		vector3 *= t;
		vector3 += new Vector2(0.5f, 0.5f);
		result += new Vector2((vector3.x - vector4.x) * hud.rainWorld.screenSize.x, (vector3.y - vector4.y) * hud.rainWorld.screenSize.y);
		vector3 = new Vector2(Mathf.InverseLerp(0f, hud.rainWorld.screenSize.x, result.x), Mathf.InverseLerp(0f, hud.rainWorld.screenSize.y, result.y));
		float f = Mathf.Clamp(Vector2.Distance(vector3, new Vector2(0.5f, 0.5f)), 0f, 1f);
		Vector2 vector5 = (vector3 - new Vector2(0.5f, 0.5f)).normalized * 1.32f * Mathf.Pow(f, 4.81f);
		vector5.y *= 0.5f;
		vector5.x /= mapSize.x / 500f;
		vector5.y /= mapSize.y / 500f;
		vector5.x *= mapSize.x * t * 4f;
		vector5.y *= mapSize.y * t * 4f;
		result.x += vector5.x;
		result.y += vector5.y;
		return result;
	}

	private Vector2 OnTexturePos(Vector2 pos, int room, bool accountForLayer)
	{
		Vector2 vector = mapData.PositionOfRoom(room) / 3f + new Vector2(10f, 10f);
		Vector2 vector2 = pos - new Vector2((float)mapData.SizeOfRoom(room).x * 20f, (float)mapData.SizeOfRoom(room).y * 20f) / 2f;
		if (accountForLayer)
		{
			vector.y += (float)mapData.LayerOfRoom(room) * mapSize.y;
		}
		return vector + vector2 / 20f;
	}

	private void LoadConnectionPositions()
	{
		mapConnections = new List<OnMapConnection>();
		string path = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + RegionName + Path.DirectorySeparatorChar + "map_" + RegionName + "-" + hud.rainWorld.progression.PlayingAsSlugcat.value + ".txt");
		if (!File.Exists(path))
		{
			path = AssetManager.ResolveFilePath("World" + Path.DirectorySeparatorChar + RegionName + Path.DirectorySeparatorChar + "map_" + RegionName + ".txt");
			if (!File.Exists(path))
			{
				return;
			}
		}
		string[] array = File.ReadAllLines(path);
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = Regex.Split(Custom.ValidateSpacedDelimiter(array[i], ":"), ": ");
			if (array2.Length != 2 || !(array2[0] == "Connection"))
			{
				continue;
			}
			string[] array3 = Regex.Split(array2[1], ",");
			if (array3.Length != 8)
			{
				continue;
			}
			int num = -1;
			int num2 = -1;
			if (array3[0] == "HR_LAYERS_OF_REALITY" || array3[1] == "HR_LAYERS_OF_REALITY")
			{
				continue;
			}
			string item = array3[0] + "," + array3[1];
			string item2 = array3[1] + "," + array3[0];
			if (!mapData.roomConnections.Contains(item) && !mapData.roomConnections.Contains(item2))
			{
				continue;
			}
			for (int j = mapData.firstRoomIndex; j < mapData.firstRoomIndex + mapData.roomSizes.Length; j++)
			{
				if (num >= 0 && num2 >= 0)
				{
					break;
				}
				if (mapData.NameOfRoom(j) == array3[0])
				{
					num = j;
				}
				else if (mapData.NameOfRoom(j) == array3[1])
				{
					num2 = j;
				}
			}
			if (num > 0 && num2 > 0)
			{
				mapConnections.Add(new OnMapConnection(this, num, num2, new IntVector2(int.Parse(array3[2], NumberStyles.Any, CultureInfo.InvariantCulture), int.Parse(array3[3], NumberStyles.Any, CultureInfo.InvariantCulture)), new IntVector2(int.Parse(array3[4], NumberStyles.Any, CultureInfo.InvariantCulture), int.Parse(array3[5], NumberStyles.Any, CultureInfo.InvariantCulture)), int.Parse(array3[6], NumberStyles.Any, CultureInfo.InvariantCulture), int.Parse(array3[7], NumberStyles.Any, CultureInfo.InvariantCulture)));
			}
		}
	}

	public override void ClearSprites()
	{
		if (backgroundBlack != null)
		{
			backgroundBlack.RemoveFromContainer();
		}
		if (mapSprites != null)
		{
			for (int i = 0; i < mapSprites.Length; i++)
			{
				mapSprites[i].RemoveFromContainer();
			}
		}
		if (playerMarker != null)
		{
			playerMarker.ClearSprite();
			playerMarkerFade.RemoveFromContainer();
		}
		for (int j = 0; j < mapConnections.Count; j++)
		{
			if (mapConnections[j].lineSprite != null)
			{
				mapConnections[j].lineSprite.RemoveFromContainer();
				mapConnections[j].dotA.RemoveFromContainer();
				mapConnections[j].dotB.RemoveFromContainer();
			}
		}
		for (int k = 0; k < swarmCircles.Count; k++)
		{
			swarmCircles[k].Destroy();
		}
		swarmCircles.Clear();
		inFrontContainer.RemoveFromContainer();
		for (int l = 0; l < mapObjects.Count; l++)
		{
			mapObjects[l].Destroy();
		}
		if (cycleLabel != null)
		{
			cycleLabel.ClearSprites();
		}
	}

	public void DestroyTextures()
	{
		if (revealTexture != null)
		{
			UnityEngine.Object.Destroy(revealTexture);
			revealTexture = null;
		}
	}

	public void AddDiscoveryTexture(Texture2D t2D)
	{
		Custom.Log($"discover texture! {t2D.width} {t2D.height} {discoverTexture.width} {discoverTexture.height}");
		Color[] pixels = t2D.GetPixels();
		Color[] pixels2 = discoverTexture.GetPixels();
		for (int i = 0; i < pixels.Length && i < pixels2.Length; i++)
		{
			pixels[i] = new Color(Mathf.Max(pixels[i].r, pixels2[i].r), Mathf.Max(pixels[i].g, pixels2[i].g), Mathf.Max(pixels[i].b, pixels2[i].b));
		}
		discoverTexture.SetPixels(pixels);
		discoverTexture.Apply();
	}

	public void addTracker(PersistentObjectTracker tracker)
	{
		if (MMF.cfgKeyItemTracking.Value)
		{
			mapData.objectTrackers.Add(tracker);
			AbstractPhysicalObject obj = tracker.obj;
			if (obj != null)
			{
				ItemMarker item = new ItemMarker(this, obj.Room.index, obj.pos.Tile.ToVector2(), obj);
				itemMarkers.Add(item);
				mapObjects.Add(item);
			}
			else
			{
				itemMarkers.Add(null);
			}
		}
	}

	public void removeTracker(PersistentObjectTracker tracker)
	{
		if (!mapData.objectTrackers.Remove(tracker))
		{
			return;
		}
		for (int i = 0; i < itemMarkers.Count; i++)
		{
			if (itemMarkers[i] != null && itemMarkers[i].obj == tracker.obj)
			{
				mapObjects.Remove(itemMarkers[i]);
				if (itemMarkers[i].symbol != null)
				{
					itemMarkers[i].RemoveSprites();
				}
				itemMarkers.RemoveAt(i);
				break;
			}
		}
	}
}
