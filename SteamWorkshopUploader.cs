using System;
using Menu;
using UnityEngine;

internal class SteamWorkshopUploader
{
	public enum UploadStep
	{
		INIT,
		CHECK_EXISTS,
		CREATE,
		WAIT_FOR_CREATE,
		ACCEPT_LEGAL,
		WAIT_FOR_ACCEPT_LEGAL,
		UPLOAD,
		WAIT_FOR_UPLOAD,
		PREVIEW,
		FAIL,
		SUCCEED,
		UNINIT
	}

	public UploadStep currentStep;

	public global::Menu.Menu menu;

	public ModManager.Mod modToUpload;

	public bool readyToDispose;

	private string failMessage;

	private bool newMod;

	private DialogBoxAsyncWait asyncWaitDialog;

	private DialogBoxMultiButtonNotify buttonsDialog;

	public SteamWorkshopUploader(global::Menu.Menu menu, ModManager.Mod modToUpload)
	{
		currentStep = UploadStep.INIT;
		this.menu = menu;
		this.modToUpload = modToUpload;
	}

	private string Translate(string t)
	{
		return menu.manager.rainWorld.inGameTranslator.Translate(t);
	}

	private void CreateAsyncDialog(string message)
	{
		if (asyncWaitDialog == null)
		{
			asyncWaitDialog = new DialogBoxAsyncWait(menu, menu.pages[0], message, new Vector2(menu.manager.rainWorld.options.ScreenSize.x / 2f - 240f + (1366f - menu.manager.rainWorld.options.ScreenSize.x) / 2f, 224f), new Vector2(480f, 320f));
			menu.pages[0].subObjects.Add(asyncWaitDialog);
		}
		else
		{
			asyncWaitDialog.SetText(message);
		}
	}

	private void CloseAsyncDialog()
	{
		if (asyncWaitDialog != null)
		{
			menu.pages[0].subObjects.Remove(asyncWaitDialog);
			asyncWaitDialog.RemoveSprites();
			asyncWaitDialog = null;
		}
	}

	private void CreateButtonsDialog(string message, string[] buttons, string[] signals)
	{
		if (buttonsDialog == null)
		{
			buttonsDialog = new DialogBoxMultiButtonNotify(menu, menu.pages[0], message, signals, buttons, new Vector2(menu.manager.rainWorld.options.ScreenSize.x / 2f - 240f + (1366f - menu.manager.rainWorld.options.ScreenSize.x) / 2f, 224f), new Vector2(480f, 320f));
			menu.pages[0].subObjects.Add(buttonsDialog);
		}
	}

	private void CloseButtonsDialog()
	{
		if (buttonsDialog != null)
		{
			menu.pages[0].subObjects.Remove(buttonsDialog);
			buttonsDialog.RemoveSprites();
			buttonsDialog = null;
		}
	}

