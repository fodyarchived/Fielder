## This is an add-in for [Fody](https://github.com/Fody/Fody/) 

Converts public fields to public properties

[Introduction to Fody](http://github.com/Fody/Fody/wiki/SampleUsage)

This feature means you can exclude the `{ get; set; }` on your properties and use fields instead.

## Nuget
 
Nuget package http://nuget.org/packages/Fielder.Fody 

To Install from the Nuget Package Manager Console 
    
    PM> Install-Package Fielder.Fody

## How does this work. 

 * For all types find all public instance fields with a capitalised first character
 * Convert all those fields to properties with the same name
 * Find all usages of those fields and point them to the new properties
