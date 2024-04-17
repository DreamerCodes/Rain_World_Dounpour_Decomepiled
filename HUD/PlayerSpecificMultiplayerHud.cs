using System;
using System.Collections.Generic;
using RWCustom;
using UnityEngine;

namespace HUD;

public class PlayerSpecificMultiplayerHud : HudPart
{
	public abstract class Part
	{
		public PlayerSpecificMultiplayerHud owner;

		public Vector2 pos;

		public Vector2 lastPos;

		public bool slatedForDeletion;

		public Part(PlayerSpecificMultiplayerHud owner)
		{
			this.owner = owner;
		}

		public virtual void Update()
		{
			lastPos = pos;
		}

		public virtual void Draw(float timeStacker)
		{
		}

		public virtual void ClearSprites()
		{
		}
	}

	public class KillList : Part
	{
		public class SymbolHolder
		{
			public CreatureSymbol symbol;

			public bool slatedForDeletion;

			private KillList owner;

			public float xDist;

			public float lastXDist;

			public float idealXDist;

			public bool shown;

			public int fadeAway = -1;

			public bool InPlace => Mathf.Abs(xDist - idealXDist) < 1f;

			public float Alpha
			{
				get
				{
					if (!shown)
					{
						return 0f;
					}
					return Mathf.InverseLerp(80f, 20f, fadeAway) * owner.owner.DeadFade;
				}
			}

			public Vector2 DrawPos(float timeStacker)
			{
				return owner.owner.cornerPos + new Vector2((float)owner.owner.flip * (40f + Mathf.Lerp(lastXDist, xDist, timeStacker)) + 0.01f, 0.01f);
			}

			public void Show()
			{
				shown = true;
				symbol.Show(showShadowSprites: false);
			}

			public SymbolHolder(KillList owner, IconSymbol.IconSymbolData symbolData, FContainer container)
			{
				this.owner = owner;
				symbol = new CreatureSymbol(symbolData, container);
			}

			public void Update()
			{
				symbol.Update();
				lastXDist = xDist;
				xDist = Custom.LerpAndTick(xDist, idealXDist, 0.09f, 3f);
				if (fadeAway > -1)
				{
					fadeAway++;
				}
				if (fadeAway > 85)
				{
					slatedForDeletion = true;
				}
			}

			public void Draw(float timeStacker)
			{
				symbol.Draw(timeStacker, DrawPos(timeStacker));
				if (symbol.symbolSprite != null)
				{
					symbol.symbolSprite.alpha = Mathf.InverseLerp(80f, 20f, (float)fadeAway + timeStacker) * owner.owner.DeadFade;
				}
			}
		}

		public List<SymbolHolder> symbolHolders;

		public int allSymbolsFadeDelay;

		public int symbolFadeDelay;

		private FSprite shadow;

		public float leftShadowPos;

		public float lastLeftShadowPos;

		public float rightShadowPos;

		public float lastRightShadowPos;

		public float shadowFade;

		public float lastShadowFade;

		private bool sleep;

		public List<IconSymbol.IconSymbolData> toBeAdded;

		public int toBeAddedDelay;

		public KillList(PlayerSpecificMultiplayerHud owner)
			: base(owner)
		{
			symbolHolders = new List<SymbolHolder>();
			shadow = new FSprite("Futile_White");
			shadow.color = new Color(0f, 0f, 0f);
			shadow.shader = owner.hud.rainWorld.Shaders["FlatLight"];
			shadow.alpha = 0f;
			owner.hud.fContainers[0].AddChild(shadow);
			leftShadowPos = owner.cornerPos.x + 50f * (float)owner.flip;
			lastLeftShadowPos = owner.cornerPos.x + 50f * (float)owner.flip;
			rightShadowPos = owner.cornerPos.x + 50f * (float)owner.flip;
			lastRightShadowPos = owner.cornerPos.x + 50f * (float)owner.flip;
			toBeAdded = new List<IconSymbol.IconSymbolData>();
		}

