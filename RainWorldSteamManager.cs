using System;
using System.Collections.Generic;
using System.IO;
using DevInterface;
using Expedition;
using JollyCoop;
using MoreSlugcats;
using Steamworks;
using UnityEngine;

internal class RainWorldSteamManager : MainLoopProcess
{
	public static uint APP_ID = 312520u;

	public static uint DOWNPOUR_APP_ID = 1933390u;

	public bool shutdown;

	public static ulong ownerUserID;

	public ModManager.Mod creatingMod;

	public bool isCurrentlyCreating;

	public string lastCreateFail = "";

	public bool needsLegalAgreement;

	public ModManager.Mod uploadingMod;

	public bool isCurrentlyUploading;

	public string lastUploadFail = "";

	public ulong bytesProcessed;

	public ulong bytesTotal;

	public bool isCurrentlyQuerying;

	public int lastQueryCount;

	public string lastQueryFail = "";

	public UGCQueryHandle_t lastQueryHandle;

	public List<ulong> lastQueryOwners;

	public List<PublishedFileId_t> lastQueryFiles;

	public List<PublishedFileId_t> currentlyDownloading;

	public int numberItemsAddedForDownloading;

	private UGCUpdateHandle_t updateHandle;

	private CallResult<CreateItemResult_t> createItemCallback;

	private CallResult<SubmitItemUpdateResult_t> updateItemCallback;

	private CallResult<SteamUGCQueryCompleted_t> queryCallback;

	private Callback<DownloadItemResult_t> downloadCallback;

	public RainWorldSteamManager(ProcessManager manager, ProcessManager.ProcessID ID)
		: base(manager, ID)
	{
	}

	public RainWorldSteamManager(ProcessManager manager)
		: base(manager, ProcessManager.ProcessID.RainWorldSteamManager)
	{
		if (SteamManager.Initialized)
		{
			SteamFriends.GetPersonaName();
			ownerUserID = SteamUser.GetSteamID().m_SteamID;
			SteamUserStats.RequestCurrentStats();
			currentlyDownloading = new List<PublishedFileId_t>();
			createItemCallback = CallResult<CreateItemResult_t>.Create(OnCreateItemResult);
			updateItemCallback = CallResult<SubmitItemUpdateResult_t>.Create(OnUpdateItemResult);
			queryCallback = CallResult<SteamUGCQueryCompleted_t>.Create(OnQueryResult);
			downloadCallback = Callback<DownloadItemResult_t>.Create(OnDownloadItemResult);
		}
		else
		{
			ShutDown();
		}
	}

	public override void Update()
	{
		if (SteamManager.Initialized)
		{
			if (isCurrentlyUploading)
			{
				SteamUGC.GetItemUpdateProgress(updateHandle, out bytesProcessed, out bytesTotal);
			}
			SteamAPI.RunCallbacks();
		}
	}

	public void ShutDown()
	{
		manager.StopSideProcess(this);
		shutdown = true;
	}

	private string Translate(string t)
	{
		return manager.rainWorld.inGameTranslator.Translate(t);
	}

	public bool HasAchievement(string name)
	{
		if (!SteamManager.Initialized)
		{
			ShutDown();
			return false;
		}
		SteamUserStats.GetAchievement(name, out var pbAchieved);
		return pbAchieved;
	}

	public void SetAchievement(string name)
	{
		if (!SteamManager.Initialized)
		{
			ShutDown();
			return;
		}
		SteamUserStats.SetAchievement(name);
		SteamUserStats.StoreStats();
	}

	public void ClearAchievement(string name)
	{
		if (!SteamManager.Initialized)
		{
			ShutDown();
			return;
		}
		SteamUserStats.ClearAchievement(name);
		SteamUserStats.StoreStats();
	}

	public static bool ValidateModApplicableForWorkshopUpload(ModManager.Mod mod)
	{
		if (mod.id == global::MoreSlugcats.MoreSlugcats.MOD_ID || mod.id == MMF.MOD_ID || mod.id == global::Expedition.Expedition.MOD_ID || mod.id == global::JollyCoop.JollyCoop.MOD_ID || mod.id == DevTools.MOD_ID)
		{
			return false;
		}
		if (mod.id == "RainWorld_BaseGame" || mod.id == "_TestDummy_")
		{
			return false;
		}
		if (mod.workshopMod)
		{
			return false;
		}
		return true;
	}

