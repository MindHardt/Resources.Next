// See https://aka.ms/new-console-template for more information

using Resources.Next.Generated;

Console.WriteLine(BarResources.Test);
Console.WriteLine(FooResources.Qix);
Console.WriteLine(ResourceFinder.FindIn<BarResources>("Lorem"));