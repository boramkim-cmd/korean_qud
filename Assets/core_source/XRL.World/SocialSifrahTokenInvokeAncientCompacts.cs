using System;

namespace XRL.World;

[Serializable]
public class SocialSifrahTokenInvokeAncientCompacts : SifrahToken
{
	public SocialSifrahTokenInvokeAncientCompacts()
	{
		Description = "invoke ancient compacts";
		Tile = "Items/sw_scroll2.bmp";
		RenderString = "\u0015";
		ColorString = "&y";
		DetailColor = 'r';
	}
}
