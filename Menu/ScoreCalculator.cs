using System.Collections.Generic;
using System.Linq;
using Expedition;
using Menu.Remix;
using RWCustom;
using UnityEngine;

namespace Menu;

public class ScoreCalculator : PositionedMenuObject
{
	public ScoreCalculatorPhase phase = ScoreCalculatorPhase.Setup;

	public MenuLabel totalLabel;

	public MenuLabel sumLabel;

	public FSprite totalDivider;

	public FSprite[] challengeHighlightSprites;

	public int challengesChecked;

	public List<MenuLabel> challengeLabels;

	public float pointTotal;

	public float lastTotal;

	public float lastLastTotal;

	public List<float> pointQueue;

	public List<float> hiddenPointQueue;

	public float counter;

	public float delay;

	public float speedUp = 1f;

	public int[] challengeTypes;

	public float[] challengePoints;

	public FSprite currentIndicator;

	public float multiplier;

	public float lastMultiplier;

	public MenuLabel currentAction;

	public MenuLabel completedChallengesLabel;

	public int burdensChecked;

	public int finalScore;

	public MenuLabel finalScoreLabel;

	public float leftAnchor;

	public float rightAnchor;

	public MenuLabel[] activeBurdenLabels;

	public List<string> activeBurdens;

	public InGameTranslator IGT => Custom.rainWorld.inGameTranslator;

