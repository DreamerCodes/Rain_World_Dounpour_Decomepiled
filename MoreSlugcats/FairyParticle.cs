using RWCustom;
using UnityEngine;

namespace MoreSlugcats;

public class FairyParticle : CosmeticSprite
{
	public class LerpMethod : ExtEnum<LerpMethod>
	{
		public static readonly LerpMethod SIN_IO = new LerpMethod("SIN_IO", register: true);

		public static readonly LerpMethod LINEAR = new LerpMethod("LINEAR", register: true);

		public static readonly LerpMethod QUAD_IN = new LerpMethod("QUAD_IN", register: true);

		public static readonly LerpMethod QUAD_OUT = new LerpMethod("QUAD_OUT", register: true);

		public static readonly LerpMethod EXP = new LerpMethod("EXP", register: true);

		public LerpMethod(string value, bool register = false)
			: base(value, register)
		{
		}
	}

	private Vector2 dir;

	private Vector2 lastLastPos;

	private LightSource light;

	public Color col;

	private Color skyColor;

	public float depth;

	public float pulse_min;

	public float pulse_max;

	public float pulse_rate;

	public bool abs_pulse;

	public float scale_min;

	public float scale_max;

	public float scale_multiplier;

	public string spriteName;

	public int num_keyframes;

	public float interp_trans_ratio;

	public float alpha_trans_ratio;

	public float interp_duration_min;

	public float interp_duration_max;

	public float interp_dist_min;

	public float interp_dist_max;

	public float dir_deviation_min;

	public float dir_deviation_max;

	public float direction_min;

	public float direction_max;

	public float glowRadius;

	public float glowIntensity;

	private float ticker;

	private int pulse_ticker;

	private float local_speed;

	private int keyframe_ind;

	private float[] keyframe_dir;

	private float[] keyframe_dur;

	private float[] keyframe_speed;

	private float alpha;

	private float direction;

	private bool resetScale;

	public bool reset;

	public bool resetSprite;

	private float rotation;

	private float z_rotation;

	public float rotation_rate;

	public Vector3 minHSL;

	public Vector3 maxHSL;

	public LerpMethod interp_dir_method;

	public LerpMethod interp_speed_method;

	public bool InPlayLayer => depth == 0f;

	public FairyParticle(float direction, int num_keyframes, float interp_duration_min, float interp_duration_max, float interp_dist_min, float interp_dist_max, float dir_deviation_min, float dir_deviation_max)
	{
		ticker = 0f;
		pulse_ticker = 0;
		local_speed = 0f;
		pulse_min = 1f;
		pulse_max = 1f;
		pulse_rate = 0f;
		abs_pulse = false;
		scale_min = 1f;
		scale_max = 4f;
		glowRadius = 80f;
		glowIntensity = 0.5f;
		num_keyframes = 1;
		interp_trans_ratio = 0.5f;
		alpha_trans_ratio = 0.75f;
		direction_min = 0f;
		direction_max = 360f;
		spriteName = "pixel";
		scale_multiplier = 1f;
		rotation_rate = 0f;
		this.num_keyframes = num_keyframes;
		keyframe_ind = 1;
		keyframe_dir = new float[num_keyframes + 2];
		keyframe_dur = new float[num_keyframes + 2];
		keyframe_speed = new float[num_keyframes + 2];
		this.direction = direction;
		this.interp_duration_max = interp_duration_max;
		this.interp_duration_min = interp_duration_min;
		this.interp_dist_min = interp_dist_min;
		this.interp_dist_max = interp_dist_max;
		this.dir_deviation_max = dir_deviation_max;
		this.dir_deviation_min = dir_deviation_min;
		minHSL = new Vector3(0.5f, 1f, 0.5f);
		maxHSL = new Vector3(0.7f, 1f, 1f);
		resetDepth();
		ResetMe();
	}

	public void ResetMe()
	{
		ResetNoPositionChange();
		ResetPosition();
	}

