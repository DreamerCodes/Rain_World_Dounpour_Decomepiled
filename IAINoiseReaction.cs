using Noise;

public interface IAINoiseReaction
{
	void ReactToNoise(NoiseTracker.TheorizedSource source, InGameNoise noise);
}
