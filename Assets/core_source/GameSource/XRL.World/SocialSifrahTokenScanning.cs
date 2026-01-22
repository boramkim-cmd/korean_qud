using System;
using XRL.World.Capabilities;

namespace XRL.World;

[Serializable]
public class SocialSifrahTokenScanning : SifrahToken
{
	public SocialSifrahTokenScanning()
	{
		Description = "scanning";
		Tile = "Items/sw_aircurrent.bmp";
		RenderString = "å";
		ColorString = "&C";
		DetailColor = 'W';
	}

	public SocialSifrahTokenScanning(Scanning.Scan scan)
		: this()
	{
		Description = "interpret " + Scanning.GetScanSubjectName(scan);
	}
}
