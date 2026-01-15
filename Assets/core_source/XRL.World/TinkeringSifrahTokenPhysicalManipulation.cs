using System;

namespace XRL.World;

[Serializable]
public class TinkeringSifrahTokenPhysicalManipulation : SifrahToken
{
	public TinkeringSifrahTokenPhysicalManipulation()
	{
		Description = "physical manipulation";
		Tile = "Items/sw_flexors.bmp";
		RenderString = "\u001d";
		ColorString = "&y";
		DetailColor = 'K';
	}
}
