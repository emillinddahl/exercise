using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Microsoft.Azure.Kinect.BodyTracking;
using UnityEngine.UI;
using System.IO;

public class TrackerHandler : MonoBehaviour
{
    public Dictionary<JointId, JointId> parentJointMap;
    Dictionary<JointId, Quaternion> basisJointMap;
    private List<JointId> jointIds = new List<JointId>();
    public Quaternion[] absoluteJointRotations = new Quaternion[(int)JointId.Count];
    public bool drawSkeletons = true;
    [SerializeField] private bool applyFilter = true;
    Quaternion Y_180_FLIP = new Quaternion(0.0f, 1.0f, 0.0f, 0.0f);
    [SerializeField] private float minCutOff = 0.4f, derivateCutOff = 1.0f;
    private Filter filter; // = new Filter(0.4f, 0.4f);
    private Smoother Smoother;
    private SkeletonPosition Pos;
    //private SkeletonPosition Pos1;
    private bool hasHit = false;
    public Text text;
    public Text text1;
    public Text text2;
    public AudioClip audioClip;
    public AudioSource audioSource;
    public System.Numerics.Vector3 previousNosePos;
    public System.Numerics.Vector3 previousPelvisPos;
    public System.Numerics.Vector3 previousPelvisVelocity;
    public System.Numerics.Vector3 previousNoseVelocity;
    public float time = 0.0f;
    public Dictionary<float, float> data = new Dictionary<float, float>();
    


