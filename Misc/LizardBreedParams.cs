using MoreSlugcats;
using UnityEngine;

public class LizardBreedParams : BreedParameters
{
	public struct SpeedMultiplier
	{
		public float speed;

		public float horizontal;

		public float up;

		public float down;

		public SpeedMultiplier(float speed, float horizontal, float up, float down)
		{
			this.speed = speed;
			this.horizontal = horizontal;
			this.up = up;
			this.down = down;
		}

		public static SpeedMultiplier operator +(SpeedMultiplier a, SpeedMultiplier b)
		{
			return new SpeedMultiplier(a.speed + b.speed, a.horizontal + b.horizontal, a.up + b.up, a.down + b.down);
		}

		public static SpeedMultiplier operator /(SpeedMultiplier a, float b)
		{
			return new SpeedMultiplier(a.speed / b, a.horizontal / b, a.up / b, a.down / b);
		}
	}

	public CreatureTemplate.Type template;

	public float toughness;

	public float stunToughness;

	public float biteDamage;

	public float biteDamageChance;

	public float aggressionCurveExponent = 0.925f;

	public float danger;

	public int biteDelay;

	public float baseSpeed;

	public float biteInFront;

	public float biteRadBonus;

	public float biteHomingSpeed;

	public float biteChance;

	public float attemptBiteRadius;

	public float getFreeBiteChance;

	public SpeedMultiplier baseSpeedMultiplier;

	public Color standardColor;

	public int regainFootingCounter;

	public float bodyMass;

	public float bodySizeFac;

	public float bodyLengthFac;

	public float bodyRadFac;

	public float pullDownFac;

	public float floorLeverage;

	public float maxMusclePower;

	public SpeedMultiplier[] terrainSpeeds;

	public float wiggleSpeed;

	public int wiggleDelay;

	public float bodyStiffnes;

	public float swimSpeed;

	public int idleCounterSubtractWhenCloseToIdlePos;

	public float headShieldAngle;

	public bool canExitLounge;

	public bool canExitLoungeWarmUp;

	public float findLoungeDirection;

	public float loungeDistance;

	public int preLoungeCrouch;

	public float preLoungeCrouchMovement;

	public float loungeSpeed;

	public int loungePropulsionFrames;

	public int loungeMaximumFrames;

	public float loungeJumpyness;

	public int loungeDelay;

	public float riskOfDoubleLoungeDelay;

	public int postLoungeStun;

	public float loungeTendensy;

	public float perfectVisionAngle;

	public float periferalVisionAngle;

	public int shakePrey = 100;

	public float biteDominance;

	public float limbSize;

	public float limbThickness;

	public float stepLength;

	public float liftFeet;

	public float feetDown;

	public float noGripSpeed;

	public float limbSpeed;

	public float limbQuickness;

	public int limbGripDelay;

	public bool smoothenLegMovement;

	public float legPairDisplacement;

	public float walkBob;

	public int tailSegments;

	public float tailStiffness;

	public float tailStiffnessDecline;

	public float tailLengthFactor;

	public float tailColorationStart;

	public float tailColorationExponent;

	public float headSize;

	public float neckStiffness;

	public float jawOpenAngle;

	public float jawOpenLowerJawFac;

	public float jawOpenMoveJawsApart;

	public int[] headGraphics;

	public int framesBetweenLookFocusChange;

	public bool tongue;

	public float tongueAttackRange;

	public int tongueWarmUp;

	public int tongueSegments;

	public float tongueChance;

	public float tamingDifficulty = 1f;

	public bool WallClimber
	{
		get
		{
			if (ModManager.MSC && template == MoreSlugcatsEnums.CreatureTemplateType.EelLizard)
			{
				return true;
			}
			if (!(template == CreatureTemplate.Type.BlueLizard))
			{
				return template == CreatureTemplate.Type.WhiteLizard;
			}
			return true;
		}
	}

	public LizardBreedParams(CreatureTemplate.Type template)
	{
		this.template = template;
	}

	public SpeedMultiplier TerrainSpeed(AItile.Accessibility acc)
	{
		return terrainSpeeds[(int)acc];
	}
}
