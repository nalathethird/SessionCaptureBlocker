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
	public override string Version => "1.0.2";
	public override string Link => "https://github.com/nalathethird/Fck-SessionCapture";

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
	private static readonly ModConfigurationKey<bool> captureInContactsPlus =
		new ModConfigurationKey<bool>("capture_in_contactsplus_session", "Allow capture in contacts+ sessions", () => false);

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<bool> captureInRegisteredUsers =
		new ModConfigurationKey<bool>("capture_in_registeredusers_session", "Allow capture in registered users sessions", () => false);

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<bool> captureInLAN =
		new ModConfigurationKey<bool>("capture_in_lan_session", "Allow capture in LAN (local network) sessions", () => false);

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<bool> captureInPublic =
		new ModConfigurationKey<bool>("capture_in_public_session", "Allow capture in public sessions (Anyone)", () => false);

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<bool> captureLocal =
		new ModConfigurationKey<bool>(
			"capture_local_session",
			"Allow local session capture (only visible to people inside the session; block to maximize privacy).",
			() => true
		);

	[AutoRegisterConfigKey]
	private static readonly ModConfigurationKey<bool> alwaysCapture =
		new ModConfigurationKey<bool>(
			"always_locally_capture",
			"Always locally capture thumbnails, even when not uploading or sharing with the local session.",
			() => false
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

	[HarmonyPatch(typeof(SessionThumbnailData), "ShouldCapture")]
	class SessionThumbnailData_ShouldCapture_Patch {
		static bool Prefix(SessionThumbnailData __instance) {
			if (Config == null) {
				Error("FckSessionCapture: Config is null! Allowing capture.");
				return true;
			}

			bool modEnabled = Config.GetValue(enabled);
			bool allowPrivate = Config.GetValue(captureInPrivate);
			bool allowContacts = Config.GetValue(captureInContactsOnly);
			bool allowContactsPlus = Config.GetValue(captureInContactsPlus);
			bool allowRegisteredUsers = Config.GetValue(captureInRegisteredUsers);
			bool allowLAN = Config.GetValue(captureInLAN);
			bool allowPublic = Config.GetValue(captureInPublic);
			bool allowLocal = Config.GetValue(captureLocal);
			bool locallyCapture = Config.GetValue(alwaysCapture);

			if (!modEnabled) {
				return true;
			}

			if (locallyCapture) {
				return true;
			}

			var world = __instance.World;
			if (world == null) {
				Error("FckSessionCapture: __instance.World is null! Allowing capture.");
				return true;
			}

			var accessLevel = world.AccessLevel;

			// Block or allow based on session type and config
			switch (accessLevel) {
				case SessionAccessLevel.Private:
					if (!allowLocal && !allowPrivate) {
						return false;
					}
					break;
				case SessionAccessLevel.Contacts:
					if (!allowLocal && !allowContacts) {
						return false;
					}
					break;
				case SessionAccessLevel.ContactsPlus:
					if (!allowLocal && !allowContactsPlus) {
						return false;
					}
					break;
				case SessionAccessLevel.RegisteredUsers:
					if (!allowLocal && !allowRegisteredUsers) {
						return false;
					}
					break;
				case SessionAccessLevel.LAN:
					if (!allowLocal && !allowLAN) {
						return false;
					}
					break;
				case SessionAccessLevel.Anyone:
					if (!allowLocal && !allowPublic) {
						return false;
					}
					break;
			}

			return true;
		}
	}

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
			bool allowContactsPlus = Config.GetValue(captureInContactsPlus);
			bool allowRegisteredUsers = Config.GetValue(captureInRegisteredUsers);
			bool allowLAN = Config.GetValue(captureInLAN);
			bool allowPublic = Config.GetValue(captureInPublic);
			bool allowLocal = Config.GetValue(captureLocal);

			Msg($"FckSessionCapture: Config - enabled={modEnabled}, private={allowPrivate}, contacts={allowContacts}, contactsPlus={allowContactsPlus}, registeredUsers={allowRegisteredUsers}, lan={allowLAN}, public={allowPublic}, local={allowLocal}");

			if (!modEnabled) {
				Msg("FckSessionCapture: Mod is disabled, allowing upload.");
				return true;
			}

			var world = __instance.World;
			if (world == null) {
				Error("FckSessionCapture: __instance.World is null! Allowing upload.");
				return true;
			}

			// Only block if this is the focused world (the user's active session)
			if (world.Focus != World.WorldFocus.Focused) {
				Msg($"FckSessionCapture: World '{world.Name}' is not focused (Focus={world.Focus}), allowing upload.");
				return true;
			}

			var accessLevel = world.AccessLevel;
			Msg($"FckSessionCapture: World '{world.Name}' (AccessLevel={accessLevel}, Focused={world.Focus == World.WorldFocus.Focused})");

			// Block or allow based on session type and config
			switch (accessLevel) {
				case SessionAccessLevel.Private:
					if (!allowPrivate) {
						Warn("Blocked session thumbnail upload in private session.");
						if (!allowLocal) {
							Warn("Also blocking local session capture in private session.");
							__instance.InvalidateThumbnail();
						}
						return false;
					}
					break;
				case SessionAccessLevel.Contacts:
					if (!allowContacts) {
						Warn("Blocked session thumbnail upload in contacts-only session.");
						if (!allowLocal) {
							Warn("Also blocking local session capture in contacts-only session.");
							__instance.InvalidateThumbnail();
						}
						return false;
					}
					break;
				case SessionAccessLevel.ContactsPlus:
					if (!allowContactsPlus) {
						Warn("Blocked session thumbnail upload in contacts+ session.");
						if (!allowLocal) {
							Warn("Also blocking local session capture in contacts+ session.");
							__instance.InvalidateThumbnail();
						}
						return false;
					}
					break;
				case SessionAccessLevel.RegisteredUsers:
					if (!allowRegisteredUsers) {
						Warn("Blocked session thumbnail upload in registered users session.");
						if (!allowLocal) {
							Warn("Also blocking local session capture in registered users session.");
							__instance.InvalidateThumbnail();
						}
						return false;
					}
					break;
				case SessionAccessLevel.LAN:
					if (!allowLAN) {
						Warn("Blocked session thumbnail upload in LAN session.");
						if (!allowLocal) {
							Warn("Also blocking local session capture in LAN session.");
							__instance.InvalidateThumbnail();
						}
						return false;
					}
					break;
				case SessionAccessLevel.Anyone:
					if (!allowPublic) {
						Warn("Blocked session thumbnail upload in public (Anyone) session.");
						if (!allowLocal) {
							Warn("Also blocking local session capture in public session.");
							__instance.InvalidateThumbnail();
						}
						return false;
					}
					break;
				default:
					Msg("FckSessionCapture: Unknown session type, allowing upload.");
					break;
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