    // Start is called before the first frame update
    void Awake()
    {
        previousPelvisPos = System.Numerics.Vector3.Zero;
        previousNosePos = System.Numerics.Vector3.Zero;
        previousPelvisVelocity = System.Numerics.Vector3.Zero;
        previousNoseVelocity = System.Numerics.Vector3.Zero;
        
        parentJointMap = new Dictionary<JointId, JointId>();

        // pelvis has no parent so set to count
        parentJointMap[JointId.Pelvis] = JointId.Count;
        parentJointMap[JointId.SpineNavel] = JointId.Pelvis;
        parentJointMap[JointId.SpineChest] = JointId.SpineNavel;
        parentJointMap[JointId.Neck] = JointId.SpineChest;
        parentJointMap[JointId.ClavicleLeft] = JointId.SpineChest;
        parentJointMap[JointId.ShoulderLeft] = JointId.ClavicleLeft;
        parentJointMap[JointId.ElbowLeft] = JointId.ShoulderLeft;
        parentJointMap[JointId.WristLeft] = JointId.ElbowLeft;
        parentJointMap[JointId.HandLeft] = JointId.WristLeft;
        parentJointMap[JointId.HandTipLeft] = JointId.HandLeft;
        parentJointMap[JointId.ThumbLeft] = JointId.HandLeft;
        parentJointMap[JointId.ClavicleRight] = JointId.SpineChest;
        parentJointMap[JointId.ShoulderRight] = JointId.ClavicleRight;
        parentJointMap[JointId.ElbowRight] = JointId.ShoulderRight;
        parentJointMap[JointId.WristRight] = JointId.ElbowRight;
        parentJointMap[JointId.HandRight] = JointId.WristRight;
        parentJointMap[JointId.HandTipRight] = JointId.HandRight;
        parentJointMap[JointId.ThumbRight] = JointId.HandRight;
        parentJointMap[JointId.HipLeft] = JointId.SpineNavel;
        parentJointMap[JointId.KneeLeft] = JointId.HipLeft;
        parentJointMap[JointId.AnkleLeft] = JointId.KneeLeft;
        parentJointMap[JointId.FootLeft] = JointId.AnkleLeft;
        parentJointMap[JointId.HipRight] = JointId.SpineNavel;
        parentJointMap[JointId.KneeRight] = JointId.HipRight;
        parentJointMap[JointId.AnkleRight] = JointId.KneeRight;
        parentJointMap[JointId.FootRight] = JointId.AnkleRight;
        parentJointMap[JointId.Head] = JointId.Pelvis;
        parentJointMap[JointId.Nose] = JointId.Head;
        parentJointMap[JointId.EyeLeft] = JointId.Head;
        parentJointMap[JointId.EarLeft] = JointId.Head;
        parentJointMap[JointId.EyeRight] = JointId.Head;
        parentJointMap[JointId.EarRight] = JointId.Head;

        Vector3 zpositive = Vector3.forward;
        Vector3 xpositive = Vector3.right;
        Vector3 ypositive = Vector3.up;
        // spine and left hip are the same
        Quaternion leftHipBasis = Quaternion.LookRotation(xpositive, -zpositive);
        Quaternion spineHipBasis = Quaternion.LookRotation(xpositive, -zpositive);
        Quaternion rightHipBasis = Quaternion.LookRotation(xpositive, zpositive);
        // arms and thumbs share the same basis
        Quaternion leftArmBasis = Quaternion.LookRotation(ypositive, -zpositive);
        Quaternion rightArmBasis = Quaternion.LookRotation(-ypositive, zpositive);
        Quaternion leftHandBasis = Quaternion.LookRotation(-zpositive, -ypositive);
        Quaternion rightHandBasis = Quaternion.identity;
        Quaternion leftFootBasis = Quaternion.LookRotation(xpositive, ypositive);
        Quaternion rightFootBasis = Quaternion.LookRotation(xpositive, -ypositive);

        basisJointMap = new Dictionary<JointId, Quaternion>();

        // pelvis has no parent so set to count
        basisJointMap[JointId.Pelvis] = spineHipBasis;
        basisJointMap[JointId.SpineNavel] = spineHipBasis;
        basisJointMap[JointId.SpineChest] = spineHipBasis;
        basisJointMap[JointId.Neck] = spineHipBasis;
        basisJointMap[JointId.ClavicleLeft] = leftArmBasis;
        basisJointMap[JointId.ShoulderLeft] = leftArmBasis;
        basisJointMap[JointId.ElbowLeft] = leftArmBasis;
        basisJointMap[JointId.WristLeft] = leftHandBasis;
        basisJointMap[JointId.HandLeft] = leftHandBasis;
        basisJointMap[JointId.HandTipLeft] = leftHandBasis;
        basisJointMap[JointId.ThumbLeft] = leftArmBasis;
        basisJointMap[JointId.ClavicleRight] = rightArmBasis;
        basisJointMap[JointId.ShoulderRight] = rightArmBasis;
        basisJointMap[JointId.ElbowRight] = rightArmBasis;
        basisJointMap[JointId.WristRight] = rightHandBasis;
        basisJointMap[JointId.HandRight] = rightHandBasis;
        basisJointMap[JointId.HandTipRight] = rightHandBasis;
        basisJointMap[JointId.ThumbRight] = rightArmBasis;
        basisJointMap[JointId.HipLeft] = leftHipBasis;
        basisJointMap[JointId.KneeLeft] = leftHipBasis;
        basisJointMap[JointId.AnkleLeft] = leftHipBasis;
        basisJointMap[JointId.FootLeft] = leftFootBasis;
        basisJointMap[JointId.HipRight] = rightHipBasis;
        basisJointMap[JointId.KneeRight] = rightHipBasis;
        basisJointMap[JointId.AnkleRight] = rightHipBasis;
        basisJointMap[JointId.FootRight] = rightFootBasis;
        basisJointMap[JointId.Head] = spineHipBasis;
        basisJointMap[JointId.Nose] = spineHipBasis;
        basisJointMap[JointId.EyeLeft] = spineHipBasis;
        basisJointMap[JointId.EarLeft] = spineHipBasis;
        basisJointMap[JointId.EyeRight] = spineHipBasis;
        basisJointMap[JointId.EarRight] = spineHipBasis;
        
        // add joint ids to list from dictionary
        jointIds = parentJointMap.Keys.ToList();

        //initialise smoother
        Smoother = new Smoother();
        Pos = new SkeletonPosition();
        //initialise skeleton position

        audioSource = GetComponent<AudioSource>();
    }