		public void Killing(IconSymbol.IconSymbolData data)
		{
			if (symbolHolders.Count == 0)
			{
				toBeAddedDelay = 80;
			}
			else
			{
				toBeAddedDelay = Math.Max(toBeAddedDelay, 20);
			}
			toBeAdded.Add(data);
		}

		public override void Update()
		{
			base.Update();
			if (toBeAddedDelay > 0)
			{
				toBeAddedDelay--;
			}
			else if (toBeAdded.Count > 0)
			{
				PushKill(toBeAdded[0]);
				toBeAdded.RemoveAt(0);
			}
			if (sleep)
			{
				return;
			}
			lastLeftShadowPos = leftShadowPos;
			lastRightShadowPos = rightShadowPos;
			lastShadowFade = shadowFade;
			float num = owner.cornerPos.x + 30f * (float)owner.flip;
			float num2 = owner.cornerPos.x + 30f * (float)owner.flip;
			float num3 = 0f;
			if (symbolHolders.Count > 0)
			{
				num = symbolHolders[0].DrawPos(1f).x;
				num2 = leftShadowPos;
			}
			for (int num4 = symbolHolders.Count - 1; num4 >= 0; num4--)
			{
				if (symbolHolders[num4].slatedForDeletion)
				{
					symbolHolders[num4].symbol.RemoveSprites();
					symbolHolders.RemoveAt(num4);
				}
				else
				{
					symbolHolders[num4].Update();
					float alpha = symbolHolders[num4].Alpha;
					float x = symbolHolders[num4].DrawPos(1f).x;
					float graphWidth = symbolHolders[num4].symbol.graphWidth;
					if (x - graphWidth / 2f < num)
					{
						num = Mathf.Lerp(num, x - graphWidth / 2f, alpha);
					}
					if (x + graphWidth / 2f > num2)
					{
						num2 = Mathf.Lerp(num2, x + graphWidth / 2f, alpha);
					}
					num3 = Mathf.Max(num3, alpha);
				}
			}
			leftShadowPos = Custom.LerpAndTick(leftShadowPos, num, 0.04f, 2f);
			rightShadowPos = Custom.LerpAndTick(rightShadowPos, num2, 0.04f, 2f);
			if (shadowFade < num3)
			{
				shadowFade = Custom.LerpAndTick(shadowFade, num3, 0.02f, 0.025f);
			}
			else
			{
				shadowFade = Custom.LerpAndTick(shadowFade, num3, 0.12f, 0.1f);
			}
			float num5 = 0f;
			for (int i = 0; i < symbolHolders.Count; i++)
			{
				num5 = ((i != 0 && !symbolHolders[i].shown) ? (num5 + (symbolHolders[i].symbol.graphWidth + 5f) / 2f) : (num5 + (symbolHolders[i].symbol.graphWidth + 5f)));
				symbolHolders[i].idealXDist = num5 - symbolHolders[i].symbol.graphWidth / 2f;
			}
			if (symbolHolders.Count > 0 && !symbolHolders[0].shown)
			{
				for (int num6 = symbolHolders.Count - 1; num6 >= 0; num6--)
				{
					if (!symbolHolders[num6].shown)
					{
						if (num6 == symbolHolders.Count - 1)
						{
							symbolHolders[num6].Show();
						}
						else if (symbolHolders[num6 + 1].InPlace)
						{
							symbolHolders[num6].Show();
						}
						break;
					}
				}
			}
			if (allSymbolsFadeDelay > 0)
			{
				allSymbolsFadeDelay--;
				symbolFadeDelay = 0;
			}
			else if (symbolFadeDelay > 0)
			{
				symbolFadeDelay--;
			}
			else
			{
				for (int num7 = symbolHolders.Count - 1; num7 >= 0; num7--)
				{
					if (symbolHolders[num7].fadeAway < 0)
					{
						symbolHolders[num7].fadeAway = 0;
						break;
					}
				}
				symbolFadeDelay = 10;
			}
			for (int num8 = symbolHolders.Count - 1; num8 >= 10; num8--)
			{
				if (symbolHolders[num8].fadeAway < 0)
				{
					symbolHolders[num8].fadeAway = 0;
				}
			}
			if (symbolHolders.Count == 0 && lastShadowFade == 0f && shadowFade == 0f && allSymbolsFadeDelay == 0)
			{
				sleep = true;
			}
		}

