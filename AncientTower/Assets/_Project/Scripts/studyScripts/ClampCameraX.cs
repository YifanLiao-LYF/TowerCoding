using UnityEngine;
using Cinemachine;

[SaveDuringPlay]
//[AddComponentMenu("")]
public class ClampCameraX : CinemachineExtension
{
    public float minX = 0f;
    public float maxX = 20f;

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage,
        ref CameraState state,
        float deltaTime)
    {
        if (stage != CinemachineCore.Stage.Finalize) return;

        Vector3 pos = state.RawPosition;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        state.RawPosition = pos;
    }
}