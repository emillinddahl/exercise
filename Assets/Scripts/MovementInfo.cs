using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.Azure.Kinect.BodyTracking;
using UnityEngine.UI;

public class MovementInfo : MonoBehaviour
{
    
    private Smoother Smoother1;
    private SkeletonPosition Pos1;
    private Vector3 previousPelvisPos;
    private Vector3 previousNosePos;
    private Vector3 previousLeftHandPos;
    private float currentTime;
    private float previousTime;
    // Start is called before the first frame update
    void Start()
    {
        Smoother1 = new Smoother();
        Pos1 = new SkeletonPosition();
        previousNosePos = Pos1.GetJointPosition(JointId.Pelvis);
        previousPelvisPos = Pos1.GetJointPosition(JointId.Nose);
        previousLeftHandPos = Pos1.GetJointPosition(JointId.HandLeft);
        currentTime = Time.deltaTime;
        //float currentTimee = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        var currentPelvisPos = Pos1.GetJointPosition(JointId.Pelvis);
        var currentNosePos = Pos1.GetJointPosition(JointId.Nose);
        print(currentPelvisPos);
        print(currentNosePos);
        
        //calculate the velocity of the pelvis
        var pelvisVelocity = (currentPelvisPos - previousPelvisPos) / currentTime;
        //calculate the velocity of the nose
        var noseVelocity = (currentNosePos - previousNosePos) / currentTime;
        
        //calculate the acceleration of the pelvis
        var pelvisAcceleration = (pelvisVelocity - previousPelvisPos) / currentTime;
        //calculate the acceleration of the nose
        var noseAcceleration = (noseVelocity - previousNosePos) / currentTime;
        
        //update the previous pelvis position
        previousPelvisPos = currentPelvisPos;
        //update the previous nose position
        previousNosePos = currentNosePos;
        //update the previous time
        previousTime = currentTime;
        
        
        
        //print out the velocity and acceleration of the pelvis
        print("Pelvis Velocity: " + pelvisVelocity);
        print("Pelvis Acceleration: " + pelvisAcceleration);
        
        //print out the velocity and acceleration of the nose
        print("Nose Velocity: " + noseVelocity);
        print("Nose Acceleration: " + noseAcceleration);
        
    }
    
    
}
