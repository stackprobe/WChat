﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Security.Permissions;

namespace Charlotte
{
	public partial class MainWin : Form
	{
		#region ALT_F4 抑止

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		protected override void WndProc(ref Message m)
		{
			const int WM_SYSCOMMAND = 0x112;
			const long SC_CLOSE = 0xF060L;

			if (m.Msg == WM_SYSCOMMAND && (m.WParam.ToInt64() & 0xFFF0L) == SC_CLOSE)
			{
				this.BeginInvoke((MethodInvoker)delegate
				{
					this.CloseWindow();
				});

				return;
			}
			base.WndProc(ref m);
		}

		#endregion

		public MainWin()
		{
			InitializeComponent();
		}

		private void MainWin_Load(object sender, EventArgs e)
		{
			// noop
		}

		private void MainWin_Shown(object sender, EventArgs e)
		{
			Gnd.I.Init_Files(); // zantei -- ウィルス対策ソフトか何かで FJammer.Decode が重いことがある。
			this.RefreshUi();
		}

		private void MainWin_FormClosing(object sender, FormClosingEventArgs e)
		{
			Gnd.I.DoSave();
		}

		private void MainWin_FormClosed(object sender, FormClosedEventArgs e)
		{
			// noop
		}

		private void 終了XToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.CloseWindow();
		}

		private void CloseWindow()
		{
			this.Visible = false;

			// プロセス終了時にすること
			{
				Gnd.I.ConsoleProcEnd(true);
			}

			this.Close();
		}

		private void 設定SToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.Visible = false;
			Gnd.I.ConsoleProcEnd(true);

			using (SettingWin f = new SettingWin())
			{
				f.ShowDialog();
			}
			Gnd.I.DoSave();
			this.Visible = true;
			this.RefreshUi();
		}

		private void 開始SToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Gnd.I.ServerStartFlag = true;
			this.RefreshUi();
		}

		private void 停止TToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Gnd.I.ServerStartFlag = false;
			this.RefreshUi();
		}

		private void ファイル転送サーバーFToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Gnd.I.RevServerEnabled = Gnd.I.RevServerEnabled == false;
			Gnd.I.DoSave();
			this.RefreshUi();
		}

		private void RefreshUi()
		{
			this.開始SToolStripMenuItem.Checked = Gnd.I.ServerStartFlag;
			this.停止TToolStripMenuItem.Checked = Gnd.I.ServerStartFlag == false;
			this.ファイル転送サーバーFToolStripMenuItem.Checked = Gnd.I.RevServerEnabled;

			this.SetStatus(Gnd.I.ServerStartFlag, Gnd.I.RevServerEnabled);

			Gnd.I.ConsoleProcBegin();
		}

		private void SetStatus(bool statChatSv, bool statFileSv)
		{
			this.ChatSvStatus.Text = statChatSv ? "開始" : "停止";
			this.FileSvStatus.Text = statFileSv ? "有効" : "無効";

			if (statChatSv)
			{
				this.ChatSvStatus.ForeColor = Color.Blue;
				this.FileSvStatusLabel.Enabled = true;
				this.FileSvStatus.Enabled = true;

				if (statFileSv)
					this.FileSvStatus.ForeColor = Color.Blue;
				else
					this.FileSvStatus.ForeColor = Color.DarkRed;
			}
			else
			{
				this.ChatSvStatus.ForeColor = Color.DarkRed;
				this.FileSvStatusLabel.Enabled = false;
				this.FileSvStatus.Enabled = false;
			}
		}
	}
}
