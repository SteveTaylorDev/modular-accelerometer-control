# Modular Accelerometer Control

Wanting to practise a more modular design, this project started as a refactoring of an earlier Accelerometer Control project, aiming to split systems up into more specialized components. 

Physics Controller script was introduced to handle objects, and is largely customizable, with settings for gravity, input and rotation, along with "arcade" styled physics that apply movement calculations directly to the object transform, bypassing Unity's rigidbody forces system.

The scope then expanded to focus more on building specific, focused behaviour scripts, and allowing them to be attached to an object that contains the corresponding controller type. These scripts would automatically reference the controller and apply the desired behaviour, such as setting orientation, gravity direction, or performing ground detection using raycasts. 

This means that gameplay can be built for each object entirely from the Unity editor, with no coding neccesary.