    public void updateTracker(BackgroundData trackerFrameData)
    {
        //this is an array in case you want to get the n closest bodies
        int closestBody = findClosestTrackedBody(trackerFrameData);

        // render the closest body
        Body skeleton = trackerFrameData.Bodies[closestBody];
        
        
      
        var jointPositions = new List<Vector3>();

        for (var i = 0; i < skeleton.JointPositions3D.Length; i++)
        {
            var s = skeleton.JointPositions3D[i];
            var x = s.X;
            var y = s.Y;
            var z = s.Z;
            var jointPos = new Vector3(x, y, z);
            jointPositions.Add(jointPos);
            // print(s);
            // print the joint from index and appripriate jointId index
          //  print("joint " + i + " " + (JointId)i + " " + s.X + " " + s.Y + " " + s.Z + " ");
        }

        if (applyFilter)
        {
            //assign filtered values to JointPositions3D
            // filter = new Filter(minCutOff, derivateCutOff);
            // filter.InitializeTimestamp();
            // skeleton.JointPositions3D = filter.DoFilter(skeleton.JointPositions3D);
            //Smoother.NumberSmoothingFrames = 3;
            Pos = Smoother.ReceiveNewSensorData(new SkeletonPosition(skeleton, jointIds, Vector3.zero), true);
            
            // loop through all the joints and assign the filtered values to the skeleton
            for (int i = 0; i < (int)JointId.Count; i++)
            {
                var jointPos = Pos.GetJointPosition((JointId)i);
                skeleton.JointPositions3D[i].X = jointPos.x;
                skeleton.JointPositions3D[i].Y = -jointPos.y;
                skeleton.JointPositions3D[i].Z = jointPos.z;
                
            }
            
        }
        //this part of the code will calculate if there is a specific amount of length between the head and the right hand
        //is that the case a message will be displayed. "Great Job!!"
        
        // get pelvis joint position
        var pelvisPos = Pos.GetJointPosition(JointId.Pelvis);
        //get filtered pelvis joint position
        var pelvisPos2 = skeleton.JointPositions3D[(int)JointId.Pelvis];
        
        // get right hand joint position
        var rightHandPos = Pos.GetJointPosition(JointId.HandRight);
        var headVsRightHand = pelvisPos.x - rightHandPos.x;
        
        // print the head position minus the right hand position in the x axis
        // print("headPos - rightHandPos " + (headVsRightHand));
        bool isWithinProximity = headVsRightHand < -0.20f && headVsRightHand > -0.60f;
        
        // if the right hand position x axis vs the head position is less than 2
        if (isWithinProximity && !hasHit)
        {
            print("iisWithinProximity + ");
            text.text = "Great Job!!";
        
          //  audioSource.PlayOneShot(audioSource.clip);
            //isWithinProximity = true;
            hasHit = true;
        }
        else if (!isWithinProximity && hasHit)
        {
            isWithinProximity = false;
            hasHit = false;
            print("Reset!!");
            text.text = "";
        }
        
        
        //the following code tries to calculate the velocity and acceleration of the pelvis and the nose. 
        time = Time.time;
        //get current positions
       // var currentNosePos = Pos.GetJointPosition(JointId.Nose);
       // var currentPelvisPos = Pos.GetJointPosition(JointId.Pelvis);
        
        
        var currentNosePos = skeleton.JointPositions3D[(int)JointId.Nose];
        var currentPelvisPos = skeleton.JointPositions3D[(int)JointId.Pelvis];
        
        //calculate the velocity of the pelvis
        var pelvisVelocity = (currentPelvisPos - previousPelvisPos) / Time.deltaTime;
        //calculate the velocity of the nose
        var noseVelocity = (currentNosePos - previousNosePos) / Time.deltaTime;

        var pelvisacc = (currentPelvisPos - previousPelvisPos) / Time.deltaTime / Time.deltaTime;
        
        //update the previous pelvis position
        previousPelvisPos = currentPelvisPos;
        previousNosePos = currentNosePos;
        
        //print out the velocity of pelvis and nose
        //print("Pelvis Velocity: " + pelvisVelocity);
        //print("Nose Velocity: " + noseVelocity);
        text.text = "Pelvis Velocity: " + pelvisVelocity.Length();
        text1.text = "Nose Velocity: " + noseVelocity.Length();
        text2.text = "Pelvis Acc: " + pelvisacc.Length();
     
        //calculate the acceleration of the pelvis and the nose
        var pelvisAcceleration = (pelvisVelocity - previousPelvisVelocity) / Time.deltaTime;
        var noseAcceleration = (noseVelocity - previousNoseVelocity) / Time.deltaTime;
        
        previousPelvisVelocity = pelvisVelocity;
        previousNoseVelocity = noseVelocity;
        
        //print out the acceleration of the pelvis and the nose
        print("Pelvis Acceleration: " + pelvisAcceleration.Length());
      //  print("Nose Acceleration: " + noseAcceleration);
        print("other Pelvisacc: " + pelvisacc);
        print("Pelvis acceleration: " + pelvisacc.Length());
        
        data.Add(time, pelvisVelocity.Length());
       
        
        
        /*
        //what should be printed into the file?
        //float time
        //float pelvisVelocity.magnitude?
        //float noseVelocity
        //float pelvisAcceleration
        //float noseAcceleration
        string filename = Application.dataPath + "/data.csv";
        TextWriter tw = new StreamWriter(filename, false);
        tw.WriteLine("Time, Pelvis Velocity, Nose Velocity, Pelvis Acceleration, Nose Acceleration");
        tw.Close();
        */
       
        //render skeleton
        renderSkeleton(skeleton, 0);
    }
    //how to get the data from the dictionary into the csv file?
    
