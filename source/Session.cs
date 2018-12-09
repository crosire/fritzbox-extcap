/**
 * Copyright (C) 2017 Patrick Mours. All rights reserved.
 * License: https://github.com/crosire/fritzbox-extcap#license
 */

using System.IO;
using System.Net;
using System.Security.Authentication;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

class FritzBoxSession
{
	#region Internals
	const string loginUrl = "http://fritz.box/login_sid.lua";
	const string captureUrl = "http://fritz.box/cgi-bin/capture_notimeout";

	static Stream HttpGetRaw(string url)
	{
		HttpWebRequest request = WebRequest.CreateHttp(url);
		request.KeepAlive = true;

		return request.GetResponse().GetResponseStream();
	}
	static XmlDocument HttpGetXml(string url)
	{
		HttpWebRequest request = WebRequest.CreateHttp(url);

		using (WebResponse response = request.GetResponse())
		{
			using (StreamReader stream = new StreamReader(response.GetResponseStream()))
			{
				var document = new XmlDocument();
				document.LoadXml(stream.ReadToEnd());
				return document;
			}
		}
	}
	#endregion

	public string Username { get; private set; }
	public string SessionID { get; private set; }

	public static FritzBoxSession Login(string username, string password)
	{
		XmlDocument document = HttpGetXml(loginUrl);

		string sid = document["SessionInfo"]["SID"].InnerText;

		if (sid == "0000000000000000")
		{
			string challenge = document["SessionInfo"]["Challenge"].InnerText;
			string challengeResponse = challenge + '-';

			using (var md5 = MD5.Create())
			{
				var hash = md5.ComputeHash(Encoding.Unicode.GetBytes(challenge + '-' + password));

				for (int i = 0; i < hash.Length; i++)
				{
					challengeResponse += hash[i].ToString("x2");
				}
			}

			document = HttpGetXml(loginUrl + "?username=" + username + "&response=" + challengeResponse);

			sid = document["SessionInfo"]["SID"].InnerText;
		}

		if (sid == "0000000000000000")
		{
			throw new AuthenticationException("Login failed!");
		}

		return new FritzBoxSession { Username = username, SessionID = sid };
	}
	public static void Logout(FritzBoxSession session)
	{
		HttpGetRaw(loginUrl + "?sid=" + session.SessionID + "&logout=1");
	}

	public Stream Capture(string iface, int snaplen)
	{
		return HttpGetRaw(captureUrl + "?sid=" + SessionID + "&ifaceorminor=" + iface + "&capture=Start&snaplen=" + snaplen);
	}
}
