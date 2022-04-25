[![NuGet Status](https://img.shields.io/nuget/v/GetRequiresInit.Fody.svg)](https://www.nuget.org/packages/GetRequiresInit.Fody/)

[//]: # (![Icon]&#40;https://raw.githubusercontent.com/Fody/Home/master/GetRequiresInit/package_icon.png&#41;)

`GetRequiresInit` is a simple [Fody][fody-home] addin that adds runtime checks
to validate that properties marked with the `[GetRequiresInit]` attribute have
been initialied via their set method before their get method is called.

Calling the get method before the set method throws an
`InvalidOperationException`.

## Usage

See also [Fody usage][fody-usage].


### NuGet installation

TODO: Actually make the package

Install the [GetRequiresInit.Fody NuGet package](https://www.nuget.org/packages/GetRequiresInit.Fody/) and update the [Fody NuGet package](https://www.nuget.org/packages/Fody/):

```powershell
PM> Install-Package Fody
PM> Install-Package GetRequiresInit.Fody
```

The `Install-Package Fody` is required since NuGet always defaults to the oldest, and most buggy, version of any dependency.


### Add to FodyWeavers.xml

Add `<GetRequiresInit/>` to `FodyWeavers.xml`.

```xml
<Weavers>
  <GetRequiresInit/>
</Weavers>
```


## The moving parts

See [writing an addin](/pages/addin-development.md)


## Icon

[Lego](https://thenounproject.com/term/lego/16919/) designed by [Timur Zima](https://thenounproject.com/timur.zima/) from [The Noun Project](https://thenounproject.com).

[fody-home]: https://github.com/Fody/Home
[fody-usage]: https://github.com/Fody/Home/blob/master/pages/usage.md
