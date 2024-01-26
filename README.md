# My Shaders Portfolio

## Introduction
This repository is a showcase of my custom shader work in Unity, featuring two primary shaders: a Ghibli-style Grass Shader and an advanced Water Shader. The Water Shader is particularly complex, incorporating planar reflection, caustics, Gerstner waves, depth-based foam, and a versatile floating system.

## Grass Shader
![image](https://github.com/Chiasera/Shaders_playground/assets/70693638/79c2dc97-72cc-4105-b914-d404f1574165)

Please see an updated version for better visuals of the grass shader here: https://github.com/Chiasera/Stylized-grass
### Noise Displacement and collision detection
- The grass shader includes a basic collision detection mechanism that bends grass in a circle arround the agent.
- Three noises are used for the grass physics: one for the y movement, one for the x movement, and one for the "agitation" representing irregularities in the local wind
- ![image](https://github.com/Chiasera/Shaders_playground/assets/70693638/27587b09-6366-4230-af9e-1a3348913643)
- The grass models have been made using blender, and are vertex colored. We then sample the red channel of the vertex color to determine the amplitude of the grass blade at different height: the root should not move, and the tip of the grass should be strongly affected by the wind. This also allows us to realistically bend the grass blade and stay consistent: when a vertex needs to go go further along a given axis, we also decrease its height. Then, we sample the vertex color once more to get a curvature instead of a linear movement (You can look into the shader to dissect it, I also put sticky notes in the shadergraph)
- ![image](https://github.com/Chiasera/Shaders_playground/assets/70693638/40b2e84e-6d2c-4699-bdab-424e17724ccf)


## Water Shader
![image](https://github.com/Chiasera/Shaders_playground/assets/70693638/078ea4c8-386c-491b-bfc3-b1770262c267)
The Water Shader is a comprehensive solution for realistic water rendering. Key features include:

### Planar Reflections
The shader achieves planar reflections by utilizing a secondary camera that mirrors the main camera's view onto the water's surface. This camera only renders a specific layer to capture the necessary reflections, ensuring performant and realistic water surfaces.

### Caustics
Caustics are implemented to simulate light patterns created when light rays are refracted or reflected by water surfaces. This adds a layer of realism and depth to underwater environments.

### Gerstner Waves
Gerstner waves provide a realistic wave pattern on the water surface. The implementation is based on the equation provided by Nvidia, offering a mathematically accurate simulation of wave movement. The equation takes into account factors such as wave direction, wavelength, and steepness to produce a convincing wave pattern.
See equation *9* of following article: https://developer.nvidia.com/gpugems/gpugems/part-i-natural-effects/chapter-1-effective-water-simulation-physical-models

### Depth Texture and Foam
The shader uses the scene's depth texture to determine pixel depth, allowing for dynamic foam generation around shorelines and objects intersecting with the water surface. This technique adds realism by highlighting areas where water interacts with its surroundings.

### Floating System
The floating system in the shader accommodates two types of floaters:
- **Basic Floater**: This simple floater considers the object's center of mass for buoyancy calculations.
- **Advanced Floater**: This more sophisticated system allows for defining custom anchor points for buoyancy, enabling objects to float based on their actual physical structure rather than a presumed center of mass.

### Refraction and Distortion
Refraction is achieved by distorting normal textures and applying the result in the vertex shader. This technique simulates the bending of light as it passes through the water.

## Usage
Clone the repository and use it at your will

## License
[MIT](https://choosealicense.com/licenses/mit/)

---

Enjoy and feel free to reach out with any questions or suggestions to improve the shaders!
