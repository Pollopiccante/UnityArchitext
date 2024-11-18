using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.VFX;

public enum ReaderControllerState
{
    Idle = 0,
    StartingToRead = 1,
    StoppingToRead = 2,
}

public class DistanceBasedReaderController : MonoBehaviour
{
    // mandatory connections
    public VisualEffect effect;
    public Transform leftLineTransform;
    public Transform rightLineTransform;

    // parameters
    public float showHighlighterDistance = 40f;
    public float reading = 0f; // (0..1) (not reading..reading)
    public ReaderControllerState controllerState = ReaderControllerState.Idle;
    
    [CanBeNull] private GameObject highlightSphere = null; 
    [CanBeNull] private Vector3[] indexToPosMap;
    private int lastMinDistIndex = -1;
    private bool showHighlightSphere = true;
        
    [CanBeNull] private IndexStepper _indexStepper;
    [CanBeNull] private ReaderStepper _readerStepper;
    private void Start()
    {
        if (effect != null)
        {
            SetEffect(effect);
        }
    }

    public void SetEffect(VisualEffect effect)
    {
        this.effect = effect;
        _indexStepper = effect.GetComponent<IndexStepper>();
        _readerStepper = effect.GetComponent<ReaderStepper>();
        
        // save final positions of effect
        indexToPosMap = VFXDataScriptableObject.GetTextureAsVectors((Texture2D) effect.GetTexture("Positions"), effect);
        
        effect.SetFloat("SnapToCameraLinePercentage", reading);
    }

    private void HighlightIndex(int index)
    {
        // create new highlight sphere in case there is none
        if (highlightSphere == null)
        {
            highlightSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            highlightSphere.name = "HighlightSphere";
            highlightSphere.transform.localScale = new Vector3(2, 2, 2);
        }
        
        // set sphere position
        if (indexToPosMap != null)
            highlightSphere.transform.position = indexToPosMap[index];
    }

    private void RemoveHighLight()
    {
        Destroy(highlightSphere);
        highlightSphere = null;
    }
    
    private void UpdateHighlightSphere()
    {
        if (!showHighlightSphere)
        {
            if (highlightSphere != null)
                Destroy(highlightSphere);
            highlightSphere = null;
            return;
        }
        
        if (_indexStepper == null || indexToPosMap == null)
            return;
        
        // get current time index
        int indexTime = Mathf.FloorToInt(_indexStepper.currentIndexTime);
        
        // iterate all indices before current to check distances
        Vector3 ownPosition = gameObject.transform.position;
        float minDist = Mathf.Infinity;
        lastMinDistIndex = -1;
        for (int i = 0; i <= indexTime; i++)
        {
            // calc distance
            float dist = Vector3.Distance(ownPosition, this.indexToPosMap[i]);
            if (dist < minDist)
            {
                minDist = dist;
                lastMinDistIndex = i;
            }
        }
        
        // highlight min dist point
        if (lastMinDistIndex > 0 && minDist <= showHighlighterDistance)
            HighlightIndex(lastMinDistIndex);
        else
            RemoveHighLight();
    }

    public void StartReading()
    {
        if (lastMinDistIndex < 0 || IsReading() || _readerStepper == null)
            return;

        controllerState = ReaderControllerState.StartingToRead;
        _readerStepper.currentReadingTime = lastMinDistIndex;
        showHighlightSphere = false;
    }

    public void StopReading()
    {
        if (IsNotReading())
            return;
        
        controllerState = ReaderControllerState.StoppingToRead;
        showHighlightSphere = true;
    }

    private bool IsReading()
    {
        return Mathf.Abs(reading - 1f) < 0.001f;
    }

    private bool IsNotReading()
    {
        return Mathf.Abs(reading) < 0.001f;
    }
    
    private void UpdateReading()
    {
        if (controllerState == ReaderControllerState.Idle)
            return;
        if (controllerState == ReaderControllerState.StartingToRead)
        {
            reading = Mathf.Lerp(reading, 1f, 0.3f);
            effect.SetFloat("SnapToCameraLinePercentage", reading);
            if (IsReading())
                controllerState = ReaderControllerState.Idle;
        }
        if (controllerState == ReaderControllerState.StoppingToRead)
        {
            reading = Mathf.Lerp(reading, 0f, 0.3f);
            effect.SetFloat("SnapToCameraLinePercentage", reading);
            if (IsNotReading())
                controllerState = ReaderControllerState.Idle;
        }
    }
    
    private void FocusReaderOnCamera()
    {
        if (_readerStepper == null)
            return;
        
        _readerStepper.UpdateReaderPosition(
            leftLineTransform.position,
            rightLineTransform.position,
            leftLineTransform.rotation,
            rightLineTransform.rotation
            );
    }

    private void CheckClicks()
    {
        /*RaycastHit hit;
        if(clickedLeft && Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, 1000.0f))
        {
            Debug.Log("Clicked on gameobject: " +  hit.collider.name);
            if (highlightSphere != null && hit.collider.name.Equals(highlightSphere.name))
            {
                Debug.Log("STARTREADING");
                StartReading();
            }
        }*/
        bool clickedLeft = Input.GetMouseButtonDown(0);
        bool clickedRight = Input.GetMouseButtonDown(1);
        if (clickedRight)
        {
            StopReading();
        }

        if (clickedLeft)
        {
            StartReading();
        }
    }

    public void Reset()
    {
        if (_readerStepper == null)
            return;
        
        _readerStepper.currentReadingTime = 0;
    }
    
    void Update()
    {
        FocusReaderOnCamera();
        UpdateHighlightSphere();
        UpdateReading();
        CheckClicks();
    }
}