	public ScoreCalculator(Menu menu, MenuObject owner, Vector2 pos)
		: base(menu, owner, pos)
	{
		float[] screenOffsets = Custom.GetScreenOffsets();
		leftAnchor = screenOffsets[0];
		rightAnchor = screenOffsets[1];
		phase = ScoreCalculatorPhase.Setup;
		int count = ExpeditionData.completedChallengeList.Count;
		challengeLabels = new List<MenuLabel>();
		challengesChecked = 0;
		challengeTypes = new int[count];
		challengePoints = new float[count];
		challengeHighlightSprites = new FSprite[count];
		multiplier = 100f;
		lastMultiplier = multiplier;
		activeBurdens = new List<string>();
		for (int i = 0; i < ExpeditionGame.activeUnlocks.Count; i++)
		{
			if (ExpeditionGame.activeUnlocks[i].StartsWith("bur-"))
			{
				activeBurdens.Add(ExpeditionGame.activeUnlocks[i]);
			}
		}
		MenuLabel menuLabel = new MenuLabel(menu, this, IGT.Translate("CHALLENGES"), new Vector2(-200f, 560f), default(Vector2), bigText: true)
		{
			label = 
			{
				alignment = FLabelAlignment.Left,
				shader = menu.manager.rainWorld.Shaders["MenuText"]
			}
		};
		subObjects.Add(menuLabel);
		FSprite fSprite = new FSprite("LinearGradient200");
		fSprite.rotation = 90f;
		fSprite.scaleY = 0.8f;
		fSprite.scaleX = 2f;
		fSprite.SetAnchor(new Vector2(0.5f, 0f));
		fSprite.x = 50f;
		fSprite.y = pos.y + menuLabel.pos.y - 15f;
		fSprite.shader = menu.manager.rainWorld.Shaders["MenuText"];
		Container.AddChild(fSprite);
		for (int j = 0; j < count; j++)
		{
			Challenge challenge = ExpeditionData.completedChallengeList[j];
			challengeHighlightSprites[j] = new FSprite("Futile_White");
			challengeHighlightSprites[j].scaleX = 50f;
			challengeHighlightSprites[j].scaleY = 3.2f;
			challengeHighlightSprites[j].anchorX = 0.2f;
			challengeHighlightSprites[j].shader = menu.manager.rainWorld.Shaders["FlatLight"];
			if (challenge.hidden)
			{
				challenge.revealed = true;
				challenge.UpdateDescription();
			}
			MenuLabel menuLabel2 = new MenuLabel(menu, this, ExpeditionData.completedChallengeList[j].description, menuLabel.pos + new Vector2(0f, -30f - 30f * (float)j), default(Vector2), bigText: true)
			{
				label = 
				{
					alignment = FLabelAlignment.Left
				}
			};
			challengeHighlightSprites[j].x = 20f;
			challengeHighlightSprites[j].y = pos.y + menuLabel2.pos.y;
			challengeHighlightSprites[j].alpha = 0f;
			if (challenge.completed)
			{
				challengePoints[j] = challenge.Points();
				challengeTypes[j] = (challenge.hidden ? 1 : 0);
				menuLabel2.label.color = (challenge.hidden ? new Color(1f, 0.75f, 0f) : new Color(1f, 1f, 1f));
				challengeHighlightSprites[j].color = (challenge.hidden ? new Color(1f, 0.75f, 0f) : new Color(1f, 1f, 1f));
			}
			else
			{
				challengePoints[j] = -(challenge.Points() / 2);
				challengeTypes[j] = 2;
				menuLabel2.label.color = Color.Lerp(challenge.hidden ? new Color(1f, 0.75f, 0f) : new Color(1f, 1f, 1f), new Color(0f, 0f, 0f), 0.6f);
				challengeHighlightSprites[j].color = Color.Lerp(challenge.hidden ? new Color(1f, 0.75f, 0f) : new Color(1f, 1f, 1f), new Color(0f, 0f, 0f), 0.6f);
			}
			challengeLabels.Add(menuLabel2);
			Container.AddChild(challengeHighlightSprites[j]);
		}
		for (int k = 0; k < challengeLabels.Count; k++)
		{
			subObjects.Add(challengeLabels[k]);
		}
		Vector2 vector = challengeLabels.Last().pos;
		if (activeBurdens.Count > 0)
		{
			MenuLabel menuLabel3 = new MenuLabel(menu, this, IGT.Translate("BURDENS"), new Vector2(-200f, vector.y - 90f), default(Vector2), bigText: true)
			{
				label = 
				{
					alignment = FLabelAlignment.Left,
					shader = menu.manager.rainWorld.Shaders["MenuText"]
				}
			};
			subObjects.Add(menuLabel3);
			FSprite fSprite2 = new FSprite("LinearGradient200");
			fSprite2.rotation = 90f;
			fSprite2.scaleY = 0.7f;
			fSprite2.scaleX = 2f;
			fSprite2.SetAnchor(new Vector2(0.5f, 0f));
			fSprite2.x = 50f;
			fSprite2.y = pos.y + menuLabel3.pos.y - 15f;
			fSprite2.shader = menu.manager.rainWorld.Shaders["MenuText"];
			Container.AddChild(fSprite2);
			activeBurdenLabels = new MenuLabel[activeBurdens.Count];
			int num = 0;
			int num2 = 0;
			for (int l = 0; l < activeBurdenLabels.Length; l++)
			{
				activeBurdenLabels[l] = new MenuLabel(menu, this, ExpeditionProgression.BurdenName(activeBurdens[l]), new Vector2(-200f + 120f * (float)num, menuLabel3.pos.y - 35f - 30f * (float)num2), default(Vector2), bigText: true);
				activeBurdenLabels[l].label.alignment = FLabelAlignment.Left;
				activeBurdenLabels[l].label.color = ExpeditionProgression.BurdenMenuColor(activeBurdens[l]);
				activeBurdenLabels[l].label.alpha = 0.4f;
				subObjects.Add(activeBurdenLabels[l]);
				num++;
				if (num == 4)
				{
					num2++;
					num = 0;
				}
			}
		}
		completedChallengesLabel = new MenuLabel(menu, this, IGT.Translate("Completed Challenges"), new Vector2(-200f, vector.y - 40f), default(Vector2), bigText: true);
		completedChallengesLabel.label.alignment = FLabelAlignment.Left;
		completedChallengesLabel.label.color = new Color(0.45f, 0.45f, 0.45f);
		completedChallengesLabel.label.alpha = 0f;
		subObjects.Add(completedChallengesLabel);
		finalScoreLabel = new MenuLabel(menu, this, IGT.Translate("FINAL SCORE:"), new Vector2(-200f, 0f), default(Vector2), bigText: true);
		finalScoreLabel.label.alignment = FLabelAlignment.Left;
		finalScoreLabel.label.shader = menu.manager.rainWorld.Shaders["MenuText"];
		subObjects.Add(finalScoreLabel);
		currentAction = new MenuLabel(menu, this, IGT.Translate("POINTS:"), new Vector2(-200f, 30f), default(Vector2), bigText: true);
		currentAction.label.alignment = FLabelAlignment.Left;
		currentAction.label.color = new Color(0.45f, 0.45f, 0.45f);
		subObjects.Add(currentAction);
		totalLabel = new MenuLabel(menu, this, "0", default(Vector2), default(Vector2), bigText: true);
		totalLabel.label.alignment = FLabelAlignment.Right;
		subObjects.Add(totalLabel);
		totalDivider = new FSprite("LinearGradient200");
		totalDivider.rotation = 90f;
		totalDivider.scaleY = 1.45f;
		totalDivider.scaleX = 2f;
		totalDivider.SetAnchor(new Vector2(0.5f, 0f));
		totalDivider.x = 50f;
		totalDivider.y = pos.y + 15f;
		totalDivider.shader = menu.manager.rainWorld.Shaders["MenuText"];
		Container.AddChild(totalDivider);
		sumLabel = new MenuLabel(menu, this, "0", totalLabel.pos + new Vector2(0f, 30f), default(Vector2), bigText: true);
		sumLabel.label.alignment = FLabelAlignment.Right;
		subObjects.Add(sumLabel);
		currentIndicator = new FSprite("Menu_Symbol_Arrow");
		currentIndicator.rotation = 90f;
		Container.AddChild(currentIndicator);
	}

