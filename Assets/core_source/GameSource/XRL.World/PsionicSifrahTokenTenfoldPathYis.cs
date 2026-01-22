using System;

namespace XRL.World;

[Serializable]
public class PsionicSifrahTokenTenfoldPathYis : SifrahToken
{
	public PsionicSifrahTokenTenfoldPathYis()
	{
		Description = "draw on the depths of Yis";
		Tile = "Items/ms_yis.bmp";
		RenderString = "*";
		ColorString = "&m";
		DetailColor = 'Y';
	}
}
