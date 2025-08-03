using HarmonyLib;
using ResoniteModLoader;
using FrooxEngine;
using SkyFrost.Base;
using System;
using System.Reflection;

namespace FckSessionCapture;

public class FckSessionCapture : ResoniteMod {
	public override string Name => "FckSessionCapture";
	public override string Author => "NalaTheThird";
	public override string Version => "1.0.0";
	public override string Link => "https://github.com/nalathethird/FckSessionCapture";

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<bool> enabled =
		new ModConfigurationKey<bool>("enabled", "Enable FckSessionCapture mod", () => true);

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<bool> captureInPrivate =
		new ModConfigurationKey<bool>("capture_in_private_session", "Allow capture in private sessions", () => false);

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<bool> captureInContactsOnly =
		new ModConfigurationKey<bool>("capture_in_contactsonly_session", "Allow capture in contacts-only sessions", () => false);

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<bool> captureInLAN =
		new ModConfigurationKey<bool>("capture_in_lan_session", "Allow capture in LAN (local network) sessions", () => false);

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<bool> captureInPublic =
		new ModConfigurationKey<bool>("capture_in_public_session", "Allow capture in public sessions", () => false);

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<bool> captureLocal =
		new ModConfigurationKey<bool>(
			"capture_local_session",
			"Allow local session capture (only visible to people inside the session; block to maximize privacy).",
			() => true
		);

	private static ModConfiguration Config;

	public override void OnEngineInit() {
		Msg("FckSessionCapture: OnEngineInit called.");
		Config = GetConfiguration();
		Config.Save(true);

		var harmony = new Harmony("com.nalathethird.fcksessioncapture");
		harmony.PatchAll();
		Msg("FckSessionCapture: Harmony patches applied.");
	}

	// Patch StartUpload to block uploads based on config and session type
	[HarmonyPatch(typeof(SessionThumbnailData), "StartUpload")]
	class SessionThumbnailData_StartUpload_Patch {
		static bool Prefix(SessionThumbnailData __instance) {
			Msg("FckSessionCapture: StartUpload Prefix entered.");

			if (Config == null) {
				Error("FckSessionCapture: Config is null! Allowing upload.");
				return true;
			}

			bool modEnabled = Config.GetValue(enabled);
			bool allowPrivate = Config.GetValue(captureInPrivate);
			bool allowContacts = Config.GetValue(captureInContactsOnly);
			bool allowLAN = Config.GetValue(captureInLAN);
			bool allowPublic = Config.GetValue(captureInPublic);
			bool allowLocal = Config.GetValue(captureLocal);

			Msg($"FckSessionCapture: Config - enabled={modEnabled}, private={allowPrivate}, contacts={allowContacts}, lan={allowLAN}, public={allowPublic}, local={allowLocal}");

			if (!modEnabled) {
				Msg("FckSessionCapture: Mod is disabled, allowing upload.");
				return true;
			}

			var world = __instance.World;
			if (world == null) {
				Error("FckSessionCapture: __instance.World is null! Allowing upload.");
				return true;
			}

			var accessLevel = world.AccessLevel;
			bool isPublic = world.IsPublic;

			Msg($"FckSessionCapture: World '{world.Name}' (AccessLevel={accessLevel}, IsPublic={isPublic})");

			// Block in Private sessions
			if (accessLevel == SessionAccessLevel.Private) {
				Msg("FckSessionCapture: Detected Private session.");
				if (!allowPrivate) {
					Warn("Blocked session thumbnail upload in private session.");
					if (!allowLocal) {
						Warn("Also blocking local session capture in private session.");
						__instance.InvalidateThumbnail();
					}
					return false;
				} else {
					Msg("FckSessionCapture: Allowed upload in private session (config).");
				}
			}
			// Block in Contacts/ContactsPlus sessions
			else if (accessLevel == SessionAccessLevel.Contacts || accessLevel == SessionAccessLevel.ContactsPlus) {
				Msg("FckSessionCapture: Detected Contacts/ContactsPlus session.");
				if (!allowContacts) {
					Warn("Blocked session thumbnail upload in contacts-only session.");
					if (!allowLocal) {
						Warn("Also blocking local session capture in contacts-only session.");
						__instance.InvalidateThumbnail();
					}
					return false;
				} else {
					Msg("FckSessionCapture: Allowed upload in contacts-only session (config).");
				}
			}
			// Block in LAN sessions
			else if (accessLevel == SessionAccessLevel.LAN) {
				Msg("FckSessionCapture: Detected LAN session.");
				if (!allowLAN) {
					Warn("Blocked session thumbnail upload in LAN session.");
					if (!allowLocal) {
						Warn("Also blocking local session capture in LAN session.");
						__instance.InvalidateThumbnail();
					}
					return false;
				} else {
					Msg("FckSessionCapture: Allowed upload in LAN session (config).");
				}
			}
			// Block in Public sessions
			else if (isPublic) {
				Msg("FckSessionCapture: Detected Public session.");
				if (!allowPublic) {
					Warn("Blocked session thumbnail upload in public session.");
					if (!allowLocal) {
						Warn("Also blocking local session capture in public session.");
						__instance.InvalidateThumbnail();
					}
					return false;
				} else {
					Msg("FckSessionCapture: Allowed upload in public session (config).");
				}
			} else {
				Msg("FckSessionCapture: Session type did not match any known block type. Allowing upload.");
			}

			// If upload is allowed, but local capture is not, clear the local thumbnail
			if (!allowLocal) {
				Warn("Blocking local session capture (even though upload is allowed).");
				__instance.InvalidateThumbnail();
			}

			Msg("FckSessionCapture: StartUpload Prefix exiting, allowing upload.");
			return true;
		}
	}
}
