/**
 * Copyright (C) 2017 Patrick Mours. All rights reserved.
 * License: https://github.com/crosire/fritzbox-extcap#license
 */

using System;
using System.Runtime.InteropServices;

class Win32
{
	[DllImport("kernel32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall, SetLastError = true)]
	internal static extern Microsoft.Win32.SafeHandles.SafeFileHandle CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode, IntPtr SecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, IntPtr hTemplateFile);
}