	public void ResetNoPositionChange()
	{
		ticker = 0f;
		pulse_ticker = 0;
		direction = Random.Range(direction_min, direction_max);
		resetScale = true;
		resetSprite = true;
		resetKeyFrames();
		if (rotation_rate != 0f)
		{
			rotation = Random.Range(0, 360);
			z_rotation = Random.Range(0, 360);
		}
		else
		{
			rotation = 0f;
			z_rotation = 0f;
		}
	}

	public void ResetPosition()
	{
		reset = true;
	}

	public void resetKeyFrames()
	{
		keyframe_ind = 1;
		keyframe_dir = new float[num_keyframes + 2];
		keyframe_dur = new float[num_keyframes + 2];
		keyframe_speed = new float[num_keyframes + 2];
		keyframe_dir[0] = direction;
		for (int i = 1; i <= num_keyframes; i++)
		{
			float num = Random.Range(interp_dist_min, interp_dist_max);
			float num2 = (float)((Random.value < 0.5f) ? 1 : (-1)) * Random.Range(dir_deviation_min, dir_deviation_max);
			keyframe_dir[i] = keyframe_dir[i - 1] + num2;
			keyframe_dur[i] = Random.Range(interp_duration_min, interp_duration_max);
			keyframe_speed[i] = num / keyframe_dur[i];
		}
		keyframe_speed[0] = keyframe_speed[1];
		keyframe_speed[num_keyframes + 1] = keyframe_speed[num_keyframes];
		keyframe_dir[num_keyframes + 1] = keyframe_dir[num_keyframes];
		keyframe_dur[num_keyframes + 1] = 0f;
	}

	public void resetDepth()
	{
		if (Random.value < 0.4f)
		{
			depth = 0f;
		}
		else if (Random.value < 0.3f)
		{
			depth = -0.5f * Random.value;
		}
		else
		{
			depth = Mathf.Pow(Random.value, 1.5f) * 3f;
		}
	}

