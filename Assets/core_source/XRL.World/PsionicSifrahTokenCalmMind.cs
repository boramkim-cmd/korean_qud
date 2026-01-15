using System;

namespace XRL.World;

[Serializable]
public class PsionicSifrahTokenCalmMind : SifrahToken
{
	public PsionicSifrahTokenCalmMind()
	{
		Description = "calm mind";
		Tile = "Items/ms_willpower.bmp";
		RenderString = "÷";
		ColorString = "&Y";
		DetailColor = 'Y';
	}
}
