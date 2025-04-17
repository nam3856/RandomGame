using DG.Tweening;
using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;
using MoreMountains.Feedbacks;
public class RandomMoveLight : MonoBehaviour
{
    [SerializeField] private Light2D[] movingLights;
    [SerializeField] private Transform[] teamTargets; // 각 팀 위치를 Transform으로 등록
    [SerializeField] private float randomMoveDuration = 30f;

    private bool isFocusing = false;

    public void GoLight()
    {
        StartCoroutine(RandomMoveLights());
    }

    private IEnumerator RandomMoveLights()
    {
        isFocusing = true;
        yield return new WaitForSeconds(0.5f);
        isFocusing = false;
        while (!isFocusing)
        {
            foreach (var light in movingLights)
            {
                Vector3 randomPos = new Vector3(
                    Random.Range(-8f, 7f),
                    Random.Range(-6f, 6f)
                );
                light.transform.DOMove(randomPos, 1.5f).SetEase(Ease.InOutExpo);
                light.GetComponent<MMF_Player>()?.PlayFeedbacks();
            }

            yield return new WaitForSeconds(1.5f);
        }
    }

    private int currentLightIndex = 0;

    public void FocusLightOnTeam(int teamIndex)
    {
        isFocusing = true;

        var light = movingLights[currentLightIndex];
        currentLightIndex = (currentLightIndex + 1) % movingLights.Length;

        var targetPos = teamTargets[teamIndex].position + new Vector3(0, 1f, -3f); // 살짝 위에서 비추도록
        light.transform.DOMove(targetPos, 0.8f).SetEase(Ease.OutBack);
    }

    public void MoveLightAgain()
    {
        StartCoroutine(RandomMoveLights());
    }
}
