using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;
using Leap;


public class InteractableCollision : MonoBehaviour {

    // Variable Declarations
    LeapProvider provider;

    Frame handFrame;
    Transform currTransform;

    public AudioSource audio;

    public float collisionRefreshTimer = 1.0f;

    private Arduino_Access arduino;

    //***********************************
    // Haptic Feedback
    private float collisionRefreshInterval = 0.0001F;

    // Finger Stimulation
    //float ballRadius = 0.0014F; // fopr 0.1
    //float ballRadius = 0.003F; //0.05
    float ballRadius = 0.0023F;

    float ballSurfaceRoughness = 0;

    private float[,] fingerSegmentStimStatus = new float[5, 4] { {0,0,0,0}, { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 } };
    private bool[,] motorConstraintStatus = new bool[5, 4] { { false, false, false, false }, { false, false, false, false }, { false, false, false, false }, { false, false, false, false }, { false, false, false, false } };

    // for computational optimization, when the ball is far from the hand, do not do custom collision detection
    bool objectWanderedAway = false;

    // Tracking object position for BallGenerator
    BallGenerator ballGenerator;
    Vector3 initialPosition;
    int altarIndex = -1;
    bool nextBallGenerated = false;

    // initialization
    void Start()
    {
        //AddComponents
        arduino = gameObject.AddComponent<Arduino_Access>() as Arduino_Access;

        // Get Components
        provider = FindObjectOfType<LeapProvider>() as LeapProvider;
        initialPosition = gameObject.transform.position;

        currTransform = GetComponent<Transform>();

        GameObject ballGeneratorObject = GameObject.Find("LightBall Generator");
        ballGenerator = ballGeneratorObject.GetComponent<BallGenerator>();

    }


    // Update is called once per frame
    void Update()
    {

        // If the ball rolled away, do not do any extra collision detections
        if (objectWanderedAway)
        {
            return;
        }

        //*****************************************
        // Detect object's collision with user's fingers
        handFrame = provider.CurrentFrame;

        collisionRefreshTimer -= Time.deltaTime;
        if (collisionRefreshTimer <= 0)
        {
            collisionRefreshTimer = collisionRefreshInterval;

            foreach (Hand hand in handFrame.Hands)
            {

                foreach (Finger finger in hand.Fingers)
                {
                    Bone bone0 = finger.Bone(Bone.BoneType.TYPE_DISTAL);
                    Bone bone1 = finger.Bone(Bone.BoneType.TYPE_INTERMEDIATE);
                    Bone bone2 = finger.Bone(Bone.BoneType.TYPE_PROXIMAL);
                    Bone bone3 = finger.Bone(Bone.BoneType.TYPE_METACARPAL);

                    Vector3 bone0Center = bone0.Center.ToVector3();
                    Vector3 bone1Center = new Vector3(0, 0, 0);
                    if (finger.Type != Finger.FingerType.TYPE_THUMB)
                    {
                        bone1Center = bone1.Center.ToVector3();
                    }
                    Vector3 bone2Center = bone2.Center.ToVector3();
                    Vector3 bone3Center = bone3.Center.ToVector3();

                    float[] distToBones = new float[4] { 100, 100, 100, 100 };
                    distToBones[0] = (bone0Center - currTransform.position).sqrMagnitude;
                    distToBones[1] = (bone1Center - currTransform.position).sqrMagnitude;
                    distToBones[2] = (bone2Center - currTransform.position).sqrMagnitude;
                    distToBones[3] = (bone3Center - currTransform.position).sqrMagnitude;

                    Finger.FingerType type = finger.Type;
                    int fingerIndex = ConvertFingerTypeToIndex(type);

                    for (int i = 0; i < distToBones.Length; ++i)
                    {
                        if (distToBones[i] < ballRadius)
                        {
                            float depthPercentage = (ballRadius - distToBones[i]) / ballRadius;
                            StimulateFingerSegment(fingerIndex, i, 1, depthPercentage);
                            MotorConstraint(fingerIndex, i, true, depthPercentage);

                        }
                        else
                        {
                            EndStimulationForFingerSegment(fingerIndex, i);
                            EndMotorConstraint(fingerIndex, i);
                        }
                    }
                }
            }
        }

        //*****************************************
        // Generating new ball
        //Detecting changes in object's position

        float movedDistance = Vector3.Distance(currTransform.position, initialPosition);
        if (movedDistance > 1.0F && !nextBallGenerated)
        {
            ballGenerator.GenerateNewBall(initialPosition);
            nextBallGenerated = true;
        }
        if (movedDistance > 5.0F)
        {
            objectWanderedAway = true;
        }

    }

