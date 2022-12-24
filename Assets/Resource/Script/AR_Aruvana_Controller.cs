using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class AR_Aruvana_Controller : MonoBehaviour
{
    // Start is called before the first frame update
    public Animation anim,anime;
    private Vector2 initialDistance;
    private float initialDistances;
    private Vector3 initialScale;

    public GameObject objectImRotating;
    public Button[] btn;

    [SerializeField]
    private Renderer threeDrdObject;
    public float rotatespeed = 10f;
    private float startingPosition;
    private float turnspeed = 10;

    [SerializeField]
    private AudioSource audioEnter,audioDisapear;
    [SerializeField]
    ARTrackedImageManager m_TrackedImageManager;

    void OnEnable() => m_TrackedImageManager.trackedImagesChanged += OnChanged;
    void OnDisable() => m_TrackedImageManager.trackedImagesChanged -= OnChanged;
    // Update is called once per frame
    private void Start()
    {
        anim = gameObject.GetComponent<Animation>();
        anime = gameObject.GetComponent<Animation>();
    }
    IEnumerator BlinkingEffect()
    {
       
        threeDrdObject.enabled = false;
        yield return new WaitForSeconds(0.2f);
        threeDrdObject.enabled = true;
    }
    public void blinkingPress()
    {
        StartCoroutine(BlinkingEffect());
        InvokeRepeating("BlinkingEffect", 4.0f, 2.0f);
        
    }
    IEnumerator ButtonShow()
    {
        for (int i = 0; i <= 3; i++)
        {
            yield return new WaitForSeconds(6.0f);
            btn[i].gameObject.SetActive(true);
        }
    }
    IEnumerator ButtonHide()
    {
        for (int i = 0; i <= 3; i++)
        {
            yield return new WaitForSeconds(1.0f);
            btn[i].gameObject.SetActive(false);
        }
    }
    void OnChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (var newImage in eventArgs.added)
        {
            // Handle added event
            audioEnter.Play();
        }

        foreach (var updatedImage in eventArgs.updated)
        {
            // Handle updated event
        }

        foreach (var removedImage in eventArgs.removed)
        {
            // Handle removed event
            audioDisapear.Play();
        }
    }
    private void OnTrackedImageChanged(ARTrackedImagesChangedEventArgs eventArges)
    {
        for (int i = 0; i < eventArges.added.Count; i++)
        {
            //eventArges.added[i].referenceImage.name;
            // use that to get the gname
        }
    }
    void Update()
    {
        
        if (Input.touchCount == 2)
        {
            var touchZero = Input.GetTouch(0);
            var touchOne = Input.GetTouch(1);

            // if one of the touches Ended or Canceled do nothing
            if (touchZero.phase == TouchPhase.Ended || touchZero.phase == TouchPhase.Canceled
               || touchOne.phase == TouchPhase.Ended || touchOne.phase == TouchPhase.Canceled)
            {
                return;
            }

            // It is enough to check whether one of them began since we
            // already excluded the Ended and Canceled phase in the line before
            if (touchZero.phase == TouchPhase.Began || touchOne.phase == TouchPhase.Began)
            {
                // track the initial values
                initialDistances = Vector2.Distance(touchZero.position, touchOne.position);
                initialScale = objectImRotating.transform.localScale;
            }
            // else now is any other case where touchZero and/or touchOne are in one of the states
            // of Stationary or Moved
            else
            {
                // otherwise get the current distance
                var currentDistance = Vector2.Distance(touchZero.position, touchOne.position);

                // A little emergency brake ;)
                if (Mathf.Approximately(initialDistances, 0)) return;

                // get the scale factor of the current distance relative to the inital one
                var factor = currentDistance / initialDistances;

                // apply the scale
                // instead of a continuous addition rather always base the 
                // calculation on the initial and current value only
                objectImRotating.transform.localScale = initialScale * factor;
            }
        }
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    startingPosition = touch.position.x;
                    break;
                case TouchPhase.Moved:
                    if (startingPosition > touch.position.x)
                    {
                        objectImRotating.transform.Rotate(Vector3.back, -turnspeed * Time.deltaTime);
                        anim.Stop("RotateAnim");
                    }
                    else if (startingPosition < touch.position.x)
                    {
                        
                        objectImRotating.transform.Rotate(Vector3.back, rotatespeed * Time.deltaTime);
                        anim.Play("RotateAnim");
                    }
                    break;
                case TouchPhase.Ended:
                    Debug.Log("Touch Phase Ended.");
                    break;
            }
        }
    }

    IEnumerator AndroidSaveScreenshot()
    {
        StartCoroutine(ButtonHide());
        yield return new WaitForEndOfFrame();
        // string TwoStepScreenshotPath = MobileNativeShare.SaveScreenshot("Screenshot" + System.DateTime.Now.Hour + System.DateTime.Now.Minute + System.DateTime.Now.Second);
        // Debug.Log("A new screenshot was saved at " + TwoStepScreenshotPath);

        string myFileName = "Screenshot" + System.DateTime.Now.Hour + System.DateTime.Now.Minute + System.DateTime.Now.Second + ".png";
        string myDefaultLocation = Application.persistentDataPath + "/" + myFileName;
        string myFolderLocation = "/storage/emulated/0/DCIM/Camera/JCB/";  //EXAMPLE OF DIRECTLY ACCESSING A CUSTOM FOLDER OF THE GALLERY
        string myScreenshotLocation = myFolderLocation + myFileName;

        //ENSURE THAT FOLDER LOCATION EXISTS
        if (!System.IO.Directory.Exists(myFolderLocation))
        {
            System.IO.Directory.CreateDirectory(myFolderLocation);
        }

        ScreenCapture.CaptureScreenshot(myFileName);
        //MOVE THE SCREENSHOT WHERE WE WANT IT TO BE STORED

        yield return new WaitForSeconds(1);

        System.IO.File.Move(myDefaultLocation, myScreenshotLocation);

        //REFRESHING THE ANDROID PHONE PHOTO GALLERY IS BEGUN
        AndroidJavaClass classPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject objActivity = classPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaClass classUri = new AndroidJavaClass("android.net.Uri");
        AndroidJavaObject objIntent = new AndroidJavaObject("android.content.Intent", new object[2] { "android.intent.action.MEDIA_MOUNTED", classUri.CallStatic<AndroidJavaObject>("parse", "file://" + myScreenshotLocation) });
        objActivity.Call("sendBroadcast", objIntent);
        StartCoroutine(ButtonShow());


    }
    public void clickShare()
    {
        StartCoroutine(TakeSSAndShare());
    }

    private IEnumerator TakeSSAndShare()
    {
        StartCoroutine(ButtonHide());
        yield return new WaitForEndOfFrame();

        Texture2D ss = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        ss.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        ss.Apply();

        string filePath = Path.Combine(Application.temporaryCachePath, "shared img.png");
        File.WriteAllBytes(filePath, ss.EncodeToPNG());

        // To avoid memory leaks
        Destroy(ss);

        new NativeShare().AddFile(filePath).SetSubject("Aruvana AR").SetText("asep jumadi aruvana AR Test!").Share();
        StartCoroutine(ButtonShow());
    }
    public void TakeScreenShootAndShare()
    {
        StartCoroutine(AndroidSaveScreenshot());
    }
}
