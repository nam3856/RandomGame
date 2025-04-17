using System.Collections;
using UnityEngine;

public class Subscription : MonoBehaviour
{
    public GameObject[] objectsToActive;

    public void GoSub()
    {
        StartCoroutine(StartText());
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
        yield return new WaitForSeconds(6f);
        objectsToActive[i].SetActive(false);
        i++;
        objectsToActive[i].SetActive(false);
    }
}
