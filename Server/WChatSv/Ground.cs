﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Charlotte
{
	public class Gnd
	{
		public static Gnd I = new Gnd();

		private Gnd()
		{ }

		// ---- setting ----

		public int ChatSvPort = 59999;
		public int RevServerPort = 60001;
		public bool RevServerEnabled = true;
		public ProcessMan.Mode_e ProcMode = ProcessMan.Mode_e.非表示;

		// ----

		public ProcessMan ChatSv = new ProcessMan();
		public ProcessMan RevServer = new ProcessMan();
		public bool ServerStartFlag = true;

		private static string DAT_FILE
		{
			get
			{
				return EraseExt(BootTools.SelfFile) + ".dat";
			}
		}

		public static string EraseExt(string path)
		{
			int index = path.LastIndexOf('.');

			if (index != -1 && path.IndexOf('\\', index + 1) == -1)
				path = path.Substring(0, index);

			return path;
		}

		private static readonly Encoding DAT_FILE_ENCODING = Encoding.ASCII;

		public void DoLoad()
		{
			if (File.Exists(DAT_FILE) == false)
				return;

			string[] l = File.ReadAllLines(DAT_FILE, DAT_FILE_ENCODING);
			int c = 0;

			this.ChatSvPort = int.Parse(l[c++]);
			this.RevServerPort = int.Parse(l[c++]);
			this.RevServerEnabled = int.Parse(l[c++]) != 0;
			this.ProcMode = (ProcessMan.Mode_e)int.Parse(l[c++]);
		}

		public void DoSave()
		{
			List<string> l = new List<string>();

			l.Add("" + this.ChatSvPort);
			l.Add("" + this.RevServerPort);
			l.Add("" + (this.RevServerEnabled ? 1 : 0));
			l.Add("" + (int)this.ProcMode);

			File.WriteAllLines(DAT_FILE, l, DAT_FILE_ENCODING);
		}

		private static string GetFile(string file, string dir)
		{
			if (File.Exists(file) == false)
				file = Path.Combine(dir, file);

			return file;
		}

		private static string GetFile_FJ(string file, string dir)
		{
			FJammer.Decode(file);
			return GetFile(file, dir);
		}

		private string ChatSvFile;
		private string RevServerFile;

		public void Init_Files()
		{
			ChatSvFile = GetFile_FJ("ChatSv.exe", @"C:\Factory\SubTools\Chat");
			RevServerFile = GetFile_FJ("revServer.exe", @"C:\Factory\Labo\Socket\tunnel");
		}

		public void ConsoleProcBegin()
		{
			this.ConsoleProcEnd();

			if (Gnd.I.ServerStartFlag == false)
				return;

			Gnd.I.ChatSv.Start(this.ChatSvFile, "" + this.ChatSvPort);

			if (Gnd.I.RevServerEnabled)
				Gnd.I.RevServer.Start(this.RevServerFile, this.RevServerPort + " a 1");
		}

		public void ConsoleProcEnd()
		{
			if (this.ConsoleProcEndTimer() == false)
			{
				using (BusyDlg f = new BusyDlg(this.ConsoleProcEndTimer))
				{
					f.ShowDialog();
				}
			}
		}

		private IEnumerator<bool> CPET_IE = null;

		private IEnumerable<bool> CPET_GetIE()
		{
			ProcessMan pm = new ProcessMan();

			for (; ; )
			{
				while (this.ChatSv.IsEnd() && this.RevServer.IsEnd())
				{
					yield return true;
				}
				if (this.ChatSv.IsEnd() == false)
				{
					pm.Start(this.ChatSvFile, "/S " + Gnd.I.ChatSvPort);

					do
					{
						yield return false;
					}
					while (pm.IsEnd() == false);
				}
				if (this.RevServer.IsEnd() == false)
				{
					pm.Start(this.RevServerFile, Gnd.I.RevServerPort + " a 1 /S");

					do
					{
						yield return false;
					}
					while (pm.IsEnd() == false);
				}
				for (int c = 0; c < 20; c++)
				{
					yield return false;
				}
			}
		}

		public bool ConsoleProcEndTimer()
		{
			if (this.CPET_IE == null)
				this.CPET_IE = this.CPET_GetIE().GetEnumerator();

			if (this.CPET_IE.MoveNext() == false)
				throw null; // never

			return this.CPET_IE.Current;
		}

		public bool Is初回起動()
		{
			return File.Exists(DAT_FILE) == false; // ? DoSave()を1度も実行していない。
		}
	}
}
