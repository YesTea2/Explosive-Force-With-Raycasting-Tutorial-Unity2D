using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Objects for the laser mouth to use")]
    [SerializeField] GameObject laserObject;
    [SerializeField] Transform areaToShootLaserFrom;
    [SerializeField] float waitToDestroy = .5f;
    [SerializeField] Sprite spriteForHubertShooting;

    private SpriteRenderer thisRenderer;
    private Sprite storedSprite;
    private void Start()
    {
        thisRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
        storedSprite = thisRenderer.sprite;
    }
    // Start is called before the first frame update
    public void ShootLaserMouth()
    {
        thisRenderer.sprite = spriteForHubertShooting;
        GameObject objectForLaserMouth = Instantiate(laserObject, areaToShootLaserFrom.position, Quaternion.identity);
        Destroy(objectForLaserMouth, waitToDestroy);

        StartCoroutine(ResetSpriteToNormal());
    }

    IEnumerator ResetSpriteToNormal()
    {
        yield return new WaitForSeconds(.5f);
        thisRenderer.sprite = storedSprite;
        yield break;

    }
}