	public string ValidateWorkshopModForProblems(ModManager.Mod mod)
	{
		string thumbnailPath = mod.GetThumbnailPath();
		if (File.Exists(thumbnailPath))
		{
			try
			{
				if (new FileInfo(thumbnailPath).Length >= 1000000)
				{
					return Translate("Mod's thumbnail image must be less than 1 MB in size.");
				}
				Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, mipChain: false);
				AssetManager.SafeWWWLoadTexture(ref texture2D, thumbnailPath, clampWrapMode: false, crispPixels: true);
				double num = (double)texture2D.height / (double)texture2D.width;
				if (num < 0.5616 || num > 0.5634)
				{
					return Translate("Mod's thumbnail image should have a 16:9 aspect ratio.");
				}
			}
			catch
			{
			}
		}
		if (!ValidateModApplicableForWorkshopUpload(mod))
		{
			return Translate("This mod cannot be uploaded to the Workshop.");
		}
		return null;
	}

	private bool InitCreate()
	{
		if (!SteamManager.Initialized)
		{
			isCurrentlyCreating = false;
			lastCreateFail = Translate("Steam manager not initialized");
			ShutDown();
			return false;
		}
		lastCreateFail = "";
		needsLegalAgreement = false;
		isCurrentlyCreating = true;
		return true;
	}

	public void CreateWorkshopMod(ModManager.Mod mod)
	{
		if (InitCreate())
		{
			creatingMod = mod;
			SteamAPICall_t hAPICall = SteamUGC.CreateItem(new AppId_t(APP_ID), EWorkshopFileType.k_EWorkshopFileTypeFirst);
			createItemCallback.Set(hAPICall);
		}
	}

	private bool InitUpload()
	{
		if (!SteamManager.Initialized)
		{
			isCurrentlyUploading = false;
			lastUploadFail = Translate("Steam manager not initialized");
			ShutDown();
			return false;
		}
		lastUploadFail = "";
		bytesTotal = 0uL;
		needsLegalAgreement = false;
		isCurrentlyUploading = true;
		return true;
	}

	public bool UploadWorkshopMod(ModManager.Mod mod, bool unlisted)
	{
		if (!InitUpload())
		{
			return false;
		}
		if (mod.workshopId == 0L)
		{
			isCurrentlyUploading = false;
			lastUploadFail = Translate("Cannot update mod with no pre-existing workshop ID.");
			return false;
		}
		uploadingMod = mod;
		updateHandle = SteamUGC.StartItemUpdate(new AppId_t(APP_ID), new PublishedFileId_t(mod.workshopId));
		SteamUGC.SetItemTitle(updateHandle, mod.name);
		SteamUGC.SetItemDescription(updateHandle, mod.description.Replace("<LINE>", Environment.NewLine));
		if (unlisted)
		{
			SteamUGC.SetItemVisibility(updateHandle, ERemoteStoragePublishedFileVisibility.k_ERemoteStoragePublishedFileVisibilityUnlisted);
		}
		SteamUGC.AddItemKeyValueTag(updateHandle, "id", mod.id);
		SteamUGC.AddItemKeyValueTag(updateHandle, "version", mod.version);
		SteamUGC.AddItemKeyValueTag(updateHandle, "targetGameVersion", mod.targetGameVersion);
		SteamUGC.AddItemKeyValueTag(updateHandle, "authors", mod.authors.Replace("<LINE>", Environment.NewLine));
		SteamUGC.AddItemKeyValueTag(updateHandle, "requirements", string.Join(",", mod.requirements));
		SteamUGC.AddItemKeyValueTag(updateHandle, "requirementNames", string.Join(",", mod.requirementsNames));
		SteamUGC.SetItemTags(updateHandle, mod.tags);
		SteamUGC.SetItemContent(updateHandle, mod.path);
		string thumbnailPath = mod.GetThumbnailPath();
		if (File.Exists(thumbnailPath))
		{
			try
			{
				if (new FileInfo(thumbnailPath).Length < 1000000)
				{
					SteamUGC.SetItemPreview(updateHandle, thumbnailPath);
				}
			}
			catch
			{
			}
		}
		if (mod.trailerID != null && mod.trailerID != "")
		{
			SteamUGC.AddItemPreviewVideo(updateHandle, mod.trailerID);
		}
		SteamAPICall_t hAPICall = SteamUGC.SubmitItemUpdate(updateHandle, "");
		updateItemCallback.Set(hAPICall);
		return true;
	}

	private bool InitQuery()
	{
		if (!SteamManager.Initialized)
		{
			isCurrentlyQuerying = false;
			lastQueryFail = Translate("Steam manager not initialized");
			ShutDown();
			return false;
		}
		lastQueryCount = -1;
		lastQueryFail = "";
		isCurrentlyQuerying = true;
		return true;
	}

	public void FindWorkshopItemsWithKeyValue(string key, string value)
	{
		if (InitQuery())
		{
			AppId_t appId_t = new AppId_t(APP_ID);
			lastQueryHandle = SteamUGC.CreateQueryAllUGCRequest(EUGCQuery.k_EUGCQuery_RankedByPublicationDate, EUGCMatchingUGCType.k_EUGCMatchingUGCType_Items_ReadyToUse, appId_t, appId_t);
			SteamUGC.AddRequiredKeyValueTag(lastQueryHandle, key, value);
			SteamAPICall_t hAPICall = SteamUGC.SendQueryUGCRequest(lastQueryHandle);
			queryCallback.Set(hAPICall);
		}
	}

	public void ShowLegalAgreement()
	{
		if (!SteamManager.Initialized)
		{
			ShutDown();
		}
		else
		{
			SteamFriends.ActivateGameOverlayToWebPage("https://steamcommunity.com/sharedfiles/workshoplegalagreement");
		}
	}

	public void ShowWorkshopDetails(ulong workshopId)
	{
		if (!SteamManager.Initialized)
		{
			ShutDown();
		}
		else
		{
			SteamFriends.ActivateGameOverlayToWebPage("steam://url/CommunityFilePage/" + workshopId);
		}
	}

	public void ShowStorePage()
	{
		if (!SteamManager.Initialized)
		{
			ShutDown();
		}
		else
		{
			SteamFriends.ActivateGameOverlayToStore(new AppId_t(APP_ID), EOverlayToStoreFlag.k_EOverlayToStoreFlag_None);
		}
	}

	public void ShowDownpourStorePage()
	{
		if (!SteamManager.Initialized)
		{
			ShutDown();
		}
		else
		{
			SteamFriends.ActivateGameOverlayToStore(new AppId_t(DOWNPOUR_APP_ID), EOverlayToStoreFlag.k_EOverlayToStoreFlag_None);
		}
	}

	public PublishedFileId_t[] GetSubscribedItems()
	{
		if (!SteamManager.Initialized)
		{
			ShutDown();
			return new PublishedFileId_t[0];
		}
		uint numSubscribedItems = SteamUGC.GetNumSubscribedItems();
		PublishedFileId_t[] array = new PublishedFileId_t[numSubscribedItems];
		if (numSubscribedItems != 0)
		{
			SteamUGC.GetSubscribedItems(array, numSubscribedItems);
		}
		return array;
	}

	public bool DownloadWorkshopMod(PublishedFileId_t fileid)
	{
		if (!SteamManager.Initialized)
		{
			ShutDown();
			return false;
		}
		currentlyDownloading.Add(fileid);
		numberItemsAddedForDownloading++;
		bool num = SteamUGC.DownloadItem(fileid, bHighPriority: true);
		if (!num)
		{
			currentlyDownloading.Remove(fileid);
			numberItemsAddedForDownloading--;
		}
		return num;
	}

	public void ResetDownloadBatch()
	{
		numberItemsAddedForDownloading = 0;
	}

	public double GetWorkshopDownloadProgress()
	{
		if (!SteamManager.Initialized)
		{
			ShutDown();
			return 0.0;
		}
		double num = 1.0 / (double)numberItemsAddedForDownloading;
		double num2 = 0.0;
		int num3 = numberItemsAddedForDownloading - currentlyDownloading.Count;
		num2 += (double)num3 * num;
		for (int i = 0; i < currentlyDownloading.Count; i++)
		{
			if (SteamUGC.GetItemDownloadInfo(currentlyDownloading[i], out var punBytesDownloaded, out var punBytesTotal) && punBytesTotal != 0)
			{
				num2 += (double)punBytesDownloaded / (double)punBytesTotal * num;
			}
		}
		return num2;
	}

	private void OnCreateItemResult(CreateItemResult_t callback, bool ioFailure)
	{
		if (callback.m_eResult == EResult.k_EResultOK)
		{
			PublishedFileId_t nPublishedFileId = callback.m_nPublishedFileId;
			creatingMod.workshopId = nPublishedFileId.m_PublishedFileId;
			if (callback.m_bUserNeedsToAcceptWorkshopLegalAgreement)
			{
				needsLegalAgreement = true;
			}
		}
		else if (callback.m_eResult == EResult.k_EResultBanned || callback.m_eResult == EResult.k_EResultInsufficientPrivilege)
		{
			lastCreateFail = Translate("Your account is locked or has a community or VAC ban.");
		}
		else if (callback.m_eResult == EResult.k_EResultTimeout)
		{
			lastCreateFail = Translate("The operation timed out. Try again later.");
		}
		else if (callback.m_eResult == EResult.k_EResultNotLoggedOn)
		{
			lastCreateFail = Translate("You are not currently logged into Steam.");
		}
		else if (callback.m_eResult == EResult.k_EResultServiceUnavailable)
		{
			lastCreateFail = Translate("The workshop service is unavailable. Try again later.");
		}
		else if (callback.m_eResult == EResult.k_EResultInvalidParam)
		{
			lastCreateFail = Translate("Some of the metadata for this mod was invalid and cannot be accepted.");
		}
		else if (callback.m_eResult == EResult.k_EResultAccessDenied)
		{
			lastCreateFail = Translate("Access was denied. Do you own the game on Steam?");
		}
		else if (callback.m_eResult == EResult.k_EResultLimitExceeded)
		{
			lastCreateFail = Translate("Limit exceeded. You may need to reduce the filesize or remove prior workshop content.");
		}
		else if (callback.m_eResult == EResult.k_EResultFileNotFound)
		{
			lastCreateFail = Translate("Could not find the files to upload.");
		}
		else if (callback.m_eResult == EResult.k_EResultDuplicateRequest)
		{
			lastCreateFail = Translate("This item already exists on the workshop.");
		}
		else if (callback.m_eResult == EResult.k_EResultDuplicateName)
		{
			lastCreateFail = Translate("One of your workshop mods already has the same name as this one.");
		}
		else if (callback.m_eResult == EResult.k_EResultServiceReadOnly)
		{
			lastCreateFail = Translate("Your account is in read-only mode due to a recent password or email change.");
		}
		else
		{
			lastCreateFail = callback.m_eResult.ToString();
		}
		isCurrentlyCreating = false;
	}

	private void OnUpdateItemResult(SubmitItemUpdateResult_t callback, bool ioFailure)
	{
		isCurrentlyUploading = false;
		if (callback.m_eResult == EResult.k_EResultOK)
		{
			if (callback.m_bUserNeedsToAcceptWorkshopLegalAgreement)
			{
				needsLegalAgreement = true;
			}
		}
		else if (callback.m_eResult == EResult.k_EResultAccessDenied)
		{
			lastUploadFail = Translate("Access was denied. Do you own the game on Steam?");
		}
		else if (callback.m_eResult == EResult.k_EResultInvalidParam)
		{
			lastUploadFail = Translate("Some of the metadata for this mod was invalid and cannot be accepted.");
		}
		else if (callback.m_eResult == EResult.k_EResultFileNotFound)
		{
			lastUploadFail = Translate("Could not find the files to upload.");
		}
		else if (callback.m_eResult == EResult.k_EResultLimitExceeded)
		{
			lastUploadFail = Translate("Limit exceeded. You may need to reduce the filesize or remove prior workshop content.");
		}
		else if (callback.m_eResult == EResult.k_EResultLockingFailed)
		{
			lastUploadFail = Translate("Failed to acquire User Generated Content lock. Try again later.");
		}
		else if (callback.m_eResult == EResult.k_EResultFail)
		{
			lastUploadFail = Translate("Generic failure.");
		}
		else
		{
			lastUploadFail = callback.m_eResult.ToString();
		}
		isCurrentlyUploading = false;
	}

	private void OnQueryResult(SteamUGCQueryCompleted_t callback, bool ioFailure)
	{
		lastQueryOwners = new List<ulong>();
		lastQueryFiles = new List<PublishedFileId_t>();
		if (callback.m_eResult == EResult.k_EResultOK)
		{
			lastQueryCount = (int)callback.m_unTotalMatchingResults;
			for (int i = 0; i < lastQueryCount; i++)
			{
				if (SteamUGC.GetQueryUGCResult(lastQueryHandle, (uint)i, out var pDetails))
				{
					lastQueryOwners.Add(pDetails.m_ulSteamIDOwner);
					lastQueryFiles.Add(pDetails.m_nPublishedFileId);
				}
			}
		}
		else
		{
			lastQueryFail = callback.m_eResult.ToString();
			lastQueryCount = -1;
		}
		isCurrentlyQuerying = false;
	}

	private void OnDownloadItemResult(DownloadItemResult_t callback)
	{
		if (callback.m_unAppID.m_AppId == APP_ID)
		{
			PublishedFileId_t nPublishedFileId = callback.m_nPublishedFileId;
			_ = callback.m_eResult;
			_ = 1;
			if (currentlyDownloading.Contains(nPublishedFileId))
			{
				currentlyDownloading.Remove(nPublishedFileId);
			}
		}
	}
}