		public void PushKill(IconSymbol.IconSymbolData symbolData)
		{
			symbolHolders.Insert(0, new SymbolHolder(this, symbolData, owner.hud.fContainers[0]));
			allSymbolsFadeDelay = 200;
			sleep = false;
		}

		public override void Draw(float timeStacker)
		{
			base.Draw(timeStacker);
			if (!sleep)
			{
				for (int i = 0; i < symbolHolders.Count; i++)
				{
					symbolHolders[i].Draw(timeStacker);
				}
				shadow.x = (Mathf.Lerp(lastRightShadowPos, rightShadowPos, timeStacker) + Mathf.Lerp(lastLeftShadowPos, leftShadowPos, timeStacker)) / 2f;
				shadow.y = owner.cornerPos.y;
				float num = (Mathf.Lerp(lastRightShadowPos, rightShadowPos, timeStacker) - Mathf.Lerp(lastLeftShadowPos, leftShadowPos, timeStacker)) * 1.8f;
				shadow.scaleX = (40f + num) / 16f;
				shadow.scaleY = (40f + Mathf.Min(40f, num)) / 16f;
				shadow.alpha = 0.25f * Mathf.Lerp(lastShadowFade, shadowFade, timeStacker) * owner.DeadFade;
			}
		}

		public override void ClearSprites()
		{
			base.ClearSprites();
			for (int i = 0; i < symbolHolders.Count; i++)
			{
				symbolHolders[i].symbol.RemoveSprites();
			}
		}
	}

	public class ScoreCounter : Part
	{
		public int currentDisplayScore;

		public FLabel scoreText;

		public FSprite darkGradient;

		public FSprite lightGradient;

		public float alpha;

		public float lastAlpha;

		public int changeCounter;

		public int betweenNumbersCounter;

		public float bump;

		public float lastBump;

		public int remainVisible;

		public int Score => owner.session.ScoreOfPlayer(owner.RealizedPlayer, inHands: false);

		public ScoreCounter(PlayerSpecificMultiplayerHud owner)
			: base(owner)
		{
			darkGradient = new FSprite("Futile_White");
			darkGradient.color = new Color(0f, 0f, 0f);
			darkGradient.shader = owner.hud.rainWorld.Shaders["FlatLight"];
			owner.hud.fContainers[0].AddChild(darkGradient);
			scoreText = new FLabel(Custom.GetDisplayFont(), "0");
			owner.hud.fContainers[0].AddChild(scoreText);
			scoreText.color = PlayerGraphics.DefaultSlugcatColor((owner.abstractPlayer.state as PlayerState).slugcatCharacter);
			lightGradient = new FSprite("Futile_White");
			lightGradient.color = PlayerGraphics.DefaultSlugcatColor((owner.abstractPlayer.state as PlayerState).slugcatCharacter);
			lightGradient.shader = owner.hud.rainWorld.Shaders["FlatLight"];
			owner.hud.fContainers[0].AddChild(lightGradient);
			pos = new Vector2(owner.cornerPos.x + (float)owner.flip * 20f + 0.01f, owner.cornerPos.y + 0.01f);
		}