	public void Update()
	{
		RainWorldSteamManager mySteamManager = menu.manager.mySteamManager;
		if (currentStep == UploadStep.INIT)
		{
			if (!mySteamManager.isCurrentlyQuerying && !mySteamManager.isCurrentlyCreating && !mySteamManager.isCurrentlyUploading)
			{
				string text = mySteamManager.ValidateWorkshopModForProblems(modToUpload);
				if (text == null)
				{
					mySteamManager.FindWorkshopItemsWithKeyValue("id", modToUpload.id);
					CreateAsyncDialog(Translate("Gathering information") + "...");
					currentStep = UploadStep.CHECK_EXISTS;
				}
				else
				{
					failMessage = Translate("Failure") + ": " + text;
					currentStep = UploadStep.FAIL;
				}
			}
		}
		else if (currentStep == UploadStep.CHECK_EXISTS)
		{
			if (mySteamManager.isCurrentlyQuerying)
			{
				return;
			}
			if (mySteamManager.lastQueryCount < 0 || mySteamManager.lastQueryFail != "")
			{
				failMessage = Translate("Failure") + ": " + mySteamManager.lastQueryFail;
				currentStep = UploadStep.FAIL;
			}
			else if (mySteamManager.lastQueryCount == 0)
			{
				newMod = true;
				currentStep = UploadStep.CREATE;
			}
			else
			{
				newMod = false;
				bool flag = false;
				for (int i = 0; i < mySteamManager.lastQueryFiles.Count; i++)
				{
					if (mySteamManager.lastQueryOwners[i] == RainWorldSteamManager.ownerUserID)
					{
						modToUpload.workshopId = mySteamManager.lastQueryFiles[i].m_PublishedFileId;
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					failMessage = Translate("Failure") + ": " + Translate("This mod already exists on the workshop by another author.");
					currentStep = UploadStep.FAIL;
				}
				else
				{
					currentStep = UploadStep.UPLOAD;
				}
			}
			if (currentStep == UploadStep.FAIL)
			{
				CloseAsyncDialog();
			}
		}
		else if (currentStep == UploadStep.CREATE)
		{
			mySteamManager.CreateWorkshopMod(modToUpload);
			CreateAsyncDialog(Translate("Preparing for upload") + "...");
			currentStep = UploadStep.WAIT_FOR_CREATE;
		}
		else if (currentStep == UploadStep.WAIT_FOR_CREATE)
		{
			if (!mySteamManager.isCurrentlyCreating)
			{
				if (mySteamManager.lastCreateFail != "")
				{
					failMessage = Translate("Failure") + ": " + mySteamManager.lastCreateFail;
					currentStep = UploadStep.FAIL;
				}
				else if (mySteamManager.needsLegalAgreement)
				{
					currentStep = UploadStep.ACCEPT_LEGAL;
				}
				else
				{
					currentStep = UploadStep.UPLOAD;
				}
				if (currentStep == UploadStep.FAIL || currentStep == UploadStep.ACCEPT_LEGAL)
				{
					CloseAsyncDialog();
				}
			}
		}
		else if (currentStep == UploadStep.ACCEPT_LEGAL)
		{
			if (buttonsDialog == null)
			{
				CreateButtonsDialog(Translate("You must accept the End User License Agreement.") + Environment.NewLine + Translate("Press CONTINUE to view and accept it."), new string[2]
				{
					Translate("CONTINUE"),
					Translate("EXIT")
				}, new string[2] { "UPLOADER_CONTINUE", "UPLOADER_DISPOSE" });
			}
		}
		else if (currentStep == UploadStep.WAIT_FOR_ACCEPT_LEGAL)
		{
			if (buttonsDialog == null)
			{
				CreateButtonsDialog(Translate("Press CONTINUE after accepting the End User License Agreement."), new string[3]
				{
					Translate("CONTINUE"),
					Translate("VIEW EULA"),
					Translate("EXIT")
				}, new string[3] { "UPLOADER_CONTINUE", "EULA", "UPLOADER_DISPOSE" });
			}
			currentStep = UploadStep.UPLOAD;
		}
		else if (currentStep == UploadStep.UPLOAD)
		{
			mySteamManager.UploadWorkshopMod(modToUpload, newMod);
			CreateAsyncDialog(Translate("Uploading") + "...");
			currentStep = UploadStep.WAIT_FOR_UPLOAD;
		}
		else if (currentStep == UploadStep.WAIT_FOR_UPLOAD)
		{
			if (!mySteamManager.isCurrentlyUploading)
			{
				CloseAsyncDialog();
				if (mySteamManager.lastUploadFail != "")
				{
					failMessage = Translate("Failure") + ": " + mySteamManager.lastUploadFail;
					currentStep = UploadStep.FAIL;
				}
				else if (mySteamManager.needsLegalAgreement)
				{
					currentStep = UploadStep.ACCEPT_LEGAL;
				}
				else
				{
					currentStep = UploadStep.PREVIEW;
				}
			}
			else if (mySteamManager.bytesTotal != 0)
			{
				asyncWaitDialog.SetText(Translate("Uploading") + " (" + ((double)mySteamManager.bytesProcessed / (double)mySteamManager.bytesTotal * 100.0).ToString("0.00") + "%)...");
			}
		}
		else if (currentStep == UploadStep.PREVIEW)
		{
			string text2 = "";
			text2 = ((!newMod) ? (text2 + Translate("Previously uploaded mod has been updated.") + Environment.NewLine) : (text2 + Translate("Mod uploaded with Unlisted visibility.") + Environment.NewLine));
			text2 += Translate("Press CONTINUE to view and/or edit your uploaded workshop mod.");
			if (buttonsDialog == null)
			{
				CreateButtonsDialog(text2, new string[1] { Translate("CONTINUE") }, new string[1] { "UPLOADER_CONTINUE" });
			}
		}
		else if (currentStep == UploadStep.FAIL)
		{
			if (buttonsDialog == null)
			{
				CreateButtonsDialog(failMessage, new string[1] { Translate("EXIT") }, new string[1] { "UPLOADER_DISPOSE" });
			}
		}
		else if (currentStep == UploadStep.SUCCEED)
		{
			if (buttonsDialog == null)
			{
				CreateButtonsDialog(Translate("Workshop upload completed successfully!"), new string[1] { Translate("EXIT") }, new string[1] { "UPLOADER_DISPOSE" });
			}
		}
		else if (currentStep == UploadStep.UNINIT)
		{
			CloseButtonsDialog();
			CloseAsyncDialog();
			readyToDispose = true;
		}
	}

	public void Singal(MenuObject sender, string message)
	{
		RainWorldSteamManager mySteamManager = menu.manager.mySteamManager;
		switch (message)
		{
		case "UPLOADER_CONTINUE":
			CloseButtonsDialog();
			if (currentStep == UploadStep.ACCEPT_LEGAL)
			{
				mySteamManager.ShowLegalAgreement();
				currentStep = UploadStep.WAIT_FOR_ACCEPT_LEGAL;
			}
			else if (currentStep == UploadStep.ACCEPT_LEGAL)
			{
				currentStep = UploadStep.UPLOAD;
			}
			else if (currentStep == UploadStep.PREVIEW)
			{
				mySteamManager.ShowWorkshopDetails(modToUpload.workshopId);
				currentStep = UploadStep.SUCCEED;
			}
			else
			{
				currentStep = UploadStep.UNINIT;
			}
			break;
		case "EULA":
			mySteamManager.ShowLegalAgreement();
			break;
		case "UPLOADER_DISPOSE":
			CloseButtonsDialog();
			currentStep = UploadStep.UNINIT;
			break;
		}
	}
}
