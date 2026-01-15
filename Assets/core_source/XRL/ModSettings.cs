using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace XRL;

public class ModSettings
{
	public string Title;

	public bool Enabled = true;

	public string FilesHash;

	public string SourceHash;

	public Version? UpdateVersion;

	[JsonIgnore]
	public bool Failed;

	[JsonIgnore]
	public List<string> Errors = new List<string>();

	[JsonIgnore]
	public List<string> Warnings = new List<string>();

	public string CalcFilesHash(IReadOnlyList<ModFile> Files, string Root)
	{
		using SHA1 sHA = SHA1.Create();
		int i = 0;
		for (int count = Files.Count; i < count; i++)
		{
			ModFile modFile = Files[i];
			if (IsHashFile(modFile))
			{
				byte[] bytes = Encoding.UTF8.GetBytes(modFile.FullName.Replace(Root, ""));
				sHA.TransformBlock(bytes, 0, bytes.Length, bytes, 0);
				bytes = BitConverter.GetBytes(modFile.Size);
				sHA.TransformBlock(bytes, 0, bytes.Length, bytes, 0);
			}
		}
		sHA.TransformFinalBlock(new byte[0], 0, 0);
		return string.Concat(sHA.Hash.Select((byte x) => x.ToString("X2")));
	}

	public bool IsHashFile(ModFile File)
	{
		if (File.Type != ModFileType.CSharp)
		{
			return File.Type == ModFileType.XML;
		}
		return true;
	}

	public string CalcSourceHash(IReadOnlyList<ModFile> Files)
	{
		using SHA1 sHA = SHA1.Create();
		byte[] array = new byte[8192];
		int num = 0;
		int i = 0;
		for (int count = Files.Count; i < count; i++)
		{
			ModFile modFile = Files[i];
			if (!IsHashFile(modFile))
			{
				continue;
			}
			using FileStream fileStream = new FileStream(modFile.OriginalName, FileMode.Open);
			do
			{
				num = fileStream.Read(array, 0, 8192);
				sHA.TransformBlock(array, 0, num, array, 0);
			}
			while (num > 0);
		}
		sHA.TransformFinalBlock(array, 0, 0);
		return string.Concat(sHA.Hash.Select((byte x) => x.ToString("X2")));
	}
}
