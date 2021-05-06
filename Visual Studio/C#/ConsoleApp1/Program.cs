using System;
using Eruru.QQMiniDebugger;

namespace ConsoleApp1 {

	class Program {

		static void Main (string[] args) {
			Console.Title = nameof (ConsoleApp1);
#if DEBUG
			QMDebugger.IsDebugConfigure = true;
#else
			QMDebugger.IsDebugConfigure = false;
#endif
			QMDebugger.StartByPackageId ("com.eruru.warframe");
		}

	}

}