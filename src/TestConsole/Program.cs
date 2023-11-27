using Rpg.SciFi.Engine.Artifacts.Expressions;
using System.Reflection;
using TestConsole;

Child child = new Child
{
    Prop1 = "1",
    Child1 = "2",
};

var name = Helper.GetCalculationMethod<int>(() => Child.DoAnotherThing);

var num = Helper.RunCalculationMethod<int>(name, 2);


var propInfos = child.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
foreach (var propInfo in propInfos)
{
    Console.WriteLine(propInfo.Name);
}
// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");
