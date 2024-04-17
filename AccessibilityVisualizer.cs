using System.Collections.Generic;
using RWCustom;
using UnityEngine;

public class AccessibilityVisualizer
{
	private struct SpriteAndPos
	{
		public FSprite sprite;

		public IntVector2 pos;

		public bool doubleAccessible;

		public SpriteAndPos(FSprite sprite, IntVector2 pos, bool doubleAccessible)
		{
			this.sprite = sprite;
			this.pos = pos;
			this.doubleAccessible = doubleAccessible;
		}
	}

	private class Mapper
	{
		private AccessibilityVisualizer accVis;

		private AImap aiMap;

		private List<IntVector2> checkNext;

		private List<IntVector2> alreadyChecked;

		private CreatureTemplate crit;

		private IntVector2 initPos;

		private int pass;

		public Mapper(AccessibilityVisualizer accVis, AImap aiMap, IntVector2 initPos, CreatureTemplate crit)
		{
			this.accVis = accVis;
			this.aiMap = aiMap;
			this.initPos = initPos;
			this.crit = crit;
			pass = 0;
			NewPass();
		}

		public void Update()
		{
			if (checkNext.Count == 0)
			{
				if (pass != 0)
				{
					return;
				}
				pass++;
				NewPass();
			}
			IntVector2 pos = checkNext[0];
			checkNext.RemoveAt(0);
			accVis.MarkSpriteAccessible(pos, pass > 0);
			if (pass == 0)
			{
				foreach (MovementConnection outgoingPath in aiMap.getAItile(pos).outgoingPaths)
				{
					if (aiMap.IsConnectionAllowedForCreature(outgoingPath, crit) && !alreadyChecked.Contains(outgoingPath.DestTile))
					{
						checkNext.Add(outgoingPath.DestTile);
						alreadyChecked.Add(outgoingPath.DestTile);
					}
				}
				return;
			}
			foreach (MovementConnection incomingPath in aiMap.getAItile(pos).incomingPaths)
			{
				if (aiMap.IsConnectionAllowedForCreature(incomingPath, crit) && !alreadyChecked.Contains(incomingPath.StartTile))
				{
					checkNext.Add(incomingPath.StartTile);
					alreadyChecked.Add(incomingPath.StartTile);
				}
			}
		}

		private void NewPass()
		{
			checkNext = new List<IntVector2> { initPos };
			alreadyChecked = new List<IntVector2> { initPos };
		}
	}

	public RainWorldGame game;

	private FLabel creatureSelector;

	private FSprite cursor;

	private int selectedCrit;

	private Room room;

	private Mapper mapper;

	private IntVector2 clickedPos;

	private bool upArrow;

	private bool downArrow;

	private bool click;

	private List<SpriteAndPos> sprites;

	public AccessibilityVisualizer(RainWorldGame game)
	{
		this.game = game;
		creatureSelector = new FLabel(Custom.GetFont(), "");
		creatureSelector.color = new Color(1f, 1f, 0f);
		creatureSelector.x = 80f;
		creatureSelector.y = 550f;
		Futile.stage.AddChild(creatureSelector);
		cursor = new FSprite("pixel");
		cursor.color = new Color(1f, 1f, 0f);
		Futile.stage.AddChild(cursor);
		sprites = new List<SpriteAndPos>();
		UpdateCreatureSelectorText();
	}

