using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Windows.Forms;

//			MessageBox.Show("oi","eae",MessageBoxButtons.YesNo);

namespace HideProcess{
	class tpc_client{
		static void Main(string[] args){
			if(args.Length==0){
				System.Environment.Exit(1);
			}
			System.Diagnostics.Process process = new System.Diagnostics.Process();
			System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
			startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
			startInfo.FileName = "cmd.exe";
			startInfo.Arguments = "/C cls && csc /target:winexe "+args[0]+" && start "+args[0].Replace(".cs", "")+".exe";
			process.StartInfo = startInfo;
			process.Start();
		}
	}
}