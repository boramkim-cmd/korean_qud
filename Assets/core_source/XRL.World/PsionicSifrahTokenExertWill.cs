using System;

namespace XRL.World;

[Serializable]
public class PsionicSifrahTokenExertWill : SifrahToken
{
	public PsionicSifrahTokenExertWill()
	{
		Description = "exert will";
		Tile = "Items/ms_ego.bmp";
		RenderString = "\u0004";
		ColorString = "&M";
		DetailColor = 'Y';
	}
}
