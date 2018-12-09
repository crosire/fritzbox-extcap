/**
 * Copyright (C) 2017 Patrick Mours. All rights reserved.
 * License: https://github.com/crosire/fritzbox-extcap#license
 */

using System;
using System.IO;

class Program
{
	static void Main(string[] args)
	{
		bool startCapture = false;
		bool showExtcapDlts = false;
		bool showExtcapConfig = false;
		string fifo = string.Empty;
		string username = string.Empty;
		string password = string.Empty;
		string ifaceorminor = string.Empty;
		int snaplen = 1600;

		for (int i = 0; i < args.Length; i++)
		{
			if (args[i] == "--extcap-version")
			{
				PrintExtcapVersion();
				return;
			}
			if (args[i] == "--extcap-interfaces")
			{
				PrintExtcapInterfaces();
				return;
			}

			if (args[i] == "--capture")
				startCapture = true;
			if (args[i] == "--extcap-dlts")
				showExtcapDlts = true;
			if (args[i] == "--extcap-config")
				showExtcapConfig = true;

			if (i + 1 < args.Length)
			{
				if (args[i] == "--extcap-interface")
					i += 1;
				if (args[i] == "--fifo")
					fifo = args[i += 1];
				if (args[i] == "--username")
					username = args[i += 1];
				if (args[i] == "--password")
					password = args[i += 1];
				if (args[i] == "--ifaceorminor")
					ifaceorminor = args[i += 1];
				if (args[i] == "--snaplen")
					snaplen = int.Parse(args[i += 1]);
			}
		}

		if (showExtcapDlts)
		{
			PrintExtcapDlts(string.Empty);
			return;
		}
		if (showExtcapConfig)
		{
			PrintExtcapConfig(string.Empty);
			return;
		}

		if (startCapture && fifo.Length != 0 && ifaceorminor.Length != 0)
		{
			var fifoHandle = Win32.CreateFile(fifo, 0x40000000, 0, IntPtr.Zero, 3, 0, IntPtr.Zero);

			using (FileStream file = new FileStream(fifoHandle, FileAccess.Write))
			{
				var session = FritzBoxSession.Login(username, password);

				try
				{
					using (Stream stream = session.Capture(ifaceorminor, snaplen))
					{
						stream.CopyTo(file, snaplen);
					}
				}
				finally
				{
					FritzBoxSession.Logout(session);
				}
			}
		}
	}

	static void PrintExtcapVersion()
	{
		Console.WriteLine("extcap {version=1.0}");
	}
	static void PrintExtcapInterfaces()
	{
		PrintExtcapVersion();
		Console.WriteLine("interface {value=fritzbox}{display=FRITZ!Box Routerschnittstelle}");
	}

	static void PrintExtcapDlts(string iface)
	{
		Console.WriteLine("dlt {number=147}{name=USER0}{display=FRITZ!Box}");
	}
	static void PrintExtcapConfig(string iface)
	{
		Console.WriteLine("arg {number=0}{call=--username}{display=Username}{tooltip=FRITZ!Box admin username}{type=string}");
		Console.WriteLine("arg {number=1}{call=--password}{display=Password}{tooltip=FRITZ!Box admin password}{type=password}");
		Console.WriteLine("arg {number=2}{call=--ifaceorminor}{display=Capture Interface}{tooltip=Network interface to capture from}{type=selector}{required=true}");
		Console.WriteLine("arg {number=2}{call=--snaplen}{display=Snap Length}{type=integer}{default=1600}");
		Console.WriteLine("value {arg=2}{value=2-1}{display=1. Internetverbindung}");
		Console.WriteLine("value {arg=2}{value=3-17}{display=Schnittstelle 0 ('internet')}");
		Console.WriteLine("value {arg=2}{value=3-18}{display=Schnittstellt 1 ('mstv')}");
		Console.WriteLine("value {arg=2}{value=3-0}{display=Routing-Schnittstelle}");
	}
}
