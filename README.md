# VR-Molecular-Chemistry-Lab

VR Molecular Chemistry Lab - XR Developer Assessment
A high-fidelity Virtual Reality chemistry simulation built with Unity 6 , designed for the Meta Quest ecosystem. This project allows students to interactively combine atomic elements (H, O, C, N) to form valid chemical molecules using intuitive VR hand interactions.

🛠 Features & Architecture
The project follows a modular, data-driven approach to ensure scalability and maintainability:

Data-Driven Chemistry: Uses ScriptableObjects (MoleculeDefinition) to define valid molecular combinations, avoiding hardcoded formulas in scripts.

XR Interaction System: Fully integrated with XR Interaction Toolkit (XRIT) 3.0 , supporting smooth grabbing, hovering, and ray-cast UI interactions.

Holographic Bonding Zone: A visual-affordance system that changes colors based on the interaction state (Idle, Active, Processing).

Real-time Molecule Inspector: A world-space UI panel that dynamically displays the name, formula, and bond types of newly discovered molecules.

Robust Reset Mechanism: Molecules can be broken back into individual atoms, utilizing a circular spawn algorithm to prevent physics collisions.

Audio Feedback: An AudioManager provides satisfying auditory cues upon successful bond formation.

🤖 AI Tools Used
In compliance with the assessment requirements, AI was utilized as a core development partner for architecture and technical problem-solving:

ChatGPT / Claude (Architecture & Logic): Used to plan the modular C# architecture, specifically for decoupling the BondManager from the UIManager through C# Events, ensuring the code adheres to SOLID principles.

Gemini AI Debugging (Physics Stabilization): Assisted in resolving "Physics Explosions" during the Molecule Reset phase by suggesting a circular offset spawn logic with zero-velocity resets for Rigidbodies.

AI Research (Chemical Accuracy): Rapidly gathered and verified the required bond types (Single, Double, Triple) and required elements for all 18 target molecules to ensure scientific accuracy within the MoleculeDatabase.

⚙️ Setup & Installation
To run or build this project, please follow these steps

Prerequisites

Unity Version: Unity 6 (6000.0.x or latest stable).

Platform: Android (Meta Quest 2/3/Pro)

Render Pipeline: Universal Render Pipeline (URP).

Steps to Open
1.Clone the Repository: git clone [Your Repository Link]

2.Open in Unity Hub: Add the project and ensure the Editor version is set to Unity 6.

3.Package Manager: Upon opening, Unity should automatically resolve dependencies (XR Interaction Toolkit, TextMeshPro, OpenXR).

4.Scene Location: Open Assets/Scenes/MainLab.unity.

5.Build Settings: * Switch platform to Android.
  -Set Texture Compression to ASTC.
  -Ensure Minimum API Level is set to 29.
  -Set Scripting Backend to IL2CPP.

🧪 Implemented MoleculesThis project includes a comprehensive library of 18 valid molecules:
Gases: H2O, O2, N2, CO, NO
Solids/Liquids: H2O, NH3, CO2, CH4, HCN, OH, N2H2, N2H4, C2N2, CH2O, CH4O, CH4N2O, C2H5NO2

📄 Submission Materials

GitHub Repository: https://github.com/sanhavichaom/VR-Molecular-Chemistry-Lab

Video Demo: 

Android APK: [Google Drive/WeTransfer Link]
