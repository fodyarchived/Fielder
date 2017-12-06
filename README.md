[![Chat on Gitter](https://img.shields.io/gitter/room/fody/fody.svg?style=flat)](https://gitter.im/Fody/Fody)
[![NuGet Status](http://img.shields.io/nuget/v/Fielder.Fody.svg?style=flat)](https://www.nuget.org/packages/Fielder.Fody/)


## This is an add-in for [Fody](https://github.com/Fody/Fody/)

![Icon](https://raw.github.com/Fody/Fielder/master/package_icon.png)

Converts public fields to public properties

[Introduction to Fody](http://github.com/Fody/Fody/wiki/SampleUsage)

This feature means you can exclude the `{ get; set; }` on your properties and use fields instead.


## Usage

See also [Fody usage](https://github.com/Fody/Fody#usage).


### NuGet installation

Install the [Fielder.Fody NuGet package](https://nuget.org/packages/Fielder.Fody/) and update the [Fody NuGet package](https://nuget.org/packages/Fody/):

```
PM> Install-Package Fielder.Fody
PM> Update-Package Fody
```

The `Update-Package Fody` is required since NuGet always defaults to the oldest, and most buggy, version of any dependency.


### Add to FodyWeavers.xml

Add `<Fielder/>` to [FodyWeavers.xml](https://github.com/Fody/Fody#add-fodyweaversxml)

```xml
<?xml version="1.0" encoding="utf-8" ?>
<Weavers>
  <Fielder/>
</Weavers>
```


## How does this work

 * For all types find all public instance fields with a capitalised first character
 * Convert all those fields to properties with the same name
 * Find all usages of those fields and point them to the new properties


## Icon

<a href="http://thenounproject.com/noun/cow/#icon-No5849" target="_blank">Cow</a> designed by <a href="http://thenounproject.com/yxorama" target="_blank">Anuar Zhumaev</a> from The Noun Project