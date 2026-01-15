using System;

namespace XRL.World;

[Serializable]
public class PsionicSifrahTokenTenfoldPathVur : SifrahToken
{
	public PsionicSifrahTokenTenfoldPathVur()
	{
		Description = "draw on the might of Vur";
		Tile = "Items/ms_vur.bmp";
		RenderString = "(";
		ColorString = "&R";
		DetailColor = 'Y';
	}
}
