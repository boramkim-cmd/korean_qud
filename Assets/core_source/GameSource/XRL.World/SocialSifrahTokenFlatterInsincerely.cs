using System;

namespace XRL.World;

[Serializable]
public class SocialSifrahTokenFlatterInsincerely : SifrahToken
{
	public SocialSifrahTokenFlatterInsincerely()
	{
		Description = "flatter insincerely";
		Tile = "Items/sw_twohearted.bmp";
		RenderString = "\u0006";
		ColorString = "&K";
		DetailColor = 'W';
	}
}
