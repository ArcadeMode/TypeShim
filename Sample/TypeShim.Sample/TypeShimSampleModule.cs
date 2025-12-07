using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using TypeShim;
using TypeShim.Sample;

namespace TypeShim.Sample;

[TsExport]
public static class TypeShimSampleModule
{
    public static PeopleProvider? PeopleProvider { get; internal set; }
}