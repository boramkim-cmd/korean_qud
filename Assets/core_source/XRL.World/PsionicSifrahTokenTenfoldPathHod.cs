using System;

namespace XRL.World;

[Serializable]
public class PsionicSifrahTokenTenfoldPathHod : SifrahToken
{
	public PsionicSifrahTokenTenfoldPathHod()
	{
		Description = "draw on the majesty of Hod";
		Tile = "Items/ms_hod.bmp";
		RenderString = "(";
		ColorString = "&O";
		DetailColor = 'Y';
	}
}
