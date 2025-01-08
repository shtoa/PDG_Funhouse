## 🧱 PDG_Funhouse 🧱 ◅(•-•)▻/*
---

**Final Funhouse**, is a procedural dungeon generator implementation based on the binary space partitioning algorithm. At the core of the generation are these three steps:
1. Space Partitioning
2. Room Connection
3. Corridor Generation  

Here is a small demo of the current state of the project:

[![PDG Funhouse Demo](https://img.youtube.com/vi/709TQdN045I/0.jpg)](https://www.youtube.com/watch?v=709TQdN045I&ab_channel=SHTOA)

---
--- 
<details open>

<summary>✒️ Acknowledgements ✒️</summary>

---

The following projects have contributed in large to the development of the prototype:

- **Third Person Character Controller** - developed from tutorial by *spaderdabomb*

> Spaderdabomb (2024). *FinalCharacterController*. URL: https://github.com/spaderdabomb/FinalCharacterController

- **BSP Dungeon Generation Architecture** - adapted from *SunnyValleyStudio* [MIT Licence]

> SunnyValleyStudio (2020). *Unity_Procedural_Dungeon_binary_space_partitioning*. URL: https://github.com/SunnyValleyStudio/Unity_Procedural_Dungeon_binary_space_partitioning


</details>

---

--- 

<details open>

<summary>📥 Project Installation 📥</summary>

---

#### Project Dependencies
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
After cloning the repo, open the file via Unity Hub, first navigate to the _Projects_ tab on the left hand menu. Then select the _Add_ dropdown on the right and _add project from disk_. 

![hubNavigation](markdownAssets/hubNavigation.png)

:warning: make sure to select the correct Unity version ([Unity 2022.3.28f1](https://unity.com/releases/editor/whats-new/2022.3.28#notes)) in the editor version dropdown.

Then double click the file in the projects menu to open.

---

</details>

---
---

<details open>
<summary>⚙️ Setting Up Generator ⚙️</summary>

---


Opening up the Unity file you will be greeted with an untitled scene. 

To setup the dungeon generator scene, navigate to 
```  
./Assets/Scenes
```  

double click the _Dungeon_ file to open the scene.


---

</details>


---
---

<details>
<summary> 🧱 Using the Generator 🧱 </summary>


---

#### DungeonGen GameObject
The generator will be found on the left-hand side in the hierarchy labeled DungeonGen:

![hierarchyLocation](markdownAssets/DungeonGenLoc.png)

It is possible to manipulate various fields by clicking on the DungeonGen game object such as the ones outlined below:

![dgvalues](https://github.com/user-attachments/assets/d2f7d59a-b05c-438d-ac46-11b9b799b7aa)


---

</details>

---
---