    //Send message to actuator
    private void StimulateFingerSegment(int fingerIndex, int fingerSegmentIndex, float intensity, float depthPercentage)
    {
        if (fingerSegmentStimStatus[fingerIndex, fingerSegmentIndex] == intensity)
        {
            // Have already set, no need to send message again
            return;
        }

        fingerSegmentStimStatus[fingerIndex, fingerSegmentIndex] = intensity;

        //Send message to serial communication class

    }

    
    private void EndStimulationForFingerSegment(int fingerIndex, int fingerSegmentIndex)
    {
        StimulateFingerSegment(fingerIndex, fingerSegmentIndex, 0.0F, 0.0F);
        arduino.CloseConnection();
    }


    private void MotorConstraint(int fingerIndex, int fingerSegmentIndex, bool constrained, float depthPercentage)
    {
        if (motorConstraintStatus[fingerIndex, fingerSegmentIndex] == constrained)
        {
            // Have already set, no need to send message again
            return;
        }

        audio.Play();
        //print("Motor Constrained");

        motorConstraintStatus[fingerIndex, fingerSegmentIndex] = constrained;

        //Send message to serial communication class
        arduino.Motor_Write(fingerIndex, constrained);
    }


    private void EndMotorConstraint(int fingerIndex, int fingerSegmentIndex)
    {
        MotorConstraint(fingerIndex, fingerSegmentIndex, false, 0.0F);
        arduino.CloseConnection();
    }


    //Convenience Functions
    private HandModel GetHand(Collider other)
    {
        if (other.transform.parent && other.transform.parent.parent.parent &&
            other.transform.parent.parent.GetComponent<HandModel>())
            return other.transform.parent.parent.GetComponent<HandModel>();
        else
            return null;
    }


    private int ConvertFingerTypeToIndex(Finger.FingerType fingerType)
    {
        int fingerIndex = -1;
        switch(fingerType)
        {
            case Finger.FingerType.TYPE_THUMB:
                fingerIndex = 0;
                break;
            case Finger.FingerType.TYPE_INDEX:
                fingerIndex = 1;
                break;
            case Finger.FingerType.TYPE_MIDDLE:
                fingerIndex = 2;
                break;
            case Finger.FingerType.TYPE_RING:
                fingerIndex = 3;
                break;
            case Finger.FingerType.TYPE_PINKY:
                fingerIndex = 4;
                break;
        }
        return fingerIndex;
    }





    // Getting index fingers
    //Vector3 lbp = Vector3.zero;
    //Vector3 rbp = Vector3.zero;
    //foreach (Hand hand in handFrame.Hands)
    //{
    //    if (hand.IsLeft)
    //    {
    //        lbp = hand.Fingers[1].Bone(Bone.BoneType.TYPE_DISTAL).Center.ToVector3();
    //    } else if(hand.IsRight)
    //    {
    //        rbp = hand.Fingers[1].Bone(Bone.BoneType.TYPE_DISTAL).Center.ToVector3();
    //    }  
    //}


    //int startInd = 1 + this.name.IndexOf(" ", System.StringComparison.OrdinalIgnoreCase);
    //string altarPosString = this.name.Substring(startInd);
    //altarIndex = int.Parse(altarPosString);
}
