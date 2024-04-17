using System.Collections.Generic;
using System.IO;
using DevInterface;
using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class FairyParticleRepresentation : PlacedObjectRepresentation
{
	public class FairyParticleControlPanel : Panel, IDevUISignals
	{
		public class FairyControlSlider : Slider
		{
			public FairyControlSlider(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos, string title)
				: base(owner, IDstring, parentNode, pos, title, inheritButton: false, 110f)
			{
			}

			public override void Refresh()
			{
				base.Refresh();
				float num = 0f;
				float nubPos = 0f;
				bool flag = false;
				bool flag2 = false;
				if (IDstring == "ScaleMin_Slider")
				{
					num = ((parentNode.parentNode as FairyParticleRepresentation).pObj.data as PlacedObject.FairyParticleData).scaleMin;
					nubPos = num / 25f;
				}
				else if (IDstring == "ScaleMax_Slider")
				{
					num = ((parentNode.parentNode as FairyParticleRepresentation).pObj.data as PlacedObject.FairyParticleData).scaleMax;
					nubPos = num / 25f;
				}
				else if (IDstring == "DirMin_Slider")
				{
					num = ((parentNode.parentNode as FairyParticleRepresentation).pObj.data as PlacedObject.FairyParticleData).dirMin;
					nubPos = num / 360f;
				}
				else if (IDstring == "DirMax_Slider")
				{
					num = ((parentNode.parentNode as FairyParticleRepresentation).pObj.data as PlacedObject.FairyParticleData).dirMax;
					nubPos = num / 360f;
				}
				else if (IDstring == "DirDevMin_Slider")
				{
					num = ((parentNode.parentNode as FairyParticleRepresentation).pObj.data as PlacedObject.FairyParticleData).dirDevMin;
					nubPos = num / 360f;
				}
				else if (IDstring == "DirDevMax_Slider")
				{
					num = ((parentNode.parentNode as FairyParticleRepresentation).pObj.data as PlacedObject.FairyParticleData).dirDevMax;
					nubPos = num / 360f;
				}
				else if (IDstring == "ColorHmin_Slider")
				{
					num = ((parentNode.parentNode as FairyParticleRepresentation).pObj.data as PlacedObject.FairyParticleData).colorHmin;
					nubPos = num;
					flag = true;
				}
				else if (IDstring == "ColorSmin_Slider")
				{
					num = ((parentNode.parentNode as FairyParticleRepresentation).pObj.data as PlacedObject.FairyParticleData).colorSmin;
					nubPos = num;
					flag = true;
				}
				else if (IDstring == "ColorLmin_Slider")
				{
					num = ((parentNode.parentNode as FairyParticleRepresentation).pObj.data as PlacedObject.FairyParticleData).colorLmin;
					nubPos = num;
					flag = true;
				}
				else if (IDstring == "ColorHmax_Slider")
				{
					num = ((parentNode.parentNode as FairyParticleRepresentation).pObj.data as PlacedObject.FairyParticleData).colorHmax;
					nubPos = num;
					flag = true;
				}
				else if (IDstring == "ColorSmax_Slider")
				{
					num = ((parentNode.parentNode as FairyParticleRepresentation).pObj.data as PlacedObject.FairyParticleData).colorSmax;
					nubPos = num;
					flag = true;
				}
				else if (IDstring == "ColorLmax_Slider")
				{
					num = ((parentNode.parentNode as FairyParticleRepresentation).pObj.data as PlacedObject.FairyParticleData).colorLmax;
					nubPos = num;
					flag = true;
				}
				else if (IDstring == "AlphaTrans_Slider")
				{
					num = ((parentNode.parentNode as FairyParticleRepresentation).pObj.data as PlacedObject.FairyParticleData).alphaTrans;
					nubPos = num;
					flag = true;
				}
				else if (IDstring == "Keyframes_Slider")
				{
					num = ((parentNode.parentNode as FairyParticleRepresentation).pObj.data as PlacedObject.FairyParticleData).numKeyframes;
					nubPos = num / 10f;
				}
				else if (IDstring == "InterpDistMin_Slider")
				{
					num = ((parentNode.parentNode as FairyParticleRepresentation).pObj.data as PlacedObject.FairyParticleData).interpDistMin;
					nubPos = num / 1000f;
				}
				else if (IDstring == "InterpDistMax_Slider")
				{
					num = ((parentNode.parentNode as FairyParticleRepresentation).pObj.data as PlacedObject.FairyParticleData).interpDistMax;
					nubPos = num / 1000f;
				}
				else if (IDstring == "InterpDurMin_Slider")
				{
					num = ((parentNode.parentNode as FairyParticleRepresentation).pObj.data as PlacedObject.FairyParticleData).interpDurMin;
					nubPos = num / 500f;
				}
				else if (IDstring == "InterpDurMax_Slider")
				{
					num = ((parentNode.parentNode as FairyParticleRepresentation).pObj.data as PlacedObject.FairyParticleData).interpDurMax;
					nubPos = num / 500f;
				}
				else if (IDstring == "InterpTrans_Slider")
				{
					num = ((parentNode.parentNode as FairyParticleRepresentation).pObj.data as PlacedObject.FairyParticleData).interpTrans;
					nubPos = num;
					flag = true;
				}
				else if (IDstring == "PulseMin_Slider")
				{
					num = ((parentNode.parentNode as FairyParticleRepresentation).pObj.data as PlacedObject.FairyParticleData).pulseMin;
					nubPos = num / 50f;
				}
				else if (IDstring == "PulseMax_Slider")
				{
					num = ((parentNode.parentNode as FairyParticleRepresentation).pObj.data as PlacedObject.FairyParticleData).pulseMax;
					nubPos = num / 50f;
				}
				else if (IDstring == "PulseRate_Slider")
				{
					num = ((parentNode.parentNode as FairyParticleRepresentation).pObj.data as PlacedObject.FairyParticleData).pulseRate;
					nubPos = num;
					flag = true;
				}
				else if (IDstring == "GlowRad_Slider")
				{
					num = ((parentNode.parentNode as FairyParticleRepresentation).pObj.data as PlacedObject.FairyParticleData).glowRad;
					nubPos = num / 200f;
				}
				else if (IDstring == "GlowStrength_Slider")
				{
					num = ((parentNode.parentNode as FairyParticleRepresentation).pObj.data as PlacedObject.FairyParticleData).glowStrength;
					nubPos = num;
					flag = true;
				}
				else if (IDstring == "Rotation_Slider")
				{
					num = ((parentNode.parentNode as FairyParticleRepresentation).pObj.data as PlacedObject.FairyParticleData).rotationRate;
					nubPos = num / 20f;
				}
				if (flag)
				{
					base.NumberText = (int)(num * 100f) + "%";
				}
				else if (flag2)
				{
					base.NumberText = num.ToString("0.00");
				}
				else
				{
					base.NumberText = ((int)num).ToString();
				}
				RefreshNubPos(nubPos);
			}

			public override void NubDragged(float nubPos)
			{
				if (IDstring == "ScaleMin_Slider")
				{
					((parentNode.parentNode as FairyParticleRepresentation).pObj.data as PlacedObject.FairyParticleData).scaleMin = nubPos * 24f + 1f;
				}
				else if (IDstring == "ScaleMax_Slider")
				{
					((parentNode.parentNode as FairyParticleRepresentation).pObj.data as PlacedObject.FairyParticleData).scaleMax = nubPos * 24f + 1f;
				}
				else if (IDstring == "DirMin_Slider")
				{
					((parentNode.parentNode as FairyParticleRepresentation).pObj.data as PlacedObject.FairyParticleData).dirMin = nubPos * 360f;
				}
				else if (IDstring == "DirMax_Slider")
				{
					((parentNode.parentNode as FairyParticleRepresentation).pObj.data as PlacedObject.FairyParticleData).dirMax = nubPos * 360f;
				}
				else if (IDstring == "DirDevMin_Slider")
				{
					((parentNode.parentNode as FairyParticleRepresentation).pObj.data as PlacedObject.FairyParticleData).dirDevMin = nubPos * 360f;
				}
				else if (IDstring == "DirDevMax_Slider")
				{
					((parentNode.parentNode as FairyParticleRepresentation).pObj.data as PlacedObject.FairyParticleData).dirDevMax = nubPos * 360f;
				}
				else if (IDstring == "ColorHmin_Slider")
				{
					((parentNode.parentNode as FairyParticleRepresentation).pObj.data as PlacedObject.FairyParticleData).colorHmin = nubPos;
				}
				else if (IDstring == "ColorSmin_Slider")
				{
					((parentNode.parentNode as FairyParticleRepresentation).pObj.data as PlacedObject.FairyParticleData).colorSmin = nubPos;
				}
				else if (IDstring == "ColorLmin_Slider")
				{
					((parentNode.parentNode as FairyParticleRepresentation).pObj.data as PlacedObject.FairyParticleData).colorLmin = nubPos;
				}
				else if (IDstring == "ColorHmax_Slider")
				{
					((parentNode.parentNode as FairyParticleRepresentation).pObj.data as PlacedObject.FairyParticleData).colorHmax = nubPos;
				}
				else if (IDstring == "ColorSmax_Slider")
				{
					((parentNode.parentNode as FairyParticleRepresentation).pObj.data as PlacedObject.FairyParticleData).colorSmax = nubPos;
				}
				else if (IDstring == "ColorLmax_Slider")
				{
					((parentNode.parentNode as FairyParticleRepresentation).pObj.data as PlacedObject.FairyParticleData).colorLmax = nubPos;
				}
				else if (IDstring == "AlphaTrans_Slider")
				{
					((parentNode.parentNode as FairyParticleRepresentation).pObj.data as PlacedObject.FairyParticleData).alphaTrans = nubPos;
				}
				else if (IDstring == "Keyframes_Slider")
				{
					((parentNode.parentNode as FairyParticleRepresentation).pObj.data as PlacedObject.FairyParticleData).numKeyframes = nubPos * 9f + 1f;
				}
				else if (IDstring == "InterpDistMin_Slider")
				{
					((parentNode.parentNode as FairyParticleRepresentation).pObj.data as PlacedObject.FairyParticleData).interpDistMin = nubPos * 1000f;
				}
				else if (IDstring == "InterpDistMax_Slider")
				{
					((parentNode.parentNode as FairyParticleRepresentation).pObj.data as PlacedObject.FairyParticleData).interpDistMax = nubPos * 1000f;
				}
				else if (IDstring == "InterpDurMin_Slider")
				{
					((parentNode.parentNode as FairyParticleRepresentation).pObj.data as PlacedObject.FairyParticleData).interpDurMin = nubPos * 500f;
				}
				else if (IDstring == "InterpDurMax_Slider")
				{
					((parentNode.parentNode as FairyParticleRepresentation).pObj.data as PlacedObject.FairyParticleData).interpDurMax = nubPos * 500f;
				}
				else if (IDstring == "InterpTrans_Slider")
				{
					((parentNode.parentNode as FairyParticleRepresentation).pObj.data as PlacedObject.FairyParticleData).interpTrans = nubPos;
				}
				else if (IDstring == "PulseMin_Slider")
				{
					((parentNode.parentNode as FairyParticleRepresentation).pObj.data as PlacedObject.FairyParticleData).pulseMin = nubPos * 50f;
				}
				else if (IDstring == "PulseMax_Slider")
				{
					((parentNode.parentNode as FairyParticleRepresentation).pObj.data as PlacedObject.FairyParticleData).pulseMax = nubPos * 50f;
				}
				else if (IDstring == "PulseRate_Slider")
				{
					((parentNode.parentNode as FairyParticleRepresentation).pObj.data as PlacedObject.FairyParticleData).pulseRate = nubPos;
				}
				else if (IDstring == "GlowRad_Slider")
				{
					((parentNode.parentNode as FairyParticleRepresentation).pObj.data as PlacedObject.FairyParticleData).glowRad = nubPos * 200f;
				}
				else if (IDstring == "GlowStrength_Slider")
				{
					((parentNode.parentNode as FairyParticleRepresentation).pObj.data as PlacedObject.FairyParticleData).glowStrength = nubPos;
				}
				else if (IDstring == "Rotation_Slider")
				{
					((parentNode.parentNode as FairyParticleRepresentation).pObj.data as PlacedObject.FairyParticleData).rotationRate = nubPos * 20f;
				}
				((parentNode.parentNode as FairyParticleRepresentation).pObj.data as PlacedObject.FairyParticleData).Apply((parentNode.parentNode as FairyParticleRepresentation).owner.room);
				parentNode.parentNode.Refresh();
				Refresh();
			}
		}

		public SelectPresetPanel presetsSelectPanel;

		public FairyParticleControlPanel(DevUI owner, string IDstring, DevUINode parentNode, Vector2 pos)
			: base(owner, IDstring, parentNode, pos, new Vector2(500f, 320f), "Fairy Particle Settings")
		{
			float num = 5f;
			float num2 = 5f;
			subNodes.Add(new Button(owner, "Save_Button", this, new Vector2(num2, num), 100f, "Save Preset"));
			num += 20f;
			subNodes.Add(new Button(owner, "Sprite_Button", this, new Vector2(num2, num), 150f, "Sprite: " + ((parentNode as FairyParticleRepresentation).pObj.data as PlacedObject.FairyParticleData).spriteType.ToString()));
			num += 20f;
			subNodes.Add(new FairyControlSlider(owner, "ScaleMin_Slider", this, new Vector2(num2, num), "Scale Min: "));
			num += 20f;
			subNodes.Add(new FairyControlSlider(owner, "ScaleMax_Slider", this, new Vector2(num2, num), "Scale Max: "));
			num += 20f;
			subNodes.Add(new FairyControlSlider(owner, "DirMin_Slider", this, new Vector2(num2, num), "Direction Min: "));
			num += 20f;
			subNodes.Add(new FairyControlSlider(owner, "DirMax_Slider", this, new Vector2(num2, num), "Direction Max: "));
			num += 20f;
			subNodes.Add(new FairyControlSlider(owner, "DirDevMin_Slider", this, new Vector2(num2, num), "Dir. Deviation Min: "));
			num += 20f;
			subNodes.Add(new FairyControlSlider(owner, "DirDevMax_Slider", this, new Vector2(num2, num), "Dir. Deviation Max: "));
			num += 20f;
			subNodes.Add(new Button(owner, "DirLerp_Button", this, new Vector2(num2, num), 150f, "Dir. Lerp: " + ((parentNode as FairyParticleRepresentation).pObj.data as PlacedObject.FairyParticleData).dirLerpType.ToString()));
			num += 20f;
			subNodes.Add(new FairyControlSlider(owner, "ColorLmin_Slider", this, new Vector2(num2, num), "Color L (Min): "));
			num += 20f;
			subNodes.Add(new FairyControlSlider(owner, "ColorSmin_Slider", this, new Vector2(num2, num), "Color S (Min): "));
			num += 20f;
			subNodes.Add(new FairyControlSlider(owner, "ColorHmin_Slider", this, new Vector2(num2, num), "Color H (Min): "));
			num += 20f;
			subNodes.Add(new FairyControlSlider(owner, "ColorLmax_Slider", this, new Vector2(num2, num), "Color L (Max): "));
			num += 20f;
			subNodes.Add(new FairyControlSlider(owner, "ColorSmax_Slider", this, new Vector2(num2, num), "Color S (Max): "));
			num += 20f;
			subNodes.Add(new FairyControlSlider(owner, "ColorHmax_Slider", this, new Vector2(num2, num), "Color H (Max): "));
			num += 20f;
			subNodes.Add(new FairyControlSlider(owner, "Rotation_Slider", this, new Vector2(num2, num), "Rotation Rate: "));
			num = 5f;
			num2 += 250f;
			subNodes.Add(new Button(owner, "Load_Button", this, new Vector2(num2, num), 100f, "Load Preset"));
			num += 20f;
			subNodes.Add(new FairyControlSlider(owner, "AlphaTrans_Slider", this, new Vector2(num2, num), "Alpha Trans Ratio: "));
			num += 20f;
			subNodes.Add(new FairyControlSlider(owner, "Keyframes_Slider", this, new Vector2(num2, num), "Num Keyframes: "));
			num += 20f;
			subNodes.Add(new Button(owner, "SpeedLerp_Button", this, new Vector2(num2, num), 150f, "Speed Lerp: " + ((parentNode as FairyParticleRepresentation).pObj.data as PlacedObject.FairyParticleData).speedLerpType.ToString()));
			num += 20f;
			subNodes.Add(new FairyControlSlider(owner, "InterpDistMin_Slider", this, new Vector2(num2, num), "Interp Dist Min: "));
			num += 20f;
			subNodes.Add(new FairyControlSlider(owner, "InterpDistMax_Slider", this, new Vector2(num2, num), "Interp Dist Max: "));
			num += 20f;
			subNodes.Add(new FairyControlSlider(owner, "InterpDurMin_Slider", this, new Vector2(num2, num), "Interp Duration Min: "));
			num += 20f;
			subNodes.Add(new FairyControlSlider(owner, "InterpDurMax_Slider", this, new Vector2(num2, num), "Interp Duration Min: "));
			num += 20f;
			subNodes.Add(new FairyControlSlider(owner, "InterpTrans_Slider", this, new Vector2(num2, num), "Interp Trans Ratio: "));
			num += 20f;
			subNodes.Add(new FairyControlSlider(owner, "PulseMin_Slider", this, new Vector2(num2, num), "Pulse Min: "));
			num += 20f;
			subNodes.Add(new FairyControlSlider(owner, "PulseMax_Slider", this, new Vector2(num2, num), "Pulse Max: "));
			num += 20f;
			subNodes.Add(new FairyControlSlider(owner, "PulseRate_Slider", this, new Vector2(num2, num), "Pulse Rate: "));
			num += 20f;
			subNodes.Add(new Button(owner, "PulseAbs_Button", this, new Vector2(num2, num), 150f, ((parentNode as FairyParticleRepresentation).pObj.data as PlacedObject.FairyParticleData).absPulse ? "ABS Pulse: ON" : "ABS Pulse: OFF"));
			num += 20f;
			subNodes.Add(new FairyControlSlider(owner, "GlowRad_Slider", this, new Vector2(num2, num), "Glow Size: "));
			num += 20f;
			subNodes.Add(new FairyControlSlider(owner, "GlowStrength_Slider", this, new Vector2(num2, num), "Glow Strength: "));
		}

		public override void Move(Vector2 newPos)
		{
			base.Move(newPos);
			parentNode.Refresh();
		}

		public override void Refresh()
		{
			PlacedObject.FairyParticleData fairyParticleData = (parentNode as FairyParticleRepresentation).pObj.data as PlacedObject.FairyParticleData;
			foreach (DevUINode subNode in subNodes)
			{
				if (subNode is Button)
				{
					if ((subNode as Button).IDstring == "Sprite_Button")
					{
						(subNode as Button).Text = "Sprite: " + fairyParticleData.spriteType.ToString();
					}
					else if ((subNode as Button).IDstring == "DirLerp_Button")
					{
						(subNode as Button).Text = "Dir. Lerp: " + fairyParticleData.dirLerpType.ToString();
					}
					else if ((subNode as Button).IDstring == "SpeedLerp_Button")
					{
						(subNode as Button).Text = "Speed Lerp: " + fairyParticleData.speedLerpType.ToString();
					}
					else if ((subNode as Button).IDstring == "PulseAbs_Button")
					{
						(subNode as Button).Text = (fairyParticleData.absPulse ? "ABS Pulse: ON" : "ABS Pulse: OFF");
					}
				}
			}
			base.Refresh();
		}

		public void Signal(DevUISignalType type, DevUINode sender, string message)
		{
			PlacedObject.FairyParticleData fairyParticleData = (parentNode as FairyParticleRepresentation).pObj.data as PlacedObject.FairyParticleData;
			if (sender.IDstring == "Sprite_Button")
			{
				int num = fairyParticleData.spriteType.Index + 1;
				if (num >= ExtEnum<PlacedObject.FairyParticleData.SpriteType>.values.Count)
				{
					num = 0;
				}
				fairyParticleData.spriteType = new PlacedObject.FairyParticleData.SpriteType(ExtEnum<PlacedObject.FairyParticleData.SpriteType>.values.GetEntry(num));
				(sender as Button).Text = "Sprite: " + fairyParticleData.spriteType.ToString();
			}
			else if (sender.IDstring == "DirLerp_Button")
			{
				int num2 = fairyParticleData.dirLerpType.Index + 1;
				if (num2 >= ExtEnum<FairyParticle.LerpMethod>.values.Count)
				{
					num2 = 0;
				}
				fairyParticleData.dirLerpType = new FairyParticle.LerpMethod(ExtEnum<FairyParticle.LerpMethod>.values.GetEntry(num2));
				(sender as Button).Text = "Dir. Lerp: " + fairyParticleData.dirLerpType.ToString();
			}
			else if (sender.IDstring == "SpeedLerp_Button")
			{
				int num3 = fairyParticleData.speedLerpType.Index + 1;
				if (num3 >= ExtEnum<FairyParticle.LerpMethod>.values.Count)
				{
					num3 = 0;
				}
				fairyParticleData.speedLerpType = new FairyParticle.LerpMethod(ExtEnum<FairyParticle.LerpMethod>.values.GetEntry(num3));
				(sender as Button).Text = "Speed Lerp: " + fairyParticleData.speedLerpType.ToString();
			}
			else if (sender.IDstring == "PulseAbs_Button")
			{
				fairyParticleData.absPulse = !fairyParticleData.absPulse;
				(sender as Button).Text = (fairyParticleData.absPulse ? "ABS Pulse: ON" : "ABS Pulse: OFF");
			}
			else if (sender.IDstring == "Save_Button")
			{
				string text = Custom.RootFolderDirectory() + (Path.DirectorySeparatorChar + "FairyPresets").ToLowerInvariant();
				if (!Directory.Exists(text))
				{
					Directory.CreateDirectory(text);
				}
				int num4 = 0;
				while (File.Exists(text + Path.DirectorySeparatorChar + "preset" + num4 + ".txt"))
				{
					num4++;
				}
				File.WriteAllText(text + Path.DirectorySeparatorChar + "preset" + num4 + ".txt", fairyParticleData.ToString());
				(parentNode as FairyParticleRepresentation).RefreshLoadPresets();
			}
			else if (sender.IDstring == "Load_Button")
			{
				if (presetsSelectPanel != null)
				{
					subNodes.Remove(presetsSelectPanel);
					presetsSelectPanel.ClearSprites();
					presetsSelectPanel = null;
				}
				else
				{
					string[] presetFiles = (parentNode as FairyParticleRepresentation).presetFiles;
					foreach (string text2 in presetFiles)
					{
						if (!File.Exists(AssetManager.ResolveFilePath("FairyPresets" + Path.DirectorySeparatorChar + text2 + ".txt")))
						{
							(parentNode as FairyParticleRepresentation).RefreshLoadPresets();
							break;
						}
					}
					presetsSelectPanel = new SelectPresetPanel(owner, this, new Vector2(200f, 15f) - absPos, (parentNode as FairyParticleRepresentation).presetFiles);
					subNodes.Add(presetsSelectPanel);
				}
			}
			else
			{
				(parentNode as FairyParticleRepresentation).LoadPreset(sender.IDstring);
				if (presetsSelectPanel != null)
				{
					subNodes.Remove(presetsSelectPanel);
					presetsSelectPanel.ClearSprites();
					presetsSelectPanel = null;
				}
			}
			((parentNode as FairyParticleRepresentation).pObj.data as PlacedObject.FairyParticleData).Apply((parentNode as FairyParticleRepresentation).owner.room);
		}
	}

	public class SelectPresetPanel : Panel
	{
		public SelectPresetPanel(DevUI owner, DevUINode parentNode, Vector2 pos, string[] presetNames)
			: base(owner, "Select_Preset_Panel", parentNode, pos, new Vector2(20f + (float)Mathf.RoundToInt((float)presetNames.Length / 32f + 0.5f) * 150f, 350f), "Select preset")
		{
			IntVector2 intVector = new IntVector2(0, 0);
			for (int i = 0; i < presetNames.Length; i++)
			{
				subNodes.Add(new Button(owner, presetNames[i], this, new Vector2(5f + (float)intVector.x * 150f, size.y - 25f - 20f * (float)intVector.y), 145f, presetNames[i]));
				intVector.y++;
				if (intVector.y > 16)
				{
					intVector.x++;
					intVector.y = 0;
				}
			}
		}
	}

	public string[] presetFiles;

	public FairyParticleRepresentation(DevUI owner, string IDstring, DevUINode parentNode, PlacedObject pObj, string name)
		: base(owner, IDstring, parentNode, pObj, name)
	{
		subNodes.Add(new FairyParticleControlPanel(owner, "FairyParticle_Control_Panel", this, new Vector2(10f, 10f)));
		(subNodes[subNodes.Count - 1] as FairyParticleControlPanel).pos = (pObj.data as PlacedObject.FairyParticleData).panelPos;
		RefreshLoadPresets();
		(pObj.data as PlacedObject.FairyParticleData).Apply(owner.room);
	}

	public override void Refresh()
	{
		base.Refresh();
		(pObj.data as PlacedObject.FairyParticleData).panelPos = (subNodes[0] as Panel).pos;
	}

	public void RefreshLoadPresets()
	{
		presetFiles = new string[0];
		if (!Directory.Exists(AssetManager.ResolveDirectory("FairyPresets")))
		{
			return;
		}
		string[] array = AssetManager.ListDirectory("FairyPresets");
		List<string> list = new List<string>();
		for (int i = 0; i < array.Length; i++)
		{
			if (Path.GetExtension(array[i]) == ".txt")
			{
				list.Add(Path.GetFileName(array[i]));
			}
		}
		presetFiles = list.ToArray();
	}

	public void LoadPreset(string fileName)
	{
		string path = AssetManager.ResolveFilePath("FairyPresets" + Path.DirectorySeparatorChar + fileName);
		if (!File.Exists(path))
		{
			return;
		}
		string s = File.ReadAllText(path).Trim();
		(pObj.data as PlacedObject.FairyParticleData).FromString(s);
		foreach (DevUINode subNode in subNodes[0].subNodes)
		{
			subNode.Refresh();
		}
		subNodes[0].Refresh();
		(pObj.data as PlacedObject.FairyParticleData).Apply(owner.room);
	}
}
