using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using Menu.Remix;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace Expedition;

public class VistaChallenge : Challenge
{
	public class VistaPoint : UpdatableAndDeletable, IDrawable
	{
		public Vector2 inRoomPos;

		public VistaChallenge vista;

		public LightSource lightSource;

		public Color color;

		public bool collected;

		public bool notify;

		public float phase;

		public float time;

		public float lastTime;

		public VistaPoint(Room room, VistaChallenge vista, Vector2 inRoomPos)
		{
			this.vista = vista;
			base.room = room;
			this.inRoomPos = inRoomPos;
			notify = false;
		}

		public void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
		{
			rCam.ReturnFContainer("HUD").AddChild(sLeaser.sprites[3]);
			rCam.ReturnFContainer("HUD").AddChild(sLeaser.sprites[0]);
			rCam.ReturnFContainer("HUD").AddChild(sLeaser.sprites[1]);
			rCam.ReturnFContainer("HUD").AddChild(sLeaser.sprites[2]);
		}

		public void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
		{
		}

		public void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
		{
			sLeaser.sprites[0].x = inRoomPos.x - rCam.pos.x;
			sLeaser.sprites[0].y = inRoomPos.y - rCam.pos.y;
			sLeaser.sprites[1].x = inRoomPos.x - rCam.pos.x;
			sLeaser.sprites[1].y = inRoomPos.y - rCam.pos.y;
			sLeaser.sprites[2].x = inRoomPos.x - rCam.pos.x;
			sLeaser.sprites[2].y = inRoomPos.y - rCam.pos.y;
			sLeaser.sprites[3].x = inRoomPos.x - rCam.pos.x;
			sLeaser.sprites[3].y = inRoomPos.y - rCam.pos.y;
			if (vista != null && vista.completed)
			{
				sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["Basic"];
				sLeaser.sprites[1].shader = rCam.game.rainWorld.Shaders["Basic"];
				sLeaser.sprites[0].alpha = 0f;
				sLeaser.sprites[1].alpha = 0f;
				sLeaser.sprites[2].alpha = 0f;
				sLeaser.sprites[3].alpha = 0f;
			}
			phase += 0.13f * Time.deltaTime;
			if (phase > 1f)
			{
				phase = 0f;
			}
			color = new HSLColor(phase, 0.85f, 0.75f).rgb;
			sLeaser.sprites[0].scaleX = Mathf.Sin(time / 20f);
			sLeaser.sprites[1].scaleX = Mathf.Sin(time / 20f) * 1.3f;
			sLeaser.sprites[0].y = inRoomPos.y - rCam.pos.y + 3f * Mathf.Sin(time / 20f);
			sLeaser.sprites[1].y = inRoomPos.y - rCam.pos.y + 3f * Mathf.Sin(time / 20f);
			sLeaser.sprites[0].color = color;
			sLeaser.sprites[1].color = color;
			sLeaser.sprites[2].color = color;
			sLeaser.sprites[3].color = new Color(0.01f, 0.01f, 0.01f);
			if (lightSource != null)
			{
				lightSource.color = color;
			}
		}

		public void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
		{
			sLeaser.sprites = new FSprite[4];
			sLeaser.sprites[0] = new FSprite("TravellerB");
			sLeaser.sprites[0].x = inRoomPos.x - rCam.pos.x;
			sLeaser.sprites[0].y = inRoomPos.y - rCam.pos.y;
			sLeaser.sprites[0].scaleX = 1.25f;
			sLeaser.sprites[0].scaleY = 1.25f;
			sLeaser.sprites[0].shader = rCam.game.rainWorld.Shaders["GateHologram"];
			sLeaser.sprites[0].alpha = 0.85f;
			sLeaser.sprites[1] = new FSprite("TravellerB");
			sLeaser.sprites[1].x = inRoomPos.x - rCam.pos.x;
			sLeaser.sprites[1].y = inRoomPos.y - rCam.pos.y;
			sLeaser.sprites[1].scaleX = 1.55f;
			sLeaser.sprites[1].scaleY = 1.55f;
			sLeaser.sprites[1].shader = rCam.game.rainWorld.Shaders["GateHologram"];
			sLeaser.sprites[1].alpha = 0.35f;
			sLeaser.sprites[2] = new FSprite("Futile_White");
			sLeaser.sprites[2].x = inRoomPos.x - rCam.pos.x;
			sLeaser.sprites[2].y = inRoomPos.y - rCam.pos.y;
			sLeaser.sprites[2].scaleX = 15f;
			sLeaser.sprites[2].scaleY = 15f;
			sLeaser.sprites[2].shader = rCam.game.rainWorld.Shaders["FlatLight"];
			sLeaser.sprites[2].alpha = 0.35f;
			sLeaser.sprites[3] = new FSprite("Futile_White");
			sLeaser.sprites[3].x = inRoomPos.x - rCam.pos.x;
			sLeaser.sprites[3].y = inRoomPos.y - rCam.pos.y;
			sLeaser.sprites[3].scaleX = 6f;
			sLeaser.sprites[3].scaleY = 6f;
			sLeaser.sprites[3].shader = rCam.game.rainWorld.Shaders["FlatLight"];
			sLeaser.sprites[3].alpha = 0.3f;
			AddToContainer(sLeaser, rCam, null);
		}

