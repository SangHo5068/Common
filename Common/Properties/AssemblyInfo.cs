﻿using System.Reflection;
using System.Runtime.InteropServices;

// 어셈블리에 대한 일반 정보는 다음 특성 집합을 통해 
// 제어됩니다. 어셈블리와 관련된 정보를 수정하려면
// 이러한 특성 값을 변경하세요.
[assembly: AssemblyTitle("Common")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("Common")]
[assembly: AssemblyCopyright("Copyright ©  2019")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// ComVisible을 false로 설정하면 이 어셈블리의 형식이 COM 구성 요소에 
// 표시되지 않습니다. COM에서 이 어셈블리의 형식에 액세스하려면
// 해당 형식에 대해 ComVisible 특성을 true로 설정하세요.
[assembly: ComVisible(false)]

// 이 프로젝트가 COM에 노출되는 경우 다음 GUID는 typelib의 ID를 나타냅니다.
[assembly: Guid("0707e28b-6c2e-4a61-9ec6-891a0c9ead18")]

// 어셈블리의 버전 정보는 다음 네 가지 값으로 구성됩니다.
//
//      주 버전
//      부 버전 
//      빌드 번호
//      수정 버전
//
// 모든 값을 지정하거나 아래와 같이 '*'를 사용하여 빌드 번호 및 수정 번호를
// 기본값으로 할 수 있습니다.
[assembly: AssemblyVersion("1.0.0.*")]
//[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]
//[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config", Watch = true)]


// From C#, F# and VB, by default code is generated too so that the same 
// information can be accessed from code, to construct your own 
// assembly/file version attributes with whatever format you want:

//[assembly: AssemblyVersion(ThisAssembly.Git.SemVer.Major + "." + ThisAssembly.Git.SemVer.Minor + "." + ThisAssembly.Git.SemVer.Patch)]
//[assembly: AssemblyFileVersion(ThisAssembly.Git.SemVer.Major + "." + ThisAssembly.Git.SemVer.Minor + "." + ThisAssembly.Git.SemVer.Patch)]
//[assembly: AssemblyInformationalVersion(
//  ThisAssembly.Git.SemVer.Major + "." +
//  ThisAssembly.Git.SemVer.Minor + "." +
//  ThisAssembly.Git.SemVer.Patch + "-" +
//  ThisAssembly.Git.Branch + "+" +
//  ThisAssembly.Git.Commit)]
//// i..e ^: 1.0.2-main+c218617
