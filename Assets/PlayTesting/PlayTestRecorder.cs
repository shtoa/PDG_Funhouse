using System.Collections;
using System.Collections.Generic;
using UnityEditor.Recorder.Input;
using UnityEditor.Recorder;
using UnityEditor;
using UnityEngine;
using UnityEditor.Recorder.Encoder;

[InitializeOnLoad]
public static class PlayTestRecorder
{
    public static RecorderController TestRecorderController;
    public static RecorderControllerSettings controllerSettings;


    static public void StartRecording(string dir, string filename)
    {
        // code from https://discussions.unity.com/t/control-unity-recorder-from-script/779432/7
        controllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
        TestRecorderController = new RecorderController(controllerSettings);

        var videoRecorder = ScriptableObject.CreateInstance<MovieRecorderSettings>();
        videoRecorder.name = "My Video Recorder";
        videoRecorder.Enabled = true;

        videoRecorder.EncoderSettings = new CoreEncoderSettings
        {
            Codec = CoreEncoderSettings.OutputCodec.MP4,
            TargetBitRate = 4000,
            GopSize = 240

        };

        videoRecorder.ImageInputSettings = new GameViewInputSettings
        {
            OutputWidth = 1280,
            OutputHeight = 720
        };

        videoRecorder.CaptureAudio = false;

        videoRecorder.OutputFile = dir+filename;
        

        controllerSettings.AddRecorderSettings(videoRecorder);
        controllerSettings.SetRecordModeToManual();
        controllerSettings.FrameRate = 100;
        //controllerSettings.CapFrameRate = false;


        RecorderOptions.VerboseMode = false;
        TestRecorderController.PrepareRecording();
        TestRecorderController.StartRecording();

        Debug.Log($"Started Recording: {TestRecorderController.IsRecording()}");
    }

    static public void StopRecording()
    {
        if (TestRecorderController != null && TestRecorderController.IsRecording())
        {
            TestRecorderController.StopRecording();
            Debug.Log("Stopped Recording.");
        }
    }

}