		public override void Update()
		{
			base.Update();
			lastAlpha = alpha;
			lastBump = bump;
			bump = Custom.LerpAndTick(bump, 0f, 0.04f, 1f / 30f);
			if (currentDisplayScore != Score)
			{
				changeCounter++;
				alpha = Custom.LerpAndTick(alpha, (remainVisible > 0) ? 1f : Mathf.InverseLerp(10f, 50f, changeCounter), 0.06f, 1f / 30f);
				if (changeCounter > 70)
				{
					if (betweenNumbersCounter > 0)
					{
						betweenNumbersCounter--;
					}
					else
					{
						betweenNumbersCounter = ((Math.Abs(currentDisplayScore - Score) < 10) ? 8 : 2);
						if (currentDisplayScore != Score)
						{
							currentDisplayScore += Math.Sign(Score - currentDisplayScore);
							scoreText.text = currentDisplayScore.ToString();
							bump = 1f;
							remainVisible = 20;
							if (currentDisplayScore == Score)
							{
								owner.hud.fadeCircles.Add(new FadeCircle(owner.hud, 10f, 10f, 0.82f, 30f, 4f, pos, owner.hud.fContainers[1]));
								owner.hud.PlaySound(SoundID.HUD_Food_Meter_Fill_Fade_Circle);
							}
						}
					}
				}
			}
			else
			{
				changeCounter = 0;
				betweenNumbersCounter = 0;
				alpha = Custom.LerpAndTick(alpha, ((float)remainVisible > 0f) ? 1f : 0f, 0.04f, 1f / 60f);
			}
			if (owner.RealizedPlayer != null && owner.RealizedPlayer.mapInput.mp)
			{
				remainVisible = Math.Max(remainVisible, 10);
			}
			else if (remainVisible > 0)
			{
				remainVisible--;
			}
		}

		public override void Draw(float timeStacker)
		{
			base.Draw(timeStacker);
			Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker);
			float num = Mathf.Lerp(lastAlpha, alpha, timeStacker) * owner.DeadFade;
			float num2 = Mathf.Lerp(lastBump, bump, timeStacker);
			scoreText.x = vector.x;
			scoreText.y = vector.y;
			scoreText.alpha = num * ((currentDisplayScore < Score && changeCounter % 8 < 4) ? 1f : 0.5f);
			darkGradient.x = vector.x;
			darkGradient.y = vector.y;
			darkGradient.scale = Mathf.Lerp(35f, 40f, num) / 16f;
			darkGradient.alpha = 0.17f * Mathf.Pow(alpha, 2f) + 0.1f * num2 * ((currentDisplayScore < Score && changeCounter % 8 < 4) ? 1f : 0.5f);
			lightGradient.x = vector.x;
			lightGradient.y = vector.y;
			lightGradient.scale = Mathf.Lerp(40f, 50f, Mathf.Pow(num2, 2f)) / 16f;
			lightGradient.alpha = num2 * 0.2f;
		}

