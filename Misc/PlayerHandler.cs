using System.Collections;
using Kittehface.Framework20;
using Rewired;
using RWCustom;
using UnityEngine;

public class PlayerHandler : MonoBehaviour
{
	private ControllerHandler controllerHandler;

	private bool signingIn;

	private bool pendingSignIn;

	public RainWorld rainWorld { get; private set; }

	public bool initialized => profile != null;

	public int playerIndex { get; private set; }

	public Profiles.Profile profile { get; private set; }

	public bool SigningIn
	{
		get
		{
			if (!signingIn)
			{
				return pendingSignIn;
			}
			return true;
		}
	}

	public ControllerHandler ControllerHandler
	{
		get
		{
			if (controllerHandler == null)
			{
				controllerHandler = GetComponent<ControllerHandler>();
			}
			return controllerHandler;
		}
	}

	public Joystick requestedJoystick { get; private set; }

	public void Initialize(RainWorld rainWorld, int playerIndex, Profiles.Profile profile)
	{
		if (this.profile == null)
		{
			this.rainWorld = rainWorld;
			this.playerIndex = playerIndex;
			this.profile = profile;
		}
	}

	public void RequestSignIn(Joystick requestJoystick)
	{
		if (signingIn)
		{
			return;
		}
		for (int i = 0; i < 4; i++)
		{
			if (i != playerIndex)
			{
				PlayerHandler playerHandlerRaw = rainWorld.GetPlayerHandlerRaw(i);
				if (playerHandlerRaw != null && playerHandlerRaw.signingIn)
				{
					pendingSignIn = true;
					requestedJoystick = requestJoystick;
					StartCoroutine(DoPendingSignIn());
					return;
				}
			}
		}
		signingIn = true;
		Profiles.OnSignedIn += Profiles_OnSignedIn;
		if (requestJoystick == null)
		{
			Profiles.RequestSignIn(null);
			return;
		}
		Profiles.RequestSignIn(requestJoystick.id);
	}

	public void Deactivate()
	{
		if (playerIndex != 0 && playerIndex != -1)
		{
			Custom.LogImportant("Deactivating player!!!", (profile == null) ? "NULL" : profile.GetDisplayName(), "playerindex:", playerIndex.ToString());
			if (profile != null && !Custom.rainWorld.IsPrimaryProfile(profile))
			{
				profile.Deactivate();
			}
			profile = null;
			signingIn = false;
			pendingSignIn = false;
			requestedJoystick = null;
		}
	}

	private void Awake()
	{
		Profiles.OnWillDeactivate += Profiles_OnWillDeactivate;
		Profiles.OnDeactivated += Profiles_OnDeactivated;
	}

	private void OnDestroy()
	{
		Profiles.OnWillDeactivate -= Profiles_OnWillDeactivate;
		Profiles.OnDeactivated -= Profiles_OnDeactivated;
	}

	private IEnumerator DoPendingSignIn()
	{
		bool flag = true;
		while (flag)
		{
			yield return 0;
			flag = false;
			for (int i = 0; i < 4; i++)
			{
				if (i != playerIndex)
				{
					PlayerHandler playerHandlerRaw = rainWorld.GetPlayerHandlerRaw(i);
					if (playerHandlerRaw != null && playerHandlerRaw.signingIn)
					{
						flag = true;
						break;
					}
				}
			}
		}
		pendingSignIn = false;
		Joystick joystick = requestedJoystick;
		requestedJoystick = null;
		signingIn = true;
		Profiles.OnSignedIn += Profiles_OnSignedIn;
		Profiles.RequestSignIn(joystick?.systemId);
	}

	private void Profiles_OnSignedIn(Profiles.Profile profile, Profiles.SignInResult result)
	{
		Profiles.OnSignedIn -= Profiles_OnSignedIn;
		if (signingIn)
		{
			signingIn = false;
			if (result == Profiles.SignInResult.Success)
			{
				this.profile = profile;
				ControllerHandler.ResetHandler(isPlayerDead: false);
			}
		}
	}

	private void Profiles_OnWillDeactivate(Profiles.Profile profile)
	{
		if (this.profile == profile)
		{
			signingIn = false;
			pendingSignIn = false;
			requestedJoystick = null;
		}
	}

	private void Profiles_OnDeactivated(Profiles.Profile profile)
	{
		if (this.profile == profile && playerIndex > 0)
		{
			Custom.LogImportant("Deactivating player!!!", (profile == null) ? "NULL" : profile.GetDisplayName());
			this.profile = null;
			ControllerHandler.ResetHandler(isPlayerDead: false);
		}
	}
}
