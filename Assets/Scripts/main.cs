using UnityEngine;
using System.Collections.Generic;


public class main : MonoBehaviour
{
    // Handler for SkeletalTracking thread.
    public GameObject m_tracker;
    private SkeletalTrackingProvider m_skeletalTrackingProvider;
    public BackgroundData m_lastFrameData = new BackgroundData();
  // public List<SkeletonPosition> rawData = new List<SkeletonPosition>();
    //public Smoother smoother = new Smoother();
    public float proximityThreshold = 1f;
    public Transform cube;
    public PuppetAvatar puppetAvatar;
    bool hasHit = false;
    bool isWithinProximity = false;
  

    void Start()
    {
        //tracker ids needed for when there are two trackers
        const int TRACKER_ID = 0;
        m_skeletalTrackingProvider = new SkeletalTrackingProvider(TRACKER_ID);
        print("This is seen in the console window.");
        puppetAvatar = GetComponent<PuppetAvatar>();

    }

    void Update()
    {
        if (m_skeletalTrackingProvider.IsRunning)
        {
            if (m_skeletalTrackingProvider.GetCurrentFrameData(ref m_lastFrameData))
            {
                if (m_lastFrameData.NumOfBodies != 0)
                {
                    m_tracker.GetComponent<TrackerHandler>().updateTracker(m_lastFrameData);
                   
                    //convert numeric vector to unity vector
                    UnityEngine.Vector3 unityHandVector = new UnityEngine.Vector3(m_lastFrameData.Bodies[0].JointPositions3D[11].X, m_lastFrameData.Bodies[0].JointPositions3D[11].Y, m_lastFrameData.Bodies[0].JointPositions3D[11].Z);
                   //calculate distance between hand and cube 
                    float distance = Vector3.Distance(unityHandVector, cube.position);
                    
                    //puppetAvatar.CharacterRootTransform.position = leftHand;
                  //  bool isWithinProximity = distance < proximityThreshold;
                    
                    //set isWithinProximity to true if the distance is less than the proximity threshold
                    //set isWithinProximity to false if the distance is greater than the proximity threshold
               /*     if (distance < proximityThreshold)
                    {
                        isWithinProximity = true;
                        print("WITHIN RANGE");
                    }
                    else
                    {
                        isWithinProximity = false;
                        print("Outside RANGE");
                    }
                    
                    if (isWithinProximity && !hasHit )
                    {
                        print("Exercise 1 YAASSSSS isWithinProximity + " + isWithinProximity);
                        hasHit = true;
                    }
                    else if (!isWithinProximity && hasHit)
                    {
                        hasHit = false;
                        print("Reset!!");
                    }
                    */
                    
                }
            }
        }
     //   print(rawData);
       
    }

    void OnApplicationQuit()
    {
        if (m_skeletalTrackingProvider != null)
        {
            m_skeletalTrackingProvider.Dispose();
        }
    }
}