		public override void ClearSprites()
		{
			base.ClearSprites();
			scoreText.RemoveFromContainer();
		}
	}

	public class PlayerArrow : Part
	{
		public FSprite arrowSprite;

		public FSprite gradient;

		public FLabel label;

		public int counter;

		public int fadeAwayCounter;

		public float alpha;

		public float lastAlpha;

		public float blink;

		public float lastBlink;

		public PlayerArrow(PlayerSpecificMultiplayerHud owner)
			: base(owner)
		{
			base.owner = owner;
			pos = new Vector2(-1000f, -1000f);
			lastPos = pos;
			gradient = new FSprite("Futile_White");
			gradient.shader = owner.hud.rainWorld.Shaders["FlatLight"];
			if ((owner.abstractPlayer.state as PlayerState).slugcatCharacter != SlugcatStats.Name.Night)
			{
				gradient.color = new Color(0f, 0f, 0f);
			}
			owner.hud.fContainers[0].AddChild(gradient);
			gradient.alpha = 0f;
			gradient.x = -1000f;
			label = new FLabel(Custom.GetFont(), owner.hud.rainWorld.inGameTranslator.Translate("Player") + " " + ((owner.abstractPlayer.state as PlayerState).playerNumber + 1));
			label.color = PlayerGraphics.DefaultSlugcatColor((owner.abstractPlayer.state as PlayerState).slugcatCharacter);
			owner.hud.fContainers[0].AddChild(label);
			label.alpha = 0f;
			label.x = -1000f;
			arrowSprite = new FSprite("Multiplayer_Arrow");
			arrowSprite.color = PlayerGraphics.DefaultSlugcatColor((owner.abstractPlayer.state as PlayerState).slugcatCharacter);
			owner.hud.fContainers[0].AddChild(arrowSprite);
			arrowSprite.alpha = 0f;
			arrowSprite.x = -1000f;
			blink = 1f;
		}

		public override void Update()
		{
			base.Update();
			lastAlpha = alpha;
			lastBlink = blink;
			blink = Mathf.Max(0f, blink - 0.0125f);
			if (owner.camera.room == null || owner.abstractPlayer.Room != owner.camera.room.abstractRoom || owner.RealizedPlayer == null)
			{
				return;
			}
			if (owner.RealizedPlayer.room == null)
			{
				Vector2? vector = owner.camera.game.shortcuts.OnScreenPositionOfInShortCutCreature(owner.camera.room, owner.RealizedPlayer);
				if (vector.HasValue)
				{
					pos = vector.Value - owner.camera.pos;
				}
			}
			else
			{
				pos = Vector2.Lerp(owner.RealizedPlayer.bodyChunks[0].pos, owner.RealizedPlayer.bodyChunks[1].pos, 1f / 3f) + new Vector2(0f, 60f) - owner.camera.pos;
			}
			alpha = Custom.LerpAndTick(alpha, Mathf.InverseLerp(80f, 20f, fadeAwayCounter), 0.08f, 1f / 30f);
			if (owner.RealizedPlayer.input[0].x != 0 || owner.RealizedPlayer.input[0].y != 0 || owner.RealizedPlayer.input[0].jmp || owner.RealizedPlayer.input[0].thrw || owner.RealizedPlayer.input[0].pckp)
			{
				fadeAwayCounter++;
			}
			if (counter > 10 && !Custom.DistLess(owner.RealizedPlayer.firstChunk.lastPos, owner.RealizedPlayer.firstChunk.pos, 3f))
			{
				fadeAwayCounter++;
			}
			if (fadeAwayCounter > 0)
			{
				fadeAwayCounter++;
				if (fadeAwayCounter > 120 && alpha == 0f && lastAlpha == 0f)
				{
					slatedForDeletion = true;
				}
			}
			else if (counter > 200)
			{
				fadeAwayCounter++;
			}
			counter++;
		}

		public override void Draw(float timeStacker)
		{
			Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker) + new Vector2(0.01f, 0.01f);
			float num = Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(lastAlpha, alpha, timeStacker)), 0.7f);
			gradient.x = vector.x;
			gradient.y = vector.y + 10f;
			gradient.scale = Mathf.Lerp(80f, 110f, num) / 16f;
			gradient.alpha = 0.17f * Mathf.Pow(num, 2f);
			arrowSprite.x = vector.x;
			arrowSprite.y = vector.y;
			label.x = vector.x;
			label.y = vector.y + 20f;
			Color color = PlayerGraphics.DefaultSlugcatColor((owner.abstractPlayer.state as PlayerState).slugcatCharacter);
			if (counter % 6 < 2 && lastBlink > 0f)
			{
				color = ((!((owner.abstractPlayer.state as PlayerState).slugcatCharacter == SlugcatStats.Name.White)) ? Color.Lerp(color, new Color(1f, 1f, 1f), Mathf.InverseLerp(0f, 0.5f, Mathf.Lerp(lastBlink, blink, timeStacker))) : Color.Lerp(color, new Color(0.9f, 0.9f, 0.9f), Mathf.InverseLerp(0f, 0.5f, Mathf.Lerp(lastBlink, blink, timeStacker))));
			}
			label.color = color;
			arrowSprite.color = color;
			label.alpha = num;
			arrowSprite.alpha = num;
		}

		public override void ClearSprites()
		{
			base.ClearSprites();
			gradient.RemoveFromContainer();
			arrowSprite.RemoveFromContainer();
			label.RemoveFromContainer();
		}
	}

	public class PlayerDeathBump : Part
	{
		public FSprite symbolSprite;

		public FSprite gradient;

		public float alpha;

		public float lastAlpha;

		public int counter = -20;

		private float blink;

		private float lastBlink;

		public bool removeAsap;

		public bool PlayerHasExplosiveSpearInThem
		{
			get
			{
				if (owner.RealizedPlayer == null)
				{
					return false;
				}
				if (owner.RealizedPlayer.abstractCreature.stuckObjects.Count == 0)
				{
					return false;
				}
				for (int i = 0; i < owner.RealizedPlayer.abstractCreature.stuckObjects.Count; i++)
				{
					if (owner.RealizedPlayer.abstractCreature.stuckObjects[i].A is AbstractSpear && (owner.RealizedPlayer.abstractCreature.stuckObjects[i].A as AbstractSpear).explosive)
					{
						return true;
					}
				}
				return false;
			}
		}

		public void SetPosToPlayer()
		{
			if (owner.RealizedPlayer != null)
			{
				pos = Vector2.Lerp(owner.RealizedPlayer.mainBodyChunk.pos, owner.RealizedPlayer.bodyChunks[1].pos, 0.2f) - owner.camera.pos + new Vector2(0f, 30f);
			}
			pos.x = Mathf.Clamp(pos.x, 30f, owner.camera.sSize.x - 30f);
			pos.y = Mathf.Clamp(pos.y, 30f, owner.camera.sSize.y - 30f);
			lastPos = pos;
		}

		public PlayerDeathBump(PlayerSpecificMultiplayerHud owner)
			: base(owner)
		{
			base.owner = owner;
			pos = owner.cornerPos + new Vector2(40f * (float)owner.flip, 2f);
			SetPosToPlayer();
			gradient = new FSprite("Futile_White");
			gradient.shader = owner.hud.rainWorld.Shaders["FlatLight"];
			if ((owner.abstractPlayer.state as PlayerState).slugcatCharacter != SlugcatStats.Name.Night)
			{
				gradient.color = new Color(0f, 0f, 0f);
			}
			owner.hud.fContainers[0].AddChild(gradient);
			gradient.alpha = 0f;
			gradient.x = -1000f;
			symbolSprite = new FSprite("Multiplayer_Death");
			symbolSprite.color = PlayerGraphics.DefaultSlugcatColor((owner.abstractPlayer.state as PlayerState).slugcatCharacter);
			owner.hud.fContainers[0].AddChild(symbolSprite);
			symbolSprite.alpha = 0f;
			symbolSprite.x = -1000f;
		}

		public override void Update()
		{
			base.Update();
			lastAlpha = alpha;
			lastBlink = blink;
			if (counter < 0)
			{
				SetPosToPlayer();
				if (owner.RealizedPlayer == null || owner.RealizedPlayer.room == null || !owner.RealizedPlayer.room.ViewedByAnyCamera(owner.RealizedPlayer.mainBodyChunk.pos, 200f) || removeAsap || owner.RealizedPlayer.grabbedBy.Count > 0)
				{
					counter = 0;
				}
				else if (Custom.DistLess(owner.RealizedPlayer.bodyChunks[0].pos, owner.RealizedPlayer.bodyChunks[0].lastLastPos, 6f) && Custom.DistLess(owner.RealizedPlayer.bodyChunks[1].pos, owner.RealizedPlayer.bodyChunks[1].lastLastPos, 6f) && !PlayerHasExplosiveSpearInThem)
				{
					counter++;
				}
				if (counter < 0)
				{
					return;
				}
			}
			counter++;
			if (removeAsap)
			{
				counter += 10;
			}
			if (counter < 40)
			{
				alpha = Mathf.Sin(Mathf.InverseLerp(0f, 40f, counter) * (float)Math.PI);
				blink = Custom.LerpAndTick(blink, 1f, 0.07f, 1f / 30f);
				if (counter == 5 && !removeAsap)
				{
					owner.hud.fadeCircles.Add(new FadeCircle(owner.hud, 10f, 10f, 0.82f, 30f, 4f, pos, owner.hud.fContainers[1]));
					owner.hud.PlaySound(SoundID.UI_Multiplayer_Player_Dead_A);
				}
			}
			else if (counter == 40 && !removeAsap)
			{
				FadeCircle fadeCircle = new FadeCircle(owner.hud, 20f, 30f, 0.94f, 60f, 4f, pos, owner.hud.fContainers[1]);
				fadeCircle.alphaMultiply = 0.5f;
				fadeCircle.fadeThickness = false;
				owner.hud.fadeCircles.Add(fadeCircle);
				alpha = 1f;
				blink = 0f;
				owner.hud.PlaySound(SoundID.UI_Multiplayer_Player_Dead_B);
			}
			else if (counter <= 220)
			{
				alpha = Mathf.InverseLerp(220f, 110f, counter);
			}
			else if (counter > 220)
			{
				slatedForDeletion = true;
			}
		}

		public override void Draw(float timeStacker)
		{
			Vector2 vector = Vector2.Lerp(lastPos, pos, timeStacker) + new Vector2(0.01f, 0.01f);
			float num = Mathf.Pow(Mathf.Max(0f, Mathf.Lerp(lastAlpha, alpha, timeStacker)), 0.7f);
			gradient.x = vector.x;
			gradient.y = vector.y + 10f;
			gradient.scale = Mathf.Lerp(80f, 110f, num) / 16f;
			gradient.alpha = 0.17f * Mathf.Pow(num, 2f);
			symbolSprite.x = vector.x;
			symbolSprite.y = Mathf.Min(vector.y + Custom.SCurve(Mathf.InverseLerp(40f, 130f, (float)counter + timeStacker), 0.8f) * 80f, owner.camera.sSize.y - 30f);
			Color color = PlayerGraphics.DefaultSlugcatColor((owner.abstractPlayer.state as PlayerState).slugcatCharacter);
			if (counter % 6 < 2 && lastBlink > 0f)
			{
				color = ((!((owner.abstractPlayer.state as PlayerState).slugcatCharacter == SlugcatStats.Name.White)) ? Color.Lerp(color, new Color(1f, 1f, 1f), Mathf.InverseLerp(0f, 0.5f, Mathf.Lerp(lastBlink, blink, timeStacker))) : Color.Lerp(color, new Color(0.9f, 0.9f, 0.9f), Mathf.InverseLerp(0f, 0.5f, Mathf.Lerp(lastBlink, blink, timeStacker))));
			}
			symbolSprite.color = color;
			symbolSprite.alpha = num;
		}

		public override void ClearSprites()
		{
			base.ClearSprites();
			gradient.RemoveFromContainer();
			symbolSprite.RemoveFromContainer();
		}
	}

	public ArenaGameSession session;

	public AbstractCreature abstractPlayer;

	public int flip = 1;

	public Vector2 cornerPos;

	public PlayerArrow playerArrow;

	public ScoreCounter scoreCounter;

	public KillList killsList;

	public PlayerDeathBump deathBump;

	public int deadCounter = -1;

	public int antiDeathBumpFlicker;

	private List<Part> parts;

	public bool lastDead;

	public Player RealizedPlayer => abstractPlayer.realizedCreature as Player;

	public RoomCamera camera => abstractPlayer.world.game.cameras[0];

	public float DeadFade => Mathf.InverseLerp(40f, 0f, deadCounter);

	public bool PlayerConsideredDead
	{
		get
		{
			if (RealizedPlayer != null && !RealizedPlayer.dead)
			{
				if (RealizedPlayer.dangerGrasp != null)
				{
					return RealizedPlayer.dangerGraspTime > 20;
				}
				return false;
			}
			return true;
		}
	}

	public PlayerSpecificMultiplayerHud(HUD hud, ArenaGameSession session, AbstractCreature abstractPlayer)
		: base(hud)
	{
		this.session = session;
		this.abstractPlayer = abstractPlayer;
		switch ((abstractPlayer.state as PlayerState).playerNumber)
		{
		case 0:
			cornerPos = new Vector2(hud.rainWorld.options.ScreenSize.x - hud.rainWorld.options.SafeScreenOffset.x, 20f + hud.rainWorld.options.SafeScreenOffset.y);
			flip = -1;
			break;
		case 1:
			cornerPos = new Vector2(hud.rainWorld.options.SafeScreenOffset.x, 20f + hud.rainWorld.options.SafeScreenOffset.y);
			flip = 1;
			break;
		case 2:
			cornerPos = new Vector2(hud.rainWorld.options.SafeScreenOffset.x, hud.rainWorld.options.ScreenSize.y - 20f - hud.rainWorld.options.SafeScreenOffset.y);
			flip = 1;
			break;
		case 3:
			cornerPos = new Vector2(hud.rainWorld.options.ScreenSize.x - hud.rainWorld.options.SafeScreenOffset.x, hud.rainWorld.options.ScreenSize.y - 20f - hud.rainWorld.options.SafeScreenOffset.y);
			flip = -1;
			break;
		}
		parts = new List<Part>();
		killsList = new KillList(this);
		parts.Add(killsList);
		playerArrow = new PlayerArrow(this);
		parts.Add(playerArrow);
		scoreCounter = new ScoreCounter(this);
		parts.Add(scoreCounter);
	}

	public override void Update()
	{
		base.Update();
		for (int num = parts.Count - 1; num >= 0; num--)
		{
			if (parts[num].slatedForDeletion)
			{
				if (parts[num] == playerArrow)
				{
					playerArrow = null;
				}
				else if (parts[num] == deathBump)
				{
					deathBump = null;
				}
				parts[num].ClearSprites();
				parts.RemoveAt(num);
			}
			else
			{
				parts[num].Update();
			}
		}
		if (antiDeathBumpFlicker > 0)
		{
			antiDeathBumpFlicker--;
		}
		if (PlayerConsideredDead)
		{
			if (antiDeathBumpFlicker < 1)
			{
				deadCounter++;
				if (deadCounter == 10 && session.SessionStillGoing)
				{
					antiDeathBumpFlicker = 80;
					deathBump = new PlayerDeathBump(this);
					parts.Add(deathBump);
				}
			}
		}
		else if (lastDead && session.SessionStillGoing)
		{
			Custom.Log("revivePlayer");
			antiDeathBumpFlicker = 80;
			if (deathBump != null)
			{
				deathBump.removeAsap = true;
			}
			deadCounter = -1;
			hud.PlaySound(SoundID.UI_Multiplayer_Player_Revive);
			if (RealizedPlayer != null)
			{
				hud.fadeCircles.Add(new FadeCircle(hud, 10f, 10f, 0.82f, 30f, 4f, RealizedPlayer.mainBodyChunk.pos, hud.fContainers[1]));
			}
			if (playerArrow == null)
			{
				playerArrow = new PlayerArrow(this);
				parts.Add(playerArrow);
			}
		}
		lastDead = PlayerConsideredDead;
	}

	public override void Draw(float timeStacker)
	{
		base.Draw(timeStacker);
		for (int i = 0; i < parts.Count; i++)
		{
			parts[i].Draw(timeStacker);
		}
	}

	public override void ClearSprites()
	{
		base.ClearSprites();
		for (int i = 0; i < parts.Count; i++)
		{
			parts[i].ClearSprites();
		}
	}
}
