using System;
using System.Collections.Generic;
using Genkit;
using XRL.UI.Framework;

namespace XRL.CharacterBuilds;

public abstract class AbstractEmbarkBuilderModule
{
	public EmbarkBuilder builder;

	public Dictionary<string, EmbarkBuilderModuleWindowDescriptor> windows = new Dictionary<string, EmbarkBuilderModuleWindowDescriptor>();

	protected bool _enabled = true;

	private AbstractEmbarkBuilderModuleData _values;

	protected EmbarkBuilderModuleWindowDescriptor CurrentLoadingWindowDescriptor;

	public string type => GetType().Name;

	public bool enabled => _enabled;

	public virtual AbstractEmbarkBuilderModuleData DefaultData => null;

	public virtual Dictionary<string, Action<XmlDataHelper>> XmlNodes => new Dictionary<string, Action<XmlDataHelper>> { { "window", HandleWindowNode } };

	protected virtual Dictionary<string, Action<XmlDataHelper>> XmlWindowNodes => new Dictionary<string, Action<XmlDataHelper>>
	{
		{ "icon", HandleWindowIconNode },
		{ "name", HandleWindowNameNode },
		{ "title", HandleWindowTitleNode }
	};

	public event Action<AbstractEmbarkBuilderModuleData, AbstractEmbarkBuilderModuleData> OnChange;

	public virtual bool IncludeInBuildCodes()
	{
		if (enabled)
		{
			return getData() != null;
		}
		return false;
	}

	public virtual SummaryBlockData GetSummaryBlock()
	{
		return null;
	}

	public virtual bool shouldBeEnabled()
	{
		return enabled;
	}

	public virtual bool shouldBeEditable()
	{
		return true;
	}

	public virtual void assembleWindowDescriptors(List<EmbarkBuilderModuleWindowDescriptor> windows)
	{
		windows.AddRange(this.windows.Values);
	}

	public void enable()
	{
		if (!_enabled)
		{
			_enabled = true;
			OnEnabled();
		}
	}

	public virtual void OnEnabled()
	{
	}

	public void disable()
	{
		if (_enabled)
		{
			_enabled = false;
			OnDisabled();
		}
	}

	public virtual void OnDisabled()
	{
	}

	public virtual void handleModuleDataChange(AbstractEmbarkBuilderModule module, AbstractEmbarkBuilderModuleData oldValues, AbstractEmbarkBuilderModuleData newValues)
	{
	}

	public virtual void OnBeforeDataChange(AbstractEmbarkBuilderModuleData oldValues, AbstractEmbarkBuilderModuleData newValues)
	{
	}

	public virtual void OnDataChange(AbstractEmbarkBuilderModuleData oldValues, AbstractEmbarkBuilderModuleData newValues)
	{
		_values = newValues;
	}

	public virtual void OnAfterDataChange(AbstractEmbarkBuilderModuleData oldValues, AbstractEmbarkBuilderModuleData newValues)
	{
		builder.NotifyModuleChanges(this, oldValues, newValues);
	}

	public virtual void setData(AbstractEmbarkBuilderModuleData values)
	{
		AbstractEmbarkBuilderModuleData values2 = _values;
		OnBeforeDataChange(values2, values);
		OnDataChange(values2, values);
		if (this.OnChange != null)
		{
			this.OnChange(values2, values);
		}
		OnAfterDataChange(values2, values);
	}

	public virtual void setDataDirect(AbstractEmbarkBuilderModuleData values)
	{
		_values = values;
	}

	public virtual Type getDataType()
	{
		return typeof(AbstractEmbarkBuilderModuleData);
	}

	public virtual AbstractEmbarkBuilderModuleData getData()
	{
		return _values;
	}

	public virtual bool IsDataValid()
	{
		if (DataErrors() == null)
		{
			return DataWarnings() == null;
		}
		return false;
	}

	public virtual string DataErrors()
	{
		if (getData() == null)
		{
			return "You must make a selection before advancing.";
		}
		return null;
	}

	public virtual string DataWarnings()
	{
		return null;
	}

	protected Random getModuleRngFromSeed(string seed)
	{
		return new Random(Hash.String(type + seed));
	}

	public virtual void InitFromSeed(string seed)
	{
	}

	public virtual void RandomSelection()
	{
	}

	public virtual void ResetSelection()
	{
		setData(DefaultData);
	}

	public virtual void Init()
	{
	}

	public virtual void bootGame(XRLGame game, EmbarkInfo info)
	{
	}

	public virtual object handleUIEvent(string ID, object Element)
	{
		return Element;
	}

	public virtual object handleBootEvent(string id, XRLGame game, EmbarkInfo info, object element = null)
	{
		return element;
	}

	public virtual void onNext()
	{
		builder.advance();
	}

	public virtual void onBack()
	{
		builder.back();
	}

	public virtual void HandleNodes(XmlDataHelper xml)
	{
		xml.HandleNodes(XmlNodes);
	}

	public virtual void HandleWindowNode(XmlDataHelper xml)
	{
		string text = xml.GetAttribute("ID") ?? Guid.NewGuid().ToString();
		if (!windows.TryGetValue(text, out CurrentLoadingWindowDescriptor))
		{
			CurrentLoadingWindowDescriptor = new EmbarkBuilderModuleWindowDescriptor(this);
			CurrentLoadingWindowDescriptor.viewID = text;
			windows.Set(text, CurrentLoadingWindowDescriptor);
		}
		CurrentLoadingWindowDescriptor.prefab = xml.GetAttributeString("Prefab", CurrentLoadingWindowDescriptor.prefab);
		try
		{
			string attributeString = xml.GetAttributeString("Class", null);
			Type type = ModManager.ResolveType(attributeString);
			if (type == null)
			{
				xml.ParseWarning("Unresolved type " + attributeString);
			}
			if (type != null)
			{
				CurrentLoadingWindowDescriptor.windowType = type;
			}
		}
		catch
		{
			xml.ParseWarning("Error finding Class");
		}
		xml.HandleNodes(XmlWindowNodes);
		CurrentLoadingWindowDescriptor = null;
	}

	public virtual void HandleWindowIconNode(XmlDataHelper xml)
	{
		CurrentLoadingWindowDescriptor.tile = xml.GetAttribute("Tile");
		xml.DoneWithElement();
	}

	public virtual void HandleWindowNameNode(XmlDataHelper xml)
	{
		CurrentLoadingWindowDescriptor.name = xml.GetTextNode();
	}

	public virtual void HandleWindowTitleNode(XmlDataHelper xml)
	{
		CurrentLoadingWindowDescriptor.title = xml.GetTextNode();
	}
}
