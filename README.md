# Caravel
Caravel is a simple 2D game engine built using MonoGame and C#. It was started as my experiment to learn about how a game engine is structured (what main components does it have and how do these interact with each other) . It is not meant to replace any more complex game engines like Unity, GameMaker, Godot or Unreal, although you are free to use this if you like. I've created a [startup project](https://github.com/jocamar/CaravelStarterProject) as a basis for quickly getting up and running using Caravel.

Conceptually think of Caravel as a mix between Unity and Godot. You have the Unity way of building entities with the several components in an entity dictating how it behaves, but you have a bit of Godot in how the "prefabs" work. Each scene in Caravel can be instantiated inside any other scene (be careful for circular references though if you're instantiating scenes from resources) and this is how you create reausable complex objects in your games.

Additionally Caravel is built with code first in mind. While there is an editor and it is very useful in speeding up scene and object creation, it is strictly optional. You can do everything by code or by hand crafting the scene and entity type xml files.

## Features
Currently the engine supports the following features:
* Easy to use entity component system that allows users to create a large number of different behaviors by combining sets of components.
* Entity type system that allows users to define entity types that act as templates for entities.
* Flexible scene system that allows creating nested scenes (Caravel's version of prefabs).
* Scenes and entity types are in a human readable format that is easily accepted by version control systems and can be edited by hand if necessary (no editor required).
* Event system that allows for easy decoupling between systems and message passing.
* Process system that allows users to create assincronous tasks.
* 2D physics integration using Velcro Physics. Allow to easily create physics bodies by using the Rigid Body component.
* Game scripting using Lua. Code can be written in C# or using Lua by means of Script components.
* Several useful components to get a game up and running such as particle emitters, animations, sound emitters, sprites, clickable areas, etc.
* Simple to use Game Views that keep visualization separate from logic and allow users to customize how the scenes are rendered (allowing for example to easily setup a split screen mode).
* Easy to use logging system and ability to view debug information such as collision shapes.
* Simple animation system that allows animating an entity's transform over several key frames.
* Made with code first in mind, using the editor is entirely optional.

## Screenshots

Here are some screenshots of a game made using Caravel.

<img src="https://giant.gfycat.com/FreshVacantBlowfish.gif" width="400" height="225"/> <img src="https://giant.gfycat.com/LimpAdoredDore.gif" width="400" height="225"/>
<img src="https://giant.gfycat.com/ImpishFrankEel.gif" width="700" height="450"/>

## How to Start

If you want to start a project using Caravel, the easiest way is to clone or download the [starter project](https://github.com/jocamar/CaravelStartupProject) and edit the project files and the base class provided by adding your own components, events, entity types and scenes.

If you want you can also build and edit the engine by cloning this repository and opening the project in Visual Studio. I run it in VS2017 since the project uses [Vitevic Assembly Embedder](https://marketplace.visualstudio.com/items?itemName=Vitevic.VitevicAssemblyEmbedder) to package all dependencies inside the engine dll. As far as I know this extension has not yet been updated to more recent versions of VS. After building the engine you can use it by linking to the generated dll.

If you'd like you can also use the [Caravel Editor](https://github.com/jocamar/CaravelStartupProject) to help in creating scenes, projects, entity types and materials. However one of the advantages of Caravel is that this is purely optional as every asset file can be edited and created by hand. This is great for people like me that prefer to make games closer to the metal so to say, without relying so much on an editor such as in Unity or Unreal. Caravel gives you more control than these engines when creating your game logic though it also doesn't provide the wide array of features and ease of use that these engines provide.

I wouldn't recommend using Caravel for any big projects though if you do use it I'd love to know about it.

## Usage Examples

Here are some simple usage examples of how to use Caravel.

### Creating and Editing entities

```
//Create templated entity and edit its components
var myEnt = CaravelApp.Instance.Logic.CreateEntity("entity_types/myEntityTypeResource.cve", "myEntityID", "Default", true);
myEnt.GetComponent<SomeComponent>().SomeProperty = "New Value";
myEnt.GetComponent<Cv_TransformComponent>().SetPosition(new Vector3(100,100,0));

//Create empty entity and a child
var myEmptyEnt = CaravelApp.Instance.Logic.CreateEmptyEntity("myEntityID", "Default", true);
var myChildEnt = CaravelApp.Instance.Logic.CreateEntity("entity_types/myEntityTypeResource.cve", "myEntityID", "Default", true, myEmptyEnt.ID);

//Add and remove components
var newComponent = CaravelApp.Instance.Logic.CreateComponent<SomeOtherComponent>();
CaravelApp.Instance.Logic.RemoveComponent<SomeComponent>(myEnt.ID);
CaravelApp.Instance.Logic.AddComponent(myEnt.ID, newComponent);
```

### Obtaining Entities
```
var myEnt = CaravelApp.Instance.Logic.GetEntity("/path/to/entity/from/root");
var childEnt = myEnt.GetEntity("/path/to/entity/from/parent");
var entByID = CaravelApp.Instance.Logic.GetEntity(someEntityID);
```

### Managing Scenes
```
var _sceneID = CaravelApp.Instance.Logic.LoadScene("scenes/SomeScene.cvs", "Default", "SceneName");

//Multiple scenes can be loaded at the same time
var _scene2ID = CaravelApp.Instance.Logic.LoadScene("scenes/SomeScene.cvs", "Default", "SceneName2");

//Load scene as a child of an entity (the null parameters scene overrides the allow overriding the root entity's transform and component parameters)
CaravelApp.Instance.Logic.LoadScene("scenes/SubScenes/playerCard.cvs", "Default", "SecondSceneName", null, null, myParentEnd.ID);

//Unload Scene by ID and by Path
CaravelApp.Instance.Logic.UnloadScene(_scene2ID);
CaravelApp.Instance.Logic.UnloadScene(myParentEnt.EntityPath + "/SecondSceneName");

//Setting scene as main. New created entities are created as part of the main scene unless they're created as children of an entity in a different scene)
CaravelApp.Instance.Logic.SetMainScene(_sceneID);
```

### Using the Event Manager and Process Manager
```
Cv_EventManager.Instance.AddListener<SomeEvent>((Cv_Event evt) => { /* Do something when the event is fired */ });
Cv_EventManager.Instance.QueueEvent(new SomeEvent("Some Event Data", 4));
Cv_EventManager.Instance.RemoveListener<SomeEvent>(OnEventCallback);

var myProcess = new Cv_TimerProcess(100, () => { /* Do someting after 100ms */ });
Cv_ProcessManager.Instance.AttachProcess(myProcess);
myProcess.Fail() //Stops the process
myProcess.Succeed() //Stops the process
myProcess.Pause() //Pauses the process
```

In order to create new events or processes simply extend the Cv_Event and Cv_Process classes in your code.

### Defining a new component
In order to define a new component simply inherit from the Cv_EntityComponent class and implement its methods (Initialize, Update, Destroy, etc).
**You will also need to define that component in the components XML definition file (components.xml if you're using the startup project though you can change this in the .cvp file).**

Here is an example of a component definition in the components XML file:
```
<Component name="SomeComponent" namespace="YourNameSpace">
  <Element name="PropertyNameInEntityXML" type="float" fieldNames="value" editorNames="Value" propertyNames="PropertyNameInC#"/>
  <Element name="Property2NameInEntityXML" type="boolean" fieldNames="value" editorNames="Value" propertyNames="Property2NameInC#"/>
  <Element name="Property3NameInEntityXML" type="int" fieldNames="x,y" editorNames="X,Y" propertyNames="Property3NameInC#"/>
  <Element name="Property4NameInEntityXML" type="string" fieldNames="value" editorNames="Value" propertyNames="Property4NameInC#"/>
  <Element name="Property5NameInEntityXML" type="file" fieldNames="file" extensions="Sound Files(*.WAV;)|*.WAV;|All files (*.*)|*.*" propertyNames="Property5NameInC#"/>
  <Element name="Property6NameInEntityXML" type="comboBox" fieldNames="value" options="Option1,Option2,Option3" propertyNames="Property6NameInC#"/>
</Component>
```
These definitions also tell the editor how to show the component fields in the UI (for example a Boolean will be a checkbox while an int will be a text box and a file will show a browse button).

### How to do Other Stuff

Unfortunately I don't have time to create a comprehensive guide but you are free to take a look in the source code. If I have time later down the line I might expand this list with more examples (such as creating game views, playing sounds, using the built in components to play sounds and create sprites, run lua scripts, perform raycasts, etc).
