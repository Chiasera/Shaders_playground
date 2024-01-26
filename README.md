# My Shaders Portfolio

## Introduction
This repository is a showcase of my custom shader work in Unity, featuring two primary shaders: a Ghibli-style Grass Shader and an advanced Water Shader. The Water Shader is particularly complex, incorporating planar reflection, caustics, Gerstner waves, depth-based foam, and a versatile floating system.

## Grass Shader
Please see an updated and better version of the grass shader here:

## Water Shader
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
Instructions on how to implement and use these shaders in your Unity projects are provided in the respective directories:
- `GrassShader/`: Contains the grass shader assets and implementation details.
- `WaterShader/`: Contains the water shader assets, technical documentation, and setup instructions.

## Contributing
Contributions to enhance the shaders or add new ones are welcome. Please feel free to fork the repository, make your changes, and submit a pull request.

## License
[MIT](https://choosealicense.com/licenses/mit/)

---

Enjoy and feel free to reach out with any questions or suggestions to improve the shaders!
