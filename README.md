# Gemini Plugin

![](images/gemini_image.png)

The Gemini package is a framework for the creation of Digital Twins in Unity.
It provides a built in custom editor window, which guides the user through the process of creating their own Digital Twin.
Configuration of network protocols are made uncomplicated and basic support for MQTT and GraphQL are provided.
Further, we allow the user to easily configure sensors of a device as inputs of the Digital Twin.
We provide and generate basic prefabs, that can be used to visualize and simulate the data received from the real device.
Finally, our manager scripts track all registered objects, trigger events, and are able to replay collected data.

![](images/brewery_scene.png)


## Requirements

**Platform:** Unity (recommended version: 2020.1.17f1 or newer)

**Programming Language**: C#

**Supported network protocols**: MQTT, Graph Ql

**Message format**: JSON

**Unity package**: jilleJr Newtonsoft package 

Simply open the Unity package manager through **Window > Package Manager**. Then click on **+** to add a package from this Git URL: https://github.com/jilleJr/Newtonsoft.Json-for-Unity.git#upm


## Usage

Download the Gemini package and import it into a Unity project.
Load the pre-configured scene under ***Scenes/GEMINI Digital Twin*** and start the plugin from the ***Gemini Editor > Run*** menu entry.
The wizard will take you step by step through the process of configuring the basics of your Digital Twin implementation.

For more detailed instructions check out our **user guide**.
