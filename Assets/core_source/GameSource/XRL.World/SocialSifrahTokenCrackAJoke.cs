using System;

namespace XRL.World;

[Serializable]
public class SocialSifrahTokenCrackAJoke : SifrahToken
{
	public SocialSifrahTokenCrackAJoke()
	{
		Description = "crack a joke";
		Tile = "Items/sw_mask.bmp";
		RenderString = "\u0001";
		ColorString = "&C";
		DetailColor = 'W';
	}
}