    void OnApplicationQuit()
    {
        string filename = Application.dataPath + "/data.csv";
        TextWriter tw = new StreamWriter(filename, false);
        tw.WriteLine("Time, Pelvis Velocity");
        foreach (KeyValuePair<float, float> entry in data)
        {
            tw.WriteLine(entry.Key + "," + entry.Value);
        }
        tw.Close();
    }

    int findIndexFromId(BackgroundData frameData, int id)
    {
        int retIndex = -1;
        for (int i = 0; i < (int)frameData.NumOfBodies; i++)
        {
            if ((int)frameData.Bodies[i].Id == id)
            {
                retIndex = i;
                break;
            }
        }

        return retIndex;
    }

    private int findClosestTrackedBody(BackgroundData trackerFrameData)
    {
        int closestBody = -1;
        const float MAX_DISTANCE = 5000.0f;
        float minDistanceFromKinect = MAX_DISTANCE;
        for (int i = 0; i < (int)trackerFrameData.NumOfBodies; i++)
        {
            var pelvisPosition = trackerFrameData.Bodies[i].JointPositions3D[(int)JointId.Pelvis];
            Vector3 pelvisPos = new Vector3((float)pelvisPosition.X, (float)pelvisPosition.Y, (float)pelvisPosition.Z);
            if (pelvisPos.magnitude < minDistanceFromKinect)
            {
                closestBody = i;
                minDistanceFromKinect = pelvisPos.magnitude;
            }
        }

        return closestBody;
    }

    public void turnOnOffSkeletons()
    {
        drawSkeletons = !drawSkeletons;
        const int bodyRenderedNum = 0;
        for (int jointNum = 0; jointNum < (int)JointId.Count; jointNum++)
        {
            transform.GetChild(bodyRenderedNum).GetChild(jointNum).gameObject.GetComponent<MeshRenderer>().enabled =
                drawSkeletons;
            transform.GetChild(bodyRenderedNum).GetChild(jointNum).GetChild(0).GetComponent<MeshRenderer>().enabled =
                drawSkeletons;
        }
    }

