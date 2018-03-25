﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace Charlotte
{
	public class ProcessMan
	{
		public ProcessMan()
		{ }

		public enum Mode_e
		{
			非表示,
			表示,
			表示_最小化,
		};

		public Mode_e Mode = Mode_e.非表示;

		private Thread ProcStartTh;
		private Process Proc;
		private string LastCommandLine = "<none>";

		public void Start(string file, string args)
		{
			string commandLine = file + " " + args;

			if (this.Proc != null)
				throw new Exception("既に実行中です。" + this.LastCommandLine + " -> " + commandLine);

			this.Mode = Gnd.I.ProcMode; // app

			ProcessStartInfo psi = new ProcessStartInfo();

			psi.FileName = file;
			psi.Arguments = args;

			if (this.Mode == Mode_e.非表示)
			{
				psi.CreateNoWindow = true;
				psi.UseShellExecute = false;
			}
			else
			{
				psi.CreateNoWindow = false;
				psi.UseShellExecute = true;

				if (this.Mode == Mode_e.表示_最小化)
					psi.WindowStyle = ProcessWindowStyle.Minimized;
			}

			this.ProcStartTh = new Thread(() =>
			{
				this.Proc = Process.Start(psi);
			});

			this.ProcStartTh.Start();
			this.LastCommandLine = commandLine;
		}

		public bool IsEnd()
		{
			if (this.ProcStartTh != null)
			{
				if (this.ProcStartTh.Join(0) == false)
					return false;

				this.ProcStartTh = null;
			}
			if (this.Proc == null)
				return true;

			if (this.Proc.HasExited)
			{
				this.Proc.Close();
				this.Proc = null;
				return true;
			}
			return false;
		}

		public void End()
		{
			if (this.ProcStartTh != null)
			{
				this.ProcStartTh.Join();
				this.ProcStartTh = null;
			}
			if (this.Proc == null)
				return;

			if (this.Proc.HasExited == false)
			{
				this.Proc.WaitForExit();
				this.Proc.Close();
				this.Proc = null;
			}
		}
	}
}
