using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VendingMachine : MonoBehaviour
{
    //  Headers let you see exactly what your editing in the editor with its own seperated header on the game object
    //  tooltips add the ability to hover over the area in the editor where the value is and see any text you want, great for reminders as to a order in a array or many other reasons
    //  [SerializeField]'s let you set objects in the inspector like saying Public, but keeps the information to this script instead of allowing everything to access it 

    [Header("Toggle for clicking on gameobject with script or use raycasting to detect clicking")]
    [SerializeField] bool isUsingRaycastMethod;
    [SerializeField] bool isUsingDefaultClickMethod;

    [Header("Objects to use in explosion")]
    [Tooltip("put objects that you want to launch in the explosion here, rigidbodies needed")]
    [SerializeField] GameObject[] objectsToLaunch;
    [SerializeField] GameObject particalSystem;

    [Header("Area to launch objects from")]
    [Tooltip("the spawn position empty object we set as a child of the object to explode goes here")]
    [SerializeField] Transform areaToLaunchFrom;
    [SerializeField] Transform areaToCauseParticals;

    [Header("A sprite to update the vending machine with on use")]
    [Tooltip("drag a sprite to change this object to if its been damaged / made to explode ")]
    [SerializeField] Sprite spriteForEmptyMachine;

    [Header("The amount of force to give the explosion")]
    [SerializeField] float forceOfExplosion;
    [SerializeField] float amountToShootUpwards;
    [SerializeField] float amountOfTorque;
    [SerializeField] float amountToTurn;

    //these are variables that do not need to be seen outside of the script
    SpriteRenderer vendingMachineSpriteRenderer;

    bool hasClicked;
    bool hasShotRaycast;
    PlayerController _pC;
    Sprite storedSprite;

    int layerMask = 1 << 7;


    private void Start()
    {
        
        // using FindObjectOfType lets us find the player by looking for the player controller since its the only object in the scene with that script attached 
        // another way is to find the object with a tag  this can be like below 

        // _pc = GameObject.FindGameObjectWithTag("Player");

        // using finds  are great, but can be taxing if you are constantly calling different searches this way

        _pC = FindObjectOfType<PlayerController>();
        vendingMachineSpriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
        storedSprite = vendingMachineSpriteRenderer.sprite;
    }


    private void OnMouseDown()
    {
        // if we are using the default clicking method and not using raycast method then we check that hasclicked is false
        // if hasclicked is false, we set it to true so it wont start looping forever
        // then call two functions one for the vending machine and one for the laser

        if (isUsingDefaultClickMethod && !isUsingRaycastMethod)
        {
            if (!hasClicked)
            {
                hasClicked = true;
                HitTheVendingMachine();
                ShootTheMouthLaser();

            }
        }

        // if instead isUsingRaycastMethod is true and isUsingDefaultClickMethod is false we set the bool of hasClicked to true and the rest is handled in FixedUpdate

        if (isUsingRaycastMethod && !isUsingDefaultClickMethod)
        {
            hasClicked = true;
        }

    }


    private void FixedUpdate()
    {
        // when using physics in Unity  the calls should be made in fixed update or  your going to have undesired results. so things like raycasts and addforce should be in here or in their own functions outside of Update()

        // this is raycasting from the camera, and ignoring anything currently set to the layermask number we defined above. alternitvly we can invert the bit mask with  layerMask = ~layerMask
        // inverting it will make any object that is not on the layer number ignored in the raycast
        if (isUsingRaycastMethod && !isUsingDefaultClickMethod)
        {
            if (hasClicked)
            {
                hasClicked = false;

                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray.GetPoint(Mathf.Infinity), Camera.main.transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, layerMask)) ;
                {
                    // here after we know we clicked a object not assigned to the layermask number  we run two functions
                    HitTheVendingMachine();
                    ShootTheMouthLaser();

                }

            }
        }
    }



    void ShootTheMouthLaser()
    {
        // this function looks at the player controller we stored above and runs a function inside of the script that is set to public called ShootLaserMouth
        _pC.ShootLaserMouth();
    }

    void HitTheVendingMachine()
    {
            // spawning the partical system on the location of areaToCauseParticals
            GameObject objectForPartical = Instantiate(particalSystem, areaToCauseParticals.position, Quaternion.identity);

        // this is a for each loop, this loop is looking at our array "objectsToLaunch" and grabbing all the GameeObjects inside of it as  objectToSpawn
        foreach (GameObject objectToSpawn in objectsToLaunch)
        {
            //by having it check to see if there is a rigidbody or not on the object it prevents it from trying to launch a object that does not have on
            if (objectToSpawn.GetComponent<Rigidbody2D>() != null)
            {

                // here we take all the objects that have been assigned to objectToSpawn and spawn them into the game and assign them to a  privately stored GameObject called objectToLaunch
                GameObject objectToLaunch = Instantiate(objectToSpawn, areaToLaunchFrom.position, Quaternion.identity);

                // here we store the rigidbody2D off of the objectToLaunch GameObject
                Rigidbody2D _storedRig = objectToLaunch.GetComponent<Rigidbody2D>();

                //  we take the current posiition of the objectToLaunch and minus the transform of the object it will launch from ( in this case, this vending machine ) 
                Vector2 storedDistance = objectToLaunch.transform.position - transform.position;

                // if the length of storedDistance is more then 0
                if (storedDistance.magnitude > 0)
                {
                    // here we take the float from above forceOfExplosion and devide it by the length of the storedDistance and store that in a new private float called forceForExplosion
                    float forceForExplosion = forceOfExplosion / storedDistance.magnitude;
                    // now we grab the stored rigidbody  _storedRig and use AddForce()  on it adding a impulse, taking the normalized value of our stored distance and multipling it by the forceForExplosio 
                    _storedRig.AddForce(storedDistance.normalized * forceForExplosion, ForceMode2D.Impulse);

                }
                // setting the snacks to destroy after a while so objects do not pile up to much 
                Destroy(objectToLaunch, 2f);
            }

           
            
            // setting the particals that we spawned to become destroyed after 5 seconds
            Destroy(objectForPartical, 2f);
        }
            
            // updating the sprite we use for the vending machine with a new sprite showing the machine empty
            vendingMachineSpriteRenderer.sprite = spriteForEmptyMachine;

        // coroutines are ways to set timers and have to called with a StartCoroutine and then the name of the coroutine passed in
        StartCoroutine(ResetTheMachine());
    }

    // this is how you call a coroutine 
    // it must always return, its alos best to end the routine itself at some point otherwise it can cause problems if you call it again
    IEnumerator ResetTheMachine()
    {
        yield return new WaitForSeconds(2f);
        vendingMachineSpriteRenderer.sprite = storedSprite;
        hasClicked = false;
        yield break;
    }
}
