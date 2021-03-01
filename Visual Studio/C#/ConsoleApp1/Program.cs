using System;
using Eruru.QQMini.PluginSDKDebugger;

namespace ConsoleApp1 {

	class Program {

		static void Main (string[] args) {
			Console.Title = nameof (ConsoleApp1);
#if DEBUG
			QMDebugger.IsDebugConfigure = true;
#else
			QMDebugger.IsDebugConfigure = false;
#endif
			QMDebugger.StartByRelative ("com.eruru.warframe");
		}

	}

}