    public void renderSkeleton(Body skeleton, int skeletonNumber)
    {
        for (int jointNum = 0; jointNum < (int)JointId.Count; jointNum++)
        {
            Vector3 jointPos = new Vector3(skeleton.JointPositions3D[jointNum].X,
                -skeleton.JointPositions3D[jointNum].Y, skeleton.JointPositions3D[jointNum].Z);
            Vector3 offsetPosition = transform.rotation * jointPos;
            Vector3 positionInTrackerRootSpace = transform.position + offsetPosition;
            Quaternion jointRot = Y_180_FLIP * new Quaternion(skeleton.JointRotations[jointNum].X,
                                      skeleton.JointRotations[jointNum].Y,
                                      skeleton.JointRotations[jointNum].Z, skeleton.JointRotations[jointNum].W) *
                                  Quaternion.Inverse(basisJointMap[(JointId)jointNum]);
            absoluteJointRotations[jointNum] = jointRot;
            // these are absolute body space because each joint has the body root for a parent in the scene graph
            transform.GetChild(skeletonNumber).GetChild(jointNum).localPosition = jointPos;
            transform.GetChild(skeletonNumber).GetChild(jointNum).localRotation = jointRot;

            const int boneChildNum = 0;
            if (parentJointMap[(JointId)jointNum] != JointId.Head && parentJointMap[(JointId)jointNum] != JointId.Count)
            {
                Vector3 parentTrackerSpacePosition = new Vector3(
                    skeleton.JointPositions3D[(int)parentJointMap[(JointId)jointNum]].X,
                    -skeleton.JointPositions3D[(int)parentJointMap[(JointId)jointNum]].Y,
                    skeleton.JointPositions3D[(int)parentJointMap[(JointId)jointNum]].Z);
                Vector3 boneDirectionTrackerSpace = jointPos - parentTrackerSpacePosition;
                Vector3 boneDirectionWorldSpace = transform.rotation * boneDirectionTrackerSpace;
                Vector3 boneDirectionLocalSpace =
                    Quaternion.Inverse(transform.GetChild(skeletonNumber).GetChild(jointNum).rotation) *
                    Vector3.Normalize(boneDirectionWorldSpace);
                transform.GetChild(skeletonNumber).GetChild(jointNum).GetChild(boneChildNum).localScale =
                    new Vector3(1, 20.0f * 0.5f * boneDirectionWorldSpace.magnitude, 1);
                transform.GetChild(skeletonNumber).GetChild(jointNum).GetChild(boneChildNum).localRotation =
                    Quaternion.FromToRotation(Vector3.up, boneDirectionLocalSpace);
                transform.GetChild(skeletonNumber).GetChild(jointNum).GetChild(boneChildNum).position =
                    transform.GetChild(skeletonNumber).GetChild(jointNum).position - 0.5f * boneDirectionWorldSpace;
            }
            else
            {
                transform.GetChild(skeletonNumber).GetChild(jointNum).GetChild(boneChildNum).gameObject
                    .SetActive(false);
            }
        }
    }

    public Quaternion GetRelativeJointRotation(JointId jointId)
    {
        JointId parent = parentJointMap[jointId];
        Quaternion parentJointRotationBodySpace = Quaternion.identity;
        if (parent == JointId.Count)
        {
            parentJointRotationBodySpace = Y_180_FLIP;
        }
        else
        {
            parentJointRotationBodySpace = absoluteJointRotations[(int)parent];
        }

        Quaternion jointRotationBodySpace = absoluteJointRotations[(int)jointId];
        Quaternion relativeRotation = Quaternion.Inverse(parentJointRotationBodySpace) * jointRotationBodySpace;

        return relativeRotation;
    }
}