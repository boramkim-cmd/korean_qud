using System;
using System.Collections.Generic;

namespace XRL.UI.Framework;

[Serializable]
public class CategoryMenuData : FrameworkDataElement
{
	public string Title;

	public List<PrefixMenuOption> menuOptions;
}
