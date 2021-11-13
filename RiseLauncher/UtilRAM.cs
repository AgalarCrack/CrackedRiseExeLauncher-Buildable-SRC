using System;
using Microsoft.VisualBasic.Devices;

namespace RiseLauncher
{
	internal class UtilRAM
	{
		private static ulong GetTotalMemoryInBytes()
		{
			return new ComputerInfo().TotalPhysicalMemory;
		}

		public static int getRam()
		{
			return Convert.ToInt32(UtilRAM.GetTotalMemoryInBytes() / 1073741824UL);
		}
	}
}
