using System;

namespace XRL.World;

[Serializable]
public class SocialSifrahTokenBoastOfAccomplishments : SifrahToken
{
	public SocialSifrahTokenBoastOfAccomplishments()
	{
		Description = "boast of my accomplishments";
		Tile = "Items/sw_armlocks.bmp";
		RenderString = "\u0013";
		ColorString = "&M";
		DetailColor = 'm';
	}
}
