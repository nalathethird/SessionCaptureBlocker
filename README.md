# SessionCaptureBlocker

A [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader) mod for [Resonite](https://resonite.com/) that gives you full control over session capture privacy.

### This Mod is to prevent SessionCaptures to be Uploaded to a minimaly secure server. This mod is meant to fix a security exploit, until the Session Orbs's API is updated and Session Captures are a API Check first, to be able to see the Image.

**FckSessionCapture** allows you to block the automatic upload and/or local generation of session thumbnails ("session captures") in Resonite. This helps protect your privacy by preventing images of your session from being uploaded to the internet or even stored locally, depending on your configuration.

- Block session capture in sessions labled, Private, Contacts, LAN, and Public.
- Optionally block local session capture generation (Session Capture is Triggered and Cached on the Session/Host, so anyone can see the Session Capture you create, but not upload yet).
- All blocking is local: only affects your client, not others in the session.

> **Why?**  
> When you are in a session, Resonite uploads a 360° image of the session to a Image Caching server. This, while a lovely feature, can be accessed by anyone with the UUID of the Image. 
> This UUID, can be reverse enginered to scrape the website for any images that are not dead links. Yes, this also means any Private or LAN sessions you have ever had, are/have been at some point, is seen on an WEBP image PUBLICALLY ON THE INTERNET!
>This mod gives you control over when the local user opens a dashboard or Userspace UI, and sends an image of the current session to a image caching server for other sessions and people to see the Session Preview.
> The Mod I've created, intends to stop sending images to the Image Caching server, and can be ether removed or considered "Legacy", until theres at minimum, an API check to see if the user is able to view the Session Image.

## Installation
1. Install [ResoniteModLoader](https://github.com/resonite-modding-group/ResoniteModLoader).
2. Place [SessionCaptureBlocker.dll](https://github.com/nalathethird/SessionCaptureBlocker/releases/latest/download/SessionCaptureBlocker.dll) into your `rml_mods` folder.  
   This folder should be at `C:\Program Files (x86)\Steam\steamapps\common\Resonite\rml_mods` for a default install.  
   You can create it if it's missing, or if you launch the game once with ResoniteModLoader installed it will create this folder for you.
3. Start the game.  
   If you want to verify that the mod is working you can check your Resonite logs for messages from `FckSessionCapture`.

## Configuration

You can configure the mod in the ResoniteModLoader mod settings UI.  
Options include:

- **Enable SessionCaptureBlocker mod**: Master toggle for the mod.
- **Allow capture in private/contacts/LAN/public sessions**: Control uploads for each session type.
- **Allow local session capture**: If disabled, even local session captures are blocked (max privacy).

**Star this repo if it helped you!** ⭐ It keeps me motivated to maintain and improve my mods.

Or, if you want to go further, Support me on [Ko-fi!](https://ko-fi.com/nalathethird) ☕
It helps me pay bills, and other things someone whos unemployed cant pay!
****

---
