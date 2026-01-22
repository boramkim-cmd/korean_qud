using System;

namespace XRL.World;

[Serializable]
public class TinkeringSifrahTokenVisualInspection : SifrahToken
{
	public TinkeringSifrahTokenVisualInspection()
	{
		Description = "visual inspection";
		Tile = "Mutations/night_vision_mutation.bmp";
		RenderString = "\u001d";
		ColorString = "&y";
		DetailColor = 'B';
	}
}
