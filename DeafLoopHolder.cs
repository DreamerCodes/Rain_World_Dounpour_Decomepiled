using MoreSlugcats;
using RWCustom;

public class DeafLoopHolder : UpdatableAndDeletable
{
	private Player player;

	public DisembodiedDynamicSoundLoop deafLoop;

	public bool muted;

	public float Deaf => room.game.cameras[0].virtualMicrophone.deafContribution;

	public DeafLoopHolder(Player player)
	{
		this.player = player;
	}

	public override void Update(bool eu)
	{
		base.Update(eu);
		if (Deaf > 0f && player.abstractCreature.world.game.cameras[0].room != room)
		{
			player.deafLoopHolder = new DeafLoopHolder(player);
			player.abstractCreature.world.game.cameras[0].room.AddObject(player.deafLoopHolder);
			Destroy();
		}
		else if (deafLoop == null && Deaf > 0f)
		{
			deafLoop = new DisembodiedDynamicSoundLoop(this);
			deafLoop.sound = SoundID.Deaf_Sine_LOOP;
			deafLoop.VolumeGroup = 1;
		}
		else
		{
			if (deafLoop == null)
			{
				return;
			}
			deafLoop.Update();
			if (muted || (ModManager.MMF && MMF.cfgNoMoreTinnitus.Value))
			{
				deafLoop.Volume = 0f;
			}
			else
			{
				deafLoop.Volume = Custom.LerpAndTick(deafLoop.Volume, Deaf, 0.06f, 1f / 30f);
			}
			if (Deaf == 0f && deafLoop.Volume == 0f)
			{
				if (deafLoop.emitter != null)
				{
					deafLoop.emitter.slatedForDeletetion = true;
				}
				deafLoop = null;
			}
		}
	}

	public override void Destroy()
	{
		base.Destroy();
		if (deafLoop != null && deafLoop.emitter != null)
		{
			deafLoop.emitter.slatedForDeletetion = true;
		}
	}
}
