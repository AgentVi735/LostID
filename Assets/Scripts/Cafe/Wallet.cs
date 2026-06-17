using System.Collections;
using UnityEngine;

public class Wallet : MonoBehaviour
{
    [Header("Objects")]
    [SerializeField] private Transform wallet;
    [SerializeField] private GameObject charm;
    [SerializeField] private PoofParticle particle;

    [Header("Settings")]
    [SerializeField] private float delayAfterPoof;
    private WaitForSeconds waitDelayAfterPoof;
    [SerializeField] private float slideTime;
    [SerializeField] private Vector3 startPos;
    [SerializeField] private Vector3 endPos;
    [SerializeField] private float delayBeforePoof;
    private WaitForSeconds waitDelayBeforePoof;

    private void Awake()
    {
        waitDelayAfterPoof = new WaitForSeconds(delayAfterPoof);
        waitDelayBeforePoof = new WaitForSeconds(delayBeforePoof);
    }

    public IEnumerator StartSlide(bool showCharm)
    {
        particle.Play();
        charm.SetActive(showCharm);
        wallet.gameObject.SetActive(true);
        wallet.position = startPos;

        yield return waitDelayAfterPoof;

        for (float i = 0; i < slideTime; i += Time.deltaTime)
        {
            wallet.position = Vector3.Lerp(startPos, endPos, i / slideTime);

            yield return null;
        }

        wallet.position = endPos;

        yield return waitDelayBeforePoof;

        particle.Play();
        wallet.gameObject.SetActive(false);
    }
}
