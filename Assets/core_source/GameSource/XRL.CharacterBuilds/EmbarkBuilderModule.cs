using System;

namespace XRL.CharacterBuilds;

public abstract class EmbarkBuilderModule<T> : AbstractEmbarkBuilderModule where T : AbstractEmbarkBuilderModuleData
{
	public T data
	{
		get
		{
			return getData() as T;
		}
		set
		{
			setData(value);
		}
	}

	public override Type getDataType()
	{
		return typeof(T);
	}
}
