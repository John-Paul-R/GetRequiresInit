[![NuGet Status](https://img.shields.io/nuget/v/GetRequiresInit.Fody.svg)](https://www.nuget.org/packages/GetRequiresInit.Fody/)

![Icon](https://raw.githubusercontent.com/Fody/Home/master/GetRequiresInit/package_icon.png)

This is a simple solution used to illustrate how to [write a Fody addin](/pages/addin-development.md).


## Usage

See also [Fody usage](/pages/usage.md).


### NuGet installation

Install the [GetRequiresInit.Fody NuGet package](https://www.nuget.org/packages/GetRequiresInit.Fody/) and update the [Fody NuGet package](https://www.nuget.org/packages/Fody/):

```powershell
PM> Install-Package Fody
PM> Install-Package GetRequiresInit.Fody
```

The `Install-Package Fody` is required since NuGet always defaults to the oldest, and most buggy, version of any dependency.


### Add to FodyWeavers.xml

Add `<GetRequiresInit/>` to [FodyWeavers.xml](/pages/configuration.md#fodyweaversxml)

```xml
<Weavers>
  <GetRequiresInit/>
</Weavers>
```


## The moving parts

See [writing an addin](/pages/addin-development.md)


## Icon

[Lego](https://thenounproject.com/term/lego/16919/) designed by [Timur Zima](https://thenounproject.com/timur.zima/) from [The Noun Project](https://thenounproject.com).