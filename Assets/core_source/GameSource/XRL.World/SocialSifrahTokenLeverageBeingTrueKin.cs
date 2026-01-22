using System;

namespace XRL.World;

[Serializable]
public class SocialSifrahTokenLeverageBeingTrueKin : SifrahToken
{
	public SocialSifrahTokenLeverageBeingTrueKin()
	{
		Description = "leverage being True Kin";
		Tile = "Items/ms_happy_face.png";
		RenderString = "\u0002";
		ColorString = "&Y";
		DetailColor = 'B';
	}
}
