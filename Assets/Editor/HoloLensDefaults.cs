/*
 * Adam Tuliper  5/23/2016
 * This Editor script should go in your /Assets/Editor folder.
 * It will create a HoloLens menu with an option "Set Project And Scene Defaults"
 * This option will set your project defaults as described when you run it.
 * When complete, it will open the Quality Settings menu so you can manually set
 * the 'Fastest' default level (as that API isn't exposed yet in Unity)
 * */
using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Linq;
using System;
using System.Text;

public class HoloLensDefaults : MonoBehaviour
{

    // Use this for initialization
    [MenuItem("HoloLens/Set Project And Scene Defaults")]
    private static void SetHoloLensDefaults()
    {
        StringBuilder output = new StringBuilder();
        if (EditorUtility.DisplayDialog("Save HoloLens Changes",
    "This process will:\r\n\r\n1. Set the camera defaults\r\n2. Change the build default to Windows Universal and enable the Virtual Reality Supported option.\r\n\r\nAll changes will be saved as soon as completed including any pending scene changes.\r\nContinue?", "Yes", "No"))
        {
            SwitchBuildTargetToUWP(output);
            ApplyVrSettings(output);
            //No api to control this in the editor currently.
            //SetQualityLevel(output);
            UpdateCameras(output);
            output.Append("\r\n\r\n**One last thing you will need to manually check is Edit/Project Settings/Quality. Look at the Windows Store app column and at the bottom select the default quality setting as 'Fastest'. Opening Quality Settings now...**");
            EditorUtility.DisplayDialog("Results", output.ToString(), "Ok");
            EditorApplication.ExecuteMenuItem("Edit/Project Settings/Quality");
        }

    }

    private static void ApplyVrSettings(StringBuilder output)
    {

        output.Append("Checking VR Settings for WSA\r\n");
        //bool vrSupported = PlayerSettings.virtualRealitySupported = true;
        //Warning, not public yet. May change. Above API setting will work if we assume
        //WSA platform is active build target, but if they don't have WSA players installed
        //then this be an installed option. Not a problem in HoloLens Technical Preview
        //but could be if run on any other version or in the future.
        bool vrSupported = UnityEditorInternal.VR.VREditor.GetVREnabled(BuildTargetGroup.WSA);

        output.Append("     VR Enabled: ");
        //enable VR
        if (!vrSupported)
        {
            //PlayerSettings.virtualRealitySupported = true;
            UnityEditorInternal.VR.VREditor.SetVREnabled(BuildTargetGroup.WSA, true);
            output.Append("  Updated");
        }
        else
        {
            output.Append("  OK");
        }

        bool windowsHolographic = false;

        var vrDevices = UnityEditorInternal.VR.VREditor.GetVREnabledDevices(BuildTargetGroup.WSA);
        foreach (var device in vrDevices)
        {
            //Debug.Log(device);
            if (device == "HoloLens")
            {
                windowsHolographic = true;
            }
        }

        if (!windowsHolographic)
        {
            output.Append(
                "\r\n'Windows Holographic' (HoloLens) was not found as a VR type in the Windows 10 Player settings. This should be listed there. This process will continue, but when it's done verify in your Player Settings (File-Build Settings, click on Windows Store, ensure Windows 10 is selected in the dropdown, and click Player Settings. Then on the right in 'Other Settings' ensure VR Supported is checked off and Windows Holographic shows up in the options. If not either the SDK name has changed or you possibly have a version of Unity that doesn't support the HoloLens.");
        }
        else
        {
            output.Append("     Windows Holographic SDK: OK");
        }
        output.AppendLine();
        output.AppendLine();
    }

    private static void SwitchBuildTargetToUWP(StringBuilder output)
    {
        //Check current sdk.
        output.Append("Checking Build Settings for WSA\r\n");
        bool currentlyWSA = EditorUserBuildSettings.activeBuildTarget == BuildTarget.WSAPlayer;
        bool wsaSDK = EditorUserBuildSettings.wsaSDK == WSASDK.UWP;
        bool unityCSharpProjs = EditorUserBuildSettings.wsaGenerateReferenceProjects;

        output.Append("     Build Target WSA:");
        if (!currentlyWSA)
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.WSAPlayer);
            output.Append("  Updated");
        }
        else
        {
            output.Append("  OK");
        }

        output.Append("\r\n     Targeting Windows 10:");
        if (!wsaSDK)
        {
            EditorUserBuildSettings.wsaSDK = WSASDK.UWP;
            output.Append("  Updated");
        }
        else
        {
            output.Append("  OK");
        }
        output.Append("\r\n     Generate Unity C# Projects:");
        if (!unityCSharpProjs)
        {
            EditorUserBuildSettings.wsaGenerateReferenceProjects = true;
            output.Append("  Updated");
        }
        else
        {
            output.Append("  OK");
        }

        output.AppendLine();
        output.AppendLine();
    }

    private static void SetQualityLevel(StringBuilder output)
    {
        output.Append("Checking Quality Settings for WSA\r\n");
        //TODO edit ProjectSettings or overwrite.

    }

    private static void UpdateCameras(StringBuilder output)
    {
        output.Append("Checking Camera Settings\r\n");
        Camera[] cameras = new Camera[Camera.allCamerasCount];
        int cameraCount = Camera.GetAllCameras(cameras);
        Debug.Log(string.Format("{0} cameras found", cameraCount));

        if (cameraCount == 1)
        {
            //With one camera, we just update it without prompting
            SetCameraDefaults(cameras[0], output);
        }
        else
        {
            //More than one camera, lets prompt for each one.
            //Do we want two at the same location?
            foreach (var camera in cameras)
            {
                var cameraDetails = string.Format("Update this camera for HoloLens defaults? \r\nThis will set position to 0,0,0 and background color to Clear Flags:Solid color(Black). \r\n \r\n---------------\r\nName: {0} \r\nPosition: {1}\r\nTag: {2}\r\n---------------",
                    camera.name, camera.transform.position, camera.tag);
                if (EditorUtility.DisplayDialog("Multiple Cameras Detected",
cameraDetails, "Yes", "No"))
                {
                    SetCameraDefaults(camera, output);
                }
            }
        }
        output.AppendLine();
    }


    private static void SetCameraDefaults(Camera camera, StringBuilder output)
    {
        output.Append(string.Format("     Checking camera {0} at {1}", camera.name, camera.transform.position.ToString()));
        output.Append("\r\n     Checking position:");
        if (camera.transform.position != Vector3.zero)
        {
            camera.transform.position = Vector3.zero;
            output.Append("  Updated position\r\n");
        }
        else
        {
            output.Append("  OK\r\n");
        }
        output.Append("     Checking Clear Flags:");
        if (camera.clearFlags != CameraClearFlags.Color)
        {
            camera.clearFlags = CameraClearFlags.Color;
            camera.backgroundColor = Color.black;
            output.Append("  Updated\r\n");
        }
        else
        {
            output.Append("  OK");
        }
        output.AppendLine();
    }

}