	public override void Update(bool eu)
	{
		if (reset)
		{
			IntVector2 tilePosition = room.GetTilePosition(room.game.cameras[0].pos + new Vector2(Mathf.Lerp(-200f, (ModManager.MMF ? room.game.rainWorld.options.ScreenSize.x : 1024f) + 200f, Random.value), Mathf.Lerp(-200f, 968f, Random.value)));
			pos = room.MiddleOfTile(tilePosition);
			lastPos = pos;
			reset = false;
			if (light != null)
			{
				light.stayAlive = true;
			}
			ticker = 0f;
			keyframe_ind = 1;
			alpha = 0f;
			return;
		}
		rotation = (rotation + rotation_rate * (0.5f + Random.value * 0.5f)) % 360f;
		z_rotation = (z_rotation + rotation_rate * (0.5f + Random.value * 0.5f)) % 360f;
		lastLastPos = lastPos;
		lastPos = pos;
		pulse_ticker++;
		float num = Mathf.Sin((float)pulse_ticker * pulse_rate);
		num = ((!(num <= 0f) || abs_pulse) ? (Mathf.Abs(num) * (pulse_max - pulse_min) + pulse_min) : pulse_min);
		ticker += num;
		float num2 = ticker / keyframe_dur[keyframe_ind];
		if (num2 > 1f)
		{
			num2 = 0f;
			ticker = 0f;
			keyframe_ind++;
			if (keyframe_ind > num_keyframes)
			{
				ResetMe();
			}
		}
		if (num2 < interp_trans_ratio)
		{
			if (interp_speed_method == LerpMethod.LINEAR)
			{
				local_speed = Mathf.Lerp(keyframe_speed[keyframe_ind - 1], keyframe_speed[keyframe_ind], num2 / interp_trans_ratio);
			}
			else if (interp_speed_method == LerpMethod.SIN_IO)
			{
				local_speed = Custom.LerpSinEaseInOut(keyframe_speed[keyframe_ind - 1], keyframe_speed[keyframe_ind], num2 / interp_trans_ratio);
			}
			else if (interp_speed_method == LerpMethod.EXP)
			{
				local_speed = Custom.LerpExpEaseIn(keyframe_speed[keyframe_ind - 1], keyframe_speed[keyframe_ind], num2 / interp_trans_ratio);
			}
			else if (interp_speed_method == LerpMethod.QUAD_IN)
			{
				local_speed = Custom.LerpQuadEaseIn(keyframe_speed[keyframe_ind - 1], keyframe_speed[keyframe_ind], num2 / interp_trans_ratio);
			}
			else if (interp_speed_method == LerpMethod.QUAD_OUT)
			{
				local_speed = Custom.LerpQuadEaseOut(keyframe_speed[keyframe_ind - 1], keyframe_speed[keyframe_ind], num2 / interp_trans_ratio);
			}
			if (interp_dir_method == LerpMethod.LINEAR)
			{
				direction = Mathf.Lerp(keyframe_dir[keyframe_ind - 1], keyframe_dir[keyframe_ind], num2 / interp_trans_ratio);
			}
			else if (interp_dir_method == LerpMethod.SIN_IO)
			{
				direction = Custom.LerpSinEaseInOut(keyframe_dir[keyframe_ind - 1], keyframe_dir[keyframe_ind], num2 / interp_trans_ratio);
			}
			else if (interp_dir_method == LerpMethod.EXP)
			{
				direction = Custom.LerpExpEaseIn(keyframe_dir[keyframe_ind - 1], keyframe_dir[keyframe_ind], num2 / interp_trans_ratio);
			}
			else if (interp_dir_method == LerpMethod.QUAD_IN)
			{
				direction = Custom.LerpQuadEaseIn(keyframe_dir[keyframe_ind - 1], keyframe_dir[keyframe_ind], num2 / interp_trans_ratio);
			}
			else if (interp_dir_method == LerpMethod.QUAD_OUT)
			{
				direction = Custom.LerpQuadEaseOut(keyframe_dir[keyframe_ind - 1], keyframe_dir[keyframe_ind], num2 / interp_trans_ratio);
			}
		}
		pos += Custom.DegToVec(direction) * (local_speed * num);
		if (keyframe_ind == num_keyframes && num2 >= 1f - alpha_trans_ratio)
		{
			alpha = Custom.LerpQuadEaseOut(1f, 0f, (num2 - (1f - alpha_trans_ratio)) / alpha_trans_ratio);
		}
		else if (keyframe_ind == 1 && num2 < alpha_trans_ratio)
		{
			alpha = Custom.LerpQuadEaseIn(0f, 1f, num2 / alpha_trans_ratio);
		}
		else
		{
			alpha = 1f;
		}
		if (Custom.VectorRectDistance(pos, room.RoomRect) > 100f && !room.ViewedByAnyCamera(pos, 400f))
		{
			Destroy();
		}
		if (depth <= 0f && room.Darkness(pos) > 0f && glowIntensity > 0f && glowRadius > 0f)
		{
			if (light == null)
			{
				light = new LightSource(pos, environmentalLight: false, col, this);
				light.noGameplayImpact = true;
				room.AddObject(light);
				light.requireUpKeep = true;
				light.setRad = glowRadius + scale_max;
			}
			light.setPos = pos;
			light.setAlpha = Mathf.Max(0.01f, glowIntensity * alpha);
			light.stayAlive = true;
		}
		else if (light != null)
		{
			light.Destroy();
			light = null;
		}
		if (pos.x < room.game.cameras[0].pos.x - 200f)
		{
			if (room.gravity == 0f)
			{
				pos.x = room.game.cameras[0].pos.x + 1366f + 200f;
			}
			else
			{
				reset = true;
			}
		}
		if (pos.x > room.game.cameras[0].pos.x + 1366f + 200f)
		{
			if (room.gravity == 0f)
			{
				pos.x = room.game.cameras[0].pos.x - 200f;
			}
			else
			{
				reset = true;
			}
		}
		if (pos.y < room.game.cameras[0].pos.y - 200f)
		{
			if (room.gravity == 0f)
			{
				pos.y = room.game.cameras[0].pos.y + 768f + 200f;
			}
			else
			{
				reset = true;
			}
		}
		if (pos.y > room.game.cameras[0].pos.y + 768f + 200f)
		{
			if (room.gravity == 0f)
			{
				pos.y = room.game.cameras[0].pos.y - 200f;
			}
			else
			{
				reset = true;
			}
		}
		if (!room.BeingViewed)
		{
			Destroy();
		}
	}

