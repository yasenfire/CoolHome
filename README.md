# CoolHome
This is a mod for The Long Dark by Hinterland Studio, Inc.

Created by yasenfire

## Description

<img src="https://github.com/yasenfire/CoolHome/blob/main/Images/CoolHome.png"><br>

This island was full of life. Now everyone is dead or left. Buildings died with their inhabitants: with no one to maintain them they cooled down, turned into frozen husks. There is no safe shelter anymore, so you'll die too. That is if you aren't ready to work hard to make with your hands what was given for granted before: to make your home warm.

This mod removes all (or almost all) temperature bonuses from being inside. Every shelter you can enter will be at ambient temperature. However, by using heat sources inside you're able to warm it back. The heat will remain for long time, though eventually every shelter will cool down. Real (even if simplified) physics is used to calculate temperature inside. Small buildings will heat faster than large ones. Concrete is worse for insulation than wood. Choose your shelter wise.

As every shelter requires specific metadata for temperature calculations to work correctly, not everything is supported yet. Unsupported buildings will behave like Camp Office in Mystery Lake temperature-wise. Updates are coming!

### Update v0.4.0 Not Cool

The temporary version, mainly published to restore functionality for the actual TLD version. However, there are still new features:

- Electric devices generate heat during aurora. Lamps and computers will make your home warmer a bit (even if you destroy them. Don't!)
- Additional option to enable in mod settings adds invisible electric heating devices powered during aurora in some large industrial buildings (The Dam for now).
- Medium, large and colossal buildings made smaller (in heat calculations) than they really should be, but their behavior in-game makes slightly more sense that way.
- I like mods, so I decided to put mods in my mod. For now there's just a little compatibility mod between CoolHome and CandleLight, but probably there will be more.

All in all, it's very raw and only a plug till some real work is done. Sorry for the quality, sorry for the delay.

### Update v0.3.0 Cold Greeting

- Wall heat loss is now counted in more physically correct way. Thermodynamics is not your friend. Nights will be colder.
- In addition to wall heat loss there is also ventilation heat loss. Nights will be much colder.
- Vehicles are now heatable. Only trucks and the broken plane in Mystery Lake are fully supported though, everything else will behave like trucks.
- Thermic profiles now can override more properties, including wall material properties, size and ventilation dynamics. Caves will be warmer. Eventually. Not now.
- Some more stability fixes.

### Update v0.2.2

- Storm lantern heat is changed to more scientifically correct 1.5KW. Days will be warmer.
- Fixed the bug that was crashing the game when entering some buildings.
- ModSettings should work correctly now.

### Update v0.2.0

- Blizzard and windchill temperature changes now don't apply to inside, instead they influence shelters' heat loss. Nights will be colder.
- Flares, torches, storm lanterns and matches are now able to warm environment. The heat power of a flare is 1KW, 800W for a torch, 400W for a lantern and mere 60W for a match. But desperate can't choose; maybe this box of matches will save your life.
- ModSettings is supported. Some basic settings are added. You can change heat gain and heat loss multipliers. You can set fires to change radiated heat based on their temperature. This would completely break vanilla, so only use it with mods that fix fires' thermodynamics (or just to make the game easier). With this setting enabled a 10C fire will radiate as much heat as any fire with the setting off, 20C fire twice the amount and so on.
- More thermic profiles added. Mystery Lake cabins, trailers and all prepper caches in the game excluding Signal Void ones should be supported now. The Dam is partially supported. Try to warm it up! Feel free to edit or delete thermic profiles if you don't like how they work.
- Some bugfixes to make the mod more stable.

### Update v0.1.1

- Player breath is now able to warm environment. For mod's purposes a player is interpreted as a mobile 100W heater. Days will be warmer.

### Update v0.1.0

- No-loading-area buildings now should give correct temperature bonus.
- Windows are added into calculations. Windows are always a vulnerability and lose heat. In day it slows down due to sunlight bringing some solar power in. Nights will be colder.
- There is a small test set of profiles, their purpose is to add some individuality to shelters by making them behave differently. The folder should be placed in Mods/, same principle as in AmbientLights. The set is very short now: besides Camp Office it includes Trapper's Homestead, Forestry Lookout (small wooden buildings) and three small granite caves in Mystery Lake. More will be added soon.

## Installation
* Move **CoolHome.dll** and **CoolHome.json** in the downloaded ZIP to the Mods folder. Move **CoolHome** folder to the Mods folder.

## Useful Mod Combo

* Extreme Drop Temperature by The Illusion to make nights even colder.
* Cozy Blankets by Jods and Cozy Cushions by DZ. Is this mod too cool for you? Sit down on one of these cushions to get additional warmth.
* Bedroll Tweaker by Cass to stack temperature bonus from bedrolls.
* Solstice by WulfMarius and Romain. What can be worse than nights getting colder with each update? Nights getting longer!
* FireRV by Deus. Thermodynamics is your friend. (No, no, it really isn't, not at all. No) Use it with temperature-based heat generation
* Indoor Campfire by Xpazeman. Can be the only way to make some places warm.

## Special Thanks
The Long Dark Modding Server (discord)