	public override void Update()
	{
		speedUp = (menu.input.mp ? 0.15f : 0.025f);
		if (phase == ScoreCalculatorPhase.Setup && (menu.manager.fadeSprite == null || menu.manager.fadeSprite.alpha == 0f))
		{
			phase = ScoreCalculatorPhase.Challenges;
		}
		if (phase == ScoreCalculatorPhase.Challenges)
		{
			if (challengesChecked != challengePoints.Length)
			{
				float a = ((challengesChecked == 0) ? 0f : 1f);
				float b = ((challengesChecked == challengePoints.Length) ? 0f : 1f);
				currentAction.label.alpha = ((delay < 1f) ? Mathf.Lerp(a, 1f, Mathf.InverseLerp(0f, 0.35f, delay)) : Mathf.Lerp(1f, b, counter));
				currentIndicator.x = 20f;
				currentIndicator.y = pos.y + challengeLabels[challengesChecked].pos.y;
				currentIndicator.alpha = ((delay < 1f) ? Mathf.Lerp(0f, 1f, Mathf.InverseLerp(0f, 0.35f, delay)) : Mathf.Lerp(1f, 0f, counter));
				challengeHighlightSprites[challengesChecked].alpha = ((delay < 1f) ? Mathf.Lerp(0f, 0.25f, Mathf.InverseLerp(0f, 0.35f, delay)) : Mathf.Lerp(0.25f, 0f, counter));
				sumLabel.label.alpha = ((delay < 1f) ? Mathf.Lerp(0f, 1f, Mathf.InverseLerp(0f, 0.35f, delay)) : Mathf.Lerp(1f, 0f, counter));
				string text = ((challengePoints[challengesChecked] > 0f) ? "+" : "");
				Color color = ((!ExpeditionData.completedChallengeList[challengesChecked].hidden) ? new Color(1f, 1f, 1f) : new Color(1f, 0.75f, 0f));
				sumLabel.label.color = ((challengePoints[challengesChecked] > 0f) ? color : new Color(0.8f, 0f, 0f));
				Color color2 = Color.Lerp(Color.Lerp(color, new Color(0f, 0f, 0f), (challengePoints[challengesChecked] > 0f) ? 0f : 0.6f), (challengePoints[challengesChecked] > 0f) ? color : new Color(0.8f, 0f, 0f), (delay < 1f) ? Mathf.Lerp(0f, 1f, Mathf.InverseLerp(0f, 0.35f, delay)) : Mathf.Lerp(1f, 0f, counter));
				currentIndicator.color = color2;
				challengeHighlightSprites[challengesChecked].color = color2;
				challengeLabels[challengesChecked].label.color = color2;
				sumLabel.text = text + ValueConverter.ConvertToString(Mathf.RoundToInt(Mathf.Lerp(challengePoints[challengesChecked], 0f, Custom.SCurve(counter, 0.3f))));
				sumLabel.pos.x = Mathf.Lerp(-30f, 0f, Custom.SCurve(Mathf.InverseLerp(0f, 0.35f, delay), 0.8f));
				sumLabel.lastPos.x = sumLabel.pos.x;
				delay += 1f * speedUp;
				if (delay >= 1f)
				{
					counter += Mathf.Lerp(1.5f, 0.5f, Mathf.InverseLerp(0f, 200f, Mathf.Abs(challengePoints[challengesChecked]))) * speedUp;
					pointTotal = Mathf.Lerp(lastLastTotal, lastTotal + challengePoints[challengesChecked], Custom.SCurve(counter, 0.3f));
					totalLabel.text = ValueConverter.ConvertToString(Mathf.RoundToInt(pointTotal));
					if (pointTotal == lastTotal + challengePoints[challengesChecked])
					{
						challengesChecked++;
						lastTotal = pointTotal;
						lastLastTotal = lastTotal;
						counter = 0f;
						delay = 0f;
						sumLabel.pos.x = -30f;
						menu.PlaySound(SoundID.MENU_Checkbox_Check);
					}
				}
			}
			else
			{
				currentIndicator.alpha = 0f;
				delay = 0f;
				lastTotal = pointTotal;
				lastLastTotal = lastTotal;
				challengesChecked = 0;
				phase = ScoreCalculatorPhase.Multipliers;
			}
			delay = Mathf.Clamp(delay, 0f, 1f);
			counter = Mathf.Clamp(counter, 0f, 1f);
		}
		if (phase == ScoreCalculatorPhase.Multipliers)
		{
			if (challengesChecked != challengeLabels.Count)
			{
				float a2 = ((challengesChecked == 0) ? 0f : 1f);
				float b2 = ((challengesChecked == challengePoints.Length) ? 0f : 1f);
				currentAction.label.alpha = ((delay < 1f) ? Mathf.Lerp(a2, 1f, Mathf.InverseLerp(0f, 0.35f, delay)) : Mathf.Lerp(1f, b2, counter));
				currentAction.text = IGT.Translate("BONUS:");
				int num = 0;
				for (int i = 0; i < challengesChecked + 1; i++)
				{
					if (ExpeditionData.completedChallengeList[i].completed)
					{
						num++;
					}
				}
				completedChallengesLabel.label.alpha = Mathf.Lerp(a2, 1f, Mathf.InverseLerp(0f, 0.35f, delay));
				completedChallengesLabel.text = menu.Translate("Challenges Completed:") + " " + num;
				currentIndicator.x = 20f;
				currentIndicator.y = pos.y + challengeLabels[challengesChecked].pos.y;
				currentIndicator.alpha = ((delay < 1f) ? Mathf.Lerp(0f, 1f, Mathf.InverseLerp(0f, 0.35f, delay)) : Mathf.Lerp(1f, 0f, counter));
				challengeHighlightSprites[challengesChecked].alpha = ((delay < 1f) ? Mathf.Lerp(0f, 0.25f, Mathf.InverseLerp(0f, 0.35f, delay)) : Mathf.Lerp(0.25f, 0f, counter));
				bool hidden = ExpeditionData.completedChallengeList[challengesChecked].hidden;
				bool completed = ExpeditionData.completedChallengeList[challengesChecked].completed;
				delay += (completed ? 1.3f : 2f) * speedUp;
				Color color3 = ((!hidden) ? new Color(1f, 1f, 1f) : new Color(1f, 0.75f, 0f));
				sumLabel.label.color = new Color(1f, 1f, 1f);
				Color color4 = Color.Lerp(Color.Lerp(color3, new Color(0f, 0f, 0f), (challengePoints[challengesChecked] > 0f) ? 0f : 0.6f), (challengePoints[challengesChecked] > 0f) ? color3 : new Color(0.8f, 0f, 0f), (delay < 1f) ? Mathf.Lerp(0f, 1f, Mathf.InverseLerp(0f, 0.35f, delay)) : Mathf.Lerp(1f, 0f, counter));
				currentIndicator.color = color4;
				challengeHighlightSprites[challengesChecked].color = color4;
				challengeLabels[challengesChecked].label.color = color4;
				multiplier = lastMultiplier + ((!completed) ? 0f : ((num == 1) ? 0f : 10f));
				sumLabel.label.alpha = ((delay < 1f) ? Mathf.Lerp((challengesChecked == 0 || challengesChecked == challengeLabels.Count) ? 0f : 1f, 1f, Mathf.InverseLerp(0f, 0.35f, delay)) : Mathf.Lerp(1f, (challengesChecked == challengeLabels.Count) ? 0f : 1f, counter));
				sumLabel.text = ValueConverter.ConvertToString(Mathf.RoundToInt(Mathf.Lerp(lastMultiplier, multiplier, Custom.SCurve(counter, 0.3f)))) + "%";
				sumLabel.pos.x = Mathf.Lerp((challengesChecked == 0) ? (-30f) : 0f, 0f, Custom.SCurve(Mathf.InverseLerp(0f, 0.35f, delay), 0.8f));
				sumLabel.lastPos.x = sumLabel.pos.x;
				if (delay >= 1f)
				{
					counter += (completed ? 1.5f : 2.2f) * speedUp;
					if (counter >= 1f)
					{
						counter = 0f;
						delay = 0f;
						challengesChecked++;
						lastMultiplier = multiplier;
						menu.PlaySound(SoundID.MENU_Checkbox_Check);
					}
				}
			}
			else
			{
				currentIndicator.alpha = 0f;
				delay = 0f;
				lastTotal = pointTotal;
				lastLastTotal = lastTotal;
				challengesChecked = 0;
				phase = ((activeBurdens.Count > 0) ? ScoreCalculatorPhase.Burdens : ScoreCalculatorPhase.Finalise);
			}
			delay = Mathf.Clamp(delay, 0f, 1f);
			counter = Mathf.Clamp(counter, 0f, 1f);
		}
		if (phase == ScoreCalculatorPhase.Burdens)
		{
			if (burdensChecked != activeBurdenLabels.Length)
			{
				currentIndicator.x = pos.x - leftAnchor + activeBurdenLabels[burdensChecked].pos.x - 15f;
				currentIndicator.y = pos.y + activeBurdenLabels[burdensChecked].pos.y;
				currentIndicator.alpha = ((delay < 1f) ? Mathf.Lerp(0f, 1f, Mathf.InverseLerp(0f, 0.35f, delay)) : Mathf.Lerp(1f, 0f, counter));
				currentIndicator.color = activeBurdenLabels[burdensChecked].label.color;
				sumLabel.label.color = new Color(1f, 1f, 1f);
				activeBurdenLabels[burdensChecked].label.alpha = Mathf.Lerp(0.4f, 1f, Mathf.InverseLerp(0f, 0.35f, delay));
				multiplier = lastMultiplier + ExpeditionProgression.BurdenScoreMultiplier(activeBurdens[burdensChecked]);
				sumLabel.text = ValueConverter.ConvertToString(Mathf.RoundToInt(Mathf.Lerp(lastMultiplier, multiplier, Custom.SCurve(counter, 0.3f)))) + "%";
				delay += 1.3f * speedUp;
				if (delay >= 1f)
				{
					counter += 1.5f * speedUp;
					if (counter >= 1f)
					{
						counter = 0f;
						delay = 0f;
						burdensChecked++;
						lastMultiplier = multiplier;
						menu.PlaySound(SoundID.MENU_Checkbox_Check);
					}
				}
			}
			else
			{
				currentIndicator.alpha = 0f;
				delay = 0f;
				lastTotal = pointTotal;
				lastLastTotal = lastTotal;
				challengesChecked = 0;
				burdensChecked = 0;
				phase = ScoreCalculatorPhase.Finalise;
			}
			delay = Mathf.Clamp(delay, 0f, 1f);
			counter = Mathf.Clamp(counter, 0f, 1f);
		}
		if (phase == ScoreCalculatorPhase.Finalise)
		{
			delay += 1f * speedUp;
			sumLabel.label.alpha = Mathf.Lerp(1f, 0f, counter);
			sumLabel.text = ValueConverter.ConvertToString(Mathf.RoundToInt(Mathf.Lerp(multiplier, 0f, Custom.SCurve(counter, 0.3f)))) + "%";
			if (delay >= 1f)
			{
				counter += 0.7f * speedUp;
				pointTotal = Mathf.Lerp(lastLastTotal, lastTotal * (multiplier / 100f), Custom.SCurve(counter, 0.3f));
				if (pointTotal < 0f)
				{
					pointTotal = 0f;
				}
				totalLabel.text = ValueConverter.ConvertToString(Mathf.RoundToInt(pointTotal));
				currentAction.label.alpha = Mathf.Lerp(1f, 0f, counter);
				if (counter >= 1f)
				{
					lastTotal = pointTotal;
					lastLastTotal = lastTotal;
					counter = 0f;
					delay = 0f;
					menu.PlaySound(SoundID.Slugcat_Ghost_Dissappear);
					finalScore = Mathf.RoundToInt(pointTotal);
					phase = ScoreCalculatorPhase.Done;
				}
			}
		}
		if (phase == ScoreCalculatorPhase.Done)
		{
			totalLabel.label.shader = menu.manager.rainWorld.Shaders["MenuTextGold"];
			finalScoreLabel.label.shader = menu.manager.rainWorld.Shaders["MenuTextGold"];
		}
	}
}
