
all: run

run: AOTStats.exe
	mono --debug AOTStats.exe PEAPI.dll.json I18N.CJK.dll.json I18N.dll.json I18N.MidEast.dll.json I18N.Other.dll.json I18N.Rare.dll.json I18N.West.dll.json Microsoft.CSharp.dll.json Mono.Cairo.dll.json Mono.CompilerServices.SymbolWriter.dll.json Mono.CSharp.dll.json Mono.Data.Sqlite.dll.json Mono.Data.Tds.dll.json Mono.Dynamic.Interpreter.dll.json Mono.Security.dll.json Mono.Security.Providers.DotNet.dll.json Mono.Security.Providers.NewSystemSource.dll.json Mono.Security.Providers.NewTls.dll.json Mono.Simd.dll.json mscorlib.dll.json PEAPI.dll.json SMDiagnostics.dll.json System.ComponentModel.Composition.dll.json System.ComponentModel.DataAnnotations.dll.json System.Core.dll.json System.Data.dll.json System.Data.Services.Client.dll.json System.dll.json System.IO.Compression.dll.json System.IO.Compression.FileSystem.dll.json System.Json.dll.json System.Net.dll.json System.Net.Http.dll.json System.Net.Http.WinHttpHandler.dll.json System.Numerics.dll.json System.Numerics.Vectors.dll.json System.Reflection.Context.dll.json System.Reflection.DispatchProxy.dll.json System.Runtime.InteropServices.RuntimeInformation.dll.json System.Runtime.Serialization.dll.json System.Security.dll.json System.ServiceModel.dll.json System.ServiceModel.Internals.dll.json System.ServiceModel.Web.dll.json System.Transactions.dll.json System.Web.Services.dll.json System.Windows.dll.json System.Xml.dll.json System.Xml.Linq.dll.json System.Xml.Serialization.dll.json System.Xml.XPath.XmlDocument.dll.json

AOTStats.exe: AOTStats.cs
	mcs -debug AOTStats.cs -r:System.Json

clean:
	rm AOTStats.exe
