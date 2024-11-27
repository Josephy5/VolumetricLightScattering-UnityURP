![Godrays](https://github.com/user-attachments/assets/c283213e-a2e1-4544-a230-91b8c30db6be)

# Volumetric Light Scattering / God rays
![Unity Version](https://img.shields.io/badge/Unity-6000.0.27%27LTS%2B-blueviolet?logo=unity)
![Unity Pipeline Support (Built-In)](https://img.shields.io/badge/BiRP_❌-darkgreen?logo=unity)
![Unity Pipeline Support (URP)](https://img.shields.io/badge/URP_✔️-blue?logo=unity)
![Unity Pipeline Support (HDRP)](https://img.shields.io/badge/HDRP_❌-darkred?logo=unity)
 
A Volumetric light scattering effect that is used to replicate the visuals of god rays in Unity. It was created for Unity URP (6000.0.27f1) and for Serious Point Games as part of my studies in shader development.
It could theoretically run on Unity 2022 since its using the same code I used for the effects's render feature and pass within Unity 2022, but it is untested.

This effect only works on one directional light, and does not support point, spot, or multiple directional lights.
<br>
***
**Note:** I have previously created a separate volumetric light scattering effect in Unity 2022 for my own personal use. The code I used for that effect is entirely different from the code in this repository. 
However, I may reuse some code from this repository in my personal project, as some of its ray marching and light scattering-related code is better than what I developed earlier.
___

## Features
- Fully adjustable screen-based ray marched godrays
- Can choose to use Schlick or Henyey Greenstein
- Adjustable downsampling
- Adjustable via Volume Component

## Example[s]
![Godrays](https://github.com/user-attachments/assets/c283213e-a2e1-4544-a230-91b8c30db6be)
<br>
What it looks with the effect on
<br><br>
![GodraysWithout](https://github.com/user-attachments/assets/570083a0-e176-4996-a722-15b5b3464284)
<br>
What it looks without the effect

## Installation
1. Clone repo or download the folder and load it into an unity project.
2. Ensure that under the project settings > graphics > Render Graph, you enable Compatibility Mode on (meaning you have Render Graph Disabled).
3. Add the render feature of the effect to the Universal Renderer Data you are using.
4. [Optional] Create a volume game object and load the effect's volume component in the volume profile to adjust values
5. If needed, you can change the effect's render pass event in its render feature under settings.

## Credits/Assets used
Some of the shader code is based on Mr-222’s Unity Volumetric Rendering Repo’s
Volumetric Lighting code. [GitHub Repo Link](https://github.com/Mr-222/Unity_Volumetric_Rendering/tree/main)
<br>
<br>
"Mossy/Grassy Landscape" (https://skfb.ly/6RYvL) by Šimon Ustal is licensed under Creative
Commons Attribution (http://creativecommons.org/licenses/by/4.0/). No changes made.
(The mountain landscape that you see in the screenshots for demonstration purposes)
