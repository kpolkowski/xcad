# xCAD.NET: SOLIDWORKS API development made easy

![Logo](https://raw.githubusercontent.com/xarial/xcad/master/data/icon.png)

[![NuGet version (xCAD.NET)](https://img.shields.io/nuget/v/Xarial.XCad.svg?style=flat-square)](https://www.nuget.org/packages/Xarial.XCad/)
[![Build status](https://dev.azure.com/xarial/xcad/_apis/build/status/xcad)](https://dev.azure.com/xarial/xcad/_build/latest?definitionId=34)

[![User Guide](https://img.shields.io/badge/-Documentation-green.svg)](https://xcad.xarial.com)
[![Examples](https://img.shields.io/badge/-Examples-blue.svg)](https://github.com/xarial/xcad-examples)

[xCAD.NET](https://xcad.net) is a framework for building CAD agnostic applications. It allows developers to implement complex functionality with a very simple innovative approach. This brings the best user experience to the consumers of the software.

## SOLIDWORKS Add-in Applications

It has never been easier to create SOLIDWORKS add-ins with toolbar and menu commands.

~~~ cs
[ComVisible(true)]
public class XCadAddIn : SwAddInEx
{
    public enum Commands_e
    {
        Command1,
        Command2
    }

    public override void OnConnect()
    {
        this.CommandManager.AddCommandGroup<Commands_e>().CommandClick += OnCommandsButtonClick;
    }

    private void OnCommandsButtonClick(Commands_e cmd)
    {
        //TODO: handle the button click
    }
}
~~~

## Property Manager Pages

Framework reinvents the way you work with Property Manager Pages. No need to code a complex code behind for adding the controls and handling the values. Simply define your data model and the framework will build the suitable Property Manager Page automatically and two-way bind controls to the data model.

~~~ cs
[ComVisible(true)]
public class IntroPmpPageAddIn : SwAddInEx
{
    [ComVisible(true)]
    public class MyPMPageData : SwPropertyManagerPageHandler
    {
        public string Text { get; set; }
        public int Number { get; set; }
        public IXComponent Component { get; set; }
    }

    private enum Commands_e
    {
        ShowPmpPage
    }

    private IXPropertyPage<MyPMPageData> m_Page;
    private MyPMPageData m_Data = new MyPMPageData();

    public override void OnConnect()
    {
        m_Page = this.CreatePage<MyPMPageData>();
        m_Page.Closed += OnPageClosed;
        this.CommandManager.AddCommandGroup<Commands_e>().CommandClick += ShowPmpPage;
    }

    private void ShowPmpPage(Commands_e cmd)
    {
        m_Page.Show(m_Data);
    }

    private void OnPageClosed(PageCloseReasons_e reason)
    {
        Debug.Print($"Text: {m_Data.Text}");
        Debug.Print($"Number: {m_Data.Number}");
        Debug.Print($"Selection component name: {m_Data.Component.Name}");
    }
}
~~~

## Macro Features

Complex macro features became an ease with xCAD.NET

~~~ cs
[ComVisible(true)]
public class IntroMacroFeatureAddIn : SwAddInEx 
{
    [ComVisible(true)]
    public class BoxData : SwPropertyManagerPageHandler
    {
        public double Width { get; set; }
        public double Length { get; set; }
        public double Height { get; set; }
    }

    [ComVisible(true)]
    public class BoxMacroFeature : SwMacroFeatureDefinition<BoxData, BoxData>
    {
        public override ISwBody[] CreateGeometry(ISwApplication app, ISwDocument model, BoxData data)
        {
            var body = (ISwBody)app.MemoryGeometryBuilder.CreateSolidBox(new Point(0, 0, 0),
                new Vector(1, 0, 0), new Vector(0, 1, 0),
                data.Width, data.Length, data.Height).Bodies.First();

            return new ISwBody[] { body };
        }

    }

    public enum Commands_e
    {
        InsertMacroFeature,
    }

    public override void OnConnect()
    {
        this.CommandManager.AddCommandGroup<Commands_e>().CommandClick += OnCommandsButtonClick;
    }

    private void OnCommandsButtonClick(Commands_e cmd)
    {
        switch (cmd) 
        {
            case Commands_e.InsertMacroFeature:
                Application.Documents.Active.Features.CreateCustomFeature<BoxMacroFeature, BoxData, BoxData>();
                break;
        }
    }
}
~~~

## SOLIDWORKS And Document Manager API

xCAD.NET allows to write the same code targeting different CAD implementation in a completely agnostic way. Example below demonstrates how to perform opening of assembly, traversing components recursively and closing the assembly via SOLIDWORKS API and [SOLIDWORKS Document Manager API](https://www.codestack.net/solidworks-document-manager-api/) using the same code base.

~~~ cs
static void Main(string[] args)
{
    var assmFilePath = @"C:\sample-assembly.sldasm";

    //print assembly components using SOLIDWORKS API
    var swApp = SwApplicationFactory.Create(SwVersion_e.Sw2022, ApplicationState_e.Silent);
    PrintAssemblyComponents(swApp, assmFilePath);

    //print assembly components using SOLIDWORKS Document Manager API
    var swDmApp = SwDmApplicationFactory.Create("[Document Manager Lincese Key]");
    PrintAssemblyComponents(swDmApp, assmFilePath);
}

//CAD-agnostic function to open assembly, print all components and close assembly
private static void PrintAssemblyComponents(IXApplication app, string filePath) 
{
    using (var assm = app.Documents.Open(filePath, DocumentState_e.ReadOnly))
    {
        IterateComponentsRecursively(((IXAssembly)assm).Configurations.Active.Components, 0);
    }
}

private static void IterateComponentsRecursively(IXComponentRepository compsRepo, int level) 
{
    foreach (var comp in compsRepo)
    {
        Console.WriteLine(Enumerable.Repeat("  ", level) + comp.Name);

        IterateComponentsRecursively(comp.Children, level + 1);
    }
}
~~~

Watch the [video demonstration](https://www.youtube.com/watch?v=BuiFfv7-Qig) of xCAD in action.

Visit [User Guide](https://xcad.net) page and start exploring the framework.