	public void Update()
	{
		if (room != game.cameras[0].room)
		{
			mapper = null;
			ClearSprites();
			room = game.cameras[0].room;
		}
		if (mapper != null)
		{
			for (int i = 0; i < 10; i++)
			{
				mapper.Update();
			}
		}
		IntVector2 tilePosition = room.GetTilePosition((Vector2)Futile.mousePosition + game.cameras[0].pos);
		cursor.x = (float)tilePosition.x * 20f - game.cameras[0].pos.x + 10f;
		cursor.y = (float)tilePosition.y * 20f - game.cameras[0].pos.y + 10f;
		for (int j = 0; j < sprites.Count; j++)
		{
			sprites[j].sprite.x = (float)sprites[j].pos.x * 20f - game.cameras[0].pos.x + 10f;
			sprites[j].sprite.y = (float)sprites[j].pos.y * 20f - game.cameras[0].pos.y + 10f;
			if (sprites[j].pos == clickedPos || (room.shortCutsReady && room.GetTile(sprites[j].pos).Terrain == Room.Tile.TerrainType.ShortcutEntrance && room.shortcutData(sprites[j].pos).ToNode))
			{
				sprites[j].sprite.alpha = Mathf.Lerp(0.4f, 0.6f, Random.value);
				sprites[j].sprite.scale = Mathf.Lerp(16f, 20f, Random.value);
			}
			else
			{
				sprites[j].sprite.scale = (sprites[j].doubleAccessible ? 20f : 10f);
				sprites[j].sprite.alpha = (sprites[j].doubleAccessible ? 0.5f : 0.3f);
			}
		}
		if (room.readyForAI && room.aimap.TileAccessibleToCreature(tilePosition, StaticWorld.creatureTemplates[selectedCrit]))
		{
			cursor.scale = 20f;
			cursor.alpha = ((Random.value < 0.5f) ? 0.75f : 0.5f);
			if (Input.GetMouseButton(0) && !click)
			{
				InitMapping(tilePosition);
			}
		}
		else
		{
			cursor.scale = 10f;
			cursor.alpha = 0.25f;
			if (Input.GetMouseButton(0) && !click)
			{
				ClearSprites();
			}
		}
		if (!upArrow && Input.GetKey("up"))
		{
			selectedCrit--;
			if (selectedCrit < 0)
			{
				selectedCrit = StaticWorld.creatureTemplates.Length - 1;
			}
			UpdateCreatureSelectorText();
		}
		if (!downArrow && Input.GetKey("down"))
		{
			selectedCrit++;
			if (selectedCrit >= StaticWorld.creatureTemplates.Length)
			{
				selectedCrit = 0;
			}
			UpdateCreatureSelectorText();
		}
		upArrow = Input.GetKey("up");
		downArrow = Input.GetKey("down");
		click = Input.GetMouseButton(0);
	}

	private void InitMapping(IntVector2 pos)
	{
		ClearSprites();
		if (room.readyForAI)
		{
			mapper = new Mapper(this, room.aimap, pos, StaticWorld.creatureTemplates[selectedCrit]);
			clickedPos = pos;
		}
	}

	private void UpdateCreatureSelectorText()
	{
		string text = "";
		for (int i = 0; i < StaticWorld.creatureTemplates.Length; i++)
		{
			if (i > selectedCrit - 4 && i < selectedCrit + 4)
			{
				text = ((i != selectedCrit) ? (text + StaticWorld.creatureTemplates[i].name + ((i < StaticWorld.creatureTemplates.Length - 1) ? "\r\n" : "")) : (text + "<" + StaticWorld.creatureTemplates[i].name + ">" + ((i < StaticWorld.creatureTemplates.Length - 1) ? "\r\n" : "")));
			}
		}
		creatureSelector.text = text;
	}

	public void Destroy()
	{
		ClearSprites();
		creatureSelector.RemoveFromContainer();
		cursor.RemoveFromContainer();
	}

	public void ClearSprites()
	{
		for (int i = 0; i < sprites.Count; i++)
		{
			sprites[i].sprite.RemoveFromContainer();
		}
		sprites.Clear();
	}

	public void MarkSpriteAccessible(IntVector2 pos, bool secondPass)
	{
		if (!secondPass)
		{
			FSprite fSprite = new FSprite("pixel");
			fSprite.color = new Color(1f, 1f, 0f);
			fSprite.alpha = 0.5f;
			fSprite.scale = 20f;
			Futile.stage.AddChild(fSprite);
			sprites.Add(new SpriteAndPos(fSprite, pos, doubleAccessible: false));
			return;
		}
		for (int i = 0; i < sprites.Count; i++)
		{
			if (sprites[i].pos == pos)
			{
				sprites[i] = new SpriteAndPos(sprites[i].sprite, pos, doubleAccessible: true);
				break;
			}
		}
	}
}
