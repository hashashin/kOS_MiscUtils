using System;
using System.Text;
using MuMech;

using kOS.Safe.Encapsulation;
using System.Collections.Generic;

namespace kOS.AddOns.kOSMiscUtils
{
	[kOSAddon("MISCUTILS")]
	[kOS.Safe.Utilities.KOSNomenclature("MiscUtilsAddon")]
	public class Addon : Suffixed.Addon
	{
		public Addon(SharedObjects shared) : base(shared)
		{
			InitializeSuffixes();
		}

		void InitializeSuffixes()
		{
			AddSuffix("REALTIME", new kOS.Safe.Encapsulation.Suffixes.NoArgsSuffix<StringValue>(GetRealTime, "Get the current human world time"));
			AddSuffix("REALDATE", new kOS.Safe.Encapsulation.Suffixes.NoArgsSuffix<StringValue>(GetRealDate, "Get the current human world date"));
			AddSuffix("SUNLIGHT", new kOS.Safe.Encapsulation.Suffixes.NoArgsSuffix<BooleanValue>(GetAmblight, "It's direct sun light?"));
			AddSuffix("GPSBIO", new kOS.Safe.Encapsulation.Suffixes.NoArgsSuffix<StringValue>(GetGpsBio, "Obtains current biome name from ModuleGPS"));
			AddSuffix("GPSLAT", new kOS.Safe.Encapsulation.Suffixes.NoArgsSuffix<StringValue>(GetGpsLat, "Obtains current latitude from ModuleGPS"));
			AddSuffix("GPSLON", new kOS.Safe.Encapsulation.Suffixes.NoArgsSuffix<StringValue>(GetGpsLon, "Obtains current longitude from ModuleGPS"));
			AddSuffix("TOFLOAT", new kOS.Safe.Encapsulation.Suffixes.OneArgsSuffix<ScalarDoubleValue, StringValue>(ToFloat, "Converts given string to a float number"));
			AddSuffix("HIDECUR", new kOS.Safe.Encapsulation.Suffixes.NoArgsSuffix<StringValue>(HideCursor, "Disable terminal cursor"));
			AddSuffix("SHOWCUR", new kOS.Safe.Encapsulation.Suffixes.NoArgsSuffix<StringValue>(ShowCursor, "Enable terminal cursor"));
			AddSuffix("RNDID", new kOS.Safe.Encapsulation.Suffixes.TwoArgsSuffix<StringValue, ScalarIntValue, ScalarIntValue>(GetRndID, "Get a random ID in base 62 or 36 with any given number of chars"));
			AddSuffix("KTM", new kOS.Safe.Encapsulation.Suffixes.NoArgsSuffix<BooleanValue>(GetKTM, "It's Kerbin time active?"));
			AddSuffix("LAUNCHMJ", new kOS.Safe.Encapsulation.Suffixes.OneArgsSuffix<StringValue, ScalarValue>(LaunchWithMechJeb, "Launch a vessel with mechjeb to desired orbit altitude"));
		}

		public override BooleanValue Available()
		{
			return true;
		}

		StringValue GetRealTime()
		{
			var date = DateTime.Now;
			return (date.Hour + ":" + date.Minute + ":" + date.Second);
		}

		StringValue GetRealDate()
		{
			var date = DateTime.Now;
			return (date.Day + "/" + date.Month + "/" + date.Year);
		}

		BooleanValue GetAmblight()
		{
			return FlightGlobals.ActiveVessel.directSunlight;
		}

		StringValue GetGpsBio()
		{
			var gpsdata = FlightGlobals.ActiveVessel.FindPartModulesImplementing<ModuleGPS>();
			if (gpsdata.Count > 0)
			{
				return gpsdata[0].bioName;
			}
			return "error: no gps data";
		}

		StringValue GetGpsLat()
		{
			var gpsdata = FlightGlobals.ActiveVessel.FindPartModulesImplementing<ModuleGPS>();
			if (gpsdata.Count > 0)
			{
				return gpsdata[0].lat.Split(Convert.ToChar(" "))[1].Replace("[", "").Replace("]", "");
			}
			return "error: no gps data";
		}

		StringValue GetGpsLon()
		{
			var gpsdata = FlightGlobals.ActiveVessel.FindPartModulesImplementing<ModuleGPS>();
			if (gpsdata.Count > 0)
			{
				return gpsdata[0].lon.Split(Convert.ToChar(" "))[1].Replace("[", "").Replace("]", "");
			}
			return "error: no gps data";
		}

		ScalarDoubleValue ToFloat(StringValue argOne)
		{
			var number =
				Convert.ToDouble(
					(argOne.ToString().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)[0]));
			return number;
		}

		StringValue HideCursor()
		{
			if (shared.Window.ShowCursor)
			{
				shared.Window.ShowCursor = false;
				return "";
			}
			return "";
		}

		StringValue ShowCursor()
		{
			if (!shared.Window.ShowCursor)
			{
				shared.Window.ShowCursor = true;
				return "";
			}
			return "";
		}

		StringValue GetRndID(ScalarIntValue _base, ScalarIntValue _chars)
		{
			if (_chars < 0)
			{
				return "error: " + _chars + " it's not a valid number of characters";
			}
			if (_base == 36)
			{
				return utils.GetBase36(_chars);
			}
			if (_base == 62)
			{
				return utils.GetBase62(_chars);
			}
			return "error: " + _base + " is not a valid base (use 36 or 62)";
		}

		BooleanValue GetKTM()
		{
			return GameSettings.KERBIN_TIME;
		}

		StringValue LaunchWithMechJeb(ScalarValue alt)
		{
			MechJebCore activejeb = utils.GetJeb();
			MechJebModuleAscentAutopilot activeasc =
				activejeb?.GetComputerModule("MechJebModuleAscentAutopilot") as MechJebModuleAscentAutopilot;
			if (activeasc != null)
			{
				activeasc.desiredOrbitAltitude.val = alt;
				activeasc.enabled = true;
				return "";
			}
			return "error: no Mechjeb found in this vessel";
		}

		static class utils // code from: http://stackoverflow.com/questions/9543715/generating-human-readable-usable-short-but-unique-ids
		{
			static char[] _base62chars =
				"0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz"
				.ToCharArray();

			static System.Random _random = new System.Random();

			public static string GetBase62(int length)
			{
				var sb = new StringBuilder(length);

				for (int i = 0; i < length; i++)
					sb.Append(_base62chars[_random.Next(62)]);

				return sb.ToString();
			}
			public static string GetBase36(int length)
			{
				var sb = new StringBuilder(length);

				for (int i = 0; i < length; i++)
					sb.Append(_base62chars[_random.Next(36)]);

				return sb.ToString();
			}

			public static MechJebCore GetJeb()
			{
				MechJebCore activemech = null;

				List<Part> p = FlightGlobals.ActiveVessel.parts;

				foreach (Part thatpart in p)
				{
					foreach (PartModule thatmodule in thatpart.Modules)
					{
						if (thatmodule is MechJebCore)
						{
							var thatmechjeb = thatmodule as MechJebCore;
							activemech = thatmechjeb.MasterMechJeb;
						}
					}
				}
				return activemech;
			}
		}
	}
}
