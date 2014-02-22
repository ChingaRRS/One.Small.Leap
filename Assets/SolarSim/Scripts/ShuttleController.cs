﻿using UnityEngine;
using System.Collections;
using Leap;

public class ShuttleController : MonoBehaviour 
{
	//Game Objects for accessing the fuel storage
	public GameObject fuelLeft;
	public GameObject fuelRight;
	public GameObject fuelMain;

	//The value of force to apply to thruster
	public float thrustPower = 20.0f;

	//The Leap Motion controller object
	private Leap.Controller leapController;


	//Flags for determining what area of atmosphere shuttle is in
	private bool inTropo;
	private bool inStrat;
	private bool inMeso;
	private bool inTherm;

	//Use this for initialization
	void Start () 
	{
		leapController = new Controller();
		if (leapController != null)
		{
			Debug.Log ("Leap On!");
		}

		inTropo = true;
		inStrat = false;
		inMeso = false;
		inTherm = false;
	}

	//Get the left hand, not 100% sure how it works yet
	Hand GetLeftMostHand(Frame f)
	{
		float smallestVal = float.MaxValue;
		Hand h = null;
		for (int i = 0; i < f.Hands.Count; ++i)
		{
			if (f.Hands[i].PalmPosition.ToUnity().x < smallestVal)
			{
				smallestVal = f.Hands[i].PalmPosition.ToUnity().x;
				h = f.Hands[i];
			}
		}
		return h;
	}

	//Get the right hand, not 100% sure how it works yet
	Hand GetRightMostHand(Frame f)
	{
		float largestVal = -float.MaxValue;
		Hand h = null;
		for (int i = 0; i < f.Hands.Count; ++i)
		{
			if (f.Hands[i].PalmPosition.ToUnity().x > largestVal)
			{
				largestVal = f.Hands[i].PalmPosition.ToUnity().x;
				h = f.Hands[i];
			}
		}
		return h;
	}

	//Physics frame updates
	void FixedUpdate()
	{
		//Get the frame info from the leap motion controller
		Frame frame = leapController.Frame();

		//If there are 2 hands
		if (frame.Hands.Count >= 2)
		{
			//Assign the hands to variables
			Hand leftHand = GetLeftMostHand(frame);
			Hand rightHand = GetRightMostHand(frame);
			
			//Takes the average forward vector of palms, used for x rotation
			Vector3 avgPalmForward = (frame.Hands[0].Direction.ToUnity() + frame.Hands[1].Direction.ToUnity()) * 0.5f;

			//Gets the Vector difference between the palm positions
			Vector3 handDiff = leftHand.PalmPosition.ToUnityScaled() - rightHand.PalmPosition.ToUnityScaled();

			//Get the current shuttle rotation, then applies hand difference (y) to shuttle's z rotation
			Vector3 newRot = transform.localRotation.eulerAngles;
			newRot.z = -handDiff.y * 20.0f;
			
			// adding the rot.z as a way to use banking (rolling) to turn.
			newRot.y += handDiff.z * 3.0f - newRot.z * 0.03f * transform.rigidbody.velocity.magnitude;
			newRot.x = -(avgPalmForward.y - 0.1f) * 100.0f;



			
			// if closed fist, then stop the plane and slowly go backwards.
			if (frame.Fingers.Count < 3)
			{
				//thrustPower = -3.0f;
			}
			
			transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(newRot), 0.1f);
			transform.rigidbody.velocity = transform.up * thrustPower;
		}
	}
}