using System;

namespace XRL.UI;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class UIView : Attribute
{
	public string ID;

	public bool WantsTileOver;

	public bool ForceFullscreen;

	public bool ForceFullscreenInLegacy;

	public bool IgnoreForceFullscreen;

	public string NavCategory;

	public string UICanvas;

	public int UICanvasHost;

	public bool TakesScroll;

	public int OverlayMode;

	public UIView(string ID, bool WantsTileOver = false, bool ForceFullscreen = false, bool IgnoreForceFullscreen = false, string NavCategory = null, string UICanvas = null, bool TakesScroll = false, int UICanvasHost = 0, bool ForceFullscreenInLegacy = false)
	{
		this.ID = ID;
		this.WantsTileOver = WantsTileOver;
		this.IgnoreForceFullscreen = IgnoreForceFullscreen;
		this.ForceFullscreen = ForceFullscreen;
		this.ForceFullscreenInLegacy = ForceFullscreenInLegacy;
		this.NavCategory = NavCategory;
		this.UICanvas = UICanvas;
		this.TakesScroll = TakesScroll;
		this.UICanvasHost = UICanvasHost;
	}

	public GameManager.ViewInfo AsGameManagerViewInfo()
	{
		return new GameManager.ViewInfo(WantsTileOver, ForceFullscreen, NavCategory, UICanvas, OverlayMode, ExecuteActions: false, TakesScroll, UICanvasHost, IgnoreForceFullscreen, ForceFullscreenInLegacy);
	}
}