	public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
	{
		sLeaser.sprites = new FSprite[1];
		sLeaser.sprites[0] = new FSprite(spriteName);
		sLeaser.sprites[0].anchorY = 0.5f;
		sLeaser.sprites[0].anchorX = 0.5f;
		if (depth > 0f)
		{
			sLeaser.sprites[0].shader = rCam.room.game.rainWorld.Shaders["CustomDepth"];
			sLeaser.sprites[0].alpha = 0f;
		}
		AddToContainer(sLeaser, rCam, null);
	}

	public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
	{
		if (resetSprite)
		{
			sLeaser.sprites[0].SetElementByName(spriteName);
			resetPalette(sLeaser);
			resetSprite = false;
		}
		sLeaser.sprites[0].x = Mathf.Lerp(lastPos.x, pos.x, timeStacker) - camPos.x;
		sLeaser.sprites[0].y = Mathf.Lerp(lastPos.y, pos.y, timeStacker) - camPos.y;
		if (rotation_rate == 0f)
		{
			sLeaser.sprites[0].rotation = Custom.AimFromOneVectorToAnother(Vector2.Lerp(lastLastPos, lastPos, timeStacker), Vector2.Lerp(lastPos, pos, timeStacker));
		}
		else
		{
			sLeaser.sprites[0].rotation = rotation;
		}
		sLeaser.sprites[0].alpha = alpha;
		if (resetScale)
		{
			if (depth < 0f)
			{
				sLeaser.sprites[0].scaleX = Custom.LerpMap(depth, 0f, -0.5f, 1.5f, 2f);
			}
			else if (depth > 0f)
			{
				sLeaser.sprites[0].scaleX = Custom.LerpMap(depth, 0f, 5f, 1.5f, 0.1f);
			}
			else
			{
				sLeaser.sprites[0].scaleX = 1.5f;
			}
			sLeaser.sprites[0].scaleX *= Random.Range(scale_min, scale_max) * scale_multiplier;
			sLeaser.sprites[0].scaleY = sLeaser.sprites[0].scaleX;
			resetScale = false;
		}
		if (rotation_rate > 0f)
		{
			float num = 1f;
			num = ((!(z_rotation <= 180f)) ? ((360f - z_rotation) / 180f) : (z_rotation / 180f));
			sLeaser.sprites[0].scaleY = sLeaser.sprites[0].scaleX * num;
		}
		if (light != null)
		{
			light.setRad = glowRadius + sLeaser.sprites[0].scaleX;
		}
		base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
	}

	public override void ApplyPalette(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
	{
		skyColor = palette.skyColor;
		resetPalette(sLeaser);
	}

	public void resetPalette(RoomCamera.SpriteLeaser sLeaser)
	{
		col = Custom.HSL2RGB(Random.Range(minHSL.x, maxHSL.x), Random.Range(minHSL.y, maxHSL.y), Random.Range(minHSL.z, maxHSL.z));
		if (depth <= 0f)
		{
			sLeaser.sprites[0].color = col;
		}
		else
		{
			sLeaser.sprites[0].color = Color.Lerp(skyColor, col, Mathf.InverseLerp(0f, 5f, depth));
		}
		if (light != null)
		{
			light.color = col;
		}
	}

	public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
	{
		newContatiner = rCam.ReturnFContainer("Foreground");
		sLeaser.sprites[0].RemoveFromContainer();
		newContatiner.AddChild(sLeaser.sprites[0]);
	}
}
