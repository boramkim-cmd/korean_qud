using System;

namespace XRL.World;

[Serializable]
public class RitualSifrahTokenPrayHumbly : SifrahToken
{
	public RitualSifrahTokenPrayHumbly()
	{
		Description = "pray humbly";
		Tile = "Items/sw_gianthands.bmp";
		RenderString = "\u0014";
		ColorString = "&y";
		DetailColor = 'W';
	}
}
