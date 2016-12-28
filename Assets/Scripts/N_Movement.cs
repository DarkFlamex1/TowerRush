﻿using UnityEngine;
using DarkRift;


public class N_Movement : MonoBehaviour {

    public ushort networkID;
    public bool isMine;

    Vector3 lastPosition;
    Quaternion lastRotation;

    //public GameObject playerObject;

    // Use this for initialization
    void Start () {
        DarkRiftAPI.onDataDetailed += OnDataRecieved;

        DarkRiftAPI.onPlayerDisconnected += PlayerDisconnected;

        if (isMine)
        {
            
            gameObject.GetComponent<MouseLock>().enabled = true;
            gameObject.GetComponent<Pl_MouseLook>().enabled = true;
            gameObject.GetComponent<Pl_Controller>().enabled = true;
            gameObject.GetComponent<InputManager>().enabled = true;
            transform.GetChild(0).gameObject.SetActive(true);
            transform.GetChild(2).gameObject.SetActive(true);

        }
	}

    // Update is called once per frame
    void Update()
    {
        if (DarkRiftAPI.isConnected && DarkRiftAPI.id == networkID)
        {
            //Serialize if the position or rotation changes
            if (transform.position != lastPosition)
            {
                //DarkRiftAPI.SendMessageToOthers(TagIndex.PlayerUpdate, TagIndex.PlayerUpdateSubjects.Position, transform.position);
                SerialisePos(transform.position);
            }
            if (transform.rotation != lastRotation)
            {
                DarkRiftAPI.SendMessageToOthers(TagIndex.PlayerUpdate, TagIndex.PlayerUpdateSubjects.Rotation, transform.rotation);
                
            }

            //Update stuff
            lastPosition = transform.position;
            lastRotation = transform.rotation;
        }

    }


    void SerialisePos(Vector3 pos)
    {

        using (DarkRiftWriter writer = new DarkRiftWriter())
        {
            //Next we write any data to the writer
            writer.Write(pos.x);
            writer.Write(pos.y);
            writer.Write(pos.z);

            Debug.Log("Serialize");
            DarkRiftAPI.SendMessageToOthers(TagIndex.PlayerUpdate, TagIndex.PlayerUpdateSubjects.Position, writer);
        }
    }

    void DeserialisePos(object data)
    {

        if (data is DarkRiftReader)
        {
            using (DarkRiftReader reader = (DarkRiftReader)data)
            {
                //Then read!
                transform.position = new Vector3(
                    reader.ReadSingle(),
                    reader.ReadSingle(),
                    reader.ReadSingle()
                );
            }
        }
        else
        {
            Debug.LogError("Should have recieved a DarkRiftReciever but didn't! (Got: " + data.GetType() + ")");
            transform.position = transform.position;
        }
    }
        void OnDataRecieved(ushort senderID, byte tag, ushort subject, object data) {
        Debug.Log("Recieve");
        if (senderID == networkID)
        {
            if (tag == TagIndex.PlayerUpdate)
            {
                Debug.Log("PlayerUpdate");
                //update our position
                if (subject == TagIndex.PlayerUpdateSubjects.Position)
                {
                    Debug.Log("De Position");
                    DeserialisePos(data);
                    //transform.position = (Vector3)data;
                    //Vector3.Lerp(transform.position, (Vector3)data, Time.deltaTime * 5);
                   
                }

                //update our rotation
                if (subject == TagIndex.PlayerUpdateSubjects.Rotation)
                {
                    Debug.Log("Rotation");
                    //Quaternion.Lerp(transform.rotation, (Quaternion)data, Time.deltaTime * 5);
                    transform.rotation = (Quaternion)data;
                  
                }

            }


        }
    }
  
    
        
     
    void PlayerDisconnected(ushort ID) { 

        //Get rid of gameobject

        if (ID == networkID)
        {
            DestroyImmediate(gameObject);
            return;
        }
    }
}
