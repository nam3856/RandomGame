using System.Collections;
using UnityEngine;

public class Subscription : MonoBehaviour
{
    public GameObject[] objectsToActive;
    public GameObject LeftText;
    public GameObject RightText;
    private Coroutine textMoveCoroutine;
    public void GoSub()
    {
        StartCoroutine(StartText());
        textMoveCoroutine = StartCoroutine(TextMove());
    }
    public IEnumerator StartText()
    {
        objectsToActive[^1].SetActive(true);
        int i = 0;
        yield return new WaitForSeconds(0.2f);
        objectsToActive[i].SetActive(true);
        yield return new WaitForSeconds(4f);
        objectsToActive[i].SetActive(false);
        i++;
        objectsToActive[i].SetActive(true);
        yield return new WaitForSeconds(6f);
        objectsToActive[i].SetActive(false);
        i++;
        objectsToActive[i].SetActive(true);
        yield return new WaitForSeconds(4f);
        objectsToActive[i].SetActive(false);
        i++;
        objectsToActive[i].SetActive(true);
        yield return new WaitForSeconds(4f);
        objectsToActive[i].SetActive(false);
        i++;
        objectsToActive[i].SetActive(true);
        yield return new WaitForSeconds(6f);
        objectsToActive[i].SetActive(false);
        i++;
        objectsToActive[i].SetActive(true);
        yield return new WaitForSeconds(5.5f);
        objectsToActive[i].SetActive(false);
        i++;
        objectsToActive[i].SetActive(false);

        StopCoroutine(textMoveCoroutine);
        LeftText.SetActive(false);
        RightText.SetActive(false);
    }

    public IEnumerator TextMove()
    {
        while (true)
        {
            float moveSpeed = 0.012f;

            // 로컬 기준의 오른쪽 방향으로 이동
            LeftText.transform.localPosition += LeftText.transform.right * moveSpeed;
            RightText.transform.localPosition -= RightText.transform.right * moveSpeed;

            yield return null;
        }
    }
}
