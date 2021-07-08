# Modular Accelerometer Control

Started as a refactoring of the accelerometer-control repo, and wanting to practise a more modular design, this project aimed to split systems up into more specialized components. 

Physics Controller script handles object forces, with customization options for gravity, input and rotation, along with "arcade" styled physics that apply movement calculations directly to the object transform, bypassing Unity's rigidbody forces system.

The scope then expanded to focus more on building specific, focused behaviour scripts, and allowing them to be attached to an object that contains the corresponding controller type. These scripts automatically reference the controller and apply the desired behaviour, such as setting orientation, gravity direction, or performing ground detection using raycasts. 

This means that gameplay can be built for each object using these prebuilt behaviour scripts coupled with their respective controller, all through the Unity editor GUI.

Both a 2D and 3D gameplay application for these tools are included as seperate scenes in the Unity project.
