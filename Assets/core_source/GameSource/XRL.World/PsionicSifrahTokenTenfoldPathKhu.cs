using System;

namespace XRL.World;

[Serializable]
public class PsionicSifrahTokenTenfoldPathKhu : SifrahToken
{
	public PsionicSifrahTokenTenfoldPathKhu()
	{
		Description = "draw on the solidity of Khu";
		Tile = "Items/ms_khu.bmp";
		RenderString = "*";
		ColorString = "&g";
		DetailColor = 'Y';
	}
}