		public override void Update(bool eu)
		{
			base.Update(eu);
			lastTime = time;
			time += 1f;
			if (room.BeingViewed && !notify)
			{
				room.game.cameras[0].hud.textPrompt.AddMessage(ChallengeTools.IGT.Translate("You feel the presence of a vista . . ."), 20, 150, darken: true, hideHud: true);
				notify = true;
			}
			if (lightSource == null)
			{
				lightSource = new LightSource(inRoomPos, environmentalLight: false, new Color(1f, 0.85f, 0.2f), this);
				lightSource.setRad = 130f;
				lightSource.setAlpha = 1f;
				room.AddObject(lightSource);
			}
			if (vista.completed && !collected)
			{
				room.PlaySound(SoundID.SS_AI_Give_The_Mark_Boom, inRoomPos, 1f, 1f);
				for (int i = 0; i < 20; i++)
				{
					room.AddObject(new Spark(inRoomPos, Custom.RNV() * (25f * UnityEngine.Random.value), color, null, 70, 150));
				}
				collected = true;
			}
			else if (vista.completed && collected)
			{
				RemoveFromRoom();
				Destroy();
			}
		}
	}

	public string room;

	public string region;

	public Vector2 location;

	public override void Update()
	{
		base.Update();
		if (completed)
		{
			return;
		}
		for (int i = 0; i < game.Players.Count; i++)
		{
			if (game.Players[i].realizedCreature != null && game.Players[i].realizedCreature.room != null && game.Players[i].realizedCreature.room.abstractRoom.name == room && Vector2.Distance(game.Players[i].realizedCreature.mainBodyChunk.pos, location) < 30f)
			{
				CompleteChallenge();
			}
		}
		if (game.world == null || game.world.activeRooms == null)
		{
			return;
		}
		for (int j = 0; j < game.world.activeRooms.Count; j++)
		{
			if (!(game.world.activeRooms[j].abstractRoom.name == room))
			{
				continue;
			}
			for (int k = 0; k < game.world.activeRooms[j].updateList.Count; k++)
			{
				if (game.world.activeRooms[j].updateList[k] is VistaPoint)
				{
					return;
				}
			}
			ExpLog.Log("SPAWN VISTA");
			game.world.activeRooms[j].AddObject(new VistaPoint(game.world.activeRooms[j], this, location));
			break;
		}
	}

	public override string ChallengeName()
	{
		return ChallengeTools.IGT.Translate("Vista Visiting");
	}

	public override void UpdateDescription()
	{
		description = ChallengeTools.IGT.Translate("Reach the vista point in <region_name>").Replace("<region_name>", ChallengeTools.IGT.Translate(Region.GetRegionFullName(region, ExpeditionData.slugcatPlayer)));
		base.UpdateDescription();
	}

	public override bool Duplicable(Challenge challenge)
	{
		if (challenge is VistaChallenge)
		{
			if ((challenge as VistaChallenge).region != region)
			{
				return true;
			}
			return false;
		}
		return true;
	}

	public override Challenge Generate()
	{
		List<(string, string)> list = new List<(string, string)>();
		foreach (KeyValuePair<string, Dictionary<string, Vector2>> vistaLocation in ChallengeTools.VistaLocations)
		{
			if (!SlugcatStats.SlugcatStoryRegions(ExpeditionData.slugcatPlayer).Contains(vistaLocation.Key))
			{
				continue;
			}
			foreach (KeyValuePair<string, Vector2> item3 in vistaLocation.Value)
			{
				list.Add((vistaLocation.Key, item3.Key));
			}
		}
		(string, string) tuple = list[UnityEngine.Random.Range(0, list.Count)];
		string item = tuple.Item1;
		string item2 = tuple.Item2;
		Vector2 vector = ChallengeTools.VistaLocations[item][item2];
		VistaChallenge vistaChallenge = new VistaChallenge
		{
			region = item,
			room = item2,
			location = vector
		};
		ModifyVistaCandidates(vistaChallenge);
		return vistaChallenge;
	}

	public void ModifyVistaCandidates(VistaChallenge input)
	{
		if (input.room == "GW_E02" && ModManager.MSC && (ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Artificer || ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Spear))
		{
			input.room = "GW_E02_PAST";
			ExpLog.Log("Switch room to past version");
		}
		else if (input.room == "GW_D01" && ModManager.MSC && (ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Artificer || ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Spear))
		{
			input.room = "GW_D01_PAST";
			ExpLog.Log("Switch room to past version");
		}
		else if (input.room == "UW_C02" && ModManager.MSC && ExpeditionData.slugcatPlayer == MoreSlugcatsEnums.SlugcatStatsName.Rivulet)
		{
			input.room = "UW_C02RIV";
			input.location = new Vector2(450f, 1170f);
			ExpLog.Log("Switch room to future version");
		}
	}

	public override int Points()
	{
		return 40;
	}

	public override bool CombatRequired()
	{
		return false;
	}

	public override string ToString()
	{
		return "VistaChallenge" + "~" + region + "><" + room + "><" + ValueConverter.ConvertToString(location.x) + "><" + ValueConverter.ConvertToString(location.y) + "><" + (completed ? "1" : "0") + "><" + (hidden ? "1" : "0") + "><" + (revealed ? "1" : "0");
	}

	public override void FromString(string args)
	{
		try
		{
			string[] array = Regex.Split(args, "><");
			region = array[0];
			room = array[1];
			location = default(Vector2);
			location.x = float.Parse(array[2], NumberStyles.Any, CultureInfo.InvariantCulture);
			location.y = float.Parse(array[3], NumberStyles.Any, CultureInfo.InvariantCulture);
			completed = array[4] == "1";
			hidden = array[5] == "1";
			revealed = array[6] == "1";
			UpdateDescription();
		}
		catch (Exception ex)
		{
			ExpLog.Log("ERROR: VistaChallenge FromString() encountered an error: " + ex.Message);
		}
	}
}
