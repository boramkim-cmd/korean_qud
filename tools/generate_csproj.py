
import os
import glob

managed_dir = os.path.expanduser("~/Library/Application Support/Steam/steamapps/common/Caves of Qud/CoQ.app/Contents/Resources/Data/Managed")
csproj_path = "QudKorean.csproj"

dlls = glob.glob(os.path.join(managed_dir, "*.dll"))

references = []
for dll in dlls:
    name = os.path.splitext(os.path.basename(dll))[0]
    # Skip system assemblies that might conflict with SDK references
    if name.lower() in ["mscorlib", "system", "system.core", "system.xml", "system.numerics", "netstandard", "system.runtime", "system.threading.tasks.extensions", "system.text.encoding.codepages", "microsoft.netframework.referenceassemblies.net48", "system.io.compression", "system.net.http"]:
        continue
    
    ref_entry = f'''    <Reference Include="{name}">
      <HintPath>{dll}</HintPath>
      <Private>False</Private>
    </Reference>'''
    references.append(ref_entry)

csproj_content = f'''<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net48</TargetFramework>
    <LangVersion>9.0</LangVersion>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
    <RootNamespace>QudKRTranslation</RootNamespace>
    <AssemblyName>QudKorean</AssemblyName>
    <ManagedDir>{managed_dir}</ManagedDir>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
  </PropertyGroup>

  <ItemGroup>
{chr(10).join(references)}
  </ItemGroup>

  <ItemGroup>
    <Compile Include="Scripts/**/*.cs" />
    <Compile Remove="Scripts/_Legacy/**/*.cs" />
    <Compile Remove="Scripts/**/_DEPRECATED/**/*.cs" />
  </ItemGroup>

</Project>
'''

with open(csproj_path, "w") as f:
    f.write(csproj_content)

print(f"Generated {csproj_path} with {len(dlls)} references.")
