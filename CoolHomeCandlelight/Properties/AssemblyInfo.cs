﻿using MelonLoader;
using CoolHome;
using System.Reflection;
using System.Runtime.InteropServices;

//This is a C# comment. Comments have no impact on compilation.

//ModName, ModVersion, ModAuthor, and ModNamespace.ModClassInheritingFromMelonMod all need changed.

[assembly: AssemblyTitle("CoolHomeCandlelight")]
[assembly: AssemblyCopyright("yasenfire")]

//Version numbers in C# are a set of 1 to 4 positive integers separated by periods.
//Mods typically use 3 numbers. For example: 1.2.1
//The mod version need specified in three places.
[assembly: AssemblyVersion("0.4.5")]
[assembly: AssemblyFileVersion("0.4.5")]
[assembly: MelonInfo(typeof(CoolHomeCandlelight.CoolHomeCandlelight), "CoolHomeCandlelight", "0.4.5", "yasenfire", null)]

//This tells MelonLoader that the mod is only for The Long Dark.
[assembly: MelonGame("Hinterland", "TheLongDark")]