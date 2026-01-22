using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Occult.Engine.CodeGeneration;
using XRL.Liquids;
using XRL.Rules;
using XRL.World.Anatomy;

namespace XRL.World.Parts;

[Serializable]
[GenerateSerializationPartial]
public class DismemberedProperties : IPart
{
	public string SourceID;

	public string SourceBlueprint;

	public string SourceGenotype;

	public string SourceBlood;

	public BodyPart BodyPart;

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public override void Write(GameObject Basis, SerializationWriter Writer)
	{
		Writer.WriteOptimized(SourceID);
		Writer.WriteOptimized(SourceBlueprint);
		Writer.WriteOptimized(SourceGenotype);
		Writer.WriteOptimized(SourceBlood);
		Writer.Write(BodyPart);
	}

	[GeneratedCode("SerializationPartialsGenerator", "1.0.0.0")]
	public override void Read(GameObject Basis, SerializationReader Reader)
	{
		SourceID = Reader.ReadOptimizedString();
		SourceBlueprint = Reader.ReadOptimizedString();
		SourceGenotype = Reader.ReadOptimizedString();
		SourceBlood = Reader.ReadOptimizedString();
		BodyPart = (BodyPart)Reader.ReadComposite();
	}

	public void SetFrom(BodyPart BodyPart)
	{
		GameObject gameObject = BodyPart.ParentBody?.ParentObject;
		if (gameObject != null)
		{
			SourceID = gameObject.ID;
			SourceBlueprint = gameObject.Blueprint;
			SourceGenotype = gameObject.GetGenotype();
			SourceBlood = gameObject.GetBleedLiquid();
		}
		this.BodyPart = BodyPart.DeepCopy(null, null, null, null, CopyGameObjects: false);
	}

	public override bool WantEvent(int ID, int Cascade)
	{
		if (!base.WantEvent(ID, Cascade))
		{
			return ID == PooledEvent<ConfigureMissileVisualEffectEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(ConfigureMissileVisualEffectEvent E)
	{
		if (E.Projectile == ParentObject && !SourceBlood.IsNullOrEmpty())
		{
			BaseLiquid liquid = LiquidVolume.GetLiquid(SourceBlood.AsSpan(0, SourceBlood.IndexOf('-')));
			List<string> colors = liquid.GetColors();
			string text = ((colors.Count > 0) ? colors[0] : liquid.GetColor());
			string value = ((colors.Count > 1) ? colors[Stat.Rnd.Next(1, colors.Count)] : text);
			E.Path.SetParameter("ParticleStartColor", text);
			E.Path.SetParameter("ParticleEndColor", value);
		}
		return base.HandleEvent(E);
	}
}
