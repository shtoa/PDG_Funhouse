# <div align="center"> \*\ ◅(•-•)▻ 🧱 PDG_Funhouse 🧱 ◅(•-•)▻/* </div>
---

**Final Funhouse**, is a procedural dungeon generator implementation based on the binary space partitioning algorithm. At the core of the generation are these three steps:


> <ol>
> <li> Room Connection </li>
> <li> Space Partitioning </li>
> <li> Corridor Generation </li>
> </ol>

Here is a small demo of the current state of the project:

<div align="center">

[![PDG Funhouse Demo](https://img.youtube.com/vi/709TQdN045I/0.jpg)](https://www.youtube.com/watch?v=709TQdN045I&ab_channel=SHTOA)

</div>

## Contents
1. [Acknowledgements](#1-acknowledgements)
2. [Project Installation](#2-project-installation)
3. [Setting Up Generator](#3-setting-up-project)
4. [Using the Generator](#4-using-the-generator)
5. [Play Mode](#5-play-mode)

---
--- 

<details open>
<summary align="center"> 
✒️ Acknowledgements ✒️ 
</summary>

## 1. Acknowledgements

The following projects have contributed in large to the development of the prototype:

- **Third Person Character Controller** - developed from tutorial by *spaderdabomb*

> Spaderdabomb (2024). *FinalCharacterController*. URL: https://github.com/spaderdabomb/FinalCharacterController

- **BSP Dungeon Generation Architecture** - adapted from *SunnyValleyStudio* [MIT Licence]

> SunnyValleyStudio (2020). *Unity_Procedural_Dungeon_binary_space_partitioning*. URL: https://github.com/SunnyValleyStudio/Unity_Procedural_Dungeon_binary_space_partitioning


</details>

---

--- 

<details open>

<summary align="center">📥 Project Installation 📥</summary>

## 2. Project Installation


#### :warning: Project Dependencies
- [Unity 2022.3.28f1](https://unity.com/releases/editor/whats-new/2022.3.28#notes)
- [Unity Hub 3.8.0](https://docs.unity3d.com/hub/manual/InstallHub.html) (if Unity 2022.3.28f1 downloaded separatley)

To run the project please install the above version of Unity. Then proceed to clone the project to create a local repository using the following commands:

#### HTTPS 
```   
git clone https://gitlab.doc.gold.ac.uk/miliy001/PDG_Funhouse.git 
```  

#### SSH
```   
git clone git@gitlab.doc.gold.ac.uk:miliy001/PDG_Funhouse.git
```
After cloning the repo, open the file via Unity Hub: 
1. Navigate to the _Projects_ tab on the left hand menu. 
2. Select the _Add_ dropdown on the right and _add project from disk_. 

<div align="center">
![hubNavigation](markdownAssets/hubNavigation.png)
</div>

:warning: make sure to select the correct Unity version ([Unity 2022.3.28f1](https://unity.com/releases/editor/whats-new/2022.3.28#notes)) in the editor version dropdown.

3. Then double click the file in the projects menu to open.

---

</details>

---
---

<details open>
<summary align="center">⚙️ Setting Up Generator ⚙️</summary>

## 3. Setting Up Generator


Opening up the Unity file you will be greeted with an untitled scene:

<div align="center">
![hierarchyLocation](markdownAssets/dungeonScene.png)
</div>

To setup the dungeon generator scene, navigate to:

```  
./Assets/Scenes
```  

double click the <code>Dungeon</code> file to open the scene.


---

</details>


---
---

<details open>
<summary align="center"> 🧱 Using the Generator 🧱 </summary>

## 4. Using the Generator

#### DungeonGen GameObject
The generator will be found on the left-hand side in the hierarchy labeled <code>DungeonGen</code>:

![hierarchyLocation](markdownAssets/DungeonGenLoc.png)

It is possible to manipulate various properties of the dungeon such as the **Dungeon Properties** and **Room Properties** by clicking on the <code>DungeonGen</code> game object:

1. For the DungeonCalculator:

<div align="center">
![hierarchyLocation](markdownAssets/dungeonCalculator.png)
</div>

:warning: Door Thickness has not yet been implemented

2. For the DungeonDecorator:

<div align="center">
![hierarchyLocation](markdownAssets/dungeonDecorator.png)
</div>

The gizmo on the dungeon generator can be moved and edited within the editor or in play mode:

<div align="center">
![hierarchyLocation](markdownAssets/regenerateDungeon.gif)
</div>

---

</details>

---

<details open>
<summary align="center"> 🕹️ Play Mode 🕹️ </summary>

## 5. Play Mode


You can demo the generated dungeon using a simple third person character controller in playmode. To enter play mode in the editor press <code>ctrl-p</code> 

Use the following controlls to navigate around the dungeon:
<div align="center">
![hierarchyLocation](markdownAssets/keyboardControls.png)
</div>

---

</details>

---
