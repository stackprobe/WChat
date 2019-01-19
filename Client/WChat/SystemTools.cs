using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Text;
using System.Security.Cryptography;
using System.Net;
using System.Management;
using System.Windows.Forms;

namespace Charlotte
{
	public class SystemTools
	{
		private static bool WL_Enabled = false;
		private static int WL_Count = 0;

		public static void WL_Start()
		{
			WL_Enabled = true;
			File.Delete(GetLogFile());
		}

		public static bool WL_MainWinStatus_Enabled = true;

		public static void WriteLog(object message)
		{
			try
			{
				if (WL_Enabled == false)
					return;

				if (WL_MainWinStatus_Enabled)
				{
					string tmpMsg = "" + message;

					int index = StringTools.IndexOfChar(tmpMsg, "\r\n");

					if (index != -1)
						tmpMsg = tmpMsg.Substring(0, index);

					EventCenter.I.AddEvent(Consts.EVENT_PREFERENCE, delegate
					{
						Gnd.I.MainWin.SetStatusMessage(tmpMsg);
					});
				}

				using (StreamWriter sw = new StreamWriter(GetLogFile(), WL_Count++ % 1000 != 0, StringTools.ENCODING_SJIS))
				{
					sw.WriteLine("[" + DateTime.Now + "." + WL_Count.ToString("D3") + "] " + message);
				}
			}
			catch
			{ }
		}

		public static string GetLogFile()
		{
			return FileTools.EraseExt(BootTools.SelfFile) + ".log";
		}

		public static string GetSaveDataFile()
		{
			return FileTools.EraseExt(BootTools.SelfFile) + ".dat";
		}

		public static string GetTmp()
		{
			return GetEnv("TMP", @"C:\temp");
		}

		public static string GetEnv(string name, string defval)
		{
			try
			{
				string value = Environment.GetEnvironmentVariable(name);

				if (value == null || value.Length == 0)
					value = defval;

				return value;
			}
			catch
			{
				return defval;
			}
		}

		public class Discontinued : Exception
		{ }

		public static List<string> GetAllFontFamily()
		{
			List<string> result = new List<string>();

			foreach (FontFamily family in new InstalledFontCollection().Families)
			{
				result.Add(family.Name);
			}
			return result;
		}

		public static bool IsBrightColor(Color color)
		{
			return 0xc0 * 3 < color.R + color.G + color.B;
		}

		public delegate bool IsSame_d<T>(T a, T b);

		private static RNGCryptoServiceProvider _rngc = new RNGCryptoServiceProvider();

		public static uint GetCryptoRand()
		{
			byte[] buff = new byte[4];

			_rngc.GetBytes(buff);

			return
				((uint)buff[0] << 24) |
				((uint)buff[1] << 16) |
				((uint)buff[2] << 8) |
				((uint)buff[3] << 0);
		}

		public static uint GetCryptoRand(uint modulo)
		{
			return GetCryptoRand() % modulo; // FIXME
		}

		public static uint GetCryptoRand(uint minval, uint maxval)
		{
			return GetCryptoRand(maxval + 1 - minval) + minval;
		}

		public static string GetEnvHashCode()
		{
			try
			{
				//return "DVSN:" + GetHDDVolumeSerial() + ":IP:" + GetSelfIPAddress();
				return "DVSN:" + GetHDDVolumeSerial();
			}
			catch
			{ }

			return "EHC_UNKNOWN";
		}

		public static string GetHDDVolumeSerial()
		{
			try
			{
				using (ManagementObject mo = new ManagementObject("Win32_LogicalDisk=\"" + BootTools.SelfDir.Substring(0, 2) + "\""))
				{
					string ret = "" + mo.Properties["VolumeSerialNumber"].Value;
					ret = StringTools.ToContainsOnly(ret, StringTools.DIGIT + StringTools.ALPHA + StringTools.alpha);

					if (ret == "")
						throw null;

					return ret;
				}
			}
			catch
			{ }

			return "DVSN_UNKNOWN";
		}

		public static string GetSelfIPAddress()
		{
			try
			{
				List<string> ips = new List<string>();

				foreach (IPAddress ipa in Dns.GetHostAddresses(Dns.GetHostName()))
				{
					string ip = "" + ipa;

					if (ip.Contains(':')) // ignore v6
						continue;

					ips.Add(ip);
				}
				string ret = string.Join(":", ips);
				ret = StringTools.ToContainsOnly(ret, StringTools.DIGIT + ".:");

				if (ret == "")
					throw null;

				return ret;
			}
			catch
			{ }

			return "IP_UNKNOWN";
		}

		public static void AntiWindowsDefenderSmartScreen()
		{
			WriteLog("awdss_1");

			if (Gnd.I.Sd.Is初回起動())
			{
				WriteLog("awdss_2");

				foreach (string exeFile in Directory.GetFiles(BootTools.SelfDir, "*.exe", SearchOption.TopDirectoryOnly))
				{
					try
					{
						WriteLog("awdss_exeFile: " + exeFile);

						if (exeFile.ToLower() == BootTools.SelfFile.ToLower())
						{
							WriteLog("awdss_self_noop");
						}
						else
						{
							byte[] exeData = File.ReadAllBytes(exeFile);
							File.Delete(exeFile);
							File.WriteAllBytes(exeFile, exeData);
						}
						WriteLog("awdss_OK");
					}
					catch (Exception e)
					{
						WriteLog(e);
					}
				}
				WriteLog("awdss_3");
			}
			WriteLog("awdss_4");
		}

		public static void PostShown(Form f)
		{
			List<Control.ControlCollection> controlTable = new List<Control.ControlCollection>();

			controlTable.Add(f.Controls);

			for (int index = 0; index < controlTable.Count; index++)
			{
				foreach (Control control in controlTable[index])
				{
					GroupBox gb = control as GroupBox;

					if (gb != null)
					{
						controlTable.Add(gb.Controls);
					}
					TabControl tc = control as TabControl;

					if (tc != null)
					{
						foreach (TabPage tp in tc.TabPages)
						{
							controlTable.Add(tp.Controls);
						}
					}
					SplitContainer sc = control as SplitContainer;

					if (sc != null)
					{
						controlTable.Add(sc.Panel1.Controls);
						controlTable.Add(sc.Panel2.Controls);
					}
					TextBox tb = control as TextBox;

					if (tb != null)
					{
						if (tb.ContextMenuStrip == null)
						{
							ToolStripMenuItem item = new ToolStripMenuItem();

							item.Text = "項目なし";
							item.Enabled = false;

							ContextMenuStrip menu = new ContextMenuStrip();

							menu.Items.Add(item);

							tb.ContextMenuStrip = menu;
						}
					}
				}
			}
		}
	}
}
