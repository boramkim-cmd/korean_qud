using System;

namespace XRL.World;

[Serializable]
public class RitualSifrahTokenSingAHistoricalEpic : SifrahToken
{
	public RitualSifrahTokenSingAHistoricalEpic()
	{
		Description = "sing a historical epic";
		Tile = "Items/sw_cherubic.bmp";
		RenderString = "\u000e";
		ColorString = "&Y";
		DetailColor = 'W';
	}
}
