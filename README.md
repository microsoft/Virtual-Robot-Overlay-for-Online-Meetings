
# VROOM: Virtual Robot Overlay for Online Meetings

VROOM is the XR Telepresence technology probe/prototype system featured in these research publications:

- Jones, B., Zhang, Y., Wong, P.N.Y., and Rintel, S. (2021). Belonging There: VROOM-ing into the Uncanny Valley of XR Telepresence. In *Proceedings of the ACM on Human-Computer Interaction, 5 (CSCW1)*, ACM.

- Jones, B., Zhang, Y., Wong, P.N.Y., and Rintel, S. (2020). VROOM: Virtual Robot Overlay for Online Meetings. In *Extended Abstracts of the 2020 ACM Conference on Human Factors in Computing Systems (CHI 2020)*, ACM.

- Jones, B., Zhang, Y., Wong, P.N.Y., Rintel, S., and Heshmat, Y. (2020). VR-Enabled Telepresence as a Bridge for People, Environments, and Experiences. In *Social VR: A New Medium for Remote Communication and Collaboration (Workshop at CHI 2020)*.


<img src="/Figure1-Overview.png" width=50% height=50%>

<img src="/Figure2-System.jpg" width=80% height=80%>

Demo video:

[![VROOM DEMO VIDEO](http://img.youtube.com/vi/9ZZ-YdUU01w/0.jpg)](http://www.youtube.com/watch?v=9ZZ-YdUU01w "VROOM Demo Video")

Contained in this repository is the source code for the VROOM technology probe, slightly modified to include only open-source components. **Please note that as this is a research prototype, the code is not stable and may not be fully functional on your environment.** Furthermore, getting the experience fully running as it was in our research explorations requires time, commitment, and access to expensive equipment. In the future, we hope to turn this repository into a toolkit for easy prototyping of XR telepresence experiences in general, which could be used with both specialized and general-purpose equipment. In the meantime, we try to make this repository as easy-to-understand, easy-to-run as possible. We hope this repository can at least be useful as a set of code samples illustrating how we got the original VROOM system up and running, and hopefully it can help others prototype similar XR telepresence experiences.

This repository consists of the following components:

- **VROOM-Server:** A Node.js server that mediates communication between all of the other components of VROOM.
- **VROOM-360Broadcaster:** A web app (running in a web browser) used to stream video from a 360° camera attached to the telepresence robot to the VR client app. This web app runs on a laptop computer attached to the robot, which the 360° camera is plugged in to.
- **VROOM-Remote-VR:** The VR app, which runs on a Windows desktop computer with a Windows Mixed Reality VR headset connected to it. This app is used by the remote user to drive the telepresence robot. Through this app, the remote user can see an immersive view of the telepresence robot's surroundings (via the 360° camera attached to the top of the robot) and a first-person view of their virtual avatar body. The remote user can also use Windows Mixed Reality motion controllers to control their avatar's hands, and look around to tilt their avatar's head. All of these movements will be seen by the local user on the other side through their app. Using joysticks on the Windows Mixed Reality motion controllers, the remote user can also drive the telepresence robot.
- **VROOM-Local-AR:** The AR app, which runs on a Microsoft HoloLens (v1, not yet tested on HoloLens 2). This app is used by the local user to see the remote user's avatar superimposed over the telepresence robot. The remote user's avatar is overlaid by using QR-code-like marker patterns printed on paper and attached to the telepresence robot.
- **VROOM-RobotController:** This app receives virtual keyboard commands from the VROOM-Remote-VR app, sent when the remote user uses the joystick on their Windows Mixed Reality motion controllers, and uses those commands to drive the telepresence robot (a Beam robot) by sending those commands to the commercial Beam desktop app (https://app.suitabletech.com/installers/stable). This app must run on a separate Windows desktop computer with the Beam app connected to the telepresence robot and active in the foreground.

To run the system, follow these steps:

1. **Copy the repository to a directory on your machine.**

2. **Install a self-signed certificate in the server directory.** This is needed for the Node.js server app, to allow a browser to load the 360° broadcaster app via HTTPS by connecting to the server. To do this, install OpenSSL (https://www.openssl.org/) on your machine, then CD to the *VROOM-Server* directory and run the following command:
   
   ```
   > openssl req -nodes -new -x509 -keyout key.pem -out cert.pem
   ```
   Then answer the questions as you wish. For more information on this process, visit here: https://flaviocopes.com/express-https-self-signed-certificate/

3. **Run the server.** Install Node.js (https://nodejs.org/) on your machine. CD to the *VROOM-Server* directory. Run the following commands:

   ```
   > npm install
   > npm start
   ```

   *npm install* only needs to be run once, to install the Node.js dependencies that the server app relies on. Following this, you only need to run *npm start* whenever you want to get the server up and running on your machine.

4. **Attach the QR-code-like marker patterns to the telepresence robot.** The marker patterns needed can be downloaded from here: https://github.com/qian256/HoloLensARToolKit/tree/master/Markers. The markers should be attached to the robot in a style similar to this:

   <img src="/robot-marker-patterns.png" width=20% height=20%>

5. **Setup and run the 360° streaming from the robot:**

   - Attach the RICOH Theta V 360° camera to the top of the Beam robot.
   - Plug the camera in to a laptop via USB (to connect as a standard webcam). Turn on the RICOH Theta V camera and set it to livestream mode.
   - Attach the laptop to the Beam robot.
   - On the laptop, open a web browser (preferably Chrome, Edge, or a modern browser compatible with WebRTC).
   - Open the address *https://(address-of-machine-running-node-server-app):8082*. If you see a security warning, acknowledge and proceed to load the webpage.
   - Grant permission for the webpage to use the webcam. Make sure the RICOH Theta V camera is the selected webcam.
   - Make sure the laptop will not fall asleep if the lid is closed or after being inactive for a period of time. There are various means and tools (e.g., InsomniaX for macOS) that will allow you to do this, depending on your OS.

6. **Build, deploy and run the VR app:**

   - Open the Unity project contained in *VROOM-Remote-VR*. Open the scene titled *VROOM-Remote-VR*.
   - Select the *VR360Viewer/NetworkEvents* object in the scene. Under the *Network Events* script/component, set the *HTTP Server Address* value to the IP address of your machine (or the machine running the Node.js server app). Keep the 'http://' and the port number (3000) in the address.
   - Select the *VR360Viewer/PeerConnection* object in the scene. Under the *Node DSS Signaler* script/component, set the *HTTP Server Address* value to the IP address of your machine (or the machine running the Node.js server app). Keep the 'http://' and the port number (3000) in the address.
   - Go to *File > Build Settings*, make sure the build platform is set to UWP (Universal Windows Platform), select *Switch Platform*.
   - Go to *Player Settings (UWP) > XR Settings*, make sure *Virtual Reality Supported* or *XR Supported* is enabled.
   - Build the project. This will build a Visual Studio project/solution. Open this in Visual Studio 2019.
   - In Visual Studio, configure the solution to build a *Release* build for *x64*, to deploy on the *Local Machine*, then build. This will build and run a UWP app, which will run on Windows Mixed Reality.

7. **Build, deploy, and run the AR app:**

   - Open the Unity project contained in *VROOM-Local-AR*. Open the scene titled *VROOM-Local-AR*.
   -  The AR app has a dependency that you need to add manually: [HoloLensARToolKit(v0.2)](https://github.com/qian256/HoloLensARToolKit/tree/6d3560739dbab6cdb8a22396092a4cf3554bb0fc). Make sure you downloaded this [unity packages](https://github.com/qian256/HoloLensARToolKit/blob/6d3560739dbab6cdb8a22396092a4cf3554bb0fc/ARToolKitUWP.unitypackage), and import it into the 'VROOM-Local-AR' project. To import the package after you download it to your computer, in Unity editor, select 'Assets > Import package > Custom package..', and select the package you have downloaded.
   - Select the *NetworkEvents* object in the scene. Under the *Network Events* script/component, set the *HTTP Server Address* value to the IP address of the machine running the Node.js server app. Keep the 'http://' and the port number (3000) in the address.
   - Go to *File > Build Settings*, make sure the build platform is set to Windows Store or UWP (Universal Windows Platform). Set the *SDK* to 'Universal 10', *target device* to 'HoloLens', *UWP Build Type* to 'D3D', *UWP SDK* to 'Latest installed', and *build and run on* to 'Local Machine'. Then select *Switch Platform*.
   - Go to *Player Settings (UWP)*, make sure *VR Supported* (or *XR Supported*) is enabled, and the VR SDK is set to *Windows Holographic* or *Windows Mixed Reality*.
   - Build the project into a new directory: *(repo-directory)/VROOM-Local-AR/Build*. This will build a Visual Studio project/solution. Open this in Visual Studio 2017.
   - In Visual Studio, configure the solution to build a *Release* build for *x86* (if deploying to HoloLens 2, select *ARM*). Select deployment machine as *Device* if deploying to a HoloLens plugged in to your computer via USB, or *Remote Machine* is deploying to a HoloLens via the network. Then build.
   - If an error occurs in the build process, try the following:
     - Copy the file *(repo-directory)/VROOM-Local-AR/Other/project.lock.json*, paste it into the directory *(repo-directory)/VROOM-Local-AR/Build/VROOM-Local-AR* (replacing the previous version of the file that was there).
     - Try building again.
   - Accept all permissions it asks for, then test the app on your HoloLens.
   

> The avatar body we use in this repository is from [Microsoft-RocketBox](https://github.com/microsoft/Microsoft-Rocketbox), an open source avatar library that provides a nice variety of fully rigged high-definition avatar choices. The avatar head we use is made from [AvatarMakerPro](https://assetstore.unity.com/packages/tools/modeling/avatar-maker-pro-3d-avatar-from-a-single-selfie-134800), a tool from Unity Asset Store - in case you are interested in how we created the avatars and possibly create your own :)


8. **Build, deploy, and run the Robot Controller app:**

   - On another Windows desktop computer (separate from the one running the VR app), open the Visual Studio project/solution contained in *VROOM-RobotController* in Visual Studio 2019.
   - Open the file *Program.cs*. On line 18, change the IP address argument to the IP address of the machine running the Node.js server app. Keep the 'http://' and the port number (3000) in the address.
   - Build and run the app in Visual Studio.
   - Leave the app running in the background (i.e., minimized). Open the Beam desktop app (https://app.suitabletech.com/installers/stable) and connect to the Beam robot. Keep the Beam app active in the foreground and select the bottom (downward-facing camera) view. This will allow the virtual keyboard commands coming from the VR app to be applied to the Beam app, thus driving the robot.

